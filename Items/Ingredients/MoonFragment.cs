using PostLunarAcc.Rarity;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace PostLunarAcc.Items.Ingredients
{
    public class MoonFragment : ModItem
    {
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(10, 3));
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 14;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<MoonFragmentRarity>();
        }
    }
}