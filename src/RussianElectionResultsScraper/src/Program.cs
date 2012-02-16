using System;
using System.Collections.Generic;
using System.Linq;
using ManyConsole;

namespace RussianElectionResultsScraper
{

    //TODO: Counter Descriptions
    // Small web site
    public class CounterDescription
        {
        public string counterName;
        public string counterSource;
        };

    class Program
        {
        static int  Main(string[] args)

            {
            Console.WriteLine( String.Join( ";", args ));
            var commands = GetCommands();

            // optionally, include ConsoleModeCommand if you want to allow the user to run
            // commands from the console.
            var consoleRunner = new ConsoleModeCommand(GetCommands);
            commands = commands.Concat( new[] { consoleRunner });

            // run the command for the console input
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            }

        static IEnumerable<ConsoleCommand> GetCommands()
            {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
            }

        public Program()
            {
            }

    }

}
