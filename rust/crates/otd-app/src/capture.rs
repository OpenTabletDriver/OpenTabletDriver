//! Render only this application's own control window to a BMP for layout checks.
use std::{
    fs::File,
    io::{self, Write},
    mem::{size_of, zeroed},
    path::Path,
};
use windows_sys::Win32::{
    Foundation::*, Graphics::Gdi::*, Storage::Xps::PrintWindow, UI::WindowsAndMessaging::*,
};

pub fn window(hwnd: HWND, destination: &Path) -> io::Result<()> {
    // SAFETY: all GDI objects are created here, restored before reading, and released
    // on all paths. PrintWindow targets this process's own window, never the desktop.
    unsafe {
        let mut rect: RECT = zeroed();
        if GetWindowRect(hwnd, &mut rect) == 0 {
            return Err(io::Error::last_os_error());
        }
        let width = rect.right - rect.left;
        let height = rect.bottom - rect.top;
        if width <= 0 || height <= 0 || width > 4096 || height > 4096 {
            return Err(io::Error::other("Invalid capture size"));
        }
        let screen = GetDC(hwnd);
        let dc = CreateCompatibleDC(screen);
        let bitmap = CreateCompatibleBitmap(screen, width, height);
        if screen.is_null() || dc.is_null() || bitmap.is_null() {
            if !bitmap.is_null() {
                DeleteObject(bitmap);
            }
            if !dc.is_null() {
                DeleteDC(dc);
            }
            if !screen.is_null() {
                ReleaseDC(hwnd, screen);
            }
            return Err(io::Error::other("Cannot create capture surface"));
        }
        let old = SelectObject(dc, bitmap);
        let background = RECT {
            left: 0,
            top: 0,
            right: width,
            bottom: height,
        };
        FillRect(dc, &background, GetSysColorBrush(COLOR_WINDOW));
        let rendered = PrintWindow(hwnd, dc, PW_RENDERFULLCONTENT);
        SendMessageW(
            hwnd,
            WM_PRINT,
            dc as usize,
            (PRF_CLIENT | PRF_NONCLIENT | PRF_CHILDREN | PRF_ERASEBKGND) as isize,
        );
        SelectObject(dc, old);
        let mut info: BITMAPINFO = zeroed();
        info.bmiHeader.biSize = size_of::<BITMAPINFOHEADER>() as u32;
        info.bmiHeader.biWidth = width;
        info.bmiHeader.biHeight = -height;
        info.bmiHeader.biPlanes = 1;
        info.bmiHeader.biBitCount = 32;
        info.bmiHeader.biCompression = BI_RGB;
        let mut pixels = vec![0u8; width as usize * height as usize * 4];
        let rows = if rendered != 0 {
            GetDIBits(
                dc,
                bitmap,
                0,
                height as u32,
                pixels.as_mut_ptr().cast(),
                &mut info,
                DIB_RGB_COLORS,
            )
        } else {
            0
        };
        DeleteObject(bitmap);
        DeleteDC(dc);
        ReleaseDC(hwnd, screen);
        if rows != height {
            return Err(io::Error::other("Window rendering failed"));
        }
        for pixel in pixels.as_chunks_mut::<4>().0 {
            pixel[3] = 255;
        }
        let mut file = File::create(destination)?;
        file.write_all(b"BM")?;
        file.write_all(&(54 + pixels.len() as u32).to_le_bytes())?;
        file.write_all(&[0u8; 4])?;
        file.write_all(&54u32.to_le_bytes())?;
        file.write_all(&40u32.to_le_bytes())?;
        file.write_all(&width.to_le_bytes())?;
        file.write_all(&(-height).to_le_bytes())?;
        file.write_all(&1u16.to_le_bytes())?;
        file.write_all(&32u16.to_le_bytes())?;
        file.write_all(&[0u8; 24])?;
        file.write_all(&pixels)?;
        Ok(())
    }
}
