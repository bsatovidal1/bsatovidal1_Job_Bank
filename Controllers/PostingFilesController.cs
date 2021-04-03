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
    [Authorize]
    public class PostingFilesController : Controller
    {
        private readonly JobbBankContext _context;

        public PostingFilesController(JobbBankContext context)
        {
            _context = context;
        }

        // GET: PostingFiles
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Index(string SearchString, int? PositionID, int? page, int? pageSizeID)
        {
            PopulateDropDownList();
            ViewData["Filtering"] = "";


            var postingFiles = from p in _context.PostingFiles
                .Include(p => p.Posting)
                    .ThenInclude(p => p.Position)
                select p;

            //Filters
            if (PositionID.HasValue)
            {
                postingFiles = postingFiles.Where(p => p.Posting.PositionID == PositionID);
                ViewData["Filtering"] = "show";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                postingFiles = postingFiles.Where(p => p.FileName.ToUpper().Contains(SearchString.ToUpper()));
                ViewData["Filtering"] = "show";
            }

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

            var pagedData = await PaginatedList<PostingFile>.CreateAsync(postingFiles.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: PostingFiles/Details/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postingFile = await _context.PostingFiles
                .Include(p => p.Posting)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (postingFile == null)
            {
                return NotFound();
            }

            return View(postingFile);
        }

        // GET: PostingFiles/Edit/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postingFile = await _context.PostingFiles.FindAsync(id);
            if (postingFile == null)
            {
                return NotFound();
            }
            ViewData["PostingID"] = new SelectList(_context.Postings, "ID", "ID", postingFile.PostingID);
            return View(postingFile);
        }

        // POST: PostingFiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var postingFileToUpdate = await _context.PostingFiles.SingleOrDefaultAsync(p => p.ID == id);
            if (postingFileToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<PostingFile>(postingFileToUpdate, "", p => p.PostingID,
                p => p.FileName, p => p.Description))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostingFileExists(postingFileToUpdate.ID))
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
            ViewData["PostingID"] = new SelectList(_context.Postings, "ID", "ID", postingFileToUpdate.PostingID);
            return View(postingFileToUpdate);
        }

        // GET: PostingFiles/Delete/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postingFile = await _context.PostingFiles
                .Include(p => p.Posting)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (postingFile == null)
            {
                return NotFound();
            }

            return View(postingFile);
        }

        // POST: PostingFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postingFile = await _context.PostingFiles.FindAsync(id);
            try
            {
                _context.PostingFiles.Remove(postingFile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete Occupation.");
            }
            return View(postingFile);
        }

        private void PopulateDropDownList (PostingFile postingFile = null)
        {
            var pQuery = from p in _context.Positions
                         join pg in _context.Postings on p.ID equals pg.PositionID
                         join pf in _context.PostingFiles on pg.ID equals pf.PostingID
                         orderby p.Name
                         select p;
            ViewData["PositionID"] = new SelectList(pQuery, "ID", "Name", postingFile?.ID);
        }

        private bool PostingFileExists(int id)
        {
            return _context.PostingFiles.Any(e => e.ID == id);
        }
    }
}
