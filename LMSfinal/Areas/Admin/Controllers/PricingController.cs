using LMSfinal.Data;
using LMSfinal.Models.ViewModels.Admin;
using LMSfinal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PricingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPricingService _pricingService;

        public PricingController(ApplicationDbContext context, IPricingService pricingService)
        {
            _context = context;
            _pricingService = pricingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var history = await _context.PricePerCreditHistories
                .OrderByDescending(x => x.EffectiveFrom)
                .ToListAsync();

            var currentPrice = await _pricingService.GetCurrentPriceAsync();

            var viewModel = new PricingHistoryViewModel
            {
                CurrentPrice = currentPrice,
                NewEffectiveFrom = DateTime.Now,
                History = history
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrice(PricingHistoryViewModel input)
        {
            if (!ModelState.IsValid)
            {
                input.CurrentPrice = await _pricingService.GetCurrentPriceAsync();
                input.History = await _context.PricePerCreditHistories
                    .OrderByDescending(x => x.EffectiveFrom)
                    .ToListAsync();

                return View("Index", input);
            }

            await _pricingService.SetNewPriceAsync(input.NewPrice, input.NewEffectiveFrom);

            TempData["success"] = "✅ Đã cập nhật giá tín chỉ.";
            return RedirectToAction(nameof(Index));
        }
    }
}