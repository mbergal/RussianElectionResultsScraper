using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web.Mvc;
using System.Linq;
using System.Web.Routing;
using MvcApplication2.Models;
using NHibernate;
using NHibernate.Linq;
using RussianElectionResultsScraper.Model;

namespace RussianElectionResultScraper.Web
    {
    public partial class HomeController : Controller
        {
        public enum  Tabs
            {
            summary = 0,
            details = 1
            }
        private readonly ISessionFactory _sessionFactory;

        public HomeController( ISessionFactory sessionFactory )
            {
            this._sessionFactory = sessionFactory;
            }

        public virtual ActionResult Index()
            {
            return View( new IndexModel( _sessionFactory.GetCurrentSession().Query<Election>() ) );
            }

        public virtual ActionResult Place(string votingPlaceId, Tabs? tab)
            {
            VotingPlace main = string.IsNullOrEmpty(votingPlaceId) 
                ? _sessionFactory.GetCurrentSession().Query<VotingPlace>().Where( x => x.Parent == null).FetchMany( x=>x.Children ).ThenFetchMany( x=>x.Results ).ToArray()[0] 
                : _sessionFactory.GetCurrentSession().Query<VotingPlace>().Where(x => x.Id == votingPlaceId).SingleOrDefault();

        

            return View( new VotingResultModel( main, tab, s1=> this._sessionFactory.GetCurrentSession().Get<VotingPlace>( s1 ) ) );
            }

        public virtual ActionResult Footer()
            {
            return View( new FooterModel() 
                { 
                    WebSiteVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    DatabaseVersion = this._sessionFactory.GetCurrentSession().Query<Election>().Max(x => x.LastUpdateTimeStamp)
                } );
            }
        }

    }