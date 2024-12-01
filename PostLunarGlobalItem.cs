using PostLunarAcc.Items.Ingredients;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace PostLunarAcc
{
    public class PostLunarGlobalItem : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (item.type == ItemID.MoonLordBossBag)
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<MoonFragment>(), minimumDropped: 36, maximumDropped: 54));
        }
    }
}