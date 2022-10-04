#nullable enable
using CSharpUI;
using OpenCvSharp;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using PhotoFlow.CommandButton.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using System;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace PhotoFlow;

public class PropertiesCommandButton : CommandButtonBase
{

    private readonly PropertiesCB PropertiesCommandBar = new();
    protected override CommandButtonCommandBar CommandBar => PropertiesCommandBar;

    public PropertiesCommandButton(Border CommandBarPlace, LayerContainer LayerContainer, ScrollViewer MainScrollViewer) : base(Symbol.Repair, CommandBarPlace, LayerContainer, MainScrollViewer)
    {

    }

    protected override void LayerChanged(Layer.Layer Layer)
    {
        PropertiesCommandBar.Layer = Layer;
        base.LayerChanged(Layer);
    }


    class PropertiesCB : CommandButtonCommandBar
    {
        public event Action? LayerChanged;
        Layer.Layer? _Layer;
        public Layer.Layer? Layer
        {
            get => _Layer; set
            {
                _Layer = value;
                LayerChanged?.Invoke();
            }
        }
        public PropertiesCB()
        {

            Children.Add(new PropertiesPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            }.Edit(x => LayerChanged += () => x.Layer = Layer));

            //Children.Add(new PropertiesButton
            //{
            //    Margin = new Thickness(0, 0, 10, 0)
            //}.Edit(x => LayerChanged += () => x.Layer = Layer));
        }
    }
}