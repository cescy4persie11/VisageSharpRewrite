using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.AbilityInfo;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Items;

namespace VisageSharpRewrite.Features
{
    public class ItemUsage
    {
        private List<Item> items;

        private bool hasLens;

        private Hero me
        {
            get
            {
                return Variables.Hero;
            }
        }

        public ItemUsage()
        {
            this.UpdateItems();
        }

        public void UpdateItems()
        {
            if (Variables.Hero == null || !Variables.Hero.IsValid)
            {
                return;
            }
            this.items = Variables.Hero.Inventory.Items.ToList();
            this.hasLens = me.HasItem(ClassID.CDOTA_Item_Aether_Lens);
            var powerTreads = this.items.FirstOrDefault(x => x.StoredName() == "item_power_treads");
            if (powerTreads != null)
            {
                Variables.PowerTreadsSwitcher = new PowerTreadsSwitcher(powerTreads as PowerTreads);
            }
        }

        public void Medalion(Hero target)
        {
            Item Medalion = me.FindItem("item_medallion_of_courage");
            if (Medalion == null) return;
            bool MedalionCond = !target.IsMagicImmune() && target.Distance2D(me) <= Medalion.CastRange + (hasLens? 200 : 0) + 100 && Medalion.CanBeCasted();
            if (!MedalionCond) return;
            if (Utils.SleepCheck("MedalionCond"))
            {
                Medalion.UseAbility(target);
                Utils.Sleep(100, "MedalionCond");
            }
        }

        public void SolarCrest(Hero target)
        {
            Item SolarCrest = me.FindItem("item_solar_crest");
            if (SolarCrest == null) return;
            bool SolarCrestCond = !target.IsMagicImmune() && target.Distance2D(me) <= SolarCrest.CastRange + (hasLens ? 200 : 0) + 100 && SolarCrest.CanBeCasted();
            if (!SolarCrestCond) return;
            if (Utils.SleepCheck("SolarCrest"))
            {
                SolarCrest.UseAbility(target);
                Utils.Sleep(100, "SolarCrest");
            }
        }

        public void RodOfAtos(Hero target)
        {
            Item RodOfAtos = me.FindItem("item_rod_of_atos");
            if (RodOfAtos == null) return;
            bool RodOfAtosCond = !target.IsMagicImmune() && target.Distance2D(me) <= RodOfAtos.CastRange + (hasLens ? 200 : 0) + 100 && RodOfAtos.CanBeCasted();
            if (!RodOfAtosCond) return;
            if (Utils.SleepCheck("RodOfAtos"))
            {
                RodOfAtos.UseAbility(target);
                Utils.Sleep(100, "RodOfAtos");
            }
        }

        public void UseVeil(Hero target)
        {
            Item Veil = me.FindItem("item_veil_of_discord");
            if (Veil == null) return;
            bool VeilCond = target.Distance2D(me) <= Veil.CastRange + 100 && Veil.CanBeCasted();
            if (!VeilCond) return;
            if (Utils.SleepCheck("Veil"))
            {
                Veil.UseAbility(target.Position);
                Utils.Sleep(100, "Veil");
            }
        }



        public void OffensiveItem(Hero target)
        {
            UseVeil(target);
            Medalion(target);
            SolarCrest(target);
            RodOfAtos(target);
        }



    }
}
