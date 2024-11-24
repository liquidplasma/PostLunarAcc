global using static PostLunarAcc.HelperStats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel.DataAnnotations.Schema;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace PostLunarAcc
{
    /// <summary>
    /// Useful random stuff
    /// </summary>
    internal static class HelperStats
    {
        private static int
            KnifeDustEffect;

        public static int RandomCyanDust => Utils.SelectRandom(Main.rand, 226, 300);

        /// <summary>
        /// Grenade Smoke
        /// </summary>
        public static int GrenadeGore => Main.rand.Next(61, 64);

        /// <summary>
        /// Dust that looks smokey
        /// </summary>
        public static int SmokeyDust => Utils.SelectRandom(Main.rand, DustID.Smoke, DustID.Torch);

        /// <summary>
        /// Goes from 0f to 1f slowly, MP friendly (probably?)
        /// </summary>
        public static float GlobalTick => Main.GameUpdateCount % 1500 / 1500f;

        public static bool TestRange(float numberToCheck, float bottom, float top)
        {
            return numberToCheck >= bottom && numberToCheck <= top;
        }

        public static bool TestRange(int numberToCheck, int bottom, int top)
        {
            return numberToCheck >= bottom && numberToCheck <= top;
        }

        public static void Fire(Vector2 position, int amount, float scale, float magnitudeRange, float rotationRange = MathHelper.TwoPi)
        {
            for (int i = 0; i < amount; ++i)
            {
                Vector2 velocity = Utils.RandomVector2(Main.rand, -magnitudeRange, magnitudeRange).RotatedByRandom(rotationRange);
                Dust.NewDustDirect(position, 0, 0, SmokeyDust, 0, 0, Scale: scale).velocity = velocity;
            }
        }

        public static void SmokeGore(IEntitySource source, Vector2 position, int amount, float magnitudeRange, float rotationRange = MathHelper.TwoPi)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            for (int i = 0; i < amount; ++i)
            {
                Vector2 velocity = Utils.RandomVector2(Main.rand, -magnitudeRange, magnitudeRange).RotatedByRandom(rotationRange);
                Gore.NewGore(source, position, velocity, GrenadeGore);
            }
        }

        ///<summary>
        ///Returns a vector pointing from a source, to a target, with a speed.
        ///Simplifies basic projectile, enemy dash, etc aiming calculations to a single call.
        ///If "ballistic" is true it adjusts for gravity. Default is 0.1f, may be stronger or weaker for some projectiles though.
        ///</summary>
        ///<param name="source">The start point of the vector</param>
        ///<param name="target">The end point it is aiming towards</param>
        ///<param name="speed">The length of the resulting vector</param>
        public static Vector2 GenerateTargetingVector(Vector2 source, Vector2 target, float speed)
        {
            Vector2 distance = target - source;
            distance.Normalize();
            return distance * speed;
        }

        /// <summary>
        /// Returns the index of a nearby NPC
        /// </summary>
        /// <param name="pos">The center position</param>
        /// <param name="dist">The distance</param>
        /// <returns></returns>
        public static NPC FindNearestNPC(Vector2 pos, float dist, bool lineOfSight = true)
        {
            NPC selectedNPC = null;
            float shortestDistance = float.MaxValue;

            foreach (NPC target in Main.ActiveNPCs)
            {
                if (target.friendly || !target.CanBeChasedBy())
                    continue;

                float distance = pos.Distance(target.Center);
                bool canHit = !lineOfSight || Collision.CanHitLine(pos, 1, 1, target.Center, 1, 1);

                // Update nearest NPC if this one is closer and meets the criteria
                if (distance < shortestDistance && canHit)
                {
                    selectedNPC = target;
                    shortestDistance = distance;
                }

            }

            return selectedNPC;
        }

        /// <summary>
        /// Returns the index of a player
        /// </summary>
        /// <param name="pos">The center position</param>
        /// <param name="dist">The distance</param>
        /// <returns></returns>
        public static Player FindNearestPlayer(Vector2 pos, float dist)
        {
            Player playerIndex = null;
            foreach (Player player in Main.ActivePlayers)
            {
                if (!player.dead && pos.DistanceSQ(player.Center) < dist * dist)
                    playerIndex = player;
            }
            return playerIndex;
        }

        public static Vector2 GetDrawOrigin(Texture2D texture)
        {
            return texture.Size() * 0.5f;
        }

        /// <summary>
        /// Returns the amount of active dust in this world
        /// </summary>
        /// <returns></returns>
        public static int GetDustAmount()
        {
            int amount = 0;
            for (int i = 0; i < Main.maxDust; i++)
            {
                Dust dust = Main.dust[i];
                if (dust.active)
                {
                    amount++;
                }
            }
            return amount;
        }

        /// <summary>
        /// Returns the amount of active projectiles in this world
        /// </summary>
        /// <returns></returns>
        public static int GetProjectileAmount()
        {
            int amount = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active)
                {
                    amount++;
                }
            }
            return amount;
        }

        private static Dust QuickDust(Vector2 pos, Color color, int type)
        {
            Dust obj = Dust.NewDustPerfect(pos, type, Vector2.Zero, 0, color, 1f);
            obj.position = pos;
            obj.velocity = Vector2.Zero;
            obj.fadeIn = 1f;
            obj.noLight = true;
            obj.noGravity = true;
            obj.color = color;
            return obj;
        }

        /// <summary>
        /// Draws dust line
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="dustAmount"></param>
        /// <param name="dustType"></param>
        public static void DustLine(Vector2 start, Vector2 end, float splits, Color color, int type)
        {
            QuickDust(start, color, type).scale = 1f;
            QuickDust(end, color, type).scale = 1f;
            float num = 1f / splits;
            for (float num2 = 0f; num2 < 1f; num2 += num)
            {
                QuickDust(Vector2.Lerp(start, end, num2), color, type).scale = 1f;
            }
        }

        /// <summary>
        /// Finds the nearest enemy projectile
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static int FindNearestHostileProj(float radius, Player player)
        {
            int foundProj = -1;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.hostile && projectile.active && projectile.Center.DistanceSQ(player.Center) <= radius * radius)
                {
                    foundProj = projectile.identity;
                }
            }
            return foundProj;
        }

        /// <summary>
        /// Find and return the projectile index of this projectile type. Needs a player so owner check can be made.
        /// </summary>
        /// <param name="player">Owner</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int FindProjectileIndex(Player player, int type)
        {
            int proj = -1;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == type)
                {
                    proj = p.identity;
                }
            }
            return proj;
        }

        public static void KnifeDust(Vector2 position, Projectile proj, int randomNum)
        {
            switch (randomNum)
            {
                case 0:
                    KnifeDustEffect = DustID.Shadowflame;
                    Lighting.AddLight(proj.Center, Color.Purple.ToVector3());
                    break;

                case 1:
                    KnifeDustEffect = DustID.SomethingRed;
                    Lighting.AddLight(proj.Center, Color.Red.ToVector3());
                    break;

                case 2:
                    KnifeDustEffect = DustID.MagicMirror;
                    Lighting.AddLight(proj.Center, Color.AliceBlue.ToVector3());
                    break;
            }
            if (KnifeDustEffect > 0)
            {
                Dust dust = Dust.NewDustDirect(position, proj.width / 2, proj.height, KnifeDustEffect, proj.velocity.X, proj.velocity.Y, 100, default, 1);
                dust.velocity *= 0.25f * Main.rand.NextFloat();
                dust.position -= proj.velocity * 0.5f;
                dust.scale += Main.rand.Next(150) * 0.001f;
                if (Main.rand.NextBool(5))
                    dust.noGravity = true;
            }
        }

        /// <summary>
        /// Gets the index of the NPC that's not the one in the param. This one requires line of sight.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int FindNextNPC(Projectile proj, NPC target, float distance)
        {
            int npc = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC targetNext = Main.npc[i];
                if (targetNext.whoAmI != target.whoAmI && targetNext.active && !targetNext.friendly && targetNext.CanBeChasedBy() && !targetNext.CountsAsACritter && proj.DistanceSQ(targetNext.Center) <= distance * distance && Collision.CanHitLine(proj.Center, 1, 1, targetNext.Center, 1, 1))
                {
                    npc = targetNext.whoAmI;
                }
            }
            return npc;
        }

        /// <summary>
        /// Returns NPC index from projectile center
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="maxRange"></param>
        /// <returns></returns>
        public static int FindTargetProjectileCenter(Projectile proj, float maxRange)
        {
            float close = maxRange * maxRange;
            int result = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];
                bool canBeChased = target.CanBeChasedBy();
                if (proj.localNPCImmunity[i] != 0)
                {
                    canBeChased = false;
                }
                if (!canBeChased)
                {
                    continue;
                }
                else
                {
                    float distance = proj.Center.DistanceSQ(target.Center);
                    if (distance < close)
                    {
                        close = MathHelper.Min(distance, close);
                        result = target.whoAmI;
                    }
                }
            }
            return result;
        }

        /// <summary> Returns a NPC index within the range of maxRange. This one does NOT requires line of sight.</summary>>
        public static int FindTargetNoLOS(Projectile proj, float maxRange)
        {
            float num = maxRange;
            int result = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC nPC = Main.npc[i];
                bool canBeChased = nPC.CanBeChasedBy();
                if (proj.localNPCImmunity[i] != 0)
                {
                    canBeChased = false;
                }
                if (canBeChased)
                {
                    Player player = Main.player[proj.owner];
                    float num2 = player.Distance(nPC.Center);
                    if (num2 < num)
                    {
                        num = MathHelper.Min(num2, num);
                        result = i;
                    }
                }
            }
            return result;
        }

        /// <summary> Returns a NPC index within the range of maxRange. This one requires line of sight from player center.</summary>>
        public static int FindTargetLOSPlayer(Projectile proj, float maxRange, Player player)
        {
            float num = maxRange;
            int result = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC nPC = Main.npc[i];
                bool flag = nPC.CanBeChasedBy();
                if (proj.localNPCImmunity[i] != 0)
                {
                    flag = false;
                }
                if (flag)
                {
                    float num2 = player.Distance(nPC.Center);
                    if (num2 < num && Collision.CanHitLine(player.Center, 1, 1, nPC.Center, 1, 1))
                    {
                        num = MathHelper.Min(num2, num);
                        result = i;
                    }
                }
            }
            return result;
        }

        /// <summary> Returns a NPC index within the range of maxRange. This one requires line of sight from Projectile center.</summary>>
        public static int FindTargetLOSProjectile(Projectile proj, float maxRange)
        {
            float num = maxRange;
            int result = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC nPC = Main.npc[i];
                bool flag = nPC.CanBeChasedBy();
                if (proj.localNPCImmunity[i] != 0)
                {
                    flag = false;
                }
                if (flag)
                {
                    float num2 = proj.Distance(nPC.Center);
                    if (num2 < num && Collision.CanHitLine(proj.Center, 1, 1, nPC.Center, 1, 1))
                    {
                        num = MathHelper.Min(num2, num);
                        result = i;
                    }
                }
            }
            return result;
        }
    }
}