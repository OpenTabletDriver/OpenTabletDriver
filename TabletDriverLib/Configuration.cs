using System.Collections.Generic;
using TabletDriverLib.Class;

namespace TabletDriverLib
{
    public class Configuration
    {
        public Configuration()
        {
        }

        public List<Area> ScreenAreas { set; get; } = new List<Area>();
        public List<Area> TabletAreas { set; get; } = new List<Area>();
    }
}