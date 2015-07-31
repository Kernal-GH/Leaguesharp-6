using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization.Configuration;
using LeagueSharp;
using LeagueSharp.Common;
namespace StreamSharp
{
    class Program
    {
        static Menu root = new Menu("Stream", "Stream", true);
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
            Game.OnUpdate += OnUpdate;
            Hacks.DisableDrawings = true;
            Hacks.ZoomHack = false;
            Hacks.DisableSay = true;
        }

        static void OnLoad(EventArgs args)
        {
            root.AddItem(new MenuItem("Drawings", "Drawings").SetValue(new KeyBind('H', KeyBindType.Toggle, false)));
            root.AddItem(new MenuItem("Zoom", "Zoom").SetValue(new KeyBind('J', KeyBindType.Toggle, true)));
            root.AddItem(new MenuItem("Game Say", "Game Say").SetValue(new KeyBind('K', KeyBindType.Toggle, true)));
            root.AddItem(new MenuItem("Stream", "Stream").SetValue(new KeyBind('L', KeyBindType.Toggle, true)));
            root.AddItem(new MenuItem("Config", "Config").SetValue(new KeyBind(';', KeyBindType.Toggle, false)));
            root.AddToMainMenu();
        }

        static void OnUpdate(EventArgs args)
        {
            if (!root.Item("Stream").IsActive() && !root.Item("Config").IsActive())
            {
                Hacks.DisableDrawings = !(root.Item("Drawings").IsActive());
                Hacks.ZoomHack = (root.Item("Zoom").IsActive());
                Hacks.DisableSay = (root.Item("Game Say").IsActive());
            }
            else if (root.Item("Config").IsActive())
            {
                Hacks.DisableDrawings = false;
                Hacks.DisableSay = false;
                Hacks.ZoomHack = (root.Item("Zoom").IsActive());
            }
            else
            {
                Hacks.DisableDrawings = true;
                Hacks.ZoomHack = false;
                Hacks.DisableSay = true;
            }
        }
    }
}
