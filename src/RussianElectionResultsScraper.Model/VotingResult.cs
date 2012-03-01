namespace RussianElectionResultsScraper.Model
    {
    public class VotingResult
        {
        public virtual int          Id { get; set; }
        public virtual string       Counter { get; set; }
        public virtual VotingPlace  VotingPlace { get; set; }
        public virtual int          Value { get; set; }
        public virtual string       Message { get; set; }
        }
    }