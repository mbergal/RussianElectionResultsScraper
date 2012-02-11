using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace RussianElectionResultsScraper
{
    public class Anchor
        {
        public string href;
        public string text;
        }
    public class PageDocument
    {
        private HtmlDocument htmlDocument;

        public PageDocument(HtmlDocument htmlDocument)
        {
            this.htmlDocument = htmlDocument;
        }

        public Anchor[] History
            {
            get
                {
                var nameCell = this.htmlDocument.DocumentNode.SelectSingleNode("//table[@width='100%' and @height='80%']/tr[@height='20px']/td");
                return nameCell.SelectNodes("a").Select(x => new Anchor() { href = x.GetAttributeValue( "href", null ), text = x.InnerText } ).ToArray();
                }
            }


        public string FullName
            {
            get
                {
                return string.Join(" > ", this.History.Select( x=> x.text ) );
                }
            }

        public string Name
        {
            get
            {
                return this.History.Select( x=> x.text ).LastOrDefault();
            }
        }

        public IList<Tuple<string, string>> Children
            {
            get
                {
                var nodes = this.htmlDocument.DocumentNode.SelectNodes("//select[@name='gs']/option");
                if (nodes != null)
                    return nodes.Select(x => new Tuple<string, string>(
                    HttpUtility.HtmlDecode(x.GetAttributeValue("value", null)),
                    HttpUtility.HtmlDecode(x.NextSibling.InnerText).Trim())).Where(x => x.Item2 != "---").ToList();
                else
                    return null;
                }
            }

        public IList<List<string>> ResultTable
            {
            get
                {
                var table = new List<List<string>>();
                var r = this.htmlDocument.DocumentNode.SelectNodes("//table[@border='0' and @bgcolor='#ffffff' and @cellpadding='2' and @cellspacing='1']/tr");
                if (r != null)
                    foreach (var a in r)
                        {
                        var tds = a.SelectNodes("td").ToArray();
                        table.Add(tds.Select(x => x.FirstChild != null ? x.FirstChild.InnerText : x.InnerText ).ToList());
                        }
                return table;
                }
            }

        public bool IsRedirect
            {
            get {
                return this.ResultsRedirect != null;
                }
            }

        public string ResultsRedirect
            {
            get {
                var a = this.htmlDocument.DocumentNode.SelectSingleNode("//a[text()='сайт избирательной комиссии субъекта Российской Федерации']");
                if ( a != null )
                    {
                    var href = a.GetAttributeValue("href", null);
                    if (href != null)
                        return HttpUtility.HtmlDecode(href);
                    }
                    
                return null;
                }
            }

        public string PageText
            {
            get
                {
                var a = new StringWriter();
                this.htmlDocument.Save(a);
                return a.ToString();
                }
            }
    }
}
