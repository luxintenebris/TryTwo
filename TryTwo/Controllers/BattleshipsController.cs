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
        public class UserIDName
        {
            public int ID { get; set; }
            public string username { get; set; }
        }

        // GET: BattleshipsController/JoinGame/5
        public ActionResult JoinGame(int id)
        {
            int gameID = id;
            System.Diagnostics.Debug.WriteLine("..Joining game.. " + gameID);
            var db = new DBContext();
            var userLogin = User.FindFirstValue(ClaimTypes.Name);
            var user = db.Users.First(x => x.Name == userLogin);
            var openGame = db.OpenGames.FirstOrDefault(x => x.GameID == gameID);
            if (openGame != null && openGame.Player1 != user.Id)
            {
                return StartGame(p1: openGame.Player1, p2: user.Id, openGame);
            }
            return RedirectToAction(nameof(Lobby));
        }

        private ActionResult StartGame(int p1, int p2, OpenGames lobbyRoom)
        {
            // РАСКОММЕНТИРУЙ

            //var db = new DBContext();
            //db.OpenGames.Remove(lobbyRoom);
            //db.SaveChangesAsync();

            return RedirectToAction("CreateGame", "Game", new { player1ID = p1, player2ID = p2 });
            //return RedirectToAction("WaitingRoom", "Game", new { player1ID=p1, player2ID=p2 });
        }

        private ActionResult WaitingRoom()
        {
            return View();
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
            
            return RedirectToAction("WaitingRoom", "Battleships");
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
