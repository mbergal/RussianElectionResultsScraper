using System;
using System.Drawing;
using System.Xml;
using System.Xml.Serialization;

namespace RussianElectionResultsScraper.src
{
    [Serializable]
    [XmlRoot( "configuration" )]
    public class Configuration
        {
        [XmlElement("id")]
        public string Id;

        [XmlElement( "name") ]
        public string Name;

        [XmlArray( "counters")]
        [XmlArrayItem("counter")]
        public CounterConfiguration[] Counters;

        public static Configuration Load( XmlReader xmlReader )
            {
            return (Configuration) new XmlSerializer(typeof(Configuration)).Deserialize(xmlReader);
            }

        public void Validate()
            {
            }
        }

    public class CounterConfiguration
        {
        [XmlAttribute("counter")]
        public int      Counter;

        [XmlAttribute("name")]
        public string   Name;

        [XmlAttribute("shortname")]
        public string   ShortName;

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
