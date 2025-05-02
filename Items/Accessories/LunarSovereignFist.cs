using Microsoft.Xna.Framework;
using PostLunarAcc.Items.Ingredients;
using PostLunarAcc.Rarity;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc.Items.Accessories
{
    public class LunarSovereign : ModPlayer
    {
        public bool isActive;

        public Item Accessory = null;

        public int explosionCooldown;

        public override void ResetEffects()
        {
            isActive = false;
            if (explosionCooldown > 0)
                explosionCooldown--;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (isActive && (hit.DamageType == DamageClass.Melee || hit.DamageType == DamageClass.MeleeNoSpeed))
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    var instance = ModContent.GetInstance<PostLunarAcc>().GetPacket();
                    instance.Write((byte)PostLunarAcc.PacketType.SovereignAddServer);
                    instance.Write7BitEncodedInt(target.whoAmI);
                    instance.Write7BitEncodedInt(Player.whoAmI);
                    instance.Send();
                }
                target.GetGlobalNPC<PostLunarGlobalNPC>().sovereignHits++;
                int damage = (int)(damageDone * 0.75);
                if (Main.myPlayer == Player.whoAmI && Accessory != null)
                {
                    Projectile.NewProjectile(Player.GetSource_Accessory(Accessory), target.Center, Vector2.Zero, ProjectileID.Volcano, 0, 0f, Player.whoAmI);
                    foreach (NPC closeNPC in Main.ActiveNPCs)
                    {
                        if (!closeNPC.friendly && closeNPC.Center.Distance(Player.itemLocation) <= target.width * 1.5f)
                        {
                            NPC.HitInfo hitAttack = new()
                            {
                                Damage = damage,
                                DamageType = DamageClass.Melee,
                                Knockback = 0f
                            };
                            closeNPC.StrikeNPC(hitAttack);
                            Player.addDPS(damage);
                            if (Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendStrikeNPC(closeNPC, hit);
                        }
                    }
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (isActive && (hit.DamageType == DamageClass.Melee || hit.DamageType == DamageClass.MeleeNoSpeed))
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    var instance = ModContent.GetInstance<PostLunarAcc>().GetPacket();
                    instance.Write((byte)PostLunarAcc.PacketType.SovereignAddServer);
                    instance.Write7BitEncodedInt(target.whoAmI);
                    instance.Write7BitEncodedInt(Player.whoAmI);
                    instance.Send();
                }
                target.GetGlobalNPC<PostLunarGlobalNPC>().sovereignHits++;
                int damage = (int)(damageDone * 0.75);
                if (Main.myPlayer == Player.whoAmI && Accessory != null)
                {
                    Projectile.NewProjectile(Player.GetSource_Accessory(Accessory), target.Center, Vector2.Zero, ProjectileID.Volcano, 0, 0f, Player.whoAmI);
                    foreach (NPC closeNPC in Main.ActiveNPCs)
                    {
                        if (!closeNPC.friendly && closeNPC.Center.Distance(proj.Center) <= target.width * 1.5f)
                        {
                            NPC.HitInfo hitAttack = new()
                            {
                                Damage = damage,
                                DamageType = DamageClass.Melee,
                                Knockback = 0f
                            };
                            closeNPC.StrikeNPC(hitAttack);
                            Player.addDPS(damage);                            
                            if (Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendStrikeNPC(closeNPC, hit);
                        }
                    }
                }
            }
            base.OnHitNPCWithProj(proj, target, hit, damageDone);
        }
    }

    internal class LunarSovereignFist : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.rare = ModContent.RarityType<MoonFragmentRarity>();
            Item.accessory = true;
            Item.hasVanityEffects = true;
            Item.value = Item.sellPrice(platinum: 2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.25f;
            player.GetDamage(DamageClass.MeleeNoSpeed) += 0.25f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
            player.GetAttackSpeed(DamageClass.MeleeNoSpeed) += 0.25f;
            player.GetCritChance(DamageClass.Melee) += 20;
            player.GetCritChance(DamageClass.MeleeNoSpeed) += 20;
            player.Sovereign().isActive = true;
            player.Sovereign().Accessory = Item;
            base.UpdateAccessory(player, hideVisual);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FireGauntlet)
                .AddIngredient(ModContent.ItemType<MoonFragment>(), 16)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}