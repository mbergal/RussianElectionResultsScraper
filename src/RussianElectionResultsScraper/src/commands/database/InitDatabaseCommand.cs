namespace RussianElectionResultsScraper.Commands.Database
{
    public class InitDatabaseCommand : BaseCommand
        {
        public InitDatabaseCommand( string connectionString )
            : base( connectionString: connectionString )
            {
            }

        public override int Execute()
            {
            var runner = new RussianElectionResultsScraper.MigrationRunner.MigrationRunner( this._providerName );
            runner.MigrateUp( this._connectionString );
            return 0;
            }
        
        }
}
