using Microsoft.Xna.Framework;
using PostLunarAcc.Projectiles;
using PostLunarAcc.Rarity;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc.Items.Accessories
{
    public class SoulbindingAmulet : ModItem
    {
        public override void SetStaticDefaults()
        {
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
            player.maxMinions += 4;
            player.whipRangeMultiplier += 0.25f;
            player.autoReuseGlove = true;
            base.UpdateAccessory(player, hideVisual);
        }
    }
}