using System.Runtime.InteropServices;
using System.Windows;
using MediatR;

namespace PrettyPrintClipboardPls
{
    public class GetCursorPosHandler : IRequestHandler<GetCursorPosCommand, Point>
    {
        public Point Handle(GetCursorPosCommand message)
        {
            var pos = GetMousePosition();

            return new Point(pos.X, pos.Y);
        }

        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        }
    }
}