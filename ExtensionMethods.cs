using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PostLunarAcc
{
    /// <summary>
    /// Useful extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        public static void SpawnNPC(this NPC _, IEntitySource source, Vector2 position, int type)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int index = NPC.NewNPC(source, (int)position.X, (int)position.Y, type);
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
                return;
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
                NPC.NewNPCDirect(source, position, type);
        }

        /// <summary>
        /// This EntityDraw call already subtracts screen position
        /// <code>position - Main.screenPosition</code>
        /// </summary>
        public static void BetterEntityDraw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float worthless = 0f) => Main.EntitySpriteDraw(texture, position - Main.screenPosition, sourceRectangle, color, rotation, origin, new Vector2(scale), effects, worthless);

        public static CombatText CreateCombatText(Player sourcePlayer, Color color, string text, bool dramatic = true, bool dot = false)
        {
            int combatText = CombatText.NewText(sourcePlayer.getRect(), color, text, dramatic, dot);
            if (combatText < 100)
                return Main.combatText[combatText];
            return null;
        }

        public static CombatText CreateCombatText(NPC sourceNPC, Color color, string text, bool dramatic = true, bool dot = false)
        {
            int combatText = CombatText.NewText(sourceNPC.getRect(), color, text, dramatic, dot);
            if (combatText < 100)
                return Main.combatText[combatText];
            return null;
        }

        /// <summary>
        /// Uses either ChatHelper.BroadcastChatMessage or Main.NewText to send messages
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="substitutions"></param>
        public static void Announce(string text, Color color = default, params string[] substitutions)
        {
            if (color == default)
                color = Color.White;

            if (Main.netMode == NetmodeID.Server)
            {
                ChatHelper.BroadcastChatMessage(
                    NetworkText.FromKey(text, substitutions.Select(text =>
                        NetworkText.FromKey(text)
                    ).ToArray()), color);
            }
            else
            {
                Main.NewText(
                    Language.GetTextValue(text, substitutions.Select(text =>
                        Language.GetTextValue(text)
                    ).ToArray()), color);
            }
        }

        /// <summary>
        /// Finds said item type in the player inventory
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Item FindItemInInventory(this Player player, int type)
        {
            Item Item = null;
            for (int i = 0; i < player.inventory.Length; i++)
            {
                Item p = player.inventory[i];
                if (p != null && !p.IsAir && p.active && p.type == type)
                {
                    Item = p;
                }
            }
            return Item;
        }

        /// <summary>
        /// Used on my held projectiles
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="Projectile"></param>
        public static int HoldOutArm(this Player Player, Projectile Projectile, Vector2 angleVector)
        {
            //Vanilla code below, ech
            float angleFloat = MathF.Sin(Player.Center.AngleTo(angleVector));
            bool addAngle = HelperStats.TestRange(angleFloat, 0.12f, 1f);
            angleFloat /= 2f;
            float num7 = -MathF.PI / 10f;
            if (Player.direction == -1)
            {
                num7 *= -1f;
            }
            float num8 = Projectile.rotation - MathF.PI / 4f + MathF.PI;
            if (Player.direction == 1)
            {
                num8 += MathF.PI / 2f;
            }
            float rotation = num8 + num7;
            if (Player.direction == 1)
            {
                if (addAngle)
                    rotation -= angleFloat;
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
                return 1;
            }
            else
            {
                if (addAngle)
                    rotation += angleFloat;
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
                return -1;
            }
        }

        public static bool ShiftDown
        {
            get
            {
                // Shamelessly taken from https://github.com/SamsonAllen13/ClickerClass/blob/master/Items/ClickerItemCore.cs#L266
                var keys = PlayerInput.CurrentProfile.InputModes[InputMode.Keyboard].KeyStatus[TriggerNames.SmartSelect];
                string key = keys.Count == 0 ? null : keys[0];
                return key == null || PlayerInput.Triggers.Current.SmartSelect;
            }
        }

        public static void Animate(this Projectile proj, int tickPerFrame)
        {
            if (++proj.frameCounter >= tickPerFrame)
            {
                proj.frameCounter = 0;
                proj.frame = ++proj.frame % Main.projFrames[proj.type];
            }
        }

        /// <summary>
        /// Returns the player stealth when he is Vortex
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static float GetStealth(this Player player)
        {
            if (player.shroomiteStealth)
                return Math.Clamp(Math.Abs(player.stealth - 2f), 0f, 1.6f);
            return Math.Clamp(Math.Abs(player.stealth - 2f), 0f, 1.8f);
        }

        /// <summary>
        /// Associated texture for this Item type
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Texture2D MyTexture(this Item item) => TextureAssets.Item[item.type].Value;

        /// <summary>
        /// Associated texture for this Item type, with some useful outs
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Texture2D MyTexture(this Item Item, out Vector2 origin)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            origin = tex.Size() / 2;
            return tex;
        }

        /// <summary>
        /// Associated texture for this projectile type
        /// </summary>
        /// <param name="projectile"></param>
        /// <returns></returns>
        public static Texture2D MyTexture(this Projectile projectile) => TextureAssets.Projectile[projectile.type].Value;

        /// <summary>
        /// Associated texture for this projectile type, with some useful outs
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Texture2D MyTexture(this Projectile projectile, out Vector2 origin, out SpriteEffects dir)
        {
            dir = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
            origin = tex.Size() / 2;
            return tex;
        }

        /// <summary>
        /// This projectile will rotate based on it's velocity with the following formula
        /// <para>projectile.rotation += (projectile.velocity.X * 0.04f) + (projectile.velocity.Y * 0.04f)</para>
        /// </summary>
        /// <param name="projectile"></param>
        public static void RotateBasedOnVelocity(this Projectile projectile, float mult = 0.04f) => projectile.rotation += (projectile.velocity.X * mult) + (projectile.velocity.Y * mult);

        /// <summary>
        /// Projectile will bounce back, best use in OnTileCollide
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="radians">Radians rotated randomly when bounced</param>
        public static void Bounce(this Projectile proj, int radians = 0)
        {
            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(proj.velocity.X - proj.oldVelocity.X) > float.Epsilon)
                proj.velocity.X = -proj.oldVelocity.X;

            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(proj.velocity.Y - proj.oldVelocity.Y) > float.Epsilon)
                proj.velocity.Y = -proj.oldVelocity.Y;

            proj.velocity = proj.velocity.RotatedByRandom(MathHelper.ToRadians(radians));
            proj.netUpdate = true;
        }

        public static double CalculateDamagePlayersTake(this Player _, int damage, int defense)
        {
            double actualDamage = damage - defense * 0.5;
            if (Main.masterMode)
            {
                actualDamage = damage - defense;
            }
            else if (Main.expertMode)
            {
                actualDamage = damage - defense * 0.75;
            }
            if (actualDamage < 1.0)
            {
                actualDamage = 1.0;
            }
            return actualDamage;
        }

        public static double CalculateDamageNPCsTake(this NPC _, int damage, int defense)
        {
            double actualDamage = damage - defense * 0.5;
            if (actualDamage < 1.0)
            {
                actualDamage = 1.0;
            }
            return actualDamage;
        }

        ///<summary>
        ///Grabbed from <see href="https://github.com/Zeodexic/tsorcRevamp/blob/main/tsorcRevampUtils.cs#L774">here</see>.
        ///Accelerates an entity toward a target in a smooth way
        ///Returns a Vector2 with length 'acceleration' that points in the optimal direction to accelerate the NPC toward the target.
        ///If the target is moving, then it accounts for that.
        ///(No, unfortunately the optimal direction is not actually a straight line most of the time).
        ///Accelerates until the NPC is moving fast enough that the acceleration can *just* slow it down in time, then does so
        ///Do not ask me how long this took 💀
        ///</summary>
        ///<param name="actor">The entity moving</param>
        ///<param name="target">The target point it is aiming for</param>
        ///<param name="acceleration">The rate at which it can accelerate</param>
        ///<param name="topSpeed">The max speed of the entity</param>
        ///<param name="targetVelocity">The velocity of its target, defaults to 0</param>
        ///<param name="bufferZone">Should it smoothly slow down on approach?</param>
        public static void SmoothHoming(this Entity actor, Vector2 target, float acceleration, float topSpeed, Vector2? targetVelocity = null, bool bufferZone = true, float bufferStrength = 0.1f)
        {
            //If the target has a velocity then factor it in
            Vector2 velTarget = Vector2.Zero;
            if (targetVelocity != null)
            {
                velTarget = targetVelocity.Value;
            }

            //Get the difference between the center of both entities
            Vector2 posDifference = target - actor.Center;

            //Get the distance between them
            float distance = posDifference.Length();

            //Get the difference of velocities
            //This shifts the reference frame of the calculations, from here on out we are looking at the problem as if Entity 1 was still and Entity 2 had the velocity of both entities combined
            //The formulas below calculate where it will be in the future and then the entity is accelerated toward that point on an intercept trajectory
            Vector2 vTarget = velTarget - actor.velocity;

            //Normalize posDifference to get the direction of it, ignoring the length
            posDifference.Normalize();

            //Use a dot product to get the length of the velocity vector in the direction of the target.
            //This tells us how fast the actor is moving toward the target already
            float velocity = Vector2.Dot(-vTarget, posDifference);

            //Use the current velocity plus acceleration to calculate how long it will take to arrive using the formula for acceleration
            float eta = (-velocity / acceleration) + (float)Math.Sqrt((velocity * velocity / (acceleration * acceleration)) + 2 * distance / acceleration);

            //Use the velocity plus arrival time plus current target location to calculate the location the target will be at in the future
            Vector2 impactPos = target + vTarget * eta;

            //Generate a vector with length 'acceleration' pointing at that future location
            Vector2 fixedAcceleration = HelperStats.GenerateTargetingVector(actor.Center, impactPos, acceleration);

            //If distance or acceleration is 0 it will have nans, this deals with that
            if (fixedAcceleration.HasNaNs())
            {
                fixedAcceleration = Vector2.Zero;
            }

            //Update its acceleration
            actor.velocity += fixedAcceleration;

            //Slow it down to the speed limit if it is above it
            if (actor.velocity.Length() > topSpeed)
            {
                actor.velocity.Normalize();
                actor.velocity *= topSpeed;
            }

            //If it needs to slow down when arriving then do so
            //A distance of 300 and the formula here are super fudged. Could use improvement.
            if (bufferZone && distance < 300)
            {
                actor.velocity *= (float)Math.Pow(distance / 300, bufferStrength);
            }
        }

        /// <summary> Checks if the player that owns this projectile is alive, else kill this projectile. </summary>>

        public static void KeepAliveIfOwnerIsAlive(this Projectile proj, Player owner)
        {
            if (owner.dead || !owner.active)
                proj.Kill();
            if (!owner.dead || owner.active)
                proj.timeLeft = 2;
        }

        /// <summary>
        /// Makes this projectile rotate around something when given rotationAnchor, uses ai[1] slot of this projectile.
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="distance"></param>
        /// <param name="rotateAmount"></param>
        /// <param name="rotationAnchor"></param>
        /// <param name="reversed"></param>
        public static void RotateAroundSomething(this Projectile projectile, float distance, float rotateAmount, Vector2 rotationAnchor, bool reversed = false)
        {
            if (reversed)
                projectile.ai[1] -= rotateAmount;
            else
                projectile.ai[1] += rotateAmount;

            projectile.Center = rotationAnchor + new Vector2(distance, 0).RotatedBy(MathHelper.ToRadians(projectile.ai[1]));
            projectile.netUpdate = true;
        }

        /// <summary>
        /// Returns this player team color
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Color TeamColor(this Player player) => player.team switch
        {
            1 => new(.9f, .3f, .3f, .85f),// Red
            2 => new(.3f, .9f, .3f, .85f),// Green
            3 => new(.4f, .8f, .8f, .8f),// Blue
            4 => new(.8f, .8f, .25f, .8f),// Yellow
            5 => new(.8f, .25f, .75f, .8f),// Pink
            _ => new(.95f, .95f, .95f, .85f),// No team
        };

        /// <summary>
        /// Returns true if you are within range of DD2 NPCs
        /// </summary>
        /// <param name="player"></param>
        /// <param name="dist">The distance</param>
        /// <returns></returns>
        public static bool AmINearDD2NPCs(this Player player, float dist) => Main.npc.Take(Main.maxNPCs).Where(npc => npc.active && !npc.friendly).Any(npc => NPCID.Search.GetName(npc.type).Contains("DD2") && npc.WithinRange(player.Center, dist));

        /// <summary>
        /// Calculates the multiplier for the damage class of this player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="dmgClass"></param>
        /// <returns></returns>
        public static float GetPlayerDamageMultiplier(this Player player, DamageClass dmgClass) => player.GetTotalDamage(dmgClass).Additive;

        /// <summary>
        /// Adds this buff to every player of the same team, this player instance must be alive for this to work
        /// </summary>
        /// <param name="_"></param>
        /// <param name="type"></param>
        /// <param name="duration"></param>
        public static void AddABuffPlayerAndTeam(this Player player, int type, int duration)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player teamPlayer = Main.player[i];
                if (teamPlayer.whoAmI == player.whoAmI)
                    continue;

                if (teamPlayer.HasBuff(type))
                    break;
                else if (teamPlayer.team == player.team && !teamPlayer.dead && teamPlayer.active && player.active && !player.dead)
                {
                    player.AddBuff(type, duration);
                    teamPlayer.AddBuff(type, duration);
                }
            }
        }

        /// <summary>
        /// All players are dead
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        public static bool EveryOneIsDead(this Player _) => Main.player.Take(Main.maxPlayers).Where(x => x.active).All(x => x.dead || x.ghost);

        public static bool AreMobsInRange(this Player player, int range)
        {
            Vector2 position = player.Center;
            IEnumerable<NPC> mobs = Main.npc.Where(n => n.active && !n.friendly && n.Center.DistanceSQ(position) <= range * range);
            return mobs.Any();
        }

        /// <summary> Checks if the NPC is NOT active or is friendly. If true kill this projectile else set it's timeleft to 2. (Projectile is alive as long as NPC is alive). Good for chasing AI. </summary>>
        public static void CheckAliveNPCProj(this Projectile proj, NPC target)
        {
            if (target == null || !target.active || target.friendly)
                proj.Kill();
            else
                proj.timeLeft = 2;
        }

        /// <summary>
        /// Correctly angles a projectile sprite based on velocity if the sprite is facing up.
        /// <para>Rotating in the direction of travel is often used in projectiles like arrows.</para>
        /// </summary>
        /// <param name="proj"></param>
        public static void FaceForward(this Projectile proj, float pi = MathHelper.PiOver2) => proj.rotation = proj.velocity.ToRotation() + pi;

        /// <summary>
        /// Returns true if this NPC is airborn
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static bool IsAirborn(this NPC npc) => !Collision.SolidCollision(npc.BottomLeft, npc.width, 6);

        /// <summary>
        /// Returns true if this player is airborn
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsAirborn(this Player player) => !Collision.SolidCollision(player.BottomLeft, player.width, 6);

        /// <summary>
        /// Teleports this projectile to whatever idle position is.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="idlePosition"></param>
        public static void TeleportToOrigin(this Projectile proj, Player player, Vector2 idlePosition, int dustType)
        {
            proj.position = idlePosition;
            proj.velocity *= 0.1f;
            proj.netUpdate = true;
            SoundEngine.PlaySound(SoundID.Item8 with
            {
                Volume = 0.66f
            }, player.position);
            for (int i = 0; i < 50; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(player.Center, dustType, speed * 5, Scale: 1.5f);
                d.noGravity = true;
            }
        }
    }
}