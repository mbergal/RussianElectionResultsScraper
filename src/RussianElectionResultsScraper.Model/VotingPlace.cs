using System;
using Iesi.Collections.Generic;
using System.Linq;

namespace RussianElectionResultsScraper.Model
    {
    public enum Type { CIK = 1, Region = 2, TIK = 3, OIK = 4, UIK = 5 };

    public class VotingPlace 
        {

        public VotingPlace()
            {
            Results = new HashedSet<VotingResult>();
            Children  = new HashedSet<VotingPlace>();
            }
        public virtual string       Id { get; set; }
        public virtual string       Name { get; set; }
        public virtual string       FullName { get; set; }
        public virtual string       Uri { get; set; }
        public virtual Type         Type { get; set; }
        public virtual string       Path
            {
            get { return Parent != null ? Parent.Path + Parent.Id + ":" : "";  }
            set { }
            }
        public virtual Election           Election { get; set; }
        public virtual VotingPlace        Parent { get; set; }
        public virtual ISet<VotingPlace>  Children { get; set; }
        public virtual ISet<VotingResult> Results { get; set; }

        public virtual int?               Counter(int i)
            {
            var counter = this.Results.FirstOrDefault(x => x.Counter == i);
            return counter != null ? counter.Value : (int?) null;
            }

        /// <summary>
        /// Число избирателей, внесенных в список избирателей
        /// </summary>
        public virtual int? NumberOfVotersInVoterList
            {
            get {
                return this.Counter(1);
                }
            set {}
            }

        /// <summary>
        /// Число избирательных бюллетеней, полученных участковой избирательной комиссией
        /// </summary>
        public virtual int? NumberOfBallotsReceivedByElectoralComission 
            {
            get {
                return this.Counter(2);
                }
            set { }
            }

        /// <summary>
        /// Число избирательных бюллетеней, выданных избирателям, проголосовавшим досрочно
        /// </summary>
        public virtual int? NumberOfBallotsIssuedToVotersWhoVotedEarly 
            {
            get {
                return this.Counter(3);
                }
            set { }
            }

        /// <summary>
        /// Число избирательных бюллетеней, выданных избирателям в помещении для голосования
        /// </summary>
        public virtual int? NumberOfBallotsIssuedToVoterAtPollStation 
            {
            get {
                return this.Counter(4);
                }
            set { }
            }
        /// <summary>
        /// Число избирательных бюллетеней, выданных избирателям вне помещения для голосования
        /// </summary>
        public virtual int? NumberOfBallotsIssuedToVotersOutsideOfPollStation 
            {
            get {
                return this.Counter(5);
                }
            set { }
            }
        /// <summary>
        /// Число погашенных избирательных бюллетеней
        /// </summary>
        public virtual int? NumberOfCancelledBallots  
            {
            get {
                return this.Counter(6);
                }
            set { }
            }

        /// <summary>
        /// Число избирательных бюллетеней в переносных ящиках для голосования
        /// </summary>
        public virtual int? NumberOfBallotsInPortableBallotBoxes 
            {
            get {
                return this.Counter(7);
                }
            set { }
            }
        /// <summary>
        /// Число избирательных бюллетеней в стационарных ящиках для голосования
        /// </summary>
        public virtual int? NumberOfBallotsInStationaryBallotBoxes 
            {
            get {
                return this.Counter(8);
                }
            set { }
            }

        /// <summary>
        /// Число недействительных избирательных бюллетеней
        /// </summary>
        public virtual int? NumberOfInvalidBallots 
            {
            get {
                return this.Counter(9);
                }
            set { }
            }

        /// <summary>
        /// Число действительных избирательных бюллетеней
        /// </summary>
        public virtual int? NumberValidBallots 
            {
            get {
                return this.Counter(10);
                }
            set { }
            }

        /// <summary>
        /// Число открепительных удостоверений, полученных участковой избирательной комиссией
        /// </summary>
        public virtual int? NumberOfAbsenteePermitsReceivedByElectoralComission 
            {
            get {
                return this.Counter(11);
                }
            set { }
            }
        /// <summary>
        /// Число открепительных удостоверений, выданных избирателям на избирательном участке
        /// </summary>
        public virtual int? NumberOfAbsenteePermitsIssuedAtPollingStation 
            {
            get {
                return this.Counter(12);
                }
            set { }
            }

        /// <summary>
        /// Число избирателей, проголосовавших по открепительным удостоверениям на избирательном участке
        /// </summary>
        public virtual int? NumberOfAbsenteeBallotsCastAtPollingStation 
            {
            get {
                return this.Counter(13);
                }
            set { }
            }

        /// <summary>
        /// Число погашенных неиспользованных открепительных удостоверений
        /// </summary>
        public virtual int? NumberOfCanceledAbsenteePermits 
            {
            get {
                return this.Counter(14);
                }
            set { }
            }

        /// <summary>
        /// Число открепительных удостоверений, выданных избирателям территориальной избирательной комиссией
        /// </summary>
        public virtual int? NumberOfAbsenteeBallotsIssuedByTerritorialElectionComission 
            {
            get {
                return this.Counter(15);
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
                return this.Counter(16);
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
                return this.Counter(17);
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
                return this.Counter(18);
                }
            set { }
            }

        public virtual double? Attendance
            {
            get {
                try
                    {
                    return (double)(100.00 * (this.NumberOfBallotsIssuedToVotersWhoVotedEarly + this.NumberOfBallotsIssuedToVoterAtPollStation + this.NumberOfBallotsIssuedToVotersOutsideOfPollStation ) / this.NumberOfVotersInVoterList);
                    }
                catch (Exception)
                    {
                    return 0;
                    }
                }
            set { }
            }
        }
    }