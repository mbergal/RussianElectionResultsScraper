using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RussianElectionResultsScraper.Model;

namespace TestElectionResultsScraper
{
    [TestFixture]
    class TestValidationVotingPlace
        {
        [Test]
        public void ShouldIdentifyProblemWhenParentCountersDoNotMatchChildCounters()
            {
            var parent = Factory.MakeVotingPlace( counters: new [] { Factory.MakeVotingResult( "1", 3 ) } );
            Factory.MakeVotingPlace( parent: parent, counters: new [] { Factory.MakeVotingResult( "1", 1 ) } );
            Factory.MakeVotingPlace( parent: parent, counters: new[] { Factory.MakeVotingResult("1", 1) });

            var problems = parent.Check().ToList();
            Assert.That( 1, Is.EqualTo(problems.Count()) );
            Assert.That( problems[0], Is.InstanceOf<ChildCountersDontMatch>() );

            Factory.MakeVotingPlace(parent: parent, counters: new[] { Factory.MakeVotingResult("1", 1) });
            problems = parent.Check().ToList();
            Assert.That( 0, Is.EqualTo(problems.Count()));
            }

        [Test]
        public void ShouldIdentifyProblemWhenParentCountersDoNotExist()
            {
            var parent = Factory.MakeVotingPlace();
            Factory.MakeVotingPlace(parent: parent, counters: new[] { Factory.MakeVotingResult("1", 1) });
            Factory.MakeVotingPlace(parent: parent, counters: new[] { Factory.MakeVotingResult("1", 1) });

            var problems = parent.Check().ToList();
            Assert.That(problems.Count, Is.EqualTo( 2 ));
            Assert.That(problems[0], Is.InstanceOf<ChildCounterIsMissingInParent>());
            }
        }

    public partial class Factory
        {
        public static ValidationVotingPlace MakeVotingPlace( ValidationVotingPlace               parent = null, 
                                                             IEnumerable<ValidationVotingResult> counters = null )
            {
            var vp = new ValidationVotingPlace();
            if (parent != null)
                vp.Parent = parent;
            if ( counters != null )
                foreach (var c in counters)
                    vp.Results.Add( c.Counter, c );
            return vp;
            }

        public static ValidationVotingResult MakeVotingResult( string counter, int value )
            {
            var votingResult = new ValidationVotingResult {Counter = counter, Value = value};
            return votingResult;
            }

        }
}
