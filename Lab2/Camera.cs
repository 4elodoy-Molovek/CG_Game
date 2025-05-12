using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Lab2
{
    public class Camera
    {
        public Vector3 Position { get; private set; }
        public Vector3 Front { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 WorldUp { get; private set; }

        private float yaw;
        private float pitch;
        private float movementSpeed;
        private float mouseSensitivity;
        private float aspectRatio;

        private float lastX;
        private float lastY;
        private bool firstMouse;

        public Camera(Vector3 position, Vector3 up, float yaw, float pitch, float movementSpeed = 2.0f, float mouseSensitivity = 0.05f, float aspectRatio = 0)
        {
            Position = position;
            WorldUp = up;
            this.yaw = yaw;
            this.pitch = pitch;
            this.movementSpeed = movementSpeed;
            this.mouseSensitivity = mouseSensitivity;
            this.aspectRatio = aspectRatio;

            Front = Vector3.Normalize(new Vector3(0, -0.5f, -1));
            Right = Vector3.Cross(Front, WorldUp);
            Up = Vector3.Cross(Right, Front);

            lastX = 400;
            lastY = 300;
            firstMouse = true;
        }

        public void SetAspectRatio(float ratio)
        {
            aspectRatio = ratio;
        }

        public void UpdateMouseMovement(float xoffset, float yoffset)
        {
            xoffset *= mouseSensitivity;
            yoffset *= mouseSensitivity;

            yaw += xoffset;
            pitch -= yoffset;

            if (pitch > 89.0f) pitch = 89.0f;
            if (pitch < -89.0f) pitch = -89.0f;

            UpdateCameraVectors();
        }


        public void ProcessKeyboardInput(KeyboardState keyboardState, float deltaTime)
        {
            float velocity = movementSpeed * deltaTime;

            if (keyboardState.IsKeyDown(Keys.W))
                Position += Front * velocity;
            if (keyboardState.IsKeyDown(Keys.S))
                Position -= Front * velocity;
            if (keyboardState.IsKeyDown(Keys.A))
                Position -= Right * velocity;
            if (keyboardState.IsKeyDown(Keys.D))
                Position += Right * velocity;
        }

        private void UpdateCameraVectors()
        {
            Front = new Vector3
            (
                (float)(Math.Cos(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch))),
                (float)Math.Sin(MathHelper.DegreesToRadians(pitch)),
                (float)(Math.Sin(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch)))
            );
            Front = Vector3.Normalize(Front);

            Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), aspectRatio, 0.1f, 100f);
        }
    }
}