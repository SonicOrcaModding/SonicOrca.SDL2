// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.GLVertexArray
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using OpenTK.Graphics.OpenGL;
using SonicOrca.Graphics;
using System;
using System.Collections.Generic;

namespace SonicOrca.SDL2
{

    internal class GLVertexArray : IVertexArray, IDisposable
    {
      private static readonly IReadOnlyList<OpenTK.Graphics.OpenGL.PrimitiveType> BeginModesForPrimitiveTypes = (IReadOnlyList<OpenTK.Graphics.OpenGL.PrimitiveType>) new OpenTK.Graphics.OpenGL.PrimitiveType[10]
      {
        OpenTK.Graphics.OpenGL.PrimitiveType.Points,
        OpenTK.Graphics.OpenGL.PrimitiveType.Lines,
        OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip,
        OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop,
        OpenTK.Graphics.OpenGL.PrimitiveType.Triangles,
        OpenTK.Graphics.OpenGL.PrimitiveType.TriangleStrip,
        OpenTK.Graphics.OpenGL.PrimitiveType.TriangleFan,
        OpenTK.Graphics.OpenGL.PrimitiveType.Quads,
        OpenTK.Graphics.OpenGL.PrimitiveType.QuadStrip,
        OpenTK.Graphics.OpenGL.PrimitiveType.Polygon
      };
      private readonly GLGraphicsContext _context;
      private readonly int _id;

      public GLVertexArray(GLGraphicsContext context)
      {
        this._context = context;
        this._id = GL.GenVertexArray();
      }

      public void Dispose() => GL.DeleteVertexArray(this._id);

      public void Bind() => GL.BindVertexArray(this._id);

      public void DefineAttribute(
        int attributeLocation,
        VertexAttributePointerType type,
        int size,
        int stride,
        int offset)
      {
        GL.VertexAttribPointer(attributeLocation, size, (VertexAttribPointerType) type, false, stride, offset);
        GL.EnableVertexAttribArray(attributeLocation);
      }

      public void Render(SonicOrca.Graphics.PrimitiveType type, int index, int count)
      {
        this.Bind();
        GL.DrawArrays(GLVertexArray.BeginModesForPrimitiveTypes[(int) type], index, count);
      }
    }
}
