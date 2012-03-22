using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RussianElectionResultsScraper.Model.Validation
{
    public class ChildCounterIsMissingInParent : ValidationProblem
    {
        private readonly ValidationVotingPlace _parent;
        private readonly ValidationVotingPlace _child;
        private readonly ValidationVotingResult _counter;

        public ChildCounterIsMissingInParent(ValidationVotingPlace parent, ValidationVotingPlace child, ValidationVotingResult counter)
            : base( counter.Id, string.Format( "Counter present in {{vp:{0}}} is missing here", child.Id ) )
            {
            this._parent = parent;
            this._child = child;
            this._counter = counter;
            }

        public ValidationVotingPlace Parent
            {
            get { return _parent; }
            }

        public ValidationVotingPlace Child
            {
            get { return _child; }
            }

        public ValidationVotingResult Counter
            {
            get { return _counter; }
            }

        public override string ToString()
            {
            return string.Format("Child counter {0} for {1} is missing in {2}", this.Counter.Counter, this.Child.Id, this.Parent.Id);
            }

    }
}
