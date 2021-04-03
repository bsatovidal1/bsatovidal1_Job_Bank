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
using Microsoft.EntityFrameworkCore.Storage;
using bsatovidal1_Job_Bank.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace bsatovidal1_Job_Bank.Controllers
{
    [Authorize]
    public class PositionsController : Controller
    {
        private readonly JobbBankContext _context;

        public PositionsController(JobbBankContext context)
        {
            _context = context;
        }

        // GET: Positions
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {
            var jobbBankContext = from p in _context.Positions
                                    .Include(p => p.Occupation)
                                    .Include(p => p.PositionSkills)
                                        .ThenInclude(ps => ps.Skill)
                                    .OrderBy(p => p.Name)
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

            var pagedData = await PaginatedList<Position>.CreateAsync(jobbBankContext.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Positions/Details/5
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .Include(p => p.Occupation)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (position == null)
            {
                return NotFound();
            }

            return View(position);
        }

        // GET: Positions/Create
        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult Create()
        {
            PopulateDropDownList();
            Position position = new Position();
            PopulateAssignedSkillData(position);
            return View();
        }

        // POST: Positions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Description,Salary,OccupationID")] Position position, string[] selectedOptions)
        {
            try
            {
                UpdatePositionSkill(selectedOptions, position);
                if (ModelState.IsValid)
                {
                    _context.Add(position);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please try again.");
            }
            catch (DbUpdateException dex)
            {
                if(dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Positions.Name"))
                {
                    ModelState.AddModelError("Name", "Unable to save changes. You cannot have 2 of the same position.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            PopulateDropDownList(position);
            PopulateAssignedSkillData(position);
            return View(position);
        }

        // GET: Positions/Edit/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .Include(p => p.PositionSkills)
                .ThenInclude(p => p.Skill)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.ID == id);
            if (position == null)
            {
                return NotFound();
            }
            PopulateDropDownList(position);
            PopulateAssignedSkillData(position);
            return View(position);
        }

        // POST: Positions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions)
        {
            //Go get the Positions to update
            var positionToUpdate = await _context.Positions
                .Include(p => p.PositionSkills)
                .ThenInclude(p => p.Skill)
                .SingleOrDefaultAsync(p => p.ID == id);
            //Check that you got it or exit with a not found error
            if (positionToUpdate == null)
            {
                return NotFound();
            }

            //Update Position's Skills
            UpdatePositionSkill(selectedOptions, positionToUpdate);

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Position>(positionToUpdate, "",
                p => p.Name, p => p.Description, p => p.Salary, p => p.OccupationID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please try again.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PositionExists(positionToUpdate.ID))
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
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Positions.Name"))
                    {
                        ModelState.AddModelError("Name", "Unable to save changes. You cannot have duplicate of the same position.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes.");
                    }
                }
            }
            //Validaiton Error so give the user another chance.
            PopulateDropDownList(positionToUpdate);
            PopulateAssignedSkillData(positionToUpdate);
            return View(positionToUpdate);
        }

        // GET: Positions/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .Include(p => p.Occupation)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (position == null)
            {
                return NotFound();
            }

            return View(position);
        }

        // POST: Positions/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            try
            {
                _context.Positions.Remove(position);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to delete Position, You are not alowed to delete a Position with Postings assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to delete Position.");
                }
            }
            
            return View(position);
        }
       
        private void PopulateDropDownList(Position position = null)
        {
            ViewData["OccupationID"] = OccupationSelectList(position?.OccupationID);
        }

        private SelectList OccupationSelectList(int? id)
        {
            var oQuery = from o in _context.Occupations
                   orderby o.Title
                   select o;
            return new SelectList(oQuery, "ID", "Title", id);
        }

        private void PopulateAssignedSkillData(Position position)
        {
            var allOptions = _context.Skills;
            var currentOptionsHS = new HashSet<int>(position.PositionSkills.Select(p => p.SkillID));
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var s in allOptions)
            {
                if (currentOptionsHS.Contains(s.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.Name
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.Name
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdatePositionSkill(string[] selectedOptions, Position positionToUpdate)
        {
            if (selectedOptions == null)
            {
                positionToUpdate.PositionSkills = new List<PositionSkill>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(positionToUpdate.PositionSkills.Select(b => b.SkillID));
            foreach (var s in _context.Skills)
            {
                if (selectedOptionsHS.Contains(s.ID.ToString()))
                {
                    if (!currentOptionsHS.Contains(s.ID))
                    {
                        positionToUpdate.PositionSkills.Add(new PositionSkill
                        {
                            SkillID = s.ID,
                            PositionID = positionToUpdate.ID
                        });
                    }
                }
                else
                {
                    if (currentOptionsHS.Contains(s.ID))
                    {
                        PositionSkill posiToRemove = positionToUpdate.PositionSkills.SingleOrDefault(d => d.SkillID == s.ID);
                        _context.Remove(posiToRemove);
                    }
                }
            }
        }
        private bool PositionExists(int id)
        {
            return _context.Positions.Any(e => e.ID == id);
        }
    }
}
