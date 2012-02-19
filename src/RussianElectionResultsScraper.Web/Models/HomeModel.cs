using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RussianElectionResultsScraper.Model;
using Type = RussianElectionResultsScraper.Model.Type;

namespace MvcApplication2.Models
{
    public class VotingPlaceBreadCrumb
        {
        public string Id;
        public string Name;
        };

    public class Candidate
        {
        public string ShortName;
        public string Name;
        public Color Color;
        };

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

        public IEnumerable<Candidate>               Candidates
            {
                get {
                    return this._currentRegion.Election.Candidates.Select( x=> new Candidate() { Color = x.Color, Name = x.Name, ShortName = x.ShortName });
                    }
            }

        public bool                                 ShowBreakdown
            {
            get { return this._currentRegion.Type != Type.UIK;  }
            }

        private static IEnumerable<VotingPlaceBreadCrumb>    MakeBreadcrumbs( VotingPlace vp )
            {
            return (vp.Parent != null ? MakeBreadcrumbs(vp.Parent) : new List<VotingPlaceBreadCrumb>()).Concat(new[] { new VotingPlaceBreadCrumb { Id = vp.Id, Name = vp.Name } });
            }

        public VotingPlaceLine currentRegion
            {
            get { return new VotingPlaceLine( _currentRegion ); }
            }

        public IEnumerable<VotingPlaceLine> regions
            {
            get { return _regions.Select( x=>new VotingPlaceLine( x ) ); }
            }

        public bool ShowDetails
            {
            get {
                return this._currentRegion.Type == Type.UIK;
                }
            }
        }

    public class CounterLine
        {
        public string Counter;
        public string Description;
        public string Value;
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

        public string NumberOfVotersInVoterList
            {
            get
                {
                return string.Format( "{0:N0}", this._votingPlace.NumberOfVotersInVoterList );
                }
            }

        public IList<CounterLine>  Results
            {
            get {
                return this._votingPlace.Results.ToList().Select(x => new CounterLine
                                                                          {
                                                                          Counter = x.Counter.ToString(), 
                                                                          Value = x.Value.ToString(), 
                                                                          Description = this._votingPlace.Election.Counter(x.Counter).Name
                                                                          }).ToList();
                }
            }
        public IEnumerable<CandidateResult> CandidateResults
            {
            get { return this._votingPlace.CandidateResults; }
            }

        public string  CECUrl
            {
            get { return this._votingPlace.Uri;  }
            }
        }
}
