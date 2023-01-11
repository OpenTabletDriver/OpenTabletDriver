using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop
{
    public class TabletState
    {
        [JsonConstructor]
        public TabletState()
        {
        }

        public TabletState(int id, InputDeviceState state)
        {
            Id = id;
            State = state;
        }

        public int Id { get; set; }
        public InputDeviceState State { get; set; }
    }
}
