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
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authorization;
using bsatovidal1_Job_Bank.ViewModels;

namespace bsatovidal1_Job_Bank.Controllers
{
    [Authorize]
    public class PostingsController : Controller
    {
        private readonly IMyEmailSender _emailSender;
        private readonly JobbBankContext _context;

        public PostingsController(JobbBankContext context, IMyEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Postings
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {
            var jobbBankContext = from p in _context.Postings
                    .Include(p => p.Position)
                    .Include(p => p.PostingFiles)
                    .OrderBy(p => p.ClosingDate)
                    select p;

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

            var pagedData = await PaginatedList<Posting>.CreateAsync(jobbBankContext.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET/POST: Postings/Notification/5
        public async Task<IActionResult> Notification(int? id, string Subject, string emailContent, string position)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["id"] = id;
            ViewData["Position"] = position;

            if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent))
            {
                ViewData["Message"] = "You must enter both a Subject and some message Content before sending the message.";
            }
            else
            {
                int folksCount = 0;
                try
                {
                    //Send a Notice.
                    List<EmailAddress> folks = (from p in _context.Postings
                                                join ap in _context.Applications on p.ID equals ap.PostingID
                                                join a in _context.Applicants on ap.ApplicantID equals a.ID
                                                join ps in _context.Positions on p.PositionID equals ps.ID
                                                where p.ID == id
                                                select new EmailAddress
                                                {
                                                    Name = a.FullName,
                                                    Address = a.Email
                                                }).ToList();
                    folksCount = folks.Count();
                    if (folksCount > 0)
                    {
                        var msg = new EmailMessage()
                        {
                            ToAddresses = folks,
                            Subject = Subject,
                            Content = "<p>" + emailContent + "</p><p>Please access the <strong>Niagara College</strong> web site to review.</p>"

                        };
                        await _emailSender.SendToManyAsync(msg);
                        ViewData["Message"] = "Message sent to " + folksCount + " Applicant"
                            + ((folksCount == 1) ? "." : "s.");
                    }
                    else
                    {
                        ViewData["Message"] = "Message NOT sent!  No Applicants in this Job Posting.";
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.GetBaseException().Message;
                    ViewData["Message"] = "Error: Could not send email message to the " + folksCount + " Applicant"
                        + ((folksCount == 1) ? "" : "s") + " in the posting.";
                }
            }
            return View();
        }

        // GET: Postings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posting = await _context.Postings
                .Include(p => p.Position)
                .Include(p => p.PostingFiles)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (posting == null)
            {
                return NotFound();
            }

            return View(posting);
        }

        // GET: Postings/Create
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Postings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,NumberOpen,ClosingDate,StartDate,PositionID")] Posting posting,
            List<IFormFile> theFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await AddDocumentsAsync(posting, theFiles);
                    _context.Add(posting);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts.");
            }
            catch (DbUpdateException dex)
            {
                if(dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, you cannot have multiple postings for the same position with the same closing date.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            PopulateDropDownLists(posting);
            return View(posting);
        }

        // GET: Postings/Edit/5
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posting = await _context.Postings
                .Include(p => p.PostingFiles)
                .SingleOrDefaultAsync(p => p.ID == id);
            if (posting == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(posting);
            return View(posting);
        }

        // POST: Postings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, List<IFormFile> theFiles)
        {
            var postingToUpdate = await _context.Postings
                .Include(p => p.PostingFiles)
                .SingleOrDefaultAsync(p => p.ID == id);
            if (postingToUpdate ==null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Posting>(postingToUpdate, "",
                d => d.NumberOpen, d => d.ClosingDate, d => d.StartDate, d => d.PositionID))
            {
                try
                {
                    await AddDocumentsAsync(postingToUpdate, theFiles);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostingExists(postingToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts.");
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("", "Unable to save changes. Remember, you cannot have multiple postings for the same position with the same closing date.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes.");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropDownLists(postingToUpdate);
            return View(postingToUpdate);
        }

        // GET: Postings/Delete/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posting = await _context.Postings
                .Include(p => p.Position)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (posting == null)
            {
                return NotFound();
            }

            return View(posting);
        }

        // POST: Postings/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var posting = await _context.Postings.FindAsync(id);
            try
            {
                _context.Postings.Remove(posting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to delete Posting. Remember, you cannot delete a Posting once applications have been submitted.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            return View(posting);
        }

        public FileContentResult Download(int id)
        {
            var theFile = _context.PostingFiles
                .Include(p => p.FileContent)
                .Where(f => f.ID == id)
                .SingleOrDefault();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(Posting posting, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    //Note: you could filter for mime types if you only want to allow
                    //certain types of files.  I am allowing everything.
                    if (!(fileName == "" || fileLength == 0))//Looks like we have a file!!!
                    {
                        PostingFile p = new PostingFile();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            p.FileContent.Content = memoryStream.ToArray();
                        }
                        p.MimeType = mimeType;
                        p.FileName = fileName;
                        posting.PostingFiles.Add(p);
                    };
                }
            }
        }

        private void PopulateDropDownLists(Posting posting = null)
        {
            ViewData["PositionID"] = PositionSelectList(posting?.PositionID);
        }
        private SelectList PositionSelectList(int? id)
        {
            var pQuery = from p in _context.Positions
                         orderby p.Name
                         select p;
            return new SelectList(pQuery, "ID", "Name", id);
        }

        private bool PostingExists(int id)
        {
            return _context.Postings.Any(e => e.ID == id);
        }
    }
}
