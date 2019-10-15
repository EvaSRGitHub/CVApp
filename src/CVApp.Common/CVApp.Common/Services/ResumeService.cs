﻿using CVApp.Common.Repository;
using CVApp.Common.Services.Contracts;
using CVApp.Models;
using CVApp.ViewModels.Resume;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static CVApp.ViewModels.Education.EducationViewModels;
using static CVApp.ViewModels.Language.LanguageViewModels;
using static CVApp.ViewModels.PersonalInfo.PersonalInfoViewModels;
using static CVApp.ViewModels.Skill.SkillViewModels;
using static CVApp.ViewModels.Work.WorkViewModels;


namespace CVApp.Common.Services
{
    public class ResumeService : IResumeService
    {
        private readonly ILogger<ResumeService> logger;
        private readonly IRepository<Resume> resumeRepo;
        private readonly IRepository<CVAppUser> userRepo;

        public ResumeService(ILogger<ResumeService> logger, IRepository<Resume> resumeRepo, IRepository<CVAppUser> userRepo)
        {
            this.logger = logger;
            this.resumeRepo = resumeRepo;
            this.userRepo = userRepo;
        }

        public async Task Delete(int id, string userName)
        {
            var resume = await this.resumeRepo.GetByIdAsync(id);
            var user = await this.userRepo.All().SingleOrDefaultAsync(u => u.UserName == userName);

            if (resume.Id != user.ResumeId)
            {
                throw new InvalidOperationException($"User {userName} doesn't has access to delete resume {resume.Id} id.");
            }

            try
            {
                this.resumeRepo.Delete(resume);
                await this.resumeRepo.SaveChangesAsync();



                await this.CreateResume(user.Id);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }

        public async Task CreateResume(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var resume = new Resume
                {
                    UserId = userId
                };

                await this.resumeRepo.AddAsync(resume);

                try
                {
                    await this.resumeRepo.SaveChangesAsync();

                    var user = await this.userRepo.GetByIdAsync(userId);

                    user.ResumeId = resume.Id;

                    this.userRepo.Update(user);

                    await this.userRepo.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

            }
        }

        public async Task<ResumeDisplayViewModel> DisplayResume(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new InvalidOperationException("Something went wrong.Please try again later.");
            }

            var resume = await this.resumeRepo.All()
                .Include("User")
                .Include("Education")
                .Include("Works")
                .Include("Languages")
                .Include("Skills")
                .AsNoTracking().SingleOrDefaultAsync(u => u.User.UserName == userName);

            if (resume == null)
            {
                throw new NullReferenceException("Something went wrong.Please try again later.");
            }

            ResumeDisplayViewModel model = new ResumeDisplayViewModel();
            model.Id = resume.Id;

            PersonalInfoOutViewModel personalInfo  = CreatePersonalInvoDisplayVM(resume);

            model.PersonalInfo = personalInfo;

            var educationCollection = new List<EducationOutViewModel>();

            if (resume.Education.Count() > 0)
            {
                educationCollection = CreateEducationDisplayVM(resume);
            }

            model.Educations = educationCollection;

            var workCollection = new List<WorkOutViewModel>();

            if (resume.Works.Count() > 0)
            {
                workCollection = CreateWorkDisplayVM(resume);
            }

            model.Employments = workCollection;

            var languageCollection = new List<LanguageOutViewModel>();

            if(resume.Languages.Count() > 0)
            {
                languageCollection = CreateLanguageDisplayVM(resume);
            }

            model.Languages = languageCollection;

            var skillsCollection = new List<SkillOutViewModel>();

            if (resume.Skills.Count() > 0)
            {
                skillsCollection = CreateSkillDisplayVM(resume);
            }

            model.Skills = skillsCollection;

            return model;
        }

        private List<SkillOutViewModel> CreateSkillDisplayVM(Resume resume)
        {
            return resume.Skills.Select(s => new SkillOutViewModel
            {
                Name = s.Name,
                Id = s.Id
            }).ToList();
        }

        private List<LanguageOutViewModel> CreateLanguageDisplayVM(Resume resume)
        {
            return resume.Languages.Select(l => new LanguageOutViewModel
            {
                Name = l.Name,
                Level = l.Level,
                Id = l.Id
            }).ToList();
        }

        private List<WorkOutViewModel> CreateWorkDisplayVM(Resume resume)
        {
            return resume.Works.OrderByDescending(w => w.StartDate).ThenByDescending(w => w.EndDate).Select(w => new WorkOutViewModel
            {
                Company = HttpUtility.HtmlDecode(w.Company),
                Title = HttpUtility.HtmlDecode(w.Title),
                StartDate = w.StartDate.ToString("MM/yyyy"),
                EndDate = w.EndDate.HasValue? w.EndDate.Value.ToString("MM/yyyy") : "",
                Description = HttpUtility.HtmlDecode(w.Description),
                Country = w.Country,
                City = w.City,
                Id = w.Id
            }).ToList();
        }

        private List<EducationOutViewModel> CreateEducationDisplayVM(Resume resume)
        {
            return resume.Education.OrderByDescending(e => e.StartDate).ThenByDescending(e => e.EndDate).Select(e => new EducationOutViewModel
            {
                Institution = HttpUtility.HtmlDecode(e.Institution),
                StartDate = e.StartDate.ToString("MM/yyyy"),
                EndDate = e.EndDate.HasValue ? e.EndDate.Value.ToString("MM/yyyy") : "",
                GPA = e.GPA,
                MainSubjects = HttpUtility.HtmlDecode(e.MainSubjects),
                Diploma = HttpUtility.HtmlDecode(e.Diploma),
                City = e.City,
                Country = e.Country,
                EducationId = e.Id
            }).ToList();
        }

        private PersonalInfoOutViewModel CreatePersonalInvoDisplayVM(Resume resume)
        {
            return new PersonalInfoOutViewModel()
            {
                Id = resume.User.Id,
                Address = resume.User.Address,
                Picture = resume.User.Picture,
                FirstName = resume.User.FirstName,
                LastName = resume.User.LastName,
                Email = resume.User.Email,
                PhoneNumber = resume.User.PhoneNumber,
                DateOfBirth = resume.User.DateOfBirth.HasValue ? resume.User.DateOfBirth.Value.ToShortDateString() : null,
                RepoProfile = resume.User.RepoProfile,
                Summary = HttpUtility.HtmlDecode(resume.User.Summary),
            };
        }
    }
}
