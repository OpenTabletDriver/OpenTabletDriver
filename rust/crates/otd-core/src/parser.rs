use crate::config::Variant;
use serde::{Deserialize, Serialize};

#[derive(Clone, Copy, Debug, PartialEq, Serialize, Deserialize)]
pub struct PenReport {
    pub x: u32,
    pub y: u32,
    pub pressure: u16,
    /// Three bits for V1, two for V2, matching the upstream report structures.
    pub buttons: u8,
}

#[derive(Clone, Copy, Debug, PartialEq, Serialize, Deserialize)]
#[serde(tag = "type", rename_all = "snake_case")]
pub enum Report {
    Pen(PenReport),
    OutOfRange,
    Auxiliary { buttons: u16 },
    Wheel { delta: i8 },
    Ignore,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct ParseError {
    pub expected: usize,
    pub actual: usize,
}

pub fn parse(variant: Variant, bytes: &[u8]) -> Result<Report, ParseError> {
    let expected = variant.report_len();
    if bytes.len() != expected {
        return Err(ParseError {
            expected,
            actual: bytes.len(),
        });
    }
    Ok(match variant {
        Variant::S640 => parse_v1(bytes),
        Variant::S640V2 => parse_v2(bytes),
    })
}

fn u16le(bytes: &[u8], index: usize) -> u16 {
    u16::from_le_bytes([bytes[index], bytes[index + 1]])
}

fn parse_v1(b: &[u8]) -> Report {
    if b[0] == 3 {
        let mut buttons = 0;
        for &code in &b[2..] {
            let index = match code {
                0x3e => 0,
                0x0c => 1,
                0x2c => 2,
                0x19 => {
                    if b[1] == 0 {
                        3
                    } else {
                        5
                    }
                }
                0x06 => 4,
                0x1d => 6,
                0x16 => 7,
                0x2e => 8,
                0x2d => 9,
                0x30 => 10,
                0x2f => 11,
                _ => continue,
            };
            buttons |= 1 << index;
        }
        return Report::Auxiliary { buttons };
    }
    match (b[1], b[2]) {
        (0x41, flags) if flags & 0xf0 == 0xa0 => Report::Pen(PenReport {
            x: u16le(b, 3) as u32,
            y: u16le(b, 5) as u32,
            pressure: u16le(b, 7),
            buttons: (flags >> 1) & 7,
        }),
        (0x41, 0xc0) => Report::OutOfRange,
        _ => Report::Ignore,
    }
}

fn parse_v2(b: &[u8]) -> Report {
    if b[1] == 0x43 {
        return Report::Ignore;
    }
    if b[2] & 0x20 != 0 {
        return Report::Pen(PenReport {
            x: u16le(b, 3) as u32 | ((b[5] as u32) << 16),
            y: u16le(b, 6) as u32 | ((b[8] as u32) << 16),
            pressure: u16le(b, 9),
            buttons: (b[2] >> 1) & 3,
        });
    }
    match b[2] {
        1 => Report::Auxiliary {
            buttons: if b[3] & 1 != 0 {
                u16le(b, 4) & 0x0fff
            } else {
                0
            },
        },
        3 => Report::Wheel {
            delta: if b[3] & 1 == 0 {
                0
            } else if b[4] & 2 != 0 {
                1
            } else if b[4] & 1 != 0 {
                -1
            } else {
                0
            },
        },
        _ => Report::Ignore,
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn rejects_all_incorrect_lengths_without_panicking() {
        for variant in [Variant::S640, Variant::S640V2] {
            for len in 0..128 {
                let bytes = [0xff; 128];
                assert_eq!(
                    parse(variant, &bytes[..len]).is_ok(),
                    len == variant.report_len()
                );
            }
        }
    }

    #[test]
    fn decodes_s640_original_offsets_and_buttons() {
        let b = [8, 0x41, 0xa6, 0x10, 0x27, 0x20, 0x4e, 0xff, 0x1f];
        assert_eq!(
            parse(Variant::S640, &b),
            Ok(Report::Pen(PenReport {
                x: 10000,
                y: 20000,
                pressure: 8191,
                buttons: 3,
            }))
        );
    }

    #[test]
    fn v2_uses_full_twenty_four_bit_coordinates() {
        let b = [8, 0, 0x22, 1, 2, 3, 4, 5, 6, 0xfe, 0x1f, 0, 0];
        assert_eq!(
            parse(Variant::S640V2, &b),
            Ok(Report::Pen(PenReport {
                x: 0x030201,
                y: 0x060504,
                pressure: 8190,
                buttons: 1,
            }))
        );
    }
}
