namespace RussianElectionResultsScraper.BulkCopy
    {
    public interface ISendDataDestination
        {
        void BeginReceiving();

        void ReceiveBlock( byte[] block );

        void EndReceiving();
        }
    }
