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
using VisageSharpRewrite.Abilities;

namespace VisageSharpRewrite.Features
{
    public class AutoNuke
    {
        private Hero me
        {
            get
            {
                return Variables.Hero;
            }
        }

        private SoulAssumption soulAssumption
        {
            get
            {
                return Variables.soulAssumption;
            }
        }

        private bool hasLens
        {
            get
            {
                return me.HasItem(ClassID.CDOTA_Item_Aether_Lens);
            }
        }

        public AutoNuke()
        {

        }

        public void Execute()
        {

            // Auto Kill steal
            var NearbyEnemy = ObjectManager.GetEntities<Hero>().Where(x => !x.IsMagicImmune() && x.IsAlive
                                                                           && !x.IsIllusion && x.Team != me.Team
                                                                           && x.Distance2D(me) <= (hasLens ? 1080 : 900) + 100);
            if (NearbyEnemy == null) return;
            var MinHpTargetNearbyEnemy = NearbyEnemy.OrderBy(x => x.Health).FirstOrDefault();
            if (MinHpTargetNearbyEnemy == null) return;
            var killableTarget = NearbyEnemy.Where(x => x.Health <= soulAssumption.Damage(x, hasLens)).FirstOrDefault();
            if (killableTarget == null)
            {
                var SoulAssumpCharges = me.Modifiers.Where(x => x.Name == "modifier_visage_soul_assumption").FirstOrDefault();
                if (SoulAssumpCharges == null) return;
                if(soulAssumption.HasMaxCharges() && soulAssumption.CanbeCastedOn(MinHpTargetNearbyEnemy, hasLens))
                {
                    if (Utils.SleepCheck("soulassumption"))
                    {
                        soulAssumption.Use(MinHpTargetNearbyEnemy);
                        Utils.Sleep(100, "soulassumption");
                    }
                }
            }
            else
            {
                if (soulAssumption.CanbeCastedOn(killableTarget, hasLens))
                {
                    if (Utils.SleepCheck("soulassumption"))
                    {
                        soulAssumption.Use(killableTarget);
                        Utils.Sleep(100, "soulassumption");
                    }
                }
            }

        }



    }
}
