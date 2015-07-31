using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization.Configuration;
using LeagueSharp;
using SharpDX;
using LeagueSharp.Common;
namespace StreamSharp
{
    class Program
    {
        static Menu root = new Menu("Stream", "Stream", true);
        private static float deltaT = .2f;
        private static Vector3 lastEndpoint = new Vector3();
        private static float lastTime = 0f;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
            Game.OnUpdate += OnUpdate;
            Hacks.DisableDrawings = true;
            //Hacks.ZoomHack = false;
            Hacks.DisableSay = true;
            Obj_AI_Hero.OnNewPath += DrawFake;
            Orbwalking.BeforeAttack += BeforeAttackFake;
            Spellbook.OnCastSpell += BeforeSpellCast;
        }

        private static void BeforeAttackFake(Orbwalking.BeforeAttackEventArgs args)
        {
            Hud.ShowClick(ClickType.Attack, args.Target.Position);
        }

        private static void BeforeSpellCast(Spellbook s, SpellbookCastSpellEventArgs args)
        {
            if(args.Target.Position.Distance(ObjectManager.Player.Position)>=5f)
            Hud.ShowClick(ClickType.Attack, args.Target.Position);
        }

        private static void DrawFake(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (sender.IsMe && lastTime + deltaT < Game.Time && args.Path.LastOrDefault()!=lastEndpoint)
            {
                lastEndpoint = args.Path.LastOrDefault();
                Hud.ShowClick(ClickType.Move, Game.CursorPos);
                lastTime = Game.Time;
            }
        }

        static void OnLoad(EventArgs args)
        {
            root.AddItem(new MenuItem("Drawings", "Drawings").SetValue(new KeyBind('H', KeyBindType.Toggle, true)));
            //root.AddItem(new MenuItem("Zoom (USE AT YOUR RISK; NOT GUARANTEED SAFE)", "Zoom (USE AT YOUR RISK; NOT GUARANTEED SAFE)").SetValue(new KeyBind('J', KeyBindType.Toggle, false)));
            root.AddItem(new MenuItem("Game Say", "Game Say").SetValue(new KeyBind('K', KeyBindType.Toggle, false)));
            root.AddItem(new MenuItem("Stream", "Stream").SetValue(new KeyBind('L', KeyBindType.Toggle, false)));
            root.AddItem(new MenuItem("Config", "Config").SetValue(new KeyBind(';', KeyBindType.Toggle, false)));
            root.AddToMainMenu();
        }

        static void OnUpdate(EventArgs args)
        {
            if (!root.Item("Stream").IsActive() && !root.Item("Config").IsActive())
            {
                Hacks.DisableDrawings = !(root.Item("Drawings").IsActive());
                //Hacks.ZoomHack = (root.Item("Zoom (USE AT YOUR RISK; NOT GUARANTEED SAFE)").IsActive());
                Hacks.DisableSay = (root.Item("Game Say").IsActive());
            }
            else if (root.Item("Config").IsActive())
            {
                Hacks.DisableDrawings = false;
                Hacks.DisableSay = false;
                //Hacks.ZoomHack = (root.Item("Zoom (USE AT YOUR RISK; NOT GUARANTEED SAFE)").IsActive());
            }
            else
            {
                Hacks.DisableDrawings = true;
                //Hacks.ZoomHack = false;
                Hacks.DisableSay = true;
            }
        }
    }
}
