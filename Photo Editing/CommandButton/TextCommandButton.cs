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
using PhotoEditing.CommandButton.Controls;

namespace PhotoEditing
{
    public class TextCommandButton : CommandButtonBase
    {
        private readonly Text TextCommandBar = new Text();
        protected override CommandButtonCommandBar CommandBar => TextCommandBar;

        public TextCommandButton(Border CommandBarPlace) : base(Symbol.Font, CommandBarPlace)
        {
            TextCommandBar.CreateNewLayer.Click += (_, _1) => AddNewLayer(new Layer.TextLayer(new Windows.Foundation.Point(0,0), "Test Text"));
        }
        protected override void RequestAddLayerEvent(Layer.Layer Layer)
        {
            base.RequestAddLayerEvent(Layer);
            if (Layer.LayerType == PhotoEditing.Layer.Types.Text)
            {
                (Layer as Layer.TextLayer).TextBox.IsEnabled = true;
            }
        }
        protected override void RequestRemoveLayerEvent(Layer.Layer Layer)
        {
            base.RequestRemoveLayerEvent(Layer);
            if (Layer.LayerType == PhotoEditing.Layer.Types.Text)
            {
                (Layer as Layer.TextLayer).TextBox.IsEnabled = false;
            }
        }
    }
}
