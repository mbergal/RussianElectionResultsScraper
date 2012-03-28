namespace RussianElectionResultsScraper.Console
{
    public class RepairConsoleCommand : BaseConsoleCommand
    {
        public RepairConsoleCommand()
            {
            this.IsCommand("repair", "Repair scraped data");
            this.HasConnectionOption();
            this.HasElectionOption();
            }

        public override int Run(string[] args)
            {
            return new RepairCommand( connectionString: this._connectionString ).Execute();
            }
    }
}
