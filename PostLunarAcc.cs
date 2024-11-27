using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PostLunarAcc.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using System.IO;
using Terraria.Audio;
using Terraria.ID;

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

        private List<CustomVertexInfo> Vertices = new();

        public HelperWraithTracking TrackStats => Player.GetModPlayer<HelperWraithTracking>();

        public void DrawTrail(Color color)
        {
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 position = Projectile.oldPos[i];
                Vector2 direction = Projectile.oldPos[i + 1] - position;
                direction.Normalize();

                float width = MathHelper.Lerp(10f, 2f, progress);
                color = Color.Lerp(color, Color.Transparent, progress); ;

                Vector2 offset = new Vector2(-direction.Y, direction.X) * width / 2f;

                Vertices.Add(new CustomVertexInfo(position + offset - Main.screenPosition, color, new Vector2(progress, 0)));
                Vertices.Add(new CustomVertexInfo(position - offset - Main.screenPosition, color, new Vector2(progress, 1)));
            }

            Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("PostLunarAcc/Assets/trailTexture").Value;
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
            RangedLunarExplosion,

            SoulboundProjectile
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte packetType = reader.ReadByte();
            switch (packetType)
            {
                case (byte)PacketType.RangedLunarExplosion:
                    {
                        Vector2 position = reader.ReadVector2();
                        int lastHitterLunar = reader.ReadInt32();
                        Player Player = Main.player[lastHitterLunar];
                        Projectile.NewProjectileDirect(Player.GetSource_FromThis(), position + new Vector2(0, -64), Vector2.Zero, ProjectileID.DD2ExplosiveTrapT3Explosion, 0, 0, Player.whoAmI);
                        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, position);
                        ExtensionMethods.Announce("Packet received: " + packetType.ToString());
                        break;
                    }
                case (byte)PacketType.SoulboundProjectile:
                    {
                        Vector2 position = reader.ReadVector2();
                        int lastHitterSoulbind = reader.ReadInt32();
                        Player Player = Main.player[lastHitterSoulbind];
                        Projectile.NewProjectileDirect(Player.GetSource_FromThis(), position, Vector2.UnitY * -2f, ModContent.ProjectileType<HelperWraithPellets>(), 0, 0, Player.whoAmI);
                        ExtensionMethods.Announce("Packet received: " + packetType.ToString());
                        break;
                    }
            }
            base.HandlePacket(reader, whoAmI);
        }
    }
}