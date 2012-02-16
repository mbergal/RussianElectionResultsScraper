using System.Data;
using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Engine;

namespace RussianElectionResultsScraper
    {
//    public class ThrottlingConnectionManager : ConnectionManager
//        {
//        public ThrottlingConnectionManager(ISessionImplementor session, IDbConnection suppliedConnection, ConnectionReleaseMode connectionReleaseMode, IInterceptor interceptor) : base(session, suppliedConnection, connectionReleaseMode, interceptor)
//            {
//            }
//        }
//    public class ThrottlingSessionFactory
//        {
//        private ISessionFactory _sessionFactory;
//
//        public ThrottlingSessionFactory( ISessionFactory sessionFactory )
//            {
//            this._sessionFactory = sessionFactory;
//            }
//        NHibernate.ISession OpenSession()
//            {
//            ISession session = _sessionFactory.OpenSession( ).GetSessionImplementation().ConnectionManager.Factory;
//            }
//        }

    }