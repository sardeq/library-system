using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Xml.Serialization;

namespace LibrarySystem_Shared.Models
{
    [Serializable]
    [XmlType(Namespace = "http://tempuri.org/")]
    public class ReviewResult
    {
        [XmlElement(Order = 1)]
        public bool Success { get; set; }

        [XmlElement(Order = 2)]
        public string Sentiment { get; set; }

        [XmlElement(Order = 3)]
        public string Error { get; set; }
    }
}