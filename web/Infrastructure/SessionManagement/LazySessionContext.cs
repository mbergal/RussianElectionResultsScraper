using System;
using System.Collections.Generic;
using System.Web;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;

namespace RussianElectionResultScraper.Web.Infrastructure.SessionManagement
{
	//Is up to you to:
	//-set the currentsessioncontextclass in nhibernate as follows:
	//		configuration.Properties[Environment.CurrentSessionContextClass]
	//      	= typeof (LazySessionContext).AssemblyQualifiedName;
	//-implement ISessionFactoryProvider or use Castle Typed Factories:
	//		container.Register(Component.For<ISessionFactoryProvider>().AsFactory());
	//-load the SessionFactoryProvider in the HttpApplication as follows:
	//		HttpContext.Current.Application[SessionFactoryProvider.Key]
	//				= your instance of ISessionFactoryProvider;
	//-inject ISessionFactory in Daos and use GetCurrentSessionContext()
	
	
	public class LazySessionContext : ICurrentSessionContext
	{
		private readonly ISessionFactoryImplementor factory;
		private const string CurrentSessionContextKey = "NHibernateCurrentSession";

		public LazySessionContext(ISessionFactoryImplementor factory)
		{
			this.factory = factory;
		}

		/// <summary>
		/// Retrieve the current session for the session factory.
		/// </summary>
		/// <returns></returns>
		public ISession CurrentSession()
		{
			Lazy<ISession> initializer;
			var currentSessionFactoryMap = GetCurrentFactoryMap();
			if(currentSessionFactoryMap == null || 
				!currentSessionFactoryMap.TryGetValue(factory, out initializer))
			{
				return null;
			}
			return initializer.Value;
		}

		/// <summary>
		/// Bind a new sessionInitializer to the context of the sessionFactory.
		/// </summary>
		/// <param name="sessionInitializer"></param>
		/// <param name="sessionFactory"></param>
		public static void Bind(Lazy<ISession> sessionInitializer, ISessionFactory sessionFactory)
		{
			var map = GetCurrentFactoryMap();
			map[sessionFactory] = sessionInitializer;
		}

		/// <summary>
		/// Unbind the current session of the session factory.
		/// </summary>
		/// <param name="sessionFactory"></param>
		/// <returns></returns>
		public static ISession UnBind(ISessionFactory sessionFactory)
		{
			var map = GetCurrentFactoryMap();
			var sessionInitializer = map[sessionFactory];
			map[sessionFactory] = null;
			if(sessionInitializer == null || !sessionInitializer.IsValueCreated) return null;
			return sessionInitializer.Value;
		}

		/// <summary>
		/// Provides the CurrentMap of SessionFactories.
		/// If there is no map create/store and return a new one.
		/// </summary>
		/// <returns></returns>
		private static IDictionary<ISessionFactory, Lazy<ISession>> GetCurrentFactoryMap()
		{
			var currentFactoryMap = (IDictionary<ISessionFactory,Lazy<ISession>>)
			                        HttpContext.Current.Items[CurrentSessionContextKey];
			if(currentFactoryMap == null)
			{
				currentFactoryMap = new Dictionary<ISessionFactory, Lazy<ISession>>();
				HttpContext.Current.Items[CurrentSessionContextKey] = currentFactoryMap;
			}
			return currentFactoryMap;
		}
	}
	
	public interface ISessionFactoryProvider
	{
		IEnumerable<ISessionFactory> GetSessionFactories();
	}

	public class SessionFactoryProvider
	{
		public const string Key = "NHibernateSessionFactoryProvider";
	}

	public class NHibernateSessionModule : IHttpModule
	{
		private HttpApplication app;
		
		public void Init(HttpApplication context)
		{
			app = context;
			context.BeginRequest += ContextBeginRequest;
			context.EndRequest += ContextEndRequest;
		}

		private void ContextBeginRequest(object sender, EventArgs e)
		{
			var sfp = (ISessionFactoryProvider)app.Context.Application[SessionFactoryProvider.Key];
			foreach (var sf in sfp.GetSessionFactories())
			{
				var localFactory = sf;
				LazySessionContext.Bind(
					new Lazy<ISession>(() => BeginSession(localFactory)), 
					sf);
			}
		}

		private static ISession BeginSession(ISessionFactory sf)
		{
			var session = sf.OpenSession();
			return session;
		}

		private void ContextEndRequest(object sender, EventArgs e)
		{
			var sfp = (ISessionFactoryProvider)app.Context.Application[SessionFactoryProvider.Key];
			foreach (var sf in sfp.GetSessionFactories())
			{
				var session = LazySessionContext.UnBind(sf);
				if (session == null) continue;
				EndSession(session);
			}
		}

		private static void EndSession(ISession session)
		{
			if(session.Transaction != null && session.Transaction.IsActive)
			{
				session.Transaction.Commit();
			}
			session.Dispose();
		}

		public void Dispose()
		{
			app.BeginRequest -= ContextBeginRequest;
			app.EndRequest -= ContextEndRequest;
		}
	}
}