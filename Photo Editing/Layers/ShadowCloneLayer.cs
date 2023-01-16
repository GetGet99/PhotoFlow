#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Imaging;

namespace PhotoFlow.Layers;

class ShadowCloneLayer : Layer
{
    public override UIElement UIElementDirect => Grid;
    uint? TargetLayerId;
    Layer? Layer;
    Grid Grid;
    public ShadowCloneLayer(Layer Layer)
    {
        this.Layer = Layer;
        Transform = Layer.Transform;
        OnCreate();
        FinalizeLoad();
    }
    public ShadowCloneLayer(JObject json, bool Runtime = false)
    {
        LoadData(json, Runtime: Runtime);
        OnCreate();
        FinalizeLoad();
    }
    public override Types LayerType => Types.ShadowClone;

    [MemberNotNull(nameof(Grid))]
    protected override void OnCreate()
    {
        Extension.RunOnUIThread(delegate
        {
            LayerUIElement.Children.Add(Grid = new() { IsHitTestVisible = false });
        });
    }

    public override void FinalizeLoad()
    {
        if (Layer is null && TargetLayerId is not null && LayerContainer is not null)
            Layer = LayerContainer.GetLayerFromId(LayerId);
        if (Layer == this)
        {
            Debugger.Break();
            return;
        }
        if (Layer is null) return;
        void UpdateSize()
        {
            Width = Layer.Width;
            Height = Layer.Height;
        }
        Layer.SizeChanged += UpdateSize;
        UpdateSize();
        var visual = ElementCompositionPreview.GetElementVisual(Layer.UIElementDirect);
        var cloned = visual.Compositor.CreateRedirectVisual(visual);
        cloned.IsHitTestVisible = false;
        ElementCompositionPreview.SetElementChildVisual(Grid, cloned);
    }
    //protected override Layer RequestDuplicateLayer()
    //    => Layer is null ? base.RequestDuplicateLayer() : new ShadowCloneLayer(Layer);

    public override void Dispose()
    {
        
    }

    protected override void OnDataLoading(JObject storage, Task _)
    {
        TargetLayerId = storage["LayerId"]?.ToObject<uint>();
    }

    protected override JObject OnDataSaving(bool Runtime)
    {
        if (Layer is null) return new JObject();
        return new JObject(
            new JProperty("LayerId", Runtime ? Layer.LayerId : Layer.Index)
        );
    }
}


class ShadowCloneLayerOld : Layer
{
    public override UIElement UIElementDirect => Image;
    uint? TargetLayerId;
    Layer? Layer;
    Image Image;
    public ShadowCloneLayerOld(Layer Layer)
    {
        this.Layer = Layer;
        Transform = Layer.Transform;
        OnCreate();
        FinalizeLoad();
    }
    public ShadowCloneLayerOld(JObject json, bool Runtime = false)
    {
        LoadData(json, Runtime: Runtime);
        OnCreate();
    }
    public override Types LayerType => Types.ShadowClone;

    [MemberNotNull(nameof(Image))]
    protected override void OnCreate()
    {
        Extension.RunOnUIThread(delegate
        {
            LayerUIElement.Children.Add(Image = new Image());
        });
    }

    public override async void FinalizeLoad()
    {
        if (Layer is null && TargetLayerId is not null && LayerContainer is not null)
            Layer = LayerContainer.GetLayerFromId(LayerId);
        if (Layer == this)
        {
            Debugger.Break();
            return;
        }
        if (Layer is null) return;
        void UpdateSize()
        {
            Width = Layer.Width;
            Height = Layer.Height;
        }
        Layer.SizeChanged += UpdateSize;
        UpdateSize();
        Layer.OnLayerPreviewUpdate += LayerPreviewUpdateEventHandler;
        await Extension.RunOnUIThreadAsync(async delegate
        {
            Image.Source = await Layer.UIElementDirect.ToRenderTargetBitmapAsync();
            await UpdatePreviewAsync();
        });
    }
    //protected override Layer RequestDuplicateLayer()
    //    => Layer is null ? base.RequestDuplicateLayer() : new ShadowCloneLayer(Layer);

    public override void Dispose()
    {
        if (Layer is null) return;
        Layer.OnLayerPreviewUpdate -= LayerPreviewUpdateEventHandler;
    }
    async void LayerPreviewUpdateEventHandler()
    {
        if (Layer is null) return;
        Image.Source = await Layer.UIElementDirect.ToRenderTargetBitmapAsync();
        await UpdatePreviewAsync();
    }

    protected override void OnDataLoading(JObject storage, Task _)
    {
        TargetLayerId = storage["LayerId"]?.ToObject<uint>();
    }

    protected override JObject OnDataSaving(bool Runtime)
    {
        if (Layer is null) return new JObject();
        return new JObject(
            new JProperty("LayerId", Runtime ? Layer.LayerId : Layer.Index)
        );
    }
}
