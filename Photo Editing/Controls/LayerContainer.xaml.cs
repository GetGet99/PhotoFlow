#nullable enable
using Newtonsoft.Json.Linq;
using PhotoFlow.Layer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PhotoFlow;

public sealed partial class LayerContainer : Panel
{
    public readonly History History = new();
    public void SetScrollViewer(ScrollViewer ScrollViewer)
    {
        this.ScrollViewer = ScrollViewer;
    }
    ScrollViewer? ScrollViewer;
    public float? ZoomFactor => ScrollViewer?.ZoomFactor;
    public ObservableCollection2<Layer.Layer> Layers { get; } = new();
    public delegate void SelectionUpdateHandler(int OldIndex, int NewINdex);
    public event SelectionUpdateHandler? SelectionUpdate;
    public VariableUpdateAlert<int> SelectionIndex = new(-1);
    public Layer.Layer? Selection => SelectionIndex == -1 ? null : Layers[SelectionIndex];
    double _PaddingPixel = 0;
    public Layer.Layer? GetLayerFromId(uint id) => Layers.FirstOrDefault(x => x.LayerId == id);
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
    int CurrentSelectionIndex => SelectionIndex.Value;
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
        Layers.Added += Layers_Added;
        Layers.Removed += Layers_Removed;
        Layers.Moved += Layers_Moved;
        Layers.Replaced += Layers_Replaced;
        Layers.Cleared += Layers_Cleared;
        Layers.Added += Layers_Added_History;
        Layers.Removed += Layers_Removed_History;
        Layers.Moved += Layers_Moved_History;
        Layers.Replaced += Layers_Replaced_History;
        Layers.Cleared += Layers_Cleared_History;
    }
    void UpdateAllIndex()
    {
        foreach (var (i, layer) in Layers.Enumerate())
            layer.UpdateThisIndex(i);
    }
    bool LayerChangingUndoing = false;
    private void Layers_Cleared_History(Layer.Layer[] Values)
    {
        if (LayerChangingUndoing) return;
        History.NewAction(new HistoryAction<JObject[]>(
            (from x in Values select x.SaveData(Runtime: true)).ToArray(),
            Tag: this,
            Undo: items =>
            {
                LayerChangingUndoing = true;
                Layers.AddRange(from x in items select LoadLayer(x, Runtime: true));
                LayerChangingUndoing = false;
            },
            Redo: x =>
            {
                LayerChangingUndoing = true;
                Layers.Clear();
                LayerChangingUndoing = false;
            }
        ));
    }

    private void Layers_Replaced_History(int Index, Layer.Layer OldLayer, Layer.Layer NewLayer)
    {
        if (LayerChangingUndoing) return;
        History.NewAction(new HistoryAction<(int Index, JObject oldObj, JObject newObj)>(
            (Index, OldLayer.SaveData(), NewLayer.SaveData()),
            Tag: this,
            Undo: x =>
            {
                LayerChangingUndoing = true;
                var (Index, oldObj, newObj) = x;
                Layers[Index] = LoadLayer(oldObj) ?? throw new ArgumentException();
                LayerChangingUndoing = false;
            },
            Redo: x =>
            {
                LayerChangingUndoing = true;
                var (Index, oldObj, newObj) = x;
                Layers[Index] = LoadLayer(newObj) ?? throw new ArgumentException();
                LayerChangingUndoing = false;
            }
        ));
    }

    private void Layers_Moved_History(int Index1, int Index2, Layer.Layer Item1, Layer.Layer Item2)
    {
        if (LayerChangingUndoing) return;
        History.NewAction(new HistoryAction<(int Index, int Index2)>(
            (Index1, Index2),
            Tag: this,
            Undo: x =>
            {
                LayerChangingUndoing = true;
                Layers.Move(x.Index, x.Index2); // order doesn't really matter
                LayerChangingUndoing = false;
            },
            Redo: x =>
            {
                LayerChangingUndoing = true;
                Layers.Move(x.Index, x.Index2); // order doesn't really matter
                LayerChangingUndoing = false;
            }
        ));
    }

    private void Layers_Removed_History(int Index, Layer.Layer Layer)
    {
        if (LayerChangingUndoing) return;
        //History.ClearHistory();
        History.NewAction(new HistoryAction<(int Index, JObject LayerData)>(
            (Index, Layer.SaveData(Runtime: true)),
            Tag: this,
            Undo: x =>
            {
                LayerChangingUndoing = true;
                var (Index, LayerData) = x;
                Layers.Insert(Index, LoadLayer(LayerData, Runtime: true) ?? throw new ArgumentException());
                LayerChangingUndoing = false;
            },
            Redo: x =>
            {
                LayerChangingUndoing = true;
                var (Index, LayerData) = x;
                Layers.RemoveAt(Index);
                LayerChangingUndoing = false;
            }
        ));
    }

    private void Layers_Added_History(int Index, Layer.Layer Layer)
    {
        if (LayerChangingUndoing) return;
        History.NewAction(new HistoryAction<(int Index, JObject LayerData)>(
            (Index, Layer.SaveData(Runtime: true)),
            Tag: this,
            Undo: x =>
            {
                LayerChangingUndoing = true;
                var (Index, LayerData) = x;
                Layers.RemoveAt(Index);
                LayerChangingUndoing = false;
            },
            Redo: x =>
            {
                LayerChangingUndoing = true;
                var (Index, LayerData) = x;
                Layers.Insert(Index, LoadLayer(LayerData, Runtime: true) ?? throw new ArgumentException());
                LayerChangingUndoing = false;
            }
        ));
    }

    private void Layers_Cleared(Layer.Layer[] Values)
    {
        Extension.RunOnUIThread(() => Children.Clear());
        SelectionIndex.Value = -1;
    }

    private void Layers_Replaced(int OldIndex, Layer.Layer OldItem, Layer.Layer NewItem)
    {
        Extension.RunOnUIThread(() => Children[OldIndex] = Layers[OldIndex].LayerUIElement);
        OldItem.UpdateThisIndex(-1);
        NewItem.UpdateThisIndex(OldIndex);
        SelectionIndex.InvokeUpdate();
    }

    private void Layers_Moved(int Index1, int Index2, Layer.Layer Item1, Layer.Layer Item2)
    {
        Extension.RunOnUIThread(() => Children.Move((uint)Index1, (uint)Index2));
        Item1.UpdateThisIndex(Index1);
        Item2.UpdateThisIndex(Index2);
        if (CurrentSelectionIndex == Index1) SelectionIndex.Value = Index2;
        UpdateAllIndex();
    }

    private void Layers_Added(int Index, Layer.Layer Layer)
    {
        Extension.RunOnUIThread(() => Children.Insert(Index, Layer.LayerUIElement));
        Layer.UpdateThisIndex(Index);
        if (CurrentSelectionIndex >= Index) SelectionIndex.Value = CurrentSelectionIndex + 1;
        if (SelectionIndex == -1) SelectionIndex.Value = Index;
        UpdateAllIndex();
    }
    private void Layers_Removed(int Index, Layer.Layer Item)
    {
        Extension.RunOnUIThread(() => Children.RemoveAt(Index));
        Item.UpdateThisIndex(-1);
        if (CurrentSelectionIndex >= Index) SelectionIndex.Value = CurrentSelectionIndex - 1;
        UpdateAllIndex();
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
        if (LayerJson == null) throw new NullReferenceException();
        return new JObject(
            new JProperty("Type", "LayerContainer"),
            new JProperty("Width", Width),
            new JProperty("Height", Height),
            new JProperty("Layers", LayerJson)
            );
    }
    public static Layer.Layer? LoadLayer(JObject JSON, bool Runtime = false)
    {
        var layertype = JSON["LayerType"]?.ToObject<Layer.Types>();
        return layertype switch
        {
            Layer.Types.Background => null, // Deprecated Layer
            Layer.Types.Inking => new Layer.InkingLayer(JSON, Runtime),
            Layer.Types.Mat => new Layer.MatLayer(JSON, Runtime),
            Layer.Types.Text => new Layer.TextLayer(JSON, Runtime),
            Layer.Types.RectangleShape => new Layer.RectangleLayer(JSON, Runtime),
            Layer.Types.EllipseShape => new Layer.EllipseLayer(JSON, Runtime),
            _ => throw new NotImplementedException(),
        };
    }
    public async Task LoadAndReplace(JObject json)
    {
        Width = json["Width"]?.ToObject<double>() ?? throw new FormatException("Error Reading File: Canvas Width does not exist");
        Height = json["Height"]?.ToObject<double>() ?? throw new FormatException("Error Reading File: Canvas Height does not exist");
        Layers.Clear();

        var TaskLayers = json["Layers"]?.ToObject<JObject[]>().ForEachParallel(x => LoadLayer(x));
        if (TaskLayers != null)
        {
            var layers = await TaskLayers;
            if (layers != null)
                foreach (var Layer in layers)
                    if (Layer != null) Layers.Add(Layer);
        }
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

public delegate void OC2AddEventHandler<T>(int Index, T Item);
public delegate void OC2RemoveEventHandler<T>(int Index, T Item);
public delegate void OC2ReplaceEventHandler<T>(int Index, T OldItem, T NewItem);
public delegate void OC2MoveEventHandler<T>(int Index1, int Index2, T Item1, T Item2);
public delegate void OC2ClearEventHandler<T>(T[] Values);

public class ObservableCollection2<T> : ICollection<T>
{
    public enum ChangeType
    {

    }

    public event OC2AddEventHandler<T>? Adding, Added;
    public event OC2RemoveEventHandler<T>? Removing, Removed;
    public event OC2ReplaceEventHandler<T>? Replacing, Replaced;
    public event OC2MoveEventHandler<T>? Moving, Moved;
    public event OC2ClearEventHandler<T>? Clearing, Cleared;

    readonly ObservableCollection<T> Values = new();
    public int Count => Values.Count;
    public bool IsReadOnly { get; } = false;

    public void Add(T item) => Insert(Count, item);
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items) Add(item);
    }
    public void Insert(int index, T item)
    {
        Adding?.Invoke(index, item);
        Values.Insert(index, item);
        Added?.Invoke(index, item);
    }

    public void Clear()
    {
        var arr = Values.ToArray();
        Clearing?.Invoke(arr);
        Values.Clear();
        Cleared?.Invoke(arr);
    }

    public bool Contains(T item) => Values.Contains(item);
    public int IndexOf(T item) => Values.IndexOf(item);

    public void CopyTo(T[] array, int arrayIndex) => Values.CopyTo(array, arrayIndex);

    public void Move(int Index1, int Index2)
    {
        var item1 = Values[Index1];
        var item2 = Values[Index2];
        Moving?.Invoke(Index1, Index2, item1, item2);
        Values.Move(Index1, Index2);
        Moved?.Invoke(Index1, Index2, item1, item2);

    }

    public IEnumerator<T> GetEnumerator() => Values.GetEnumerator();

    public bool Remove(T item)
    {
        var idx = IndexOf(item);
        if (idx == -1) return false;
        RemoveAt(idx);
        return true;
    }

    public void RemoveAt(int index)
    {
        var value = Values[index];
        Removing?.Invoke(index, value);
        Values.RemoveAt(index);
        Removed?.Invoke(index, value);
    }

    IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();

    public T this[int index]
    {
        get => Values[index];
        set
        {
            var oldItem = Values[index];
            Replacing?.Invoke(index, oldItem, value);
            Values[index] = value;
            Replaced?.Invoke(index, oldItem, value);
        }
    }
}