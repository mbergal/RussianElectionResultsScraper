using System.Collections.Generic;
using System.Linq;
using ManyConsole;

namespace RussianElectionResultsScraper.Console
{
    public class Program
        {
        public static int  Main(string[] args)
            {
            var commands = GetCommands();

            // optionally, include ConsoleModeCommand if you want to allow the user to run
            // commands from the console.
            var consoleRunner = new ConsoleModeCommand(GetCommands);
            commands = commands.Concat( new[] { consoleRunner });

            // run the command for the console input
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, System.Console.Out );
            }

        private static IEnumerable<ConsoleCommand> GetCommands()
            {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
            }
        }

}
