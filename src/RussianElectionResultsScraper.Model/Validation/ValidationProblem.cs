using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RussianElectionResultsScraper.Model.Validation
{
    public class ValidationProblem
        {
        private readonly int _votingResultId;
        private readonly string _message;

        public ValidationProblem( int votingResultId, string validationMessage )
            {
            this._votingResultId = votingResultId;
            this._message = validationMessage;
            }

        public int          VotingResultId
            {
            get { return this._votingResultId; }
            }

        public string       ValidationMessage
            {
            get { return this._message; }
            }
        }
}
