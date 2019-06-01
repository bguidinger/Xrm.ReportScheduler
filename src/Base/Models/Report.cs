namespace BGuidinger.Base
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot(Namespace = "http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition", ElementName = "Report")]
    public class Report
    {
        public List<ReportParameter> ReportParameters { get; set; }
    }
}