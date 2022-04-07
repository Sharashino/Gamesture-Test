using System.Windows.Forms;
using System;

// Hook to displaying window dialog parented by current window (game window)
public class WindowWrapper : IWin32Window
{
    private IntPtr _hwnd;
    public WindowWrapper(IntPtr handle) { _hwnd = handle; }
    public IntPtr Handle { get { return _hwnd; } }
}