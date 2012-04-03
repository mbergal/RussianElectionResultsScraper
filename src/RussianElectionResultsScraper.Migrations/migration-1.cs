using FluentMigrator;

namespace RussianElectionResultsScraper.Migrations
    {
    [Migration(1)]
    public class Migration_1 : Migration
        {
        public override void Up()
            {
            Execute.Script( "migration-scripts\\1\\up.sql" );
            }

        public override void Down()
            {
            throw new System.NotImplementedException();
            }
        }
    }