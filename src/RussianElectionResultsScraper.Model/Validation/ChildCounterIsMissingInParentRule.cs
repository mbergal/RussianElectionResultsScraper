using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RussianElectionResultsScraper.Model.Validation
{
    public class ChildCounterIsMissingInParentRule : ValidationRule
        {
        public ChildCounterIsMissingInParentRule( ValidationVotingPlace votingPlace )
            : base( votingPlace )
            {
            }

        public override IEnumerable<ValidationProblem> Check( ValidationVotingResult votingResult )
            {
            var parent = this._votingPlace.Parent;
            if ( parent  != null && !this._votingPlace.Parent.Results.Values.Any(x => parent.ChildCounter( x.Counter ) == votingResult.Counter ) )
                yield return new ChildCounterIsMissingInParent( parent: this._votingPlace.Parent, child: this._votingPlace, counter: votingResult );
            }
        }

    
}
