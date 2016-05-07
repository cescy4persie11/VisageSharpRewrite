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
    public class GraveChill
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

        public GraveChill(Ability ability)
        {
            this.ability = ability;
            //this.abilityIcon = Drawing.GetTexture("materials/ensage_ui/spellicons/storm_spirit_static_remnant");
            this.iconSize = new Vector2(HUDInfo.GetHpBarSizeY() * 2);
        }

        public bool CanBeCastedOn(Hero target, bool hasLens)
        {
            return this.ability.CanBeCasted() && !target.IsMagicImmune()
                    && Variables.Hero.Distance2D(target) <= this.ability.CastRange + (hasLens ? 200 : 0) + 100;
        }

        public void UseOn(Hero target)
        {
            this.ability.UseAbility(target);
        }
    }
}
