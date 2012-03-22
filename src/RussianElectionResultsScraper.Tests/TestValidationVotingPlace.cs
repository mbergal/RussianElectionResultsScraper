using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.Model.Validation;
using Type = RussianElectionResultsScraper.Model.Type;

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
            Assert.That( problems[0], Is.InstanceOf<ParentChildrenSumMismatch>() );

            Factory.MakeVotingPlace(parent: parent, counters: new[] { Factory.MakeVotingResult("1", 1) });
            problems = parent.Check().ToList();
            Assert.That( 0, Is.EqualTo(problems.Count()));
            }

        [Test]
        public void Should_identify_problem_when_parent_counter_does_not_exist()
            {
            var parent = Factory.MakeVotingPlace();
            Factory.MakeVotingPlace(parent: parent, counters: new[] { Factory.MakeVotingResult("1", 1) });
            Factory.MakeVotingPlace(parent: parent, counters: new[] { Factory.MakeVotingResult("1", 1) });

            var problems = parent.Check().ToList();
            Assert.That(problems.Count, Is.EqualTo( 2 ));
            Assert.That(problems[0], Is.InstanceOf<ChildCounterIsMissingInParent>());
            }

        [Test]
        public void Should_correctly_determine_child_counter_name_for_CIK()
            {
            var cik = new ValidationVotingPlace() { Type = Type.CIK };
            Assert.AreEqual( "д", cik.ChildCounter( "и" ) );
            Assert.AreEqual( "е", cik.ChildCounter("к") );
            Assert.AreEqual( "ж", cik.ChildCounter("л") );
            Assert.AreEqual( "з", cik.ChildCounter("м") );
            }

        [Test]
        public void Should_correctly_determine_child_counter_name_for_Summary()
            {
            var cik = new ValidationVotingPlace() { Type = Type.Summary };
            Assert.AreEqual("а", cik.ChildCounter("а"));
            Assert.AreEqual("б", cik.ChildCounter("б"));
            Assert.AreEqual("в", cik.ChildCounter("в"));
            Assert.AreEqual("г", cik.ChildCounter("г"));
            Assert.AreEqual("д", cik.ChildCounter("д"));
            Assert.AreEqual("е", cik.ChildCounter("е"));
            Assert.AreEqual("ж", cik.ChildCounter("ж"));
            Assert.AreEqual("з", cik.ChildCounter("з"));
            }

        [Test]
        public void Should_correctly_determine_child_counter_name_for_RIK()
            {
            var cik = new ValidationVotingPlace() { Type = Type.RIK };
            Assert.AreEqual( Counters.А, cik.ChildCounter( Counters.Д ) );
            Assert.AreEqual( Counters.Б, cik.ChildCounter( Counters.Е ) );
            Assert.AreEqual( Counters.В, cik.ChildCounter( Counters.Ж ) );
            Assert.AreEqual( Counters.Г, cik.ChildCounter( Counters.З ) );
            }

        [Test]
        public void Should_correctly_determine_child_counter_name_for_TIK()
            {
            var cik = new ValidationVotingPlace() { Type = Type.TIK };
            Assert.AreEqual( null, cik.ChildCounter( Counters.А ) );
            Assert.AreEqual( null, cik.ChildCounter( Counters.Б ) );
            Assert.AreEqual( null, cik.ChildCounter( Counters.В ) );
            Assert.AreEqual( null, cik.ChildCounter( Counters.Г ) );
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
