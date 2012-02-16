using System;
using Iesi.Collections.Generic;

namespace RussianElectionResultsScraper.Model
    {
    public class VotingResult
        {
        public virtual int          Id { get; set; }
        public virtual int          Counter { get; set; }
        public virtual string       VotingPlaceId { get; set; }
        public virtual int          Value { get; set; }
        }
    }