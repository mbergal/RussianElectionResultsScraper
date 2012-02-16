using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using log4net;
using System.Linq;

namespace RussianElectionResultsScraper
{
    public class PageParser
        {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private readonly IPageCache _pageCache;

        public PageParser( IPageCache pageCache )
            {
            this._pageCache = pageCache;
            }

        public Task<ResultPage> ParsePage( string uri, string parentVotingPlaceId, IDictionary<int, CounterDescription> counterDescriptions )
            {
            counterDescriptions = counterDescriptions ?? new Dictionary<int, CounterDescription>();
            var w3client = new WebClient();
            var parsePageTask = ReadPage( uri, w3client, _pageCache );
            var s = parsePageTask.ContinueWith<ResultPage>(task =>
                {
                var htmlDoc = new HtmlDocument();
                task.Result.Seek(0, SeekOrigin.Begin);
                htmlDoc.Load(task.Result, Encoding.GetEncoding(1251));
                var sw = new StringWriter();
                htmlDoc.Save(sw );
                var vibid = HttpUtility.ParseQueryString(uri)["vibid"];
                var page = new ResultPage(vibid, uri, new PageDocument(htmlDoc));
                    
                return page;
                }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.AttachedToParent, TaskScheduler.Current) ;
            return s;
            }

        private Task<Stream>        ReadPage( string address, WebClient webClient, IPageCache pageCache )
            {
            return log.Info(string.Format("ReadPage: {0}", address), () =>
                {
                var cachedPage = pageCache.Get(address.ToString());
                if (cachedPage != null)
                    {
                    log.Info( "It is cached." );
                    return Task.Factory.StartNew<Stream>(() =>
                        {
                        log.Info( "Read cached page, size " + cachedPage.Length );
                        if (cachedPage.Length == 0 )
                            System.Diagnostics.Debugger.Break();
                        return cachedPage;
                        }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default );
                    }
                else
                    {
                    log.Info("We have to retrieve it.");
                    return OpenReadTask(webClient, new Uri(address))                     
                            .ContinueWith<Stream>(x =>
                                {
                                    var m = new MemoryStream();
                                    x.Result.CopyTo(m);
                                    m.Seek(0, SeekOrigin.Begin);
                                    pageCache.Put(address, m);
                                    m.Seek(0, SeekOrigin.Begin);
                                    log.Info("Retrieved page, size " + m.Length );
                                    return (Stream)m;
                                }, CancellationToken.None, TaskContinuationOptions.AttachedToParent, TaskScheduler.Default );
                    }
                });
            }

        public static Task<Stream> OpenReadTask( WebClient webClient, Uri address)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<Stream>(address);

            // Setup the callback event handler
            OpenReadCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, () => e.Result, () => webClient.OpenReadCompleted -= handler);
            webClient.OpenReadCompleted += handler;

            // Start the async work
            try
            {
                webClient.OpenReadAsync(address, tcs);
            }
            catch (Exception exc)
            {
                // If something goes wrong kicking off the async work,
                // unregister the callback and cancel the created task
                webClient.OpenReadCompleted -= handler;
                tcs.TrySetException(exc);
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }

        internal static void HandleCompletion<T>(
            TaskCompletionSource<T> tcs, AsyncCompletedEventArgs e, Func<T> getResult, Action unregisterHandler)
        {
            // Transfers the results from the AsyncCompletedEventArgs and getResult() to the
            // TaskCompletionSource, but only AsyncCompletedEventArg's UserState matches the TCS
            // (this check is important if the same WebClient is used for multiple, asynchronous
            // operations concurrently).  Also unregisters the handler to avoid a leak.
            if (e.UserState == tcs)
            {
                if (e.Cancelled) tcs.TrySetCanceled();
                else if (e.Error != null) tcs.TrySetException(e.Error);
                else tcs.TrySetResult(getResult());
                unregisterHandler();
            }
        }

        }

    public class ResultPage
        {
        public ResultPage(string vibid, string uri, PageDocument doc)
        {
            this._doc = doc;
            this._uri = uri;
            this.Id = vibid;
            this.Children = doc.Children;
            this.IsRedirect = doc.IsRedirect;
            this.RedirectsTo = doc.ResultsRedirect;

            if (!doc.IsRedirect)
            {
                var resultsTable = doc.ResultTable;
                this.CounterDescriptions = new Dictionary<int, CounterDescription>();
                this.CounterValues = new Dictionary<int, int>();

                foreach (var row in resultsTable)
                {
                    if (row.Count > 0)
                    {
                        int counterId = 0;
                        int.TryParse(row[0], out counterId);
                        if (counterId > 0)
                        {
                            var counterName = row[1];
                            int counterValue;
                            int.TryParse(row[2], out counterValue);
                            this.CounterValues.Add(counterId, counterValue);
                            CounterDescriptions.Add(counterId, new CounterDescription() { counterSource = uri, counterName = counterName });
                        }
                    }
                }
                this.CounterDescriptions = CounterDescriptions;
            }

            
        }

        public string                               Id { get; set; }
        public string                               Uri { get { return _uri; } }
        public string                               Name 
            { 
            get
                {
                return this._doc.Name;
                }
            }
        public string                               FullName
            {
            get
                {
                return this._doc.FullName;
                }
            }

        public IList<Tuple<string, string>>         Children { get; set; }
        public IDictionary<int, int>                CounterValues;
        public IDictionary<int, CounterDescription> CounterDescriptions { get; set; }
        public IList<string>                        Hierarchy
            {
            get {
                var h = this._doc.History.Select(x => x.text).ToList();
                if ( h[0] != "ЦИК России" )
                    h.Insert(0, "ЦИК России" );
                return h;    
                }
            }

        public bool                                 IsRedirect
            {
            get;
            set;
            }
        public string                               RedirectsTo
            {
            get;
            set;
            }

        public string                               PageText
            {
            get {
                return this._doc.PageText;
                }
            }
        private readonly string                     _uri;
        private PageDocument                        _doc;
        };
}