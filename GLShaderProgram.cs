// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.GLShaderProgram
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using OpenTK.Graphics.OpenGL;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using System;
using System.Collections.Generic;

namespace SonicOrca.SDL2
{

    internal class GLShaderProgram : IShaderProgram, IDisposable
    {
      private readonly GLGraphicsContext _context;
      private readonly int _glId;
      private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();

      public GLShaderProgram(GLGraphicsContext context, IEnumerable<GLShader> shaders)
      {
        this._context = context;
        this._glId = this.CreateShaderProgram(shaders);
      }

      public virtual void Dispose()
      {
        GL.DeleteProgram(this._glId);
        this._context.ShaderPrograms.Remove((IShaderProgram) this);
      }

      private int CreateShaderProgram(IEnumerable<GLShader> shaders)
      {
        int program = GL.CreateProgram();
        try
        {
          foreach (GLShader shader in shaders)
            GL.AttachShader(program, shader.Id);
          GL.LinkProgram(program);
          int @params;
          GL.GetProgram(program, ProgramParameter.LinkStatus, out @params);
          if (@params != 1)
            throw new Exception(GL.GetProgramInfoLog(program));
          this._context.ShaderPrograms.Add((IShaderProgram) this);
        }
        catch
        {
          GL.DeleteProgram(program);
          throw;
        }
        return program;
      }

      public void Activate() => GL.UseProgram(this._glId);

      public int GetAttributeLocation(string name) => GL.GetAttribLocation(this._glId, name);

      public int GetUniformLocation(string name)
      {
        int uniformLocation;
        if (!this._uniformLocations.TryGetValue(name, out uniformLocation))
          uniformLocation = this._uniformLocations[name] = GL.GetUniformLocation(this._glId, name);
        return uniformLocation;
      }

      public void SetUniform(string name, int value)
      {
        int uniformLocation = this.GetUniformLocation(name);
        if (uniformLocation < 0)
          return;
        GL.Uniform1(uniformLocation, value);
      }

      public void SetUniform(string name, float value)
      {
        int uniformLocation = this.GetUniformLocation(name);
        if (uniformLocation < 0)
          return;
        GL.Uniform1(uniformLocation, value);
      }

      public void SetUniform(string name, double value)
      {
        int uniformLocation = this.GetUniformLocation(name);
        if (uniformLocation < 0)
          return;
        GL.Uniform1(uniformLocation, (float) value);
      }

      public void SetUniform(string name, Vector2 value)
      {
        int uniformLocation = this.GetUniformLocation(name);
        if (uniformLocation < 0)
          return;
        GL.Uniform2(uniformLocation, (float) value.X, (float) value.Y);
      }

      public void SetUniform(string name, Vector3 value)
      {
        int uniformLocation = this.GetUniformLocation(name);
        if (uniformLocation < 0)
          return;
        GL.Uniform3(uniformLocation, (float) value.X, (float) value.Y, (float) value.Z);
      }

      public void SetUniform(string name, Vector4 value)
      {
        int uniformLocation = this.GetUniformLocation(name);
        if (uniformLocation < 0)
          return;
        GL.Uniform4(uniformLocation, (float) value.X, (float) value.Y, (float) value.Z, (float) value.W);
      }

      public void SetUniform(string name, Matrix4 value)
      {
        int uniformLocation = this.GetUniformLocation(name);
        if (uniformLocation < 0)
          return;
        GL.UniformMatrix4(uniformLocation, 1, false, new float[16 /*0x10*/]
        {
          (float) value.M11,
          (float) value.M21,
          (float) value.M31,
          (float) value.M41,
          (float) value.M12,
          (float) value.M22,
          (float) value.M32,
          (float) value.M42,
          (float) value.M13,
          (float) value.M23,
          (float) value.M33,
          (float) value.M43,
          (float) value.M14,
          (float) value.M24,
          (float) value.M34,
          (float) value.M44
        });
      }

      public void SetUniform(string name, Colour value)
      {
        int uniformLocation = this.GetUniformLocation(name);
        if (uniformLocation < 0)
          return;
        GL.Uniform4(uniformLocation, (float) value.Red / (float) byte.MaxValue, (float) value.Green / (float) byte.MaxValue, (float) value.Blue / (float) byte.MaxValue, (float) value.Alpha / (float) byte.MaxValue);
      }
    }
}
