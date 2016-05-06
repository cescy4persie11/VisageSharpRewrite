﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using SharpDX;
using VisageSharpRewrite.Abilities;

namespace VisageSharpRewrite.Features
{
    public class FamiliarAutoLast
    {

        public Unit _LowestHpCreep { get; set; }

        public Unit _creepTarget { get; set; }

        public int autoAttackMode { get; set; }

        private FamiliarControl familiarControl
        {
            get
            {
                return Variables.familiarControl;
            }
        }

        private Hero me
        {
            get
            {
                return Variables.Hero;
            }
        }

        public FamiliarAutoLast()
        {
            //this.familiarControl = Variables.familiarControl;
            this.autoAttackMode = 2;
        }

        public void Update()
        {
            //this.familiarControl = Variables.familiarControl;
        }

        public void Execute(List<Unit> familiars)
        {
            //Update();
            this.setAutoAttackMode();
            if (familiars == null) return;
            //Console.WriteLine("familiar pos " + familiar.FirstOrDefault().Position);

            if (!familiarControl.AnyEnemyCreepsAroundFamiliar(familiars))
            {
                //there is no enemy creeps around
                //return;
            }
            
            var AnyoneAttackingMe = ObjectManager.TrackingProjectiles.Any(x => x.Target.Name.Equals(familiars.FirstOrDefault().Name));
            //if no ally creeps nearby, go follow the nearst ally creeps

            if (!familiarControl.AnyAllyCreepsAroundFamiliar(familiars))
            {
                var closestAllyCreep = ObjectManager.GetEntities<Unit>().Where(_x =>
                                                                          _x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
                                                                          && _x.IsAlive
                                                                          && _x.Name.Equals("npc_dota_creep_badguys_melee")).
                                                                          OrderBy(x => x.Distance2D(familiars.FirstOrDefault())).FirstOrDefault();
                if (closestAllyCreep == null) return;
                if (Utils.SleepCheck("move"))
                {
                    foreach (var f in familiars)
                    {
                        if (f.CanMove())
                        {
                            f.Follow(closestAllyCreep);
                        }
                    }
                    Utils.Sleep(100, "move");
                }
            }
            else
            {
                if (AnyoneAttackingMe)
                {
                    //go the the cloestallycreeps
                    var closestAllyCreep = ObjectManager.GetEntities<Unit>().Where(_x =>
                                                                          _x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
                                                                          && _x.IsAlive
                                                                          && _x.Name.Equals("npc_dota_creep_badguys_melee")).
                                                                          OrderBy(x => x.Distance2D(familiars.FirstOrDefault())).FirstOrDefault();
                    if (closestAllyCreep == null) return;
                    foreach (var f in familiars)
                    {
                        if (f.CanMove())
                        {
                            f.Move(closestAllyCreep.Position);
                        }
                    }
                    Utils.Sleep(100, "move");
                }
                else
                {
                    // has enemy creeps around
                    //AutoLastHit Mode
                    getLowestHpCreep(familiars.FirstOrDefault(), 1000);
                    if (this._LowestHpCreep == null) return;
                    getKillableCreep(familiars.FirstOrDefault(), this._LowestHpCreep);
                    if (this._creepTarget == null) return;
                    if (this._creepTarget.IsValid && this._creepTarget.IsVisible && this._creepTarget.IsAlive)
                    {
                        var damageThreshold = GetDmanageOnTargetFromSource(familiars.FirstOrDefault(), _creepTarget, 0);
                        var numOfMeleeOnKillable = NumOfMeleeCreepsAttackingMe(_creepTarget);
                        var numOfRangedOnKillable = NumOfRangedCreepsAttackingMe(_creepTarget);
                        if (numOfMeleeOnKillable + numOfRangedOnKillable != 0)
                        {
                            var AttackableFamiliar = familiars.Where(x => x.CanAttack()
                                                            
                            && x.Modifiers.Any(y => y.Name == "modifier_visage_summon_familiars_damage_charge")
                                                                     && x.IsAlive
                                                                     );
                            var AttackableFamilarInRange = familiars.Where(x => x.CanAttack()
                                                                     && x.Modifiers.Any(y => y.Name == "modifier_visage_summon_familiars_damage_charge")
                                                                     && x.Distance2D(_creepTarget) <= x.AttackRange
                                                                     );

                            if (AttackableFamiliar == null) return;
                            if (AttackableFamiliar.All<Unit>(f => _creepTarget.Distance2D(f) <= f.AttackRange && f.CanAttack()))
                            {
                                var familiarDmg = AttackableFamilarInRange.Sum(f => GetDmanageOnTargetFromSource(f, _creepTarget, 0));
                                //Console.WriteLine("familiar dmg is " + familiarDmg);
                                if (_creepTarget.Health < familiarDmg)
                                {
                                    foreach (var f in AttackableFamiliar)
                                    {
                                        if (!f.IsAttacking())
                                        {
                                            f.Attack(_creepTarget);
                                        }
                                    }
                                }
                                else if(_creepTarget.Health < familiarDmg * 2 && _creepTarget.Health > familiarDmg)
                                //attack-hold
                                {
                                    if (Utils.SleepCheck("familiarAttack"))
                                    {
                                        foreach (var f in AttackableFamiliar)
                                        {
                                            f.Hold();
                                            f.Attack(_creepTarget);
                                        }
                                        Utils.Sleep(100, "familiarAttack");
                                    }
                                }
                                else
                                {
                                    if (AttackableFamiliar.Any<Unit>(x => x.Distance2D(_creepTarget) > x.AttackRange) && _creepTarget.ClassID != ClassID.CDOTA_BaseNPC_Creep_Siege)
                                    {
                                        if (Utils.SleepCheck("familiarmove"))
                                        {
                                            foreach (var f in AttackableFamiliar)
                                            {
                                                f.Move(this._LowestHpCreep.Position);
                                            }
                                            Utils.Sleep(100, "familiarmove");
                                        }

                                    }
                                }
                            }
                            else // not in range
                            {
                                if (Utils.SleepCheck("move") && _creepTarget.ClassID != ClassID.CDOTA_BaseNPC_Creep_Siege)
                                {
                                    foreach (var f in familiars)
                                    {
                                        f.Move(_creepTarget.Position);
                                    }
                                    Utils.Sleep(200, "move");
                                }
                            }

                        }
                        else
                        {
                            var AttackableFamilarInRange = familiars.Where(x => x.CanAttack()
                                                                    && x.Modifiers.Any(y => y.Name == "modifier_visage_summon_familiars_damage_charge")
                                                                    && x.Distance2D(_creepTarget) <= x.AttackRange
                                                                    );
                            var familiarDmg = AttackableFamilarInRange.Sum(f => GetDmanageOnTargetFromSource(f, _creepTarget, 0));
                            if (Utils.SleepCheck("attack") && _creepTarget.Health < familiarDmg * 1.5)
                            {
                                foreach (var f in familiars)
                                {
                                    if (_creepTarget.ClassID != ClassID.CDOTA_BaseNPC_Creep_Siege)
                                    {
                                        f.Attack(_creepTarget);

                                    }
                                    Utils.Sleep(200, "attack");
                                }
                            }
                        }
                    }
                }
            }
            


        }

