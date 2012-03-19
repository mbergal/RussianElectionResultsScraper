using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using ManyConsole;
using NHibernate;
using NHibernate.Caches.SysCache2;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Linq;
using NHibernate.SqlTypes;
using NHibernate.Tool.hbm2ddl;
using RussianElectionResultsScraper.Model;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository;
using Configuration = NHibernate.Cfg.Configuration;

namespace RussianElectionResultsScraper
    {
    
    public abstract class BaseConsoleCommand : ConsoleCommand
        {
        protected string _configFile;

        public override int Run(string[] args)
            {
            XmlConfigurator.Configure();
            this.SetLogDirectory( Path.Combine(Directory.GetCurrentDirectory(), "logging") );


            _pageCacheSessionFactory = ConfigurePageCacheDatabase();
            _electionResultsSessionFactory = ConfigureElectionResultsDatabase();

            return 0;
            }

        //
        // http://geekswithblogs.net/wpeck/archive/2009/10/08/setting-log4net-fileappender.file-at-runtime.aspx
        //
        public void SetLogDirectory(string logDirectory) 
            { 
            //get the current logging repository for this application 
            ILoggerRepository repository = LogManager.GetRepository(); 
            //get all of the appenders for the repository 
            IAppender[] appenders = repository.GetAppenders(); 
            //only change the file path on the 'FileAppenders' 
            foreach (IAppender appender in (from iAppender in appenders 
                                            where iAppender is FileAppender 
                                            select iAppender)) 
                { 
                var fileAppender = appender as FileAppender; 
                //set the path to your logDirectory using the original file name defined 
                //in configuration 
                fileAppender.File = Path.Combine(logDirectory, Path.GetFileName(fileAppender.File)); 
                //make sure to call fileAppender.ActivateOptions() to notify the logging 
                //sub system that the configuration for this appender has changed. 
                fileAppender.ActivateOptions(); 
                }    
            }
            
        public void HasConfigOption()
            {
            this.HasRequiredOption("c|config=", "<config-file>", configFile => this._configFile = configFile);
            }
                

        private ISessionFactory ConfigurePageCacheDatabase()
            {
            return this.ConfigureDatabase("pagecache.sdf", typeof( CachedPage ).Assembly, recreate: false );
            }

        public Configuration BuildElectionResultsDatabaseConfiguration()
            {
            var configuration = Fluently.Configure()
                .Database(MsSqlConfiguration
                        .MsSql2008
                        .Dialect<MsSql2008Dialect>()
                        .ConnectionString(ConfigurationManager.ConnectionStrings[ "Elections" ].ConnectionString))
                .Cache(c => c.UseQueryCache().ProviderClass(typeof(SysCacheProvider).AssemblyQualifiedName))
                .BuildConfiguration();

            configuration.AddAssembly(typeof(VotingPlace).Assembly);
            return configuration;
            }

        private ISessionFactory ConfigureElectionResultsDatabase()
            {
            var configuration = this.BuildElectionResultsDatabaseConfiguration();
            return configuration.BuildSessionFactory();
            }

        private ISessionFactory     ConfigureDatabase( string path, Assembly containingAssembly, bool recreate )
            {
            string connString = string.Format( "Data Source='{0}';Max Database Size=4000;", path );
            bool newDatabase = false;
            if ( !File.Exists(path) )
                {
                var engine = new SqlCeEngine(connString);
                engine.CreateDatabase();
                newDatabase = true;
                }

            var configuration = Fluently.Configure()
                .Database(MsSqlCeConfiguration
                        .Standard
                        .Driver<FixedSqlServerCeDriver>()
                        .IsolationLevel( IsolationLevel.Snapshot )
                        .ConnectionString(connString) )
                .BuildConfiguration();

            configuration.AddAssembly( containingAssembly );

            if (newDatabase || recreate)
                {
                var lines = new List<string>();

                var schemaExport = new SchemaExport(configuration);
                schemaExport.Create(lines.Add, true);
                schemaExport.Execute(true, false, false);
                }
            return configuration.BuildSessionFactory();
            }

        public ElectionConfig LoadConfiguration()
            {
            var configuration = ElectionConfig.Load(new XmlTextReader(new StreamReader(this._configFile)));
            configuration.Validate();
            return configuration;
            }


        public Election SaveElection(ElectionConfig electionConfig)
            {
            Election election;
            using (ISession session = this._electionResultsSessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
                {
                election = session.Get<Election>(electionConfig.Id) ?? new Election() { Id = electionConfig.Id };
                election.Name = electionConfig.Name;
                election.Update(electionConfig.Counters.Select( x=>new Model.CounterDescription
                                                                       { 
                                                                        Color = x.Color,
                                                                        Counter = x.Counter,
                                                                        ShortName = x.ShortName,
                                                                        IsCandidate = x.IsCandidate,
                                                                        Name = x.Name
                                                                        }));
                session.Save(election);
                transaction.Commit();
                session.Evict( election );
                }
            return election;
        }

        protected ISessionFactory _pageCacheSessionFactory;
        protected ISessionFactory _electionResultsSessionFactory;
        }

    public class FixedSqlServerCeDriver : SqlServerCeDriver
    {
        protected override void InitializeParameter(IDbDataParameter dbParam, string name, SqlType sqlType)
        {
            base.InitializeParameter(dbParam, name, sqlType);

            if (sqlType is BinarySqlType)
            {

                PropertyInfo dbParamSqlDbTypeProperty = dbParam.GetType().GetProperty("SqlDbType");

                dbParamSqlDbTypeProperty.SetValue(dbParam, SqlDbType.Image, null);
            }

        }

    }


    }
