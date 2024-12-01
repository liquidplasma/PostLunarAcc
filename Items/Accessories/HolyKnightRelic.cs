using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PostLunarAcc.Items.Ingredients;
using PostLunarAcc.Rarity;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc.Items.Accessories
{
    public class HolyKnightBar : PlayerDrawLayer
    {
        private Texture2D Bar = ModContent.Request<Texture2D>("PostLunarAcc/Items/Accessories/HolyKnightBar", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        private Texture2D BarBorder = ModContent.Request<Texture2D>("PostLunarAcc/Items/Accessories/HolyKnightBarBorder", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return Main.myPlayer == drawInfo.drawPlayer.whoAmI && drawInfo.drawPlayer.GetModPlayer<HolyKnightRelicPlayer>().active;
        }

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.CaptureTheGem);

        public void DrawBar(PlayerDrawSet drawInfo, Vector2 position, float fillPercentage, Color barColor, Color backgroundColor)
        {
            fillPercentage = Math.Clamp(fillPercentage, 0f, 1f);

            drawInfo.DrawDataCache.Add(new DrawData(
                Bar, // The texture to render.
                position, // Position to render at.
                Bar.Bounds, // Source rectangle.
                barColor, // Color.
                0f, // Rotation.
                Bar.Size() * 0.5f, // Origin. Uses the texture's center.
                fillPercentage, // Scale.
                SpriteEffects.None, // SpriteEffects.
                0 // 'Layer'. This is always 0 in Terraria.
            ));

            drawInfo.DrawDataCache.Add(new DrawData(
                BarBorder, // The texture to render.
                position, // Position to render at.
                Bar.Bounds, // Source rectangle.
                backgroundColor, // Color.
                0f, // Rotation.
                Bar.Size() * 0.5f, // Origin. Uses the texture's center.
                1f, // Scale.
                SpriteEffects.None, // SpriteEffects.
                0 // 'Layer'. This is always 0 in Terraria.
            ));
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var position = drawInfo.Center + new Vector2(0, 32) - Main.screenPosition;
            position = new Vector2((int)position.X, (int)position.Y);
            float tankedDamage = drawInfo.drawPlayer.GetModPlayer<HolyKnightRelicPlayer>().tankedDamage;
            float maxTank = HolyKnightRelicPlayer.TANKMAX;
            Color barColor = Color.Lerp(Color.Red, Color.LimeGreen, tankedDamage / maxTank);
            DrawBar(drawInfo, position, tankedDamage / maxTank, barColor, Color.White);
        }
    }

    public class HolyKnightRelicPlayer : ModPlayer
    {
        public bool active;

        public int tankedDamage;

        private Projectile Counter;

        public const int TANKMAX = 300;

        public override void ResetEffects()
        {
            active = false;
        }

        public override void PostUpdateEquips()
        {
            base.PostUpdateEquips();
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            TankDamage(info);
            base.OnHurt(info);
        }

        private void TankDamage(Player.HurtInfo info)
        {
            if (active)
                tankedDamage += info.SourceDamage;
            if (active && tankedDamage > TANKMAX)
            {
                tankedDamage = TANKMAX;
                NPC target = FindNearestNPC(Player.Center, 16 * 64, false);
                if (target != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 position = Player.Center + new Vector2(0, -144) + Main.rand.NextVector2CircularEdge(128, 128);
                        int damage = (int)Player.GetTotalDamage(DamageClass.Melee).ApplyTo(info.SourceDamage * 15f);
                        if (Player.whoAmI == Main.myPlayer)
                        {
                            Counter = Projectile.NewProjectileDirect(Player.GetSource_FromThis("HolyKnightCounter"), position, position.DirectionTo(target.Center) * 16f, ProjectileID.Daybreak, damage, 16f, Player.whoAmI);
                            Counter.CritChance += 80;
                            for (int j = 1; j < 36; j++)
                            {
                                Dust dusty = Dust.NewDustDirect(Counter.Center, 0, 0, DustID.SolarFlare);
                                dusty.velocity = Utils.RandomVector2(Main.rand, -2f, 2f).RotatedByRandom(MathHelper.ToRadians(360)) * Main.rand.NextFloat(2f, 6f);
                                dusty.noGravity = true;
                                dusty.scale = 3f;
                                dusty.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.BrightGreenDye);
                            }
                        }
                    }
                    for (int i = 1; i < 36; i++)
                    {
                        Dust dusty = Dust.NewDustDirect(Player.Center, 0, 0, DustID.SolarFlare);
                        dusty.velocity = Player.Center.DirectionTo(target.Center).RotatedBy(i) * 6f;
                        dusty.noGravity = true;
                        dusty.scale = 3f;
                        dusty.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.BrightGreenDye);
                    }
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode);
                    tankedDamage = 0;
                }
            }
        }
    }

    internal class HolyKnightRelic : ModItem
    {
        private float rotationAdd = 0f;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.rare = ModContent.RarityType<MoonFragmentRarity>();
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            rotationAdd += 0.033f;
            rotation = rotationAdd;
            Texture2D texture = Item.MyTexture();
            Rectangle rect = texture.Bounds;
            scale = 0.5f;
            ExtensionMethods.BetterEntityDraw(texture, Item.Center, rect, lightColor, rotation, texture.Size() / 2, scale, SpriteEffects.None);
            return false;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            Lighting.AddLight(player.Center, Color.Gold.ToVector3() * 2f);
            player.GetModPlayer<HolyKnightRelicPlayer>().active = true;
            player.statDefense += 20;
            player.lifeRegen += 10;
            player.statLifeMax2 += 200;
            player.hasPaladinShield = true;
            player.noKnockback = true;
            foreach (Player ally in Main.ActivePlayers)
            {
                if (!ally.dead && ally.team == player.team)
                    player.statDefense += 25;
            }
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.Gold.ToVector3());
            base.Update(ref gravity, ref maxFallSpeed);
        }

        public override bool CanEquipAccessory(Player player, int slot, bool modded)
        {
            return base.CanEquipAccessory(player, slot, modded);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HeroShield)
                .AddIngredient(ModContent.ItemType<PaladinBar>(), 15)
                .AddIngredient(ModContent.ItemType<MoonFragment>(), 16)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}