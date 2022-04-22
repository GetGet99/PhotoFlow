using Newtonsoft.Json.Linq;
using System;
using OpenCvSharp;
using CvMat = OpenCvSharp.Mat;
namespace PhotoFlow.Features.Mat
{
    public class Invert : MatBasedFeature<CvMat>, IFeatureUndoRedoable
    {
        public override string FeatureName => $"{base.FeatureName}.Invert";

        public override bool IsAvaliableForward(IFeatureDataTypes<CvMat> Input) => Input.Value != null;
        
        public override void LoadFromJSON(JObject obj) => DoNothing();

        public override JObject SaveJSON()
            => new JObject(
                new JProperty("Feature", FeatureName),
                new JProperty("Parameters", Array.Empty<string>())
            );
        CvMat Mat { get; set; }
        public override IFeatureDataTypes<CvMat> ForwardApply(IFeatureDataTypes<CvMat> Input)
        {
            if (!IsAvaliableForward(Input)) throw new ArgumentException($"{nameof(IsAvaliableForward)}({nameof(Input)}) == {false}");
            Mat = Input;
            return Input.Value.Invert();
        }
        public void Undo()
        {
            return Mat.Invert();
        }
    }
}
