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
    public class ApplicationsController : Controller
    {
        private readonly JobbBankContext _context;

        public ApplicationsController(JobbBankContext context)
        {
            _context = context;
        }

        // GET: Applications
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Index()
        {
            //the Include and Then Include are being used to create a path through a series of related tables.
            var jobbBankContext = _context.Applications.Include(a => a.Applicant)
                .Include(a => a.Posting)
                    .ThenInclude(a => a.Position);
            return View(await jobbBankContext.ToListAsync());
        }

        // GET: Applications/Details/5
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Posting)
                    .ThenInclude(a => a.Position)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        // GET: Applications/Create
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public IActionResult Create()
        {
            PopulateDropDownList();
            return View();
        }

        // POST: Applications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Comment,PostingID,ApplicantID")] Application application)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(application);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Applicant cannot apply twice for the same position.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            PopulateDropDownList(application);
            return View(application);
        }

        // GET: Applications/Edit/5
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }
            PopulateDropDownList(application);
            return View(application);
        }

        // POST: Applications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var applicationToUpdate = await _context.Applications.SingleOrDefaultAsync(a => a.ID == id);

            if (applicationToUpdate == null)
            {
                return NotFound();
            }
            
            if (await TryUpdateModelAsync<Application>(applicationToUpdate, "",
                a=>a.Comment))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationExists(applicationToUpdate.ID))
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
                        ModelState.AddModelError("", "Unable to save changes. Applicant cannot apply twice for the same position.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes.");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropDownList(applicationToUpdate);
            return View(applicationToUpdate);
        }

        // GET: Applications/Delete/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Posting)
                    .ThenInclude(a => a.Position)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        // POST: Applications/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            try
            {
                _context.Applications.Remove(application);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Unable to delete Application.");
            }
            return View(application);
        }
        
        private void PopulateDropDownList(Application application = null)
        {
            ViewData["ApplicantID"] = ApplicantSelectList(application?.ApplicantID);
            ViewData["PostingID"] = PostingSelectList(application?.PostingID);
        }
        private SelectList ApplicantSelectList(int? id)
        {
            var aQuery = from a in _context.Applicants
                         orderby a.LastName, a.FirstName
                         select a;
            return new SelectList(aQuery, "ID", "FormalName", id);
        }
        private SelectList PostingSelectList(int? id)
        {
            var pQuery = from p in _context.Postings
                            .Include(b => b.Position)
                         orderby p.Position.Name
                         select p;
            return new SelectList(pQuery, "ID", "PostingSummary", id);
        }

        private bool ApplicationExists(int id)
        {
            return _context.Applications.Any(e => e.ID == id);
        }
    }
}
