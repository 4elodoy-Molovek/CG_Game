using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using Lab2;

public class Game : GameWindow
{
    private Shader tableShader;
    private Shader ballShader;
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

        tableShader = new Shader("C:/Users/4elodoy Molovek/source/repos/Lab2/Shaders/shader.vert", "C:/Users/4elodoy Molovek/source/repos/Lab2/Shaders/shader.frag");
        ballShader = new Shader("C:/Users/4elodoy Molovek/source/repos/Lab2/Shaders/ball.vert", "C:/Users/4elodoy Molovek/source/repos/Lab2/Shaders/ball.frag");
        camera = new Camera(new Vector3(0, 6, 6), Vector3.UnitY, -135.0f, -30.0f, 2f, 0.05f, Size.X / (float)Size.Y);
        CursorState = CursorState.Grabbed;

        table = new Table("C:/Users/4elodoy Molovek/source/repos/Lab2/Models/fck/untitled.obj");
        ballMesh = new Sphere(0.05f);
        cueMesh = new Sphere(0.05f, 12, 6);

        float radius = 0.05f;
        float spacing = radius * 2 + 0.01f;
        float y = 0.9f;

        ballData = new List<(Vector3, Vector3)>();

        // Define ball colors
        Vector3[] colors =
        {
        new Vector3(1f, 1f, 0f), new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 0f),
        new Vector3(0.5f, 0f, 0.5f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f),
        new Vector3(1f, 0.5f, 0f), new Vector3(0.5f, 0.3f, 0f), new Vector3(0.5f, 0f, 0.3f),
        new Vector3(0f, 1f, 1f), new Vector3(1f, 0.2f, 0.8f), new Vector3(0.7f, 0.7f, 0.2f),
        new Vector3(0.9f, 0.4f, 0.1f), new Vector3(0.1f, 0.4f, 0.9f), new Vector3(0.6f, 0.2f, 0.6f)
    };

        // Add cue ball (white), positioned lower on table
        ballData.Add((new Vector3(0.0f, y, -0.8f), new Vector3(1f, 1f, 1f)));

        // Start of the rack (yellow ball tip towards cue ball)
        Vector3 rackStart = new Vector3(0.0f, y, 0.8f);

        int colorIndex = 0;

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col <= row; col++)
            {
                if (colorIndex >= colors.Length)
                    break;

                // Calculate position
                float z = rackStart.Z - (row * spacing * 0.866f); // Move back in Z (triangle depth)
                float x = rackStart.X + (col - row * 0.5f) * spacing; // Center each row in X

                x = rackStart.X - (x - rackStart.X);
                z = rackStart.Z - (z - rackStart.Z);

                ballData.Add((new Vector3(x, y, z), colors[colorIndex]));
                colorIndex++;
            }
        }
    }



    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        tableShader.Use();
        tableShader.SetMatrix4("view", camera.GetViewMatrix());
        tableShader.SetMatrix4("projection", camera.GetProjectionMatrix());
        tableShader.SetVector3("lightPos", new Vector3(5, 5, 5));
        tableShader.SetVector3("viewPos", camera.Position);

        tableShader.SetVector3("color", new Vector3(0.2f, 0.6f, 0.2f));
        tableShader.SetMatrix4("model", Matrix4.Identity);
        table.Render(tableShader);



        // Balls
        ballShader.Use();
        ballShader.SetMatrix4("view", camera.GetViewMatrix());
        ballShader.SetMatrix4("projection", camera.GetProjectionMatrix());
        ballShader.SetVector3("lightPos", new Vector3(5, 5, 5));
        ballShader.SetVector3("viewPos", camera.Position);


        foreach (var (position, color) in ballData)
        {
            var model = Matrix4.CreateTranslation(position);
            ballShader.SetMatrix4("model", model);
            ballShader.SetVector3("objectColor", color);
            ballMesh.Draw();
        }



        tableShader.Use();
        tableShader.SetVector3("color", new Vector3(0.6f, 0.3f, 0.1f));
        var cueModel = Matrix4.CreateScale(0.05f, 0.05f, 1.2f) * Matrix4.CreateTranslation(0, 0.2f, -1);
        tableShader.SetMatrix4("model", cueModel);
        cueMesh.Draw();

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        tableShader.Dispose();
        ballShader.Dispose();
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
