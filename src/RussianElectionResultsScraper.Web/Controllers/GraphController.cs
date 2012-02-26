using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
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
        private ILog log = LogManager.GetLogger("GraphController");
        

        public GraphController( ISessionFactory sessionFactory )
            {
            this._sessionFactory = sessionFactory;
            }


        [OutputCache(Duration = int.MaxValue, NoStore = false, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "*", VaryByCustom = "LastUpdateTimestamp") ]
        public FileStreamResult  PollingStationsByAttendance(string region, int? width, int? height, bool? showGrid )
            {
            log.Info("PollingStationsByAttendance");
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
                AxisX = {IsStartedFromZero = true, Minimum = 0, Maximum = 100, Enabled = AxisEnabled.False}, 
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

        public FileStreamResult PollingStationResults(string votingPlaceId, int? width, int? height, bool? showGrid)
            {
                var  vp = _sessionFactory.GetCurrentSession().Get<VotingPlace>(votingPlaceId);
                var m = new MemoryStream();
                var chart = new Chart();
                chart.Width = width ?? 600;
                chart.Height = height ?? 400;
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
                        AxisX = { Enabled = AxisEnabled.False, IsMarginVisible = false, Interval = Double.MinValue, IntervalOffset = 0, MaximumAutoSize = 100 },
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

        }


    }