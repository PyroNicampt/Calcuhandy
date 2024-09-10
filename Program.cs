using Avalonia;
using System;
using System.Threading;

namespace Calcuhandy {
    internal sealed class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            //Mutex name is appended with the MD5 of "PyroNicamptCalcuhandy"
            using(Mutex lockMutex = new Mutex(false, "Calcuhandy_55afd9bf")) {
                if(!lockMutex.WaitOne(0, false)) {
                    return;
                }
                BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
