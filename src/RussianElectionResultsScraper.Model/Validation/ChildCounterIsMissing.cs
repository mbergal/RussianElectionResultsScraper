namespace RussianElectionResultsScraper.Model.Validation
{
    internal class ChildCounterIsMissing : ValidationProblem
        {
        private readonly ValidationVotingPlace _parent;
        private readonly ValidationVotingPlace _child;
        private readonly ValidationVotingResult _result;

        public ChildCounterIsMissing( ValidationVotingPlace     parent, 
                                      ValidationVotingPlace     child, 
                                      ValidationVotingResult    result )
            : base( result.Id, string.Format( "Is missing in child {{vp:{0}}}", child.Id ) )
            {
            this._parent = parent;
            this._child = child;
            this._result = result;
            }

        public override string ToString()
            {
            return string.Format( "Counter \"{0}\" is missing in child {1} of {2}", _result.Counter, _child.Id, _parent.Id );
            }
        }
}
