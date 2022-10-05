#nullable enable
using CSharpUI;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using PhotoFlow.Layer;

namespace PhotoFlow.CommandButton.Controls;

public class PropertiesButton : Button
{
    public void ForceUpdateLayer() => LayerChanged?.Invoke();
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
    public PropertiesButton(History History)
    {
        CornerRadius = new CornerRadius(5);
        Content = "Properties";
        Flyout = new Flyout
        {
            Content = new PropertiesPanel(History).Edit(x => LayerChanged += () => x.Layer = Layer),
            Placement = FlyoutPlacementMode.Bottom
        };
    }
}
public class PropertiesPanel : StackPanel
{
    public event Action? EnvironmentChanged;
    Layer.Layer? _Layer;
    public Layer.Layer? Layer
    {
        get => _Layer; set
        {
            _Layer = value;
            EnvironmentChanged?.Invoke();
        }
    }
    void AddChildren(UIElement[] Elements)
    {
        foreach (var element in Elements)
            Children.Add(element);
    }
    void RecordNewTransformAction(LayerTransform Old, LayerTransform New)
    {
        if (!Layer.IsNotNull(out var layer)) return;
        History.NewAction(new HistoryAction<(LayerTransform Old, LayerTransform New)>(
            (Old, New),
            Tag: this,
            Undo: x =>
            {
                layer.Transform = x.Old;
            },
            Redo: x =>
            {
                layer.Transform = x.New;
            }
        ));
    }
    readonly History History;
    public PropertiesPanel(History History)
    {
        this.History = History;
        History.UndoCompleted += () => EnvironmentChanged?.Invoke();
        History.RedoCompleted += () => EnvironmentChanged?.Invoke();
        RegisterPropertyChangedCallback(OrientationProperty, delegate
        {
            var IsVertical = Orientation == Orientation.Vertical;
            Margin = IsVertical ? new Thickness(0, 0, 0, -10) : new Thickness(10, 0, 0, 0);
        });
        AddChildren(new UIElement[] {
            //new ComboBox
            //{
            //    Items =
            //    {
            //        "Transform"
            //    },
            //    SelectedIndex = 0,
            //    Margin = new Thickness(0, 0, 0, 10),
            //},
            CreatePropertyLayer(
                FieldName: "X",
                ValueChanged: x => {
                    if (Layer is null) return;
                    var oldTransform = Layer.Transform;
                    Layer.X = x.Value;
                    RecordNewTransformAction(oldTransform, Layer.Transform);
                }, EnvironmentChanged: x => {
                    if (Layer != null) x.Value = Layer.X;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Y",
                ValueChanged: x => {
                    if (Layer is null) return;
                    var oldTransform = Layer.Transform;
                    Layer.Y = x.Value;
                    RecordNewTransformAction(oldTransform, Layer.Transform);
                }, EnvironmentChanged: x => {
                    if (Layer != null) x.Value = Layer.Y;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Scale",
                ValueChanged: x => {
                    if (Layer is null) return;
                    var oldTransform = Layer.Transform;
                    Layer.CenterX = Layer.Width / 2;
                    Layer.CenterY = Layer.Height / 2;
                    Layer.ScaleX = Layer.ScaleY = x.Value;
                    RecordNewTransformAction(oldTransform, Layer.Transform);
                }, EnvironmentChanged: x => {
                    if (Layer != null) x.Value = (Layer.ScaleX + Layer.ScaleY) / 2;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Rotation",
                ValueChanged: x => {
                    if (Layer is null) return;
                    var oldTransform = Layer.Transform;
                    Layer.Rotation = x.Value;
                    RecordNewTransformAction(oldTransform, Layer.Transform);
                }, EnvironmentChanged: x => {
                    if (Layer != null) x.Value = Layer.Rotation;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Width",
                ValueChanged: x => {
                    if (Layer is null) return;
                    var oldTransform = Layer.Transform;
                    Layer.Width = x.Value;
                    RecordNewTransformAction(oldTransform, Layer.Transform);
                }, EnvironmentChanged: x => {
                    if (Layer != null) x.Value = Layer.Width;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Height",
                ValueChanged: x => {
                    if (Layer is null) return;
                    var oldTransform = Layer.Transform;
                    Layer.Height = x.Value;
                    RecordNewTransformAction(oldTransform, Layer.Transform);
                }, EnvironmentChanged: x => {
                    if (Layer != null) x.Value = Layer.Height;
                }
            ),
        });
        Orientation = Orientation.Horizontal; // so it updates
        Orientation = Orientation.Vertical;
    }
    Grid CreatePropertyLayer(string FieldName, Action<NumberBox>? ValueChanged, Action<NumberBox>? EnvironmentChanged)
        => new Grid
        {
            ColumnDefinitions = {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
            },
            Children =
            {
                    new Grid
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = FieldName
                            }
                        }
                    }.Edit(x => Grid.SetColumn(x, 0)),
                    new Grid
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                TextAlignment = TextAlignment.Left
                            }.Assign(out var TextBlock)
                        }
                    }.Edit(x => Grid.SetColumn(x, 1)),
                    new NumberBox
                    {
                        Width = 100,
                        VerticalAlignment = VerticalAlignment.Center
                    }.Edit(x =>
                    {
                        bool LayerChanging = false;
                        x.ValueChanged += delegate
                        {
                            if (LayerChanging) return;
                            ValueChanged?.Invoke(x);
                        };
                        this.EnvironmentChanged += delegate
                        {
                            LayerChanging = true;
                            EnvironmentChanged?.Invoke(x);
                            LayerChanging = false;
                        };
                        Grid.SetColumn(x, 2);
                    })
                    .Assign(out var NumberBox)
            }
        }
        .Edit(x =>
        {
            RegisterPropertyChangedCallback(OrientationProperty, delegate
            {
                var IsVertical = Orientation == Orientation.Vertical;
                x.HorizontalAlignment = IsVertical ?
                    HorizontalAlignment.Stretch :
                    HorizontalAlignment.Left;

                x.ColumnDefinitions[1].Width =
                    IsVertical ?
                    new GridLength(1, GridUnitType.Star) :
                    GridLength.Auto;

                x.ColumnDefinitions[1].MinWidth =
                    IsVertical ? 30 : 0;

                x.Margin = IsVertical ? new Thickness(0, 0, 0, 10) : new Thickness(0);

                TextBlock.Text = IsVertical ? "" : ":";
                NumberBox.Margin = IsVertical ? new Thickness(0) : new Thickness(10, 0, 10, 0);
            });
        });
}