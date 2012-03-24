
namespace TestElectionResultsScraper
    {
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using RussianElectionResultsScraper.Model;
    using RussianElectionResultsScraper.Model.Validation;
    using Type = RussianElectionResultsScraper.Model.Type;

    [TestFixture]
    public class TestValidationVotingPlace
        {

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
            Assert.AreEqual( Counters.а, cik.ChildCounter( Counters.д ) );
            Assert.AreEqual( Counters.б, cik.ChildCounter( Counters.е ) );
            Assert.AreEqual( Counters.в, cik.ChildCounter( Counters.ж ) );
            Assert.AreEqual( Counters.г, cik.ChildCounter( Counters.з ) );
            }

        [Test]
        public void Should_correctly_determine_child_counter_name_for_TIK()
            {
            var cik = new ValidationVotingPlace() { Type = Type.TIK };
            Assert.AreEqual( null, cik.ChildCounter( Counters.а ) );
            Assert.AreEqual( null, cik.ChildCounter( Counters.б ) );
            Assert.AreEqual( null, cik.ChildCounter( Counters.в ) );
            Assert.AreEqual( null, cik.ChildCounter( Counters.г ) );
            }

        }


    public partial class Factory
        {
        public static ValidationVotingPlace MakeVotingPlace( ValidationVotingPlace               parent = null, 
                                                             Type?                               type = null,
                                                             IEnumerable<ValidationVotingResult> counters = null,
                                                             ValidationVotingPlace[]             children = null )
            {
            var vp = new ValidationVotingPlace();
            if (parent != null)
                vp.Parent = parent;
            if ( counters != null )
                foreach (var c in counters)
                    vp.Results.Add( c.Counter, c );
            if ( type != null )
                vp.Type = type.Value;
            if ( children != null )
                foreach ( var c in children )
                    c.Parent = vp;
            return vp;
            }

        public static ValidationVotingResult MakeVotingResult( string counter, int value )
            {
            var votingResult = new ValidationVotingResult {Counter = counter, Value = value};
            return votingResult;
            }

        }
    }
