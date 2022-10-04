#nullable enable
using CSharpUI;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

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
    public PropertiesButton()
    {
        CornerRadius = new CornerRadius(5);
        Content = "Properties";
        Flyout = new Flyout
        {
            Content = new PropertiesPanel().Edit(x => LayerChanged += () => x.Layer = Layer),
            Placement = FlyoutPlacementMode.Bottom
        };
    }
}
public class PropertiesPanel : StackPanel
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
    void AddChildren(UIElement[] Elements)
    {
        foreach (var element in Elements)
            Children.Add(element);
    }
    public PropertiesPanel()
    {
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
                    if (Layer != null) Layer.X = x.Value;
                }, LayerChanged: x => {
                    if (Layer != null) x.Value = Layer.X;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Y",
                ValueChanged: x => {
                    if (Layer != null) Layer.Y = x.Value;
                }, LayerChanged: x => {
                    if (Layer != null) x.Value = Layer.Y;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Scale",
                ValueChanged: x => {
                    if (Layer != null) {
                        Layer.CenterX = Layer.Width / 2;
                        Layer.CenterY = Layer.Height / 2;
                        Layer.ScaleX = Layer.ScaleY = x.Value;
                    }
                }, LayerChanged: x => {
                    if (Layer != null) x.Value = (Layer.ScaleX + Layer.ScaleY) / 2;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Rotation",
                ValueChanged: x => {
                    if (Layer != null) Layer.Rotation = x.Value;
                }, LayerChanged: x => {
                    if (Layer != null) x.Value = Layer.Rotation;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Width",
                ValueChanged: x => {
                    if (Layer != null) Layer.Width = x.Value;
                }, LayerChanged: x => {
                    if (Layer != null) x.Value = Layer.Width;
                }
            ),
            CreatePropertyLayer(
                FieldName: "Height",
                ValueChanged: x => {
                    if (Layer != null) Layer.Height = x.Value;
                }, LayerChanged: x => {
                    if (Layer != null) x.Value = Layer.Height;
                }
            ),
        });
        Orientation = Orientation.Horizontal; // so it updates
        Orientation = Orientation.Vertical;
    }
    Grid CreatePropertyLayer(string FieldName, Action<NumberBox>? ValueChanged, Action<NumberBox>? LayerChanged)
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
                        x.ValueChanged += delegate
                        {
                            ValueChanged?.Invoke(x);
                        };
                        this.LayerChanged += delegate
                        {
                            LayerChanged?.Invoke(x);
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