using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RussianElectionResultsScraper.Model.Validation
{
    class ParentChildrenSumMismatchRule : ValidationRule
        {
        public ParentChildrenSumMismatchRule(ValidationVotingPlace votingPlace ) 
            : base(votingPlace)
            {
            }

        public override IEnumerable<ValidationProblem> Check(ValidationVotingResult votingResult)
            {
            var childCounter = this._votingPlace.ChildCounter( votingResult.Counter );
            if ( childCounter != null )
                {
                int sumOfChildCounters = this._votingPlace.Children.Select( x =>
                                                                    {
                                                                    return x.GetCounter( childCounter ).Value;
                                                                    } ).Sum();
                if (sumOfChildCounters != votingResult.Value)
                    yield return new ParentChildrenSumMismatch(this._votingPlace, votingResult, sumOfChildCounters);
                }
            }
        }
}
