using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using RussianElectionResultsScraper.Model;
using Type = System.Type;

namespace RussianElectionResultScraper.Web.Infrastructure
{
    public static class HtmlCounterMessageHelper 
        {
        public static Regex vpRegex = new Regex( @"\{vp:(\d+)\}" );
        public static MvcHtmlString CounterMessage( this HtmlHelper html, ParameterizedString message )
            {
            return new MvcHtmlString( string.Format( new CounterMessageFormatProvider( html ), message.Format, message.Args ) );
            }
        }

    class CounterMessageFormatProvider : IFormatProvider, ICustomFormatter
        {
        private readonly HtmlHelper _htmlHelper;

        public CounterMessageFormatProvider( HtmlHelper htmlHelper )
            {
            this._htmlHelper = htmlHelper;
            }

        public object GetFormat( Type formatType )
            {
            return formatType == typeof(ICustomFormatter)
                ? this
                : null;
            }

        public string Format( string format, object arg, IFormatProvider formatProvider )
            {
            if ( arg.GetType() == typeof ( VotingPlace ) )
                {
                var vp = (VotingPlace) arg;
                return _htmlHelper.ActionLink( vp.Name, MVC.Home.Place( ((VotingPlace)arg).Id, null ) ).ToString();
                }
            else
                return arg.ToString();
            }
        }
}
