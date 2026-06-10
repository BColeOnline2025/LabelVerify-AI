using LabelVerify.Web.Models;

namespace LabelVerify.Web.Rules
{
    public interface ILabelRule
    {
        FieldCheckResult Evaluate(
            LabelApplication application,
            string extractedText);
    }
}
