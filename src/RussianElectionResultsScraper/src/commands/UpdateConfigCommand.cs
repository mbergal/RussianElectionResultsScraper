namespace RussianElectionResultsScraper
{
    public class UpdateConfigCommand : BaseConsoleCommand
        {
        public UpdateConfigCommand()
            {
            this.IsCommand( "update-config", "Update Configuration" );
            this.HasConfigOption();
            }

        public override int Run(string[] args)
            {
            base.Run(args);
            SaveElection(LoadConfiguration());
            return 0;
            }

        }
}
