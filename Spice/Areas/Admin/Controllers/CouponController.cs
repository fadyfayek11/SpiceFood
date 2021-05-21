using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]

    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public Coupon coupon { get; set; }
        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        //GET- Create
        public IActionResult Create()
        {
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost()
        {
            if (ModelState.IsValid)
            {
                var file = HttpContext.Request.Form.Files;
                if (file.Count>0)
                {
                    byte[] p1 = null;
                    using (var fs1 = file[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    coupon.Picture = p1;
                }
                _db.Coupon.Add(coupon);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        //GET-Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var couponitem = await _db.Coupon.FindAsync(id);
            return View(couponitem);
        }

        //Post-Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {

            var couponitem = await _db.Coupon.Where(c => c.Id == coupon.Id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                var file = HttpContext.Request.Form.Files;
                if (file.Count>0)
                {
                    byte[] p1 = null;
                    using (var fs1 = file[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    couponitem.Picture = p1;
                }
                couponitem.Name = coupon.Name;
                couponitem.Discount = coupon.Discount;
                couponitem.MinmumAmount = coupon.MinmumAmount;
                couponitem.isActive = coupon.isActive;
                couponitem.CouponType = coupon.CouponType;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        //GET-Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var couponitem = await _db.Coupon.FindAsync(id);
            return View(couponitem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {

            var couponitem = await _db.Coupon.Where(c => c.Id == coupon.Id).FirstOrDefaultAsync();

             _db.Coupon.Remove(couponitem);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var couponitem = await _db.Coupon.FindAsync(id);
            return View(couponitem);
        }
        
    }
}
