using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System.Globalization;

namespace Lab2
{
    public class Model
    {
        public int VAO;
        public int VertexCount;
        public int TextureID;
    }

    public static class ModelLoader
    {
        public static Model Load(string objPath, string mtlPath, string textureDirectory)
        {
            List<float> vertices = new();
            List<float> texCoords = new();
            List<float> normals = new();
            List<float> finalData = new();

            string currentTexture = "";
            string[] objLines = File.ReadAllLines(objPath);
            string[] mtlLines = File.ReadAllLines(mtlPath);
            Dictionary<string, string> materialTextures = new();

            // Парсим .mtl
            string currentMat = "";
            foreach (var line in mtlLines)
            {
                if (line.StartsWith("newmtl "))
                {
                    currentMat = line.Split()[1];
                }
                else if (line.StartsWith("map_Kd ") && currentMat != "")
                {
                    materialTextures[currentMat] = Path.Combine(textureDirectory, line.Split()[1]);
                }
            }

            List<Vector3> vList = new();
            List<Vector2> vtList = new();
            List<Vector3> vnList = new();

            foreach (var line in objLines)
            {
                if (line.StartsWith("mtllib ") || line.StartsWith("#") || line == "") continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                switch (parts[0])
                {
                    case "v":
                        vList.Add(ParseVec3(parts));
                        break;
                    case "vt":
                        vtList.Add(ParseVec2(parts));
                        break;
                    case "vn":
                        vnList.Add(ParseVec3(parts));
                        break;
                    case "usemtl":
                        currentTexture = parts[1];
                        break;
                    case "f":
                        for (int i = 1; i < 4; i++)
                        {
                            var indices = parts[i].Split('/');
                            var v = vList[int.Parse(indices[0]) - 1];
                            var vt = vtList[int.Parse(indices[1]) - 1];
                            var vn = vnList[int.Parse(indices[2]) - 1];

                            finalData.AddRange(new[] { v.X, v.Y, v.Z });
                            finalData.AddRange(new[] { vt.X, vt.Y });
                            finalData.AddRange(new[] { vn.X, vn.Y, vn.Z });
                        }
                        break;
                }
            }

            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, finalData.Count * sizeof(float), finalData.ToArray(), BufferUsageHint.StaticDraw);

            int stride = (3 + 2 + 3) * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);

            int textureID = LoadTexture(materialTextures[currentTexture]);

            return new Model { VAO = vao, VertexCount = finalData.Count / 8, TextureID = textureID };
        }

        private static int LoadTexture(string path)
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            StbImage.stbi_set_flip_vertically_on_load(1);
            using var stream = File.OpenRead(path);
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                          PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return textureID;
        }

        private static Vector3 ParseVec3(string[] parts) =>
            new(float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture),
                float.Parse(parts[3], CultureInfo.InvariantCulture));

        private static Vector2 ParseVec2(string[] parts) =>
            new(float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture));
    }
}
