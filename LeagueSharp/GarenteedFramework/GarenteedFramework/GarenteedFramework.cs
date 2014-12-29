using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
namespace GarenteedFramework
{
    public class GarenteedFramework
    {
        private static FrameWorkPlugin myDerived;
        private static Spell Q = new Spell(SpellSlot.Q);
        private static Spell W = new Spell(SpellSlot.W);
        private static Spell E = new Spell(SpellSlot.E);
        private static Spell R = new Spell(SpellSlot.R);
        private static int deathLogicNumber = 0;
        private string champName = "";

        GarenteedFramework(FrameWorkPlugin init)
        {
            myDerived = init;
        }
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

        private static void OnGameUpdate(EventArgs args)
        {
            if(ObjectManager.Player.Deaths>=deathLogicNumber)
                myDerived.DeathsLogic();
            if (Utility.InShopRange() || ObjectManager.Player.IsDead)
                myDerived.ShopLogic();
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
                    myDerived.QLogic(sender,args);
                }
                else if (args.SData.Name.Equals(W.ToString()))
                {
                    myDerived.WLogic(sender, args);
                }
                else if (args.SData.Name.Equals(E.ToString()))
                {
                    myDerived.ELogic(sender, args);
                }
                else if (args.SData.Name.Equals(R.ToString()))
                {
                    myDerived.RLogic(sender, args);
                }
            }
        }
    }

    public interface FrameWorkPlugin
    {
        void ExpandGameLoad(EventArgs args);
        void QLogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        void WLogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        void ELogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        void RLogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        void DeathsLogic();
        void ShopLogic();
    }
}
