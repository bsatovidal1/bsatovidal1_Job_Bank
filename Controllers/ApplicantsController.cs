using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bsatovidal1_Job_Bank.Data;
using bsatovidal1_Job_Bank.Models;
using bsatovidal1_Job_Bank.ViewModels;
using System.Data.Entity.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using RetryLimitExceededException = Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException;
using DbUpdateException = Microsoft.EntityFrameworkCore.DbUpdateException;
using DbUpdateConcurrencyException = Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;
using bsatovidal1_Job_Bank.Utilities;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace bsatovidal1_Job_Bank.Controllers
{
    [Authorize]
    public class ApplicantsController : Controller
    {
        private readonly JobbBankContext _context;

        public ApplicantsController(JobbBankContext context)
        {
            _context = context;
        }

        // GET: Applicants
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Index(string SearchString, int? PostingID, int? SkillID,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Applicant")
        {
            PopulateDropDownLists();
            ViewData["SkillID"] = new SelectList(_context
                .Skills
                .OrderBy(c => c.Name), "ID", "Name");
            ViewData["Filtering"] = "";

            //Clear the sort/filter/paging URL Cookie
            CookieHelper.CookieSet(HttpContext, "ApplicantsURL", "", -1);

            var applicant = from ap in _context.Applicants
                .Include(a => a.Applications)
                    .ThenInclude(pg => pg.Posting)
                        .ThenInclude(p => p.Position)
                .Include(a => a.ApplicantSkills)
                    .ThenInclude(a => a.Skill)
                .Include(r => r.RetrainingPrograms)
                .Include(a => a.ApplicantPhoto)
                    .ThenInclude(a => a.PhotoContentThumb)
                            select ap;
            //Add as many filters as needed
            if(PostingID.HasValue)
            {
                applicant = applicant.Where(a => a.Applications.Any(p => p.PostingID == PostingID));
                ViewData["Filtering"] = " show";
            }
            if (SkillID.HasValue)
            {
                applicant = applicant.Where(p => p.ApplicantSkills.Any(c => c.SkillID == SkillID));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                applicant = applicant.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.Email.ToUpper().Contains(SearchString.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1; //Reset page to start
                if (actionButton != "Filter")//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by
            if (sortField == "Email")
            {
                if (sortDirection == "asc")
                {
                    applicant = applicant
                        .OrderBy(p => p.Email);
                }
                else
                {
                    applicant = applicant
                        .OrderByDescending(p => p.Email);
                }
            }
            else //Sorting by Applicant Name
            {
                if (sortDirection == "asc")
                {
                    applicant = applicant
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    applicant = applicant
                        .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

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

            var pagedData = await PaginatedList<Applicant>.CreateAsync(applicant.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Applicants/Details/5
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Get the URL with the last filter, sort and page parameters
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Applicants");
            
            var applicant = await _context.Applicants
                .Include(a => a.ApplicantSkills)
                    .ThenInclude(a => a.Skill)
                .Include(r => r.RetrainingPrograms)
                .Include(a => a.ApplicantPhoto)
                    .ThenInclude(a => a.PhotoContentFull)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (applicant == null)
            {
                return NotFound();
            }
            
            return View(applicant);
        }

        // GET: Applicants/Create
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public IActionResult Create()
        {
            //Add all (unchecked) Skills to the ViewBag
            var applicant = new Applicant();
            PopulateAssignedSkillData(applicant);
            PopulateDropDownLists(applicant);
            //Get the URL with the last filter, sort and page parameters
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Applicants");
            return View();
        }

        // POST: Applicants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,SIN,Phone,Email,RetrainingProgramID")] Applicant applicant,
            string[] selectedOptions, IFormFile thePicture)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Applicants");

            try
            {
                //Add the selected skills
                if(selectedOptions != null)
                {
                    foreach (var skill in selectedOptions)
                    {
                        var skillToAdd = new ApplicantSkill { ApplicantID = applicant.ID, SkillID = int.Parse(skill) };
                        applicant.ApplicantSkills.Add(skillToAdd);
                    }
                }
                if (ModelState.IsValid)
                {
                    await AddPicture(applicant, thePicture);
                    _context.Add(applicant);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { applicant.ID });
                }
            }
            catch (RetryLimitExceededException /* dex*/)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts.");
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Email already registered in the system.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            
            //Validation Error so give the user another chance.
            PopulateAssignedSkillData(applicant);
            PopulateDropDownLists(applicant);
            return View(applicant);
        }

        // GET: Applicants/Edit/5
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Get the URL with the last filter, sort and page parameters
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Applicants");

            var applicant = await _context.Applicants
                .Include(a => a.ApplicantSkills)
                    .ThenInclude(a => a.Skill)
                .Include(r => r.RetrainingPrograms)
                .Include(a => a.ApplicantPhoto)
                    .ThenInclude(a => a.PhotoContentFull)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (applicant == null)
            {
                return NotFound();
            }
            
            PopulateAssignedSkillData(applicant);
            PopulateDropDownLists(applicant);
            return View(applicant);
        }

        // POST: Applicants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions,
            string chkRemoveImage, IFormFile thePicture, Byte[] RowVersion)
        {

            //Get the URL with the last filter, sort and page parameters
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Applicants");

            //get applicant to Update
            var applicantToUpdate = await _context.Applicants
                .Include(a => a.ApplicantSkills)
                    .ThenInclude(a => a.Skill)
                .Include(r => r.RetrainingPrograms)
                .Include(a => a.ApplicantPhoto)
                    .ThenInclude(a => a.PhotoContentFull)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (applicantToUpdate == null)
            {
                return NotFound();
            }

            //Update applicant skills
            UpdateApplicantSkill(selectedOptions, applicantToUpdate);

            //Put the original RowVersion value in the OriginalValues collection for the entity
            _context.Entry(applicantToUpdate).Property("RowVersion").OriginalValue = RowVersion;


            if (await TryUpdateModelAsync<Applicant>(applicantToUpdate, "",
                a =>a.FirstName, a=>a.MiddleName, a=>a.LastName,a=>a.SIN, a => a.Phone,
                a => a.Email, a=>a.RetrainingProgramID))
            {
                try
                {
                    //For the Image
                    if (chkRemoveImage != null)
                    {
                        applicantToUpdate.ApplicantPhoto = null;
                    }
                    else
                    {
                        await AddPicture(applicantToUpdate, thePicture);
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { applicantToUpdate.ID });
                }
                catch (RetryLimitExceededException /* dex*/)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts.");
                }
                catch (DbUpdateConcurrencyException ex)// Added for concurrency
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Applicant)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The Applicant was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Applicant)databaseEntry.ToObject();
                        if (databaseValues.FirstName != clientValues.FirstName)
                            ModelState.AddModelError("FirstName", "Current value: "
                                + databaseValues.FirstName);
                        if (databaseValues.MiddleName != clientValues.MiddleName)
                            ModelState.AddModelError("MiddleName", "Current value: "
                                + databaseValues.MiddleName);
                        if (databaseValues.LastName != clientValues.LastName)
                            ModelState.AddModelError("LastName", "Current value: "
                                + databaseValues.LastName);
                        if (databaseValues.SIN != clientValues.SIN)
                            ModelState.AddModelError("SIN", "Current value: "
                                + databaseValues.SIN);
                        if (databaseValues.Phone != clientValues.Phone)
                            ModelState.AddModelError("Phone", "Current value: "
                                + String.Format("{0:(###) ###-####}", databaseValues.Phone));
                        if (databaseValues.Email != clientValues.Email)
                            ModelState.AddModelError("Email", "Current value: "
                                + databaseValues.Email);
                        //A little extra work for the nullable foreign key.  No sense going to the database and asking for something
                        //we already know is not there.
                        if (databaseValues.RetrainingProgramID != clientValues.RetrainingProgramID)
                        {
                            if (databaseValues.RetrainingProgramID.HasValue)
                            {
                                RetrainingProgram databaseRetrainingProgram = await _context.RetrainingPrograms.SingleOrDefaultAsync(i => i.ID == databaseValues.RetrainingProgramID);
                                ModelState.AddModelError("RetrainingProgramID", $"Current value: {databaseRetrainingProgram?.Name}");
                            }
                            else

                            {
                                ModelState.AddModelError("RetrainingProgramID", $"Current value: None");
                            }
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you received your values. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to save your version of this record, click "
                                + "the Save button again. Otherwise click the 'Back to List' hyperlink.");
                        applicantToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("", "Unable to save changes. Email already registered in the system.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes.");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateAssignedSkillData(applicantToUpdate);
            PopulateDropDownLists(applicantToUpdate);
            return View(applicantToUpdate);
        }

        // GET: Applicants/Delete/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Get the URL with the last filter, sort and page parameters
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Applicants");

            var applicant = await _context.Applicants
                .Include(r => r.RetrainingPrograms)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (applicant == null)
            {
                return NotFound();
            }
            
            return View(applicant);
        }

        // POST: Applicants/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Applicants");

            var applicant = await _context.Applicants
                .Include(r => r.RetrainingPrograms)
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                _context.Applicants.Remove(applicant);
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch(DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete Applicant.");
            }
            
            return View(applicant);
        }
        //Posting for the Filter
        private void PopulateDropDownLists(Applicant applicant = null)
        {
            ViewData["PostingID"] = PostingSelectList(applicant?.ID);
            ViewData["RetrainingProgramID"] = RetrainingProgramSelectList(applicant?.RetrainingProgramID);
        }
        private SelectList PostingSelectList(int? id)
        {
            var pQuery = from p in _context.Positions
                         join pg in _context.Postings on p.ID equals pg.PositionID
                         join a in _context.Applications on pg.ID equals a.PostingID
                         join at in _context.Applicants on a.ApplicantID equals at.ID
                         orderby p.Name
                         select p;
            return new SelectList(pQuery, "ID", "Name", id);
        }
        private SelectList RetrainingProgramSelectList(int? id)
        {
            var rQuery = from r in _context.RetrainingPrograms
                   orderby r.Name
                   select r;
            return new SelectList(rQuery, "ID", "Name", id);
        }

        //Code to check if the checkboxes or checked or not.
        private void PopulateAssignedSkillData(Applicant applicant)
        {
            var allOptions = _context.Skills;
            var currentOptionIDs = new HashSet<int>(applicant.ApplicantSkills.Select(b => b.SkillID));
            var checkBoxes = new List<OptionVM>();
            foreach (var option in allOptions)
            {
                checkBoxes.Add(new OptionVM
                {
                    ID = option.ID,
                    DisplayText = option.Name,
                    Assigned = currentOptionIDs.Contains(option.ID)
                });
            }
            ViewData["SkillOptions"] = checkBoxes;
        }
        private void UpdateApplicantSkill(string[] selectedOptions, Applicant applicantToUpdate)
        {
            //if all the checkboxes aren't select, they return no value(null), so a new empty list is created and replace the existing one.
            if (selectedOptions == null)
            {
                applicantToUpdate.ApplicantSkills = new List<ApplicantSkill>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var applicantOptionsHS = new HashSet<int>
                (applicantToUpdate.ApplicantSkills.Select(c => c.SkillID));//IDs of the currently selected conditions
            foreach (var option in _context.Skills)
            {
                if (selectedOptionsHS.Contains(option.ID.ToString()))
                {
                    if (!applicantOptionsHS.Contains(option.ID))
                    {
                        applicantToUpdate.ApplicantSkills.Add(new ApplicantSkill { ApplicantID = applicantToUpdate.ID, SkillID = option.ID });
                    }
                }
                else
                {
                    if (applicantOptionsHS.Contains(option.ID))
                    {
                        ApplicantSkill skillToRemove = applicantToUpdate.ApplicantSkills.SingleOrDefault(c => c.SkillID == option.ID);
                        _context.Remove(skillToRemove);
                    }
                }
            }
        }

        private async Task AddPicture(Applicant applicant, IFormFile thePicture)
        {
            //get the picture and save it with the Applicant
            if (thePicture != null)
            {
                string mimeType = thePicture.ContentType;
                long fileLength = thePicture.Length;
                if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                {
                    if (mimeType.Contains("image"))
                    {
                        ApplicantPhoto p = new ApplicantPhoto
                        {
                            FileName = Path.GetFileName(thePicture.FileName)
                        };
                        using (var memoryStream = new MemoryStream())
                        {
                            await thePicture.CopyToAsync(memoryStream);
                            p.PhotoContentFull.Content = memoryStream.ToArray();
                            p.PhotoContentFull.MimeType = mimeType;
                            p.PhotoContentThumb.Content = ResizeImage.shrinkImagePNG(p.PhotoContentFull.Content);
                            p.PhotoContentThumb.MimeType = "image/png";
                        }
                        applicant.ApplicantPhoto = p;
                    }
                }
            }
        }

        private bool ApplicantExists(int id)
        {
            return _context.Applicants.Any(e => e.ID == id);
        }
    }
}
