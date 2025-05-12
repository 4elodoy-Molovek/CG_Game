using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;
using Assimp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lab2
{
    public class Table
    {
        private struct Mesh
        {
            public int vao;
            public int vbo;
            public int ebo;
            public int texture;
            public int indexCount;
        }

        private List<Mesh> meshes = new();

        private List<Vector3> modelVertices = new();

        public Table(string path)
        {
            var context = new AssimpContext();
            var scene = context.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

            string directory = Path.GetDirectoryName(path);

            foreach (var mesh in scene.Meshes)
            {
                foreach (var vertex in mesh.Vertices)
                {
                    modelVertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));
                }

                var vertices = new List<float>();
                var indices = mesh.GetIndices();

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var pos = mesh.Vertices[i];
                    var normal = mesh.Normals[i];
                    var tex = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][i] : new Vector3D(0, 0, 0);

                    // vertex format: position (3), normal (3), texcoords (2)
                    vertices.AddRange(new float[] { pos.X, pos.Y, pos.Z, normal.X, normal.Y, normal.Z, tex.X, tex.Y });
                }

                int vao = GL.GenVertexArray();
                int vbo = GL.GenBuffer();
                int ebo = GL.GenBuffer();

                GL.BindVertexArray(vao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

                int stride = 8 * sizeof(float);

                GL.EnableVertexAttribArray(0); // position
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

                GL.EnableVertexAttribArray(1); // normal
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

                GL.EnableVertexAttribArray(2); // texcoord
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

                GL.BindVertexArray(0);

                int texture = 0;
                if (scene.Materials[mesh.MaterialIndex].HasTextureDiffuse)
                {
                    string texPath = Path.Combine(directory, scene.Materials[mesh.MaterialIndex].TextureDiffuse.FilePath);
                    texture = LoadTexture(texPath);
                }

                meshes.Add(new Mesh
                {
                    vao = vao,
                    vbo = vbo,
                    ebo = ebo,
                    texture = texture,
                    indexCount = indices.Length
                });
            }
        }

        public void Render(Shader shader)
        {
            foreach (var mesh in meshes)
            {
                if (mesh.texture != 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, mesh.texture);
                    shader.SetInt("texture_diffuse", 0);
                }

                GL.BindVertexArray(mesh.vao);
                GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, mesh.indexCount, DrawElementsType.UnsignedInt, 0);
            }

            GL.BindVertexArray(0);
        }

        private int LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Texture file not found: {path}");
                return 0;
            }

            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            using var stream = File.OpenRead(path);
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return texture;
        }

        public List<Vector3> GetMeshVertices() => new(modelVertices);

        public (float width, float height) GetTableSize()
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            foreach (var vertex in modelVertices)
            {
                if (vertex.X < minX) minX = vertex.X;
                if (vertex.X > maxX) maxX = vertex.X;

                if (vertex.Z < minZ) minZ = vertex.Z;
                if (vertex.Z > maxZ) maxZ = vertex.Z;
            }

            float width = (maxX - minX) / 2.0f;
            float height = (maxZ - minZ) / 2.0f;

            return (width, height);
        }

        public List<Vector3> GetPocketPositions()
        {
            var (width, height) = GetTableSize();

            return new List<Vector3>
            {
                new Vector3(-width, 0, -height),
                new Vector3(0, 0, -height),
                new Vector3(width, 0, -height),
                new Vector3(-width, 0, height),
                new Vector3(0, 0, height),
                new Vector3(width, 0, height)
            };
        }

        public float GetPocketRadiusEstimate()
        {
            var pockets = GetPocketPositions();

            // Берём первый карман
            var firstPocket = pockets[0];

            // Найдём минимальное расстояние до ближайшей вершины модели
            float minDistance = float.MaxValue;

            foreach (var v in modelVertices)
            {
                float distance = (v - firstPocket).Length;
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance * 0.7f; // множитель 0.7f чтобы мяч проваливался чуть-чуть увереннее
        }

    }

}

