namespace RussianElectionResultsScraper.Console
{
    public class CheckConsoleCommand : BaseConsoleCommand
        {
        public CheckConsoleCommand()
            {
            this.IsCommand("check", "Check data integrity");
            this.HasOption("e|election=", "<election-id>", election => this._electionId = election);
            }

        public override int Run(string[] args)
            {
            return new CheckCommand().Execute();
            }
        }

}
