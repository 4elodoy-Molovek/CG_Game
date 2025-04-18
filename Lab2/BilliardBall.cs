using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

public class Sphere
{
    public int VAO { get; private set; }
    public int VertexCount { get; private set; }

    public Sphere(float radius = 1.0f, int sectorCount = 36, int stackCount = 18)
    {
        List<float> vertices = new();
        List<float> normals = new();

        for (int i = 0; i <= stackCount; ++i)
        {
            float stackAngle = MathHelper.PiOver2 - i * MathHelper.Pi / stackCount;
            float xy = radius * MathF.Cos(stackAngle);
            float z = radius * MathF.Sin(stackAngle);

            for (int j = 0; j <= sectorCount; ++j)
            {
                float sectorAngle = j * 2 * MathHelper.Pi / sectorCount;
                float x = xy * MathF.Cos(sectorAngle);
                float y = xy * MathF.Sin(sectorAngle);

                vertices.AddRange(new float[] { x, y, z });
                normals.AddRange(new float[] { x / radius, y / radius, z / radius });
            }
        }

        List<uint> indices = new();
        for (int i = 0; i < stackCount; ++i)
        {
            int k1 = i * (sectorCount + 1);
            int k2 = k1 + sectorCount + 1;
            for (int j = 0; j < sectorCount; ++j, ++k1, ++k2)
            {
                if (i != 0)
                {
                    indices.AddRange(new uint[] { (uint)k1, (uint)k2, (uint)(k1 + 1) });
                }
                if (i != (stackCount - 1))
                {
                    indices.AddRange(new uint[] { (uint)(k1 + 1), (uint)k2, (uint)(k2 + 1) });
                }
            }
        }

        VertexCount = indices.Count;

        VAO = GL.GenVertexArray();
        int VBO = GL.GenBuffer();
        int NBO = GL.GenBuffer();
        int EBO = GL.GenBuffer();

        GL.BindVertexArray(VAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, NBO);
        GL.BufferData(BufferTarget.ArrayBuffer, normals.Count * sizeof(float), normals.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);
    }

    public void Draw()
    {
        GL.BindVertexArray(VAO);
        GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, VertexCount, DrawElementsType.UnsignedInt, 0);
    }
}
