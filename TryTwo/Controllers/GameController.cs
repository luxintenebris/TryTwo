using DAL;
using Entity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TryTwo.Models
{
    public class GameController : Controller
    {
        public async Task<ActionResult> Index(int playerID = -1, int sessionId = -1)
        {
            System.Diagnostics.Debug.WriteLine("Player/Session: " + playerID + "/" + sessionId);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Account");
            }
            else if (playerID == -1 || sessionId == -1)
            {
                return RedirectToAction("Lobby", "Battleships");
            }
            else
            {
                ViewBag.sessionId = sessionId;
                ViewBag.playerId = playerID;
                return View();
            }
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
            // необходимо т.к. БД теряет связь с предыдущим объектом
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

        public ActionResult WaitForGame(int p1)
        {
            return RedirectToAction("Index", new Tuple<int, int>(p1, -1));
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
