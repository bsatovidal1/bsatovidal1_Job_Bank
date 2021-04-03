using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using bsatovidal1_Job_Bank.Models;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace bsatovidal1_Job_Bank.Data
{
    public static class JBSeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new JobbBankContext(
                serviceProvider.GetRequiredService<DbContextOptions<JobbBankContext>>()))
            {
                //Prepare Random
                Random random = new Random();

                if (!context.Occupations.Any())
                {
                    context.Occupations.AddRange(
                        new Occupation
                        {
                            Title = "Teacher"
                        },
                        new Occupation
                        {
                            Title = "Nurse"
                        },
                        new Occupation
                        {
                            Title = "Manager"
                        }
                    );

                    context.SaveChanges();
                }

                if(!context.Positions.Any())
                {
                    context.Positions.AddRange(
                        new Position
                        {
                            Name = "Kindergarten Teacher",
                            Description = "Taking care of under 4 years old students, teaching them numbers, english, and developing their creativity",
                            Salary = 60000.00m,
                            OccupationID = context.Occupations.FirstOrDefault(d => d.Title == "Teacher").ID
                        },
                        new Position
                        {
                            Name = "Emergency Room Nurse",
                            Description = "Taking care of pacients and assisting doctors on the emergency room",
                            Salary = 40000.00m,
                            OccupationID = context.Occupations.FirstOrDefault(d => d.Title == "Nurse").ID
                        },
                        new Position
                        {
                            Name = "Store Manager",
                            Description = "Responsible for scheduling workers, opening and/or closing store, cashing out, stock organization",
                            Salary = 50000.00m,
                            OccupationID = context.Occupations.FirstOrDefault(d => d.Title == "Manager").ID
                        }
                        );
                    context.SaveChanges();
                }
                if(!context.Postings.Any())
                {
                    context.Postings.AddRange(
                        new Posting 
                        {
                            NumberOpen = 4,
                            ClosingDate = DateTime.Parse("2020-11-06"),
                            StartDate = DateTime.Parse("2020-09-30"),
                            PositionID = context.Positions.FirstOrDefault(d => d.Name == "Emergency Room Nurse").ID
                        },
                        new Posting
                        {
                            NumberOpen = 3,
                            ClosingDate = DateTime.Parse("2020-10-20"),
                            PositionID = context.Positions.FirstOrDefault(d => d.Name == "Kindergarten Teacher").ID
                        },
                        new Posting
                        {
                            NumberOpen = 1,
                            ClosingDate = DateTime.Parse("2020-12-16"),
                            StartDate = DateTime.Parse("2020-10-12"),
                            PositionID = context.Positions.FirstOrDefault(d => d.Name == "Store Manager").ID
                        }
                        );
                    context.SaveChanges();
                }
                //Random seed data for the Retraining Program.
                string[] training = new string[] { "First Aid", "CPR", "Computer Programming", "Management", "Web Development", "Teaching", "Communication", "Technical Drawing" };
                if (!context.RetrainingPrograms.Any())
                {
                    foreach (string t in training)
                    {
                        RetrainingProgram r = new RetrainingProgram
                        {
                            Name = t
                        };
                        context.RetrainingPrograms.Add(r);
                    }
                    context.SaveChanges();
                }
                if (!context.Applicants.Any())
                {
                    context.Applicants.AddRange(
                        new Applicant
                        {
                            FirstName = "Bruno",
                            MiddleName = "Hikaro",
                            LastName = "Vidal",
                            SIN = "905650578",
                            Phone = 9056589820,
                            Email = "bruno.hsvidal@outlook.com",
                            RetrainingProgramID = context.RetrainingPrograms.FirstOrDefault(t=>t.Name.Contains("Computer")).ID
                        },
                        new Applicant
                        {
                            FirstName = "Tamires",
                            MiddleName = "chagas",
                            LastName = "Simoes",
                            SIN = "905650456",
                            Phone = 9056589830,
                            Email = "tamires.csimoes@gmail.com"
                        }
                        ) ;
                    context.SaveChanges();
                }
                if (!context.Applications.Any())
                {
                    context.Applications.AddRange(
                        new Application
                        {
                            Comment="I would like this position because it mirrors my life goals.",
                            ApplicantID = context.Applicants.FirstOrDefault(d => d.FirstName == "Bruno").ID,
                            PostingID = context.Postings.FirstOrDefault(d => d.Position.Name == "Store Manager").ID
                        },
                        new Application
                        {
                            Comment = "I would like this position because I always wanted to teach little kids.",
                            ApplicantID = context.Applicants.FirstOrDefault(d => d.FirstName == "Tamires").ID,
                            PostingID = context.Postings.FirstOrDefault(d => d.Position.Name == "Kindergarten Teacher").ID
                        }
                        );
                    context.SaveChanges();
                }
                //Random Seed Data.
                string[] skills = new string[] { "Logical Thinking", "Excel", "Team Work", "Leadership", "Communicative", "Proactive", "C#", "Caring", "Focused", "Python", "Back-end" };
                if (!context.Skills.Any())
                {
                    foreach (string k in skills)
                    {
                        Skill s = new Skill
                        {
                            Name = k
                        };
                        context.Skills.Add(s);
                    }
                    context.SaveChanges();
                }
                //Creating collection of the primary keys of Skills
                int[] SkillID = context.Skills.Select(k => k.ID).ToArray();
                int skillIDCount = SkillID.Count();
                //Creating collection of the primary keys of Applicants
                int[] ApplicantID = context.Applicants.Select(a => a.ID).ToArray();
                int applicantIDCount = ApplicantID.Count();

                //Random seed data for ApplicantSkill
                if (!context.ApplicantSkills.Any())
                {
                    for (int i = 0; i < skillIDCount; i++)
                    {
                        ApplicantSkill a = new ApplicantSkill
                        {
                            SkillID = SkillID[i],
                            ApplicantID = ApplicantID[random.Next(applicantIDCount)]
                        };
                        context.ApplicantSkills.Add(a);
                    }
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception)
                    {
                       
                    }
                    
                }

                int[] positionIDs = context.Positions.Select(p => p.ID).ToArray();
                //Seed Data for PositionSkill
                if(!context.PositionSkills.Any())
                {
                    foreach (int i in positionIDs)
                    {
                        int howMany = random.Next(1, 4);
                        howMany = (howMany > skillIDCount) ? skillIDCount : howMany;
                        for (int j = 1; j <= howMany; j++)
                        {
                            PositionSkill ps = new PositionSkill()
                            {
                                PositionID = i,
                                SkillID = SkillID[random.Next(skillIDCount)]
                            };
                            context.PositionSkills.Add(ps);
                            try
                            {
                                context.SaveChanges();
                            }
                            catch(Exception)
                            {

                            }
                        }
                    }
                }
            }
        }
    }
}
