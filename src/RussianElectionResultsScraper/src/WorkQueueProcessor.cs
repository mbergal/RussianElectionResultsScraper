using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using ServiceStack.Text;
using log4net;

namespace RussianElectionResultsScraper
{

    public class WorkQueueProcessor
    {
        private readonly WorkQueueService _workQueueService;
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private readonly PageParser _pageParser;
        private readonly ISessionFactory _sessionFactory;
        private IPageCache               _pageCache;

        public WorkQueueProcessor( WorkQueueService workQueueService, PageParser pageParser, ISessionFactory sessionFactoryFactory, IPageCache pageCache )
            {
            this._workQueueService = workQueueService;
            this._pageParser = pageParser;
            this._sessionFactory = sessionFactoryFactory;
            this._pageCache = pageCache;
            }

        public void Run()
            {
            RunInternal();
            }

        void RunInternal()
            {
            var tasks = new List<Task>();
            foreach( var workItem in this._workQueueService.GetConsumerEnumerable() )
                {
                this.ProcessWorkItem( workItem );
                }
            Task.WaitAll(tasks.ToArray());
            }

        void ProcessWorkItem(WorkItem workItem)
            {
            log.Info(string.Format("Processing work item \"{0}\"", workItem.Uri ), () =>
                {
                var workItemProcessingTask = new Task( delegate
                    {
                    int maxNumberOfAttempts = 10;

                    while ( true )
                        try {
                            var rootTask = new Task(delegate
                                {
                                var pageParserTask = this._pageParser.ParsePage(workItem.Uri, workItem.ParentVotingPlaceId, null );
                                var block = pageParserTask.ContinueWith(t =>
                                    {
                                    var result = t.Result;
                                    if (result.IsRedirect)
                                        ProcessRedirectPage(result, workItem.ParentVotingPlaceId);
                                    else
                                        ProcessResultPage(workItem, result);
                                    }, CancellationToken.None, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
                                });
                            rootTask.Start();
                            rootTask.Wait();
                            break;
                            }
                        catch (Exception e)
                            {
                            log.Error("Exception:", e);
                            this._pageCache.Remove( workItem.Uri );
                            if (--maxNumberOfAttempts < 0)
                                throw;
                            }
                    });

                workItemProcessingTask.Start();
                });
            }

        private void ProcessResultPage(WorkItem workItem, ResultPage result)
            {
            log.Info("ProcessResultPage:", () =>
                {
                using( ISession session = this._sessionFactory.OpenSession() )
                    {
                    var vp = session.Get<VotingPlace>(result.Id) ?? new VotingPlace();
                    vp.Id = result.Id;
                    vp.Name = result.Name;
                    vp.Uri = result.Uri;
                    vp.Parent = workItem.ParentVotingPlaceId != null
                        ? session.Get<VotingPlace>( workItem.ParentVotingPlaceId )
                        : null;
                    vp.FullName = result.FullName;

                    var votingResults = result.CounterValues.Select(entry => new VotingResult() { Counter = entry.Key, VotingPlaceId = vp.Id, Value = entry.Value }).ToList();
                    foreach (var votingResult in votingResults)
                        {
                        vp.Results.Add( votingResult );    
                        }
                    
                    session.Save(vp);
                    session.Flush();

//                    vp = session.Get<VotingPlace>(vp.Id);

                    if (result.Children != null)
                        foreach (var a in result.Children)
                            this._workQueueService.Add(new WorkItem() { Uri = a.Item1, ParentVotingPlaceId = vp.Id });
                    }
                });
            }

        private void ProcessRedirectPage(ResultPage result, string parentVotingPlaceId)
            {
            log.Info("ProcessRedirectPage: ", () =>
                {
                log.Info("ResultPage:\n" + result.Dump());
                this._workQueueService.Add(new WorkItem() { Uri = result.RedirectsTo, ParentVotingPlaceId = parentVotingPlaceId });
                });
            }

    }

}