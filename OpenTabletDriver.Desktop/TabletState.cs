using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop
{
    public class TabletProperty<T>
    {
        [JsonConstructor]
        public TabletProperty()
        {
        }

        public TabletProperty(int id, T value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; set; }
        public T? Value { get; set; }
    }
}
