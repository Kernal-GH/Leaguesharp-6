using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
namespace GarenteedFramework
{
    public abstract class GarenteedFramework
    {
        private static Spell Q = new Spell(SpellSlot.Q);
        private static Spell W = new Spell(SpellSlot.W);
        private static Spell E = new Spell(SpellSlot.E);
        private static Spell R = new Spell(SpellSlot.R);
        private string champName = "";
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Let them know it loaded.
            Game.PrintChat("Made using Garenteed Framework by Nouser");
            Game.OnGameUpdate += OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        public void SetName(string name)
        {
            champName = name;
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name.Equals(Q.ToString()))
                {
                    QLogic(sender, args);
                }
                else if (args.SData.Name.Equals(W.ToString()))
                {

                }
                else if (args.SData.Name.Equals(E.ToString()))
                {

                }
                else if (args.SData.Name.Equals(R.ToString()))
                {

                }
            }
        }
        public virtual void ExpandGameLoad(EventArgs args);
        public virtual void QLogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        public abstract void WLogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        public abstract void ELogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        public abstract void RLogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
    }
}
