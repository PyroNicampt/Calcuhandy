using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace Calcuhandy.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            calcInput.TextChanged += new System.EventHandler<TextChangedEventArgs>(UpdateResult);
            PixelRect screenBound = Screens?.Primary?.Bounds ?? new PixelRect(0, 0, 600, 800);
            Width = screenBound.Width/2;
            Height = 200;
            Position = screenBound.Center - new PixelPoint((int)Width/2, (int)Height/2 + (int)(screenBound.Height*0.15));
        }

        public void ClearInputBox(object source, RoutedEventArgs args) {
            calcInput.Text = "";
        }
        public void ShowHistory(object source, RoutedEventArgs args) {
            
        }
        public void HideWindow(object? source, EventArgs args) {
            Hide();
        }

        public void UpdateResult(object? source, TextChangedEventArgs args) {
            calcOutput.Text = EquationParser.ParseText(calcInput.Text);
        }
    }
}