#nullable enable
using Microsoft.UI.Xaml.Controls;
using PhotoFlow.CommandButton.Controls;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using CSharpUI;
using ColorPicker = Microsoft.UI.Xaml.Controls.ColorPicker;
using Windows.UI.Xaml.Media;

namespace PhotoFlow;

public class TextCommandButton : CommandButtonBase
{
    private readonly Text TextCommandBar = new();
    protected override CommandButtonCommandBar CommandBar => TextCommandBar;

    public TextCommandButton(Border CommandBarPlace) : base(Symbol.Font, CommandBarPlace)
    {
        TextCommandBar.CreateNewLayer.Click += (_, _1) =>
        {
            var newLayer = new Layer.TextLayer(new Windows.Foundation.Point(0, 0), "Text");
            newLayer.LayerName.Value = "Text Layer";
            AddNewLayer(newLayer);
        };
        TextCommandBar.TextBox.TextChanged += (_, _1) =>
        {
            if (CurrentLayer is Layer.TextLayer Layer)
                Layer.Text = TextCommandBar.TextBox.Text;
        };
        TextCommandBar.Font.TextChanged += (_, e) =>
        {
            if (e.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen)
            {
                if (CurrentLayer is Layer.TextLayer Layer)
                    Layer.Font = new Windows.UI.Xaml.Media.FontFamily(TextCommandBar.Font.Text);
            }
        };
        TextCommandBar.FontSize.ValueChanged += (_, e) =>
        {
            if (CurrentLayer is Layer.TextLayer Layer)
                Layer.FontSize = TextCommandBar.FontSize.Value;
        };
        //TextCommandBar.Font.LostFocus += (_, _1) =>
        //{
        //    if (CurrentLayer is Layer.TextLayer Layer)
        //        TextCommandBar.Font.Text = Layer.Font.Source;
        //};
    }
    protected override void LayerChanged(Layer.Layer Layer)
    {
        base.LayerChanged(Layer);
        if (Layer == null) return;
        TextCommandBar.LayerEditorControls.Visibility =
            Layer.LayerType == PhotoFlow.Layer.Types.Text ? Visibility.Visible : Visibility.Collapsed;
        if (Layer is Layer.TextLayer TextLayer)
        {
            TextCommandBar.TextBox.Text = TextLayer.Text;
            TextCommandBar.Font.Text = TextLayer.Font.Source;
            TextCommandBar.ColorPicker.Color = TextLayer.TextColor;
            if (TextLayer.FontSize != null) TextCommandBar.FontSize.Value = TextLayer.FontSize.Value;
        }
    }
}
class Text : CommandButtonCommandBar
{
    static readonly Thickness DefaultThickness = new(16, 0, 16, 0);
    public Button CreateNewLayer, ColorPickerButton;
    public ColorPicker ColorPicker;
    public StackPanel LayerEditorControls;
    public TextBox TextBox;
    public AutoSuggestBox Font;
    public NumberBox FontSize;
    public Text()
    {
        const VerticalAlignment Center = VerticalAlignment.Center;
        Children.Add(new Button
        {
            Content = "Create New Text Layer",
            Margin = new Thickness(0, 0, 10, 0)
        }.Assign(out CreateNewLayer));
        Children.Add(
            new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = Center,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Text:",
                        VerticalAlignment = Center
                    },
                    new TextBox
                    {
                        Margin = DefaultThickness,
                        Width = 300
                    }.Assign(out TextBox),
                    new TextBlock
                    {
                        Text = "Font:",
                        VerticalAlignment = Center
                    },
                    new AutoSuggestBox
                    {
                        Margin = DefaultThickness,
                        Width = 200
                    }.Assign(out Font),
                    new TextBlock
                    {
                        Text = "Font Size:",
                        VerticalAlignment = Center
                    },
                    new NumberBox
                    {
                        Margin = DefaultThickness,
                        Width = 100
                    }.Assign(out FontSize),
                    new Button
                    {
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(16, 0, 16, 0),
                        Flyout = new Flyout
                        {
                            Content = new ColorPicker
                            {
                                IsAlphaEnabled = true
                            }
                            .Edit(x =>
                            {
                                x.ColorChanged += delegate
                                {
                                    if (ColorPickerButton != null)
                                        ColorPickerButton.Background = new SolidColorBrush(x.Color);
                                };
                            })
                        }
                    }
                    .Assign(out ColorPickerButton)
                }
            }.Assign(out LayerEditorControls)
        );
        Font.ItemsSource = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();

    }
}
