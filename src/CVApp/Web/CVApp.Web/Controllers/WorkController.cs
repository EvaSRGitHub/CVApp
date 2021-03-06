﻿using CVApp.Common.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CVApp.ViewModels.Work.WorkViewModels;

namespace CVApp.Web.Controllers
{
    public class WorkController : Controller
    {
        private readonly IWorkService workService;
        private readonly ILogger<WorkController> logger;
        private readonly IHttpContextAccessor accessor;
        private readonly string userName;

        public WorkController(IWorkService workService, ILogger<WorkController> logger, IHttpContextAccessor accessor)
        {
            this.workService = workService;
            this.logger = logger;
            this.accessor = accessor;
            this.userName = this.accessor.HttpContext.User.Identity.Name;
        }

        public IActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(WorkInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.View(model);
            }

            int id = default(int);
            try
            {
                id = await this.workService.SaveFormData(model, userName);
            }
            catch (Exception e)
            {
                this.logger.LogDebug(e, $"An exception happened for user {userName}");
                return this.BadRequest();
            }
           
            return this.Redirect(Url.RouteUrl(new { controller = "Resume", action = "Display" }) + $"#{id}");
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                this.logger.LogDebug($"User {userName} tries to edit work info with negative {id} id.");
                return this.NotFound();
            }

            WorkEditViewModel model;

             try
            {
                model = await this.workService.EditDeleteForm(id, userName);
            }
            catch (Exception e)
            {
                this.logger.LogDebug(e, $"User {userName} tries to edit someone else work model.");
                return this.NotFound();
            }

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(WorkEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                await this.workService.Update(model);
            }
            catch (Exception e)
            {
                this.logger.LogDebug(e, $"An exception happened for user {userName}");
                return this.BadRequest();
            }

            return this.Redirect(Url.RouteUrl(new { controller = "Resume", action = "Display" }) + $"#{model.Id}");
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                this.logger.LogDebug($"User {userName} tries to delete work info with negative {id} id.");
                return this.NotFound();
            }

            WorkEditViewModel model;

            try
            {
                model = await this.workService.EditDeleteForm(id, userName);
            }
            catch (Exception e)
            {
                this.logger.LogDebug(e, $"User {userName} tries to delete null work model.");
                return this.NotFound();
            }

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(WorkEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                await this.workService.Delete(model);
            }
            catch (Exception e)
            {
                this.logger.LogDebug(e, $"An exception happened for user {userName}");
                return this.BadRequest();
            }

            return this.Redirect(Url.RouteUrl(new { controller = "Resume", action = "Display" }) + "#work");
        }

        public async Task<IActionResult> Display(int id)
        {
            IEnumerable<WorkOutViewModel> workInfo;

            try
            {
                workInfo = await this.workService.GetWorkInfo(id);

                if (workInfo == null)
                {
                    this.logger.LogDebug($"Can't display work info for user {this.userName}.");
                    return this.NotFound();
                }
            }
            catch (Exception e)
            {
                this.logger.LogDebug(e, $"An exception happened for user {this.userName}");
                return this.BadRequest();
            }

            return this.View(workInfo);
        }
    }
}
