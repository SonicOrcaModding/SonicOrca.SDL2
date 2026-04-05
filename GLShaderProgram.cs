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
      private readonly float[] _matrixUniformScratch = new float[16];

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
        float[] m = this._matrixUniformScratch;
        m[0] = (float) value.M11;
        m[1] = (float) value.M21;
        m[2] = (float) value.M31;
        m[3] = (float) value.M41;
        m[4] = (float) value.M12;
        m[5] = (float) value.M22;
        m[6] = (float) value.M32;
        m[7] = (float) value.M42;
        m[8] = (float) value.M13;
        m[9] = (float) value.M23;
        m[10] = (float) value.M33;
        m[11] = (float) value.M43;
        m[12] = (float) value.M14;
        m[13] = (float) value.M24;
        m[14] = (float) value.M34;
        m[15] = (float) value.M44;
        GL.UniformMatrix4(uniformLocation, 1, false, m);
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
