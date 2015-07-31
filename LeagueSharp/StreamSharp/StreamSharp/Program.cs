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
        private static bool attacking = false;
        private static float lastOrderTime = 0f;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
            Game.OnUpdate += OnUpdate;
            Hacks.DisableDrawings = true;
            Hacks.DisableSay = true;
            Hacks.PingHack = false;
        }

        private static void BeforeAttackFake(Orbwalking.BeforeAttackEventArgs args)
        {
            if (root.SubMenu("Fake Clicks").Item("Click Mode").GetValue<StringList>().SelectedIndex == 1)
            {
                Hud.ShowClick(ClickType.Attack, RandomizePosition(args.Target.Position));
                attacking = true;
            }
        }

        private static void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe && (args.Order == GameObjectOrder.MoveTo || args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackTo) 
                && lastOrderTime + (deltaT) < Game.Time && root.SubMenu("Fake Clicks").Item("Enable").IsActive() 
                && root.SubMenu("Fake Clicks").Item("Click Mode").GetValue<StringList>().SelectedIndex==0)
            {
                Vector3 vect = args.TargetPosition;
                vect.Z = ObjectManager.Player.Position.Z;
                if (args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackTo)
                    Hud.ShowClick(ClickType.Attack, RandomizePosition(vect));
                else
                    Hud.ShowClick(ClickType.Move, vect);
                lastOrderTime = Game.Time;
            }
        }
        private static void AfterAttack(AttackableUnit atk, AttackableUnit atk2)
        {
            attacking = false;
        }

        private static void BeforeSpellCast(Spellbook s, SpellbookCastSpellEventArgs args)
        {
            if(args.Target.Position.Distance(ObjectManager.Player.Position)>=5f)
            Hud.ShowClick(ClickType.Attack, args.Target.Position);
        }

        private static void DrawFake(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (sender.IsMe && lastTime + deltaT < Game.Time && args.Path.LastOrDefault() != lastEndpoint && args.Path.LastOrDefault().Distance(ObjectManager.Player.ServerPosition) >=5f && root.SubMenu("Fake Clicks").Item("Enable").IsActive()
                && root.SubMenu("Fake Clicks").Item("Click Mode").GetValue<StringList>().SelectedIndex == 1)
            {
                lastEndpoint = args.Path.LastOrDefault();
                if (!attacking)
                    Hud.ShowClick(ClickType.Move, Game.CursorPos);
                else
                    Hud.ShowClick(ClickType.Attack, Game.CursorPos);
                lastTime = Game.Time;
            }
        }

        static Vector3 RandomizePosition(Vector3 input)
        {
            Random r = new Random((int)input.X);
            if (r.Next(2) == 0)
                input.X += r.Next(100);
            else
                input.Y += r.Next(100);
            return input;
        }

        static void OnLoad(EventArgs args)
        {
            root.AddItem(new MenuItem("Drawings", "Drawings").SetValue(new KeyBind('H', KeyBindType.Toggle, true)));
            root.AddItem(new MenuItem("Game Say", "Game Say").SetValue(new KeyBind('K', KeyBindType.Toggle, false)));
            root.AddItem(new MenuItem("Stream", "Stream").SetValue(new KeyBind('L', KeyBindType.Toggle, false)));
            root.AddItem(new MenuItem("Config", "Config").SetValue(new KeyBind(';', KeyBindType.Toggle, false)));

            Menu fakeClickMenu = new Menu("Fake Clicks", "Fake Clicks", false);
            fakeClickMenu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            fakeClickMenu.AddItem(new MenuItem("Click Mode", "Click Mode")).SetValue(new StringList(new string[] {"Evade, No Cursor Position", "Cursor Position, No Evade"}));
            root.AddSubMenu(fakeClickMenu);
            root.AddToMainMenu();

            Obj_AI_Hero.OnNewPath += DrawFake;
            Orbwalking.BeforeAttack += BeforeAttackFake;
            Spellbook.OnCastSpell += BeforeSpellCast;
            Orbwalking.AfterAttack += AfterAttack;
            Obj_AI_Base.OnIssueOrder += OnIssueOrder;
        }

        static void OnUpdate(EventArgs args)
        {
            if (!root.Item("Stream").IsActive() && !root.Item("Config").IsActive())
            {
                Hacks.DisableDrawings = !(root.Item("Drawings").IsActive());
                Hacks.DisableSay = (root.Item("Game Say").IsActive());
            }
            else if (root.Item("Config").IsActive())
            {
                Hacks.DisableDrawings = false;
                Hacks.DisableSay = false;
            }
            else
            {
                Hacks.DisableDrawings = true;
                Hacks.DisableSay = true;
            }
        }
    }
}
