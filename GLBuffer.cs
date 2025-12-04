// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.GLBuffer
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using OpenTK.Graphics.OpenGL;
using SonicOrca.Graphics;
using System;
using System.Runtime.InteropServices;

namespace SonicOrca.SDL2
{

    internal class GLBuffer : IBuffer, IDisposable
    {
      private readonly GLGraphicsContext _context;
      private readonly int _id;

      public GLBuffer(GLGraphicsContext context)
      {
        this._context = context;
        this._id = GL.GenBuffer();
      }

      public void Dispose() => GL.DeleteBuffer(this._id);

      public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, this._id);

      public void SetData<T>(T[] data, int offset, int length)
      {
        IntPtr data1 = Marshal.UnsafeAddrOfPinnedArrayElement((Array) data, offset);
        IntPtr size = new IntPtr(Marshal.SizeOf(typeof (T)) * length);
        this.Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, size, data1, BufferUsageHint.StreamDraw);
      }
    }
}
