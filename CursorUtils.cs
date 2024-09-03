using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;

namespace Calcuhandy {
    internal static class CursorUtils {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out PixelPoint point);
        public static PixelPoint GetCursorPosition() {
            PixelPoint point;
            if(GetCursorPos(out point)) return point;
            return PixelPoint.Origin;
        }
    }
}
