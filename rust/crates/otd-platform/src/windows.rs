use otd_core::{DisplayRect, MappedReport, Variant};
use std::{
    ffi::OsStr,
    io,
    mem::{size_of, zeroed},
    os::windows::ffi::OsStrExt,
    path::Path,
    ptr::{null, null_mut},
};
use windows_sys::Win32::{
    Devices::{DeviceAndDriverInstallation::*, HumanInterfaceDevice::*},
    Foundation::*,
    Storage::FileSystem::*,
    System::{Console::*, IO::*, ProcessStatus::*, Threading::*},
    UI::{Input::KeyboardAndMouse::*, WindowsAndMessaging::*},
};

pub fn wide(s: &OsStr) -> Vec<u16> {
    s.encode_wide().chain(Some(0)).collect()
}

static CONSOLE_STOP: std::sync::atomic::AtomicBool = std::sync::atomic::AtomicBool::new(false);
unsafe extern "system" fn console_handler(kind: u32) -> i32 {
    if kind == CTRL_C_EVENT || kind == CTRL_BREAK_EVENT {
        CONSOLE_STOP.store(true, std::sync::atomic::Ordering::Relaxed);
        1
    } else {
        0
    }
}
pub fn install_console_stop_handler() -> io::Result<()> {
    // SAFETY: handler is a process-lifetime function and only sets an atomic flag.
    if unsafe { SetConsoleCtrlHandler(Some(console_handler), 1) } == 0 {
        Err(io::Error::last_os_error())
    } else {
        Ok(())
    }
}
pub fn console_stop_requested() -> bool {
    CONSOLE_STOP.load(std::sync::atomic::Ordering::Relaxed)
}

pub struct OwnedHandle(HANDLE);
impl OwnedHandle {
    fn new(handle: HANDLE) -> io::Result<Self> {
        if handle.is_null() || handle == INVALID_HANDLE_VALUE {
            Err(io::Error::last_os_error())
        } else {
            Ok(Self(handle))
        }
    }
}
impl Drop for OwnedHandle {
    fn drop(&mut self) {
        // SAFETY: exclusively owned valid kernel handle, closed exactly once.
        unsafe {
            CloseHandle(self.0);
        }
    }
}

pub fn single_instance() -> io::Result<OwnedHandle> {
    let name = wide(OsStr::new("Local\\OpenTabletDriverRust-S640"));
    // SAFETY: null security attributes use caller defaults; name is NUL-terminated.
    unsafe {
        let h = OwnedHandle::new(CreateMutexW(null(), 0, name.as_ptr()))?;
        if GetLastError() == ERROR_ALREADY_EXISTS {
            return Err(io::Error::new(
                io::ErrorKind::AlreadyExists,
                "Another Rust driver instance is already running",
            ));
        }
        Ok(h)
    }
}

pub fn replace_file(source: &Path, destination: &Path) -> io::Result<()> {
    let src = wide(source.as_os_str());
    let dst = wide(destination.as_os_str());
    // SAFETY: live NUL-terminated strings; source/destination share the settings directory.
    if unsafe {
        MoveFileExW(
            src.as_ptr(),
            dst.as_ptr(),
            MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH,
        )
    } == 0
    {
        Err(io::Error::last_os_error())
    } else {
        Ok(())
    }
}

#[derive(Clone, Copy, Debug, Default)]
pub struct MemoryUsage {
    pub working_set_bytes: usize,
    pub private_bytes: usize,
    pub peak_working_set_bytes: usize,
}

pub fn memory_usage() -> io::Result<MemoryUsage> {
    // SAFETY: correctly sized PROCESS_MEMORY_COUNTERS_EX passed through the base pointer.
    unsafe {
        let mut counters: PROCESS_MEMORY_COUNTERS_EX = zeroed();
        counters.cb = size_of::<PROCESS_MEMORY_COUNTERS_EX>() as u32;
        if K32GetProcessMemoryInfo(
            GetCurrentProcess(),
            &mut counters as *mut _ as *mut _,
            counters.cb,
        ) == 0
        {
            return Err(io::Error::last_os_error());
        }
        Ok(MemoryUsage {
            working_set_bytes: counters.WorkingSetSize,
            private_bytes: counters.PrivateUsage,
            peak_working_set_bytes: counters.PeakWorkingSetSize,
        })
    }
}

