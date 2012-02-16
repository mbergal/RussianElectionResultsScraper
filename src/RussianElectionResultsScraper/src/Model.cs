namespace RussianElectionResultsScraper
    {
    public class WorkItem
        {
        public bool     Recursive { get; set; }
        public bool     UpdateCounters { get; set;  }
        public string   ParentVotingPlaceId { get; set; }
        public string   Uri { get; set; }
        }
    }