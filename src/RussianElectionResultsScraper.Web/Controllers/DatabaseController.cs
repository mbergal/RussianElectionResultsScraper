using System.IO;
using System.Web.Mvc;
using log4net;
using NHibernate;
using RussianElectionResultsScraper.BulkCopy;
using RussianElectionResultsScraper.Commons;


namespace RussianElectionResultScraper.Web.Controllers
{
    public partial class DatabaseController : Controller
        {
        private static ILog log = LogManager.GetLogger( "DatabaseController" );

        private readonly ISessionFactory _sessionFactory;
        private SqlServerDatabaseDataDestination _databaseDestination;

        public DatabaseController( ISessionFactory sessionFactory )
            {
            this._sessionFactory = sessionFactory;
            this._databaseDestination = new SqlServerDatabaseDataDestination( this._sessionFactory.GetCurrentSession().Connection.ConnectionString );
            }

        [HttpPost]
        public virtual ActionResult BeginReceiving()
            {
            log.Info( "BeginReceiving", () =>
                {
                this._databaseDestination.BeginReceiving();                                
                } );

            
            return this.Content( string.Empty );
            }

        [HttpPost]
        public virtual ActionResult EndReceiving()
            {
            log.Info( "EndReceiving", () =>
                                          {
                                          this._databaseDestination.EndReceiving();                                
                                          } );
            return this.Content( string.Empty );
            }

        [HttpPost]
        public virtual ActionResult ReceiveBlock()
            {
            log.Info( string.Format( "ReceiveBlock {0}", Request.InputStream.Length ), () =>
                                                                                           {
                                                                                           byte[] block  = new byte[Request.InputStream.Length];
                                                                                           Request.InputStream.Read(
                                                                                               block, 0, block.Length );
                                                                                           this._databaseDestination.ReceiveBlock( block );
                                                                                           log.Info( "...done" );
                                                                                           } );
            return this.Content( string.Empty );
            }
        }
}
