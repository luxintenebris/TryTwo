using DAL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TryTwo.Models;
using TryTwo.Services;

namespace TryTwo.Controllers
{
    public class TokenController : Controller
    {

        private DBContext db;
        private readonly ITokenService _tokenService;

        public TokenController(DBContext context, ITokenService tokenService)
        {
            db = context;
            _tokenService = tokenService;
        }

        [HttpPost]
        public JsonResult Refresh()
        {
            var refreshToken = HttpContext.Request.Cookies.FirstOrDefault(x => x.Key == "Request-Token").Value;
            if (refreshToken is null)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json("Invalid client request");
            }
            var user = db.Users.SingleOrDefault(u => u.RefreshToken == refreshToken);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json("Invalid client request");
            }
            var newAccessToken = _tokenService.GenerateAccessToken(user.Name);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            db.SaveChanges();
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            return Json(new
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost]
        [Route("request")]
        public JsonResult RequestToken()
        {
            var refreshToken = HttpContext.Request.Cookies.FirstOrDefault(x => x.Key == "Request-Token").Value;
            if (refreshToken is null)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json("Invalid client request");
            }
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            return Json(new
            {
                RefreshToken = refreshToken
            });
        }



        [HttpPost]
        public IActionResult Revoke()
        {
            var username = User.Identity.Name;
            var user = db.Users.SingleOrDefault(u => u.Name == username);
            if (user == null) return Forbid();
            user.RefreshToken = null;
            db.SaveChanges();
            return NoContent();
        }

    }
}
