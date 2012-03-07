using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NHibernate;
using RussianElectionResultsScraper.Model;
using log4net;
using System.Linq;

namespace RussianElectionResultsScraper
    {
    public class ScrapeCommand : BaseConsoleCommand
        {
        private static readonly ILog log = LogManager.GetLogger( "ScrapeParser" );
        private static readonly ILog errorLog = LogManager.GetLogger("ScrapeParser.Errors");
        private string _root = "http://www.vybory.izbirkom.ru/region/region/izbirkom?action=show&root=1&tvd=100100028713304&vrn=100100028713299&region=0&global=1&sub_region=0&prver=0&pronetvd=null&vibid=100100028713304&type=242";
        private string _parentUrl;
        private bool   _recursive = false;
        private int    _maxworkers = 6;

        public ScrapeCommand()
                {
                this.IsCommand("scrape", "Run scraper");
                this.HasOption("u|url:", "<root-url>", url => this._root = url);
                this.HasOption("p|parentUrl:", "<parent-url>", url => this._parentUrl = url);
                this.HasConfigOption();
                this.HasOption("r|recursive", "<config-file>", recursive => this._recursive = recursive != null);
                this.HasOption<int>("m|maxworkers", "<maximum-number-of-workers>", maxworkers => this._maxworkers = maxworkers );
                }

        public override int Run(string[] args)
            {
            base.Run(args);
            log.Info( "Starting..." );
            var configuration = LoadConfiguration();

            ServicePointManager.DefaultConnectionLimit = 6;

            var election = SaveElection(configuration);

            var pageCache = new PageCache( this._pageCacheSessionFactory );
            var queueService = new WorkQueueService();
            var pageParser = new PageParser(pageCache);

            var wp = new WorkQueueProcessor( election, queueService, pageParser, _electionResultsSessionFactory, pageCache, configuration, this._maxworkers );
            string parentVotingPlaceId = this._parentUrl != null ? HttpUtility.ParseQueryString(this._parentUrl)["vibid"] : null;
            queueService.Add(new WorkItem() { Uri = this._root, ParentVotingPlaceId = parentVotingPlaceId, UpdateCounters = true, Recursive = this._recursive });
            Task.Factory.StartNew(wp.Run, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default).Wait();

            return 0;
            }
        }
    }
