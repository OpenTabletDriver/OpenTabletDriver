using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcData
    {
        [JsonConstructor]
        protected RpcData()
        {
        }

        public RpcData(object data)
        {
            SetData(data);
        }

        /// <summary>
        /// The full path for the type that in which <see cref="Data"/> refers to.
        /// </summary>
        public string Path { set; get; }

        /// <summary>
        /// The data in which to store.
        /// </summary>
        public JToken Data { set; get; }

        public void SetData(object data)
        {
            Path = data.GetType().FullName;
            Data = JToken.FromObject(data);
        }

        public T GetData<T>() where T : class
        {
            return Data == null ? default(T) : Data.Type != JTokenType.Null ? Data.ToObject<T>() : default(T);
        }

        public object GetData(PluginManager pluginManager)
        {
            var type = pluginManager.PluginTypes.First(t => t.FullName == Path);
            return Data.ToObject(type);
        }
    }
}