pub fn virtual_desktop() -> DisplayRect {
    // SAFETY: system metrics have no pointer arguments or ownership requirements.
    unsafe {
        DisplayRect {
            x: GetSystemMetrics(SM_XVIRTUALSCREEN),
            y: GetSystemMetrics(SM_YVIRTUALSCREEN),
            width: GetSystemMetrics(SM_CXVIRTUALSCREEN).max(1) as u32,
            height: GetSystemMetrics(SM_CYVIRTUALSCREEN).max(1) as u32,
        }
    }
}

pub fn initialize_dpi_awareness() {
    // SAFETY: establish physical-pixel coordinates before querying display metrics.
    unsafe {
        SetProcessDPIAware();
    }
}

pub fn primary_display() -> DisplayRect {
    // SAFETY: same as virtual_desktop.
    unsafe {
        DisplayRect {
            x: 0,
            y: 0,
            width: GetSystemMetrics(SM_CXSCREEN).max(1) as u32,
            height: GetSystemMetrics(SM_CYSCREEN).max(1) as u32,
        }
    }
}

#[derive(Debug)]
pub struct DeviceInfo {
    pub variant: Variant,
    pub path: Vec<u16>,
    pub input_len: u16,
    pub output_len: u16,
}

pub fn detect_s640() -> io::Result<Vec<DeviceInfo>> {
    // SAFETY: all SetupAPI buffers are sized from the API's required byte count,
    // aligned to usize, and kept alive until the call returns. All query handles
    // and the device-info set are released, including on failure.
    unsafe {
        let mut guid = zeroed();
        HidD_GetHidGuid(&mut guid);
        let set = SetupDiGetClassDevsW(
            &guid,
            null(),
            null_mut(),
            DIGCF_PRESENT | DIGCF_DEVICEINTERFACE,
        );
        if set == -1 {
            return Err(io::Error::last_os_error());
        }
        struct DeviceSet(HDEVINFO);
        impl Drop for DeviceSet {
            fn drop(&mut self) {
                unsafe {
                    SetupDiDestroyDeviceInfoList(self.0);
                }
            }
        }
        let set = DeviceSet(set);
        let mut result = Vec::new();
        for index in 0..4096 {
            let mut interface: SP_DEVICE_INTERFACE_DATA = zeroed();
            interface.cbSize = size_of::<SP_DEVICE_INTERFACE_DATA>() as u32;
            if SetupDiEnumDeviceInterfaces(set.0, null(), &guid, index, &mut interface) == 0 {
                if GetLastError() == ERROR_NO_MORE_ITEMS {
                    break;
                }
                return Err(io::Error::last_os_error());
            }
            let mut required = 0;
            SetupDiGetDeviceInterfaceDetailW(
                set.0,
                &interface,
                null_mut(),
                0,
                &mut required,
                null_mut(),
            );
            if !(8..=65536).contains(&required) {
                continue;
            }
            let mut buffer = vec![0usize; (required as usize).div_ceil(size_of::<usize>())];
            let detail = buffer.as_mut_ptr() as *mut SP_DEVICE_INTERFACE_DETAIL_DATA_W;
            (*detail).cbSize = size_of::<SP_DEVICE_INTERFACE_DETAIL_DATA_W>() as u32;
            if SetupDiGetDeviceInterfaceDetailW(
                set.0,
                &interface,
                detail,
                required,
                null_mut(),
                null_mut(),
            ) == 0
            {
                continue;
            }
            let path_ptr = (*detail).DevicePath.as_ptr();
            let path_slice = std::slice::from_raw_parts(path_ptr, (required as usize - 4) / 2);
            let Some(end) = path_slice.iter().position(|&v| v == 0) else {
                continue;
            };
            // Narrow by VID/PID text before opening all HID collections.
            let text = String::from_utf16_lossy(&path_slice[..end]).to_ascii_lowercase();
            if !text.contains("vid_2feb&pid_0001") {
                continue;
            }
            let Ok(handle) = OwnedHandle::new(CreateFileW(
                path_ptr,
                0,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                null(),
                OPEN_EXISTING,
                0,
                null_mut(),
            )) else {
                continue;
            };
            let mut attributes: HIDD_ATTRIBUTES = zeroed();
            attributes.Size = size_of::<HIDD_ATTRIBUTES>() as u32;
            if !HidD_GetAttributes(handle.0, &mut attributes)
                || attributes.VendorID != 12267
                || attributes.ProductID != 1
            {
                continue;
            }
            let mut preparsed = 0;
            if !HidD_GetPreparsedData(handle.0, &mut preparsed) {
                continue;
            }
            let mut caps: HIDP_CAPS = zeroed();
            let status = HidP_GetCaps(preparsed, &mut caps);
            HidD_FreePreparsedData(preparsed);
            if status != HIDP_STATUS_SUCCESS {
                continue;
            }
            let variant = match (caps.InputReportByteLength, caps.OutputReportByteLength) {
                (9, 9) => Variant::S640,
                (13, 9) => Variant::S640V2,
                _ => continue,
            };
            let mut path = path_slice[..end].to_vec();
            path.push(0);
            result.push(DeviceInfo {
                variant,
                path,
                input_len: caps.InputReportByteLength,
                output_len: caps.OutputReportByteLength,
            });
        }
        Ok(result)
    }
}

