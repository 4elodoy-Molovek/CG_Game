﻿using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace Lab2
{
    public class Shader : IDisposable
    {
        private readonly int handle;
        public int Handle => handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            Console.WriteLine("Fragment Shader Source:\n" + File.ReadAllText(fragmentPath));
            GL.ShaderSource(vertexShader, File.ReadAllText(vertexPath));
            GL.CompileShader(vertexShader);
            CheckCompileErrors(vertexShader, "VERTEX");

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText(fragmentPath));
            GL.CompileShader(fragmentShader);
            CheckCompileErrors(fragmentShader, "FRAGMENT");

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

        public void SetInt(string name, int value)
        {
            int location = GL.GetUniformLocation(handle, name);
            GL.Uniform1(location, value);
        }

        private void CheckCompileErrors(int shader, string type)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{infoLog}");
            }
        }

        public void Dispose()
        {
            GL.DeleteProgram(handle);
        }
    }
}
