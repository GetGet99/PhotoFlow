﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PhotoEditing
{
    public sealed partial class RenameDialog : ThemeContentDialog
    {
        public bool Success { get; private set; } = false;
        public string NewName => Textbox.Text;
        public RenameDialog(string OldName)
        {
            InitializeComponent();
            Textbox.Text = OldName;
            void HotKey(object Sender, KeyRoutedEventArgs e)
            {
                bool GetKeyDown(VirtualKey key) => Window.Current.CoreWindow.GetKeyState(key).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

                if (GetKeyDown(VirtualKey.Enter)) Rename();
                else if (GetKeyDown(VirtualKey.Escape)) Cancel();
            }
            KeyDown += HotKey;
        }
        void Rename(object o, RoutedEventArgs e) => Rename();
        void Rename()
        {
            Success = true;
            if (Textbox.Text == "")
            {
                Button b = new Button { Content = "I understand!" };
                var a = new StackPanel();
                a.Children.Add(new TextBlock { Text = "Please don't leave anything blank" });
                a.Children.Add(b);
                Flyout flyout = new Flyout
                {
                    Content = a
                };
                b.Click += (o, e) => flyout.Hide();

                flyout.ShowAt(RenameButton);
            }
            else
            {
                Success = true;
                Hide();
            }
            Hide();
        }
        void Cancel()
        {
            Success = false;
            Hide();
        }
        void Cancel(object o, RoutedEventArgs e) => Cancel();
    }
}
