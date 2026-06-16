using Microsoft.AspNetCore.Identity;

namespace LabelVerify.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}
