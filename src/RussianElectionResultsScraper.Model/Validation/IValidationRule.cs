using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RussianElectionResultsScraper.Model.Validation
{
    public interface IValidationRule
        {
        IEnumerable<ValidationProblem> Check();
        IEnumerable<ValidationProblem> Check(ValidationVotingResult votingResult);
        };
}
