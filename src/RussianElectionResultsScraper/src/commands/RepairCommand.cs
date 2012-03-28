using System;
using NHibernate.Linq;
using RussianElectionResultsScraper.Model;
using log4net;
using System.Linq;
using Type = RussianElectionResultsScraper.Model.Type;

namespace RussianElectionResultsScraper
{
    public class RepairCommand : BaseCommand
       {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        const int batchSize = 10000;

        public RepairCommand( string connectionString )
            {
            }

        public override int Execute()
            {
            using ( var session = _electionResultsSessionFactory.OpenSession() )
                {
                int numOfEntities = 0;
                int batchNumber = 0;
                while ( true )
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
