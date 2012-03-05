using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using RussianElectionResultsScraper.Model;
using log4net;
using Type = RussianElectionResultsScraper.Model.Type;
using NHibernate.Linq;

namespace RussianElectionResultsScraper
    {
    public class ProcessWorkItemCommand
        {
        private readonly WorkItem           _workItem;
        private static readonly ILog        log = LogManager.GetLogger("ProcessWorkItemCommand");
        private readonly PageParser         _pageParser;
        private readonly ISessionFactory    _sessionFactory;
        private readonly IPageCache         _pageCache;
        private readonly WorkQueueService   _workQueue;
        private readonly Election           _election;

        public ProcessWorkItemCommand( Election election, WorkItem workItem, PageParser pageParser, ISessionFactory sessionFactory, IPageCache pageCache, WorkQueueService workQueue )
            {
            this._workItem = workItem;
            this._pageParser = pageParser;
            this._sessionFactory = sessionFactory;
            this._pageCache = pageCache;
            this._workQueue = workQueue;
            this._election = election;
            }

        public void Execute()
            {
            log.Info(string.Format("Processing work item \"{0}\"", this._workItem.Uri ), () =>
                {
                    try
                    {
                        var workItemProcessingTask = new Task(delegate
                    {
                    int maxNumberOfAttempts = 10;

                    while (true)
                        {
                        ResultPage result = null;
                        try {
                            var rootTask = new Task(delegate
                                {
                                var pageParserTask = this._pageParser.ParsePage(this._workItem.Uri, this._workItem.ParentVotingPlaceId, null);
                                var block = pageParserTask.ContinueWith(t =>
                                    {
                                    result = t.Result;
                                    if (result.IsRedirect)
                                        ProcessRedirectPage(this._workItem, result );
                                    else
                                        ProcessResultPage(this._workItem, result);
                                    }, CancellationToken.None, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
                                });
                            rootTask.Start();
                            rootTask.Wait();
                            break;
                            }
                        catch (Exception e)
                            {
                            if ( result != null )
                                log.Warn(string.Format("Exception parsing page {0}\n{1}", this._workItem.Uri, result.PageText), e);
                            else
                                log.Warn(string.Format("Exception parsing page {0}\n", this._workItem.Uri), e);

                            
                            this._pageCache.Remove( this._workItem.Uri );
                            if (--maxNumberOfAttempts < 0)
                                {
                                log.Error( string.Format( "Could not process uri \"{0}\"", this._workItem.Uri ), e );
                                break;
                                System.Diagnostics.Debugger.Break();
                                }
                                
                            }
                        }
                    });


                    workItemProcessingTask.Start();
                    workItemProcessingTask.Wait();
                    this._workQueue.Processed(this._workItem);
                }
                catch (Exception e)
                    {
                        this._workQueue.Processed(this._workItem);    
                    }
                
                
                });
            }

        private void ProcessResultPage(WorkItem workItem, ResultPage result)
            {
            log.Info("ProcessResultPage:", () =>
                {
                using( ISession session = this._sessionFactory.OpenSession() )
                using( ITransaction transaction = session.BeginTransaction() )
                    {
                    if (this._workItem.UpdateCounters)
                        {
                        session.Update(this._election);
                        result.CounterDescriptions.ForEach(x =>
                                                            {
                                                            var counterDescription = this._election.Counter( x.Key);
                                                            this._election.SetCounter( counter: x.Key, name: x.Value.counterName);
                                                            });
                        
                        }
                    var vp = session.Get<VotingPlace>(result.Id);
                    if ( vp != null )
                        {
                        if ( vp.Election.Id != this._election.Id )
                            throw new Exception( string.Format( "Id conflict: {0} in {1} and {2}", result.Id, this._election.Id, vp.Election.Id ) );
                        }
                    else 
                        vp = new VotingPlace();
                    vp.Election = this._election;
                    vp.Id = result.Id;
                    vp.Name = result.Name;
                    vp.Uri = result.Uri;
                    vp.Parent = workItem.ParentVotingPlaceId != null
                        ? session.Get<VotingPlace>( workItem.ParentVotingPlaceId )
                        : null;
                    if (vp.Parent != null)
                        vp.Parent.Children.Add(vp);
                    else
                        vp.Election.Root = vp;
                    vp.FullName = result.FullName;
                    switch ( result.Hierarchy.Count )
                        {
                        case 1: vp.Type = Type.CIK; break;
                        case 2: vp.Type = Type.Region; break;
                        case 3: vp.Type = Type.TIK; break;
                        case 4: 
                        case 5: 
                            vp.Type = result.Children != null ? Type.OIK : Type.UIK; 
                            break;    
                        }

                    var votingResults = result.CounterValues.Select(entry => new VotingResult() { Counter = entry.Key, VotingPlace = vp, Value = entry.Value }).ToList();
                    vp.Results.Clear();
                    foreach (var votingResult in votingResults)
                        {
                        vp.Results.Add( votingResult );    
                        }
                    
                    session.Save(vp);
                    transaction.Commit();

//                    Interlocked.Increment(ref _numOfVotingPlaces);
//                    Interlocked.Add(ref _numOfVotingResults, vp.Results.Count());

                    if (workItem.Recursive && result.Children != null)
                        foreach (var a in result.Children)
                            this._workQueue.Add(new WorkItem() { Uri = a.Item1, ParentVotingPlaceId = vp.Id, Recursive = workItem.Recursive });

                    }
                });
            }

        private void ProcessRedirectPage( WorkItem workItem, ResultPage result )
            {
            log.Info("ProcessRedirectPage: ", () =>
                {
                this._workQueue.Add(new WorkItem() { Uri = result.RedirectsTo, ParentVotingPlaceId = workItem.ParentVotingPlaceId, Recursive = workItem.Recursive } );
                });
            }

    }    

    }
