using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RussianElectionResultsScraper.Model.Validation
{
    public class ParentChildrenSumMismatchRule : ValidationRule
        {
        public ParentChildrenSumMismatchRule(ValidationVotingPlace votingPlace ) 
            : base(votingPlace)
            {
            }

        public override IEnumerable<ValidationProblem> Check(ValidationVotingResult votingResult)
            {
            if (!Counters.Hierarchical.Contains(votingResult.Counter))
                {
                var childCounter = this._votingPlace.ChildCounter( votingResult.Counter );
                if ( childCounter != null )
                    {
                    int sumOfChildCounters = this._votingPlace.Children.Select( x => x.GetCounter( childCounter ).HasValue 
                                                                                         ? x.GetCounter( childCounter ).Value 
                                                                                         : 0 ).Sum();
                    if (sumOfChildCounters != votingResult.Value)
                        yield return new ParentChildrenSumMismatch(this._votingPlace, votingResult, sumOfChildCounters);
                    }
                }
            }
        }
}
