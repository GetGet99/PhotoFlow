#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PhotoFlow;

public class History : INotifyPropertyChanged
{
    const int UndoStackInit = 100;
    public bool CanUndo => UndoableActions.Count > 0;
    public bool CanRedo => RedoableActions.Count > 0;
    public event Action? Undoing, Redoing, UndoCompleted, RedoCompleted;
    public IHistoryAction? NextUndo
    {
        get
        {
            if (UndoableActions.TryPeek(out var v))
                return v;
            return null;
        }
    }
    public IHistoryAction? NextRedo
    {
        get
        {
            if (RedoableActions.TryPeek(out var v))
                return v;
            return null;
        }
    }
    readonly Stack<IHistoryAction> UndoableActions = new(UndoStackInit);
    readonly Stack<IHistoryAction> RedoableActions = new(UndoStackInit);

    public event PropertyChangedEventHandler? PropertyChanged;

    public void NewAction(IHistoryAction Action)
    {
        UndoableActions.Push(Action);
        ClearHistoryStack(RedoableActions);
        PropertyChanged?.Invoke(this, UndoEv);
        PropertyChanged?.Invoke(this, RedoEv);
    }
    static readonly PropertyChangedEventArgs UndoEv = new(nameof(CanUndo));
    static readonly PropertyChangedEventArgs RedoEv = new(nameof(CanRedo));
    public void Undo()
    {
        if (UndoableActions.TryPop(out var action))
        {
            Undoing?.Invoke();
            action.Undo();
            RedoableActions.Push(action);
            PropertyChanged?.Invoke(this, UndoEv);
            PropertyChanged?.Invoke(this, RedoEv);
            UndoCompleted?.Invoke();
        }
    }
    public void Redo()
    {
        if (RedoableActions.TryPop(out var action))
        {
            Redoing?.Invoke();
            action.Redo();
            UndoableActions.Push(action);
            PropertyChanged?.Invoke(this, UndoEv);
            PropertyChanged?.Invoke(this, RedoEv);
            RedoCompleted?.Invoke();
        }
    }
    public void ClearHistory()
    {
        ClearHistoryStack(UndoableActions);
        ClearHistoryStack(RedoableActions);
        PropertyChanged?.Invoke(this, UndoEv);
        PropertyChanged?.Invoke(this, RedoEv);
    }
    static void ClearHistoryStack(Stack<IHistoryAction> Stack)
    {
        while (Stack.TryPop(out var hisact))
            hisact.DisposeParam();
    }
}
public interface IHistoryAction
{
    object? Tag { get; }
    void Undo();
    void Redo();
    void DisposeParam();
}
public record class HistoryAction(Action Undo, Action Redo, object? Tag = null) : IHistoryAction
{
    void IHistoryAction.Redo() => Redo();
    void IHistoryAction.Undo() => Undo();
    void IHistoryAction.DisposeParam() { }
}
public record class HistoryAction<T>(T Param, Action<T> Undo, Action<T> Redo, Action<T>? DisposeParam = null, object? Tag = null) : IHistoryAction
{
    Action<T> DisposeParam { get; init; } = DisposeParam ?? delegate { };
    void IHistoryAction.Redo() => Redo(Param);
    void IHistoryAction.Undo() => Undo(Param);
    void IHistoryAction.DisposeParam() => DisposeParam(Param);
}
public record class HistoryActionMutable<T>(T Param, Action<T> Undo, Action<T> Redo, Action<T>? DisposeParam = null, object? Tag = null) : IHistoryAction
{
    public T Param { get; set; } = Param;
    Action<T> DisposeParam { get; init; } = DisposeParam ?? delegate { };
    void IHistoryAction.Redo() => Redo(Param);
    void IHistoryAction.Undo() => Undo(Param);
    void IHistoryAction.DisposeParam() => DisposeParam(Param);
}
public record class HistoryAdaptiveAction<T>(T InitialParam, Func<T, T> Undo, Func<T, T> Redo, Action<T>? DisposeParam = null, object? Tag = null) : IHistoryAction
{
    Action<T> DisposeParam { get; init; } = DisposeParam ?? delegate { };
    T AdaptiveParam { get; set; } = InitialParam;
    void IHistoryAction.Redo() => AdaptiveParam = Redo(AdaptiveParam);
    void IHistoryAction.Undo() => AdaptiveParam = Undo(AdaptiveParam);
    void IHistoryAction.DisposeParam() => DisposeParam(AdaptiveParam);
}