using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcApplication2.Models
{
    public class RegionsModel
        {
        private readonly string[] _regions;
        private readonly string _currentRegion;

        public RegionsModel(string currentRegion, string[] allRegions)
            {
            _currentRegion = currentRegion;
            _regions = allRegions;
            }

        public string currentRegion
            {
            get { return _currentRegion; }
            }
        public string[] regions
            {
            get { return _regions; }
            }
        }
}
