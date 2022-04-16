using System.Collections.ObjectModel;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace PhotoFlow
{
    partial class MainPage
    {
        public ButtonCollection Buttons { get; private set; }
        public class ButtonCollection : ObservableCollection<CommandButtonBase>
        {
            public VariableUpdateAlert<CommandButtonBase> Selection { get; } = new VariableUpdateAlert<CommandButtonBase>();

            //readonly LayerContainer LayerContainer;
            public ButtonCollection(LayerContainer LayerContainer, ScrollViewer MainScrollViewer)
            {
                Selection.Update += (oldValue, newValue) =>
                {
                    oldValue?.Deselect();
                    newValue?.Select();
                };
                //this.LayerContainer = LayerContainer;
                CollectionChanged += (o, e) =>
                {
                    foreach (var nI in e.NewItems)
                    {
                        var nI2 = (CommandButtonBase)nI;
                        nI2.SetLayerContainer(LayerContainer);
                        nI2.SetScrollViewer(MainScrollViewer);
                    }
                };
            }
            //public void HotKey(VirtualKey Key)
            //{

            //}
            public void InvokeLayerChange()
            {
                var val = Selection.GetValue();
                if (val != null) val.InvokeLayerChange();
            }
        }
        void InitializeCommandButtons()
        {
            Buttons = new ButtonCollection(LayerContainer, MainScrollView)
            {
                new MoveCommandButton(CommandBarPlace),
                new InkingCommandButton(CommandBarPlace),
                new ImageCommandButton(CommandBarPlace),
                new TextCommandButton(CommandBarPlace),
                new ShapeCommandButton(CommandBarPlace),
            }
            ;
            CommandBar.SelectionChanged += (o, e) =>
            {
                Buttons.Selection.Value = (CommandButtonBase)CommandBar.SelectedItem;
            };
            
            LayerContainer.SelectionUpdate += (oldVal, newVal) => Buttons.InvokeLayerChange();
            CommandBar.SelectedIndex = 0;
        }
    }
}
