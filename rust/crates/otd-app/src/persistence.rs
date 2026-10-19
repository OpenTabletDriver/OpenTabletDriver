use otd_core::Settings;
use std::{
    fs::{self, File, OpenOptions},
    io::{self, Read, Write},
    path::{Path, PathBuf},
    sync::atomic::{AtomicU64, Ordering},
};

static SEQUENCE: AtomicU64 = AtomicU64::new(0);
const MAX_SETTINGS_BYTES: u64 = 64 * 1024;

pub fn settings_path() -> PathBuf {
    std::env::var_os("LOCALAPPDATA")
        .map(PathBuf::from)
        .unwrap_or_else(std::env::temp_dir)
        .join("OpenTabletDriverRust")
        .join("settings.json")
}

pub fn load(path: &Path) -> Result<Settings, String> {
    let mut file = File::open(path).map_err(|e| e.to_string())?;
    if file.metadata().map_err(|e| e.to_string())?.len() > MAX_SETTINGS_BYTES {
        return Err("Settings exceed 64 KiB".into());
    }
    let mut json = String::new();
    (&mut file)
        .take(MAX_SETTINGS_BYTES + 1)
        .read_to_string(&mut json)
        .map_err(|e| e.to_string())?;
    if json.len() as u64 > MAX_SETTINGS_BYTES {
        return Err("Settings exceed 64 KiB".into());
    }
    let settings: Settings = serde_json::from_str(&json).map_err(|e| e.to_string())?;
    settings.validate().map_err(str::to_owned)?;
    Ok(settings)
}

pub fn save(path: &Path, settings: &Settings) -> Result<(), String> {
    settings.validate().map_err(str::to_owned)?;
    let json = serde_json::to_vec_pretty(settings).map_err(|e| e.to_string())?;
    save_bytes(path, &json).map_err(|e| e.to_string())
}

fn save_bytes(path: &Path, bytes: &[u8]) -> io::Result<()> {
    let directory = path
        .parent()
        .ok_or_else(|| io::Error::other("Settings need a parent directory"))?;
    fs::create_dir_all(directory)?;
    let temporary = directory.join(format!(
        ".settings-{}-{}.tmp",
        std::process::id(),
        SEQUENCE.fetch_add(1, Ordering::Relaxed)
    ));
    // Acquire the unique file before installing cleanup, so a collision is never deleted.
    let mut file = OpenOptions::new()
        .create_new(true)
        .write(true)
        .open(&temporary)?;
    struct Cleanup(PathBuf);
    impl Drop for Cleanup {
        fn drop(&mut self) {
            let _ = fs::remove_file(&self.0);
        }
    }
    let _cleanup = Cleanup(temporary.clone());
    file.write_all(bytes)?;
    file.sync_all()?;
    drop(file);
    otd_platform::replace_file(&temporary, path)
}

#[cfg(test)]
mod tests {
    use super::*;
    fn dir() -> PathBuf {
        let p = std::env::temp_dir().join(format!(
            "otd-rust-test-{}-{}",
            std::process::id(),
            SEQUENCE.fetch_add(1, Ordering::Relaxed)
        ));
        fs::create_dir(&p).unwrap();
        p
    }
    fn cleanup(p: PathBuf) {
        for entry in fs::read_dir(&p).unwrap() {
            fs::remove_file(entry.unwrap().path()).unwrap();
        }
        fs::remove_dir(p).unwrap();
    }
    #[test]
    fn settings_roundtrip_and_invalid_save_preserves_existing_bytes() {
        let dir = dir();
        let p = dir.join("settings.json");
        let s = Settings::default();
        save(&p, &s).unwrap();
        assert_eq!(load(&p).unwrap(), s);
        let original = fs::read(&p).unwrap();
        let mut bad = s.clone();
        bad.area.width_mm = 0.0;
        assert!(save(&p, &bad).is_err());
        assert_eq!(fs::read(&p).unwrap(), original);
        let mut next = s;
        next.area.rotation_deg = 90.0;
        save(&p, &next).unwrap();
        assert_eq!(load(&p).unwrap(), next);
        assert_eq!(fs::read_dir(&dir).unwrap().count(), 1);
        cleanup(dir);
    }
    #[test]
    fn replacement_failure_preserves_destination_and_cleans_stage() {
        use std::os::windows::fs::OpenOptionsExt;
        let dir = dir();
        let p = dir.join("settings.json");
        fs::write(&p, b"original").unwrap();
        let lock = OpenOptions::new()
            .read(true)
            .share_mode(0)
            .open(&p)
            .unwrap();
        assert!(save(&p, &Settings::default()).is_err());
        drop(lock);
        assert_eq!(fs::read(&p).unwrap(), b"original");
        assert_eq!(fs::read_dir(&dir).unwrap().count(), 1);
        cleanup(dir);
    }
    #[test]
    fn rejects_large_or_unknown_schema_files_without_overwriting_them() {
        let dir = dir();
        let p = dir.join("settings.json");
        fs::write(&p, vec![b' '; MAX_SETTINGS_BYTES as usize + 1]).unwrap();
        assert!(load(&p).unwrap_err().contains("64 KiB"));
        cleanup(dir);
    }
}
