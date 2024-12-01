using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PostLunarAcc.Projectiles;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc
{
    /// <summary>
    /// Contains a Player instance
    /// </summary>
    public abstract class ModProjectileImproved : ModProjectile
    {
        /// <summary>
        /// Player => Main.player[Projectile.owner];
        /// </summary>
        public Player Player => Main.player[Projectile.owner];

        public List<CustomVertexInfo> Vertices = [];

        public HelperWraithTracking TrackStats => Player.GetModPlayer<HelperWraithTracking>();
        public Effect glowEffect = ModContent.Request<Effect>("PostLunarAcc/Assets/GlowEffect", AssetRequestMode.ImmediateLoad).Value;

        private Color GlowColor(Color color, float intensity)
        {
            float alphaFactor = color.A / 255f;
            return new Color(
                (byte)MathHelper.Clamp(color.R * intensity * alphaFactor, 0, 255),
                (byte)MathHelper.Clamp(color.G * intensity * alphaFactor, 0, 255),
                (byte)MathHelper.Clamp(color.B * intensity * alphaFactor, 0, 255),
                color.A
            );
        }

        public void DrawTrail(Color color)
        {
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 position = Projectile.oldPos[i];
                Vector2 direction = Projectile.oldPos[i + 1] - position;
                direction.Normalize();
                color = GlowColor(color, 0.6f);
                float width = MathHelper.Lerp(10f, 4f, progress);
                color = Color.Lerp(color, Color.Transparent, progress);

                Vector2 offset = new Vector2(-direction.Y, direction.X) * width / 2f;

                Vertices.Add(new CustomVertexInfo(position + offset - Main.screenPosition, color, new Vector2(progress, 0)));
                Vertices.Add(new CustomVertexInfo(position - offset - Main.screenPosition, color, new Vector2(progress, 1)));
            }

            Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("PostLunarAcc/Assets/trailTexture").Value;
            glowEffect.CurrentTechnique.Passes[0].Apply();
            Main.graphics.GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleStrip,
                Vertices.ToArray(),
                0,
                Vertices.Count - 2
            );
            Vertices.Clear();
        }
    }

    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class PostLunarAcc : Mod
    {
        public enum PacketType
        {
            RangedLunarStacksToServer,

            RangedLunarStacksToPlayers,

            RangedLunarExplosion,

            SoulboundProjectile,

            SoulboundActivationServer,

            SoulboundActivationPlayers
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte packetType = reader.ReadByte();
            switch (packetType)
            {
                case (byte)PacketType.RangedLunarStacksToPlayers:
                    {
                        int targetIndex = reader.Read7BitEncodedInt();
                        if (Main.npc.IndexInRange(targetIndex))
                        {
                            NPC target = Main.npc[targetIndex];
                            target.GetGlobalNPC<PostLunarGlobalNPC>().toExplode = true;
                            target.GetGlobalNPC<PostLunarGlobalNPC>().timesHit++;
                            target.GetGlobalNPC<PostLunarGlobalNPC>().debuffCooldown = 240;
                            target.GetGlobalNPC<PostLunarGlobalNPC>().coolDownLimit = 15;
                        }
                        break;
                    }
                case (byte)PacketType.RangedLunarStacksToServer:
                    {
                        int targetIndex = reader.Read7BitEncodedInt();
                        int ignoreClient = reader.Read7BitEncodedInt();
                        if (Main.npc.IndexInRange(targetIndex))
                        {
                            NPC target = Main.npc[targetIndex];
                            target.GetGlobalNPC<PostLunarGlobalNPC>().toExplode = true;
                            target.GetGlobalNPC<PostLunarGlobalNPC>().timesHit++;
                            target.GetGlobalNPC<PostLunarGlobalNPC>().debuffCooldown = 240;
                            target.GetGlobalNPC<PostLunarGlobalNPC>().coolDownLimit = 15;
                            if (Main.dedServ)
                            {
                                var instance = ModContent.GetInstance<PostLunarAcc>().GetPacket();
                                instance.Write((byte)PacketType.RangedLunarStacksToPlayers);
                                instance.Write7BitEncodedInt(target.whoAmI);
                                instance.Send(ignoreClient: ignoreClient);
                            }
                        }
                        break;
                    }
                case (byte)PacketType.RangedLunarExplosion:
                    {
                        int targetIndex = reader.Read7BitEncodedInt();
                        if (Main.npc.IndexInRange(targetIndex))
                        {
                            NPC target = Main.npc[targetIndex];
                            target.GetGlobalNPC<PostLunarGlobalNPC>().toExplode = false;
                            Vector2 position = target.Center;
                            int playerIndex = reader.Read7BitEncodedInt();
                            Player Player = Main.player[playerIndex];
                            if (!Player.active)
                                return;
                            if (Main.myPlayer == Player.whoAmI)
                                Projectile.NewProjectileDirect(target.GetSource_Death(), position + new Vector2(0, -64), Vector2.Zero, ProjectileID.DD2ExplosiveTrapT3Explosion, 0, 0, Player.whoAmI);
                            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, position);
                        }
                        break;
                    }
                case (byte)PacketType.SoulboundActivationServer:
                    {
                        int targetIndex = reader.Read7BitEncodedInt();
                        int ignoreClient = reader.Read7BitEncodedInt();
                        if (Main.npc.IndexInRange(targetIndex))
                        {
                            NPC target = Main.npc[targetIndex];
                            target.GetGlobalNPC<PostLunarGlobalNPC>().soulboundActive = true;
                            if (Main.dedServ)
                            {
                                var instance = ModContent.GetInstance<PostLunarAcc>().GetPacket();
                                instance.Write((byte)PacketType.SoulboundActivationPlayers);
                                instance.Write7BitEncodedInt(target.whoAmI);
                                instance.Send(ignoreClient: ignoreClient);
                            }
                        }
                        break;
                    }
                case (byte)PacketType.SoulboundActivationPlayers:
                    {
                        int targetIndex = reader.Read7BitEncodedInt();
                        if (Main.npc.IndexInRange(targetIndex))
                        {
                            NPC target = Main.npc[targetIndex];
                            target.GetGlobalNPC<PostLunarGlobalNPC>().soulboundActive = true;
                        }
                        break;
                    }
                case (byte)PacketType.SoulboundProjectile:
                    {
                        int targetIndex = reader.Read7BitEncodedInt();
                        if (Main.npc.IndexInRange(targetIndex))
                        {
                            NPC target = Main.npc[targetIndex];
                            Vector2 position = target.Center;
                            int playerIndex = reader.Read7BitEncodedInt();
                            Player Player = Main.player[playerIndex];
                            if (!Player.active)
                                return;
                            if (Main.myPlayer == Player.whoAmI)
                                Projectile.NewProjectileDirect(target.GetSource_Death(), position, Vector2.UnitY * -2f, ModContent.ProjectileType<HelperWraithPellets>(), 0, 0, Player.whoAmI);
                            if (Main.LocalPlayer.TryGetModPlayer(out HelperWraithTracking result))
                                result.SoulboundNPCs.Remove(target);
                            target.GetGlobalNPC<PostLunarGlobalNPC>().soulboundActive = false;
                        }
                        break;
                    }

                default:
                    break;
            }
            base.HandlePacket(reader, whoAmI);
        }
    }
}