using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RussianElectionResultsScraper.Model.Validation
{
    class ChildCounterIsMissingRule : ValidationRule
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
                    if (c.GetCounter(counter) == null)
                        yield return new ChildCounterIsMissing(this._votingPlace, c, votingResult);
            }
        }
}
