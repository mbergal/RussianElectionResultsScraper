using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg;
using log4net;
using log4net.Config;

namespace RussianElectionResultsScraper
{

    public class CounterDescription
        {
        public string counterName;
        public string counterSource;
        };

    class Program
        {
        private WorkQueueService queueService;
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private PageParser      pageParser;
        private PageCache pageCache;
        private Configuration _configuration;
        private ISessionFactory _sessionFactory;
        private ISession _session;

        static int  Main(string[] args)
            {
            var p = new Program();
            p.Run(args);
            return 0;
            }
        
        public Program()
            {
            }

        public int Run( string[] args )
            {
            DOMConfigurator.Configure();
            _configuration = new Configuration();
            _configuration.Configure();
            _configuration.AddAssembly( typeof(VotingPlace).Assembly);
            _sessionFactory = _configuration.BuildSessionFactory();

            TaskScheduler.UnobservedTaskException += (sender, arg) =>
                {
                log.Error( "Exception: ", arg.Exception );
                arg.SetObserved();
                };
            log.Info( "Starting..." );

            this._session = _sessionFactory.OpenSession();
                {
                this.pageCache = new PageCache( this._sessionFactory );
                this.queueService = new WorkQueueService();
                this.pageParser = new PageParser( this.pageCache );

                string root = "http://www.vybory.izbirkom.ru/region/region/izbirkom?action=show&root=1&tvd=100100028713304&vrn=100100028713299&region=0&global=1&sub_region=0&prver=0&pronetvd=null&vibid=100100028713304&type=242";

                var wp = new WorkQueueProcessor( queueService, pageParser, _sessionFactory, pageCache );
                queueService.Add( new WorkItem() { Uri = root, ParentVotingPlaceId = null } );
                Task.Factory.StartNew(wp.Run, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default).Wait();
                
                }
            return 0;
            }


    }

}
