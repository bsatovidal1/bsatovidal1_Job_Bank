using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bsatovidal1_Job_Bank.Data;
using bsatovidal1_Job_Bank.Models;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.AspNetCore.Authorization;

namespace bsatovidal1_Job_Bank.Controllers
{
    [Authorize]
    public class OccupationsController : Controller
    {
        private readonly JobbBankContext _context;

        public OccupationsController(JobbBankContext context)
        {
            _context = context;
        }

        // GET: Occupations
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Occupations.ToListAsync());
        }

        // GET: Occupations/Details/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var occupation = await _context.Occupations
                .Include(o=>o.Positions)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (occupation == null)
            {
                return NotFound();
            }

            return View(occupation);
        }

        // GET: Occupations/Create
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Occupations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title")] Occupation occupation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(occupation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes.");
            }
            return View(occupation);
        }

        // GET: Occupations/Edit/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var occupation = await _context.Occupations.FindAsync(id);
            if (occupation == null)
            {
                return NotFound();
            }
            return View(occupation);
        }

        // POST: Occupations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var occupationToUpdate = await _context.Occupations.SingleOrDefaultAsync(p => p.ID == id);

            if (occupationToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Occupation>(occupationToUpdate, "",
                o => o.Title))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OccupationExists(occupationToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(occupationToUpdate);
        }

        // GET: Occupations/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var occupation = await _context.Occupations
                .FirstOrDefaultAsync(m => m.ID == id);
            if (occupation == null)
            {
                return NotFound();
            }

            return View(occupation);
        }

        // POST: Occupations/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var occupation = await _context.Occupations.FindAsync(id);
            try
            {
                _context.Occupations.Remove(occupation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch(DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to delete Occupation, You are not alowed to delete an Occupation with Positions assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to delete Occupation.");
                }
            }
            return View(occupation);
        }

        private bool OccupationExists(int id)
        {
            return _context.Occupations.Any(e => e.ID == id);
        }
    }
}
