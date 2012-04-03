using System;
using System.Drawing;
using System.Xml;
using System.Xml.Serialization;

namespace RussianElectionResultsScraper
{
    [Serializable]
    [XmlRoot( "configuration" )]
    public class ElectionConfig
        {
        [XmlElement("id")]
        public string Id;

        [XmlElement( "name" )]
        public string Name;

        [XmlElement( "date" )]
        public DateTime Date;

        [XmlArray( "counters")]
        [XmlArrayItem("counter")]
        public CounterConfiguration[] Counters;

        public static ElectionConfig Load( XmlReader xmlReader )
            {
            return (ElectionConfig) new XmlSerializer(typeof(ElectionConfig)).Deserialize(xmlReader);
            }

        public void Validate()
            {
            }
        }

    public class CounterConfiguration
        {
        [XmlAttribute("counter")]
        public string      Counter;

        [XmlAttribute("name")]
        public string   Name;

        [XmlAttribute("shortname")]
        public string   ShortName;

        [XmlAttribute("candidate")]
        public bool IsCandidate;
                
        [XmlIgnore]
        public Color    Color;

        [XmlAttribute("color")]
        public string HtmlColor
            {
            get { return ColorTranslator.ToHtml( this.Color ); }
            set { Color = ColorTranslator.FromHtml( value ); }
            }
        }
}
