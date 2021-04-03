using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bsatovidal1_Job_Bank.Data;
using bsatovidal1_Job_Bank.Models;
using Microsoft.AspNetCore.Authorization;

namespace bsatovidal1_Job_Bank.Controllers
{
    [Authorize]
    public class RetrainingProgramsController : Controller
    {
        private readonly JobbBankContext _context;

        public RetrainingProgramsController(JobbBankContext context)
        {
            _context = context;
        }

        // GET: RetrainingPrograms
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.RetrainingPrograms.ToListAsync());
        }

        // GET: RetrainingPrograms/Details/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var retrainingProgram = await _context.RetrainingPrograms
                .FirstOrDefaultAsync(m => m.ID == id);
            if (retrainingProgram == null)
            {
                return NotFound();
            }

            return View(retrainingProgram);
        }

        // GET: RetrainingPrograms/Create
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: RetrainingPrograms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] RetrainingProgram retrainingProgram)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(retrainingProgram);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Program already registered in the system.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            return View(retrainingProgram);
        }

        // GET: RetrainingPrograms/Edit/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var retrainingProgram = await _context.RetrainingPrograms.FindAsync(id);
            if (retrainingProgram == null)
            {
                return NotFound();
            }
            return View(retrainingProgram);
        }

        // POST: RetrainingPrograms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var rProgramToUpdate = await _context.RetrainingPrograms
                .FirstOrDefaultAsync(m => m.ID == id);
            if (rProgramToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<RetrainingProgram>(rProgramToUpdate, "",
                a => a.Name))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RetrainingProgramExists(rProgramToUpdate.ID))
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
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("", "Unable to save changes. Program already registered in the system.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes.");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(rProgramToUpdate);
        }

        // GET: RetrainingPrograms/Delete/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var retrainingProgram = await _context.RetrainingPrograms
                .FirstOrDefaultAsync(m => m.ID == id);
            if (retrainingProgram == null)
            {
                return NotFound();
            }

            return View(retrainingProgram);
        }

        // POST: RetrainingPrograms/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var retrainingProgram = await _context.RetrainingPrograms.FindAsync(id);
            try
            {
                _context.RetrainingPrograms.Remove(retrainingProgram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to delete Retraining Program, You are not alowed to delete a Program with Applicant assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to delete Retraining Program.");
                }
            }
            return View(retrainingProgram);
        }

        private bool RetrainingProgramExists(int id)
        {
            return _context.RetrainingPrograms.Any(e => e.ID == id);
        }
    }
}
