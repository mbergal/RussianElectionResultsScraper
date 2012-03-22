
using System.Collections.Generic;

namespace RussianElectionResultsScraper.Model.Validation
{
    public class ValidationRule : IValidationRule
    {
        protected ValidationVotingPlace _votingPlace;

        public ValidationRule( ValidationVotingPlace votingPlace )
            {
            this._votingPlace = votingPlace;
            }

        public virtual IEnumerable<ValidationProblem> Check()
            {
            return new ValidationProblem[] {};
            }

        public virtual IEnumerable<ValidationProblem> Check(ValidationVotingResult votingResult)
            {
            return new ValidationProblem[] { };
            }
    }
}
