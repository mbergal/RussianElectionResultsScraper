using RussianElectionResultsScraper.Commands.Database;

namespace RussianElectionResultsScraper.Console
{
    public class InitDatabaseConsoleCommand : BaseConsoleCommand
        {
        public InitDatabaseConsoleCommand()
            {
            this.IsCommand("database:init", "Create/recreate necessary objects in database");
            this.HasConnectionOption();
            this.HasProviderOption();
            }

        public override int Run(string[] args)
            {
            return new InitDatabaseCommand( this._connectionString ).Execute();
            }
        
        }
}
