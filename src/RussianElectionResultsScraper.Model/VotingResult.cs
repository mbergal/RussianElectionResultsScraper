using System;
using log4net;

namespace RussianElectionResultsScraper.Model
    {
    public class VotingResult
        {
        private static readonly ILog log = LogManager.GetLogger("VotingResult");

        public virtual int          Id { get; set; }
        public virtual string       Counter { get; set; }
        public virtual VotingPlace  VotingPlace { get; set; }
        public virtual int          Value { get; set; }
        public virtual decimal?     Percents
            {
            get {
                if ( VotingPlace != null && VotingPlace.Election != null && VotingPlace.Election.Counter( Counter ).IsCandidate )
                    {
                    try
                        {
                        return (decimal)Math.Round( (decimal) (100.00m * this.Value / (this.VotingPlace.NumberValidBallots + this.VotingPlace.NumberOfInvalidBallots )), 2 );
                        }
                    catch (Exception e)
                        {
                        log.Warn( string.Format( "Error calculating \"{0}/{1}/{2}\"", this.VotingPlace.Election.Id, this.VotingPlace.Id, this.Counter ), e );
                        return null;
                        }
                    }
                return null;
                }
            set {}
            }

        public virtual string       Message
            {
            get
                {
                return _message;
                }
            set
                {
                this._message = value;
                if ( this.VotingPlace != null )
                    this.VotingPlace.NumberOfErrors += string.IsNullOrWhiteSpace(this._message) ? -1 : 1;
                else
                    throw new Exception( "VotingResult has to have associated VotingPlace" );
                }
            }
        private string              _message;
        }
    }