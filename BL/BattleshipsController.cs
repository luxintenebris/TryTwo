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

            return RedirectToAction("CreateGame", "Game", new { player1ID = p1, player2ID = p2, roomID = lobbyRoom.GameID });
            //return RedirectToAction("WaitingRoom", "Game", new { player1ID=p1, player2ID=p2 });
        }

        public ActionResult WaitingRoom(int openGameID, int playerID)
        {
            System.Diagnostics.Debug.WriteLine("Waiting in room " + openGameID);
            ViewBag.lobbyID = openGameID;
            ViewBag.playerID = playerID;
            return View();
        }

        public async Task<ActionResult> CreateGame()
        {
            var db = new DBContext();

            var userLogin = User.FindFirstValue(ClaimTypes.Name);
            var user = db.Users.First(x => x.Name == userLogin);

            int openRoomID;
            var sameGame = db.OpenGames.FirstOrDefault(
                x => x.Player1 == user.Id && x.sessionID == -1);
            if (sameGame == null)
            {
                var room = db.OpenGames.Add(new OpenGames { Player1 = user.Id });
                await db.SaveChangesAsync();

                openRoomID = room.Entity.GameID;
                System.Diagnostics.Debug.WriteLine($"..Room {openRoomID} created..");
            }
            else
            {
                openRoomID = sameGame.GameID;
                System.Diagnostics.Debug.WriteLine($"..Room {openRoomID} found..");
            }
            
            return RedirectToAction("WaitingRoom", "Battleships", new { openGameID = openRoomID, playerID = user.Id });
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
                                         { GameID = g.GameID, Player1Name = u.Name, sessionID = g.sessionID });
            return openGamesWithUsers.OrderBy(x => x.GameID)
                .Where(x => x.sessionID == -1)
                .ToList();
        }

        public int ActualGameID(int lobbyID)
        {
            var db = new DBContext();
            var openGames = db.OpenGames;
            var openGame = openGames.FirstOrDefault(x => x.GameID == lobbyID);
            return openGame != null ? openGame.sessionID : -1;
        }

        public ActionResult Lobby()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Account");
            }

            var username = User.FindFirstValue(ClaimTypes.Name);
            ViewBag.username = username;
            var db = new DBContext();
            var user = db.Users.First(x => x.Name == username);
            ViewBag.wins = user.WinCount;
            ViewBag.loss = user.LoseCount;

            return View();
        }
    }
}
