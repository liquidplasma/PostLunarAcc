using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PostLunarAcc.Items.Accessories;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace PostLunarAcc
{
    internal class PostLunarGlobalProjectile : GlobalProjectile
    {
        public bool HolyKnightCounter = false;

        public float InitialVelocity;

        private Player Owner(Projectile proj) => Main.player[proj.owner];

        public override bool InstancePerEntity => true;

        public override void SetDefaults(Projectile entity)
        {
            base.SetDefaults(entity);
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (projectile.type == ProjectileID.Daybreak && source is EntitySource_Parent parent && parent.Context is "HolyKnightCounter")
            {
                projectile.tileCollide = false;
                projectile.usesLocalNPCImmunity = true;
                projectile.localNPCHitCooldown = -1;
                InitialVelocity = projectile.velocity.Length();
                HolyKnightCounter = true;
            }
            base.OnSpawn(projectile, source);
        }

        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            if (projectile.type == ProjectileID.Daybreak && HolyKnightCounter && !target.CanBeChasedBy())
                return false;

            return base.CanHitNPC(projectile, target);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (projectile.type == ProjectileID.Daybreak && HolyKnightCounter && projectile.ai[1] <= 45)
            {
                int shaderID = ContentSamples.ItemsByType[ItemID.BrightGreenDye].dye;
                Main.instance.PrepareDrawnEntityDrawing(projectile, shaderID, null);
                Texture2D texture = TextureAssets.Projectile[projectile.type].Value;
                Rectangle rect = texture.Bounds;
                Main.EntitySpriteDraw(texture, projectile.Center - Main.screenPosition, rect, Color.White, projectile.rotation, rect.Size() / 2, projectile.scale, SpriteEffects.None, 0);
                return false;
            }
            return base.PreDraw(projectile, ref lightColor);
        }

        public override void AI(Projectile projectile)
        {
            if (projectile.type == ProjectileID.Daybreak && HolyKnightCounter && projectile.ai[1] <= 45f)
            {
                Dust velocityTrail = Dust.NewDustPerfect(projectile.Center, DustID.SolarFlare);
                velocityTrail.velocity = -projectile.velocity.RotatedByRandom(MathHelper.ToRadians(10)) * 0.2f;
                velocityTrail.noGravity = true;
                velocityTrail.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.BrightGreenDye);
                velocityTrail.scale = 1f;

                NPC target = FindNearestNPC(Owner(projectile).Center, 16 * 64, false);
                if (target != null && projectile.ai[0] != 1f)
                    projectile.SmoothHoming(target.Center, 2f, 16f);
            }
            base.AI(projectile);
        }

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            if (projectile.type == ProjectileID.Daybreak)
                bitWriter.WriteBit(HolyKnightCounter);
            base.SendExtraAI(projectile, bitWriter, binaryWriter);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            if (projectile.type == ProjectileID.Daybreak)
                HolyKnightCounter = bitReader.ReadBit();
            base.ReceiveExtraAI(projectile, bitReader, binaryReader);
        }
    }
}