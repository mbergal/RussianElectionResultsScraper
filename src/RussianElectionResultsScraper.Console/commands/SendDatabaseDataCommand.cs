﻿using RussianElectionResultsScraper.Commands.Database;

namespace RussianElectionResultsScraper.Console
    {
    public class SendDatabaseDataConsoleCommand : BaseConsoleCommand
        {
        private string _destination;

        public SendDatabaseDataConsoleCommand()
            {
            this.IsCommand( "database:send-data", "Send database data" );
            this.HasConnectionOption();
            this.HasRequiredOption( "d|destination=", "connection string or url",
                                    destination => this._destination = destination );
            }

        public override int Run( string[] args )
            {
            return new SendDatabaseDataCommand( connectionString: this._connectionString, destination: this._destination ).Execute();
            }

        }
    }