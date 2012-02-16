using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Iesi.Collections.Generic;
using NDesk.Options;
using NHibernate;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.src;
using log4net;
using System.Linq;

namespace RussianElectionResultsScraper
    {
    public class ScrapeCommand : BaseConsoleCommand
        {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private string _root = "http://www.vybory.izbirkom.ru/region/region/izbirkom?action=show&root=1&tvd=100100028713304&vrn=100100028713299&region=0&global=1&sub_region=0&prver=0&pronetvd=null&vibid=100100028713304&type=242";
        private string _configFile;
        private bool   _recursive = false;

        public ScrapeCommand()
                {
                this.IsCommand("scrape", "Run scraper");
                this.Options = new OptionSet()
                        {
                            { "u|url:", "<root-url>",           url=>this._root = url  },
                            { "c|config=", "<config-file>",     configFile=>this._configFile = configFile  },
                            { "r|recursive", "<config-file>",   recursive=>this._recursive = _recursive }
                        };
                }

        public override int Run(string[] args)
            {
            base.Run(args);
            log.Info( "Starting..." );
            var configuration = Configuration.Load(new XmlTextReader(new StreamReader(this._configFile)));
            configuration.Validate();

            ServicePointManager.DefaultConnectionLimit = 6;

            Election election;
            using (ISession session = this._electionResultsSessionFactory.OpenSession())
            using ( ITransaction transaction = session.BeginTransaction() )
                {
                election = session.Get<Election>(configuration.Id) ?? new Election() { Id = configuration.Id };
                election.Counters = new HashedSet<Model.CounterDescription>( configuration.Counters.Select(x => new Model.CounterDescription() 
                    {
                    Counter = x.Counter,
                    Name = x.Name,
                    ShortName = x.ShortName,
                    Color = x.Color
                    }).ToList());
                session.Save( election );
                transaction.Commit();
                }

            var pageCache = new PageCache( this._pageCacheSessionFactory );
            var queueService = new WorkQueueService();
            var pageParser = new PageParser(pageCache);

            var wp = new WorkQueueProcessor( election, queueService, pageParser, _electionResultsSessionFactory, pageCache, configuration );
            queueService.Add( new WorkItem() { Uri = this._root, ParentVotingPlaceId = null, UpdateCounters = true, Recursive = this._recursive });
            Task.Factory.StartNew(wp.Run, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default).Wait();

            return 0;
            }
        }
    }
