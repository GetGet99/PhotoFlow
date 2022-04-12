using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI.Core;

namespace PhotoEditing
{
    public partial class Extension
    {
        static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();
        public static byte[] SerializeToByte(this object o)
        {
            using (var ms = new MemoryStream())
            {
                BinaryFormatter.Serialize(ms, o);
                return ms.ToArray();
            }
        }
        public static byte[] SerializeToXML(this object o)
        {
            
            using (var ms = new MemoryStream())
            {
                XmlSerializer ser = new XmlSerializer(o.GetType());
                ser.Serialize(ms, o);
                return ms.ToArray();
            }
        }
        public static T DeserializeTo<T>(this byte[] bytes)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                return (T)BinaryFormatter.Deserialize(ms);
            }
        }
        public static T XMLDeserializeTo<T>(this byte[] bytes)
        {
            using (var ms = new MemoryStream())
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                ms.Write(bytes, 0, bytes.Length);
                return (T)ser.Deserialize(ms);
            }
        }
        public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> input, int start = 0)
        {
            int i = start;
            foreach (var t in input)
                yield return (i++, t);
        }
        public static async Task AwaitAllAsync(this Task[] tasks) => await Task.WhenAll(tasks);
        public static async Task<TOut[]> ForEachParallel<TIn, TOut>(this ICollection<TIn> input, Func<TIn,Task<TOut>> func)
        {
            var count = input.Count;
            TOut[] arr = new TOut[count];
            Task[] arrTask = new Task[count];
            foreach (var (i, item) in input.Enumerate())
            {
                arrTask[i] = Task.Factory.StartNew(async delegate
                {
                    arr[i] = await func(item);
                });
            }
            await arrTask.AwaitAllAsync();
            return arr;
        }
        public static async Task<TOut[]> ForEachParallel<TIn, TOut>(this ICollection<TIn> input, Func<TIn, TOut> func)
        {
            var count = input.Count;
            TOut[] arr = new TOut[count];
            Task[] arrTask = new Task[count];
            foreach (var (i, item) in input.Enumerate())
            {
                arrTask[i] = Task.Factory.StartNew(delegate
                {
                    arr[i] = func(item);
                });
            }
            await arrTask.AwaitAllAsync();
            return arr;
        }
        public static void RunOnUIThread(Action a)
        {
            if (Dispatcher.HasThreadAccess)
            {
                a();
                return;
            }
            var task = RunOnUIThreadAsync(a).AsAsyncAction().AsTask();
            task.Wait();
        }
        readonly static CoreDispatcher Dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
        public static async Task RunOnUIThreadAsync(Action a)
        {
            if (Dispatcher.HasThreadAccess) a();
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.High, new DispatchedHandler(a));
        }
        public static T Cast<T>(this object o) where T : class
        {
            return o as T;
        }
        public static TChild Cast<TParent,TChild>(this TParent o) where TChild : TParent
        {
            return (TChild)o;
        }
    }
}
