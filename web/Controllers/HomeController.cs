using System.Web.Mvc;
using System.Linq;
using MvcApplication2.Models;
using NHibernate;
using NHibernate.Linq;
using RussianElectionResultsScraper.Model;

namespace RussianElectionResultScraper.Web
    {
    public class HomeController : Controller
        {
        private ISessionFactory _sessionFactory;

        public HomeController( ISessionFactory sessionFactory )
            {
            this._sessionFactory = sessionFactory;
            }

        public ActionResult  Index( string votingPlaceId )
            {
            VotingPlace main;
            if (string.IsNullOrEmpty(votingPlaceId))
                main = _sessionFactory.GetCurrentSession().Query<VotingPlace>().Where( x => x.Parent == null).FetchMany( x=>x.Children ).ThenFetchMany( x=>x.Results ).ToArray()[0];
            else
                main = _sessionFactory.GetCurrentSession().Query<VotingPlace>().Where(x => x.Id == votingPlaceId).SingleOrDefault();


            var regions = main.Children;

            return View(new HomeModel( main, regions ) );
            }
        }
    }