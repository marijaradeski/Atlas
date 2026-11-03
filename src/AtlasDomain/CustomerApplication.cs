using System;
using System.Collections.Generic;
using System.Text;

namespace AtlasDomain
{
    public class CustomerApplication
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }

        public string Country { get; set; } = string.Empty;

        public string NationalId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public ApplicationStatus Status { get; private set; }

        public DateTime CreatedAtUtc { get; set; }

        public void MarkAwaitingBranchVisit()
        {
            TransitionTo(
                ApplicationStatus.AwaitingBranchVisit,
                ApplicationStatus.Submitted,
                ApplicationStatus.IdentityVerificationPending);
        }

        public void MarkScreeningPending()
        {
            TransitionTo(
                ApplicationStatus.ScreeningPending,
                ApplicationStatus.Submitted,
                ApplicationStatus.IdentityVerificationPending,
                ApplicationStatus.AwaitingBranchVisit,
                ApplicationStatus.ManualReviewRequired);
        }

        public void MarkManualReviewRequired()
        {
            TransitionTo(
                ApplicationStatus.ManualReviewRequired,
                ApplicationStatus.Submitted,
                ApplicationStatus.IdentityVerificationPending,
                ApplicationStatus.ScreeningPending,
                ApplicationStatus.AwaitingBranchVisit);
        }

        public void MarkIdentityVerificationFailed()
        {
            TransitionTo(
                ApplicationStatus.IdentityVerificationFailed,
                ApplicationStatus.Submitted,
                ApplicationStatus.IdentityVerificationPending);
        }

        public void MarkRejected()
        {
            TransitionTo(
                ApplicationStatus.Rejected,
                ApplicationStatus.Submitted,
                ApplicationStatus.IdentityVerificationPending,
                ApplicationStatus.ScreeningPending,
                ApplicationStatus.ManualReviewRequired,
                ApplicationStatus.AwaitingBranchVisit);
        }

        private void TransitionTo(ApplicationStatus newStatus, params ApplicationStatus[] allowedPreviousStatuses)
        {
            if (Status == newStatus)
                return;

            if (!allowedPreviousStatuses.Contains(Status))
            {
                throw new InvalidOperationException($"Cannot transition application from {Status} to {newStatus}.");
            }

            Status = newStatus;
        }

        public static CustomerApplication Create(
    string firstName,
    string lastName,
    DateOnly dateOfBirth,
    string country,
    string nationalId,
    string email,
    string phone,
    DateTime createdAtUtc)
        {
            if (createdAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Creation timestamp must be UTC.");

            return new CustomerApplication
            {
                Id = Guid.NewGuid(),
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                DateOfBirth = dateOfBirth,
                Country = country.Trim(),
                NationalId = nationalId.Trim(),
                Email = email.Trim(),
                Phone = phone.Trim(),
                Status = ApplicationStatus.Submitted,
                CreatedAtUtc = createdAtUtc
            };
        }
    }
}
