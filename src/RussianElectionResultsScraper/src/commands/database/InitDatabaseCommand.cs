namespace RussianElectionResultsScraper.Commands.Database
{
    public class InitDatabaseCommand : BaseCommand
        {
        public InitDatabaseCommand( string connectionString, string providerName )
            : base( connectionString: connectionString, providerName: providerName )
            {
            }

        public override int Execute()
            {
            var runner = new MigrationRunner.MigrationRunner( this._providerName );
            runner.MigrateUp( this._connectionString );
            return 0;
            }
        
        }
}
