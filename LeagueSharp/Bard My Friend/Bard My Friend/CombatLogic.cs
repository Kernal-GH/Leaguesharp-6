using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LeagueSharp.Native;
namespace Bard_My_Friend
{
    class CombatLogic
    {
        public void Harass()
        {
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

        public void Heal()
        {
            //First and foremost, heal yourself.
            //From there, check your allies and see if they're in range and have less than 35% health.
            //If they do, cast it on them.
            //TODO: Change the values to be loaded from the menu.
            if (Program.W.IsReady())
            {
                if (Program.Player.HealthPercent <= 35)
                    Program.W.Cast(Program.Player.Position);
                else
                {
                    foreach (Obj_AI_Hero friendlies in ObjectManager.Get<Obj_AI_Hero>().Where(allies => !allies.IsEnemy))
                    {
                        if (Vector3.Distance(Program.Player.Position, friendlies.Position) < 1000f && friendlies.HealthPercent < 35)
                            Program.W.Cast(friendlies.Position);
                    }
                }
            }
        }
        public void FreezeDragon()
        {
                Obj_AI_Minion[] objects = LeagueSharp.ObjectManager.Get<Obj_AI_Minion>().ToArray();
                for (int i = 0; i < objects.Length; i++)
                {
                    if ((objects[i].Name.Equals("SRU_Dragon") || objects[i].Name.Equals("SRU_Baron")) && objects[i].Health<1750)
                    {
                        if(Vector3.Distance(objects[i].Position, Program.Player.Position) < 3400f)
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
                                Program.R.Cast(objects[i]);
                        }
                    }
                }
        }

        public void Combo()
        {
            if (Program.Q.IsReady())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(Program.Q.Range, TargetSelector.DamageType.Magical);
                Stun(target);
                if (Program.Q.IsReady())
                    Program.Q.Cast();
                int playercount = 0;
                int enemycount = 0;
                foreach (Obj_AI_Hero players in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (Vector3.Distance(target.Position, players.Position) <= 350f)
                        if (players.IsEnemy)
                            enemycount++;
                        else if(!players.IsMe)
                            playercount++;
                }
                if (playercount == 0 && enemycount != 0)
                    Program.R.CastIfHitchanceEquals(target, HitChance.High);
            }
        }
        private void Stun(Obj_AI_Hero target)
        {
                var prediction = Program.Q.GetPrediction(target);

                var direction = (Program.Player.ServerPosition - prediction.UnitPosition).Normalized();
                var endOfQ = (Program.Q.Range)*direction;
                //Modified Vayne condemn logic from DZ191
                for (int i = 0; i < 30; i++)
                {
                    var checkPoint = prediction.UnitPosition.Extend(Program.Player.ServerPosition, -Program.Q.Range/30*i);
                    var j4Flag = IsJ4FlagThere(checkPoint, target);
                    if (checkPoint.IsWall() || prediction.CollisionObjects.Count == 1 || j4Flag)
                    {
                        Program.Q.Cast(prediction.UnitPosition);
                    }
                }
        }
        //Credits to DZ191
        private bool IsJ4FlagThere(Vector3 position, Obj_AI_Hero target)
        {
            return ObjectManager.Get<Obj_AI_Base>().Any(m => m.Distance(position) <= target.BoundingRadius && m.Name == "Beacon");
        }

        public void Flee()
        {
            //For fleeing, cast your Q to slow them down. Then AA, since your AA usually has a slow from Meeps
            //Then cast W on yourself to speed yourself up.
            //TODO: Check if you have a meep before doing AA.
            Obj_AI_Hero target = TargetSelector.GetTarget(Program.Q.Range, TargetSelector.DamageType.Magical);
            if (Program.Q.IsReady() && target.IsValidTarget())
                Program.Q.Cast(target);
            target = TargetSelector.GetTarget(Program.Player.AttackRange, TargetSelector.DamageType.Physical);
            if(Program.Player.CanAttack)
            Program.Player.IssueOrder(GameObjectOrder.AttackUnit, target);

            if (Program.W.IsReady())
            {
                Program.Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                Program.W.Cast(Game.CursorPos);
            }

        }
    }
}
