using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PostLunarAcc.Items.Ingredients;
using PostLunarAcc.Projectiles;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc
{
    internal class PostLunarGlobalNPC : GlobalNPC
    {
        public bool toExplode, soulboundActive;

        public int lastHitDamage, timesHit, debuffCooldown, cooldownTimer, coolDownLimit;

        private float soulbouldRot;

        /// <summary>
        /// Player who hit a NPC last for ranged lunar
        /// </summary>
        public Player LastHitterRangedLunar { get; set; }

        /// <summary>
        /// Player who hit a NPC last for soulbinding
        /// </summary>
        public Player LastHitterSoulbinding { get; set; }

        private Texture2D Marked => ModContent.Request<Texture2D>("PostLunarAcc/Assets/marked", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        private Texture2D SoulBound => ModContent.Request<Texture2D>("PostLunarAcc/Assets/SoulBound", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        public override bool InstancePerEntity => true;

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (toExplode && timesHit > 0)
            {
                damage = timesHit * 13;
                npc.lifeRegen -= timesHit * 52;
                if (debuffCooldown > 0)
                    debuffCooldown--;
            }
            if (debuffCooldown == 0)
            {
                cooldownTimer++;
                if (cooldownTimer >= coolDownLimit && timesHit > 0)
                {
                    if (Main.rand.NextBool(5) && coolDownLimit >= 6)
                        coolDownLimit--;
                    cooldownTimer = 0;
                    timesHit--;
                }
            }
            if (timesHit < 0)
                timesHit = 0;
            base.UpdateLifeRegen(npc, ref damage);
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (toExplode && !npc.friendly)
            {
                Vector2 position = npc.Top - new Vector2(0, 16) - Main.screenPosition;
                Main.EntitySpriteDraw(Marked,
                    position,
                    Marked.Bounds,
                    Color.Wheat,
                    npc.velocity.X * 0.05f,
                    Marked.Size() * 0.5f,
                    1,
                    SpriteEffects.None,
                    0);
                if (timesHit > 0)
                {
                    spriteBatch.DrawString(FontAssets.ItemStack.Value, timesHit + "x", position - new Vector2(16, -8), Color.Black, 0, Marked.Size() / 2, 1.1f, 0, 0);
                    spriteBatch.DrawString(FontAssets.ItemStack.Value, timesHit + "x", position - new Vector2(16, -8), Color.White, 0, Marked.Size() / 2, 1f, 0, 0);
                }
            }
            if (soulboundActive && !npc.friendly)
            {
                soulbouldRot += 0.063f;
                Vector2 position = npc.Center;
                Main.EntitySpriteDraw(SoulBound,
                    position - Main.screenPosition,
                    SoulBound.Bounds,
                    Color.Wheat,
                    soulbouldRot,
                    SoulBound.Size() * 0.5f,
                    npc.Size.Length() / 125f,
                    SpriteEffects.None,
                    0);
            }
            base.PostDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (toExplode && !npc.friendly)
            {
                Lighting.AddLight(npc.Center, Color.Red.ToVector3() * 0.33f);
                for (int i = 0; i < 3; i++)
                {
                    Dust dusty = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Scorpion);
                    dusty.velocity = Utils.RandomVector2(Main.rand, -1, 1);
                    dusty.color = Color.Red;
                    dusty.scale = 0.8f;
                    dusty.noGravity = true;
                }
            }
        }

        public override void OnKill(NPC npc)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                if (soulboundActive && !npc.friendly)
                {
                    var instance = ModContent.GetInstance<PostLunarAcc>().GetPacket();
                    instance.Write((byte)PostLunarAcc.PacketType.SoulboundProjectile);
                    instance.WriteVector2(npc.Center);
                    instance.Write7BitEncodedInt(LastHitterSoulbinding.whoAmI);
                    instance.Send();
                    ExtensionMethods.Announce("Sent modpacket: " + instance.ToString());
                    soulboundActive = false;
                }
                if (toExplode && !npc.friendly)
                {
                    var instance = ModContent.GetInstance<PostLunarAcc>().GetPacket();
                    NPC.HitInfo explodeDamage = new()
                    {
                        Damage = npc.damage * 4 + (int)(npc.lifeMax * 0.15f),
                        Knockback = 0,
                        HideCombatText = true
                    };
                    foreach (NPC nearby in Main.ActiveNPCs)
                    {
                        if (!nearby.friendly && nearby.whoAmI != npc.whoAmI && nearby.Distance(npc.Center) <= (npc.height + npc.width) * 2)
                        {
                            nearby.StrikeNPC(explodeDamage);
                            ExtensionMethods.CreateCombatText(nearby, Color.Red, explodeDamage.Damage.ToString("0"));
                            if (Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendStrikeNPC(nearby, explodeDamage, LastHitterRangedLunar.whoAmI);
                            LastHitterRangedLunar.addDPS(explodeDamage.Damage);
                        }
                    }
                    instance.Write((byte)PostLunarAcc.PacketType.RangedLunarExplosion);
                    instance.WriteVector2(npc.Center);
                    instance.Write(LastHitterRangedLunar.whoAmI);
                    instance.Send();
                    ExtensionMethods.Announce("Sent modpacket: " + instance.ToString());
                    toExplode = false;
                }
            }
            else
            {
                if (soulboundActive && !npc.friendly)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectileDirect(npc.GetSource_Death(), npc.position + npc.Size * Main.rand.NextFloat(), Vector2.UnitY * -2f, ModContent.ProjectileType<HelperWraithPellets>(), 0, 0, LastHitterSoulbinding.whoAmI);
                    soulboundActive = false;
                }
                if (toExplode && !npc.friendly)
                {
                    NPC.HitInfo explodeDamage = new()
                    {
                        Damage = npc.damage * 4 + (int)(npc.lifeMax * 0.15f),
                        Knockback = 0,
                        HideCombatText = true
                    };
                    foreach (NPC nearby in Main.ActiveNPCs)
                    {
                        if (!nearby.friendly && nearby.whoAmI != npc.whoAmI && nearby.Distance(npc.Center) <= (npc.height + npc.width) * 2)
                        {
                            nearby.StrikeNPC(explodeDamage);
                            ExtensionMethods.CreateCombatText(nearby, Color.Red, explodeDamage.Damage.ToString("0"));
                            if (Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendStrikeNPC(nearby, explodeDamage, LastHitterRangedLunar.whoAmI);
                            LastHitterRangedLunar.addDPS(explodeDamage.Damage);
                        }
                    }
                    Vector2 position = npc.Center;
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, position);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectileDirect(npc.GetSource_Death(), npc.Center + new Vector2(0, -64), Vector2.Zero, ProjectileID.DD2ExplosiveTrapT3Explosion, 0, 0, LastHitterRangedLunar.whoAmI);
                    toExplode = false;
                }
            }
            base.OnKill(npc);
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            switch (npc.type)
            {
                case NPCID.MoonLordCore:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MoonFragment>(), 1, 16, 32));
                    break;

                case NPCID.Paladin:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PaladinBar>(), 1, 20, 40));
                    break;
            }
            base.ModifyNPCLoot(npc, npcLoot);
        }
    }
}