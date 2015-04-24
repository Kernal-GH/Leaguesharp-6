using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
namespace NoMoreFreezes
{ 
    class Program
    {
        private static int counter = 0;
        private static bool InGame = false;
        static void Main(string[] args)
        {
            Game.OnStart += GameStarted;
            Drawing.OnDraw += DrawNotFrozen;
        }

        private static void DrawNotFrozen(EventArgs args)
        {
            if (!InGame)
               
            {
                Drawing.DrawText(100,100, Color.Red, "You are still in game if this is increasing:" + counter);
                counter++;
            }
        }
        private static void GameStarted(EventArgs args)
        {
            InGame = true;
        }
    }
}
