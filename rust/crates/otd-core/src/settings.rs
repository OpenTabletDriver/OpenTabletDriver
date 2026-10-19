use serde::{Deserialize, Serialize};

#[derive(Clone, Copy, Debug, Serialize, Deserialize, PartialEq)]
#[serde(deny_unknown_fields)]
pub struct Area {
    pub width_mm: f32,
    pub height_mm: f32,
    pub center_x_mm: f32,
    pub center_y_mm: f32,
    pub rotation_deg: f32,
}

#[derive(Clone, Copy, Debug, Serialize, Deserialize, PartialEq)]
#[serde(deny_unknown_fields)]
pub struct DisplayRect {
    pub x: i32,
    pub y: i32,
    pub width: u32,
    pub height: u32,
}

#[derive(Clone, Copy, Debug, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "snake_case")]
pub enum MouseBinding {
    Disabled,
    Left,
    Right,
    Middle,
}

impl MouseBinding {
    pub const fn mask(self) -> u8 {
        match self {
            Self::Disabled => 0,
            Self::Left => 1,
            Self::Right => 2,
            Self::Middle => 4,
        }
    }
}

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq)]
#[serde(deny_unknown_fields)]
pub struct Settings {
    pub schema_version: u32,
    pub area: Area,
    pub display: DisplayRect,
    pub clip_to_area: bool,
    pub ignore_outside_area: bool,
    pub tip_threshold_percent: f32,
    pub tip_binding: MouseBinding,
    pub pen_bindings: [MouseBinding; 3],
    #[serde(default)]
    pub close_to_tray: bool,
}

impl Default for Settings {
    fn default() -> Self {
        Self {
            schema_version: 1,
            area: Area {
                width_mm: 152.4,
                height_mm: 101.6,
                center_x_mm: 76.2,
                center_y_mm: 50.8,
                rotation_deg: 0.0,
            },
            display: DisplayRect {
                x: 0,
                y: 0,
                width: 1920,
                height: 1080,
            },
            clip_to_area: true,
            ignore_outside_area: false,
            tip_threshold_percent: 0.0,
            tip_binding: MouseBinding::Left,
            pen_bindings: [
                MouseBinding::Right,
                MouseBinding::Middle,
                MouseBinding::Disabled,
            ],
            close_to_tray: false,
        }
    }
}

impl Settings {
    pub fn validate(&self) -> Result<(), &'static str> {
        if self.schema_version != 1 {
            return Err("Unsupported settings schema; original .NET settings require migration");
        }
        let a = self.area;
        if [
            a.width_mm,
            a.height_mm,
            a.center_x_mm,
            a.center_y_mm,
            a.rotation_deg,
            self.tip_threshold_percent,
        ]
        .iter()
        .any(|v| !v.is_finite())
        {
            return Err("Settings must contain finite numbers");
        }
        if !(0.01..=152.4).contains(&a.width_mm) || !(0.01..=101.6).contains(&a.height_mm) {
            return Err("Tablet area must fit within 152.4 by 101.6 mm");
        }
        if !(0.0..=152.4).contains(&a.center_x_mm) || !(0.0..=101.6).contains(&a.center_y_mm) {
            return Err("Tablet center must be inside the physical tablet");
        }
        if !(-360.0..=360.0).contains(&a.rotation_deg) {
            return Err("Rotation must be between -360 and 360 degrees");
        }
        if !(0.0..=100.0).contains(&self.tip_threshold_percent) {
            return Err("Tip threshold must be between 0 and 100 percent");
        }
        if self.display.width == 0
            || self.display.height == 0
            || self.display.width > 65536
            || self.display.height > 65536
        {
            return Err("Invalid display dimensions");
        }
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::Settings;

    #[test]
    fn close_to_tray_defaults_off_and_old_settings_still_load() {
        let current = serde_json::to_string(&Settings::default()).unwrap();
        let parsed: Settings = serde_json::from_str(&current).unwrap();
        assert!(!parsed.close_to_tray);

        let mut old = serde_json::to_value(Settings::default()).unwrap();
        old.as_object_mut().unwrap().remove("close_to_tray");
        let parsed: Settings = serde_json::from_value(old).unwrap();
        assert!(!parsed.close_to_tray);
    }
}
