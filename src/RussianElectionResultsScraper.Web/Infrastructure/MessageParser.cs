using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RussianElectionResultsScraper.Model;

namespace RussianElectionResultScraper.Web.Infrastructure
    {
    public class MessageParser
        {
        static readonly Regex vpRegex = new Regex( @"\{vp:(\d+)\}" );
        private readonly Func<string,VotingPlace>   _votingPlaceFactory;

        public MessageParser( Func<string,VotingPlace> votingPlaceFactory )
            {
            this._votingPlaceFactory = votingPlaceFactory;
            }


        public ParameterizedString ParseMessage( string message )
            {
            if (message != null)
                {
                int argPos = 0;
                var args = new List<object>();
                var format = vpRegex.Replace( message,
                                              match =>
                                                  {
                                                  var votingPlace = this._votingPlaceFactory( match.Groups[1].Value );
                                                  args.Add( votingPlace );
                                                  return string.Format( "{{{0}}}", argPos++ );
                                                  }
                                               );
                return new ParameterizedString( format, args.ToArray() );
                }
            else
                {
                return new ParameterizedString( "" );
                }
            }

        }

}