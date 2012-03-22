using System;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using System.Linq;
using log4net;

namespace RussianElectionResultsScraper.Model
    {
    public enum Type { CIK = 1, Summary = 2, RIK = 3, TIK = 4, OIK = 5, UIK = 6 };

    public class CandidateResult
        {
        public int      Votes;
        public decimal  Percents;
        }
    public class VotingPlace 
        {
        private static readonly ILog log = LogManager.GetLogger( "VotingPlace" );
        private int _numberOfErrors;

        public VotingPlace()
            {
            Results = new HashedSet<VotingResult>();
            Children  = new HashedSet<VotingPlace>();
            }

        public virtual string       Id { get; set; }
        public virtual string       Name { get; set; }
        public virtual string       FullName { get; set; }
        public virtual string       Uri { get; set; }
        public virtual Type         Type
            {
            get {
                if ( this.Parent != null )
                    {
                    if (this.Name.StartsWith("УИК"))
                        return Type.UIK;
                    if ( this.Results.All( x => x.IsCalculated ) )
                        return Type.Summary;
                    else
                        {
                        var parentType = this.Parent.Type != Type.Summary ? this.Parent.Type : this.Parent.Parent.Type;

                        switch (parentType)
                            {
                            case Type.CIK: return Type.RIK;
                            case Type.RIK: return Type.TIK;
                            case Type.TIK: return Type.OIK;
                            case Type.OIK: return Type.UIK;
                            case Type.UIK:
                                throw new Exception("UIK cannot be a parent of anything");
                            default:
                                throw new Exception("Unknown type of parent");
                            }
                        }
                    }
                else
                    return Type.CIK;
                }
            set {}
            }
        public virtual int          NumberOfErrors 
            { 
            get {
                return this._numberOfErrors;
                }
            set
                {
                if (this.Parent != null)
                    { 
                    this.Parent.NumberOfErrors -= this._numberOfErrors;
                    this.Parent.NumberOfErrors += value;
                    }
                this._numberOfErrors = value;
                }
            }
        public virtual string       Path
            {
            get { return Parent != null ? Parent.Path + Parent.Id + ":" : "";  }
            set { }
            }
        public virtual Election           Election { get; set; }
        public virtual VotingPlace        Parent { get; set; }
        public virtual Iesi.Collections.Generic.ISet<VotingPlace>  Children { get; set; }
        public virtual Iesi.Collections.Generic.ISet<VotingResult> Results { get; set; }

        public virtual int?               Counter(string i)
            {
            var counter = this.Results.FirstOrDefault(x => x.Counter == i);
            return counter != null ? counter.Value : (int?) null;
            }

        public virtual void                 RemoveCounter( string counter )
            {
            var result = this.Results.FirstOrDefault(x => x.Counter == counter);
            this.Results.Remove(result);
            }

       public virtual VotingPlace          SetCounter( string counter, int value, string message = null, bool isCalculated = false )
            {
            var votingResult = this.Results.FirstOrDefault(x => x.Counter == counter );
            if (votingResult != null)
                {
                votingResult.Value = value;
                votingResult.Message = message;
                votingResult.IsCalculated = isCalculated;
                }
            else
                this.Results.Add(new VotingResult() { VotingPlace = this, Counter = counter, Value = value, Message = message, IsCalculated = isCalculated } );
            return this;
            }

        public virtual IEnumerable<CandidateResult> CandidateResults
            {
            get
                {
                return this.Results
                    .Where(x => this.Election.Counter( x.Counter ).IsCandidate )
                    .Select(x => new CandidateResult { Votes = x.Value, Percents = (decimal)x.Percents } );
                }
            }

        /// <summary>
        /// Число избирателей, внесенных в список избирателей
        /// </summary>
        public virtual int? NumberOfVotersInVoterList
            {
            get {
                return this.Counter( "1" );
                }
            set {}
            }

        /// <summary>
        /// Число избирательных бюллетеней, полученных участковой избирательной комиссией
        /// </summary>
        public virtual int? NumberOfBallotsReceivedByElectoralComission 
            {
            get {
                return this.Counter( "2" );
                }
            set { }
            }

        /// <summary>
        /// Число избирательных бюллетеней, выданных избирателям, проголосовавшим досрочно
        /// </summary>
        public virtual int? NumberOfBallotsIssuedToVotersWhoVotedEarly 
            {
            get {
                return this.Counter( "3" );
                }
            set { }
            }

        /// <summary>
        /// Число избирательных бюллетеней, выданных избирателям в помещении для голосования
        /// </summary>
        public virtual int? NumberOfBallotsIssuedToVoterAtPollStation 
            {
            get {
                return this.Counter( "4" );
                }
            set { }
            }
        /// <summary>
        /// Число избирательных бюллетеней, выданных избирателям вне помещения для голосования
        /// </summary>
        public virtual int? NumberOfBallotsIssuedToVotersOutsideOfPollStation 
            {
            get {
                return this.Counter( "5" );
                }
            set { }
            }
        /// <summary>
        /// Число погашенных избирательных бюллетеней
        /// </summary>
        public virtual int? NumberOfCancelledBallots  
            {
            get {
                return this.Counter( "6" );
                }
            set { }
            }

        /// <summary>
        /// Число избирательных бюллетеней в переносных ящиках для голосования
        /// </summary>
        public virtual int? NumberOfBallotsInPortableBallotBoxes 
            {
            get {
                return this.Counter( "7" );
                }
            set { }
            }
        /// <summary>
        /// Число избирательных бюллетеней в стационарных ящиках для голосования
        /// </summary>
        public virtual int? NumberOfBallotsInStationaryBallotBoxes 
            {
            get {
                return this.Counter( "8" );
                }
            set { }
            }

        /// <summary>
        /// Число недействительных избирательных бюллетеней
        /// </summary>
        public virtual int? NumberOfInvalidBallots 
            {
            get {
                return this.Counter( "9" );
                }
            set { }
            }

        /// <summary>
        /// Число действительных избирательных бюллетеней
        /// </summary>
        public virtual int? NumberValidBallots 
            {
            get {
                return this.Counter( "10" );
                }
            set { }
            }

        /// <summary>
        /// Число открепительных удостоверений, полученных участковой избирательной комиссией
        /// </summary>
        public virtual int? NumberOfAbsenteePermitsReceivedByElectoralComission 
            {
            get {
                return this.Counter( "11" );
                }
            set { }
            }
        /// <summary>
        /// Число открепительных удостоверений, выданных избирателям на избирательном участке
        /// </summary>
        public virtual int? NumberOfAbsenteePermitsIssuedAtPollingStation 
            {
            get {
                return this.Counter( "12" );
                }
            set { }
            }

        /// <summary>
        /// Число избирателей, проголосовавших по открепительным удостоверениям на избирательном участке
        /// </summary>
        public virtual int? NumberOfAbsenteeBallotsCastAtPollingStation 
            {
            get {
                return this.Counter( "13" );
                }
            set { }
            }

        /// <summary>
        /// Число погашенных неиспользованных открепительных удостоверений
        /// </summary>
        public virtual int? NumberOfCanceledAbsenteePermits 
            {
            get {
                return this.Counter( "14" );
                }
            set { }
            }

        /// <summary>
        /// Число открепительных удостоверений, выданных избирателям территориальной избирательной комиссией
        /// </summary>
        public virtual int? NumberOfAbsenteeBallotsIssuedByTerritorialElectionComission 
            {
            get {
                return this.Counter( "15" );
                }
            set { }
            }

        /// <summary>
        /// Число утраченных открепительных удостоверений
        ///  </summary>
        public virtual int? NumberOfLostAbsenteeBallots 
            {
            get
                {
                return this.Counter( "16" );
                }
            set { }
            }

        /// <summary>
        /// Число утраченных избирательных бюллетеней
        /// </summary>
        public virtual int? NumberOfLostBallots 
            {
            get
                {
                return this.Counter( "17" );
                }
            set { }
            }
        /// <summary>
        /// Число избирательных бюллетеней, не учтенных при получении
        /// </summary>
        public virtual int? NumberOfBallotsNotCounterUponReceiving  
            {
            get
                {
                return this.Counter( "18" );
                }
            set { }
            }

        public virtual double? Attendance
            {
            get {
                try
                    {
                    return this.NumberOfVotersInVoterList != 0 
                        ? (double)(100.00 * (this.NumberOfBallotsIssuedToVotersWhoVotedEarly + this.NumberOfBallotsIssuedToVoterAtPollStation + this.NumberOfBallotsIssuedToVotersOutsideOfPollStation ) / this.NumberOfVotersInVoterList)
                        : 0.0;
                    }
                catch (Exception e)
                    {
                    log.Warn( string.Format( "Error calculating attendance for \"{0}/{1}\"", this.Election.Id, this.Id ), e );
                    return 0;
                    }
                }
            set { }
            }

        public virtual int     NumberOfIssuedBallots
            {
            get {
                try
                    {
                    return (int) (this.NumberOfBallotsIssuedToVotersWhoVotedEarly + this.NumberOfBallotsIssuedToVoterAtPollStation + this.NumberOfBallotsIssuedToVotersOutsideOfPollStation);
                    }
                catch (Exception)
                    {
                    return 0;
                    }
                }
            }

        // TODO
        // Needs test

        public virtual void UpdateCountersFromChildren( )
            {
            this.Results.RemoveAll(this.Results.Where(x => x.IsCalculated).ToList() );
            var counterList = this.Children.SelectMany( c => c.Results.Select(x => x.Counter)).Distinct().ToList();
            counterList.ForEach( x => this.SetCounter( 
                                            counter:  x, 
                                            value:    (int) this.Children.Select( y=>y.Counter(x)).Sum(), 
                isCalculated: true
                ));
            }

        public virtual void Check()
            {
            foreach( var c in this.Children )
                c.Check();
            }

        }
    }