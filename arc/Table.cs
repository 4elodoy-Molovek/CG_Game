using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;

public class Table
{
    private int vao;
    private int vbo;
    private int nbo;
    private int tbo;
    private int ebo;
    private int texture;
    private int indexCount;

    public Table(string objPath)
    {
        AssimpContext importer = new();
        var scene = importer.ImportFile(objPath, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.GenerateNormals);

        List<float> vertices = new();
        List<float> normals = new();
        List<float> texCoords = new();
        List<uint> indices = new();

        foreach (var mesh in scene.Meshes)
        {
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var v = mesh.Vertices[i];
                var n = mesh.Normals[i];
                vertices.AddRange(new float[] { v.X, v.Y, v.Z });
                normals.AddRange(new float[] { n.X, n.Y, n.Z });

                if (mesh.HasTextureCoords(0))
                {
                    var t = mesh.TextureCoordinateChannels[0][i];
                    texCoords.AddRange(new float[] { t.X, t.Y });
                }
                else
                {
                    texCoords.AddRange(new float[] { 0.0f, 0.0f });
                }
            }

            foreach (var face in mesh.Faces)
            {
                foreach (var idx in face.Indices)
                    indices.Add((uint)idx);
            }
        }

        indexCount = indices.Count;

        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();
        nbo = GL.GenBuffer();
        tbo = GL.GenBuffer();
        ebo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, nbo);
        GL.BufferData(BufferTarget.ArrayBuffer, normals.Count * sizeof(float), normals.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ArrayBuffer, tbo);
        GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Count * sizeof(float), texCoords.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(2);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);

        texture = LoadTextureFromMaterial(scene, objPath);
    }

    private int LoadTextureFromMaterial(Scene scene, string objPath)
    {
        var basePath = Path.GetDirectoryName(objPath);
        foreach (var material in scene.Materials)
        {
            if (material.HasTextureDiffuse)
            {
                var texPath = material.TextureDiffuse.FilePath.Replace('\\', '/');
                string fullTexPath = Path.Combine(basePath, Path.GetFileName(texPath));

                if (File.Exists(fullTexPath))
                    return LoadTexture(fullTexPath);
            }
        }

        return CreateDefaultTexture();
    }

    private int LoadTexture(string path)
    {
        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        int tex;
        GL.GenTextures(1, out tex);
        GL.BindTexture(TextureTarget.Texture2D, tex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height,
            0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        return tex;
    }

    private int CreateDefaultTexture()
    {
        byte[] defaultTex = new byte[] { 255, 0, 255, 255 };
        int tex;
        GL.GenTextures(1, out tex);
        GL.BindTexture(TextureTarget.Texture2D, tex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1,
            0, PixelFormat.Rgba, PixelType.UnsignedByte, defaultTex);
        return tex;
    }

    public void Render()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.BindVertexArray(vao);
        GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);
    }
}
