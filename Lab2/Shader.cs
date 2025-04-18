using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace Lab2
{
    public class Shader : IDisposable
    {
        private int handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, File.ReadAllText(vertexPath));
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText(fragmentPath));
            GL.CompileShader(fragmentShader);

            handle = GL.CreateProgram();
            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);
            GL.LinkProgram(handle);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        public void Use()
        {
            GL.UseProgram(handle);
        }

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(handle, name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void SetVector3(string name, Vector3 vector)
        {
            int location = GL.GetUniformLocation(handle, name);
            GL.Uniform3(location, vector);
        }

        public void Dispose()
        {
            GL.DeleteProgram(handle);
        }
    }
}