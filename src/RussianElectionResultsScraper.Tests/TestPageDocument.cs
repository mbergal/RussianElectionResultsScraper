using System.IO;
using System.Reflection;
using System.Text;
using HtmlAgilityPack;
using NUnit.Framework;
using RussianElectionResultsScraper;

namespace TestElectionResultsScraper
{
    [TestFixture]
    public class TestElectionResultsScraper
        {
        [Test]
        public void ShouldCorrectlyParseVotingPlaceName()
            {
            var pageDocument = CreatePageDocument( "TestElectionResultsScraper.data.1.html" );
            Assert.AreEqual( "ЦИК России", pageDocument.Name );
            Assert.AreEqual( "ЦИК России", pageDocument.FullName );
            pageDocument = CreatePageDocument("TestElectionResultsScraper.data.2.html");
            Assert.AreEqual( "Благодарненская", pageDocument.Name );
            Assert.AreEqual("ЦИК России > Ставропольский край > Ставропольский край - Пятигорская > Благодарненская", pageDocument.FullName);
            }


        [Test]
        public void ShouldCorrectlyParseChildren()
            {
            var pageDocument = CreatePageDocument("TestElectionResultsScraper.data.1.html");
            Assert.AreEqual( 84, pageDocument.Children.Count );
            Assert.AreEqual( "http://www.vybory.izbirkom.ru/region/izbirkom?action=show&global=true&root=1000001&tvd=100100028713305&vrn=100100028713299&prver=0&pronetvd=null&region=0&sub_region=0&type=242&vibid=100100028713305", pageDocument.Children[0].Item1);
            Assert.AreEqual( "Республика Адыгея (Адыгея)", pageDocument.Children[0].Item2 );
            pageDocument = CreatePageDocument( "TestElectionResultsScraper.data.3.html" );
            Assert.AreEqual( null, pageDocument.Children );
            Assert.AreEqual("http://www.vybory.izbirkom.ru/region/izbirkom?action=show&global=true&root=12000001&tvd=2012000183551&vrn=100100028713299&prver=0&pronetvd=null&region=1&sub_region=1&type=242&vibid=2012000183551", pageDocument.ResultsRedirect);

            }

        [Test]
        public void ShouldCorrectlyParseRedirectPages()
            {
            var pageDocument = CreatePageDocument("TestElectionResultsScraper.data.2.html");
            Assert.AreEqual( true, pageDocument.IsRedirect );
            pageDocument = CreatePageDocument("TestElectionResultsScraper.data.4.html");
            Assert.AreEqual(false, pageDocument.IsRedirect);
            }

        [Test]
        public void ShouldCorrectlyParseCounters()
            {
            var pageDocument = CreatePageDocument("TestElectionResultsScraper.data.1.html");
            Assert.AreEqual("1", pageDocument.ResultTable[0][0]);
            Assert.AreEqual("Число избирателей, внесенных в список избирателей", pageDocument.ResultTable[0][1]);
            Assert.AreEqual("109237780", pageDocument.ResultTable[0][2]);
            Assert.AreEqual("8695522", pageDocument.ResultTable[20][2]);
            }

        public PageDocument         CreatePageDocument( string resourceName )
            {
            var html = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream( resourceName ), Encoding.GetEncoding(1251));
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(html);
            return new PageDocument(htmlDocument);
            }


        }
}
