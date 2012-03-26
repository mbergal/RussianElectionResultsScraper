using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RussianElectionResultScraper.Web.Controllers;
using RussianElectionResultScraper.Web.Infrastructure;
using RussianElectionResultsScraper.Model;
using Type = RussianElectionResultsScraper.Model.Type;

namespace MvcApplication2.Models
{
    public class VotingPlaceBreadCrumb
        {
        public string Id;
        public string Name;
        public string ElectionId;
        };

    public class Candidate
        {
        public string ShortName;
        public string Name;
        public Color Color;
        };

    public class VotingResultModel
        {
        private readonly IEnumerable<VotingPlace> _regions;
        private readonly VotingPlace _currentRegion;
        public VotingResultSummaryModel Summary;
        private readonly HomeController.Tabs _tab;
        private readonly Func<string, VotingPlace> _votingPlaceFactory;

        public VotingResultModel( VotingPlace currentRegion, HomeController.Tabs? tab, Func<string,VotingPlace> votingPlaceFactory  )
            {
            _currentRegion = currentRegion;
            _regions = currentRegion.Children.OrderBy(x => x.Name);
            _votingPlaceFactory = votingPlaceFactory;
            this._tab = tab ?? HomeController.Tabs.summary;
            }

        public string                          ElectionName
            {
            get
                {
                return _currentRegion.Election.Name;
                }
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
            get { return this._currentRegion.Type != Type.UIK && this._tab != HomeController.Tabs.details;  }
            }

        private static IEnumerable<VotingPlaceBreadCrumb>    MakeBreadcrumbs( VotingPlace vp )
            {
            return (vp.Parent != null ? MakeBreadcrumbs(vp.Parent) : new List<VotingPlaceBreadCrumb>()).Concat(new[] { new VotingPlaceBreadCrumb 
                { 
                Id = vp.Id, 
                Name = vp.Name,
                ElectionId = vp.Election.Id
                } });
            }

        public VotingPlaceLine currentRegion
            {
            get { return new VotingPlaceLine( _currentRegion, _votingPlaceFactory ); }
            }

        public IEnumerable<VotingPlaceLine> regions
            {
            get { return _regions.Select( x=>new VotingPlaceLine( x, _votingPlaceFactory ) ); }
            }

        public bool ShowDetails
            {
            get {
                return this._currentRegion.Type == Type.UIK || this._tab == HomeController.Tabs.details;
                }
            }


        public HomeController.Tabs Tab
            {
            get { return this._tab; }
            }

        public bool IsDetailsSelected
            {
            get { return this.Tab == HomeController.Tabs.details; }
            }
        public bool IsSummarySelected
            {
            get { return this.Tab == HomeController.Tabs.summary; }
            }
        }

    public class VotingResultSummaryModel
        {
        }


    public class CounterLine
        {
        public string               Counter;
        public string               Description;
        public string               Value;
        public ParameterizedString  Message;
        }


    public class VotingPlaceLine
        {
        private readonly VotingPlace        _votingPlace;
        private readonly MessageParser      _messageParser;

        public VotingPlaceLine( VotingPlace votingPlace, Func<string,VotingPlace> votingPlaceFactory )
            {
            this._votingPlace = votingPlace;
            this._messageParser = new MessageParser( votingPlaceFactory );
            }

        public string Name
            {
            get { return this._votingPlace.Name; }
            }

        public string ElectionId
            {
            get { return this._votingPlace.Election.Id; }
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

        public bool ShowNumberOfErrors
            {
            get
                {
                return NumberOfErrors > 0;
                }
            }
        public int NumberOfErrors
            {
            get
                {
                return this._votingPlace.NumberOfErrors;
                }
            }
        public IList<CounterLine>  Results
            {
            get {
                return this._votingPlace.Results.ToList().Select(x => new CounterLine
                                                                          {
                                                                          Counter = x.Counter.ToString(), 
                                                                          Value = x.Value.ToString(), 
                                                                          Description = this._votingPlace.Election.Counter(x.Counter).Name,
                                                                          Message = _messageParser.ParseMessage( x.Message )
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

        public string ElectionName
            {
            get { return this._votingPlace.Election.Name; }
            }

        public bool ShowNumberOfErrorsInDetailsTab
            {
            get { return NumberOfErrorsInDetailsTab > 0; }
            }

        public int NumberOfErrorsInDetailsTab
            {
            get
                {
                return this._votingPlace.Results.Count(x => !string.IsNullOrWhiteSpace(x.Message));
                }
            }

        public string Type
            {
                get { 
                    switch( this._votingPlace.Type )
                        {
                        case RussianElectionResultsScraper.Model.Type.CIK: return "";
                        case RussianElectionResultsScraper.Model.Type.Summary: return "избирательная комиссия";
                        case RussianElectionResultsScraper.Model.Type.RIK: return "избирательная комиссия";
                        case RussianElectionResultsScraper.Model.Type.TIK: return "Территориальная избирательная комиссия";
                        case RussianElectionResultsScraper.Model.Type.OIK: return "Территориальная избирательная комиссия";
                        case RussianElectionResultsScraper.Model.Type.UIK: return "Участковая избирательная комиссия";
                        default:
                            throw new Exception( "Unknown VotingPlace.Type");
                        }
                    }
            }
        }
}
