using Newtonsoft.Json.Linq;
using System;
using OpenCvSharp;
using CvMat = OpenCvSharp.Mat;
namespace PhotoEditing.Features.Mat
{
    public class Invert : MatBasedFeature<CvMat>
    {
        public override string FeatureName => $"{base.FeatureName}.Invert";

        public override bool IsAvaliable(IFeatureDataTypes<CvMat> Input) => Input.Value != null;

        public override bool HasDialog { get; } = false;
        public override ThemeContentDialog CallDialog { get; } = null;

        public override void LoadFromJSON(JObject obj) => DoNothing();

        public override JObject SaveJSON()
            => new JObject(
                new JProperty("Feature", FeatureName),
                new JProperty("Parameters", Array.Empty<string>())
            );
        public override IFeatureDataTypes<CvMat> Apply(IFeatureDataTypes<CvMat> Input)
        {
            return Input.Value.Invert();
        }
    }
}
