#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PhotoFlow;

public sealed partial class LayerContainer : Panel
{
    public void SetScrollViewer(ScrollViewer ScrollViewer)
    {
        this.ScrollViewer = ScrollViewer;
    }
    ScrollViewer? ScrollViewer;
    public float? ZoomFactor => ScrollViewer?.ZoomFactor;
    public List<Features.IFeatureUndoRedoable> History { get; } = new List<Features.IFeatureUndoRedoable>();
    public ObservableCollection<Layer.Layer> Layers { get; } = new ObservableCollection<Layer.Layer>();
    public delegate void SelectionUpdateHandler(int OldIndex, int NewINdex);
    public event SelectionUpdateHandler? SelectionUpdate;
    public VariableUpdateAlert<int> SelectionIndex = new(-1);
    public Layer.Layer? Selection => SelectionIndex == -1 ? null : Layers[SelectionIndex];
    double _PaddingPixel = 0;
    public double PaddingPixel
    {
        get => _PaddingPixel; set
        {
            _PaddingPixel = value;
            var imgSize = ImageSize;
            var padding = value * 2;
            Width = imgSize.Width + padding;
            Height = imgSize.Height + padding;
            InvalidateArrange();
        }
    }
    Size _ImageSize;
    public event Action? SizeUpdate;
    public new double Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            SizeUpdate?.Invoke();
        }
    }
    public new double Height
    {
        get => base.Height;
        set
        {
            base.Height = value;
            SizeUpdate?.Invoke();
        }
    }
    public Size ImageSize
    {
        get => _ImageSize;
        set
        {
            _ImageSize = value;
            var padding = PaddingPixel * 2;
            Width = value.Width + padding;
            Height = value.Height + padding;
        }
    }
    public LayerContainer()
    {
        InitializeComponent();
        SelectionIndex.Update += (oldIndex, newIndex) =>
        {
            var LayerCount = Layers.Count;
            foreach (var (i, Layer) in Layers.Enumerate())
            {
                Layer.Selecting.Value = i == newIndex;
            }
            SelectionUpdate?.Invoke(oldIndex, newIndex);
        };
        Layers.CollectionChanged += (o, e) =>
        {
            var CurrentSelectionIndex = SelectionIndex.Value;
            var newIndex = e.NewStartingIndex;
            var oldIndex = e.OldStartingIndex;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var layer = Layers[newIndex];
                    Extension.RunOnUIThread(() => Children.Insert(newIndex, layer.LayerUIElement));
                    layer.UpdateThisIndex(newIndex);
                    if (CurrentSelectionIndex >= newIndex) SelectionIndex.Value = CurrentSelectionIndex + 1;
                    if (SelectionIndex == -1) SelectionIndex.Value = newIndex;
                    goto UpdateAllIndexes;
                case NotifyCollectionChangedAction.Remove:
                    Extension.RunOnUIThread(() => Children.RemoveAt(oldIndex));
                    if (e.OldItems.Count > 0) e.OldItems[0].Cast<Layer.Layer>().UpdateThisIndex(-1);
                    if (CurrentSelectionIndex >= oldIndex) SelectionIndex.Value = CurrentSelectionIndex - 1;
                    goto UpdateAllIndexes;
                case NotifyCollectionChangedAction.Move:
                    Extension.RunOnUIThread(() => Children.Move((uint)oldIndex, (uint)newIndex));
                    Layers[oldIndex].UpdateThisIndex(oldIndex);
                    Layers[newIndex].UpdateThisIndex(newIndex);
                    if (CurrentSelectionIndex == oldIndex) SelectionIndex.Value = newIndex;
                    goto UpdateAllIndexes;
                case NotifyCollectionChangedAction.Replace:
                    Layers[oldIndex].UpdateThisIndex(oldIndex);
                    Extension.RunOnUIThread(() => Children[oldIndex] = Layers[oldIndex].LayerUIElement);
                    if (e.OldItems.Count > 0) e.OldItems[0].Cast<Layer.Layer>().UpdateThisIndex(-1);
                    SelectionIndex.InvokeUpdate();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Extension.RunOnUIThread(() => Children.Clear());
                    SelectionIndex.Value = -1;
                    break;
            }
            goto End;
        UpdateAllIndexes:
            foreach (var (i, layer) in Layers.Enumerate())
                layer.UpdateThisIndex(i);
            goto End;
        End:
            return;
        };
    }
    public void AddNewLayer(Layer.Layer Layer)
    {
        var Index = SelectionIndex + 1;
        Layers.Insert(Index, Layer);
        SelectionIndex.Value = Index;
    }
    public async Task<JObject> Save()
    {
        var LayerJson = await Layers.ForEachParallel(x => x.SaveData());
        return new JObject(
            new JProperty("Type", "LayerContainer"),
            new JProperty("Width", Width),
            new JProperty("Height", Height),
            new JProperty("Layers", LayerJson)
            );
    }
    public static Layer.Layer? LoadLayer(JObject JSON)
    {
        var layertype = JSON["LayerType"]?.ToObject<Layer.Types>();
        return layertype switch
        {
            Layer.Types.Background => null, // Deprecated Layer
            Layer.Types.Inking => new Layer.InkingLayer(JSON),
            Layer.Types.Mat => new Layer.MatLayer(JSON),
            Layer.Types.Text => new Layer.TextLayer(JSON),
            Layer.Types.RectangleShape => new Layer.RectangleLayer(JSON),
            Layer.Types.EllipseShape => new Layer.EllipseLayer(JSON),
            _ => throw new NotImplementedException(),
        };
    }
    public async Task LoadAndReplace(JObject json)
    {
        Width = json["Width"]?.ToObject<double>() ?? throw new FormatException("Error Reading File: Canvas Width does not exist");
        Height = json["Height"]?.ToObject<double>() ?? throw new FormatException("Error Reading File: Canvas Height does not exist");
        Layers.Clear();

        var layers = json["Layers"]?.ToObject<JObject[]>().ForEachParallel(LoadLayer);
        if (layers != null)
            foreach (var Layer in await layers)
                if (Layer != null) Layers.Add(Layer);
    }
    public void Clear()
    {
        Layers.Clear();
    }
    protected override Size MeasureOverride(Size availableSize) => new(Width, Height);
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (UIElement UIELE in Children)
        {
            var Layer = (Grid)UIELE;
            Layer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Layer.Arrange(new Rect(0, 0, Layer.DesiredSize.Width, Layer.DesiredSize.Height));
        }

        return finalSize;
    }
}
