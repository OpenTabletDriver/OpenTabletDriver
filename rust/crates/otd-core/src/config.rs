use serde::{Deserialize, Serialize};

#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
pub enum Variant {
    S640,
    S640V2,
}

impl Variant {
    pub const fn report_len(self) -> usize {
        match self {
            Self::S640 => 9,
            Self::S640V2 => 13,
        }
    }
    pub fn configuration(self) -> Result<TabletConfig, serde_json::Error> {
        // Keep the original configurations as the source of truth.
        let json = match self {
            Self::S640 => include_str!(
                "../../../../OpenTabletDriver.Configurations/Configurations/VEIKK/S640.json"
            ),
            Self::S640V2 => include_str!(
                "../../../../OpenTabletDriver.Configurations/Configurations/VEIKK/S640 V2.json"
            ),
        };
        serde_json::from_str(json)
    }
}

#[derive(Clone, Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct TabletConfig {
    pub name: String,
    pub specifications: Specifications,
    pub digitizer_identifiers: Vec<DeviceIdentifier>,
}

#[derive(Clone, Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct Specifications {
    pub digitizer: Digitizer,
    pub pen: Pen,
}

#[derive(Clone, Copy, Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct Digitizer {
    pub width: f32,
    pub height: f32,
    pub max_x: f32,
    pub max_y: f32,
}

#[derive(Clone, Copy, Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct Pen {
    pub max_pressure: u32,
    pub button_count: u32,
}

#[derive(Clone, Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct DeviceIdentifier {
    #[serde(rename = "VendorID")]
    pub vendor_id: u16,
    #[serde(rename = "ProductID")]
    pub product_id: u16,
    pub input_report_length: u16,
    pub output_report_length: u16,
    pub report_parser: String,
    pub output_init_report: Vec<String>,
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn embedded_definitions_match_the_migrated_contract() {
        for v in [Variant::S640, Variant::S640V2] {
            let c = v.configuration().unwrap();
            let d = &c.digitizer_identifiers[0];
            assert_eq!((d.vendor_id, d.product_id), (12267, 1));
            assert_eq!(d.input_report_length as usize, v.report_len());
            assert_eq!(d.output_report_length, 9);
            assert_eq!(d.output_init_report, ["CQEE"]);
            assert_eq!(c.specifications.pen.max_pressure, 8191);
        }
    }
}
