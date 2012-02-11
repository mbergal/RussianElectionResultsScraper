using System;
using System.IO;
using System.IO.Compression;
using NHibernate;
using ServiceStack.DesignPatterns.Model;
using log4net;

namespace RussianElectionResultsScraper
{
    public class CachedPage : IHasId<string>
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
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public PageCache(ISessionFactory sessionFactory )
            {
            this._sessionFactory = sessionFactory;
            }

        public Stream Get(string pageUri)
            {
            using (ISession session = this._sessionFactory.OpenSession())
                {
                var cachedPage = session.Get<CachedPage>(pageUri);
                if (cachedPage != null)
                    {
                    var decompressed = new MemoryStream();
                    var compressor = new GZipStream(new MemoryStream(cachedPage.Content), CompressionMode.Decompress);
                    compressor.CopyTo(decompressed);
                    decompressed.Seek(0, SeekOrigin.Begin);
                    if (decompressed.Length == 0)
                        {
                        session.Delete(cachedPage);
                        session.Flush();
                        return null;
                        }

                    return decompressed;
                    }
                return null;
                }
            }

        public void Remove( string pageUri )
            {
            using (ISession session = this._sessionFactory.OpenSession())
                {
                var cachedPage = session.Get<CachedPage>(pageUri);
                if (cachedPage != null)
                    { 
                    session.Delete(cachedPage);
                    session.Flush();
                    }
                }
            }
        public void Put(string pageUri, Stream content)
            {
            if (content.Length == 0)
                System.Diagnostics.Debugger.Break();

            var compressedContent = new MemoryStream();
            var compressor = new GZipStream(compressedContent, CompressionMode.Compress);
            content.CopyTo( compressor );

            using (ISession session = this._sessionFactory.OpenSession())
                {
                var cachedPage = session.Get<CachedPage>(pageUri);
                if (cachedPage != null)
                    throw new Exception("Page is already in cache");

                log.Info(string.Format("Saving page '{0}' in cache, size {1}, compressed size {2}", pageUri, content.Length, compressedContent.Length));
                session.Save(new CachedPage { Id = pageUri, Content = compressedContent.ToArray() });
                session.Flush();
                }
            }
        }

}
