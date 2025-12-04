// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.GLShader
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using OpenTK.Graphics.OpenGL;
using SonicOrca.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonicOrca.SDL2
{

    internal class GLShader : IShader, IDisposable
    {
      private static readonly IReadOnlyList<OpenTK.Graphics.OpenGL.ShaderType> ShaderTypeConversionTable = (IReadOnlyList<OpenTK.Graphics.OpenGL.ShaderType>) new OpenTK.Graphics.OpenGL.ShaderType[6]
      {
        OpenTK.Graphics.OpenGL.ShaderType.FragmentShader,
        OpenTK.Graphics.OpenGL.ShaderType.VertexShader,
        OpenTK.Graphics.OpenGL.ShaderType.GeometryShader,
        OpenTK.Graphics.OpenGL.ShaderType.GeometryShader,
        OpenTK.Graphics.OpenGL.ShaderType.TessEvaluationShader,
        OpenTK.Graphics.OpenGL.ShaderType.TessControlShader
      };
      private readonly OpenTK.Graphics.OpenGL.ShaderType _type;
      private readonly string _sourceCode;
      private int _glId;

      public int Id => this._glId;

      public GLShader(SonicOrca.Graphics.ShaderType type, string sourceCode)
      {
        this._type = GLShader.ShaderTypeConversionTable[(int) type];
        this._sourceCode = sourceCode;
        this._glId = GL.CreateShader(this._type);
        try
        {
          GL.ShaderSource(this._glId, this._sourceCode);
          GL.CompileShader(this._glId);
          int @params;
          GL.GetShader(this._glId, ShaderParameter.CompileStatus, out @params);
          if (@params != 1)
          {
            string shaderInfoLog = GL.GetShaderInfoLog(this._glId);
            throw new SDL2Exception($"Error compiling shader.\n\n{shaderInfoLog}", (Exception) new InvalidDataException(shaderInfoLog));
          }
        }
        catch
        {
          GL.DeleteShader(this._glId);
          throw;
        }
      }

      public void Dispose() => GL.DeleteShader(this._glId);
    }
}
