using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ReactiveUI;

namespace OpenTabletDriver.Plugins.Xml
{
    public class BindingElement : ReactiveObject, IXmlSerializable
    {
        private string _path;
        public string Path
        {
            set => this.RaiseAndSetIfChanged(ref _path, value);
            get => _path;
        }

        private string _value;
        public string Value
        {
            set => this.RaiseAndSetIfChanged(ref _value, value);
            get => _value;
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                reader.Read();
                Path = reader.GetAttribute("Path");
                Value = reader.Value;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Binding");
            writer.WriteAttributeString("Path", Path);
            writer.WriteValue(Value);
            writer.WriteEndElement();
        }
    }
}