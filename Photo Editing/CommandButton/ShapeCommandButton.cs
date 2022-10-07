#nullable enable
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
using Windows.UI;
using PhotoFlow.CommandButton.Controls;

namespace PhotoFlow;

public class ShapeCommandButton : CommandButtonBase
{
    private readonly Shape ShapeCommandBar;
    protected override CommandButtonCommandBar CommandBar => ShapeCommandBar;

    public ShapeCommandButton(ScrollViewer CommandBarPlace, LayerContainer LayerContainer, ScrollViewer MainScrollViewer) : base(Symbol.Stop, CommandBarPlace, LayerContainer, MainScrollViewer)
    {
        ShapeCommandBar = new Shape(LayerContainer.History);
        ShapeCommandBar.CreateRectangle.Click += delegate
        {
            AddNewLayer(new Layers.RectangleLayer
            {
                Width = 100,
                Height = 100,
                Color = Colors.Black
            }.SetName("Rectangle"));
        };
        ShapeCommandBar.CreateEllipse.Click += delegate
        {
            AddNewLayer(new Layers.EllipseLayer
            {
                Width = 100,
                Height = 100,
                Color = Colors.Black
            }.SetName("Ellipse"));
        };
        void SetAcrylic(bool value)
        {
            if (CurrentLayer is not Layers.ShapeLayer Layer) return;
            var newColor = ShapeCommandBar.ColorPicker.Color;
            LayerContainer.History.NewAction(new HistoryAction<(bool Old, bool New, LayerContainer LayerContainer, uint LayerId)>(
                (Layer.Acrylic, value, LayerContainer, Layer.LayerId),
                Tag: this,
                Undo: x =>
                {
                    var (Old, _, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is Layers.ShapeLayer Layer)
                    {
                        Layer.Acrylic = Old;
                    }
                },
                Redo: x =>
                {
                    var (_, New, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is Layers.ShapeLayer Layer)
                    {
                        Layer.Acrylic = New;
                    }
                }
            ));
            Layer.Acrylic = value;
        }
        ShapeCommandBar.Acrylic.Checked += delegate
        {
            if (CurrentLayer is Layers.ShapeLayer ShapeLayer)
            {
                SetAcrylic(true);
                ShapeCommandBar.TintOpacityField.Value = ShapeLayer.TintOpacity * 100;
            }
        };
        ShapeCommandBar.Acrylic.Unchecked += delegate
        {
            SetAcrylic(false);
        };
        ShapeCommandBar.ColorPicker.ColorChanged += delegate
        {
            if (CurrentLayer is not Layers.ShapeLayer Layer) return;
            var newColor = ShapeCommandBar.ColorPicker.Color;
            LayerContainer.History.NewAction(new HistoryAction<(Color Old, Color New, LayerContainer LayerContainer, uint LayerId)>(
                (Layer.Color, newColor, LayerContainer, Layer.LayerId),
                Tag: this,
                Undo: x =>
                {
                    var (OldColor, _, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is Layers.ShapeLayer Layer)
                    {
                        Layer.Color = OldColor;
                    }
                },
                Redo: x =>
                {
                    var (_, NewColor, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is Layers.ShapeLayer Layer)
                    {
                        Layer.Color = NewColor;
                    }
                }
            ));
            Layer.Color = newColor;
        };
        DateTime OpacityTime = DateTime.MinValue, TintOpacityTime = DateTime.MinValue;
        HistoryActionMutable<(double Old, double New, LayerContainer LayerContainer, uint LayerId)>?
            OpacityHistoryAction = null, TintOpacityHistoryAction = null;
        ShapeCommandBar.OpacityField.ValueChanged += delegate
        {
            if (CurrentLayer is not Layers.ShapeLayer ShapeLayer) return;
            if ((DateTime.Now - OpacityTime).TotalSeconds > 2 || OpacityHistoryAction is null)
            {
                OpacityHistoryAction = new(
                    (ShapeLayer.Opacity, ShapeLayer.Opacity, LayerContainer, ShapeLayer.LayerId),
                    Tag: this,
                    Undo: x =>
                    {
                        var (Old, New, LC, LID) = x;
                        if (LC.GetLayerFromId(LID) is Layers.ShapeLayer Layer)
                            Layer.Opacity = Old;
                    },
                    Redo: x =>
                    {
                        var (Old, New, LC, LID) = x;
                        if (LC.GetLayerFromId(LID) is Layers.ShapeLayer Layer)
                            Layer.Opacity = New;
                    }
                );
                LayerContainer.History.NewAction(OpacityHistoryAction);
            }
            ShapeLayer.Opacity = ShapeCommandBar.OpacityField.Value / 100;
            var (a, b, c, d) = OpacityHistoryAction.Param;
            OpacityHistoryAction.Param = (a, ShapeLayer.Opacity, c, d);
        };
        ShapeCommandBar.TintOpacityField.ValueChanged += delegate
        {
            if (CurrentLayer is not Layers.ShapeLayer ShapeLayer) return;
            if ((DateTime.Now - TintOpacityTime).TotalSeconds > 2 || TintOpacityHistoryAction is null)
            {
                TintOpacityHistoryAction = new(
                    (ShapeLayer.TintOpacity, ShapeLayer.TintOpacity, LayerContainer, ShapeLayer.LayerId),
                    Tag: this,
                    Undo: x =>
                    {
                        var (Old, New, LC, LID) = x;
                        if (LC.GetLayerFromId(LID) is Layers.ShapeLayer Layer)
                            Layer.TintOpacity = Old;
                    },
                    Redo: x =>
                    {
                        var (Old, New, LC, LID) = x;
                        if (LC.GetLayerFromId(LID) is Layers.ShapeLayer Layer)
                            Layer.TintOpacity = New;
                    }
                );
                LayerContainer.History.NewAction(TintOpacityHistoryAction);
            }
            ShapeLayer.TintOpacity = ShapeCommandBar.OpacityField.Value / 100;
            var (a, b, c, d) = TintOpacityHistoryAction.Param;
            TintOpacityHistoryAction.Param = (a, ShapeLayer.TintOpacity, c, d);
        };
    }
    protected override void LayerChanged(Layers.Layer? Layer)
    {
        base.LayerChanged(Layer);
        if (Layer == null) return;
        ShapeCommandBar.LayerEditorControls.Visibility =
            Layer is Layers.ShapeLayer ? Visibility.Visible : Visibility.Collapsed;
        if (Layer is Layers.ShapeLayer ShapeLayer)
        {
            ShapeCommandBar.Acrylic.IsChecked = ShapeLayer.Acrylic;
            ShapeCommandBar.ColorPicker.Color = ShapeLayer.Color;
            ShapeCommandBar.OpacityField.Value = ShapeLayer.Opacity * 100;
            ShapeCommandBar.TintOpacityField.Value = ShapeLayer.TintOpacity * 100;
            ShapeCommandBar.PropertiesButton.Layer = ShapeLayer;
        }
    }
}
