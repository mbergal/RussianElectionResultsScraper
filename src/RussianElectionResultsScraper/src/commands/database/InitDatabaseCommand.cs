using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Tool.hbm2ddl;

namespace RussianElectionResultsScraper.src.commands
{
    public class InitDatabaseCommand : BaseConsoleCommand
        {
        public InitDatabaseCommand()
            {
            this.IsCommand("database:init", "Create/recreate necessary objects in database");
            this.HasConnectionOption();
            this.HasProviderOption();
            }

        public override int Run(string[] args)
            {
            var runner = new RussianElectionResultsScraper.MigrationRunner.MigrationRunner( this._providerName );
            runner.MigrateUp( this._connectionString );
            return 0;
            }
        
        }
}
