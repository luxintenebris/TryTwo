﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Entity
{
    public class OpenGamesWithPlayer
    {
        public int GameID { get; set; }
        public string Player1Name { get; set; }
        public int sessionID { get; set; } = -1;

        //public OpenGamesWithPlayer(int gameID, string player1Name, bool started)
        //{
        //    GameID = gameID;
        //    Player1Name = player1Name;
        //    Started = started;
        //}
    }
}
