using Iesi.Collections.Generic;
using System.Linq;

namespace RussianElectionResultsScraper.Model
{
    public class Election
        {
        public virtual string                   Id { get; set; }
        public virtual string                   Name { get; set; }
        public virtual VotingPlace              Root { get; set; }
        public virtual ISet<CounterDescription> Counters { get; set; }
        public virtual CounterDescription       Counter(int counter)
            {
            return this.Counters.FirstOrDefault(x => x.Counter == counter );
            }
        }

}
