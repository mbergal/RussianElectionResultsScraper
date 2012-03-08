using NUnit.Framework;
using RussianElectionResultsScraper.Model;

namespace TestElectionResultsScraper
{
    [TestFixture]
    public class TestVotingResult
        {
        [Test]
        public void ShouldBeAbleSetMessage()
            {
            var vr = new VotingResult();
            vr.Message = "!!!!";
            Assert.AreEqual( "!!!!", vr.Message );
            }
        [Test]
        public void ShouldBeAbleToIncreaseParentNumberOfErrorsWhenSettingNonEmptyMessage()
            {
            var vp = new VotingPlace();
            var vr = new VotingResult();
            vp.Results.Add(vr);
            vr.VotingPlace = vp;
            Assert.AreEqual( 0, vp.NumberOfErrors );

            vr.Message = "!";
            Assert.AreEqual(1, vp.NumberOfErrors);
            vr.Message = "";
            Assert.AreEqual(0, vp.NumberOfErrors);
            }

        }
    }