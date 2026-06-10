using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LabelVerify.Web.ViewModels
{
    public class UploadLabelViewModel
    {
        [Required]
        public string BrandName { get; set; } = "";

        public string? ClassType { get; set; } = "";

        public string? AlcoholContent { get; set; } = "";

        public string? NetContents { get; set; } = "";

        public string? GovernmentWarning { get; set; } = "";

        [Required]
        public IFormFile? LabelFile { get; set; }
    }
}