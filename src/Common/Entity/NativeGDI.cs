using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Entity
{
    public static class NativeGDI
    {
        public const int RP2_XORPEN = 7;
        public const int RP2_COPYPEN = 13;

        [DllImport("gdi32")]
        public static extern bool MoveToEx(IntPtr hdc, int x, int y, out Point point);

        [DllImport("gdi32")]
        public static extern bool LineTo(IntPtr hdc, int x, int y);

        [DllImport("gdi32")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32")]
        public static extern IntPtr CreatePen(int penStyle, int width, int color);

        [DllImport("gdi32")]
        public static extern bool DeleteObject(IntPtr hgiobj);

        [DllImport("gdi32")]
        public static extern int SetROP2(IntPtr hdc, int fnDrawMode);
    }
}
