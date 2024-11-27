using Terraria;
using Terraria.ModLoader;

namespace PostLunarAcc.Debuffs
{
    public class EasyModBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_204";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            base.SetStaticDefaults();
        }
    }

    public class Stacking : EasyModBuff
    {
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<PostLunarGlobalNPC>().toExplode = true;
            base.Update(npc, ref buffIndex);
        }
    }

    public class Soulbound : EasyModBuff
    {
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<PostLunarGlobalNPC>().soulboundActive = true;
            base.Update(npc, ref buffIndex);
        }
    }
}