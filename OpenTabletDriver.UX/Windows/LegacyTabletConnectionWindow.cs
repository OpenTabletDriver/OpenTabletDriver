using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Windows
{
    public class LegacyTabletConnectionWindow : DesktopForm
    {

        public LegacyTabletConnectionWindow()
        {
            Title = "Connect legacy tablet...";


        }
    }
}

