using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication7.ViewModels; // пространство имен моделей RegisterModel и LoginModel
using TryTwo.Models; // пространство имен UserContext и класса User
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using DAL;
using Entity;
using System;
using System.Net;
using TryTwo.Services;

namespace WebApplication7.Controllers
{
    public class AccountController : Controller
    {
        private DBContext db;
        private readonly ITokenService _tokenService;


        public AccountController(DBContext context, ITokenService tokenService)
        {
            db = context;
            _tokenService = tokenService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            //System.Diagnostics.Debug.WriteLine("I am Account/Login/Get");
            return View();
        }

        private static SHA512 hashAlgo = SHA512.Create();

        private static byte[] GetStringHash(string s)
        {
            if (s == null)
                return null;
            byte[] hash = hashAlgo.ComputeHash(Encoding.Unicode.GetBytes(s));
            Array.Resize(ref hash, 500);
            return hash;
        }

        private static string BytesToStr(byte[] bytes)
        {
            return bytes.ToString();
        }

        [HttpPost]
        public async Task<AuthenticatedResponse> Auth(LoginModel model)
        {
            //System.Diagnostics.Debug.WriteLine("I am Account/Login/Post");
            if (ModelState.IsValid)
            {
                Users user = db.Users.FirstOrDefault(u => u.Name == model.Name);
                if (user.Password.SequenceEqual(
                    GetStringHash((user.Salt != null ? user.Salt.ToString() : "") + model.Password)))
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    var authenticatedTokens = await Authenticate(model.Name, user); // аутентификация
                    HttpContext.Response.Cookies.Append("Access-Token", authenticatedTokens.Token, new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        Expires = DateTime.Now.AddMinutes(10),
                        HttpOnly = true,
                        // every othe options like path , ...
                    });
                    HttpContext.Response.Cookies.Append("Request-Token", authenticatedTokens.RefreshToken, new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        Expires = DateTime.Now.AddMinutes(10),
                        HttpOnly = true,
                        // every othe options like path , ...
                    });
                    return authenticatedTokens;
                }
            }
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return null;
        }

        private void DebugUserOutput()
        {
            foreach (var anotherUser in db.Users)
            {
                string str = "";
                foreach (var byt in anotherUser.Password)
                {
                    str += byt;
                }
                System.Diagnostics.Debug.WriteLine(str);
                System.Diagnostics.Debug.WriteLine(anotherUser.Id);
                System.Diagnostics.Debug.WriteLine(anotherUser.Name);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                Users user = await db.Users.FirstOrDefaultAsync(u => u.Name == model.Name);

                if (user == null)
                {
                    Random rnd = new Random();
                    // + пользователя в бд
                    var salt = new byte[10];
                    rnd.NextBytes(salt);
                    user = new Users
                    {
                        Name = model.Name,
                        Password = GetStringHash(salt.ToString() + model.Password),
                        Salt = salt
                    };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();

                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;
                    var authenticatedTokens = await Authenticate(model.Name, user); // аутентификация
                    return Created("" , authenticatedTokens);

                    // return RedirectToAction("Lobby", "Battleships");
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    ModelState.AddModelError("", "Некорректные логин и(или) пароль"); 
                }
            }
            return View(model);
        }

        private async Task<AuthenticatedResponse> Authenticate(string userName, Users user)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            db.SaveChanges();
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            var authenticatedResponse = new AuthenticatedResponse() { RefreshToken = refreshToken, Token = accessToken };
            return authenticatedResponse;
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