        private void getLowestHpCreep(Unit source, int range)
        {
            if (source == null)
            {
                this._LowestHpCreep = null;
                return;
            }
            var lowestHp =
                    ObjectManager.GetEntities<Unit>()
                        .Where(
                            x =>
                                (x.ClassID == ClassID.CDOTA_BaseNPC_Tower ||
                                 x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
                                 || x.ClassID == ClassID.CDOTA_BaseNPC_Creep
                                 || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral
                                 || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege
                                 || x.ClassID == ClassID.CDOTA_BaseNPC_Additive
                                 || x.ClassID == ClassID.CDOTA_BaseNPC_Barracks
                                 || x.ClassID == ClassID.CDOTA_BaseNPC_Building
                                 || x.ClassID == ClassID.CDOTA_BaseNPC_Creature) && x.IsAlive && x.IsVisible
                                && x.Team != source.Team && x.Distance2D(source) < range)
                        .OrderBy(creep => creep.Health)
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();
            //Console.WriteLine("lowestHp creep is " + lowestHp.Name);
            this._LowestHpCreep = lowestHp;
        }

        private void getKillableCreep(Unit src, Unit creep)
        {
            var percent = creep.Health / creep.MaximumHealth * 100;
            var dmgThreshold = GetDmanageOnTargetFromSource(src, creep, 0);
            //Console.WriteLine("dmg threshold is " + dmgThreshold);
            if (creep.Health < dmgThreshold * 10 &&
                (percent < 75 || creep.Health < dmgThreshold)
                )
            {
                this._creepTarget = creep;
            }
            else
            {
                this._creepTarget = null;
            }
        }

