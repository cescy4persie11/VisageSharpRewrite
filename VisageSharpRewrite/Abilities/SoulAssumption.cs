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
    public class SoulAssumption
    {
        private readonly Ability ability;

        private readonly DotaTexture abilityIcon;

        private uint level
        {
            get
            {
                return this.ability.Level;
            }
        }

        private Vector2 iconSize;

        public SoulAssumption(Ability ability)
        {
            this.ability = ability;
            //this.abilityIcon = Drawing.GetTexture("materials/ensage_ui/spellicons/storm_spirit_static_remnant");
            this.iconSize = new Vector2(HUDInfo.GetHpBarSizeY() * 2);
        }

        public bool HasMaxCharges()
        {
            var soulAssumption = Variables.Hero.Modifiers.Where(x => x.Name == "modifier_visage_soul_assumption").FirstOrDefault();
            if (soulAssumption == null) return false;
            return soulAssumption.StackCount == 2 + level;
        }

        public bool CanbeCastedOn(Hero target, bool hasLens)
        {
            return Variables.Hero.Distance2D(target) <= this.ability.CastRange + (hasLens ? 200 : 0) + 100 
                    && !target.IsMagicImmune() && this.ability.CanBeCasted();
        }

        public bool isLearned()
        {
            return this.ability.Level > 0;
        }

        public double Damage(Hero target, bool hasLens)
        {
            double dmg = 0;
            if (target == null) return 0;
            if (this.ability == null) return 0;
            var soulAssumption = Variables.Hero.Modifiers.Where(x => x.Name == "modifier_visage_soul_assumption");
            if (soulAssumption == null) return 10;
            var stackCount = soulAssumption.FirstOrDefault().StackCount;
            var magicResist = target.MagicDamageResist;
            var magicDmg = 20 + stackCount * 110;
            var intAmp = Variables.Hero.Intelligence / 16 * 0.01;
            dmg =  magicDmg * (1 - magicResist) * (1 + intAmp + (hasLens ? 0.05 : 0));
            return dmg;
        }

        public void Use(Hero target)
        {
            this.ability.UseAbility(target);
        }


    }
}
