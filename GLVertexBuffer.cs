// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.GLVertexBuffer
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using OpenTK.Graphics.OpenGL;
using SonicOrca.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonicOrca.SDL2
{

    internal class GLVertexBuffer : VertexBuffer
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
      private readonly int _numBuffers;
      private readonly int[] _vectorCounts;
      private readonly int _glId;
      private readonly int[] _glBufferIds;
      private readonly List<float>[] _data;
      private int _numPoints;
      private bool _noRender;

      public override IReadOnlyList<int> VectorCounts => (IReadOnlyList<int>) this._vectorCounts;

      public GLVertexBuffer(GLGraphicsContext context, IEnumerable<int> vectorCounts)
      {
        this._context = context;
        this._vectorCounts = vectorCounts.ToArray<int>();
        this._numBuffers = this._vectorCounts.Length;
        GL.GenVertexArrays(1, out this._glId);
        GL.BindVertexArray(this._glId);
        this._glBufferIds = new int[this._numBuffers];
        this._data = new List<float>[this._numBuffers];
        GL.GenBuffers(this._numBuffers, this._glBufferIds);
        for (int index = 0; index < this._numBuffers; ++index)
        {
          GL.EnableVertexAttribArray(index);
          GL.BindBuffer(BufferTarget.ArrayBuffer, this._glBufferIds[index]);
          GL.VertexAttribPointer(index, this._vectorCounts[index], VertexAttribPointerType.Float, false, 0, 0);
          this._data[index] = new List<float>();
        }
        this._context.VertexBuffers.Add((VertexBuffer) this);
      }

      public GLVertexBuffer(
        GLGraphicsContext context,
        IShaderProgram shaderProgram,
        IEnumerable<string> names,
        IEnumerable<int> vectorCounts)
      {
        this._context = context;
        this._vectorCounts = this.SetAttributeLocations(shaderProgram, names, vectorCounts).ToArray<int>();
        this._numBuffers = this._vectorCounts.Length;
        GL.GenVertexArrays(1, out this._glId);
        GL.BindVertexArray(this._glId);
        this._glBufferIds = new int[this._numBuffers];
        this._data = new List<float>[this._numBuffers];
        GL.GenBuffers(this._numBuffers, this._glBufferIds);
        for (int index = 0; index < this._numBuffers; ++index)
        {
          GL.EnableVertexAttribArray(index);
          GL.BindBuffer(BufferTarget.ArrayBuffer, this._glBufferIds[index]);
          GL.VertexAttribPointer(index, this._vectorCounts[index], VertexAttribPointerType.Float, false, 0, 0);
          this._data[index] = new List<float>();
        }
        this._context.VertexBuffers.Add((VertexBuffer) this);
      }

      public override void Dispose()
      {
        GL.DeleteVertexArrays(1, new int[1]{ this._glId });
        GL.DeleteBuffers(this._numBuffers, this._glBufferIds);
        this._context.VertexBuffers.Remove((VertexBuffer) this);
      }

      public override void SetBufferData(int index, IEnumerable<double> data)
      {
        this.SetBufferData(index, data.Select<double, float>((Func<double, float>) (x => (float) x)));
      }

      public void SetBufferData(int index, IEnumerable<float> data)
      {
        GL.BindBuffer(BufferTarget.ArrayBuffer, this._glBufferIds[index]);
        GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr) (data.Count<float>() * 4), data.Select<float, float>((Func<float, float>) (x => x)).ToArray<float>(), BufferUsageHint.StreamDraw);
      }

      public override void AddValue(int index, double value) => this._data[index].Add((float) value);

      public override void Begin()
      {
        for (int index = 0; index < this._numBuffers; ++index)
          this._data[index].Clear();
      }

      public override void End()
      {
        this._numPoints = 0;
        for (int index = 0; index < this._numBuffers; ++index)
        {
          int num = this._data[index].Count / this._vectorCounts[index];
          if (index == 0)
          {
            this._numPoints = num;
          }
          else
          {
            int numPoints = this._numPoints;
          }
        }
        if (this._numPoints == 0)
        {
          this._noRender = true;
        }
        else
        {
          for (int index = 0; index < this._numBuffers; ++index)
            this.SetBufferData(index, (IEnumerable<float>) this._data[index]);
          this._noRender = false;
        }
      }

      public override void Render(SonicOrca.Graphics.PrimitiveType type)
      {
        if (this._noRender)
          return;
        GL.BindVertexArray(this._glId);
        GL.DrawArrays(GLVertexBuffer.BeginModesForPrimitiveTypes[(int) type], 0, this._numPoints);
      }

      public override void Render(SonicOrca.Graphics.PrimitiveType type, int index, int count)
      {
        if (this._noRender)
          return;
        GL.BindVertexArray(this._glId);
        GL.DrawArrays(GLVertexBuffer.BeginModesForPrimitiveTypes[(int) type], index, count);
      }
    }
}
