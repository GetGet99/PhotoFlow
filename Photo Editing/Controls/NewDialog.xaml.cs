using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;
// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PhotoEditing
{
    public sealed partial class NewDialog : ThemeContentDialog
    {
        public bool Success { get; private set; } = false;
        public ushort ImageWidth => Convert.ToUInt16(WidthTB.Text);
        public ushort ImageHeight => Convert.ToUInt16(HeightTB.Text);
        public string InitBackground => (InitBg.SelectedItem as RadioButton).Content.ToString();
        public NewDialog()
        {
            InitializeComponent();
            void HotKey(object _, KeyRoutedEventArgs _1)
            {
                bool GetKeyDown(VirtualKey key) => Window.Current.CoreWindow.GetKeyState(key).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

                bool Ctrl = GetKeyDown(VirtualKey.Control);
                bool Shift = GetKeyDown(VirtualKey.Shift);
    
                if (GetKeyDown(VirtualKey.Enter)) Create();
                else if (GetKeyDown(VirtualKey.Escape)) Cancel();
            }
            KeyDown += HotKey;
        }
        void Cancel()
        {
            Hide();
        }
        void Cancel(object o, RoutedEventArgs e) => Cancel();
        void Create()
        {
            if (HeightTB.Text == "" || WidthTB.Text == "")
            {
                Button b = new Button { Content = "I understand!" };
                var a = new StackPanel();
                a.Children.Add(new TextBlock { Text = "Please enter the complete Information" });
                a.Children.Add(b);
                Flyout flyout = new Flyout
                {
                    Content = a
                };
                b.Click += (o, e) => flyout.Hide();
                
                flyout.ShowAt(CreateButton);
            } else
            {
                Success = true;
                Hide();
            }
        }
        void Create(object o, RoutedEventArgs e) => Create();

        private void NumberOnlyFilter(TextBox sender, TextBoxBeforeTextChangingEventArgs args) => args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }
}
