using DAL;
using Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace TryTwo.Controllers
{
    public class BattleshipsController : Controller
    {        
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
            
        }

        public class UserIDName
        {
            public int ID { get; set; }
            public string username { get; set; }
        }

        public async Task<ActionResult> CreateGame()
        {
            var db = new DBContext();

            var userLogin = User.FindFirstValue(ClaimTypes.Name);
            var user = db.Users.First(x => x.Name == userLogin);

            var sameGame = db.OpenGames.FirstOrDefault(x => x.Player1 == user.Id);
            if (sameGame == null)
            {
                db.OpenGames.Add(new OpenGames { Player1 = user.Id, Started = false });
                await db.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("..Game created..");
            }
            
            return RedirectToAction(nameof(Lobby));
        }

        public UserIDName CurrentUser()
        {
            return new UserIDName
            {
                ID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                username = User.FindFirstValue(ClaimTypes.Name)
            };
        }

        public List<OpenGamesWithPlayer> LobbyGames()
        {
            var db = new DBContext();
            var openGames = db.OpenGames;
            var users = db.Users;
            var openGamesWithUsers =
                openGames.Join(users, x => x.Player1, y => y.Id,
                               (g, u) => new OpenGamesWithPlayer 
                                         { GameID = g.GameID, Player1Name = u.Name, Started = g.Started });
            return openGamesWithUsers.OrderBy(x => x.GameID).Where(x => !x.Started).ToList();
        }

        public ActionResult Lobby()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Account");
            }

            ViewBag.username = User.FindFirstValue(ClaimTypes.Name);
            return View();
        }

        // GET: BattleshipsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BattleshipsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BattleshipsController/Create
        [HttpPost]
        public ActionResult Create(IFormCollection collection)
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

        // GET: BattleshipsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BattleshipsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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
