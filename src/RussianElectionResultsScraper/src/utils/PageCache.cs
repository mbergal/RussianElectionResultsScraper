using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using log4net;

namespace RussianElectionResultsScraper
{
    public class CachedPage 
        {
        public virtual string Id { get; set; }
        public virtual byte[] Content { get; set; }
        };

    public interface IPageCache
        {
        Stream Get(string pageUri);
        void Put(string pageUri, Stream content);
        void Remove(string pageUri);
        }

    internal class PageCache : IPageCache
        {
        private readonly ISessionFactory _sessionFactory;
        private static readonly ILog log = LogManager.GetLogger("PageCache");

        public PageCache(ISessionFactory sessionFactory )
            {
            this._sessionFactory = sessionFactory;
            using ( ISession session = this._sessionFactory.OpenSession() )
                {
                var ids = session.Query<CachedPage>().Select(x => x.Id);
                log.Info( string.Format( "Cache contains : {0} items", ids.Count() ) );
                ids.ForEach(x => log.Info( "    " + x) );
                }

            }

        public Stream Get(string pageUri)
            {
            log.Info( string.Format( "Getting content of {0}", pageUri ) );
            using ( ISession session = this._sessionFactory.OpenSession() )
                {
                var cachedPage = session.Get<CachedPage>(pageUri);
                if (cachedPage != null)
                    {
                    log.Info("    It is in cache");
                    var decompressed = new MemoryStream();
                    using (var compressor = new GZipStream(new MemoryStream(cachedPage.Content), CompressionMode.Decompress))
                        compressor.CopyTo(decompressed);

                    decompressed.Seek(0, SeekOrigin.Begin );
                    if (decompressed.Length == 0)
                        {
                        log.Info( string.Format( "    Decompressed page {0} length is 0 - deleting it from cache", pageUri ) );
                        session.Delete(cachedPage);
                        session.Flush();
                        return null;
                        }
                    return decompressed;
                    }
                else
                    {
                    log.Info( "    It is not in cache");
                    return null;
                    }
                    
                }
            }

        public void Remove( string pageUri )
            {
            using (ISession session = this._sessionFactory.OpenSession())
                {
                log.Info(string.Format("Removing content of {0} from cache", pageUri));
                var cachedPage = session.Get<CachedPage>(pageUri);
                if (cachedPage != null)
                    {
                    using (ITransaction transaction = session.BeginTransaction( IsolationLevel.ReadCommitted ) )
                        {
                        session.Delete(cachedPage);
                        transaction.Commit();
                        }
                    }
                }
            }
        public void Put(string pageUri, Stream content)
            {
            log.Info(string.Format("Putting content of {0} in cache", pageUri));
            if (content.Length == 0)
                System.Diagnostics.Debugger.Break();

            var compressedContent = new MemoryStream();
            using( var compressor = new GZipStream( compressedContent, CompressionMode.Compress ) )
                content.CopyTo( compressor );

            using (ISession session = this._sessionFactory.OpenSession())
                {
                var cachedPage = session.Get<CachedPage>(pageUri);
                if (cachedPage != null)
                    throw new Exception("Page is already in cache");

                byte[] compressedContentArray = compressedContent.ToArray();
                log.Info(string.Format("Saving page '{0}' in cache, size {1}, compressed size {2}", pageUri, content.Length, compressedContentArray ) );
                using (ITransaction transaction = session.BeginTransaction(IsolationLevel.ReadCommitted ))
                    {
                    session.Save(new CachedPage { Id = pageUri, Content = compressedContentArray });
                    transaction.Commit();
                    }
                }
            }
        }

}
