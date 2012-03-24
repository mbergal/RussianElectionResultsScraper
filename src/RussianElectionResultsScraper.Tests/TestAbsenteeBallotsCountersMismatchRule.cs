using System.Linq;
using NUnit.Framework;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.Model.Validation;

namespace TestElectionResultsScraper
    {
    [TestFixture]
    public class TestAbsenteeBallotsCountersMismatchRule
        {
        [Test]
        public void Should_identify_mismatches_for_TIK()
            {
            var vp = Factory.MakeVotingPlace(
                type: RussianElectionResultsScraper.Model.Type.TIK,
                counters: new[]
                            {
                            Factory.MakeVotingResult( Counters.а, 12 ),
                            Factory.MakeVotingResult( Counters.б, 1 ),
                            Factory.MakeVotingResult( Counters.в, 1 ),
                            Factory.MakeVotingResult( Counters.г, 1 ),
                            } );

            var problems = new AbsenteeBallotsCountersMismatchRule( vp ).Check().ToList();
            Assert.That( problems.Count(), Is.EqualTo( 1 )  );
            Assert.That( problems[0], Is.InstanceOf<AbsenteeBallotsCountersMismatch>()  );
            }

        [Test]
        public void Should_identify_mismatches_for_RIK()
            {
            var vp = Factory.MakeVotingPlace(
                type: RussianElectionResultsScraper.Model.Type.RIK,
                counters: new[]
                            {
                            Factory.MakeVotingResult( Counters.д, 12 ),
                            Factory.MakeVotingResult( Counters.е, 1),
                            Factory.MakeVotingResult( Counters.ж, 1 ),
                            Factory.MakeVotingResult( Counters.з, 1 ),
                            } );

            var problems = new AbsenteeBallotsCountersMismatchRule( vp ).Check().ToList();
            Assert.That( problems.Count(), Is.EqualTo( 1 )  );
            Assert.That( problems[0], Is.InstanceOf<AbsenteeBallotsCountersMismatch>()  );
            }

        }


    }
