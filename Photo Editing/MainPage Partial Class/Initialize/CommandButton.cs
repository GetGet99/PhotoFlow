#nullable enable
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace PhotoFlow;

partial class MainPage
{
    public ButtonCollection Buttons { get; private set; }
    public class ButtonCollection : ObservableCollection<CommandButtonBase>
    {
        public VariableUpdateAlert<CommandButtonBase?> Selection { get; } = new(null);

        //readonly LayerContainer LayerContainer;
        public ButtonCollection()
        {
            Selection.Update += (oldValue, newValue) =>
            {
                oldValue?.Deselect();
                newValue?.Select();
            };
        }
        public void InvokeLayerChange()
        {
            var val = Selection.GetValue();
            if (val != null) val.InvokeLayerChange();
        }
    }
    [MemberNotNull(nameof(Buttons))]
    void InitializeCommandButtons()
    {
        Buttons = new ButtonCollection()
        {
            new MoveCommandButton(CommandBarPlace, LayerContainer, MainScrollView),
            new PropertiesCommandButton(CommandBarPlace, LayerContainer, MainScrollView),
            new InkingCommandButton(CommandBarPlace, LayerContainer, MainScrollView),
            new ImageCommandButton(CommandBarPlace, LayerContainer, MainScrollView),
            new TextCommandButton(CommandBarPlace, LayerContainer, MainScrollView),
            new ShapeCommandButton(CommandBarPlace, LayerContainer, MainScrollView),
        };
        CommandBar.SelectionChanged += (o, e) =>
        {
            Buttons.Selection.Value = (CommandButtonBase)CommandBar.SelectedItem;
        };

        LayerContainer.SelectionUpdate += (oldVal, newVal) => Buttons.InvokeLayerChange();
        CommandBar.SelectedIndex = 0;
    }
}
