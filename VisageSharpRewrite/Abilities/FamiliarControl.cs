using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using SharpDX;

namespace VisageSharpRewrite.Abilities
{
    public class FamiliarControl
    {
        public FamiliarControl()
        {

        }

        public bool FamiliarHasToStone(Unit f)
        {
            return f.Spellbook.SpellQ.CanBeCasted() && (f.BonusDamage < 20 || f.Health <= 3)
                    // exclude a situation where familiars are in the summon phase
                    && Variables.Hero.Spellbook.Spell4.Cooldown <= 200 - Variables.Hero.Spellbook.Spell4.Level * 20 - 5;              
        }

        public bool FamiliarCanStoneEnemies(Hero target, Unit f)
        {
            return f.Spellbook.SpellQ.CanBeCasted() && (f.BonusDamage < 20) && f.Distance2D(target) <= 100
                // exclude a situation where familiars are in the summon phase
                && Variables.Hero.Spellbook.Spell4.Cooldown <= 200 - Variables.Hero.Spellbook.Spell4.Level * 20 - 5; 
        }

        public bool NotMuchDmgLeft(Unit f)
        {
            return f.BonusDamage < 20 && f.Spellbook.SpellQ.CanBeCasted();
        }

        public void UseStone(Unit f)
        {
            f.Spellbook.SpellQ.UseAbility();
        }

        public void Attack(Unit f, Hero target)
        {
            if (!f.CanAttack() || target == null || !target.IsAlive || target.IsAttackImmune()) return;
            f.Attack(target);
        }

        public bool AnyFamiliarNearMe(List<Unit> familiars, int range)
        {
            return familiars.Any(x => x.IsAlive && x.Team == Variables.Hero.Team
                                && x.Distance2D(Variables.Hero) <= range);
        }

        public bool AnyEnemyNearFamiliar(List<Unit> familiars, int range)
        {
            return ObjectManager.GetEntities<Hero>().Any(x => x.IsAlive && x.Team != Variables.Hero.Team
                                                        && familiars.Any(y => x.Distance2D(y) <= range));
        }

        public bool AnyEnemyCreepsAroundFamiliar(List<Unit> familiars)
        {
            
            return ObjectManager.GetEntities<Unit>().Any(_x => ((_x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane ||
                                                               _x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege)
                                                               && familiars.Any(y => y.Distance2D(_x) <= 500)
                                                              && _x.Team != Variables.Hero.Team
                                                              ));
        }

        public bool AnyAllyCreepsAroundFamiliar(List<Unit> familiars)
        {
            return ObjectManager.GetEntities<Unit>().Any(_x => _x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
                                                                && _x.IsAlive
                                                                && (_x.Name.Equals("npc_dota_creep_badguys_melee") || _x.Name.Equals("npc_dota_creep_badguys_ranged"))
                                                                && familiars.Any<Unit>(_y => _y.Distance2D(_x) < 300));
        }

        public void RetreatToTowerOrFountain(List<Unit> familiars)
        {
            if (familiars == null) return;
            var ClosestAllyTower = ObjectManager.GetEntities<Unit>().Where(x => x.ClassID == ClassID.CDOTA_BaseNPC_Tower
                                                                                        && x.Team == Variables.Hero.Team
                                                                                        && x.Distance2D(familiars.FirstOrDefault()) > 100
                                                                                        ).OrderBy(y => y.Distance2D(familiars.FirstOrDefault()))
                                                                                       .FirstOrDefault();
            if(ClosestAllyTower == null)
            {
                if (Utils.SleepCheck("move"))
                {
                    foreach (var f in familiars)
                    {
                        if (f.CanMove())
                        {
                            f.Follow(ObjectManager.GetEntities<Unit>().Where(_x => _x.ClassID == ClassID.CDOTA_Unit_Fountain && _x.Team == Variables.Hero.Team).FirstOrDefault());
                        }
                    }
                    Utils.Sleep(1000, "move");
                }
            }
            else
            {
                if (Utils.SleepCheck("move"))
                {
                    foreach (var f in familiars)
                    {
                        if (f.CanMove())
                        {
                            f.Follow(ClosestAllyTower);
                        }
                    }
                    Utils.Sleep(1000, "move");
                }
            }
        }
    }
}
