using System.Linq;
using NUnit.Framework;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.Model.Validation;

namespace TestElectionResultsScraper
    {
    [TestFixture]
    public class TestParentChildrenSumMismatchRule
        {
        [Test]
        public void Should_identify_problem_when_parent_counters_do_not_match_child_counters()
            {
            var parent = Factory.MakeVotingPlace(
                counters: new[] {Factory.MakeVotingResult( Counters._1, 3 )},
                children: new[]
                              {
                                  Factory.MakeVotingPlace( counters: new[] {Factory.MakeVotingResult( Counters._1, 1 )} ),
                                  Factory.MakeVotingPlace( counters: new[] {Factory.MakeVotingResult( Counters._1, 1 )} )
                              } );


            var problems = new ParentChildrenSumMismatchRule( parent ).Check( parent.Results[Counters._1] ).ToList();


            Assert.That( 1, Is.EqualTo( problems.Count() ) );
            Assert.That( problems[0], Is.InstanceOf<ParentChildrenSumMismatch>() );
            }

        [Test]
        public void Should_not_identify_problems_when_parent_counter_equals_sum_of_child_counters()
            {
            var parent = Factory.MakeVotingPlace( 
                    counters: new [] { Factory.MakeVotingResult( Counters._1, 1 ) },
                    children: new []
                                  {
                                  Factory.MakeVotingPlace( counters: new [] { Factory.MakeVotingResult( Counters._1, 1 ) } ),
                                  } );

            var problems = new ParentChildrenSumMismatchRule( parent ).Check( parent.Results[ Counters._1 ]).ToList();

            Assert.That( problems.Count(), Is.EqualTo( 0 ) );
            }


        [Test]
        public void Should_not_check_absentee_ballots_a_z_counters()
            {
            var parent = Factory.MakeVotingPlace( 
                type: Type.TIK, 
                counters: new [] { Factory.MakeVotingResult( Counters.а, 3 ) },
                children: new []
                              {
                              Factory.MakeVotingPlace( counters: new [] { Factory.MakeVotingResult( Counters.а, 1 ) } ),
                              Factory.MakeVotingPlace( counters: new[] { Factory.MakeVotingResult( Counters.а, 1) } )
                              });

            var problems = new ParentChildrenSumMismatchRule( parent ).Check( parent.Results[ Counters.а ]).ToList();
            Assert.That( 0, Is.EqualTo(problems.Count()));
            }
        }
    }
