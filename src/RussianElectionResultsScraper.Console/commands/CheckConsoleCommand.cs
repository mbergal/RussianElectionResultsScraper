namespace RussianElectionResultsScraper.Console
{
    public class CheckConsoleCommand : BaseConsoleCommand
        {
        public CheckConsoleCommand()
            {
            this.IsCommand("check", "Check data integrity");
            this.HasElectionOption();
            this.HasConnectionOption();
            }

        public override int Run(string[] args)
            {
            return new CheckCommand( connectionString: this._connectionString, electionId: this._electionId).Execute();
            }
        }

}
