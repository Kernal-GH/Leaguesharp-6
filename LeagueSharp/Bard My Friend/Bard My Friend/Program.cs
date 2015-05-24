using System.Collections;
using System.Linq.Expressions;
using System.Security.Cryptography;
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
        public static Vector3 destination;
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
            W.SetSkillshot(.5f, 100, 1000, false, SkillshotType.SkillshotCircle);
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
            RootMenu.SubMenu("Flee").AddItem(new MenuItem("Run Away", "Run Away").SetValue(new KeyBind('Z', KeyBindType.Press)));

            RootMenu.AddSubMenu(new Menu("Get Objects", "Get Objects"));
            RootMenu.SubMenu("Get Objects").AddItem(new MenuItem("Active", "Active").SetValue(new KeyBind('P', KeyBindType.Press)));

            RootMenu.AddSubMenu(new Menu("Chimes", "Chimes"));
            RootMenu.SubMenu("Chimes").AddItem(new MenuItem("Collect Now", "Collect Now").SetValue(new KeyBind('N', KeyBindType.Toggle)));

            RootMenu.AddToMainMenu();
            #endregion
        }
        private static void OnGameUpdate(EventArgs args)
        {

            if (RootMenu.SubMenu("Chimes").Item("Collect Now").IsActive())
            {
                int count = GetChimeLocs().Count();
                var tempDest = GetNextPoint(GetChimeLocs());
                if (count!=0 && tempDest != destination )
                {
                    destination = tempDest;
                    Player.IssueOrder(GameObjectOrder.MoveTo, destination);
                    if (Player.Position.Equals(destination))
                    {
                        destination = GetNextPoint(GetChimeLocs());
                        if (destination.Equals(Player.Position))
                            count = 0;
                    }
                }
                if (count==0)
                {
                    RootMenu.SubMenu("Chimes").Item("Collect Now").SetValue(new KeyBind('N', KeyBindType.Toggle));
                }
            }

            Combat.Heal();
            Combat.FreezeDragon();
            if(RootMenu.SubMenu("Combo").Item("Combo").IsActive())
                Combo();
            if (RootMenu.SubMenu("Flee").Item("Run Away").IsActive())
            {
                W.Cast(Player.Position);
                Flee();
            }
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

        public static Vector3 GetNextPoint(Vector3[] positions)
        {
            if(positions.Count()==0)
                return new Vector3();
            var minimum = float.MaxValue;
            var start = Player.Position;
            Vector3 tempMin = new Vector3();
            foreach (Vector3 chime in positions)
            {
                var path = ObjectManager.Player.GetPath(start, chime);
                var lastPoint = start;
                var d = 0f;
                d = DistanceFromArray(path);
                if (d < minimum)
                {
                    minimum = d;
                    tempMin = chime;
                }
            }
            return tempMin;
        }
        public static Boolean CompareVec3(Vector3 first, Vector3 second)
        {
            return (Math.Abs(first.X - second.X) < 2 && Math.Abs(first.Y - second.Y) < 2 &&
                    Math.Abs(first.Z - second.Z) < 2);
        }

        public static float DistanceFromArray(Vector3[] array)
        {
            var start = array[0];
            float distance = 0;
            for (int i = 1; i < array.Length; i++)
            {
                distance += start.Distance(array[i]);
                start = array[i];
            }
            return distance;
        }


        public static Vector3[] GetChimeLocs()
        {
            List<Vector3> locations = new List<Vector3>();
            GameObject[] objects = LeagueSharp.ObjectManager.Get<GameObject>().ToArray();
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].Name.ToLower().Equals("bardchimeminion"))
                {
                    locations.Add(objects[i].Position);
                }
            }
            return locations.ToArray();
        }
        #region Combat Methods
        public static void Harass()
        {
            //TODO: Add mana manager.
            if (Program.Q.IsReady())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(Program.Q.Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget())
                {
                    if (Program.RootMenu.SubMenu("Harass").Item("Use Q only when Stun").IsActive())
                        Stun(target);
                    else
                        Program.Q.Cast(target);
                }
            }
        }

        public static void Heal()
        {
            if (!Player.IsRecalling())
            {
                //First and foremost, heal yourself.
                //From there, check your allies and see if they're in range and have less than 35% health.
                //If they do, cast it on them.
                //TODO: Change the values to be loaded from the menu.
                //TODO: Add mana manager.
                if (Program.W.IsReady())
                {
                    if (Program.Player.HealthPercent <= 35)
                        Program.W.Cast(Player.Position);
                    else
                    {
                        foreach (
                            Obj_AI_Hero friendlies in ObjectManager.Get<Obj_AI_Hero>().Where(allies => !allies.IsEnemy))
                        {
                            if (Vector3.Distance(Player.Position, friendlies.Position) < 1000f &&
                                friendlies.HealthPercent < 35)
                                W.Cast(friendlies.Position);
                        }
                    }
                }
            }
        }
        public static void FreezeDragon()
        {
            Obj_AI_Minion[] objects = LeagueSharp.ObjectManager.Get<Obj_AI_Minion>().ToArray();
            for (int i = 0; i < objects.Length; i++)
            {
                if ((objects[i].Name.Equals("SRU_Dragon") || objects[i].Name.Equals("SRU_Baron")) && objects[i].Health < 1750)
                {
                    if (Vector3.Distance(objects[i].Position, Player.Position) < 3400f)
                    {
                        int playercount = 0;
                        int enemycount = 0;
                        foreach (Obj_AI_Hero players in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            if (Vector3.Distance(objects[i].Position, players.Position) <= 350f)
                                if (players.IsEnemy)
                                    enemycount++;
                                else
                                    playercount++;
                        }
                        if (playercount == 0 && enemycount != 0)
                            R.Cast(objects[i]);
                    }
                }
            }
        }

        public static void Combo()
        {
            if (Q.IsReady())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                Stun(target);
                if (Q.IsReady())
                    Q.Cast();
                int playercount = 0;
                int enemycount = 0;
                foreach (Obj_AI_Hero players in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (Vector3.Distance(target.Position, players.Position) <= 350f)
                        if (players.IsEnemy)
                            enemycount++;
                        else if (!players.IsMe)
                            playercount++;
                }
                if (playercount == 0 && enemycount != 0)
                    R.CastIfHitchanceEquals(target, HitChance.High);
            }
        }
        private static void Stun(Obj_AI_Hero target)
        {
            //TODO: Add mana manager.
            var prediction = Q.GetPrediction(target);

            var direction = (Player.ServerPosition - prediction.UnitPosition).Normalized();
            var endOfQ = (Q.Range) * direction;
            //Modified Vayne condemn logic from DZ191
            for (int i = 0; i < 30; i++)
            {
                var checkPoint = prediction.UnitPosition.Extend(Player.ServerPosition, -Q.Range / 30 * i);
                var j4Flag = IsJ4FlagThere(checkPoint, target);
                if (checkPoint.IsWall() || prediction.CollisionObjects.Count == 1 || j4Flag)
                {
                    Q.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }
        //Credits to DZ191
        private static bool IsJ4FlagThere(Vector3 position, Obj_AI_Hero target)
        {
            return ObjectManager.Get<Obj_AI_Base>().Any(m => m.Distance(position) <= target.BoundingRadius && m.Name == "Beacon");
        }

        public static void Flee()
        {
            //For fleeing, cast your Q to slow them down. Then AA, since your AA usually has a slow from Meeps
            //Then cast W on yourself to speed yourself up.
            //TODO: Check if you have a meep before doing AA.
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Q.IsReady() && target.IsValidTarget())
                Q.Cast(target);
            target = TargetSelector.GetTarget(Player.AttackRange, TargetSelector.DamageType.Physical);
            if (Player.CanAttack)
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);

            if (W.IsReady())
            {
                W.Cast(Game.CursorPos);
            }

        }
        #endregion
    }
}
