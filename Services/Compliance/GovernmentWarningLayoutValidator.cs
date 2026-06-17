using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services.Compliance
{
    public class GovernmentWarningLayoutValidator : ComplianceValidatorBase
    {
        private const double MinimumEstimatedHeaderHeight = 0.10;

        public FieldCheckResult ValidateHeaderFontSize(double actualHeight)
        {
            if (actualHeight <= 0)
            {
                return Fail("Government Warning Font Size", $"Estimated header height >= {MinimumEstimatedHeaderHeight:0.000}",
                    "Not detected", "Unable to determine Government Warning header font size from OCR layout.");
            }

            if (actualHeight >= MinimumEstimatedHeaderHeight)
            {
                return Pass("Government Warning Font Size", $"Estimated header height >= {MinimumEstimatedHeaderHeight:0.000}",
                    actualHeight.ToString("0.000"), "Government Warning header appears sufficiently prominent.");
            }

            return Fail("Government Warning Font Size", $"Estimated header height >= {MinimumEstimatedHeaderHeight:0.000}",
                actualHeight.ToString("0.000"), "Government Warning header appears smaller than expected and may not be conspicuous.");
        }
    }
}