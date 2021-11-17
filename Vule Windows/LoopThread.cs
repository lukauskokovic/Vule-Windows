using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Interop;

public static class LoopThread 
{
    static bool IsRunning = false;
    static Thread Thread;
    public static void StartThread()
    {
        if (IsRunning) return;
        IsRunning = true;
        Thread = new Thread(() =>
        {
            while (IsRunning)
            {
                if (IsPressed(GetKeyState(0xA3))) // Is right control(ctrl) being pressed
                {
                    int RightArrowState = GetKeyState(0x27);
                    int LeftArrowState = GetKeyState(0x25);
                    if (IsPressed(RightArrowState))
                        MoveWindow(1); 
                    else if (IsPressed(LeftArrowState))
                        MoveWindow(-1);
                }
                Thread.Sleep(20);
            }
        })
        {Priority = ThreadPriority.Lowest};
        Thread.SetApartmentState(ApartmentState.STA);
        Thread.Start();
    }

    static bool IsPressed(int key) { return key != 0 && key != 1; }

    static void MoveWindow(int direction)
    {
        if (Screen.AllScreens.Length == 1) return;

        IntPtr ApplicationHandle = GetForegroundWindow();
        GetWindowRect(ApplicationHandle, out Rect windowPos); // Getting window position
        windowPos.Left += 8;windowPos.Right += 8; // Filling out the windows padding
        Rectangle[] Screens = Screen.AllScreens.Select(i => i.Bounds).OrderBy(i => i.X).ToArray(); // Resorting all screens 

        int NewAppWindowIndex = 0, AppWindowIndex = 0;
        for(int i = 0; i < Screens.Length; i++)
        {
            bool last = i + 1 == Screens.Length;
            if(windowPos.Left >= Screens[i].X && last || windowPos.Left < Screens[i + 1].X) // Getting the id of the screen where the application is
            {
                AppWindowIndex = i;
                if (direction == 1) NewAppWindowIndex = last ? 0 : i + 1;
                else if (direction == -1) NewAppWindowIndex = i == 0 ? Screens.Length - 1 : i - 1;
                break;
            }
        }
        Rectangle AppScreen = Screens[AppWindowIndex];
        Rectangle NewAppWindowRect = Screens[NewAppWindowIndex];

        Size CurrentAppWindowSize = CalculateSize(AppScreen); // Current window(monitor) the app sits on size variable
        Size ApplicationSize = CalculateSize(windowPos); 
        Size NewAppWindowSize = CalculateSize(NewAppWindowRect); // -||- just for the window application is going to sit in

        double LeftPercentage = (double)(windowPos.Left - AppScreen.Left) / CurrentAppWindowSize.Width,
               WidthPercentage = (double)ApplicationSize.Width / CurrentAppWindowSize.Width,
               TopPercentage = (double)windowPos.Top / CurrentAppWindowSize.Height,
               HeightPercentage = (double)ApplicationSize.Height / CurrentAppWindowSize.Height;

        int NewLeft = NewAppWindowRect.Left + (int)(NewAppWindowSize.Width * LeftPercentage),
            NewWidth = (int)(WidthPercentage * NewAppWindowSize.Width),
            NewTop = NewAppWindowRect.Top + (int)(TopPercentage * NewAppWindowSize.Height),
            NewHeight = (int)(HeightPercentage * NewAppWindowSize.Height);
        MoveWindow(ApplicationHandle, NewLeft-8, NewTop, NewWidth, NewHeight, false);
        Thread.Sleep(180);
    }
    static Size CalculateSize(Rectangle rect)
    { 
        return new Size(Math.Abs(rect.Right - rect.Left), rect.Bottom - rect.Top); 
    }
    static Size CalculateSize(Rect rect)
    {
        return new Size(Math.Abs(rect.Right - rect.Left), rect.Bottom - rect.Top);
    }

    public static void StopThread()
    {
        IsRunning = false;
    }
    #region Windows Api functions
    [DllImport("User32.dll")]
    static extern short GetKeyState(int key); // Function for getting keystrokes (CTRL AND LEFT/RIGHT Arrow)
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetForegroundWindow(); // Function that gets windows handle
    [DllImport("User32.dll")]
    static extern bool GetWindowRect(IntPtr hwnd, out Rect lpRect); // Function that gets window location

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint); // Function used to move window to new location

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect // Just a struct to get the application position
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);
    public static ImageSource ToImageSource(this Icon icon)
    {
        Bitmap bitmap = icon.ToBitmap();
        IntPtr hBitmap = bitmap.GetHbitmap();

        ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
            hBitmap,
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        if (!DeleteObject(hBitmap))
        {
            throw new Win32Exception();
        }

        return wpfBitmap;
    } //Function that was coppied from stackoverflow in order for me to convert .ico file from resources to image
    #endregion
}