﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Core.Logging;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using MvcApplication2.Infrastructure;
using NHibernate;
using NHibernate.ByteCode.Castle;
using NHibernate.Cfg.Loquacious;
using NHibernate.Dialect;
using NHibernate.Linq;
using RussianElectionResultScraper.Web.Controllers;
using RussianElectionResultScraper.Web.Infrastructure.SessionManagement;
using RussianElectionResultsScraper.Model;
using log4net;
using log4net.Config;
using Configuration = NHibernate.Cfg.Configuration;
using Environment = NHibernate.Cfg.Environment;
using System.Linq;

namespace MvcApplication2
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private readonly IWindsorContainer container = new WindsorContainer();
        private static readonly ILog logger = LogManager.GetLogger("Application.Global");

        public MvcApplication()
            {
            }
        
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
            {
            XmlConfigurator.Configure();
            filters.Add(new HandleErrorAttribute());
            }

        public static void RegisterRoutes(RouteCollection routes)
            {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{file}.js");
            routes.IgnoreRoute("{file}.ico");

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
                "Graph/Candidate Results By Atendance",
                "graph/candidate-results-by-attendance/{votingPlaceId}.jpg",
                new { controller = "Graph", action = "CandidateResultsByAttendance", region = "" }
                );


            routes.MapRoute(
                "Regions",
                "regions",
                new { controller = "Home", action = "Regions", region = "" }
                );

            routes.MapRoute(
                "Places", // Route name
                "election/{electionId}/place/{votingPlaceId}/{tab}", // URL with parameters
                new { controller = "Home", action = "Place", votingPlaceId = "", tab = "" } // Parameter defaults
                );

            routes.MapRoute(
                "Controller/Action",
                "{controller}/{action}",
                new { controller = "Home", action = "Index" }  // Parameter defaults);
                );

            }

        protected void Application_Start()
            {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            InitializeContainer();

            this.WatchDatabase();
            }

        // http://stackoverflow.com/questions/619895/how-can-i-properly-handle-404-in-asp-net-mvc/620559#620559

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            // Log the exception.

            Response.Clear();

            HttpException httpException = exception as HttpException;

            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");

            if (httpException == null)
            {
                routeData.Values.Add("action", "Index");
            }
            else //It's an Http Exception, Let's handle it.
            {
                switch (httpException.GetHttpCode())
                {
                    case 404:
                        // Page not found.
                        routeData.Values.Add("action", "PageNotFound");
                        break;
                    case 500:
                        // Server error.
                        routeData.Values.Add("action", "HttpError500");
                        break;

                    // Here you can handle Views to other error codes.
                    // I choose a General error template  
                    default:
                        routeData.Values.Add("action", "General");
                        break;
                }
            }

            // Pass exception details to the target error View.
            routeData.Values.Add("error", exception);

            // Clear the error on server.
            Server.ClearError();

            // Avoid IIS7 getting in the middle
            Response.TrySkipIisCustomErrors = true;

            // Call target Controller and pass the routeData.
            IController errorController = new ErrorController();
            errorController.Execute(new RequestContext(
                 new HttpContextWrapper(Context), routeData));
        }
        protected void Applicatoin_End()
            {
            container.Dispose();
            }

        private void InitializeContainer()
            {
            container.Install( FromAssembly.This() );
			var controllerFactory = new WindsorControllerFactory(container.Kernel);
			ControllerBuilder.Current.SetControllerFactory(controllerFactory);
            }

        private void WatchDatabase()
            {
            new Task( () => TaskEx.Delay( 5000, CancellationToken.None).ContinueWith(t =>
                                                                                        {
                                                                                        this.UpdateLastTimestamp();
                                                                                        this.WatchDatabase();
                                                                                        }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Current)).Start();
            }

        private void UpdateLastTimestamp()
            {
            using (var session = container.GetService<ISessionFactory>().OpenSession())
                {
                logger.Info("UpdateLastTimestamp");
                var maxTimestamp = session.Query<Election>().Select(x => x.LastUpdateTimeStamp).Max();
                this.Application["LastUpdateTimeStamp"] = maxTimestamp;
                }
            }

        public override string GetVaryByCustomString(HttpContext context, string arg)
            {
            if (arg == "LastUpdateTimestamp")
                {
                object o = HttpContext.Current.Application["LastUpdateTimeStamp"];
                if (o == null)
                    HttpContext.Current.Application["LastUpdateTimeStamp"] = DateTime.Now;
                return o.ToString();
                }
            return base.GetVaryByCustomString(context, arg);
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
            string connectionString;
            if ( Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.IsAvailable  )
                {
                connectionString = Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue("ConnectionString");
                if ( connectionString == null )
                    throw new Exception("Please specify database connection string in the role instance setting 'Connection String'");
                }
            else
                connectionString = ConfigurationManager.ConnectionStrings[ "Elections"].ConnectionString;

            configuration.DataBaseIntegration(db =>
                {
                db.Dialect<MsSql2008Dialect>();
                db.ConnectionString = connectionString;
                db.ConnectionStringName = "Elections";
                });
            configuration.Properties[Environment.CurrentSessionContextClass] = typeof(LazySessionContext).AssemblyQualifiedName;
            configuration.Proxy( p => p.ProxyFactoryFactory<ProxyFactoryFactory>() );
            configuration.AddAssembly( typeof( VotingPlace ).Assembly );
            return configuration.BuildSessionFactory();
            }
    }
}