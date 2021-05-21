using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Spice.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spice.Models.ViewModels;
using System.IO;
using Spice.Utility;
using Spice.Models;
using Microsoft.AspNetCore.Authorization;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]

    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHost; //for saving files in server

        [BindProperty]
        public MenuItemViewModel menuItemMV { get; set; }
        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment webHost)
        {
            _db = db;
            _webHost = webHost;
            menuItemMV = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem(),
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuitems = await _db.MenuItem.Include(c=>c.Category).Include(s=>s.SubCategory).ToListAsync();
            return View(menuitems);
        }


        //Get - CREATE
        public IActionResult Create()
        {
            return View(menuItemMV);
        }

        //Get - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost()
        {
            menuItemMV.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(menuItemMV);
            }
            _db.MenuItem.Add(menuItemMV.MenuItem);
            await _db.SaveChangesAsync();

            //save image
            string webroot = _webHost.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _db.MenuItem.FindAsync(menuItemMV.MenuItem.Id);
           
            if (files.Count>0)
            {
                //files has been upload
                var upload = Path.Combine(webroot, "images");
                var extention = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(upload,menuItemMV.MenuItem.Id + extention),FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFromDb.Image = @"\images\" + menuItemMV.MenuItem.Id + extention;
            }
            else
            {
                //no file uploaded
                var upload = Path.Combine(webroot, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(upload, webroot + @"\images\" + menuItemMV.MenuItem.Id + ".png");
                menuItemFromDb.Image = @"\images\" + menuItemMV.MenuItem.Id + ".png";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Get - Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            menuItemMV.MenuItem = await _db.MenuItem.Include(c=>c.Category).Include(s=>s.SubCategory).FirstOrDefaultAsync(i=>i.Id==id);
            menuItemMV.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == menuItemMV.MenuItem.CategoryId).ToListAsync();
            
            if (menuItemMV == null)
            {
                return NotFound();
            }

            return View(menuItemMV);
        }

        //Get - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            menuItemMV.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                menuItemMV.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == menuItemMV.MenuItem.CategoryId).ToListAsync();

                return View(menuItemMV);
            }            

            //save image
            string webroot = _webHost.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _db.MenuItem.FindAsync(menuItemMV.MenuItem.Id);

            if (files.Count() > 0)
            {
                //new image has been upload
                var upload = Path.Combine(webroot, "images");
                var extention_new = Path.GetExtension(files[0].FileName);

                //delete old image
                var imagePath = Path.Combine(webroot, menuItemFromDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                //we will upload the new file
                using (var fileStream = new FileStream(Path.Combine(upload, menuItemMV.MenuItem.Id + extention_new), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFromDb.Image = @"\images\" + menuItemMV.MenuItem.Id + extention_new;
            }

            menuItemFromDb.Name = menuItemMV.MenuItem.Name;
            menuItemFromDb.Description = menuItemMV.MenuItem.Description;
            menuItemFromDb.Price = menuItemMV.MenuItem.Price;
            menuItemFromDb.Spicyness = menuItemMV.MenuItem.Spicyness;
            menuItemFromDb.CategoryId = menuItemMV.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = menuItemMV.MenuItem.SubCategoryId;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET -Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            menuItemMV.MenuItem = await _db.MenuItem.Include(c => c.Category).Include(s => s.SubCategory).FirstOrDefaultAsync(i => i.Id == id);
            menuItemMV.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == menuItemMV.MenuItem.CategoryId).ToListAsync();

            if (menuItemMV == null)
            {
                return NotFound();
            }

            return View(menuItemMV);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            MenuItem item = await _db.MenuItem.FindAsync(id);
            string webroot = _webHost.WebRootPath;

            if (item != null)
            {
                //delete old image
                var imagePath = Path.Combine(webroot, item.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.MenuItem.Remove(item);
                await _db.SaveChangesAsync();
            }
           
            return RedirectToAction(nameof(Index));
        }
        //GET -Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            menuItemMV.MenuItem = await _db.MenuItem.Include(c => c.Category).Include(s => s.SubCategory).FirstOrDefaultAsync(i => i.Id == id);
            menuItemMV.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == menuItemMV.MenuItem.CategoryId).ToListAsync();

            if (menuItemMV == null)
            {
                return NotFound();
            }

            return View(menuItemMV);
        }

    }
}
