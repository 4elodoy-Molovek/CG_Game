//// ModelLoader.cs
//using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;
//using StbImageSharp;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;

//public class Model
//{
//    public int VAO;
//    public int VertexCount;
//    public int TextureID;
//}

//public static class ModelLoader
//{
//    public static Model Load(string objPath, string mtlPath, string textureDirectory)
//    {
//        List<Vector3> vList = new();
//        List<Vector2> vtList = new();
//        List<Vector3> vnList = new();
//        List<float> finalData = new();

//        string currentMaterial = "";
//        Dictionary<string, string> materialTextures = new();

//        if (File.Exists(mtlPath))
//        {
//            string[] mtlLines = File.ReadAllLines(mtlPath);
//            string matName = "";
//            foreach (var line in mtlLines)
//            {
//                if (line.StartsWith("newmtl "))
//                    matName = line.Split()[1];
//                else if (line.StartsWith("map_Kd ") && matName != "")
//                {
//                    string texFile = line.Substring(7).Trim().Replace('\', ' / ');
//                    string fullTexPath = Path.Combine(textureDirectory, Path.GetFileName(texFile));
//                    materialTextures[matName] = fullTexPath;
//                }
//            }
//        }

//        string[] objLines = File.ReadAllLines(objPath);
//        foreach (var line in objLines)
//        {
//            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

//            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
//            switch (parts[0])
//            {
//                case "v":
//                    vList.Add(ParseVec3(parts));
//                    break;
//                case "vt":
//                    vtList.Add(ParseVec2(parts));
//                    break;
//                case "vn":
//                    vnList.Add(ParseVec3(parts));
//                    break;
//                case "usemtl":
//                    currentMaterial = parts[1];
//                    break;
//                case "f":
//                    for (int i = 2; i < parts.Length - 1; i++)
//                    {
//                        AddVertex(parts[1], vList, vtList, vnList, finalData);
//                        AddVertex(parts[i], vList, vtList, vnList, finalData);
//                        AddVertex(parts[i + 1], vList, vtList, vnList, finalData);
//                    }
//                    break;
//            }
//        }

//        int vao = GL.GenVertexArray();
//        int vbo = GL.GenBuffer();
//        GL.BindVertexArray(vao);
//        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
//        GL.BufferData(BufferTarget.ArrayBuffer, finalData.Count * sizeof(float), finalData.ToArray(), BufferUsageHint.StaticDraw);

//        int stride = 8 * sizeof(float);
//        GL.EnableVertexAttribArray(0);
//        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
//        GL.EnableVertexAttribArray(1);
//        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
//        GL.EnableVertexAttribArray(2);
//        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));

//        int texture = 0;
//        if (materialTextures.TryGetValue(currentMaterial, out string texPath) && File.Exists(texPath))
//        {
//            texture = LoadTexture(texPath);
//        }

//        return new Model
//        {
//            VAO = vao,
//            VertexCount = finalData.Count / 8,
//            TextureID = texture
//        };
//    }

//    private static void AddVertex(string faceData, List<Vector3> vList, List<Vector2> vtList, List<Vector3> vnList, List<float> output)
//    {
//        var idx = faceData.Split('/');
//        int vi = int.Parse(idx[0]) - 1;
//        int ti = (idx.Length > 1 && idx[1] != "") ? int.Parse(idx[1]) - 1 : -1;
//        int ni = (idx.Length > 2 && idx[2] != "") ? int.Parse(idx[2]) - 1 : -1;

//        Vector3 pos = vList[vi];
//        Vector2 uv = (ti >= 0 && ti < vtList.Count) ? vtList[ti] : Vector2.Zero;
//        Vector3 norm = (ni >= 0 && ni < vnList.Count) ? vnList[ni] : Vector3.UnitY;

//        output.AddRange(new float[]
//        {
//            pos.X, pos.Y, pos.Z,
//            uv.X, uv.Y,
//            norm.X, norm.Y, norm.Z
//        });
//    }

//    private static Vector3 ParseVec3(string[] p) =>
//        new Vector3(Parse(p[1]), Parse(p[2]), Parse(p[3]));

//    private static Vector2 ParseVec2(string[] p) =>
//        new Vector2(Parse(p[1]), Parse(p[2]));

//    private static float Parse(string s) =>
//        float.Parse(s, CultureInfo.InvariantCulture);

//    private static int LoadTexture(string path)
//    {
//        using var stream = File.OpenRead(path);
//        var img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

//        int tex = GL.GenTexture();
//        GL.BindTexture(TextureTarget.Texture2D, tex);
//        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
//            img.Width, img.Height, 0,
//            PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);

//        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
//        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
//        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
//        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
//        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

//        return tex;
//    }
//}
    