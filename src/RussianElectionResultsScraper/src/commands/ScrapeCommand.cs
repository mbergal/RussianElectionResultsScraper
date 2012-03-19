using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using NHibernate;
using NHibernate.Linq;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.src.utils;
using log4net;
using System.Linq;

namespace RussianElectionResultsScraper
    {
    public class ScrapeCommand : BaseConsoleCommand
        {
        private static readonly ILog log = LogManager.GetLogger( "ScrapeParser" );
        private static readonly ILog errorLog = LogManager.GetLogger("ScrapeParser.Errors");
        private string _root;
        private bool   _recursive = false;
        private int    _maxworkers = 6;
        private bool   _useCache = false;

        public ScrapeCommand()
                {
                this.IsCommand("scrape", "Run scraper");
                this.HasOption("u|url:", "<root-url>", url => this._root = url);
                this.HasConfigOption();
                this.HasOption("r|recursive", "<config-file>", recursive => this._recursive = recursive != null);
                this.HasOption<int>("m|maxworkers", "<maximum-number-of-workers>", maxworkers => this._maxworkers = maxworkers );
                this.HasOption( "cache", "<use-cache>", useCache => this._useCache = useCache != null );
                }

        public override int Run(string[] args)
            {
            base.Run(args);
            return log.Info(string.Format("Scraping {0}", this._root), () =>
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
