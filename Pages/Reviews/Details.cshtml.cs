using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class DetailsModel : PageModel
    {
        private readonly ReviewQueryService _reviewQueryService;

        public DetailsModel(ReviewQueryService reviewQueryService)
        {
            _reviewQueryService = reviewQueryService;
        }

        public ReviewSession? Review { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Review = await _reviewQueryService.GetByIdAsync(id);

            if (Review == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}