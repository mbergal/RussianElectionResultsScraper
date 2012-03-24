
namespace RussianElectionResultsScraper.Model.Validation
    {
    using System.Collections.Generic;
    using System.Linq;

    internal class ChildCounterIsMissingRule : ValidationRule
        {
        public ChildCounterIsMissingRule(ValidationVotingPlace votingPlace )
            : base( votingPlace )
            {
            }

        public override IEnumerable<ValidationProblem> Check(ValidationVotingResult votingResult)
            {
            string counter = this._votingPlace.ChildCounter(votingResult.Counter);
            if ( counter != null )
                foreach (var c in this._votingPlace.Children)
                    if ( c.Results.Any() && c.GetCounter(counter) == null )
                        yield return new ChildCounterIsMissing(this._votingPlace, c, votingResult);
            }
        }
    }
