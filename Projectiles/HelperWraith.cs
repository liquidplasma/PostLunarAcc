using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc.Projectiles
{
    public class HelperWraithTracking : ModPlayer
    {
        public bool soulbindingActive;

        public Projectile WraithInstance;

        public List<NPC> SoulboundNPCs = [];

        public override void ResetEffects()
        {
            soulbindingActive = false;
            base.ResetEffects();
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (soulbindingActive && WraithInstance != null && (proj.DamageType == DamageClass.Summon || proj.DamageType == DamageClass.SummonMeleeSpeed))
            {
                target.GetGlobalNPC<PostLunarGlobalNPC>().soulboundActive = true;
                target.GetGlobalNPC<PostLunarGlobalNPC>().LastHitterSoulbinding = Player;
                if (!SoulboundNPCs.Contains(target))
                    SoulboundNPCs.Add(target);
            }
            base.OnHitNPCWithProj(proj, target, hit, damageDone);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (soulbindingActive && WraithInstance != null && (item.DamageType == DamageClass.Summon || item.DamageType == DamageClass.SummonMeleeSpeed))
            {
                target.GetGlobalNPC<PostLunarGlobalNPC>().soulboundActive = true;
                target.GetGlobalNPC<PostLunarGlobalNPC>().LastHitterSoulbinding = Player;
                if (!SoulboundNPCs.Contains(target))
                    SoulboundNPCs.Add(target);
            }
            base.OnHitNPCWithItem(item, target, hit, damageDone);
        }
    }

    public class HelperWraith : ModProjectileImproved
    {
        public int healDelay;

        private ref float AttackTimer => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 44;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.tileCollide = false;
            Projectile.extraUpdates += 1;
            base.SetDefaults();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }

        public override void AI()
        {
            AttackTimer++;
            TrackStats.soulbindingActive = true;
            TrackStats.WraithInstance = Projectile;
            Projectile.KeepAliveIfOwnerIsAlive(Player);
            Projectile.spriteDirection = Player.direction;
            Vector2 restSpot;
            if (Player.direction == -1)
                restSpot = Player.Center - new Vector2(32, 32);
            else
                restSpot = Player.Center - new Vector2(-32, 32);

            Projectile.SmoothHoming(restSpot, 1f, 16f);
            if (healDelay > 0)
            {
                Player.Heal(20);
                healDelay--;
            }
            Projectile.Animate(12);
            if (AttackTimer > 60)
            {
                foreach (NPC target in TrackStats.SoulboundNPCs)
                {
                    if (target != null && target.active)
                    {
                        if (Player.whoAmI == Main.myPlayer)
                        {
                            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Utils.RandomVector2(Main.rand, -8f, 8f), ModContent.ProjectileType<HelperWraithShot>(), (int)(Projectile.damage * 0.8f), 4f);
                            proj.ai[0] = target.whoAmI;
                        }
                    }
                }
                AttackTimer = 0;
            }
            base.AI();
        }
    }

    public class HelperWraithPellets : ModProjectileImproved
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 22;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates += 1;
            base.SetDefaults();
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(0, Main.projFrames[Type]);
            base.OnSpawn(source);
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.velocity *= 0.99f;
            Dust dusty = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Scorpion);
            dusty.velocity = Projectile.rotation.ToRotationVector2() * 0.1f;
            dusty.noGravity = true;
            if (TrackStats.WraithInstance != null && TrackStats.WraithInstance.active && Projectile.ai[0] >= 180 && TrackStats.WraithInstance.ModProjectile is HelperWraith healMe)
            {
                Projectile.SmoothHoming(TrackStats.WraithInstance.Center, 1f, 16f);
                if (Projectile.Hitbox.Intersects(TrackStats.WraithInstance.Hitbox))
                {
                    healMe.healDelay = 1;
                    Projectile.Kill();
                }
            }
            base.AI();
        }
    }

    public class HelperWraithShot : ModProjectileImproved
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        private NPC Target
        {
            get
            {
                if (Main.npc.IndexInRange((int)Projectile.ai[0]))
                    return Main.npc[(int)Projectile.ai[0]];
                return null;
            }
        }

        private ref float Timer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 3;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = ContentSamples.ProjectilesByType[ProjectileID.Bullet].width;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates += 1;
            Projectile.alpha = 255;
            base.SetDefaults();
        }

        public override bool? CanHitNPC(NPC target)
        {
            return Target != null && Target == target;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            return base.PreDraw(ref lightColor);
        }

        public override void AI()
        {
            Timer++;
            if (Timer >= 60 && Target != null && Target.active)
            {
                Projectile.SmoothHoming(Target.Center, 8f, 16f);
            }
            base.AI();
        }
    }
}