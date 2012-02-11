using System;
using Iesi.Collections.Generic;
using ServiceStack.DesignPatterns.Model;

namespace RussianElectionResultsScraper
    {
    public class WorkItem
        {
        public virtual int      Id { get; set; }
        public virtual string   ParentVotingPlaceId { get; set; }
        public virtual string   Uri { get; set; }
        public virtual DateTime? ProcessingStartedTimestamp { get; set; }
        }

    public class VotingPlace 
        {
        public VotingPlace()
            {
            Results = new HashedSet<VotingResult>();
            }
        public virtual string       Id { get; set; }
        public virtual string       Name { get; set; }
        public virtual string       FullName { get; set; }
        public virtual string       Uri { get; set; }
        public virtual VotingPlace  Parent { get; set; }
        public virtual ISet<VotingResult> Results { get; set; }
        }

    public class VotingResult
        {
        public virtual int          Id { get; set; }
        public virtual int          Counter { get; set; }
        public virtual string       VotingPlaceId { get; set; }
        public virtual int          Value { get; set; }
        }
    }