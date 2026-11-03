using System;
using System.Collections.Generic;
using System.Text;

namespace AtlasDomain
{
    public enum ApplicationStatus
    {
        Submitted = 1,
        IdentityVerificationPending = 2,
        IdentityVerificationFailed = 3,
        ScreeningPending = 4,
        ManualReviewRequired = 5,
        Approved = 6,
        Rejected = 7,
        AwaitingBranchVisit = 8,
        Activated = 9
    }
}
