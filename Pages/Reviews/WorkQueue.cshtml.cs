using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class WorkQueueModel(ReviewQueryService reviewQueryService) : PageModel
    {
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;

        public List<ReviewSession> Reviews { get; set; } = [];

        public async Task OnGetAsync()
        {
            Reviews = await _reviewQueryService.GetWorkQueueAsync();
        }
    }
}