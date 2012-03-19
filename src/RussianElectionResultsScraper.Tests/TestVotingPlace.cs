using NUnit.Framework;
using RussianElectionResultsScraper.Model;

namespace TestElectionResultsScraper
{
    [TestFixture]
    public class TestVotingPlace
        {
        [Test]
        public void ShouldBeAbleToSetCounters()
            {
            var vp = new VotingPlace();
            vp.SetCounter( "1", 1);
            Assert.AreEqual( 1, vp.Counter( "1" ) );
            vp.SetCounter( "1", 1000);
            Assert.AreEqual(1000, vp.Counter("1"));
            }

        [Test]
        public void ShouldBeAbleToCalculateCountersFromCountersInChildren()
            {
            var election = new Election();

            var vp = new VotingPlace() { Election = election };
            vp.Children.AddAll( new []
                                    {
                                    new VotingPlace().SetCounter( "1", 1 ).SetCounter( "2", 2 ),
                                    new VotingPlace().SetCounter( "1", 100 ).SetCounter( "2", 200 )
                                    } );
            vp.UpdateCountersFromChildren();
            Assert.AreEqual( 101, vp.Counter( "1" ) );
            Assert.AreEqual(202, vp.Counter( "2") );
            }

//        [Test]
//        public void ShouldBeAbleToPromoteCountersAInUIKtoCounterDinParent
//            {
//            
//            }

        [Test]
        public void ShouldUpdateNumberOfErrorsInParentIfOwnNumberOfErrorsChanges()
            {
            var vp1 = new VotingPlace();
            var vp2 = new VotingPlace();
            vp1.Children.Add( vp1 );
            vp2.Parent = vp1;

            Assert.AreEqual( 0, vp1.NumberOfErrors );
            vp2.NumberOfErrors = 1;
            Assert.AreEqual( 1, vp1.NumberOfErrors);
            vp2.NumberOfErrors = 0;
            Assert.AreEqual( 0, vp1.NumberOfErrors);
            }

        [Test]
        public void ShouldBeAbleToDeterminePlaceType()
            {
            var cik = new VotingPlace();
            var vologodskayaObl = new VotingPlace() { Name = "Вологодская область", Parent = cik }.SetCounter( "1", 1 );
            var uik404 = new VotingPlace() { Name = "УИК №404", Parent = vologodskayaObl };
            cik.Children.Add(vologodskayaObl);
            Assert.AreEqual( Type.CIK, cik.Type );
            Assert.AreEqual( Type.RIK, vologodskayaObl.Type );
            Assert.AreEqual( Type.UIK, uik404.Type );
            }

        }
    }