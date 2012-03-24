using System.Linq;
using NUnit.Framework;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.Model.Validation;

namespace TestElectionResultsScraper
    {
    [TestFixture]
    public class TestChildCounterIsMissingInParentRule
        {
        [Test]
        public void Should_identify_problem_when_parent_counter_does_not_exist()
            {
            ValidationVotingPlace child1;
            var parent = Factory.MakeVotingPlace( 
                type: Type.TIK,
                children: new []
                              {
                              child1 = Factory.MakeVotingPlace( counters: new[] { Factory.MakeVotingResult( Counters._1, 1) }),
                              Factory.MakeVotingPlace( counters: new[] { Factory.MakeVotingResult( Counters._1, 1) })
                              });

            var problems = new ChildCounterIsMissingInParentRule( child1 ).Check( child1.Results[ Counters._1 ] ).ToList();
            Assert.That(problems.Count, Is.EqualTo( 1 ));
            Assert.That(problems[0], Is.InstanceOf<ChildCounterIsMissingInParent>());
            }


        }
    }
