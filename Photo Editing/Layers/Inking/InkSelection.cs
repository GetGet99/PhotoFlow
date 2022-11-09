#nullable enable
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using System.Linq;
using Windows.Foundation;
using System.Collections.Generic;
using Windows.UI.ViewManagement;

namespace PhotoFlow.Layers;

partial class InkingLayer
{
    void OnCreateSelection()
    {
        SelectionRectangle.RenderTransform = SelectionRenderTransform;
        SelectionRectangle.Stroke = SelectionColor;

        InkRef[] selectedInks = new InkRef[0];
        Point deltaManipulation = new();
        // Manipulation Event
        {
            SelectionRectangle.ManipulationStarted += (_, e) =>
            {
                e.Handled = true;
                deltaManipulation = new(0, 0);
                selectedInks = InkRefTracker.GetRefs(
                    from x in InkCanvas.InkPresenter.StrokeContainer.GetStrokes()
                    where x.Selected
                    select x
                );
            };
            SelectionRectangle.ManipulationDelta += (_, e) =>
            {
                e.Handled = true;
                float ZoomFactor = LayerContainer?.ZoomFactor ?? 1;
                Point d = e.Delta.Translation;
                double TwoPiOver360 = 2 * Math.PI / 360;
                var sin = Math.Sin(Rotation * TwoPiOver360);
                var cos = Math.Cos(Rotation * TwoPiOver360);
                d = new Point(
                    d.X * cos + d.Y * sin,
                    d.X * sin + d.Y * cos
                );
                d.X /= ZoomFactor;
                d.Y /= ZoomFactor;

                var SelectionBounds = this.SelectionBounds;
                //Point newPos = new(SelectionBounds.X + d.X, SelectionBounds.Y + d.Y);
                deltaManipulation = new(deltaManipulation.X + d.X, deltaManipulation.Y + d.Y);
                //var newBounds = new Windows.Foundation.Rect(newPos,
                //    new Size(SelectionBounds.Width, SelectionBounds.Height));
                UpdateInkSelectionRectangle(
                    InkCanvas.InkPresenter.StrokeContainer.MoveSelected(d)
                );
            };
            SelectionRectangle.ManipulationCompleted += delegate
            {
                if (LayerContainer is not null)
                    NewHistoryAction(LayerContainer.History, new HistoryAction<(LayerContainer LayerContainer, uint LayerId, InkRef[] InkRef, Point Dm)>(
                        (LayerContainer, LayerId, selectedInks, deltaManipulation),
                        Tag: this,
                        Undo: x =>
                        {
                            var (LayerContainer, LayerId, InkStrokes, Dm) = x;
                            if (LayerContainer.GetLayerFromId(LayerId) is not InkingLayer Layer) return;
                            if (Layer.SelectionIndexUpdateTarget is not null)
                                Layer.SelectionIndexUpdateTarget.Value = Layer.Index;
                            Layer.ClearInkSelection();
                            foreach (var ink in InkStrokes) ink.InkStroke.Selected = true;
                            Layer.UpdateInkSelectionRectangle(
                                Layer.InkCanvas.InkPresenter.StrokeContainer.MoveSelected(
                                    new Point(-Dm.X, -Dm.Y)
                                )
                            );
                            Layer.UpdatePreview();
                        },
                        Redo: x =>
                        {
                            var (LayerContainer, LayerId, InkStrokes, Dm) = x;
                            if (LayerContainer.GetLayerFromId(LayerId) is not InkingLayer Layer) return;
                            if (Layer.SelectionIndexUpdateTarget is not null)
                                Layer.SelectionIndexUpdateTarget.Value = Layer.Index;
                            Layer.ClearInkSelection();
                            foreach (var ink in InkStrokes) ink.InkStroke.Selected = true;
                            Layer.UpdateInkSelectionRectangle(
                                Layer.InkCanvas.InkPresenter.StrokeContainer.MoveSelected(
                                    new Point(Dm.X, Dm.Y)
                                )
                            );
                            Layer.UpdatePreview();
                        },
                        DisposeParam: x => { foreach (var stroke in x.InkRef) stroke.MarkUnused(); }
                    ));
            };
        }
        CommandBarFlyout? C = null;
        InkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;
        InkCanvas.InkPresenter.UnprocessedInput.PointerPressed += (_, e) =>
        {
            if (e.CurrentPoint.Properties.IsRightButtonPressed)
            {
                if (CanPasteInk)
                    new MenuFlyout
                    {
                        Items =
                            {
                                new MenuFlyoutItem
                                {
                                    Text = "Paste",
                                    Icon = new SymbolIcon(Symbol.Paste),
                                    Command = new LambdaCommand(() => PasteInkAt(e.CurrentPoint.Position))
                                }
                            }
                    }.ShowAt(InkCanvas, e.CurrentPoint.Position);
            }
        };
        SelectionRectangle.ContextFlyout = C = new CommandBarFlyout
        {
            PrimaryCommands =
                {
                    new AppBarButton
                    {
                        Label = "Cut",
                        Icon = new SymbolIcon(Symbol.Cut),
                        Command = new LambdaCommand(() => {
                            RequestCut();
                            C?.Hide();
                        })
                    },
                    new AppBarButton
                    {
                        Label = "Copy",
                        Icon = new SymbolIcon(Symbol.Copy),
                        Command = new LambdaCommand(() => {
                            RequestCopy();
                            C?.Hide();
                        })
                    },
                    new AppBarButton
                    {
                        Label = "Duplicate",
                        Icon = new SymbolIcon(Symbol.Copy),
                        Command = new LambdaCommand(() => {
                            RequestDuplicate();
                            C?.Hide();
                        })
                    },
                    new AppBarButton
                    {
                        Label = "Delete",
                        Icon = new SymbolIcon(Symbol.Delete),
                        Command = new LambdaCommand(() =>
                        {
                            RequestDelete();
                            C?.Hide();
                        })
                    }
                }
        };

        LayerUIElement.SizeChanged += delegate
        {
            Canvas.Width = Width;
            Canvas.Height = Height;
        };
        Canvas.Width = Width;
        Canvas.Height = Height;

        SelectionRectangle.RenderTransform = SelectionRenderTransform;
        SelectionRectangle.PointerEntered += delegate
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 1);
        };
        SelectionRectangle.PointerEntered += delegate
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        };
    }
    public void SelectionPreviewClear()
    {
        SelectionPolygon.Points.Clear();
    }
    public void SelectionPreviewAdd(Point pt)
    {
        SelectionPolygon.Points.Add(pt);
    }
    public void SelectInkWithPolyline(IEnumerable<Point> Polyline)
    {
        UpdateInkSelectionRectangle(InkCanvas.InkPresenter.StrokeContainer.SelectWithPolyLine(Polyline));
    }

    public Rect SelectionBounds { get; private set; }
    public void UpdateInkSelectionRectangle(Rect Bounds)
    {
        SelectionBounds = Bounds;
        if (Bounds.Width is 0 && Bounds.Height is 0)
        {
            SelectionRectangle.Visibility = Visibility.Collapsed;
            return;
        }
        else
        {
            SelectionRectangle.Visibility = Visibility.Visible;
            SelectionColor.Color =
                SelectionRectangle.ActualTheme == ElementTheme.Dark ? UISettings.GetColorValue(UIColorType.AccentLight2)
                : UISettings.GetColorValue(UIColorType.AccentDark2);
            SelectionRenderTransform.TranslateX = Bounds.X; //-(Width - Bounds.Width) / 2 + Bounds.X;
            SelectionRenderTransform.TranslateY = Bounds.Y; //-(Height - Bounds.Height) / 2 + Bounds.Y;
            SelectionRectangle.Width = Bounds.Width;
            SelectionRectangle.Height = Bounds.Height;

        }
    }
    protected override bool RequestCut()
    {
        if (RequestCopy())
        {
            DeleteSelectedAndRecord();
            UpdateInkSelectionRectangle(default);
            return true;
        }
        return false;
    }
    protected override bool RequestCopy()
    {
        if (SelectionBounds.Width == 0) return false;
        if (InkCanvas is null) return false;
        InkCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
        return true;
    }
    public bool CanPasteInk => InkCanvas?.InkPresenter.StrokeContainer.CanPasteFromClipboard() ?? false;
    public override bool RequestPaste()
    {
        if (InkCanvas is null) return false;
        if (!CanPasteInk)
            return false;
        PasteInkAt(LatestMouse);
        return true;
    }
    public void PasteInkAt(Point pt)
    {
        BackgroundInkCanvas.InkPresenter.StrokeContainer.Clear();
        BackgroundInkCanvas.InkPresenter.StrokeContainer.PasteFromClipboard(pt);
        var strokes = InkRefTracker.GetRefs(BackgroundInkCanvas.InkPresenter.StrokeContainer.GetStrokes());
        InkCanvas.InkPresenter.StrokeContainer.AddStrokes((from x in strokes select x.CreateNew()).ToArray());
        RecordAddAction(strokes);
        BackgroundInkCanvas.InkPresenter.StrokeContainer.Clear();
    }
    public override bool RequestDuplicate()
    {
        if (SelectionBounds.Width == 0) return false;
        if (InkCanvas is null) return false;
        var ir = InkRefTracker.GetRefs(
            from x in InkCanvas.InkPresenter.StrokeContainer.GetStrokes()
            where x.Selected
            select x.Clone()
        );
        RecordAddAction(ir);
        InkCanvas.InkPresenter.StrokeContainer.AddStrokes(
            ir.Select(x => x.InkStroke)
        );
        return true;
    }
    public override bool RequestDelete()
    {
        if (SelectionBounds.Width == 0) return false;
        if (InkCanvas is null) return false;
        DeleteSelectedAndRecord();
        UpdateInkSelectionRectangle(default);
        return true;
    }
}