using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using multilib.Models;
using System.Linq;


namespace multilib.Controllers
{
    public class HomeController : Controller
    {
        private LibContext _context;
        public HomeController(LibContext context)
        {
            _context = context;
        }
        // GET: /Home/
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [Route("submit")]
        public IActionResult Register(RegisterUser user)
        {
            if(ModelState.IsValid && _context.Users.Where(a=>a.Email == user.Email).ToList().Count == 0)
            {
                RegisterUser(user);
                return RedirectToAction("Index", "Lib");
            }
            return View("Index");
        }
        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginUser loguser)
        {
            ModelUser foundUser = _context.Users.SingleOrDefault(user=>user.Email == loguser.LogEmail);
            PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
            //user exists
            if((foundUser == null || loguser.LogPassword == null) || hasher.VerifyHashedPassword(loguser, (string)foundUser.Password, loguser.LogPassword) == 0)
            {
                // we are bad
                ModelState.AddModelError("LogEmail", "Invalid Email/Password");
            }
            if(ModelState.IsValid)
            {
                // go somewhere cool.  login user to session
                HttpContext.Session.SetInt32("id", (int)foundUser.id);
                HttpContext.Session.SetString("name", (string)foundUser.FirstName);
                HttpContext.Session.SetString("loggedin", "loggedin");
                return RedirectToAction("Index", "Lib");
            }
            return View("Index");
        }
        [Route("logout")]
        public IActionResult Logoff(){
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        public void RegisterUser(RegisterUser user)
        {
            PasswordHasher<RegisterUser> hasher = new PasswordHasher<RegisterUser>();
            ModelUser newUser = new ModelUser{
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = hasher.HashPassword(user, user.Password)            };
            _context.Users.Add(newUser);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("id", _context.Users.SingleOrDefault(a=>a.Email == newUser.Email).id);
            HttpContext.Session.SetString("name", newUser.FirstName);
            HttpContext.Session.SetString("loggedin", "loggedin");
        } 
    }
}
