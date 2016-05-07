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
using Ensage.Common.Extensions;

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

        private DrawText drawText;

        private TargetFind targetFind;

        private Combo combo;

        private Hero Target
        {
            get
            {
                return this.targetFind.Target;
            }
        }

        public VisageSharp()
        {
            this.familiarAutoLastHit = new FamiliarAutoLast();
        }


        public void OnDraw()
        {
            if (Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            drawText.DrawAutoLastHit(Variables.InAutoLasthiMode);
            drawText.DrawAutoNuke(Variables.AutoSoulAumptionOn);
            drawText.DrawTextCombo(Variables.ComboOn);
            drawText.DrawFollow(Variables.FollowMode);

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
            Variables.EnemyTeam = Me.GetEnemyTeam();
            this.familiarAutoLastHit = new FamiliarAutoLast();
            this.autoNuke = new AutoNuke();
            this.follow = new Follow();
            this.drawText = new DrawText();
            this.targetFind = new TargetFind();
            this.combo = new Combo();
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
            Variables.Familiars = ObjectManager.GetEntities<Unit>().Where(unit => unit.ClassID.Equals(ClassID.CDOTA_Unit_VisageFamiliar)).ToList();
            if (Familiars == null) return;
            if (this.pause || Me == null || !Familiars.Any(x => x != null) || !Familiars.Any(x => x.IsAlive) || !Familiars.Any(x => x.IsValid))
            {
                this.pause = Game.IsPaused;
                return;
            }

            if (Utils.SleepCheck("autolasthit"))
            {
                if (Variables.InAutoLasthiMode)
                {

                    familiarAutoLastHit.Execute(Familiars);
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
            var EnemyNearMe = ObjectManager.GetEntities<Hero>().Any(x => x.IsAlive
                                                                          && x.Team != Me.Team
                                                                          && Me.Distance2D(x) <= 1500);
            if (!EnemyNearMe) return;
            if (!Variables.AutoSoulAumptionOn)
            {
                return;
            }
            autoNuke.Execute(Me);
        }

        public void OnUpdate_Follow()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            if (Variables.FollowMode)
            {
                follow.Execute(Familiars);
            }
        }

        public void OnUpdate_Combo()
        {
            /*
            if (this.pause || ((Variables.Hero == null && Variables.Familiars == null) || (!Variables.Hero.IsAlive && !Variables.Familiars.Any(x => x.IsAlive)))) return;
            */
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            this.targetFind.Find();
            if (Variables.ComboOn && Target != null)
            {
                combo.Execute(Me, Target, Familiars);
            }
        }

        public void OnUpdate()
        {
            if (this.pause || ((Me == null && Familiars == null) || (!Me.IsAlive && !Familiars.Any(x => x.IsAlive))))
            {
                return;
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

            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive || Familiars == null)
            {
                return;
            }
            if (!Variables.InAutoLasthiMode)
            {
                //reset autoattack mode
                familiarAutoLastHit.PlayerExecution();
            }
            //refinements on follow mode, familiar will duplicate Hero movement if familiars are next the heros, instead of simply following.
            if (Variables.FollowMode)
            {
                follow.PlayerExecution(args, Familiars);
            }
            //unlock target
            this.targetFind.UnlockTarget();
        }

        public void OnClose()
        {
            this.pause = true;
            if (Variables.MenuManager != null)
            {
                Variables.MenuManager.Menu.RemoveFromMainMenu();
            }

            Variables.PowerTreadsSwitcher = null;
        }




    }
}
