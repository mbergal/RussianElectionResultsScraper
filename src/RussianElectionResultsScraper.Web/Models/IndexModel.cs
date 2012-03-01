using System.Collections.Generic;
using RussianElectionResultsScraper.Model;

namespace MvcApplication2.Models
{
    public class IndexModel
        {
        private readonly IEnumerable<Election> _elections;

        public IndexModel( IEnumerable<Election> elections )
            {
            this._elections = elections;
            }

        public IEnumerable<Election>    Elections
            {
            get { return this._elections;  }
            }

        }
}