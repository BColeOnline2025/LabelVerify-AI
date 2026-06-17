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
    }
}