#nullable enable
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI;
using PhotoFlow.CommandButton.Controls;
using PhotoFlow.Layers;

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
            AddNewLayer(new RectangleLayer
            {
                Width = 100,
                Height = 100,
                Color = Colors.Black
            }.SetName("Rectangle"));
        };
        ShapeCommandBar.CreateEllipse.Click += delegate
        {
            AddNewLayer(new EllipseLayer
            {
                Width = 100,
                Height = 100,
                Color = Colors.Black
            }.SetName("Ellipse"));
        };
        void SetAcrylic(bool value)
        {
            if (CurrentLayer is not ShapeLayer Layer) return;
            var newColor = ShapeCommandBar.ColorPicker.Color;
            Layer.NewHistoryAction(LayerContainer.History, new HistoryAction<(bool Old, bool New, LayerContainer LayerContainer, uint LayerId)>(
                (Layer.Acrylic, value, LayerContainer, Layer.LayerId),
                Tag: this,
                Undo: x =>
                {
                    var (Old, _, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is ShapeLayer Layer)
                    {
                        Layer.Acrylic = Old;
                        _ = Layer.UpdatePreviewAsync();
                        InvokeLayerChange();
                    }
                },
                Redo: x =>
                {
                    var (_, New, LayerContainer, LayerId) = x;
                    if (LayerContainer?.GetLayerFromId(LayerId) is ShapeLayer Layer)
                    {
                        Layer.Acrylic = New;
                        _ = Layer.UpdatePreviewAsync();
                        InvokeLayerChange();
                    }
                }
            ));
            Layer.Acrylic = value;
        }
        ShapeCommandBar.Acrylic.Checked += delegate
        {
            if (CurrentLayer is ShapeLayer ShapeLayer)
            {
                SetAcrylic(true);
                ShapeCommandBar.TintOpacityField.Value = ShapeLayer.TintOpacity * 100;
            }
        };
        ShapeCommandBar.Acrylic.Unchecked += delegate
        {
            SetAcrylic(false);
        };
        DateTime OpacityTime = DateTime.MinValue, TintOpacityTime = DateTime.MinValue, ColorTime = DateTime.MinValue;
        HistoryActionMutable<(double Old, double New, LayerContainer LayerContainer, uint LayerId)>?
            OpacityHistoryAction = null, TintOpacityHistoryAction = null;
        HistoryActionMutable<(Color Old, Color New, LayerContainer LayerContainer, uint LayerId)> ColorHistoryAction = null;
        ShapeCommandBar.ColorPicker.ColorChanged += delegate
        {
            if (CurrentLayer is not ShapeLayer Layer) return;
            var newColor = ShapeCommandBar.ColorPicker.Color;
            if ((DateTime.Now - ColorTime).TotalSeconds > 2 || ColorHistoryAction is null)
            {
                ColorTime = DateTime.Now;
                ColorHistoryAction = new HistoryActionMutable<(Color Old, Color New, LayerContainer LayerContainer, uint LayerId)>(
                    (Layer.Color, newColor, LayerContainer, Layer.LayerId),
                    Tag: this,
                    Undo: x =>
                    {
                        var (OldColor, _, LayerContainer, LayerId) = x;
                        if (LayerContainer?.GetLayerFromId(LayerId) is ShapeLayer Layer)
                        {
                            Layer.Color = OldColor;
                            _ = Layer.UpdatePreviewAsync();
                            InvokeLayerChange();
                        }
                    },
                    Redo: x =>
                    {
                        var (_, NewColor, LayerContainer, LayerId) = x;
                        if (LayerContainer?.GetLayerFromId(LayerId) is ShapeLayer Layer)
                        {
                            Layer.Color = NewColor;
                            _ = Layer.UpdatePreviewAsync();
                            InvokeLayerChange();
                        }
                    }
                );
                Layer.NewHistoryAction(LayerContainer.History, ColorHistoryAction);
            }
            Layer.Color = newColor;
            var (a, b, c, d) = ColorHistoryAction.Param;
            ColorHistoryAction.Param = (a, newColor, c, d);
            _ = Layer.UpdatePreviewAsync();
        };
        ShapeCommandBar.OpacityField.ValueChanged += delegate
        {
            if (CurrentLayer is not ShapeLayer ShapeLayer) return;
            if ((DateTime.Now - OpacityTime).TotalSeconds > 2 || OpacityHistoryAction is null)
            {
                OpacityTime = DateTime.Now;
                OpacityHistoryAction = new(
                    (ShapeLayer.Opacity, ShapeLayer.Opacity, LayerContainer, ShapeLayer.LayerId),
                    Tag: this,
                    Undo: x =>
                    {
                        var (Old, New, LC, LID) = x;
                        if (LC.GetLayerFromId(LID) is not ShapeLayer Layer) return;
                        Layer.Opacity = Old;
                        _ = Layer.UpdatePreviewAsync();
                        InvokeLayerChange();
                    },
                    Redo: x =>
                    {
                        var (Old, New, LC, LID) = x;
                        if (LC.GetLayerFromId(LID) is not ShapeLayer Layer) return;
                        Layer.Opacity = New;
                        _ = Layer.UpdatePreviewAsync();
                        InvokeLayerChange();
                    }
                );
                ShapeLayer.NewHistoryAction(LayerContainer.History, OpacityHistoryAction);
            }
            ShapeLayer.Opacity = ShapeCommandBar.OpacityField.Value / 100;
            var (a, b, c, d) = OpacityHistoryAction.Param;
            OpacityHistoryAction.Param = (a, ShapeLayer.Opacity, c, d);
            _ = ShapeLayer.UpdatePreviewAsync();
        };
        ShapeCommandBar.TintOpacityField.ValueChanged += delegate
        {
            if (CurrentLayer is not ShapeLayer ShapeLayer) return;
            if ((DateTime.Now - TintOpacityTime).TotalSeconds > 2 || TintOpacityHistoryAction is null)
            {
                TintOpacityTime = DateTime.Now;
                TintOpacityHistoryAction = new(
                    (ShapeLayer.TintOpacity, ShapeLayer.TintOpacity, LayerContainer, ShapeLayer.LayerId),
                    Tag: this,
                    Undo: x =>
                    {
                        var (Old, New, LC, LID) = x;
                        if (LC.GetLayerFromId(LID) is not ShapeLayer Layer) return;
                        Layer.TintOpacity = Old;
                        _ = Layer.UpdatePreviewAsync();
                        InvokeLayerChange();
                    },
                    Redo: x =>
                    {
                        var (Old, New, LC, LID) = x;
                        if (LC.GetLayerFromId(LID) is not ShapeLayer Layer) return;
                        Layer.TintOpacity = New;
                        _ = Layer.UpdatePreviewAsync();
                        InvokeLayerChange();
                    }
                );
                ShapeLayer.NewHistoryAction(LayerContainer.History, TintOpacityHistoryAction);
            }
            ShapeLayer.TintOpacity = ShapeCommandBar.OpacityField.Value / 100;
            var (a, b, c, d) = TintOpacityHistoryAction.Param;
            TintOpacityHistoryAction.Param = (a, ShapeLayer.TintOpacity, c, d);
            _ = ShapeLayer.UpdatePreviewAsync();
        };
    }
    protected override void LayerChanged(Layer? Layer)
    {
        base.LayerChanged(Layer);
        if (Layer == null) return;
        ShapeCommandBar.LayerEditorControls.Visibility =
            Layer is ShapeLayer ? Visibility.Visible : Visibility.Collapsed;
        if (Layer is ShapeLayer ShapeLayer)
        {
            ShapeCommandBar.Acrylic.IsChecked = ShapeLayer.Acrylic;
            ShapeCommandBar.ColorPicker.Color = ShapeLayer.Color;
            ShapeCommandBar.OpacityField.Value = ShapeLayer.Opacity * 100;
            ShapeCommandBar.TintOpacityField.Value = ShapeLayer.TintOpacity * 100;
            ShapeCommandBar.PropertiesButton.Layer = ShapeLayer;
        }
    }
}
