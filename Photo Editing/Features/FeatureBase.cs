using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace PhotoFlow.Features
{
    public abstract class FeatureBase<TInputType,TOutputType>
    {
        public virtual string FeatureName => "Features";
        public abstract bool HasDialog { get; }
        public abstract ThemeContentDialog CallDialog { get; }
        public abstract void LoadFromJSON(JObject obj);
        public abstract JObject SaveJSON();
        public Windows.Foundation.IAsyncOperation<Windows.UI.Xaml.Controls.ContentDialogResult> OpenFeatureEditDialogAsync()
            => HasDialog ? CallDialog.ShowAsync() : null;
        protected void DoNothing() { }
        public abstract bool IsAvaliable(IFeatureDataTypes<TInputType> Input);
        public abstract IFeatureDataTypes<TOutputType> Apply(IFeatureDataTypes<TInputType> Input);
    }
}
