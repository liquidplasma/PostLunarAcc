using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PostLunarAcc.Projectiles;
using System.Collections.Generic;
using Terraria;
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

        private List<CustomVertexInfo> vertices = new List<CustomVertexInfo>();

        public HelperWraithTracking TrackStats => Player.GetModPlayer<HelperWraithTracking>();

        public void DrawTrail()
        {
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 position = Projectile.oldPos[i];
                Vector2 direction = Projectile.oldPos[i + 1] - position;
                direction.Normalize();

                float width = MathHelper.Lerp(10f, 2f, progress);
                Color color = Color.Lerp(Color.Red, Color.Transparent, progress);

                Vector2 offset = new Vector2(-direction.Y, direction.X) * width / 2f;

                vertices.Add(new CustomVertexInfo(position + offset, color, new Vector2(progress, 0)));
                vertices.Add(new CustomVertexInfo(position - offset, color, new Vector2(progress, 1)));
            }

            Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("PostLunarAcc/Assets/trailTexture").Value;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleStrip,
                vertices.ToArray(),
                0,
                vertices.Count - 2
            );
        }
    }

    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class PostLunarAcc : Mod
    {
    }
}