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
using System.Text.RegularExpressions;

namespace SonicOrca.SDL2
{

    internal class GLShader : IShader, IDisposable
    {
#if __ANDROID__
      private static string AdaptSourceForOpenGlEs(string source)
      {
        if (string.IsNullOrEmpty(source))
          return source;
        const string versionEs = "#version 300 es";
        var firstVersion = new Regex(@"^\s*#version[^\r\n]*", RegexOptions.Multiline);
        var match = firstVersion.Match(source);
        if (match.Success && match.Value.IndexOf(" es", StringComparison.OrdinalIgnoreCase) >= 0)
          return ReplaceLegacyGlslTextureCalls(source);

        if (match.Success)
          source = firstVersion.Replace(source, versionEs, 1);
        else
          source = versionEs + "\n" + source;

        source = StripGlobalPrecisionDirectives(source);
        source = NormalizeUniformSamplerPrecisions(source);
        source = InsertHighpDefaultsAfterVersionLine(source);
        return ReplaceLegacyGlslTextureCalls(source);
      }

      private static string ReplaceLegacyGlslTextureCalls(string s)
      {
        if (string.IsNullOrEmpty(s))
          return s;
        s = Regex.Replace(s, @"\btexture2DLodEXT\s*\(", "textureLod(");
        s = Regex.Replace(s, @"\btexture2DLod\s*\(", "textureLod(");
        s = Regex.Replace(s, @"\btexture2DProjLodEXT\s*\(", "textureProjLod(");
        s = Regex.Replace(s, @"\btexture2DProj\s*\(", "textureProj(");
        s = Regex.Replace(s, @"\btexture2D\s*\(", "texture(");
        s = Regex.Replace(s, @"\btexture3D\s*\(", "texture(");
        s = Regex.Replace(s, @"\btextureCube\s*\(", "texture(");
        s = Regex.Replace(s, @"\btextureCubeLod\s*\(", "textureLod(");
        return s;
      }

      private static string StripGlobalPrecisionDirectives(string s)
      {
        s = Regex.Replace(s, @"^\s*precision\s+\w+\s+float\s*;\s*($|\r?\n)", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"^\s*precision\s+\w+\s+int\s*;\s*($|\r?\n)", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"^\s*precision\s+\w+\s+sampler2D\s*;\s*($|\r?\n)", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"^\s*precision\s+\w+\s+samplerCube\s*;\s*($|\r?\n)", "", RegexOptions.Multiline);
        return s;
      }

      private static string NormalizeUniformSamplerPrecisions(string s)
      {
        s = Regex.Replace(
          s,
          @"\buniform\s+(lowp|mediump|highp)\s+(sampler2D|samplerCube)\b",
          "uniform highp $2",
          RegexOptions.Multiline);
        return s;
      }

      private static string InsertHighpDefaultsAfterVersionLine(string s)
      {
        const string block =
          "precision highp float;\n" +
          "precision highp int;\n" +
          "precision highp sampler2D;\n" +
          "precision highp samplerCube;\n";
        var ver = Regex.Match(s, @"^\s*#version[^\r\n]*", RegexOptions.Multiline);
        if (!ver.Success)
          return block + s;
        int lineEnd = s.IndexOf('\n', ver.Index);
        if (lineEnd < 0)
          return s + "\n" + block;
        return s.Substring(0, lineEnd + 1) + block + s.Substring(lineEnd + 1);
      }
#endif

      private static readonly IReadOnlyList<OpenTK.Graphics.OpenGL.ShaderType> ShaderTypeConversionTable = new OpenTK.Graphics.OpenGL.ShaderType[6]
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
#if __ANDROID__
        if (type != SonicOrca.Graphics.ShaderType.Fragment && type != SonicOrca.Graphics.ShaderType.Vertex)
          throw new SDL2Exception("Only vertex and fragment shaders are supported on OpenGL ES.");
#endif
        this._type = GLShader.ShaderTypeConversionTable[(int) type];
#if __ANDROID__
        sourceCode = AdaptSourceForOpenGlEs(sourceCode);
#endif
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
