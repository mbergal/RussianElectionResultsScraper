using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using log4net;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using RussianElectionResultsScraper.Commons;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.src.utils;

namespace RussianElectionResultsScraper.Commands
    {
    public class ScrapeCommand : BaseCommand
        {
        private static readonly ILog log = LogManager.GetLogger( "ScrapeParser" );
        private static readonly ILog errorLog = LogManager.GetLogger("ScrapeParser.Errors");
        private readonly string _root;
        private readonly bool   _recursive = false;
        private readonly int    _maxworkers = 6;
        private readonly bool   _useCache = false;
        private ISessionFactory   _pageCacheSessionFactory;

        public ScrapeCommand( string connectionString, string root, string configFile, bool recursive, bool useCache, int maxWorkers )
            : base( connectionString: connectionString, configFile: configFile )
            {
            this._root = root;
            this._recursive = recursive;
            this._useCache = useCache;
            this._maxworkers = maxWorkers;
            }

        private ISessionFactory ConfigurePageCacheDatabase( string cacheName )
            {
            return this.ConfigureDatabase( cacheName + ".sdf", typeof( CachedPage ).Assembly, recreate: false );
            }

        protected ISessionFactory     ConfigureDatabase( string path, Assembly containingAssembly, bool recreate )
            {
            string connString = string.Format( "Data Source='{0}';Max Database Size=4000;", path );
            bool newDatabase = false;
            if ( !File.Exists(path) )
                {
                var engine = new SqlCeEngine(connString);
                engine.CreateDatabase();
                newDatabase = true;
                }

            var configuration = Fluently.Configure()
                .Database(MsSqlCeConfiguration
                        .Standard
                        .Driver<FixedSqlServerCeDriver>()
                        .IsolationLevel( IsolationLevel.Snapshot )
                        .ConnectionString( connString ) )
                .BuildConfiguration();

            configuration.AddAssembly( containingAssembly );

            if (newDatabase || recreate)
                {
                var lines = new List<string>();

                var schemaExport = new SchemaExport(configuration);
                schemaExport.Create(lines.Add, true);
                schemaExport.Execute(true, false, false);
                }
            return configuration.BuildSessionFactory();
            }

        
        public override int Execute()
            {
            return log.Info( string.Format( "Scraping {0}", this._root ), () =>
                {
                var configuration = LoadConfiguration();

                ServicePointManager.DefaultConnectionLimit = 6;

                var election = SaveElection(configuration);
                this._pageCacheSessionFactory = this.ConfigurePageCacheDatabase( election.Id );

                var pageCache = this._useCache
                    ? (IPageCache)new PageCache(this._pageCacheSessionFactory)
                    : (IPageCache)new NullPageCache();

                var queueService = new WorkQueueService();
                var pageParser = new PageParser(pageCache);

                var wp = new WorkQueueProcessor(election, queueService, pageParser, _electionResultsSessionFactory, pageCache, configuration, this._maxworkers);
                ResultPage rp = pageParser.ParsePage(this._root, null, null).Result;
                var parentFullPath = string.Join(" > ", rp.Hierarchy.Take(rp.Hierarchy.Count() - 1));
                VotingPlace parentVotingPlace;

                using (var session = _electionResultsSessionFactory.OpenSession())
                    parentVotingPlace = session.Query<VotingPlace>().FirstOrDefault(x => x.Election.Id == election.Id && x.FullName == parentFullPath);

               queueService.Add(new WorkItem()
                    {
                    Uri = this._root,
                    ParentVotingPlaceId = parentVotingPlace != null ? parentVotingPlace.Id : null,
                    UpdateCounters = true,
                    Recursive = this._recursive
                    });
                Task.Factory.StartNew(wp.Run, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default).Wait();

                log.Info("...done");
                return 0;                                                                       
                });
            
            }
        }
    }
