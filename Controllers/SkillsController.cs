using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bsatovidal1_Job_Bank.Data;
using bsatovidal1_Job_Bank.Models;
using bsatovidal1_Job_Bank.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace bsatovidal1_Job_Bank.Controllers
{
    //Bruno Vidal
    //2020-10-09
    [Authorize]
    public class SkillsController : Controller
    {
        private readonly JobbBankContext _context;

        public SkillsController(JobbBankContext context)
        {
            _context = context;
        }

        // GET: Skills
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {
            //Handle Paging
            int pageSize;//This is the value we will pass to PaginatedList
            if (pageSizeID.HasValue)
            {
                //Value selected from DDL so use and save it to Cookie
                pageSize = pageSizeID.GetValueOrDefault();
                CookieHelper.CookieSet(HttpContext, "pageSizeValue", pageSize.ToString(), 30);
            }
            else
            {
                //Not selected so see if it is in Cookie
                pageSize = Convert.ToInt32(HttpContext.Request.Cookies["pageSizeValue"]);
            }
            pageSize = (pageSize == 0) ? 3 : pageSize;//Neither Selected or in Cookie so go with default
            ViewData["pageSizeID"] =
                new SelectList(new[] { "3", "5", "10", "20", "30", "40", "50", "100", "500" }, pageSize.ToString());

            var pagedData = await PaginatedList<Skill>.CreateAsync(_context
                .Skills
                .OrderBy(s => s.Name)
                .AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Skills/Details/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skills
                .FirstOrDefaultAsync(m => m.ID == id);
            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // GET: Skills/Create
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Skills/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] Skill skill)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(skill);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Skills.Name"))
                {
                    ModelState.AddModelError("Name", "Unable to save changes. You cannot have 2 skills with the same name.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to add new Skill.");
                }
            }
            return View(skill);
        }

        // GET: Skills/Edit/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
            {
                return NotFound();
            }
            return View(skill);
        }

        // POST: Skills/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the Skills to update
            var skillToUpdate = await _context.Skills.SingleOrDefaultAsync(p => p.ID == id);
            if (skillToUpdate == null)
            {
                return NotFound();
            }
            if (await TryUpdateModelAsync<Skill>(skillToUpdate, "",
                s => s.Name))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillExists(skillToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Skills.Name"))
                    {
                        ModelState.AddModelError("Name", "Unable to save changes. You cannot have duplicate of the same position.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes.");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(skillToUpdate);
        }

        // GET: Skills/Delete/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skills
                .FirstOrDefaultAsync(m => m.ID == id);
            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // POST: Skills/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            try
            {
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to delete Skill, You are not alowed to delete a Skill with Applicant Skill assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to delete Skill.");
                }
            }
            return View(skill);
        }

        private bool SkillExists(int id)
        {
            return _context.Skills.Any(e => e.ID == id);
        }
    }
}
