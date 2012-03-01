using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using RussianElectionResultsScraper;
using RussianElectionResultsScraper.src;

namespace TestElectionResultsScraper
{
    [TestFixture]
    public class TestConfiguration
        {
        [Test]
        public void ShouldCorrectlyLoadFromXML()
            {
            var x = new XDocument( 
                new XElement( "configuration",
                    new XElement( "name", "Some Name" ),
                    new XElement( "counters",
                        new XElement("counter", new XAttribute("counter", "1"), new XAttribute("name", "counter name"), new XAttribute("shortname", "counter short name" ), new XAttribute( "color", "#EEEEEE"), new XAttribute( "candidate", "true" )))));
            var sw=new StringWriter();
            var c = new ElectionConfig
                        {
                        Counters = new[]
                                        {
                                        new CounterConfiguration()
                                            {
                                                Counter = "1",
                                                Name = "Counter Name",
                                                ShortName = "Counter Short Name",
                                                Color = Color.Blue,
                                                IsCandidate = true
                                            }
                                        }
                        };
            new XmlSerializer(typeof(ElectionConfig)).Serialize(sw, c);
            var cc = ElectionConfig.Load( new XmlNodeReader( x.ToXmlDocument() ) );
            Assert.AreEqual( "Some Name", cc.Name );
            Assert.AreEqual( 1, cc.Counters.Length );
            Assert.AreEqual( "1", cc.Counters[0].Counter );
            Assert.AreEqual( "counter name", cc.Counters[0].Name );
            Assert.AreEqual( true, cc.Counters[0].IsCandidate );
            Assert.AreEqual( "counter short name", cc.Counters[0].ShortName );
            Assert.AreEqual( Color.FromArgb( 0xEE, 0xEE, 0xEE ), cc.Counters[0].Color );
            }
        }

    public static class DocumentExtensions
    {
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
    }
    }