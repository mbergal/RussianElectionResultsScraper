using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Iesi.Collections.Generic;

namespace RussianElectionResultsScraper.Model
{
    public class Election
        {
        public Election()
            {
            this.Counters = new HashedSet<Model.CounterDescription>();
            }
        public virtual string                   Id { get; set; }
        public virtual string                   Name { get; set; }
        public virtual VotingPlace              Root { get; set; }
        public virtual Iesi.Collections.Generic.ISet<CounterDescription> Counters { get; set; }
        public virtual CounterDescription       Counter(int counter)
            {
            return this.Counters.FirstOrDefault(x => x.Counter == counter );
            }
        public virtual IList<CounterDescription>       Candidates
            {
            get
                {
                return Counters.Where( x => x.Counter >= 19 ).ToList();
                }
            }
        }

}
