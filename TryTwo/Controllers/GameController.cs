using DAL;
using Entity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TryTwo.Models
{
    public class GameController : Controller
    {
        public async Task<ActionResult> Index(int playerID = -1, int sessionID = -1)
        {
            System.Diagnostics.Debug.WriteLine("Player/Session: " + playerID + "/" + sessionID);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Account");
            }
            else if (playerID == -1 || sessionID == -1)
            {
                return RedirectToAction("Lobby", "Battleships");
            }
            else
            {
                ViewBag.sessionId = sessionID;
                ViewBag.playerId = playerID;
                return View();
            }
        }

        public async Task<ActionResult> JoinHost(int playerID, int sessionID)
        {
            var db = new DBContext();
            var session = db.GameSessions.FirstOrDefault(x => x.sessionId == sessionID);
            session.started = true;
            db.GameSessions.Update(session);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Game", new { playerID = playerID, sessionID = sessionID });
        }

        public async Task<ActionResult> CreateGame(int player1ID, int player2ID, int roomID)
        {
            var db = new DBContext();
            int pointsToWin = HitsToWin();
            var gameSession = new GameSession()
            {
                player1 = player1ID,
                player2 = player2ID,
                p1HitsForWin = pointsToWin,
                p2HitsForWin = pointsToWin,
            };
            var gameSessionEntry = db.GameSessions.Add(gameSession);
            await db.SaveChangesAsync();

            var sessionID = gameSessionEntry.Entity.sessionId;

            var lobbyRoom = db.OpenGames.First(x => x.GameID == roomID);
            lobbyRoom.sessionID = sessionID;
            db.OpenGames.Update(lobbyRoom);
            await db.SaveChangesAsync();
            
            var p1Map = CreateRandomShipMap();
            AddShipsToDatabase(p1Map, player1ID, sessionID);
            var p2Map = CreateRandomShipMap();
            AddShipsToDatabase(p2Map, player2ID, sessionID);

            return RedirectToAction("Index", new { playerID = player2ID, sessionID = sessionID });
        }

        public List<PlayerShips> GetShips(int playerID, int sessionID)
        {
            var db = new DBContext();
            return db.PlayerShips.Where(
                x => x.player == playerID && x.gameSessionId == sessionID
                ).ToList();
        }

        [HttpPost]
        public async Task<bool> Shoot([FromBody] SessionShotPayload shot)
        {
            var db = new DBContext();
            var userLogin = User.FindFirstValue(ClaimTypes.Name);
            var user = db.Users.First(x => x.Name == userLogin);

            var session = db.GameSessions.FirstOrDefault(x => x.sessionId == shot.sessionID);
            //System.Diagnostics.Debug.WriteLine(
            //    $"Player {} in {shot.sessionID} (x={shot.x},y={shot.y})"
            //    );
            // Проверка, что такая игровая сессия существует, игрок действительно 
            // тот, за кого себя выдает, и сейчас ход этого игрока
            if (!MyTurn(session, user.Id)) return false;
            int enemyID = 0;
            if (session.playerTurn == 1)
            {
                session.lastP1HitX = shot.x;
                session.lastP1HitY = shot.y;
                enemyID = session.player2;
            }
            else
            {
                session.lastP2HitX = shot.x;
                session.lastP2HitY = shot.y;
                enemyID = session.player1;
            }

            var shipDamaged = db.PlayerShips.FirstOrDefault(
                ship => ship.gameSessionId == session.sessionId
                        && ship.x == shot.x && ship.y == shot.y
                        && !ship.hit && ship.player == enemyID);
            if (shipDamaged != null)
            {
                shipDamaged.hit = true;
                if (session.playerTurn == 1) session.p1Hits++;
                if (session.playerTurn == 2) session.p1Hits++;

                db.PlayerShips.Update(shipDamaged);

                WinCheck(session);
            }
            else // нового попадания в корабль не было
            {
                session.playerTurn = (session.playerTurn % 2) + 1;
            }
            db.GameSessions.Update(session);
            await db.SaveChangesAsync();

            return shipDamaged != null;
        }

        public bool MyTurn(int sessionID, int userID)
        {
            var db = new DBContext();
            var session = db.GameSessions.FirstOrDefault(x => x.sessionId == sessionID);
            return MyTurn(session, userID);
        }

        private bool MyTurn(GameSession session, int userID)
        {
            return session != null
                   && ((session.playerTurn == 1 && userID == session.player1)
                      || (session.playerTurn == 2 && userID == session.player2));
        }

        public bool GetWinStatus(int sessionID, int playerID)
        {
            var db = new DBContext();
            var session = db.GameSessions.First(
                x => x.sessionId == sessionID);
            return session.finished && session.winner == playerID;
        }

        private async void WinCheck(GameSession session)
        {
            int winner = -1;
            winner = session.p1Hits >= session.p1HitsForWin ? session.player1 : winner;
            winner = session.p2Hits >= session.p2HitsForWin ? session.player2 : winner;

            if (winner != -1 && session.winner == -1)
            {
                session.winner = winner;
                session.finished = true;
                var db = new DBContext();
                var winnerUser = db.Users.First(x => x.Id == winner);
                winnerUser.WinCount += 1;
                db.GameSessions.Update(session);
                db.Users.Update(winnerUser);

                var loserID = winner != session.player1 ? session.player1 : session.player2;
                var loserUser = db.Users.First(x => x.Id == loserID);
                loserUser.LoseCount += 1;
                db.Users.Update(loserUser);
                await db.SaveChangesAsync();
            }
        }

        public GameSession GetSessionInfo(int sessionID)
        {
            var db = new DBContext();
            return db.GameSessions.FirstOrDefault(x => x.sessionId == sessionID);
        }

        private const int CELL_EMPTY = 0;
        private const int CELL_WITH_SHIP = 1;

        public class ShipConfig {
            public int maxShips;
            public int pointCount;
            public ShipConfig(int maxShips, int pointCount)
            {
                this.maxShips = maxShips;
                this.pointCount = pointCount;
            }
        }

        private int fieldW = 10;
        private int fieldH = 10;
        public List<ShipConfig> CreateShipConfiguration()
        {
            return new List<ShipConfig>
            {
                new ShipConfig(1, 4),
                new ShipConfig(2, 3),
                new ShipConfig(3, 2),
                new ShipConfig(4, 1)
            };
        }
        
        private int[,] CreateRandomShipMap()
        {
            var shipConfiguration = CreateShipConfiguration();

            int[,] map = new int[10, 10];
            var allShipsPlaced = false;
            var random = new Random();
            while (allShipsPlaced == false)
            {
                var xPoint = random.Next(fieldW);
                var yPoint = random.Next(fieldH);
                //System.Diagnostics.Debug.WriteLine($"For ship {shipConfiguration[0].pointCount} try point {xPoint} {yPoint}");
                if (IsPointFree(map, xPoint, yPoint))
                {
                    if (CanPutHorizontal(map, xPoint, yPoint, shipConfiguration[0].pointCount, fieldW))
                    {
                        for (int i = 0; i < shipConfiguration[0].pointCount; i++)
                        {
                            map[yPoint, xPoint + i] = CELL_WITH_SHIP;
                        }
                    }
                    else if (CanPutVertical(map, xPoint, yPoint, shipConfiguration[0].pointCount, fieldH))
                    {
                        for (int i = 0; i < shipConfiguration[0].pointCount; i++)
                        {
                            map[yPoint + i, xPoint] = CELL_WITH_SHIP;
                        }
                    }
                    else
                    {
                        continue;
                    }
                    shipConfiguration[0].maxShips--;
                    if (shipConfiguration[0].maxShips < 1)
                    {
                        shipConfiguration.RemoveAt(0);
                    }
                    if (shipConfiguration.Count == 0)
                    {
                        allShipsPlaced = true;
                    }
                }
            }
            return map;
        }

        private static bool IsPointFree(int[,] map, int xPoint, int yPoint)
        {
            // текущая и далее по часовй стрелке вокруг
            return EmptyOrOut(map, yPoint, xPoint)
                && EmptyOrOut(map, yPoint - 1, xPoint)
                && EmptyOrOut(map, yPoint - 1, xPoint + 1)
                && EmptyOrOut(map, yPoint, xPoint + 1)
                && EmptyOrOut(map, yPoint + 1, xPoint + 1)
                && EmptyOrOut(map, yPoint + 1, xPoint)
                && EmptyOrOut(map, yPoint + 1, xPoint - 1)
                && EmptyOrOut(map, yPoint, xPoint - 1)
                && EmptyOrOut(map, yPoint - 1, xPoint - 1);
        }

        private static bool EmptyOrOut(int [,] map, int y, int x)
        {
            return y >= 10 || y < 0 || x >= 10 || x < 0
                || map[y, x] == CELL_EMPTY;
        }

        private static bool CanPutHorizontal(int [,] map, int xPoint, int yPoint, int shipLength, int coordLength)
        {
            var freePoints = 0;
            for (var x = xPoint; x < coordLength; x++)
            {
                // текущая и далее по часовй стрелке в гориз направл
                if (EmptyOrOut(map, yPoint, x)
                    && EmptyOrOut(map, yPoint - 1, x)
                    && EmptyOrOut(map, yPoint - 1, x + 1)
                    && EmptyOrOut(map, yPoint, x + 1)
                    && EmptyOrOut(map, yPoint + 1, x + 1)
                    && EmptyOrOut(map, yPoint + 1, x)
                )
                {
                    freePoints++;
                }
                else
                {
                    break;
                }
            }
            return freePoints >= shipLength;
        }

        private static bool CanPutVertical(int [,] map, int xPoint, int yPoint, int shipLength, int coordLength)
        {
            var freePoints = 0;
            for (var y = yPoint; y < coordLength; y++)
            {
                // текущая и далее по часовй стрелке в вертикальном направлении
                if (EmptyOrOut(map, y, xPoint)
                    && EmptyOrOut(map, y + 1, xPoint)
                    && EmptyOrOut(map, y + 1, xPoint + 1)
                    && EmptyOrOut(map, y + 1, xPoint)
                    && EmptyOrOut(map, y, xPoint - 1)
                    && EmptyOrOut(map, y - 1, xPoint - 1)
                )
                {
                    freePoints++;
                }
                else
                {
                    break;
                }
            }
            return freePoints >= shipLength;
        }

        private int HitsToWin()
        {
            var shipConfig = CreateShipConfiguration();
            return shipConfig.Sum(x => x.maxShips * x.pointCount);
        }

        private async void AddShipsToDatabase(int [,] map, int playerID, int sessionID)
        {
            var db = new DBContext();

            // Временное решение, удали!
            var shipsToRemove = db.PlayerShips.Where(x => x.player == playerID);
            db.PlayerShips.RemoveRange(shipsToRemove);
            //

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i,j] == 1)
                    {
                        var playerShip = new PlayerShips
                        {
                            player = playerID,
                            gameSessionId = sessionID,
                            x = j,
                            y = i
                        };
                        db.PlayerShips.Add(playerShip);
                    }
                }
            }
            await db.SaveChangesAsync();
        }
    }
}
