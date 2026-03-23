namespace OpenTabletDriver.Daemon.Contracts
{
    public class TabletProperty<T>
    {
        public TabletProperty(int id, T value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; set; }
        public T? Value { get; set; }
    }
}
