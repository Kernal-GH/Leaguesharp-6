using System.Collections;
using System.Linq.Expressions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Bard_My_Friend
{
    internal class Program
    {
        public static Menu RootMenu;
        public static Obj_AI_Hero Player;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Vector3> GoToPath = new List<Vector3>();
        public static List<Vector3> chimeLocs;
        public static int WayPointCounter = 0;
        public static Spell Q = new Spell(SpellSlot.Q, 950f);
        public static Spell W = new Spell(SpellSlot.W, 1000f);
        public static Spell E = new Spell(SpellSlot.E, 900f);
        public static Spell R = new Spell(SpellSlot.R, 3400f);
        public static CombatLogic Combat = new CombatLogic();

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnUpdate += OnGameUpdate;
            chimeLocs = new List<Vector3>();
            Q.SetSkillshot(.5f, 50, 1500, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(.5f, 350, 2000, false, SkillshotType.SkillshotCircle);
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            
            Player = ObjectManager.Player;
            RootMenu = new Menu("Bard: My Friend", "Bard", true);

            Game.PrintChat("Bard My Friend loaded");

            
            #region MenuPopulation
            Menu tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            RootMenu.AddSubMenu(tsMenu);

            RootMenu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(RootMenu.SubMenu(("Orbwalking")));

            RootMenu.AddSubMenu(new Menu("Harass", "Harass"));
            RootMenu.SubMenu("Harass").AddItem(new MenuItem("Harass", "Harass")).SetValue(new KeyBind('C',KeyBindType.Press));
            RootMenu.SubMenu("Harass").AddItem(new MenuItem("Use Q", "Use Q")).SetValue(true);
            RootMenu.SubMenu("Harass").AddItem(new MenuItem("Use Q only when Stun", "Use Q only when Stun")).SetValue(false);

            RootMenu.AddSubMenu(new Menu("Combo", "Combo"));
            RootMenu.SubMenu("Combo").AddItem(new MenuItem("Combo", "Combo")).SetValue(new KeyBind(' ',KeyBindType.Press));
            RootMenu.SubMenu("Combo").AddItem(new MenuItem("Use Q", "Use Q")).SetValue(true);
            RootMenu.SubMenu("Combo").AddItem(new MenuItem("Use W", "Use W")).SetValue(true);
            //RootMenu.SubMenu("Combo").AddItem(new MenuItem("Use E (Experimental)", "Use E")).SetValue(true);

            RootMenu.AddSubMenu(new Menu("Flee", "Flee"));
            RootMenu.SubMenu("Flee").AddItem(new MenuItem("Run away", "Run Away").SetValue(new KeyBind('Z', KeyBindType.Press)));

            RootMenu.AddSubMenu(new Menu("Get Objects", "Get Objects"));
            RootMenu.SubMenu("Get Objects").AddItem(new MenuItem("Active", "Active").SetValue(new KeyBind('P', KeyBindType.Toggle)));


            RootMenu.AddToMainMenu();
            #endregion
        }
        private static void OnGameUpdate(EventArgs args)
        {
            //This populates the traversal. We find all bardchimeminions and add them.
            if (RootMenu.SubMenu("Get Objects").Item("Active").IsActive())
            {
                GameObject[] objects = LeagueSharp.ObjectManager.Get<GameObject>().ToArray();
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i].Name.ToLower().Equals("bardchimeminion"))
                    {
                        chimeLocs.Add(objects[i].Position);
                    }
                }
            }
            if (RootMenu.SubMenu("Chimes").Item("Collect Now").IsActive())
            {
                //If the path has no objects in it, then populate from the traversal.
                //From there, set the orbwalking point to the traversal point.
                //Once you reach it, go to the next position via the counter.
                //Once you reach the end, set the collect now to false.
                //Once it is false, clear the traversal path and the reset the counter.
                if (GoToPath.Count == 0)
                {
                    GoToPath.AddRange(GetConnections(chimeLocs.ToArray()));
                    WayPointCounter = 0;
                }
                if (GoToPath.Count != 0)
                {
                    Orbwalker.SetOrbwalkingPoint(GoToPath[WayPointCounter]);
                    if (GoToPath[WayPointCounter].Equals(Player.Position))
                        WayPointCounter++;
                    if (WayPointCounter > GoToPath.Count())
                        RootMenu.SubMenu("Chimes").Item("Collect Now").SetValue("False");
                }
            }
            else
            {
                GoToPath.Clear();
                WayPointCounter = 0;
            }

            Combat.Heal();
            Combat.FreezeDragon();
            if(RootMenu.SubMenu("Combo").Item("Combo").IsActive())
                Combat.Combo();
            if(RootMenu.SubMenu("Flee").Item("Run Away").IsActive())
                Combat.Flee();
            if (RootMenu.SubMenu("Harass").Item("Harass").IsActive())
                Combat.Harass();

            
        }

        public static Vector3[] GetConnections(Vector3[] positions)
        {
            List<Vector3> posiList = new List<Vector3>();
            Vector3 start = positions[0];
            List<Vector3> inOrderList = new List<Vector3>();
            posiList.AddRange(positions);
            posiList.RemoveAt(0);
            //Start with your player's position (position[0]) and set the minimum distance
            //To the max float value for comparison. From there, loop through every position
            //For the chimes and if it has a distance lower than the minimum, change the minimum
            //And store the position as the closest position.
            //When the closest position is found, remove it from the posiList and add it to the inOrderList.
            //Set the new start point as that last end point and then repeat until posiList is empty (everything is visited)
            while (posiList.Count != 0)
            {
                var minimum = float.MaxValue;
                Vector3 tempMin = new Vector3();
                foreach (Vector3 end in posiList)
                {
                    var path = ObjectManager.Player.GetPath(start, end);
                    var lastPoint = start;
                    var d = 0f;
                    foreach (var point in path.Where(point => !point.Equals(lastPoint)))
                    {
                        d += lastPoint.Distance(point);
                        lastPoint = point;
                    }
                    if (d < minimum)
                    {
                        minimum = d;
                        tempMin = end;
                    }
                }
                posiList.Remove(tempMin);
                inOrderList.Add(tempMin);
                start = tempMin;
            }
            return inOrderList.ToArray();
        }

        public static Boolean CompareVec3(Vector3 first, Vector3 second)
        {
            return (Math.Abs(first.X - second.X) < 2 && Math.Abs(first.Y - second.Y) < 2 &&
                    Math.Abs(first.Z - second.Z) < 2);
        }
    }
}