pub struct TabletDevice {
    handle: OwnedHandle,
    event: OwnedHandle,
    overlapped: Box<OVERLAPPED>,
    buffer: Box<[u8; 64]>,
    pending: bool,
    report_len: usize,
    pub variant: Variant,
}

impl TabletDevice {
    pub fn open(info: &DeviceInfo) -> io::Result<Self> {
        Self::open_internal(info, true)
    }
    /// Observe an already-initialized tablet without sending any feature/output reports.
    pub fn observe(info: &DeviceInfo) -> io::Result<Self> {
        Self::open_internal(info, false)
    }
    fn open_internal(info: &DeviceInfo, initialize: bool) -> io::Result<Self> {
        // SAFETY: a terminated SetupAPI device path, overlapped IO, owned handles.
        unsafe {
            let handle = OwnedHandle::new(CreateFileW(
                info.path.as_ptr(),
                GENERIC_READ | if initialize { GENERIC_WRITE } else { 0 },
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                null(),
                OPEN_EXISTING,
                FILE_FLAG_OVERLAPPED,
                null_mut(),
            ))?;
            let event = OwnedHandle::new(CreateEventW(null(), 1, 0, null()))?;
            let mut overlapped: Box<OVERLAPPED> = Box::new(zeroed());
            overlapped.hEvent = event.0;
            let mut device = Self {
                handle,
                event,
                overlapped,
                buffer: Box::new([0; 64]),
                pending: false,
                report_len: info.input_len as usize,
                variant: info.variant,
            };
            // Original JSON OutputInitReport "CQEE" = 09 01 04. HID output is padded to its reported length.
            let mut init = [0u8; 9];
            init[..3].copy_from_slice(&[9, 1, 4]);
            if initialize {
                device.write_init(&init)?;
            }
            Ok(device)
        }
    }

    fn write_init(&mut self, bytes: &[u8]) -> io::Result<()> {
        // SAFETY: stack buffer/OVERLAPPED stay alive until IO completes or cancellation drains.
        unsafe {
            let mut ov: OVERLAPPED = zeroed();
            ov.hEvent = self.event.0;
            ResetEvent(self.event.0);
            let mut count = 0;
            if WriteFile(
                self.handle.0,
                bytes.as_ptr(),
                bytes.len() as u32,
                &mut count,
                &mut ov,
            ) == 0
            {
                if GetLastError() != ERROR_IO_PENDING {
                    return Err(io::Error::last_os_error());
                }
                if WaitForSingleObject(self.event.0, 500) != WAIT_OBJECT_0 {
                    CancelIoEx(self.handle.0, &ov);
                    GetOverlappedResult(self.handle.0, &ov, &mut count, 1);
                    return Err(io::Error::new(
                        io::ErrorKind::TimedOut,
                        "S640 initialization timed out",
                    ));
                }
                if GetOverlappedResult(self.handle.0, &ov, &mut count, 0) == 0 {
                    return Err(io::Error::last_os_error());
                }
            }
            if count != bytes.len() as u32 {
                return Err(io::Error::new(
                    io::ErrorKind::WriteZero,
                    "Incomplete S640 initialization report",
                ));
            }
            Ok(())
        }
    }

