using FluentMigrator;

namespace RussianElectionResultsScraper.Migrations
    {
    [Migration(0)]
    public class Migration_0 : Migration
        {
        public override void Up()
            {
            Execute.Script( "0\\up.sql" );
            }

        public override void Down()
            {
            throw new System.NotImplementedException();
            }
        }
    }