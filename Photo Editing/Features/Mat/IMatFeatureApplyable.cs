using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditing.Features.Mat
{
    public interface IMatFeatureApplyable<T> : IFeatureApplyable
    {
        void ApplyFeature(MatBasedFeature<T> faeture);
    }
}
