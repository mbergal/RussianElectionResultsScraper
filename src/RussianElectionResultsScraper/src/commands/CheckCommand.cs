using System.Collections.Generic;
using System.Linq;
using Dapper;
using log4net;
using NHibernate;
using NHibernate.Linq;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.Model.Validation;
using RussianElectionResultsScraper.src.commands.sql;

namespace RussianElectionResultsScraper
{
    public class CheckCommand : BaseCommand
    {
        private static readonly ILog log = LogManager.GetLogger("CheckCommand");
        const int commandTimeout = 120;

        public CheckCommand( string connectionString, string electionId )
            : base( connectionString: connectionString, electionid: electionId )
            {
            }

        public class ChecksumMismatch
        {
            public string SumVotingPlaceId;
            public string SumCounter;
            public int? SumValue;
            public int? Id;
            public string VotingPlaceId;
            public string Counter;
            public int Value;
        }

        public override int Execute()
           {
           using (var session = _electionResultsSessionFactory.OpenSession())
            {
                log.Info("Clearing out existing messages in VotingResults.");
                session.Connection.Execute(new ClearMessages().TransformText(), commandTimeout: commandTimeout, param: new { electionId = this._electionId });
                log.Info("Deleting auto-calculated counters.");
                session.Connection.Execute(new DeleteAutocalculatedResults().TransformText(), commandTimeout: commandTimeout, param: new { electionId = this._electionId });
                log.Info("Clearing NumOfErrors in VotingPlaces.");
                session.Connection.Execute(new ResetNumberOfErrors().TransformText(), commandTimeout: commandTimeout, param: new { electionId = this._electionId });
            }

            UpdateMissingRegionCounters( this._electionId );



            using (var session = _electionResultsSessionFactory.OpenSession())
            {
                var votingPlaces = session.Connection.Query<ValidationVotingPlace>("select Id, ParentId, Type from VotingPlace where ElectionId = @electionId", commandTimeout: commandTimeout, param: new { electionId = this._electionId });
                var votingResults = session.Connection.Query<ValidationVotingResult>("select vr.Id, vr.VotingPlaceId, vr.Counter, vr.Value, vr.IsCalculated from VotingResult vr inner join VotingPlace vp on vr.VotingPlaceId = vp.Id where vp.ElectionId = @electionId", commandTimeout: commandTimeout, param: new { electionId = this._electionId });

                var votingPlacesById = new Dictionary<string, ValidationVotingPlace>();
                votingPlaces.ForEach(x => votingPlacesById.Add(x.Id, x));
                votingPlaces.ForEach( x =>  {
                                            if (x.ParentId != null)
                                                x.Parent = votingPlacesById[x.ParentId];
                                            });
                votingResults.ForEach(x =>
                    {
                    votingPlacesById[x.VotingPlaceId].Results[x.Counter] = x;
                    });

                var root = votingPlaces.FirstOrDefault(x => x.Parent == null);
                var problems = root.Check();
                log.Info("Identified problems: ");
                problems.ForEach(x => log.Info("   " + x.ToString()));
                this.MarkProblems( problems );
            }

            return 0;
        }

        private void MarkProblems( IEnumerable<ValidationProblem> problems )
            {
            using (var session = _electionResultsSessionFactory.OpenSession())
            using (var transaction = session.BeginTransaction() )
                {
                problems.ForEach( x=>
                                      {
                                      var vr = session.Get<VotingResult>( x.VotingResultId );
                                      vr.Message = x.ValidationMessage;
                                      } );
                transaction.Commit();
                }

            }

        //        private void LogMismatches(string s, IEnumerable<ChecksumMismatch> mismatches)
        //            {
        //            log.Info(mismatches.ToPS().FormatTable(AutoSize: true).ToString());
        //            }
        //
        //        public IEnumerable<ChecksumMismatch> GetMismatches( string electionId )
        //            {
        //            using (var session = _electionResultsSessionFactory.OpenSession())
        //                {
        //                string query = new CheckCountersSumsSQL( electionId: electionId ).TransformText();
        //                return session.Connection.Query<ChecksumMismatch>( query, commandTimeout: commandTimeout, param: new { electionId = electionId }).ToList();
        //                }
        //            }

        private static void MarkNonMatchingParentAndChildCounters(ISession session, IEnumerable<ChecksumMismatch> mismatches)
        {
            var notMatchingChildAndParentCounters =
                mismatches.Where(x => x.SumValue != null && x.Value != null && x.SumValue != x.Value && x.SumVotingPlaceId != null);
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

        private void UpdateMissingRegionCounters( string electionId )
            {
            using (ISession session = _electionResultsSessionFactory.OpenSession())
                {
                var election = session.Get<Election>(electionId);
                var summaries = session.Query<VotingPlace>().Where(x => x.Type == Type.Summary && x.Election == election ).ToList();
                using (var transaction = session.BeginTransaction())
                    {
                    summaries.ForEach(x => x.UpdateCountersFromChildren());
                    transaction.Commit();
                    }
                }

            //        private static void MarkMissingChildCounters(ISession session, IEnumerable<ChecksumMismatch> mismatches)
            //            {
            //            var noChildCounters = mismatches.Where(x => x.SumValue == null);
            //            using (var transaction = session.BeginTransaction())
            //                {
            //                noChildCounters.ForEach(x => { session.Get<VotingResult>(x.Id).Message = "No child counter"; });
            //                transaction.Commit();
            //                }
            //            ;
            //            }
            }

    }
}

  