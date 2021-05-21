using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]

    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string statusMessage { get; set; }
        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        //GET-Sub Category
        public async Task<IActionResult> Index()
        {
            var _sub = await _db.SubCategory.Include(s=>s.Category).ToListAsync();
            return View(_sub);
        }

        //GET-Sub Category Create
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel() {

                
                CategoryList = await _db.Category.ToListAsync(),

                //category
                SubCategory = new Models.SubCategory(),

                //sub category list name
                SubCategoryList = await _db.SubCategory.OrderBy(s=>s.Name).Select(s=>s.Name).Distinct().ToListAsync(),
            
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExists.Count() > 0)
                {
                    statusMessage = "Error : Sub Category exists under "+ doesSubCategoryExists.First().Category.Name + " category. Please use another name.";
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                    
                }
                
            }

            SubCategoryAndCategoryViewModel viewModel = new SubCategoryAndCategoryViewModel()
            {

                CategoryList = await _db.Category.ToListAsync(),

                //category
                SubCategory = model.SubCategory,

                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync(),

                StatusMessage = statusMessage,
            };

            return View(viewModel);
        }


        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();

            subCategories = await (from Sub in _db.SubCategory
                                   where Sub.CategoryId == id
                                   select Sub).ToListAsync();
            return Json(new SelectList( 
                subCategories,"Id","Name")
            );
        }


        //GET-Sub Category Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }

            var sub = await _db.SubCategory.SingleOrDefaultAsync(s=>s.Id == id);
            if (sub == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {


                CategoryList = await _db.Category.ToListAsync(),

                //category
                SubCategory = sub,

                //sub category list name
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).Distinct().ToListAsync(),

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExists.Count() > 0)
                {
                    statusMessage = "Error : Sub Category exists under " + doesSubCategoryExists.First().Category.Name + " category. Please use another name.";
                }
                else
                {

                    var subUpdate = await _db.SubCategory.FindAsync(model.SubCategory.Id);
                    subUpdate.Name = model.SubCategory.Name;

                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));

                }

            }

            SubCategoryAndCategoryViewModel viewModel = new SubCategoryAndCategoryViewModel()
            {

                CategoryList = await _db.Category.ToListAsync(),

                //category
                SubCategory = model.SubCategory,

                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync(),

                StatusMessage = statusMessage,
            };

            return View(viewModel);
        }

        //GET-Sub Category Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subcategory = await _db.SubCategory.Include(s=>s.Category).FirstOrDefaultAsync(s=>s.Id==id);
            if (subcategory == null)
            {
                return NotFound();
            }
            
            return View(subcategory);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sc =  await _db.SubCategory.SingleOrDefaultAsync(s => s.Id == id);
            _db.SubCategory.Remove(sc);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sub = await _db.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(s=>s.Id == id);
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {


                CategoryList = await _db.Category.ToListAsync(),

                //category
                SubCategory = sub,

                //sub category list name
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).Distinct().ToListAsync(),

            };

            return View(model);
        }
    }
}
