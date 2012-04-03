using RussianElectionResultsScraper.Commands.Database;

namespace RussianElectionResultsScraper.Console
    {
    public class GenerateSchemaConsoleCommand : BaseConsoleCommand
        {
        public GenerateSchemaConsoleCommand()
            {
            this.IsCommand( "database:generate-schema", "Generate database schema" );
            this.HasConnectionOption();
            this.HasProviderOption();
            }
    
        public override int Run(string[] args)
            {
            return new GenerateSchemaCommand( providerName: this._providerName ).Execute();
            }
        }
    }
