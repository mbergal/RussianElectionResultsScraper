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
                        return (decimal)(100.00m * this.Value / (this.VotingPlace.NumberValidBallots + this.VotingPlace.NumberOfInvalidBallots));
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
        public virtual string       Message { get; set; }
        }
    }