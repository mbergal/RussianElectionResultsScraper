using System;
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

        public virtual DateTime                 Date
            {
            get; set;
            }

        public virtual DateTime                 LastUpdateTimeStamp { get; set; }

        public virtual VotingPlace              Root { get; set; }

        public virtual Iesi.Collections.Generic.ISet<CounterDescription> Counters { get; set; }

        public virtual void                     UpdateWithCurrentTimestamp()
            {
            this.LastUpdateTimeStamp = DateTime.Now;
            }

        public virtual CounterDescription       Counter( string counter )
            {
            return this.Counters.FirstOrDefault(x => x.Counter == counter );
            }

        public virtual Election                 SetCounter( string counter, string name, string shortName = null, Color? color = null, bool? isCandidate = null )
            {
            var counterDescription = this.Counter( counter );
            if ( counterDescription != null )
                {
                counterDescription.Color = color ?? counterDescription.Color;
                counterDescription.Name = name ?? counterDescription.Name;
                counterDescription.ShortName = shortName ?? counterDescription.ShortName;
                counterDescription.IsCandidate = isCandidate ?? counterDescription.IsCandidate;
                }
            else
                {
                this.Counters.Add(new CounterDescription
                                      {
                                          Election = this, 
                                          Counter = counter, 
                                          Name = name, 
                                          ShortName = shortName, 
                                          IsCandidate = isCandidate.HasValue ? isCandidate.Value : false, 
                                          Color = color.HasValue ? color.Value : new Color()
                                      } );
                }
            return this;
            }

        public virtual IList<CounterDescription>       Candidates
            {
            get
                {
                return Counters.Where( x => x.IsCandidate ).ToList();
                }
            }


        public virtual void Update( IEnumerable<CounterDescription> counters )
            {
            foreach (var c in counters)
                {
                this.SetCounter( c.Counter, c.Name, c.ShortName, c.Color, c.IsCandidate );
                }
                
            this.UpdateWithCurrentTimestamp();

            }

        }

}
