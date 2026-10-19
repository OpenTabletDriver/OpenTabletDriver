use crate::config::Digitizer;
use crate::{PenReport, Settings};

#[derive(Clone, Copy, Debug, PartialEq)]
pub struct MappedReport {
    pub x: f32,
    pub y: f32,
    pub pressure: f32,
    /// Left/right/middle logical mouse buttons. Multiple bindings are ORed.
    pub buttons: u8,
}

/// Precomputed affine mapping. No allocation, locks, trigonometry, or IO per report.
#[derive(Clone, Debug)]
pub struct Mapper {
    settings: Settings,
    matrix: [f32; 6],
    max_pressure: f32,
}

impl Mapper {
    pub fn new(
        settings: Settings,
        digitizer: Digitizer,
        max_pressure: u32,
    ) -> Result<Self, &'static str> {
        settings.validate()?;
        if [
            digitizer.width,
            digitizer.height,
            digitizer.max_x,
            digitizer.max_y,
        ]
        .iter()
        .any(|n| !n.is_finite() || *n <= 0.0)
            || max_pressure == 0
        {
            return Err("Invalid digitizer specifications");
        }
        let a = settings.area;
        let d = settings.display;
        let (sin, cos) = (-a.rotation_deg.to_radians()).sin_cos();
        let sx = d.width as f32 / a.width_mm;
        let sy = d.height as f32 / a.height_mm;
        let rx = digitizer.width / digitizer.max_x;
        let ry = digitizer.height / digitizer.max_y;
        let matrix = [
            rx * cos * sx,
            -ry * sin * sx,
            rx * sin * sy,
            ry * cos * sy,
            (-a.center_x_mm * cos + a.center_y_mm * sin) * sx + d.x as f32 + d.width as f32 / 2.0,
            (-a.center_x_mm * sin - a.center_y_mm * cos) * sy + d.y as f32 + d.height as f32 / 2.0,
        ];
        Ok(Self {
            settings,
            matrix,
            max_pressure: max_pressure as f32,
        })
    }

    pub fn map(&self, pen: PenReport) -> Option<MappedReport> {
        let m = self.matrix;
        let mut x = pen.x as f32 * m[0] + pen.y as f32 * m[1] + m[4];
        let mut y = pen.x as f32 * m[2] + pen.y as f32 * m[3] + m[5];
        let d = self.settings.display;
        let clipped_x = x.clamp(d.x as f32, d.x as f32 + d.width as f32);
        let clipped_y = y.clamp(d.y as f32, d.y as f32 + d.height as f32);
        if self.settings.ignore_outside_area && (clipped_x != x || clipped_y != y) {
            return None;
        }
        if self.settings.clip_to_area {
            x = clipped_x;
            y = clipped_y;
        }
        let raw_pressure = (pen.pressure as f32 / self.max_pressure).clamp(0.0, 1.0);
        let threshold = self.settings.tip_threshold_percent / 100.0;
        // Upstream preserves maximum pressure, including when threshold is 100%.
        let pressure = if raw_pressure == 1.0 {
            1.0
        } else if raw_pressure > threshold {
            let rewritten =
                (self.max_pressure * ((raw_pressure - threshold) / (1.0 - threshold))) as u32;
            rewritten as f32 / self.max_pressure
        } else {
            0.0
        };
        let mut buttons = if pressure > 0.0 {
            self.settings.tip_binding.mask()
        } else {
            0
        };
        for (index, binding) in self.settings.pen_bindings.iter().enumerate() {
            if pen.buttons & (1 << index) != 0 {
                buttons |= binding.mask();
            }
        }
        Some(MappedReport {
            x,
            y,
            pressure,
            buttons,
        })
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::Variant;
    fn mapper(settings: Settings) -> Mapper {
        let config = Variant::S640.configuration().unwrap();
        Mapper::new(
            settings,
            config.specifications.digitizer,
            config.specifications.pen.max_pressure,
        )
        .unwrap()
    }
    fn pen(x: u32, y: u32, pressure: u16) -> PenReport {
        PenReport {
            x,
            y,
            pressure,
            buttons: 0,
        }
    }
    fn near(a: f32, b: f32) {
        assert!((a - b).abs() < 0.002, "{a} != {b}");
    }
    #[test]
    fn full_area_maps_corners_and_center() {
        let m = mapper(Settings::default());
        for (p, x, y) in [
            (pen(0, 0, 0), 0.0, 0.0),
            (pen(30480, 20320, 8191), 1920.0, 1080.0),
            (pen(15240, 10160, 0), 960.0, 540.0),
        ] {
            let o = m.map(p).unwrap();
            near(o.x, x);
            near(o.y, y);
        }
    }
    #[test]
    fn rotation_and_negative_monitor_origin() {
        let mut s = Settings::default();
        s.area.rotation_deg = 90.0;
        s.display.x = -1920;
        let p = mapper(s).map(pen(15240, 10160, 0)).unwrap();
        near(p.x, -960.0);
        near(p.y, 540.0);
    }
    #[test]
    fn outside_area_can_clip_or_ignore_and_releases_are_left_to_output_owner() {
        let mut s = Settings::default();
        s.area.width_mm = 50.0;
        near(mapper(s.clone()).map(pen(0, 0, 0)).unwrap().x, 0.0);
        s.ignore_outside_area = true;
        assert!(mapper(s).map(pen(0, 0, 0)).is_none());
    }
    #[test]
    fn pressure_threshold_and_overlapping_bindings() {
        let mut s = Settings {
            tip_threshold_percent: 50.0,
            ..Settings::default()
        };
        s.pen_bindings[0] = crate::MouseBinding::Left;
        let m = mapper(s);
        assert_eq!(m.map(pen(0, 0, 4095)).unwrap().buttons, 0);
        let mut p = pen(0, 0, 8191);
        p.buttons = 1;
        assert_eq!(m.map(p).unwrap().buttons, 1);
        near(m.map(p).unwrap().pressure, 1.0);
    }
    #[test]
    fn invalid_settings_are_rejected_before_replacing_live_mapper() {
        let mut s = Settings::default();
        s.area.width_mm = f32::NAN;
        assert!(s.validate().is_err());
        s.area.width_mm = 0.0;
        assert!(s.validate().is_err());
        s.area.width_mm = 152.4;
        s.schema_version = 99;
        assert!(s.validate().is_err());
    }
}
