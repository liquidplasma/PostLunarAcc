using Microsoft.Xna.Framework;
using PostLunarAcc.Debuffs;
using PostLunarAcc.Items.Ingredients;
using PostLunarAcc.Rarity;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc.Items.Accessories
{
    public class RangerLunarModplayer : ModPlayer
    {
        public bool active;

        public override void ResetEffects()
        {
            active = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (active && hit.DamageType.CountsAsClass(DamageClass.Ranged))
            {
                target.AddBuff(ModContent.BuffType<Stacking>(), int.MaxValue);
                target.GetGlobalNPC<PostLunarGlobalNPC>().LastHitterRangedLunar = Main.player[proj.owner];
                target.GetGlobalNPC<PostLunarGlobalNPC>().timesHit++;
                target.GetGlobalNPC<PostLunarGlobalNPC>().debuffCooldown = 240;
                target.GetGlobalNPC<PostLunarGlobalNPC>().coolDownLimit = 15;

                int damage = (int)(damageDone * 0.6f);
                if (damage < 1)
                    damage = 1;
                NPC.HitInfo hitAttack = new()
                {
                    Damage = damage,
                    HideCombatText = true,
                    Knockback = 0f
                };
                foreach (NPC closeNPC in Main.ActiveNPCs)
                {
                    if (!closeNPC.friendly && closeNPC.Center.Distance(proj.Center) <= target.width * 1.5f)
                    {
                        closeNPC.StrikeNPC(hitAttack);
                        Color color = hit.Crit ? new Color(Color.Red.R, Color.Red.G + 45, Color.Red.B + 45) : Color.Red;
                        CombatText explosion = ExtensionMethods.CreateCombatText(closeNPC, color, damage.ToString("0"), hit.Crit);
                        Player.addDPS(damage);
                        if (explosion != null)
                            explosion.velocity.Y += hit.Crit ? 30 : 14;
                        if (Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendStrikeNPC(closeNPC, hit);
                    }
                }
            }
        }
    }

    public class RangedLunar : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemIconPulse[Item.type] = true;
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.rare = ModContent.RarityType<MoonFragmentRarity>();
            Item.accessory = true;
            Item.hasVanityEffects = true;
            Item.value = Item.sellPrice(platinum: 2);
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.BlueViolet.ToVector3() * 2f);
            base.Update(ref gravity, ref maxFallSpeed);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RangerLunarModplayer>().active = true;
            base.UpdateAccessory(player, hideVisual);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                 .AddIngredient(ItemID.ReconScope)
                 .AddIngredient(ItemID.EyeoftheGolem)
                 .AddIngredient(ModContent.ItemType<MoonFragment>(), 16)
                 .AddTile(TileID.LunarCraftingStation)
                 .Register();
        }
    }
}