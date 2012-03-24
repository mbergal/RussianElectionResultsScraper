using System.Linq;
using NUnit.Framework;

namespace TestElectionResultsScraper
    {
    public class TestChildCounterIsMissingRule
        {
        [Test]
        public void Should_identify_problem_when_child_counter_does_not_exist()
            {
            var parent = Factory.MakeVotingPlace( counters: new [] { Factory.MakeVotingResult( "1", 3 ) } );
            Factory.MakeVotingPlace( parent: parent, counters: new [] { Factory.MakeVotingResult( "1", 3 ) } );
            Factory.MakeVotingPlace( parent: parent );

            var problems = parent.Check().ToList();
            Assert.That( 0, Is.EqualTo(problems.Count()) );
            }
        }
    }
