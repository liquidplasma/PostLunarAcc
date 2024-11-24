using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace PostLunarAcc
{
    internal class Geometry
    {
        private bool increase;

        private float size;

        private List<Dust> list = new();

        public float IncreaseDecrease(float amount, float max = 1.15f, float min = 0.5f)
        {
            if (increase)
            {
                if (size <= max)
                    size += amount;
                else
                    increase = false;
            }
            else
            {
                if (size >= min)
                    size -= amount;
                else
                    increase = true;
            }
            return size;
        }

        private void ClearDustList(int segments)
        {
            if (list.Count > segments)
                list.RemoveAt(0);
        }

        public List<Dust> DrawDustCircle(Vector2 entity, int segments, float radius, int dustID)
        {
            ClearDustList(segments);
            float deltaTheta = MathHelper.TwoPi / segments;
            float theta = 0f;
            for (int i = 0; i < segments; i++)
            {
                float x = entity.X + radius * MathF.Cos(theta);
                float y = entity.Y + radius * MathF.Sin(theta);
                theta += deltaTheta;
                list.Add(Dust.NewDustPerfect(new Vector2(x, y), dustID, Vector2.Zero, 0, default, 1f));
            }
            return list;
        }

        public List<Dust> DrawDustCircle(Entity entity, int segments, float radius, int dustID)
        {
            ClearDustList(segments);
            float deltaTheta = MathHelper.TwoPi / segments;
            float theta = 0f;
            for (int i = 0; i < segments; i++)
            {
                float x = entity.Center.X + radius * MathF.Cos(theta);
                float y = entity.Center.Y + radius * MathF.Sin(theta);
                theta += deltaTheta;
                list.Add(Dust.NewDustPerfect(new Vector2(x, y), dustID, Vector2.Zero, 0, default, 1f));
            }
            return list;
        }
    }
}