using Microsoft.Xna.Framework;
using PostLunarAcc.Projectiles;
using PostLunarAcc.Rarity;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PostLunarAcc.Items.Accessories
{
    public class SoulbindingAmulet : ModItem
    {
        public static LocalizedText Souls { get; set; }
        public static LocalizedText TooltipSmall { get; set; }
        public static LocalizedText TooltipBig { get; set; }

        public override void SetStaticDefaults()
        {
            Souls = this.GetLocalization("SoulsConsumed");
            TooltipSmall = this.GetLocalization("TooltipSmall");
            TooltipBig = this.GetLocalization("TooltipBig");
            ItemID.Sets.ItemIconPulse[Item.type] = true;
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
            Item.damage = 200;
            Item.DamageType = DamageClass.Summon;
            Item.rare = ModContent.RarityType<MoonFragmentRarity>();
            Item.accessory = true;
            Item.hasVanityEffects = true;
            Item.value = Item.sellPrice(platinum: 2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<HelperWraith>()] == 0)
                Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<HelperWraith>(), (int)player.GetTotalDamage(DamageClass.Summon).ApplyTo(Item.damage), 0, player.whoAmI);
            player.GetModPlayer<HelperWraithTracking>().SoulboundItemInstance = Item;
            player.maxMinions += 4;
            player.whipRangeMultiplier += 0.25f;
            player.autoReuseGlove = true;
            player.GetModPlayer<HelperWraithTracking>().soulbindingActive = true;
            base.UpdateAccessory(player, hideVisual);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ExtensionMethods.ShiftDown)
            {
                TooltipLine description = new(Mod, "DescriptionBig", TooltipBig.Value);
                tooltips.Add(description);
                int soulsConsumed = Main.LocalPlayer.GetModPlayer<HelperWraithTracking>().SoulsConsumed;
                string textSoulsConsumed = Souls.Format(soulsConsumed, 75 + soulsConsumed * 2);
                TooltipLine text = new(Mod, "Souls", textSoulsConsumed);
                tooltips.Add(text);
                return;
            }
            TooltipLine descriptionSmall = new(Mod, "DescriptionSmall", TooltipSmall.Value);
            tooltips.Add(descriptionSmall);
        }
    }
}