using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using System.Diagnostics;

namespace Calcuhandy {
    internal static class Utils {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out PixelPoint point);
        public static PixelPoint GetCursorPosition() {
            PixelPoint point;
            if(GetCursorPos(out point)) return point;
            return PixelPoint.Origin;
        }
        public delegate void QueuedAction();
        public static async void QueueAction(int delay, QueuedAction action) {
            await Task.Delay(delay);
            action();
        }
    }
}
