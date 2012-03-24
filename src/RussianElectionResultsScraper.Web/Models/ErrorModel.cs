using System;

namespace RussianElectionResultScraper.Web
    {
    public class ErrorModel
        {
        private readonly Exception _error;

        public ErrorModel( Exception error )
            {
            this._error = error;
            }

        public string Title
            {
            get { return this._error.Message; }
            }
        }

    }