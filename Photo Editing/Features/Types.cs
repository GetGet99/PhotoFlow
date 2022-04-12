using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditing.Features
{
    public struct IFeatureDataTypes<T>
    {
        public T Value { get; set; }
        public static implicit operator IFeatureDataTypes<T>(T input) => new IFeatureDataTypes<T> { Value = input };
        public static implicit operator T(IFeatureDataTypes<T> input) => input.Value;
        public Newtonsoft.Json.Linq.JObject ToJSON()
        {
            throw new NotImplementedException();
        }
    }
    public interface ICanBecomeFeatureDataType<T>
    {
        IFeatureDataTypes<T> GetFeatureDataType { get; }
    }
}
