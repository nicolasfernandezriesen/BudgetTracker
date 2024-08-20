﻿using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly BudgettrackerdbContext context;
        private readonly IDataProtector _protector;

        public UserController(BudgettrackerdbContext context, IDataProtectionProvider provider)
        {
            this.context = context;
            _protector = provider.CreateProtector("UserIdProtector");
        }

        // GET: UserController
        public ActionResult Index()
        {
            return View("Home");
        }

        // GET: UserController/GetUsers
        public IActionResult GetUsers()
        {
            var users = context.Users.ToList();
            return Ok(users);
        }

        private int GetUserID()
        {
            var encryptedUserId = Request.Cookies["UserId"];
            var stringId = _protector.Unprotect(encryptedUserId);
            return int.Parse(stringId);
        }

        // GET: UserController/Edit
        public ActionResult Edit()
        {
            try
            {
                int userId = GetUserID();
                var user = context.Users.Find(userId);

                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            try
            {
                int userId = GetUserID();
                var existingUser = context.Users.Find(userId);

                if (existingUser == null)
                {
                    return NotFound();
                }

                // Update the user properties
                existingUser.UserName = user.UserName;
                existingUser.UserEmail = user.UserEmail;

                // Check if the password is provided
                if (!string.IsNullOrEmpty(user.UserPassword))
                {
                    existingUser.UserPassword = user.UserPassword;
                }

                // Save the changes to the database
                await context.SaveChangesAsync();

                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
