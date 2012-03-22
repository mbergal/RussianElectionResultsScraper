using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
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
        private readonly Election _election;
        private readonly int _maxworkers;
        private Progress _progress;

        public WorkQueueProcessor(Election election, WorkQueueService workQueueService, PageParser pageParser, ISessionFactory sessionFactoryFactory, IPageCache pageCache, ElectionConfig electionConfig, int maxworkers )
            {
            this._workQueueService = workQueueService;
            this._pageParser = pageParser;
            this._sessionFactory = sessionFactoryFactory;
            this._pageCache = pageCache;
            this._election = election;
            this._maxworkers = maxworkers;
            this._progress = new Progress();
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
                    Console.WriteLine("Voting Places:  {0}", this._progress.ProcessedVotingPlaces );
                    Console.WriteLine("Voting Results: {0}", this._progress.ProcessedVotingResults );
                    Console.WriteLine();
                    TaskEx.Delay( 5000, CancellationToken.None ).ContinueWith( t => this.Watch() );
                    }
                } ).Start();
            }


        void RunInternal()
            {
            this.Watch();
            Parallel.ForEach( this._workQueueService.GetConsumerPartitioner(), new ParallelOptions() { MaxDegreeOfParallelism = this._maxworkers }, this.ProcessWorkItem );
            }

        void ProcessWorkItem(WorkItem workItem)
            {
            new ProcessWorkItemCommand( 
                    _election, 
                    workItem, 
                    _pageParser, 
                    _sessionFactory, 
                    _pageCache, 
                    _workQueueService, 
                    place => this._progress.Processed( place ) ).Execute();
            }

    }

    public class Progress
        {
        private int _processedVotingPlaces;
        private int _processedVotingResults;

        public Progress()
            {
            }

        public int ProcessedVotingPlaces
            {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return this._processedVotingPlaces;  }
            }

        public int ProcessedVotingResults
            {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return this._processedVotingResults;  }
            }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Processed( VotingPlace vp )
            {
            this._processedVotingPlaces += 1;
            this._processedVotingResults += vp.Results.Count;
            }
        }

}