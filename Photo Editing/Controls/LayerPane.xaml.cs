#nullable enable
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace PhotoFlow;

public sealed partial class LayerPane : Grid
{
    Symbol UpArrow => (Symbol)0xE971;
    Symbol DownArrow => (Symbol)0xE972;
    public LayerPane()
    {
        InitializeComponent();
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
