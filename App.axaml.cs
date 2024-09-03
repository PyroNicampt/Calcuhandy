using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Calcuhandy.ViewModels;
using Calcuhandy.Views;
using System;

namespace Calcuhandy {
    public partial class App : Application {
        static Window? mainWindow;
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
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
            }

            base.OnFrameworkInitializationCompleted();
        }
        public void CloseApp(object? source, EventArgs args) {
            mainWindow?.Close();
        }
        public void ShowApp(object? source, EventArgs args) {
            mainWindow?.Show();
        }
    }
}