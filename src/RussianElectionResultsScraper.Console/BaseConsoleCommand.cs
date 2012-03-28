using ManyConsole;

namespace RussianElectionResultsScraper
    {
    
    public abstract class BaseConsoleCommand : ConsoleCommand
        {
        protected string            _configFile;
        protected string            _electionId;
        protected string            _connectionString;
        protected string            _providerName;

        public override int Run(string[] args)
            {
            return 0;
            }

        public void HasConfigOption()
            {
            this.HasRequiredOption("c|config=", "<config-file>", configFile => this._configFile = configFile);
            }

        public void HasElectionOption()
            {
            this.HasRequiredOption("e|election=", "<election-id>", electionId => this._electionId = electionId );
            }

        public void HasConnectionOption()
            {
            this.HasRequiredOption("c|connection|connectionString=", "<connection-string>", connectionString => this._connectionString = connectionString );
            }

        public void HasProviderOption()
            {
            this.HasRequiredOption("p|provider=", "<provider-name>", providerName => this._providerName = providerName );
            }
        }
    }
