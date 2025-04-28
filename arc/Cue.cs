using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Lab2;

namespace Lab2
{
    public class Cue
    {
        private Vector3 position;
        private Vector3 direction;
        private float length;
        private Vector3 color;
        private int vao;

        public Cue(Vector3 position, Vector3 direction, float length, Vector3 color)
        {
            this.position = position;
            this.direction = direction;
            this.length = length;
            this.color = color;
            vao = GL.GenVertexArray();
        }

        public void Update(double deltaTime, Camera camera)
        {
            // Stub
        }

        public void Render(Shader shader)
        {
            shader.SetVector3("color", color);
            shader.SetMatrix4("model", Matrix4.CreateScale(0.05f, 0.05f, length) * Matrix4.CreateTranslation(position));
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(vao);
        }
    }
}