using Tommy;
using System.IO;

namespace OpenTabletDriver.Plugin.Toml
{
    public class Conf
    {
        /// <summary>
        /// Generates the config.toml unless it exist, returns true if it generated a new one, returns false if not.
        /// </summary>
        public static bool ConfGen() //This is ran at build and builds us our .toml
        {
            if (File.Exists(TomlLocation)) return false;
            // Generate a TOML file programmatically
            TomlTable toml = new TomlTable 
            {
                ["title"] = "OpenTabletDriver TOML",
                // You can also insert comments before a node with a special property
                ["logLevel"] = new TomlString
                {
                    Value = "1",
                    Comment = "Here you can set a log level, It will log everything with the priority you set, and higher (Lower numbers are higher) FATAL=-1 ERROR=0 WARN=1 INFO=2 DEBUG=3"
                }
            };

            // Write to a file (or any TextWriter)
            // You can forcefully escape ALL Unicode characters by uncommenting the following line:
            // TOML.ForceASCII = true;
            using(StreamWriter writer = File.CreateText(TomlLocation))
            {
                toml.WriteTo(writer);
                // Remember to flush the data if needed!
                writer.Flush();
            }
            return true;
        }


        /// <summary>
        /// Returns the value of key as a string.
        /// </summary>
        /// <param name="key"> The key to return the value for from the config.toml</param>
        public static string ConfGet(string key) //this gets the value of "key" out of our .toml
        {

            using(StreamReader reader = File.OpenText(TomlLocation)){
            
            // Parse the table
            TomlTable table = TOML.Parse(reader);

            // Return the value at key as string
            return table[key].ToString();
            
            }

        }


        /// <summary>
        /// Sets the value of key to a certain value in the config.toml.
        /// </summary>
        /// <param name="key"> The key to set the value for from the config.toml</param>
        /// <param name="value"> The value to set the key to in the config.toml</param>
        public static void ConfSet(string key, string value) //this sets the value of "key" to "value" in our .toml
        {

            using(StreamReader reader = File.OpenText(TomlLocation))
            {
            
                // Parse the table
                TomlTable toml = TOML.Parse(reader);

                toml[key] = value;
                
                using(StreamWriter writer = File.CreateText(TomlLocation))
                {
                    toml.WriteTo(writer);
                    // Remember to flush the data if needed!
                    writer.Flush();
                }

            }

        }

        public static string TomlLocation { set; get; }

    }
     
}