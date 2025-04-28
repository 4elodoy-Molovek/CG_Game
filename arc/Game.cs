using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using Lab2;

public class Game : GameWindow
{
    private Shader shader;
    private Camera camera;
    private Table table;
    private List<(Vector3 position, Vector3 color)> ballData;
    private Sphere ballMesh;
    private Sphere cueMesh;
    private Vector2 lastMousePos;
    private bool firstMove = true;
    private float cameraSpeed = 3.0f;
    private float mouseSensitivity = 0.2f;

    public Game(GameWindowSettings gws, NativeWindowSettings nws)
        : base(gws, nws)
    {
        CursorState CursorGrabbed = CursorState.Grabbed;
        CursorState CursorVisible = CursorState.Hidden;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);
        GL.Enable(EnableCap.DepthTest);

        shader = new Shader("C:/Users/4elodoy Molovek/source/repos/Lab2/Shaders/shader.vert", "C:/Users/4elodoy Molovek/source/repos/Lab2/Shaders/shader.frag");
        camera = new Camera(new Vector3(0, 6, 6), Vector3.UnitY, -135.0f, -30.0f, 2f, 0.05f, Size.X / (float)Size.Y);
        CursorState = CursorState.Grabbed;

        table = new Table("C:/Users/4elodoy Molovek/source/repos/Lab2/Models/fck/untitled.obj");
        ballMesh = new Sphere(0.2f);
        cueMesh = new Sphere(0.05f, 12, 6);

        ballData = new List<(Vector3, Vector3)>
        {
            (new Vector3(0, 0.2f, 0), new Vector3(1, 1, 1)), // Белый
            (new Vector3(1.5f, 0.2f, 0), new Vector3(1, 1, 0)), // Жёлтый
            (new Vector3(1.7f, 0.2f, 0.2f), new Vector3(0, 0, 1)), // Синий
            (new Vector3(1.9f, 0.2f, -0.2f), new Vector3(1, 0, 0)), // Красный
            (new Vector3(2.1f, 0.2f, 0.4f), new Vector3(0.5f, 0, 0.5f)), // Фиолетовый
            (new Vector3(2.3f, 0.2f, -0.4f), new Vector3(1, 0.5f, 0)), // Оранжевый
            (new Vector3(2.5f, 0.2f, 0.6f), new Vector3(0, 0.5f, 0)), // Зелёный
            (new Vector3(2.7f, 0.2f, -0.6f), new Vector3(0.6f, 0.3f, 0.2f)), // Коричневый
        };
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.Use();
        shader.SetMatrix4("view", camera.GetViewMatrix());
        shader.SetMatrix4("projection", camera.GetProjectionMatrix());
        shader.SetVector3("lightPos", new Vector3(5, 5, 5));
        shader.SetVector3("viewPos", camera.Position);

        shader.SetVector3("color", new Vector3(0.2f, 0.6f, 0.2f));
        shader.SetMatrix4("model", Matrix4.Identity);
        table.Render();

        foreach (var (position, color) in ballData)
        {
            shader.SetVector3("color", color);
            var model = Matrix4.CreateTranslation(position);
            shader.SetMatrix4("model", model);
            ballMesh.Draw();
        }

        shader.SetVector3("color", new Vector3(0.6f, 0.3f, 0.1f));
        var cueModel = Matrix4.CreateScale(0.05f, 0.05f, 1.2f) * Matrix4.CreateTranslation(0, 0.2f, -1);
        shader.SetMatrix4("model", cueModel);
        cueMesh.Draw();

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        shader.Dispose();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        float deltaTime = (float)args.Time;

        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        if (KeyboardState.IsKeyDown(Keys.Backspace))
            CursorState = CursorState.Normal;

        camera.ProcessKeyboardInput(KeyboardState, deltaTime);

        Console.WriteLine("Camera Position: " + camera.Position);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);
        camera.UpdateMouseMovement(e.X, e.Y);
    }
}
