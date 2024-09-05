using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcuhandy {
    internal static class ProcessOptimizer {
        private static ProcessPriorityClass prevPriority = ProcessPriorityClass.Normal;
        public static void Rest() {
            Process curProcess = Process.GetCurrentProcess();
            prevPriority = curProcess.PriorityClass;
            curProcess.PriorityClass = ProcessPriorityClass.Idle;
        }
        public static void Wakeup() {
            Process curProcess = Process.GetCurrentProcess();
            curProcess.PriorityClass = prevPriority;
        }
    }
}
