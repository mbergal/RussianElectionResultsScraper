using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NHibernate;
using NHibernate.ByteCode.Castle;
using NHibernate.Cfg;
using NHibernate.Cfg.Loquacious;
using NHibernate.Dialect;
using RussianElectionResultScraper.Web.Infrastructure.SessionManagement;
using RussianElectionResultsScraper.Model;
using log4net;
using log4net.Config;

namespace MvcApplication2
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static ILog logger = LogManager.GetLogger(typeof(MvcApplication));
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            XmlConfigurator.Configure();
            logger.Info( "!!!!!" );
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
            {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Graph/Polling Stations By Attendance",
                "graph/polling-stations-by-attendance/{region}.jpg",
                new { controller = "Graph", action = "PollingStationsByAttendance", region = "" }
                );

            routes.MapRoute(
                "Graph/Polling Station Results",
                "graph/polling-station-results/{votingPlaceId}.jpg",
                new { controller = "Graph", action = "PollingStationResults", region = "" }
                );

            routes.MapRoute(
                "Regions",
                "regions",
                new { controller = "Home", action = "Regions", region = "" }
                );


            routes.MapRoute(
                "Default", // Route name
                "{votingPlaceId}", // URL with parameters
                new { controller = "Home", action = "Index", votingPlaceId = "" } // Parameter defaults
                );
            }

        protected void Application_Start()
            {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            }
    }

    public class NHibernateInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
            {
            container.AddFacility<TypedFactoryFacility>();

            container.Register(Component.For<ISessionFactory>().UsingFactoryMethod(k => BuildSessionFactory()));
            container.Register(Component.For<ISessionFactoryProvider>().AsFactory());
            container.Register(Component.For<IEnumerable<ISessionFactory>>().UsingFactoryMethod( k => k.ResolveAll<ISessionFactory>()) );
            HttpContext.Current.Application[SessionFactoryProvider.Key] = container.Resolve<ISessionFactoryProvider>();
            }

        private ISessionFactory BuildSessionFactory()
            {
            var configuration = new Configuration();
            configuration.DataBaseIntegration(db =>
                {
                db.Dialect<MsSql2008Dialect>();
                db.ConnectionStringName = "Elections";
                });
            configuration.Properties[Environment.CurrentSessionContextClass] = typeof(LazySessionContext).AssemblyQualifiedName;
            configuration.Proxy( p => p.ProxyFactoryFactory<ProxyFactoryFactory>() );
            configuration.AddAssembly( typeof( VotingPlace ).Assembly );
            return configuration.BuildSessionFactory();
            }
    }
}