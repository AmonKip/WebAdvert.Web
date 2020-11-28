using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAdvert.Web.Models.Accounts;
using Microsoft.AspNetCore.Identity;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.AspNetCore.Identity.Cognito;

namespace WebAdvert.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public AccountController(
            SignInManager<CognitoUser> signInManager,
            UserManager<CognitoUser> userManager,
            CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }

        [HttpGet]
        public IActionResult Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
        
            var user = _pool.GetUser(model.Email);

            if (user.Status != null)
            {
                ModelState.AddModelError("UserExists", "User with this email already exists");
                return View(model);
            }
            

            user.Attributes.Add(CognitoAttribute.Email.AttributeName, model.Email);
            user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.Email);
            var createdUser = await _userManager.CreateAsync(user, model.Password);

            if(createdUser.Succeeded)
            {
        
                RedirectToAction("Confirm");
            }
        return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> Confirm()
        {
            var model = new ConfirmModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if(!ModelState.IsValid)
            {
                if(user == null)
                {
                    ModelState.AddModelError("Not Found", "A user with the given email was not found");
                    return View(model);
                }
                var result = await _userManager.ConfirmEmailAsync(user, model.Code);

                if(result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach(var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                }
            }
            return View(model);
            
        } 
        
      
        public async Task<IActionResult> Login(LoginModel model) 
        {
            return View(model);
        }

        [ActionName("Login")]
        [HttpPost]
        public async Task<IActionResult> LoginPost(LoginModel model) 
        {
            if(ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email,
                model.Password, model.RememberMe, false).ConfigureAwait(false);

                if(result.Succeeded)
                {
                    Console.WriteLine("Login successful");

                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    ModelState.AddModelError("LogginError", "Email and password don't match.");
                }
            }
            return View("Login", model);

        } 
    }

}
