using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PostLunarAcc.Items.Ingredients;
using PostLunarAcc.Rarity;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc.Items.Accessories
{
    internal class ManaCoreModPlayer : ModPlayer
    {
        public bool ManaCoreActive;

        public override void ResetEffects()
        {
            ManaCoreActive = false;
        }

        public override void PostUpdateMiscEffects()
        {
            if (ManaCoreActive)
            {
                int flatDamageInc = Player.statManaMax2;
                Player.GetDamage(DamageClass.Magic).Flat += flatDamageInc * 0.5f;
                Player.statDefense += (int)(flatDamageInc * 0.15);
                Player.statManaMax2 = 20;
                Player.statLifeMax2 += flatDamageInc;
                Player.lifeRegen += flatDamageInc / 12;
            }
        }
    }

    internal class ManaCore : ModItem
    {
        private float actualsize;

        private Geometry GeometryObject = new();

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.rare = ModContent.RarityType<MoonFragmentRarity>();
            Item.accessory = true;
            Item.hasVanityEffects = true;
            Item.value = Item.sellPrice(platinum: 2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.manaFlower = true;
            if (!hideVisual)
            {
                Vector2 pos = player.MountedCenter + Main.rand.NextVector2CircularEdge(48, 48);
                Lighting.AddLight(player.Center, Color.DarkBlue.ToVector3());
                Dust dusty = Dust.NewDustPerfect(pos, 134);
                dusty.velocity = pos.DirectionTo(player.Center) * 4f;
                dusty.shader = GameShaders.Armor.GetSecondaryShader(ContentSamples.ItemsByType[ItemID.PurpleDye].dye, Main.LocalPlayer);
                dusty.noGravity = true;
                dusty.scale = 0.8f;
            }
            base.UpdateAccessory(player, hideVisual);
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Player closestPlayer = FindNearestPlayer(Item.Center, 600);
            Lighting.AddLight(Item.Center, Color.DarkBlue.ToVector3());
            if (closestPlayer != null)
            {
                if (Main.rand.NextBool(4))
                {
                    Vector2 pos = Item.position + Item.Size * Main.rand.NextFloat(0.1f, 1f);
                    Dust dusty = Dust.NewDustPerfect(pos, 156);
                    dusty.velocity = pos.DirectionTo(closestPlayer.Center) * 50f * (closestPlayer.Distance(Item.Center) / 600f);
                    dusty.shader = GameShaders.Armor.GetSecondaryShader(ContentSamples.ItemsByType[ItemID.PurpleDye].dye, Main.LocalPlayer);
                    dusty.noGravity = true;
                    dusty.scale = 1f;
                    dusty.noLight = true;
                }
            }
            List<Dust> circle = GeometryObject.DrawDustCircle(Item.Center, 18, 48f, 134);
            foreach (Dust dusty in circle)
            {
                dusty.shader = GameShaders.Armor.GetSecondaryShader(ContentSamples.ItemsByType[ItemID.PurpleDye].dye, Main.LocalPlayer);
                dusty.noGravity = true;
            }
            Lighting.AddLight(Item.Center, Color.DarkBlue.ToVector3());
            base.Update(ref gravity, ref maxFallSpeed);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            actualsize = GeometryObject.IncreaseDecrease(0.005f);
            scale = Math.Abs(actualsize);
            return true;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<ManaCoreModPlayer>().ManaCoreActive = true;
            player.manaCost *= 0.00001f;
            base.UpdateEquip(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SuperManaPotion, 5)
                .AddIngredient(ItemID.ManaFlower)
                .AddIngredient(ModContent.ItemType<MoonFragment>(), 16)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}