using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using System.Drawing.Imaging;

public class Table
{
    private int vao;
    private int vbo;
    private int nbo;
    private int tbo;
    private int vertexCount;
    private int textureID;

    public Table(string fbxPath)
    {
        AssimpContext importer = new();
        var scene = importer.ImportFile(fbxPath,
            PostProcessSteps.Triangulate |
            PostProcessSteps.GenerateNormals |
            PostProcessSteps.FlipUVs |
            PostProcessSteps.JoinIdenticalVertices
        );

        List<float> vertices = new();
        List<float> normals = new();
        List<float> texCoords = new();

        foreach (var mesh in scene.Meshes)
        {
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var v = mesh.Vertices[i];
                vertices.AddRange(new float[] { v.X, v.Y, v.Z });

                var n = mesh.Normals[i];
                normals.AddRange(new float[] { n.X, n.Y, n.Z });

                if (mesh.HasTextureCoords(0))
                {
                    var uv = mesh.TextureCoordinateChannels[0][i];
                    texCoords.AddRange(new float[] { uv.X, uv.Y });
                }
                else
                {
                    texCoords.AddRange(new float[] { 0f, 0f });
                }
            }

            vertexCount += mesh.Vertices.Count;
        }

        // Загружаем текстуру (берём первую из материалов)
        var material = scene.Materials[scene.Meshes[0].MaterialIndex];
        if (material.HasTextureDiffuse)
        {
            string texturePath = material.TextureDiffuse.FilePath;
            if (File.Exists(texturePath))
            {
                textureID = LoadTexture(texturePath);
            }
            else
            {
                Console.WriteLine($"[Table] Текстура не найдена: {texturePath}");
            }
        }

        // OpenGL
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();
        nbo = GL.GenBuffer();
        tbo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        // Позиции
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(0);

        // Нормали
        GL.BindBuffer(BufferTarget.ArrayBuffer, nbo);
        GL.BufferData(BufferTarget.ArrayBuffer, normals.Count * sizeof(float), normals.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        // UV
        GL.BindBuffer(BufferTarget.ArrayBuffer, tbo);
        GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Count * sizeof(float), texCoords.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(2);
    }

    public void Render()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureID);
        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
    }

    private int LoadTexture(string path)
    {
        int id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, id);

        using var image = new Bitmap(path);
        var data = image.LockBits(
            new Rectangle(0, 0, image.Width, image.Height),
            ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb
        );

        GL.TexImage2D(TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            data.Width,
            data.Height,
            0,
            OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
            PixelType.UnsignedByte,
            data.Scan0
        );

        image.UnlockBits(data);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        return id;
    }
}
