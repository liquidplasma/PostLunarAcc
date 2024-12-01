using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace PostLunarAcc.Rarity
{
    internal class MoonFragmentRarity : ModRarity
    {
        public static readonly Color[] RarityColorLerp = new Color[]
        {
            Color.Cyan,
            Color.Blue,
            Color.Lime,
            Color.Purple,
            Color.Red,
        };

        private int numColors = RarityColorLerp.Length;

        private int nextIndex => (index + 1) % numColors;
        private int index => (int)(Main.GameUpdateCount / 60 % numColors);

        public override Color RarityColor => Color.Lerp(RarityColorLerp[index], RarityColorLerp[nextIndex], Main.GameUpdateCount % 60 / 60f);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }
}