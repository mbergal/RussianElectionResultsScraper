using System;
using NHibernate.Linq;
using RussianElectionResultsScraper.Model;
using log4net;
using System.Linq;
using Type = RussianElectionResultsScraper.Model.Type;

namespace RussianElectionResultsScraper
{
    public class RepairCommand : BaseConsoleCommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public RepairCommand()
            {
            this.IsCommand("repair", "Repair scraped data");
            }

        public override int Run(string[] args)
            {
            base.Run(args);
            using ( var session = _electionResultsSessionFactory.OpenSession() )
                {
                var q = from s in session.Query<VotingPlace>().FetchMany( x=>x.Results ) select s;
                foreach (VotingPlace r in q )
                    {
                    Console.WriteLine( r );
                    }
                session.Flush();
                }
            return 0;
            }
    }
}
