using System.Collections.Generic;

namespace RussianElectionResultsScraper.Model.Validation
    {
    public class AbsenteeBallotsCountersMismatchRule : ValidationRule
        {
        public AbsenteeBallotsCountersMismatchRule( ValidationVotingPlace vp )
            : base( vp )
            {
            }

        public override IEnumerable<ValidationProblem> Check()
            {
            switch ( this._votingPlace.Type )
                {
                case Type.RIK:
                    {
                    var д = this._votingPlace.GetCounter( Counters.д );
                    var е = this._votingPlace.GetCounter( Counters.е );
                    var ж = this._votingPlace.GetCounter( Counters.ж );
                    var з = this._votingPlace.GetCounter( Counters.з );

                    if ( д != ( е + ж + з ) )
                        yield return new AbsenteeBallotsCountersMismatch( 
                            this._votingPlace, 
                            Counters.д,
                            string.Format( "д({0}) != е({1}) + ж({2}) + З({3})", д, е, ж, з ) );
                    break;
                    }

                case Type.TIK:
                    {
                    var а = this._votingPlace.GetCounter( Counters.а );
                    var б = this._votingPlace.GetCounter( Counters.б );
                    var в = this._votingPlace.GetCounter( Counters.в );
                    var г = this._votingPlace.GetCounter( Counters.г );
                    if ( а != ( б + в + г ) )
                        yield return new AbsenteeBallotsCountersMismatch( 
                            this._votingPlace, 
                            Counters.а, 
                            string.Format( "а({0}) != б({1}) + в({2}) + Г({3})", а, б, в, г ) );
                    break;
                    }

                case Type.OIK:
                case Type.CIK:
                    break;
                }
            }
        }
    }
