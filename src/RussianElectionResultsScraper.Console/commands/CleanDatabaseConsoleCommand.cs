using RussianElectionResultsScraper.Commands.Database;

namespace RussianElectionResultsScraper.Console
    {
    public class CleanDatabaseConsoleCommand : BaseConsoleCommand
        {
        public CleanDatabaseConsoleCommand()
            {
            this.IsCommand( "database:clean", "Clean database" );
            this.HasConnectionOption();
            this.HasProviderOption();
            }

        public override int Run(string[] args)
            {
            return new CleanDatabaseCommand( connectionString: this._connectionString ).Execute();
            }
        }
    }