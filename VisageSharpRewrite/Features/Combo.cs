using Ensage;
using Ensage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisageSharpRewrite.Abilities;
using Ensage.Common.Extensions;

namespace VisageSharpRewrite.Features
{
    public class Combo
    {

        private SoulAssumption soulAssumption
        {
            get
            {
                return Variables.soulAssumption;
            }
        }

        private GraveChill graveChill
        {
            get
            {
                return Variables.graveChill;
            }
        }

        private FamiliarControl familiarControl
        {
            get
            {
                return Variables.familiarControl;
            }
        }

        private bool hasLens;

        private AutoNuke autoNuke;

        public Combo()
        {
            hasLens = false;
            this.autoNuke = new AutoNuke();
        }

        public void Update(Hero me)
        {
            this.hasLens = me.Inventory.Items.Any(x => x.Name == "item_aether_lens");
        }

        public void Execute(Hero me, Hero target, List<Unit> familiars)
        {
            if (!me.IsAlive) return;
            if (target == null) return;
            Update(me);
            //grave chill
            if (graveChill.CanBeCastedOn(target, hasLens))
            {
                if (Utils.SleepCheck("gravechill"))
                {
                    graveChill.UseOn(target);
                    Utils.Sleep(200, "gravechill");
                }
            }
            else
            {
                // go towards target
                if (me.CanMove())
                {
                    
                    if (Utils.SleepCheck("move"))
                    {
                        me.Move(target.Position);
                        Utils.Sleep(100, "move");
                    }
                }
                //Orbwalk
                if (Utils.SleepCheck("orbwalk"))
                {
                    if (Orbwalking.AttackOnCooldown())
                    {
                        Orbwalking.Orbwalk(target, 0, 0, false, true);
                    }
                    else
                    {
                        Orbwalking.Attack(target, true);
                    }
                    Utils.Sleep(200, "orbwalk");
                }

                //soulAssumption
                autoNuke.KillSteal(me);
                // max dmg on target
                if (soulAssumption.HasMaxCharges() && soulAssumption.CanbeCastedOn(target, hasLens))
                {
                    if (Utils.SleepCheck("soulassumption"))
                    {
                        soulAssumption.Use(target);
                        Utils.Sleep(200, "soulassumption");
                    }
                }
                //familiar controls
                if (familiars == null) return;
                if (!familiarControl.AnyFamiliarNearMe(familiars, 3000)) return;
                //auto stone
                if (Utils.SleepCheck("stone"))
                {
                    foreach (var f in familiars)
                    {

                        if (familiarControl.FaimiliarHasToStone(f))
                        {
                            familiarControl.UseStone(f);
                        }

                    }
                    Utils.Sleep(200, "stone");
                }
                // familiar attack
                if (Utils.SleepCheck("f-attack"))
                {
                    foreach (var f in familiars)
                    {
                        if (f.Distance2D(target) < 100)
                        {
                            if (f.CanAttack() && (f.BonusDamage > 15 || !f.Spellbook.SpellQ.CanBeCasted()))
                            {
                                f.Attack(target);
                            }
                        }
                        else
                        {
                            if (f.CanMove())
                            {
                                f.Move(f.Spellbook.SpellQ.GetPrediction(target));
                            }
                            else if (familiarControl.FamiliarCanStoneEnemies(target, f))
                            {
                                f.Spellbook.SpellQ.UseAbility();
                            }
                        }                      
                    }
                    Utils.Sleep(200, "f-attack");
                }

            }
        }
    }
}
