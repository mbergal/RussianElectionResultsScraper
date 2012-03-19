using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RussianElectionResultsScraper.src.utils
{
    public class NullPageCache : IPageCache
        {
        public Stream Get(string pageUri)
            {
            return null;
            }

        public void Put(string pageUri, Stream content)
            {
            }

        public void Remove(string pageUri)
            {
            }
        }
}
