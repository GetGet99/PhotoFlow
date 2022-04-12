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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PhotoEditing
{
    public sealed partial class Pane : Grid
    {
        public Pane()
        {
            this.InitializeComponent();
            
        }
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Pane), new PropertyMetadata(null));
        public UIElementCollection Content
        {
            get => (UIElementCollection)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
        static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(UIElementCollection), typeof(Pane), new PropertyMetadata(null));
    }
}
