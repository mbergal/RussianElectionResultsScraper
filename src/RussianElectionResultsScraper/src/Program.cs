using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository;

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
            var commands = GetCommands();

            // optionally, include ConsoleModeCommand if you want to allow the user to run
            // commands from the console.
            var consoleRunner = new ConsoleModeCommand(GetCommands);
            commands = commands.Concat( new[] { consoleRunner });
            XmlConfigurator.Configure();
            SetLogDirectory( Path.Combine(Directory.GetCurrentDirectory(), "logging") );


            // run the command for the console input
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            }

        //
        // http://geekswithblogs.net/wpeck/archive/2009/10/08/setting-log4net-fileappender.file-at-runtime.aspx
        //
        public static void SetLogDirectory(string logDirectory) 
            {
            // get the current logging repository for this application 
            ILoggerRepository repository = LogManager.GetRepository(); 

            // get all of the appenders for the repository 
            IAppender[] appenders = repository.GetAppenders(); 

            // only change the file path on the 'FileAppenders' 
            foreach (IAppender appender in (from iAppender in appenders 
                                            where iAppender is FileAppender 
                                            select iAppender)) 
                { 
                var fileAppender = appender as FileAppender; 
                // set the path to your logDirectory using the original file name defined 
                // in configuration 
                fileAppender.File = Path.Combine(logDirectory, Path.GetFileName(fileAppender.File)); 
                // make sure to call fileAppender.ActivateOptions() to notify the logging 
                // sub system that the configuration for this appender has changed. 
                fileAppender.ActivateOptions(); 
                }    
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
