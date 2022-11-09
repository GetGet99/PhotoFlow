#nullable enable
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using System.IO;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Input;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Windows.Foundation;
using CSharpUI;

namespace PhotoFlow.Layers;

public partial class InkingLayer : Layer
{
    Grid _UIElementDirect;
    public override UIElement UIElementDirect => _UIElementDirect;
    readonly InkRefTracker InkRefTracker = new();
    public override Types LayerType { get; } = Types.Inking;
    public readonly VariableUpdateAlert<bool> TouchAllowed = new(false);
    public readonly VariableUpdateAlert<bool> DrawingAllowed = new(false);
    static readonly SolidColorBrush SelectionColor = BorderColor;
    Canvas Canvas;
    Polygon SelectionPolygon;
    Rectangle SelectionRectangle;
    CompositeTransform SelectionRenderTransform;
    public InkCanvas InkCanvas { get; private set; }
    InkCanvas BackgroundInkCanvas { get; set; }
    public InkingLayer(Rect Where)
    {
        OnCreate();
        CompleteCreate();
        X = Where.X;
        Y = Where.Y;
        Width = Where.Width;
        Height = Where.Height;
    }
    public InkingLayer(JObject json, bool Runtime = false)
    {
        OnCreate();
        LoadData(json, Runtime);
        CompleteCreate();
    }
    Point LatestMouse = new();
    [MemberNotNull(nameof(InkCanvas), nameof(BackgroundInkCanvas), nameof(_UIElementDirect), nameof(Canvas), nameof(SelectionPolygon), nameof(SelectionRectangle))]
    protected override void OnCreate()
    {
        TouchAllowed.Update += (oldValue, newValue) => UpdateInkingDeviceOnUIThread();
        DrawingAllowed.Update += (oldValue, newValue) => UpdateInkingDeviceOnUIThread();
        Extension.RunOnUIThread(() =>
        {
            BackgroundInkCanvas = new();
            InkCanvas = new()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                CanBeScrollAnchor = false
            };
            Canvas = new()
            {

            };
            SelectionPolygon = new()
            {
                Stroke = SelectionColor,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 5, 2 },
                IsHitTestVisible = false
            };
            SelectionRectangle = new()
            {
                Visibility = Visibility.Collapsed,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 5, 2 },
                Fill = new SolidColorBrush(Colors.Transparent),
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                IsHitTestVisible = true
            };
            SelectionRenderTransform = new();
            LayerUIElement.Children.Add(_UIElementDirect = new Grid
            {
                Children =
                {
                    InkCanvas,
                    Canvas
                }
            });
            Canvas.Children.Add(SelectionPolygon);
            Canvas.Children.Add(SelectionRectangle);
            //LayerUIElement.Children.Add();

            InkCanvas.InkPresenter.StrokesCollected += (o, e) =>
            {
                ClearInkSelection();
                UpdatePreview();
                RecordAddAction(InkRefTracker.GetRefs(e.Strokes));
            };
            InkCanvas.InkPresenter.StrokesErased += (o, e) =>
            {
                ClearInkSelection();
                UpdatePreview();
                var strokes = e.Strokes;
                RecordDeleteAction(InkRefTracker.GetRefs(e.Strokes));
            };
            InkCanvas.PointerMoved += (_, e) => LatestMouse = e.GetCurrentPoint(InkCanvas).Position;
            OnCreateSelection();
        });
#pragma warning disable CS8774
    }
