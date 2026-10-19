use otd_core::{Variant, parser::parse};
use serde::Deserialize;

#[derive(Deserialize)]
struct Fixture {
    variant: Variant,
    packet: String,
    expected: serde_json::Value,
}

#[test]
fn decoded_reports_match_actual_upstream_csharp_parsers() {
    let source = include_str!("../../../tests/fixtures/upstream-veikk.jsonl");
    let mut count = 0;
    for line in source.lines() {
        let f: Fixture = serde_json::from_str(line).unwrap();
        let bytes: Vec<u8> = f
            .packet
            .as_bytes()
            .as_chunks::<2>()
            .0
            .iter()
            .map(|hex| u8::from_str_radix(std::str::from_utf8(hex).unwrap(), 16).unwrap())
            .collect();
        let actual = parse(f.variant, &bytes).unwrap();
        assert_eq!(
            serde_json::to_value(actual).unwrap(),
            f.expected,
            "case {count}: {:?} {}",
            f.variant,
            f.packet
        );
        count += 1;
    }
    assert!(
        count >= 2048,
        "Upstream parity coverage unexpectedly shrank"
    );
}
