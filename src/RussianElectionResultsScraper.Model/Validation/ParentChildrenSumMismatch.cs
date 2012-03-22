using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RussianElectionResultsScraper.Model.Validation
{
    public class ParentChildrenSumMismatch : ValidationProblem
    {
        private readonly ValidationVotingPlace _votingPlace;
        private readonly ValidationVotingResult _counter;
        private readonly int _sumOfChildCounters;

        public ParentChildrenSumMismatch(ValidationVotingPlace parent, ValidationVotingResult counter, int sumOfChildCounters)
            : base( counter.Id, string.Format( "Sum of child counters = {0}", sumOfChildCounters ) )
            {
            this._votingPlace = parent;
            this._counter = counter;
            this._sumOfChildCounters = sumOfChildCounters;
            }

        protected ValidationVotingResult Counter
            {
            get { return _counter; }
            }

        public ValidationVotingPlace VotingPlace
            {
            get { return this._votingPlace; }
            }

        public int SumOfChildCounters
        {
            get { return this._sumOfChildCounters; }
        }

        public override string ToString()
            {
            return string.Format("Then value of counter \"{0}\" of {1} does not match the sum of counters of its children ({2} != {3})", 
                Counter.Counter, 
                VotingPlace.Id, 
                Counter.Value, 
                SumOfChildCounters );
            }
    }
}
