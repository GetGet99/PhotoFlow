using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;
namespace PhotoFlow;

public static class MatExtension
{
    public static Mat AsType(this Mat m, MatType type, double alpha=1,double beta=0)
    {
        Mat m2 = new Mat();
        m.ConvertTo(m2, type, alpha: alpha, beta: beta);
        return m2;
    }
    public static Mat[] SeparateChannels(this Mat m)
    {
        int Channels = m.Channels();
        Mat[] mats = new Mat[Channels];
        for (int i = 0; i < Channels; i++)
        {
            mats[i] = m.ExtractChannel(i);
        }
        return mats;
    }
    public static Mat ToMat(this byte[] bytes) => Cv2.ImDecode(bytes, ImreadModes.Unchanged);
}
