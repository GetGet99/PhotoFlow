#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using Windows.Foundation;
namespace PhotoFlow;
public sealed class InkRefTracker : IDisposable
{
    private readonly List<InkRef> InkRefCollection = new();
    public InkRef GetRef(InkStroke InkStroke)
    {
        var inkref = InkRefCollection.FirstOrDefault(inkref => inkref.InkStroke == InkStroke);
        if (inkref is null)
        {
            inkref = new InkRef(InkStroke, this);
            InkRefCollection.Add(inkref);
        }
        else
        {
            inkref.AddUseageCount();
        }
        return inkref;
    }
    public InkRef[] GetRefs(IEnumerable<InkStroke> InkStrokes)
    {
        return (from x in InkStrokes select GetRef(x)).ToArray();
    }
    public void MarkUnused(InkRef inkRef)
    {
        inkRef.RemoveUseageCount();
        if (inkRef.UsageCount == 0) InkRefCollection.Remove(inkRef);
    }
    public void Dispose()
    {
        foreach (var a in InkRefCollection) a.Dispose();
    }
}
public sealed class InkRef : IDisposable
{
    readonly static InkStroke dummyInkStroke = new InkStrokeBuilder().CreateStroke(new Point[] { new Point() });
    readonly InkRefTracker Owner;
    public InkStroke InkStroke { get; private set; }
    public int UsageCount { get; private set; } = 1;
    public void AddUseageCount() => UsageCount++;
    public void RemoveUseageCount() => UsageCount++;
    public InkRef(InkStroke InkStroke, InkRefTracker owner)
    {
        this.InkStroke = InkStroke;
        Owner = owner;
    }
    public InkStroke CreateNew()
    {
        return InkStroke = InkStroke.Clone();
    }

    public void MarkUnused() => Owner.MarkUnused(this);
    public void Dispose()
    {
        InkStroke = dummyInkStroke;
    }
}