#pragma warning restore CS8774
    protected override void Deselect()
    {
        ClearInkSelection();
        UpdateInkingDeviceOnUIThread();
        base.Deselect();
    }
    protected override void Select()
    {
        UpdateInkingDeviceOnUIThread();
        base.Select();
    }
    public void ClearInkSelection()
    {
        foreach (var s in InkCanvas.InkPresenter.StrokeContainer.GetStrokes())
            s.Selected = false;
        UpdateInkSelectionRectangle(default);
    }
    public override void DisablePreviewEffects()
    {
        ClearInkSelection();
        base.DisablePreviewEffects();
    }
    void UpdateInkingDeviceOnUIThread() => Extension.RunOnUIThread(() => UpdateInkingDevice());
    [RunOnUIThread]
    void UpdateInkingDevice()
    {
        CoreInputDeviceTypes InputTypes;
        if (DrawingAllowed && Selecting)
        {
            InputTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;
            if (TouchAllowed) InputTypes |= CoreInputDeviceTypes.Touch;
        }
        else
        {
            InputTypes = CoreInputDeviceTypes.None;
        }
        InkCanvas!.InkPresenter.InputDeviceTypes = InputTypes;
    }
    public override void Dispose()
    {
        //InkRefTracker.Dispose();
        Extension.RunOnUIThread(() => InkCanvas!.InkPresenter.StrokeContainer.Clear());
    }
    protected override JObject OnDataSaving(bool Runtime)
    {
        async Task<JObject> func()
        {
            var ms = new InMemoryRandomAccessStream();
            await Extension.RunOnUIThreadAsync(async () =>
                await InkCanvas!.InkPresenter.StrokeContainer.SaveAsync(ms, InkPersistenceFormat.Isf)
            );

            byte[] bytes = new byte[ms.Size];
            var readstream = ms.AsStreamForRead();
            readstream.Position = 0;
            await readstream.ReadAsync(bytes, 0, bytes.Length);
            return new JObject(new JProperty("Ink", bytes));
        }
        Task<JObject> t = func();
        t.Wait();
        return t.Result;
    }
    protected override void OnDataLoading(JObject json, Task _)
    {
        var inkbytes = json["Ink"]?.ToObject<byte[]>();
        if (inkbytes != null)
        {
            using var ms = new MemoryStream(inkbytes);
            Extension.RunOnUIThread(async () => await InkCanvas.InkPresenter.StrokeContainer.LoadAsync(ms.AsRandomAccessStream()));
        }
    }


    public void DeleteSelectedAndRecord()
    {
        RecordDeleteAction(
            (
                from x in InkCanvas.InkPresenter.StrokeContainer.GetStrokes()
                where x.Selected
                select InkRefTracker.GetRef(x).Edit(x => x.CreateNew())
            ).ToArray());
        InkCanvas?.InkPresenter.StrokeContainer.DeleteSelected();
    }
    void RecordDeleteAction(InkRef[] AddedInks)
    {
        if (LayerContainer is null) return;
        NewHistoryAction(LayerContainer.History, new HistoryAction<(LayerContainer LayerContainer, uint LayerId, InkRef[] Inks)>(
            (LayerContainer, LayerId, AddedInks),
            Tag: this,
            Undo: x =>
            {
                var (LayerContainer, LayerId, strokes) = x;
                if (LayerContainer.GetLayerFromId(LayerId) is not InkingLayer Layer) return;
                if (Layer.SelectionIndexUpdateTarget is not null)
                    Layer.SelectionIndexUpdateTarget.Value = Layer.Index;
                Layer.ClearInkSelection();
                Layer.InkCanvas.InkPresenter.StrokeContainer.AddStrokes((from ink in strokes select ink.CreateNew()).ToArray());
                Layer.UpdatePreview();
            },
            Redo: x =>
            {
                var (LayerContainer, LayerId, strokes) = x;
                if (LayerContainer.GetLayerFromId(LayerId) is not InkingLayer Layer) return;
                if (Layer.SelectionIndexUpdateTarget is not null)
                    Layer.SelectionIndexUpdateTarget.Value = Layer.Index;
                Layer.ClearInkSelection();
                foreach (var ink in strokes)
                {
                    ink.InkStroke.Selected = true;
                    ink.CreateNew();
                }
                Layer.InkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                Layer.UpdatePreview();
            },
            DisposeParam: x => { foreach (var stroke in x.Inks) stroke.MarkUnused(); }
        ));
    }
    void RecordAddAction(InkRef[] AddedInks)
    {
        if (LayerContainer is null) return;
        NewHistoryAction(LayerContainer.History, new HistoryAction<(LayerContainer LayerContainer, uint LayerId, InkRef[] Inks)>(
            (LayerContainer, LayerId, AddedInks),
            Tag: this,
            Undo: x =>
            {
                var (LayerContainer, LayerId, strokes) = x;
                if (LayerContainer.GetLayerFromId(LayerId) is not InkingLayer Layer) return;
                if (Layer.SelectionIndexUpdateTarget is not null)
                    Layer.SelectionIndexUpdateTarget.Value = Layer.Index;
                Layer.ClearInkSelection();
                foreach (var ink in strokes)
                {
                    ink.InkStroke.Selected = true;
                    ink.CreateNew();
                }
                Layer.InkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                Layer.UpdatePreview();
            },
            Redo: x =>
            {
                var (LayerContainer, LayerId, strokes) = x;
                if (LayerContainer.GetLayerFromId(LayerId) is not InkingLayer Layer) return;
                if (Layer.SelectionIndexUpdateTarget is not null)
                    Layer.SelectionIndexUpdateTarget.Value = Layer.Index;
                Layer.ClearInkSelection();
                Layer.InkCanvas.InkPresenter.StrokeContainer.AddStrokes((from ink in strokes select ink.CreateNew()).ToArray());
                Layer.UpdatePreview();
            },
            DisposeParam: x => { foreach (var stroke in x.Inks) stroke.MarkUnused(); }
        ));
    }
}
