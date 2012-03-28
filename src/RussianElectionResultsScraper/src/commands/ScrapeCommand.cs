using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NHibernate.Linq;
using RussianElectionResultsScraper.Commons;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.src.utils;

namespace RussianElectionResultsScraper
    {
    public class ScrapeCommand : BaseCommand
        {
        private static readonly ILog log = LogManager.GetLogger( "ScrapeParser" );
        private static readonly ILog errorLog = LogManager.GetLogger("ScrapeParser.Errors");
        private readonly string _root;
        private readonly bool   _recursive = false;
        private readonly int    _maxworkers = 6;
        private readonly bool   _useCache = false;

        public ScrapeCommand( string connectionString, string root, string configFile, bool recursive, bool useCache, int maxWorkers )
            : base( connectionString: connectionString, configFile: configFile )
            {
            this._root = root;
            this._recursive = recursive;
            this._useCache = useCache;
            this._maxworkers = maxWorkers;
            }

        public override int Execute()
            {
            return log.Info( string.Format( "Scraping {0}", this._root ), () =>
                {
                var configuration = LoadConfiguration();

                ServicePointManager.DefaultConnectionLimit = 6;

                var election = SaveElection(configuration);

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
