using Newtonsoft.Json.Linq;
using CvMat = OpenCvSharp.Mat;

namespace PhotoFlow.Features.Mat;

public class Crop : MatBasedFeature<CvMat>
{
    IFeatureDataTypes<uint> X;
    IFeatureDataTypes<uint> Y;
    IFeatureDataTypes<uint> Width;
    IFeatureDataTypes<uint> Height;
    public override string FeatureName => $"{base.FeatureName}.Crop";

    public override bool IsAvaliableForward(IFeatureDataTypes<CvMat> Input) => Input.Value != null;

    public override JObject SaveJSON()
        => new (
            new JProperty("Feature", FeatureName),
            new JProperty("Parameters", new JObject(
                new JProperty("X", X),
                new JProperty("Y", Y),
                new JProperty("Width", Width),
                new JProperty("Height", Height)
            ))
        );
    public override void LoadFromJSON(JObject obj)
    {
        var token = obj["Parameters"];
        X = token["X"].ToObject<uint>();
        Y = token["Y"].ToObject<uint>();
        Width = token["Width"].ToObject<uint>();
        Height = token["Height"].ToObject<uint>();
    }
    public override IFeatureDataTypes<CvMat> ForwardApply(IFeatureDataTypes<CvMat> Input)
    {
        return Input.Value.SubMat((int)Y.Value, (int)Y.Value + (int)Height.Value, (int)X.Value, (int)X.Value + (int)Width.Value);
    }
}
