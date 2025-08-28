using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Services;

namespace ABCRetailers.Controllers
{
    public class UploadController : Controller
    {
        private readonly IAzureStorageService _storageService; // ✅ Correct private field naming

        // Constructor injection
        public UploadController(IAzureStorageService storageService)
        {
            _storageService = storageService;
        }

        // GET: Upload/Index
        public IActionResult Index()
        {
            return View(new FileUploadModel());
        }

        // POST: Upload/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.ProofOfPayment != null && model.ProofOfPayment.Length > 0)
                    {
                        // Upload to blob storage
                        var fileName = await _storageService.UploadFileAsync(model.ProofOfPayment, "payment-proofs");

                        // Also upload to file share
                        await _storageService.UploadToFileShareAsync(model.ProofOfPayment, "contracts", "payments");

                        TempData["Success"] = $"File uploaded successfully! File name: {fileName}";

                        // Return fresh model for new upload
                        return View(new FileUploadModel());
                    }
                    else
                    {
                        ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                }
            }

            return View(model);
        }
    }
}