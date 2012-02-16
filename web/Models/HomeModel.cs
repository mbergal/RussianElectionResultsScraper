using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RussianElectionResultsScraper.Model;

namespace MvcApplication2.Models
{
    public class VotingPlaceBreadCrumb
        {
        public string Id;
        public string Name;
        }
    public class HomeModel
        {
        private readonly IEnumerable<VotingPlace> _regions;
        private readonly VotingPlace _currentRegion;

        public HomeModel( VotingPlace currentRegion, IEnumerable<VotingPlace> children )
            {
            _currentRegion = currentRegion;
            _regions = children.OrderBy( x=>x.Name );
            }

        public IList<VotingPlaceBreadCrumb>    Breadcrumbs
            {
            get {
                return MakeBreadcrumbs(this._currentRegion).ToList();
                }
            }

        private static IEnumerable<VotingPlaceBreadCrumb>    MakeBreadcrumbs( VotingPlace vp )
            {
            return (vp.Parent != null ? MakeBreadcrumbs(vp.Parent) : new List<VotingPlaceBreadCrumb>()).Concat(new[] { new VotingPlaceBreadCrumb { Id = vp.Id, Name = vp.Name } });
            }

        public VotingPlace currentRegion
            {
            get { return _currentRegion; }
            }
        public IEnumerable<VotingPlaceLine> regions
            {
            get { return _regions.Select( x=>new VotingPlaceLine( x ) ); }
            }
        }

    public class VotingPlaceLine
        {
        private readonly VotingPlace _votingPlace;

        public VotingPlaceLine( VotingPlace votingPlace )
            {
            this._votingPlace = votingPlace;
            }

        public string Name
            {
            get { return this._votingPlace.Name; }
            }

        public string Id
            {
            get { return this._votingPlace.Id; }
            }

        public string Attendance
            {
            get
                {
                return this._votingPlace.Attendance.HasValue ? Math.Round( this._votingPlace.Attendance.Value, 2 ).ToString() + "%" : "";
                }
            }
        }
}