        private double GetDmanageOnTargetFromSource(Unit src, Unit target, double bonusdmg)
        {
            double realDamage = 0;
            double physDamage = src.MinimumDamage + src.BonusDamage;
            if (src == null)
            {
                return realDamage;
            }

            if (target.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege ||
               target.ClassID == ClassID.CDOTA_BaseNPC_Tower)
            {
                realDamage = realDamage / 2;
            }

            var damageMp = 1 - 0.06 * target.Armor / (1 + 0.06 * Math.Abs(target.Armor));
            realDamage = (bonusdmg + physDamage) * damageMp;
            return realDamage;
        }

        private int NumOfMeleeCreepsAttackingMe(Unit me)
        {
            int num = 0;
            //melee creeps name = npc_dota_creep_badguys_melee
            //ranged creps name = npc_dota_creep_badguys_ranged
            try
            {
                var allMeleeCreepsAttackingMe = ObjectManager.GetEntities<Unit>().Where(_x =>
                                                                                    (_x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
                                                                                    && me.Distance2D(_x) <= 150
                                                                                    && _x.Team != me.Team
                                                                                    && _x.IsAlive
                                                                                    && _x.GetTurnTime(me) == 0)
                                                                                    && _x.Name.Equals("npc_dota_creep_badguys_melee"));
                if (allMeleeCreepsAttackingMe == null) return num;
                num = allMeleeCreepsAttackingMe.Count();
            }
            catch
            {

            }
            return num;
        }

        private int NumOfRangedCreepsAttackingMe(Unit me)
        {
            int num = 0;
            //melee creeps name = npc_dota_creep_badguys_melee
            //ranged creeps name = npc_dota_creep_badguys_ranged
            try
            {
                var allRangedCreepsAttackingMe = ObjectManager.GetEntities<Unit>().Where(_x =>
                                                                                    (_x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
                                                                                    && me.Distance2D(_x) <= 650
                                                                                    && _x.Team != me.Team
                                                                                    && _x.IsAlive
                                                                                    && _x.GetTurnTime(me) == 0)
                                                                                    && _x.Name.Equals("npc_dota_creep_badguys_ranged"));
                if (allRangedCreepsAttackingMe == null) return num;
                num = allRangedCreepsAttackingMe.Count();
            }
            catch
            {

            }
            return num;
        }

        public void setAutoAttackMode()
        {
            if(Variables.InAutoLasthiMode && this.autoAttackMode == 2)
            {
                this.autoAttackMode = 0;
                Game.ExecuteCommand("dota_player_units_auto_attack_mode " + this.autoAttackMode);
                Console.WriteLine("mode89 is " + this.autoAttackMode);
            }
        }

        public void PlayerExecution()
        {
            resetAutoAttackMode();
        }

        private void resetAutoAttackMode()
        {
            if(this.autoAttackMode == 0)
            {
                this.autoAttackMode = 2;
                Game.ExecuteCommand("dota_player_units_auto_attack_mode " + this.autoAttackMode);
                Console.WriteLine("mode is " + this.autoAttackMode);
            }
        }
        

    }
}
