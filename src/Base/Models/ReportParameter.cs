using System.Xml.Serialization;

namespace BGuidinger.Base
{
    public class ReportParameter
    {
        [XmlAttribute]
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool Hidden { get; set; } = false;
        public bool Nullable { get; set; } = false;
    }
}
