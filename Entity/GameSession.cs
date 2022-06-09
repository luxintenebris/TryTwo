using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Entity
{
    public class GameSession
    {
        [Key]
        public int sessionId { get; set; }
        public int player1 { get; set; }
        public int p1Hits { get; set; } = 0;
        public int p1HitsForWin { get; set; }
        public int player2 { get; set; }
        public int p2Hits { get; set; } = 0;
        public int p2HitsForWin { get; set; }
        public bool finished { get; set; } = false;
        public int lobbyID { get; set; }
    }
}
