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
            calcInput.KeyDown += new System.EventHandler<KeyEventArgs>(InputHotkeys);
            Loaded += HideWindow;
            Activated += FocusInputBox;
            UpdateResult(this, EventArgs.Empty);
        }

        public void ClearInputBox(object? source, RoutedEventArgs args) {
            calcInput.Text = "";
            calcInput.Focus(NavigationMethod.Pointer);
        }
        public void HideWindow(object? source, EventArgs args) {
            Hide();
            ProcessOptimizer.Rest();
        }
        public void FocusInputBox(object? source, EventArgs args) {
            calcInput.Focus();
        }
        public void UpdateResult(object? source, EventArgs args) {
            calcOutput.Text = EquationParser.ParseText(calcInput.Text);
            if(calcOutput.Text.ToLower().Contains("error")) calcOutput.Opacity = 0.25;
            else calcOutput.Opacity = 1.0;
        }
        public void InputHotkeys(object? source, KeyEventArgs args) {
            if(args.Key == Key.Enter) {
                if(args.KeyModifiers == KeyModifiers.Control) {
                    Clipboard?.SetTextAsync(calcOutput.Text);
                }else if(args.KeyModifiers == KeyModifiers.None) {
                    Clipboard?.SetTextAsync(calcOutput.Text);
                    HideWindow(source, args);
                }
            }
            if(args.Key == Key.D && args.KeyModifiers == KeyModifiers.Control) {
                ClearInputBox(source, args);
            }
        }
    }
}