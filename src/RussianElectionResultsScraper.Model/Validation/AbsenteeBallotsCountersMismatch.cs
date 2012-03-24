namespace RussianElectionResultsScraper.Model.Validation
    {
    public class AbsenteeBallotsCountersMismatch : ValidationProblem
        {
        private readonly ValidationVotingPlace _votingPlace;

        public AbsenteeBallotsCountersMismatch( ValidationVotingPlace vp, string counter, string message  )
            : base( vp.Results[ counter ].Id, message )
            {
            this._votingPlace = vp;
            }

        public override string ToString()
            {
            return string.Format( "Counters for absentee ballots do not match in {0}: {1}", this._votingPlace.Id, this.ValidationMessage);
            }
        }
    }
