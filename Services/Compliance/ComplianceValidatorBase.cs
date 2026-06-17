using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services.Compliance
{
    public class ComplianceValidatorBase
    {
        protected static FieldCheckResult Pass(string fieldName, string expected, string actual, string notes)
        {
            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = expected,
                ActualValue = actual,
                Status = "Pass",
                ConfidenceScore = 100,
                Notes = notes
            };
        }

        protected static FieldCheckResult Fail(string fieldName, string expected, string actual, string notes)
        {
            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = expected,
                ActualValue = actual,
                Status = "Fail",
                ConfidenceScore = 100,
                Notes = notes
            };
        }

        protected FieldCheckResult NotApplicable(string fieldName, string note)
        {
            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = "Not Applicable",
                ActualValue = "Not Applicable",
                IsMatch = true,
                WasSkipped = true,
                Status = "Skipped",
                ConfidenceScore = 0,
                Notes = note
            };
        }

        protected FieldCheckResult Skipped(string fieldName, string note)
        {
            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = "Not Applicable",
                ActualValue = "Not Applicable",
                IsMatch = true,
                WasSkipped = true,
                Status = "Skipped",
                ConfidenceScore = 0,
                Notes = note
            };
        }
    }
}