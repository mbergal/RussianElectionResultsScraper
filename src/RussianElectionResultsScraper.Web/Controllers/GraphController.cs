using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;
using NHibernate;
using NHibernate.Linq;
using RussianElectionResultsScraper.Model;
using Dapper;
using log4net;


namespace RussianElectionResultScraper.Web
    {
    public class GraphController : Controller
        {
        private readonly ISessionFactory _sessionFactory;
        private readonly ILog log = LogManager.GetLogger("GraphController");
        private const int defaultWidth = 600;
        private const int defaultHeight = 400;
        
        public GraphController( ISessionFactory sessionFactory )
            {
            this._sessionFactory = sessionFactory;
            }


        [OutputCache(Duration = int.MaxValue, NoStore = false, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "*", VaryByCustom = "LastUpdateTimestamp") ]
        public FileStreamResult  PollingStationsByAttendance(string region, int? width, int? height, bool? showGrid )
            {
            log.Info( string.Format( "PollingStationsByAttendance: region={0}, width={1}, height={2}, showGrid={3}", region, width, height, showGrid ) );
            var path = _sessionFactory.GetCurrentSession().Get<VotingPlace>(region).Path + _sessionFactory.GetCurrentSession().Get<VotingPlace>(region).Id + ":";
            var a = _sessionFactory.GetCurrentSession().Connection.Query<double>("select Attendance from VotingPlace where Path like @path and TYPE = 5", new { path = path + '%' } );
            var g = new List<int>();
            for (var i = 0; i <= 100; ++i  )
                g.Add( 0 );

            a.ForEach( x=>
                           {
                           g[(int)x]++;
                           } );
            var m = new MemoryStream();
            var chart = new System.Web.UI.DataVisualization.Charting.Chart();
            chart.Width = width ?? 600;
            chart.Height = height ?? 400;
            var s = new Series( "aaaa");
            s.ChartType = SeriesChartType.Column;
            for (int i = 0; i <= 100; ++i )
                s.Points.AddXY(i, g[i] );
            chart.Series.Add(s);
            var ca = new ChartArea 
                {
                AxisX =
                    {
                    IsStartedFromZero = true,
                    Minimum = 0,
                    Maximum = 100,
                    Interval = 10,
                    Title = "Явка",
                    Enabled = showGrid ?? false ? AxisEnabled.True : AxisEnabled.False,
                    LabelStyle =
                        {
                        Format = "{0} %"
                        }
                    }, 
                AxisY =
                    {
                    Enabled = showGrid ?? false ? AxisEnabled.True : AxisEnabled.False,
                    Title = "Количество УИК"
                    } 
                };
            chart.ChartAreas.Add(ca);

            chart.SaveImage( m );
            m.Seek( 0, SeekOrigin.Begin) ;
            return new FileStreamResult( m, "image/jpg");
            }

        [OutputCache(Duration = int.MaxValue, NoStore = false, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "*", VaryByCustom = "LastUpdateTimestamp")]
        public FileStreamResult PollingStationResults(string votingPlaceId, int? width, int? height, bool? showGrid)
            {
                log.Info( string.Format( "PollingStationResults: votingPlaceId={0}, width={1}, height={2}, showGrid={3}", votingPlaceId, width, height, showGrid ) );
                var vp = _sessionFactory.GetCurrentSession().Get<VotingPlace>(votingPlaceId);
                var m = new MemoryStream();
                var chart = new Chart {Width = width ?? 600, Height = height ?? 400};
                var s = new Series("aaaa");
                s.IsXValueIndexed = true;
                s.ChartType = SeriesChartType.Column;
                s.MarkerStep = 1;
                s["PointWidth"] = "1.0";
                var results = vp.CandidateResults.ToArray();
                for (int i = 0; i < results.Count(); ++i)
                    {
                    s.Points.AddXY(i, results[i].Percents);
                    s.Points.Last().Color = vp.Election.Candidates[i].Color;

                    }
                chart.Series.Add(s);
                var ca = new ChartArea() 
                    { 
                        AxisX =
                            {
                                Enabled = showGrid ?? false ? AxisEnabled.True : AxisEnabled.False, 
                                IsMarginVisible = false, 
                                Interval = Double.MinValue, 
                                IntervalOffset = 0, 
                                MaximumAutoSize = 100,
                                Title = "Партии/Кандидаты",
                                LabelStyle = 
                                    {
                                    Enabled = false
                                    }
                            },
                        AxisY =
                            {
                                Enabled = showGrid ?? false ? AxisEnabled.True : AxisEnabled.False, 
                                Title = "Процентов голосов",
                                LabelStyle =
                                    {
                                    Format = "{0} %"    
                                    }
                            } 
                    };
                chart.ChartAreas.Add(ca);

                chart.SaveImage(m);
                m.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(m, "image/jpg");
            }


        [OutputCache(Duration = int.MaxValue, NoStore = false, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "*", VaryByCustom = "LastUpdateTimestamp")]
        public FileStreamResult CandidateResultsByAttendance(string votingPlaceId, int? width, int? height, bool? showGrid)
            {
            var vp = _sessionFactory.GetCurrentSession().Get<VotingPlace>(votingPlaceId);
            var election = vp.Election;
            IEnumerable<CandidateResultsByAttendanceRecord> points;
            using (var session = _sessionFactory.OpenStatelessSession())
                {
                points = session.Connection.Query<CandidateResultsByAttendanceRecord>("select vp.Id Id, vp.Attendance Attendance, vr.Counter, vr.Value, vr.Percents from VotingResult vr inner join VotingPlace vp on vr.VotingPlaceId = vp.Id where vp.Path like @path and vp.Type = @type and vr.Counter in ( select Counter from CounterDescription where electionId = @electionId and IsCandidate = 1 )", 
                    new
                        {
                        path = vp.Path + vp.Id + ":%", 
                        electionId = election.Id,
                        type = (int)vp.Type < 3 ? 3 : 5
                        });    
                }
            

            var chart = new Chart { Width = width ?? 600, Height = height ?? 400 };
            var ca = new ChartArea()
                {
                AxisX =
                    {
                    IsStartedFromZero = true,
                    Minimum = 0,
                    Maximum = 100,
                    Interval = 10,
                    Title = "Явка",
                    Enabled = showGrid ?? false ? AxisEnabled.True : AxisEnabled.False,
                    LabelStyle =
                        {
                        Format = "{0} %"
                        }
                    },
                AxisY =
                    {
                    Enabled = showGrid ?? false ? AxisEnabled.True : AxisEnabled.False,
                    Title = "Отданные голоса за партию/кандидата",
                    Minimum = 0,
                    Maximum = 100,
                    Interval = 10,
                    LabelStyle =
                        {
                        Format = "{0} %"
                        }
                    },
                };

            chart.ChartAreas.Add(ca);
            var serieses = new Dictionary<string, Series>();
            election.Candidates.ForEach( x=>
                                             {
                                             var series = new Series(x.ShortName)
                                                              {
                                                              MarkerColor = election.Counter(x.Counter).Color,
                                                              MarkerSize = this.MarkerSize( chart.Width, chart.Height, points.Count() ),
                                                              ChartType = SeriesChartType.Point,
                                                              MarkerStyle = MarkerStyle.Square
                                                              };

                                             serieses.Add( x.Counter, series );
                                             chart.Series.Add(series);
                                             } );
            foreach (var r in points)
                {
                serieses[ r.Counter ].Points.AddXY( r.Attendance, r.Percents );
                }
            var m = new MemoryStream();
            chart.SaveImage(m);
            m.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(m, "image/jpg");
            }

        private int MarkerSize( Unit width, Unit height, int numOfPoints )
            {
            int s = (int) (height.Value * 10);
            if (numOfPoints > s   ) return 1;
            if (numOfPoints > s/2 ) return 3;
            if (numOfPoints > s/4)  return 5;
            if (numOfPoints > s/8) return 7;
            else return 10;
            }
        }

    public class CandidateResultsByAttendanceRecord
        {
        public string Id;
        public double Attendance;
        public string Counter;
        public int Value;
        public decimal Percents;
        };

    }