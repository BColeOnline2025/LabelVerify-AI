using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class BatchesModel(ReviewBatchService reviewBatchService) : PageModel
    {
        private readonly ReviewBatchService _reviewBatchService = reviewBatchService;

        public List<ReviewBatch> Batches { get; set; } = [];

        public async Task OnGetAsync()
        {
            Batches = await _reviewBatchService.GetBatchesAsync();
        }
    }
}