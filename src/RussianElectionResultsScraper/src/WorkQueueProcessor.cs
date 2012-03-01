using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using RussianElectionResultsScraper.Model;
using RussianElectionResultsScraper.src;
using log4net;

namespace RussianElectionResultsScraper
{

    public class WorkQueueProcessor
       {
        private readonly WorkQueueService _workQueueService;
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private readonly PageParser _pageParser;
        private readonly ISessionFactory _sessionFactory;
        private readonly IPageCache      _pageCache;
        private int _numOfVotingPlaces;
        private int _numOfVotingResults;
        private readonly Election _election;

        public WorkQueueProcessor( Election election, WorkQueueService workQueueService, PageParser pageParser, ISessionFactory sessionFactoryFactory, IPageCache pageCache, ElectionConfig electionConfig)
            {
            this._workQueueService = workQueueService;
            this._pageParser = pageParser;
            this._sessionFactory = sessionFactoryFactory;
            this._pageCache = pageCache;
            this._election = election;
            this._numOfVotingPlaces = 0;
            this._numOfVotingResults = 0;
            }

        public void Run()
            {
            RunInternal();
            }

        void  Watch()
            {
            new Task( delegate
                {
                using (ISession session = this._sessionFactory.OpenSession())
                    {
                    Console.WriteLine("Queue: {0}", this._workQueueService.Count());
                    Console.WriteLine("Voting Places:  {0}", this._numOfVotingPlaces );
                    Console.WriteLine("Voting Results: {0}", this._numOfVotingResults );
                    Console.WriteLine();
                    TaskEx.Delay( 5000, CancellationToken.None ).ContinueWith( t => this.Watch() );
                    }
                } ).Start();
            }


        void RunInternal()
            {
            this.Watch();
            Parallel.ForEach( this._workQueueService.GetConsumerPartitioner(), new ParallelOptions() { MaxDegreeOfParallelism = 6 }, this.ProcessWorkItem );
            }

        void ProcessWorkItem(WorkItem workItem)
            {
            new ProcessWorkItemCommand( _election, workItem, _pageParser, _sessionFactory, _pageCache, _workQueueService ).Execute();
            }

    }


}