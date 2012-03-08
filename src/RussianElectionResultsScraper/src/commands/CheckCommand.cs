using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using RussianElectionResultsScraper.Model;
using log4net;
using System.Linq;
using Type = RussianElectionResultsScraper.Model.Type;
using Dapper;

namespace RussianElectionResultsScraper
{
    public class CheckCommand : BaseConsoleCommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public CheckCommand()
            {
            this.IsCommand( "check", "Check data integrity");
            }

        public class ChecksumMismatch
            {
		    public string  SumVotingPlaceId;
            public string  SumCounter;
		    public int?    SumValue;
            public int?    Id;
		    public string  VotingPlaceId;
		    public string  Counter;
            public int     Value;
            }

        public override int Run(string[] args)
            {
            base.Run(args);

            using ( var session = _electionResultsSessionFactory.OpenSession() )
                {
                const int commandTimeout = 120;
                session.Connection.Execute( "update VotingResult set Message = null where Message is not null ", commandTimeout: commandTimeout );
                session.Connection.Execute("update VotingPlace set NumberOfErrors = 0 where NumberOfErrors <> 0", commandTimeout: commandTimeout );
                var mismatches = session.Connection.Query<ChecksumMismatch>(Assembly.GetExecutingAssembly().LoadTextResource("RussianElectionResultsScraper.src.commands.sql.check-counters-sums.sql"), commandTimeout: commandTimeout ).ToList();
                UpdateMissingParentCounters( session, mismatches );
                mismatches = session.Connection.Query<ChecksumMismatch>(Assembly.GetExecutingAssembly().LoadTextResource("RussianElectionResultsScraper.src.commands.sql.check-counters-sums.sql"), commandTimeout: commandTimeout ).ToList();
                MarkMissingChildCounters(session, mismatches);
                mismatches = session.Connection.Query<ChecksumMismatch>(Assembly.GetExecutingAssembly().LoadTextResource("RussianElectionResultsScraper.src.commands.sql.check-counters-sums.sql"), commandTimeout: commandTimeout ).ToList();
                MarkNonMatchingParentAndChildCounters(session, mismatches);
                }
            return 0;
            }

        private static void MarkNonMatchingParentAndChildCounters(ISession session, IEnumerable<ChecksumMismatch> mismatches)
            {
            var notMatchingChildAndParentCounters =
                mismatches.Where(x => x.SumValue != null && x.Value != null && x.SumValue != x.Value && x.SumVotingPlaceId != null );
            using (var transaction = session.BeginTransaction())
                {
                notMatchingChildAndParentCounters.ForEach(x =>
                                                              {
                                                              var vr = session.Get<VotingResult>(x.Id);
                                                              var vp = vr.VotingPlace;
                                                              vr.Message =
                                                                  string.Format(
                                                                      "Counter value is {0}, but sum of child values is {1}",
                                                                      x.Value, x.SumValue);
                                                              });
                transaction.Commit();
                }
            }

        private static void UpdateMissingParentCounters(ISession session, IEnumerable<ChecksumMismatch> mismatches)
            {
            var noParentCounters = mismatches.Where(x => x.VotingPlaceId == null);
            var noCountersVotingPlaces = noParentCounters.Select(x => x.SumVotingPlaceId).Distinct().Where(x => x != null);

            using (var transaction = session.BeginTransaction())
                {
                noCountersVotingPlaces.ForEach(x => session.Get<VotingPlace>(x).UpdateCountersFromChildren());
                transaction.Commit();
                }
            ;
            }

        private static void MarkMissingChildCounters(ISession session, IEnumerable<ChecksumMismatch> mismatches)
            {
            var noChildCounters = mismatches.Where(x => x.SumValue == null);
            using (var transaction = session.BeginTransaction())
                {
                noChildCounters.ForEach(x => { session.Get<VotingResult>(x.Id).Message = "No child counter"; });
                transaction.Commit();
                }
            ;
            }
    }
}
