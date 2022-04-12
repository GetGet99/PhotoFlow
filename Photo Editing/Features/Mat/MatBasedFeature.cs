using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditing.Features.Mat
{
    public abstract class MatBasedFeature<TOutputType> : FeatureBase<OpenCvSharp.Mat, TOutputType>
    {
        public override string FeatureName => $"{base.FeatureName}.Mat";
    }
    public abstract class MatBasedFeature<TInputType, TOutputType> : FeatureBase<(OpenCvSharp.Mat Image, TInputType OtherInput), TOutputType>
    {
        public override string FeatureName => $"{base.FeatureName}.Mat";
    }
}
