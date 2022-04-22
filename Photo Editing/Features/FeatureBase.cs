using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System;
namespace PhotoFlow.Features
{
    public interface IFeature
    {
        string FeatureName { get; }
        void LoadFromJSON(JObject obj);
        JObject SaveJSON();
    }
    public interface IFeatureUndoRedoable : IFeature
    {
        void Redo();
        void Undo();
    }
    public interface IFeatureUndoRedoable<T> : IFeatureUndoRedoable
    {
        event Action A;
    }
    public abstract class FeatureBase<TInputType,TOutputType> : IFeature
    {
        public virtual string FeatureName => "Features";
        public abstract void LoadFromJSON(JObject obj);
        public abstract JObject SaveJSON();
        protected void DoNothing() { }
        public abstract bool IsAvaliableForward(IFeatureDataTypes<TInputType> Input);
        public abstract IFeatureDataTypes<TOutputType> ForwardApply(IFeatureDataTypes<TInputType> Input);
    }
}
