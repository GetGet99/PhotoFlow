//var rtb = new RenderTargetBitmap();
//await rtb.RenderAsync(ICV);
//var pixelBuffer = (await rtb.GetPixelsAsync());
//var mem = new System.IO.MemoryStream();
//pixelBuffer.AsStream().CopyTo(mem);
//Layers.Add(Cv2.ImDecode(mem.ToArray(), ImreadModes.Unchanged));
//var dc = new DataPackage();
//dc.SetData("PNG", mem.ToArray());

//Clipboard.SetContent(dc);
////CanvasDevice device = CanvasDevice.GetSharedDevice();
////CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)ICV.ActualWidth, (int)ICV.ActualHeight, 96,Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Straight);

////using (var ds = renderTarget.CreateDrawingSession())
////{
////    ds.Clear(Colors.White);
////    ds.DrawInk(ICV.InkPresenter.StrokeContainer.GetStrokes());
////}

////var bytes = renderTarget.GetPixelBytes();
////Layers.Add(Cv2.ImDecode(bytes, ImreadModes.Unchanged));



//MainScrollView.PointerPressed += (s, e) => PrintTouchEvent(e, "pointer pressed");
//MainScrollView.PointerMoved += (s, e) =>
//{


//    if (e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
//    {
//        // Re-enable the System mode so that the UI-thread bound touch input engine doesn't
//        // take precedence over DManip
//        MainScrollView.ManipulationMode |= ManipulationModes.System;

//        // Try to pass the pointer off to be processed by DManip
//        TryStartDirectManipulation(e.Pointer);
//    }
//};
//MainScrollView.PointerPressed += (s, e) => MainScrollView.ManipulationMode &= ~ManipulationModes.System;



//ImageDisplay.PointerMoved += (o, e) =>
//{
//    if (e.Pointer.IsInContact)
//    {
//        var pt = e.GetCurrentPoint(ImageDisplay).Position;
//    }
//};