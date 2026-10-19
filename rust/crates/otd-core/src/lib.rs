//! Hardware-independent, allocation-free report decoding and absolute mapping.
pub mod config;
pub mod mapping;
pub mod parser;
pub mod settings;

pub use config::{TabletConfig, Variant};
pub use mapping::{MappedReport, Mapper};
pub use parser::{ParseError, PenReport, Report};
pub use settings::{Area, DisplayRect, MouseBinding, Settings};
