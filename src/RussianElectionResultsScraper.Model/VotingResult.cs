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
                var prevMessage = this._message;
                this._message = value;
                if (this.VotingPlace != null)
                    {
                    if ( string.IsNullOrWhiteSpace(this._message) && !string.IsNullOrWhiteSpace(prevMessage) )
                        this.VotingPlace.NumberOfErrors--;
                    else if ( !string.IsNullOrWhiteSpace(this._message) && string.IsNullOrWhiteSpace(prevMessage))
                        this.VotingPlace.NumberOfErrors++;
                    }
                    
                else
                    throw new Exception("VotingResult has to have associated VotingPlace");
                }
            }

        public virtual  bool        IsCalculated
            {
            get;
            set;
            }

        private string              _message;
        }
    }