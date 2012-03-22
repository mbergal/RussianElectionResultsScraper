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
            this.HasElectionOption();
            }

        public override int Run(string[] args)
            {
            base.Run(args);
            using ( var session = _electionResultsSessionFactory.OpenSession() )
                {
                const int batchSize = 10000;
                int numOfEntities = 0;
                int batchNumber = 0;
                while( true )
                    {
                    var q = session.Query<VotingPlace>().Where( x=>x.Election.Id == this._electionId ).OrderBy( x=>x.Id).Skip(batchNumber * batchSize).Take(batchSize).FetchMany(x => x.Results);
                    bool reachedEnd = true;
                    foreach (VotingPlace r in q )
                        {
                        numOfEntities++;
                        reachedEnd = false;
                        }
                    batchNumber++;
                    Console.WriteLine( numOfEntities );
                    session.Flush();
                    session.Clear();
                    if (reachedEnd)
                        break;
                    }
                }
            return 0;
            }
    }
}
