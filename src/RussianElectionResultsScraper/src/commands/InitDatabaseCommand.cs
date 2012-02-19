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
            this.IsCommand("init-database", "Create/recreate necessary objects in database");
            }

        public override int Run(string[] args)
            {
            var lines = new List<string>();

            var schemaExport = new SchemaExport( this.BuildElectionResultsDatabaseConfiguration() );
            schemaExport.Create(x => lines.Add(x), true);
            schemaExport.Execute(true, false, false);
            return 0;
            }
        
        }
}
