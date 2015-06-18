using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using OneKeyToFlee.Plugins;


namespace OneKeyToFlee
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static void OnGameLoad(EventArgs args)
        {
            try
            {
                switch (ObjectManager.Player.ChampionName)
                {
                    case "Azir":
                        Azir.Load();
                        break;
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
