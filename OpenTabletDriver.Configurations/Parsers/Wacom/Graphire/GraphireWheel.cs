namespace OpenTabletDriver.Configurations.Parsers.Wacom.Graphire
{
    internal static class GraphireWheel
    {
        /// <summary>
        /// Decodes the fingerwheel delta from the auxiliary byte of a report.
        /// <para/>
        /// Graphire4 devices (WACOM_G4) report the wheel as a signed 3-bit value in bits 5:3,
        /// ranging from -4 to 3. This mirrors the Linux wacom driver, which does
        /// <c>rw = ((data[7] &amp; 0x18) &gt;&gt; 3) - ((data[7] &amp; 0x20) &gt;&gt; 3)</c>.
        /// </summary>
        /// <param name="auxByte">The auxiliary byte of the report, which is report[7]</param>
        /// <returns>The wheel delta in device steps</returns>
        public static int GetDelta(byte auxByte)
        {
            var steps = (auxByte >> 3) & 0x07;
            return steps >= 4 ? steps - 8 : steps;
        }
    }
}
