using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using PhotoFlow.CommandButton.Controls;

namespace PhotoFlow
{
    public class TextCommandButton : CommandButtonBase
    {
        private readonly Text TextCommandBar = new Text();
        protected override CommandButtonCommandBar CommandBar => TextCommandBar;

        public TextCommandButton(Border CommandBarPlace) : base(Symbol.Font, CommandBarPlace)
        {
            TextCommandBar.CreateNewLayer.Click += (_, _1) => {
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
                TextCommandBar.FontSize.Value = TextLayer.FontSize.Value;
            }
        }
    }
}
