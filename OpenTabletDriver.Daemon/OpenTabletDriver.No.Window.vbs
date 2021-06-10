Set Shell = CreateObject("Shell.Application")
Shell.ShellExecute "OpenTabletDriver.Daemon.exe", , , , 0
Set Shell = Nothing
