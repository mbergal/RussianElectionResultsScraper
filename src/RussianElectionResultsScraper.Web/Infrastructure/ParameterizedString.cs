namespace RussianElectionResultScraper.Web.Infrastructure
    {
    public class ParameterizedString
        {
        private readonly string      _format;
        private readonly object[]    _args;

        public ParameterizedString( string format, params object[] args )
            {
            this._format = format;
            this._args = args;
            }

        public string Format
            {
            get { return _format; }
            }

        public object[] Args
            {
            get { return this._args;  }
            }
        }
    }
