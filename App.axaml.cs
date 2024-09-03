using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Calcuhandy.ViewModels;
using Calcuhandy.Views;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Calcuhandy {
    public partial class App : Application {
        static Window? mainWindow;
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
            ProgramHotkeys.Init();
        }
        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.MainWindow = new MainWindow {
                    DataContext = new MainWindowViewModel(),
                };
                mainWindow = desktop.MainWindow;

                //Events
                mainWindow.Deactivated += HideApp;
                ProgramHotkeys.manager.HotkeyHide += HideApp;
                ProgramHotkeys.manager.HotkeyOpen += ShowApp;
            }

            base.OnFrameworkInitializationCompleted();
        }
        public void CloseApp(object? source, EventArgs args) {
            Dispatcher.UIThread.InvokeAsync(() => mainWindow?.Close());
        }
        public void ShowApp(object? source, EventArgs args) {
            /* Look into other options first, this seems like a lot of faff
            if(source?.GetType() == typeof(TrayIcon)) {
                if(ActualThemeVariant == ThemeVariant.Dark) {
                    ((TrayIcon)source).Icon = new WindowIcon()
                }
            }*/
            ProcessOptimizer.Wakeup();
            Dispatcher.UIThread.InvokeAsync(() => {
                if(mainWindow == null) return;
                if(source?.GetType() == typeof(TrayIcon)) SetWindowToCursor(mainWindow, 0.5, 0.5);
                else SetWindowToCursor(mainWindow);

                if(!mainWindow.IsEffectivelyVisible) {
                    mainWindow.Show();
                    mainWindow.Focus();
                    mainWindow.GetControl<TextBox>("calcInput")?.Focus();
                }
            });
        }
        private void SetWindowToCursor(Window window, double? forceX = null, double? forceY = null) {
            if(window == null) return;

            //PixelPoint cursorPos = new(ProgramHotkeys.manager.mouseX, ProgramHotkeys.manager.mouseY);
            PixelPoint cursorPos = CursorUtils.GetCursorPosition();
            PixelRect screenBounds = window.Screens.ScreenFromPoint(cursorPos)?.Bounds ?? new PixelRect(0, 0, 800, 600);
            //window.Width = screenBounds.Width * 0.5;

            double correctX = cursorPos.X - window.Width * Settings.popupOffsetPercentX;
            if(correctX < screenBounds.X + Settings.popupMargin) correctX = screenBounds.X + Settings.popupMargin;
            if(correctX + window.Width > screenBounds.Right - Settings.popupMargin) correctX = screenBounds.Right - window.Width - Settings.popupMargin;
            
            double correctY = cursorPos.Y - window.Height * Settings.popupOffsetPercentY;
            if(correctY < screenBounds.Y + Settings.popupMargin) correctY = screenBounds.Y + Settings.popupMargin;
            if(correctY + window.Height > screenBounds.Bottom - Settings.popupMargin) correctY = screenBounds.Bottom - window.Height - Settings.popupMargin;

            if(forceX != null) correctX = (double)forceX * screenBounds.Width + screenBounds.X - window.Width * 0.5;
            if(forceY != null) correctY = (double)forceY * screenBounds.Height + screenBounds.Y - window.Height * 0.5;
            window.Position = new PixelPoint((int)correctX, (int)correctY);
        }
        public void HideApp(object? source, EventArgs args) {
            Dispatcher.UIThread.InvokeAsync(() => mainWindow?.Hide());
            ProcessOptimizer.Rest();
        }
    }
}