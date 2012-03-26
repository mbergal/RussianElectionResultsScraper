using System;
using System.Net;
using System.Web;
using RussianElectionResultsScraper.Commons;

namespace RussianElectionResultsScraper.BulkCopy
    {
    public class SendDataDestinationFactory
        {
        public SendDataDestinationFactory()
            {
            }

        public ISendDataDestination Create( string connectionStringOrUrl )
            {
            if ( !connectionStringOrUrl.StartsWith( "http:" ) )
                return new SqlServerDatabaseDataDestination( connectionStringOrUrl );
            else
                return new WebServiceDataDestination( new Uri( connectionStringOrUrl.AppendIfNotThere( "/" ) ) );
            }
        }

    public class WebServiceDataDestination : ISendDataDestination
        {
        private readonly Uri _url;

        public WebServiceDataDestination( Uri url )
            {
            this._url = url;
            }

        public void BeginReceiving()
            {
            var req = WebRequest.Create( new Uri( this._url, "BeginReceiving" ).ToString() );
            req.Method = "POST";
            req.ContentLength = 0;
            using ( var response = (HttpWebResponse) req.GetResponse() )
                if ( response.StatusCode != HttpStatusCode.OK )
                    throw new HttpException( (int) response.StatusCode, response.StatusDescription );
            }

        public void ReceiveBlock( byte[] block )
            {
            var req = WebRequest.Create( new Uri( this._url, "ReceiveBlock" ).ToString() );
            req.Method = "POST";
            req.ContentLength = block.Length;
            using ( var stream = req.GetRequestStream() )
                stream.Write(  block, 0, block.Length );
            using ( var response = (HttpWebResponse) req.GetResponse() )
                if ( response.StatusCode != HttpStatusCode.OK )
                    throw new HttpException( (int) response.StatusCode, response.StatusDescription );
            }

        public void EndReceiving()
            {
            var req = WebRequest.Create( new Uri( this._url, "EndReceiving" ).ToString() );
            req.Method = "POST";
            req.ContentLength = 0;
            using ( var response = (HttpWebResponse) req.GetResponse() )
                if ( response.StatusCode != HttpStatusCode.OK )
                    throw new HttpException( (int) response.StatusCode, response.StatusDescription );
            }
        }
    }
