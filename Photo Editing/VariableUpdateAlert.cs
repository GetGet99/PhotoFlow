#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoFlow
{
    public class VariableUpdateAlert<T>
    {
        public delegate void UpdateEventHandler(T OldValue, T NewValue);
        public event UpdateEventHandler? Update;
        public VariableUpdateAlert(T startingValue) => PrivateValue = startingValue;
        private T PrivateValue;
        public T Value { get => PrivateValue; set {
                var oldValue = PrivateValue;
                PrivateValue = value;
                InvokeUpdate(oldValue, value);
            }
        }
        private void InvokeUpdate(T oldValue, T newValue) => Update?.Invoke(oldValue, newValue);
        public void InvokeUpdate() => InvokeUpdate(PrivateValue, PrivateValue);
        public T GetValue() => PrivateValue;
        public static implicit operator T(VariableUpdateAlert<T> variableUpdateEvent) => variableUpdateEvent.Value;
    }
}
