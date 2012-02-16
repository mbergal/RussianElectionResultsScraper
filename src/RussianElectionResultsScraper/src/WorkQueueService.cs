using System.Collections.Concurrent;
using System.Collections.Generic;
using log4net;

namespace RussianElectionResultsScraper
{

    public class WorkQueueService
        {
        private static readonly ILog log = LogManager.GetLogger(typeof(WorkQueueService));
        private readonly BlockingCollection<WorkItem>   _workItemQueue = new BlockingCollection<WorkItem>();
        private readonly IDictionary<string,WorkItem>   _workItemsByUri = new ConcurrentDictionary<string, WorkItem>();

        public WorkQueueService()
            {
            }

        public void Add( WorkItem workItem )
            {
            _workItemQueue.Add( workItem );
            _workItemsByUri.Add( workItem.Uri, workItem );
            }

        public void Processed( WorkItem workItem )
            {
            _workItemsByUri.Remove(workItem.Uri);
            if ( _workItemsByUri.Count == 0 )
                this._workItemQueue.CompleteAdding();
            }

        public IEnumerable<WorkItem>  GetConsumerEnumerable()
            {
            return _workItemQueue.GetConsumingEnumerable();
            }

        public int Count()
            {
            return _workItemQueue.Count;
            }
        }

}