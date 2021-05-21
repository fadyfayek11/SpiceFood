using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spice.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        

        [BindProperty]
        public IndexViewModel IndexView { get; set; }

       
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
            IndexView = new IndexViewModel()
            {
                MenuItem = _db.MenuItem.Include(i => i.Category).Include(i => i.SubCategory).ToList(),
                Category = _db.Category.ToList(),
                Coupon = _db.Coupon.Where(c => c.isActive == true).ToList(),
            };

        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var itemFromDb = await _db.MenuItem.Include(category => category.Category).Include(sub => sub.SubCategory).FirstOrDefaultAsync(i => i.Id == id);
            ShoppingCart cart = new ShoppingCart()
            {
                MenuItem = itemFromDb,
                MenuItemId = itemFromDb.Id,
            };
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart Cart)
        {
            Cart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                Cart.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await _db.ShoppingCart.Where(c => c.ApplicationUserId == Cart.ApplicationUserId
                                                                                            && c.MenuItemId == Cart.MenuItemId).FirstOrDefaultAsync();

                if (cartFromDb == null)
                {
                  await _db.ShoppingCart.AddAsync(Cart);
                }
                else
                {
                    cartFromDb.Count = cartFromDb.Count + cartFromDb.Count;
                }
                await _db.SaveChangesAsync();

                var count = _db.ShoppingCart.Where(c=>c.ApplicationUserId == Cart.ApplicationUserId).ToList().Count();

                HttpContext.Session.SetInt32(SD.SessionOfShoppingCart,count);
                return RedirectToAction("Index");
            }
            else
            {
                var itemFromDb = await _db.MenuItem.Include(category => category.Category).Include(sub => sub.SubCategory).Where(i => i.Id == Cart.MenuItemId).FirstOrDefaultAsync();
                ShoppingCart cart = new ShoppingCart()
                {
                    MenuItem = itemFromDb,
                    MenuItemId = itemFromDb.Id,
                };
                return View(cart);
            }

        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim!=null)
            {
                var count = _db.ShoppingCart.Where(u => u.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.SessionOfShoppingCart,count);
            }

            return View(IndexView);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
