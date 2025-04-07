using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;

namespace Lab2
{
    class Game : GameWindow
    {
        private int VAO, VBO, EBO, textureID;
        private Shader shader;

        private float rotation;
        private Matrix4 view;
        private Matrix4 projection;

        private Vector3 cameraPosition = new Vector3(0, 0, 3);
        private Vector3 cameraFront = -Vector3.UnitZ;
        private Vector3 cameraUp = Vector3.UnitY;
        private float cameraSpeed = 1.5f;

        public Game(int width, int height)
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            CenterWindow(new Vector2i(width, height));
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            float[] vertices =
            {
                // Грань задняя
                -0.5f, -0.5f, -0.5f,  0f, 0f,
                 0.5f, -0.5f, -0.5f,  1f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 1f,
                -0.5f,  0.5f, -0.5f,  0f, 1f,

                // Грань передняя
                -0.5f, -0.5f,  0.5f,  0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 1f,

                // Левая
                -0.5f, -0.5f, -0.5f,  0f, 0f,
                -0.5f, -0.5f,  0.5f,  1f, 0f,
                -0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f,  0.5f, -0.5f,  0f, 1f,

                // Правая
                 0.5f, -0.5f, -0.5f,  0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                 0.5f,  0.5f, -0.5f,  0f, 1f,

                // Нижняя
                -0.5f, -0.5f, -0.5f,  0f, 0f,
                 0.5f, -0.5f, -0.5f,  1f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 1f,

                // Верхняя
                -0.5f,  0.5f, -0.5f,  0f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 1f
            };


            uint[] indices =
            {
                // Задняя
                0, 1, 2, 2, 3, 0,
                // Передняя
                4, 5, 6, 6, 7, 4,
                // Левая
                8, 9, 10, 10, 11, 8,
                // Правая
                12, 13, 14, 14, 15, 12,
                // Нижняя
                16, 17, 18, 18, 19, 16,
                // Верхняя
                20, 21, 22, 22, 23, 20
            };


            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);

            shader = new Shader("../../../../Shaders/shader.vert", "../../../../Shaders/shader.frag");
            shader.Use();
            shader.SetInt("texture0", 0);

            textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            StbImage.stbi_set_flip_vertically_on_load(1);
            using (var stream = File.OpenRead("../../../../Textures/goyda.jpg"))
            {
                var img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            float deltaTime = (float)args.Time;
            float velocity = cameraSpeed * deltaTime;

            if (KeyboardState.IsKeyDown(Keys.W))
                cameraPosition += cameraFront * velocity;
            if (KeyboardState.IsKeyDown(Keys.S))
                cameraPosition -= cameraFront * velocity;
            if (KeyboardState.IsKeyDown(Keys.A))
                cameraPosition -= Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * velocity;
            if (KeyboardState.IsKeyDown(Keys.D))
                cameraPosition += Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * velocity;

            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            rotation += 0.5f * (float)args.Time;

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 model = Matrix4.CreateRotationY(rotation) * Matrix4.CreateRotationX(rotation / 2);
            view = Matrix4.LookAt(cameraPosition, cameraPosition + cameraFront, cameraUp);

            shader.Use();
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.BindVertexArray(VAO);
            GL.Enable(EnableCap.DepthTest);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), e.Width / (float)e.Height, 0.1f, 100f);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(EBO);
            GL.DeleteTexture(textureID);
        }
    }
}