    pub fn read_timeout(&mut self, output: &mut [u8; 64], timeout_ms: u32) -> io::Result<usize> {
        // SAFETY: boxed IO buffers have stable addresses across calls. A timeout keeps
        // the request pending; it is never reissued or freed while Windows owns it.
        unsafe {
            let mut count = 0;
            if !self.pending {
                ResetEvent(self.event.0);
                if ReadFile(
                    self.handle.0,
                    self.buffer.as_mut_ptr(),
                    self.report_len as u32,
                    &mut count,
                    &mut *self.overlapped,
                ) != 0
                {
                    output[..count as usize].copy_from_slice(&self.buffer[..count as usize]);
                    return Ok(count as usize);
                }
                if GetLastError() != ERROR_IO_PENDING {
                    return Err(io::Error::last_os_error());
                }
                self.pending = true;
            }
            match WaitForSingleObject(self.event.0, timeout_ms) {
                WAIT_TIMEOUT => return Ok(0),
                WAIT_OBJECT_0 => {}
                _ => return Err(io::Error::last_os_error()),
            }
            self.pending = false;
            if GetOverlappedResult(self.handle.0, &*self.overlapped, &mut count, 0) == 0 {
                return Err(io::Error::last_os_error());
            }
            output[..count as usize].copy_from_slice(&self.buffer[..count as usize]);
            Ok(count as usize)
        }
    }
}

impl Drop for TabletDevice {
    fn drop(&mut self) {
        if self.pending {
            // SAFETY: drain cancelled IO before freeing OVERLAPPED and its buffer.
            unsafe {
                CancelIoEx(self.handle.0, &*self.overlapped);
                let mut count = 0;
                GetOverlappedResult(self.handle.0, &*self.overlapped, &mut count, 1);
            }
        }
    }
}

pub struct PointerOutput {
    buttons: u8,
    desktop: DisplayRect,
}
impl Default for PointerOutput {
    fn default() -> Self {
        Self {
            buttons: 0,
            desktop: virtual_desktop(),
        }
    }
}
impl PointerOutput {
    pub fn refresh_desktop(&mut self) {
        self.desktop = virtual_desktop();
    }
    pub fn emit(&mut self, report: MappedReport) -> io::Result<()> {
        let d = self.desktop;
        let x = ((report.x - d.x as f32) / (d.width.saturating_sub(1).max(1) as f32) * 65535.0)
            .clamp(0.0, 65535.0)
            .round() as i32;
        let y = ((report.y - d.y as f32) / (d.height.saturating_sub(1).max(1) as f32) * 65535.0)
            .clamp(0.0, 65535.0)
            .round() as i32;
        self.send(x, y, report.buttons, true)
    }
    fn send(&mut self, x: i32, y: i32, buttons: u8, movement: bool) -> io::Result<()> {
        let mut flags = if movement {
            MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE | MOUSEEVENTF_VIRTUALDESK
        } else {
            0
        };
        for (mask, down, up) in [
            (1, MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP),
            (2, MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP),
            (4, MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP),
        ] {
            if (self.buttons ^ buttons) & mask != 0 {
                flags |= if buttons & mask != 0 { down } else { up };
            }
        }
        if flags == 0 {
            return Ok(());
        }
        // SAFETY: correctly sized single INPUT with the mouse union member initialized.
        unsafe {
            let input = INPUT {
                r#type: INPUT_MOUSE,
                Anonymous: INPUT_0 {
                    mi: MOUSEINPUT {
                        dx: x,
                        dy: y,
                        mouseData: 0,
                        dwFlags: flags,
                        time: 0,
                        dwExtraInfo: 0,
                    },
                },
            };
            if SendInput(1, &input, size_of::<INPUT>() as i32) != 1 {
                return Err(io::Error::last_os_error());
            }
        }
        self.buttons = buttons;
        Ok(())
    }
    pub fn reset(&mut self) -> io::Result<()> {
        self.send(0, 0, 0, false)
    }
}
impl Drop for PointerOutput {
    fn drop(&mut self) {
        let _ = self.reset();
    }
}
