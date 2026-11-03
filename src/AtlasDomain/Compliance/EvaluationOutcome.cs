using System;
using System.Collections.Generic;
using System.Text;

namespace AtlasDomain.Compliance
{
    public enum EvaluationOutcome
    {
        Pending = 0,
        Satisfied = 1,
        NotSatisfied = 2,
        RequiresReview = 3,
        NotApplicable = 4
    }
}
