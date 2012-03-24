using System;
using NUnit.Framework;
using RussianElectionResultScraper.Web.Infrastructure;
using RussianElectionResultsScraper.Model;

namespace TestElectionResultsScraper
    {
    [TestFixture]
    public class TestMessageParser
        {
        [Test] 
        public void Should_correctly_parse_messages_without_arguments()
            {
            var mp = new MessageParser( s1 => { throw new NotImplementedException(); } );
            var s = mp.ParseMessage( "aaaa" );
            Assert.AreEqual( "aaaa", s.Format );
            Assert.AreEqual( 0, s.Args.Length );
            }
        [Test] 
        public void Should_correctly_parse_messages_with_one_argument()
            {
            var mp = new MessageParser( s1 => new VotingPlace() { Name = "Name 123"} );
            var s = mp.ParseMessage( "aaaa {vp:123}" );
            Assert.AreEqual( "aaaa {0}", s.Format );
            Assert.AreEqual( 1, s.Args.Length );
            Assert.IsInstanceOf( typeof( VotingPlace ), s.Args[0] );
            Assert.AreEqual( "Name 123", ((VotingPlace)s.Args[0]).Name );
            }
        [Test] 
        public void Should_correctly_parse_messages_with_multiple_argument()
            {
            var mp = new MessageParser( s1 => new VotingPlace() {Name = "Name " + s1 } );
            var s = mp.ParseMessage( "aaaa {vp:123} bbbb {vp:456}" );
            Assert.AreEqual( "aaaa {0} bbbb {1}", s.Format );
            Assert.AreEqual( 2, s.Args.Length );
            Assert.IsInstanceOf( typeof( VotingPlace ), s.Args[1] );
            Assert.AreEqual( "Name 456", ((VotingPlace)s.Args[1]).Name );
            }
        }

    }