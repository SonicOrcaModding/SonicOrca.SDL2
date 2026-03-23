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
      private readonly GLGraphicsContext _context;
      private readonly int _id;

      public GLVertexArray(GLGraphicsContext context)
      {
        this._context = context;
        GL.GenVertexArrays(1, out this._id);
      }

      public void Dispose() => GL.DeleteVertexArrays(1, new int[1] { this._id });

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
        GlPrimitiveDraw.DrawArrays(type, index, count);
      }
    }
}
