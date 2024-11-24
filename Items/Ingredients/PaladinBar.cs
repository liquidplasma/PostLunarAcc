using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc.Items.Ingredients
{
    public class PaladinBar : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 14;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = 10000;
            Item.rare = ItemRarityID.Cyan;
        }
    }
}