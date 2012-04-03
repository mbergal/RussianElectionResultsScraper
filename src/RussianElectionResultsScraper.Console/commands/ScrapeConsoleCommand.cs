using RussianElectionResultsScraper.Commands;

namespace RussianElectionResultsScraper.Console
    {
    public class ScrapeConsoleCommand : BaseConsoleCommand
        {
        private string _root;
        private bool   _recursive = false;
        private bool   _useCache = false;
        private int _maxworkers = 6;

        public ScrapeConsoleCommand()
                {
                this.IsCommand("scrape", "Run scraper");
                this.HasConnectionOption();
                this.HasRequiredOption("u|url=", "<root-url>", url => this._root = url);
                this.HasConfigOption();
                this.HasOption("r|recursive", "<config-file>", recursive => this._recursive = recursive != null);
                this.HasOption<int>("m|maxworkers", "<maximum-number-of-workers>", maxworkers => this._maxworkers = maxworkers );
                this.HasOption( "cache", "<use-cache>", useCache => this._useCache = useCache != null );
                }

        public override int Run(string[] args)
            {
            return new ScrapeCommand( connectionString: this._connectionString, 
                                root: this._root, 
                                configFile: this._configFile,
                                recursive: this._recursive, 
                                useCache: this._useCache,
                                maxWorkers: this._maxworkers ).Execute();
            }
        }
    }
