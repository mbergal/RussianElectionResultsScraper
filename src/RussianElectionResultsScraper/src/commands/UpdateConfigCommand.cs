namespace RussianElectionResultsScraper.Commands
{
    public class UpdateConfigCommand : BaseCommand
        {
        public UpdateConfigCommand( string configFile, string connectionString )
            : base( configFile: configFile, connectionString: connectionString )
            {
            }

        public override int Execute()
            {
            SaveElection( LoadConfiguration() );
            return 0;
            }
        }
}
