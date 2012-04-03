using RussianElectionResultsScraper.Commands;

namespace RussianElectionResultsScraper.Console
{
    public class UpdateConfigConsoleCommand : BaseConsoleCommand
        {
        public UpdateConfigConsoleCommand()
            {
            this.IsCommand( "update-config", "Update Configuration" );
            this.HasConfigOption();
            this.HasConnectionOption();
            }

        public override int Run(string[] args)
            {
            base.Run(args);
            return new UpdateConfigCommand( configFile: this._configFile, connectionString: this._connectionString ).Execute();
            }
        }
}
