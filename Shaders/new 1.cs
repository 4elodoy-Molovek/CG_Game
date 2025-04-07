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
        private Camera camera;

        private float rotation;
        private Matrix4 projection;

        public Game(int width, int height)
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            CenterWindow(new Vector2i(width, height));
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            camera = new Camera(new Vector3(0, 0, 3), Vector3.UnitY, -90.0f, 0.0f);

            float[] vertices = { /* ... (твой массив вершин) ... */ };
            uint[] indices = { /* ... (твой массив индексов) ... */ };

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

            // Обработка клавиш для перемещения камеры
            camera.ProcessKeyboardInput(KeyboardState, deltaTime);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            // Обработка мыши для вращения камеры
            camera.UpdateMouseMovement(e.X, e.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            rotation += (float)args.Time;

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 model = Matrix4.CreateRotationY(rotation) * Matrix4.CreateRotationX(rotation / 2);
            Matrix4 view = camera.GetViewMatrix();

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
