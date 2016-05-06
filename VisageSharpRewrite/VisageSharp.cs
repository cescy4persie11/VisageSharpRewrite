using Ensage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisageSharpRewrite.Features;
using VisageSharpRewrite.Utilities;
using VisageSharpRewrite.Abilities;
using Ensage.Common;

namespace VisageSharpRewrite
{
    public class VisageSharp
    {     

        private static Hero Me
        {
            get
            {
                return Variables.Hero;
            }
        }

        private static List<Unit> Familiars
        {
            get
            {
                return Variables.Familiars;
            }
        }

        private bool pause;

        private FamiliarAutoLast familiarAutoLastHit;

        private AutoNuke autoNuke;

        private Follow follow;

        public VisageSharp()
        {
            this.familiarAutoLastHit = new FamiliarAutoLast();

        }

        public void OnClose()
        {
            this.pause = true;
        }

        public void OnDraw()
        {

        }

        public void OnLoad()
        {
            Variables.Hero = ObjectManager.LocalHero;
            this.pause = Variables.Hero.ClassID != ClassID.CDOTA_Unit_Hero_Visage;
            if (this.pause) return;
            Variables.MenuManager = new MenuManager(Me.Name);
            Variables.Familiars = ObjectManager.GetEntities<Unit>().Where(unit => unit.ClassID.Equals(ClassID.CDOTA_Unit_VisageFamiliar)).ToList();
            Variables.graveChill = new GraveChill(Me.Spellbook.Spell1);
            Variables.soulAssumption = new SoulAssumption(Me.Spellbook.Spell2);
            Variables.familiarControl = new FamiliarControl();
            Variables.MenuManager.Menu.AddToMainMenu();
            this.autoNuke = new AutoNuke();
            this.follow = new Follow();
            Game.PrintMessage(
                "VisageSharp" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " loaded",
                MessageType.LogMessage);
        }

        public void OnUpdate_AutoLastHit()
        {
            if (!this.pause)
            {
                this.pause = Game.IsPaused;
            }
            var familiars = ObjectManager.GetEntities<Unit>().Where(unit => unit.ClassID.Equals(ClassID.CDOTA_Unit_VisageFamiliar)
                && unit.IsAlive && unit.IsControllable && unit.IsValid).ToList();
            if (familiars == null) return;
            if (this.pause || Me == null || !familiars.Any(x => x != null) || !familiars.Any(x => x.IsAlive) || !familiars.Any(x => x.IsValid))
            {
                this.pause = Game.IsPaused;
                return;
            }

            if (Utils.SleepCheck("stone"))
            {
                foreach(var f in familiars)
                {
                    if (Variables.familiarControl.FaimiliarHasToStone(f))
                    {
                        Variables.familiarControl.UseStone(f);
                    }
                }
                Utils.Sleep(500, "stone");
            }

            if (Utils.SleepCheck("autolasthit"))
            {
                if (Variables.InAutoLasthiMode)
                {
                    
                    familiarAutoLastHit.Execute(familiars);
                }
                Utils.Sleep(100, "autolasthit");
            }
        }

        public void OnUpdate_AutoNuke()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            if (!Variables.AutoSoulAumptionOn)
            {
                return;
            }
            autoNuke.Execute();
        }

        public void OnUpdate_Follow()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            if (Variables.FollowMode)
            {
                follow.Execute();
            }
        }



        public void OnWndProc(WndEventArgs args)
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            /*
            if (this.Target == null || !this.Target.IsValid)
            {
                return;
            }
            */
        }

        public void Player_OnExecuteOrder(ExecuteOrderEventArgs args)
        {
            
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            if (!Variables.InAutoLasthiMode)
            {
                //reset autoattack mode
                familiarAutoLastHit.PlayerExecution();           
            }
            if (Variables.FollowMode)
            {
                follow.PlayerExecution(args);
            }
        }




        }
}
