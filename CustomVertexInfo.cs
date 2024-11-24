using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PostLunarAcc
{
    public struct CustomVertexInfo : IVertexType
    {
        public Vector3 Position; // Vertex position (X, Y, Z)

        public Color Color; // Vertex color

        public Vector2 TextureCoordinate; // UV texture coordinates

        // Define how this vertex is structured in memory
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public CustomVertexInfo(Vector2 position, Color color, Vector2 textureCoordinate)
        {
            Position = new Vector3(position, 0); // Z-coordinate is 0 for 2D
            Color = color;
            TextureCoordinate = textureCoordinate;
        }
    }
}