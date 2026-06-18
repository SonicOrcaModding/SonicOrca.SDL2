using SonicOrca.Graphics;
using System;
using OpenTK.Graphics.OpenGL;

namespace SonicOrca.SDL2
{
    internal static class GlPrimitiveDraw
    {
        public static void DrawArrays(SonicOrca.Graphics.PrimitiveType engineType, int first, int count)
        {
            if (count <= 0)
                return;
#if __ANDROID__ || __IOS__
            if (engineType == SonicOrca.Graphics.PrimitiveType.Quads)
            {
                if (count % 4 != 0)
                    throw new ArgumentException("Quad vertex count must be a multiple of 4.");
                DrawQuadsAsTriangles(first, count);
                return;
            }
            if (engineType == SonicOrca.Graphics.PrimitiveType.QuadStrip || engineType == SonicOrca.Graphics.PrimitiveType.Polygon)
                throw new NotSupportedException("Primitive type " + engineType + " is not supported on OpenGL ES.");
#endif
            GL.DrawArrays(ToGlPrimitive(engineType), first, count);
        }

#if __ANDROID__ || __IOS__
        private const int MaxPooledQuads = 16384;
        private static readonly ushort[] _pooledIndices = new ushort[MaxPooledQuads * 6];
        private static int _staticEbo = -1;
        private static int _dynamicEbo = -1;
        private static ushort[] _scratch;

        static GlPrimitiveDraw()
        {
            for (int i = 0; i < MaxPooledQuads; i++)
            {
                int b = i * 4;
                int w = i * 6;
                _pooledIndices[w]     = (ushort)b;
                _pooledIndices[w + 1] = (ushort)(b + 1);
                _pooledIndices[w + 2] = (ushort)(b + 2);
                _pooledIndices[w + 3] = (ushort)b;
                _pooledIndices[w + 4] = (ushort)(b + 2);
                _pooledIndices[w + 5] = (ushort)(b + 3);
            }
        }

        private static void DrawQuadsAsTriangles(int first, int vertexCount)
        {
            int nq = vertexCount / 4;
            int indexCount = nq * 6;

            if (first == 0 && nq <= MaxPooledQuads)
            {
                if (_staticEbo == -1)
                {
                    _staticEbo = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _staticEbo);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(_pooledIndices.Length * sizeof(ushort)), _pooledIndices, BufferUsageHint.StaticDraw);
                }
                else
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _staticEbo);
                }
                GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedShort, IntPtr.Zero);
                return;
            }

            if (_scratch == null || _scratch.Length < indexCount)
                _scratch = new ushort[indexCount];
            int p = 0;
            for (int i = 0; i < nq; i++)
            {
                int b = first + i * 4;
                _scratch[p++] = (ushort)b;
                _scratch[p++] = (ushort)(b + 1);
                _scratch[p++] = (ushort)(b + 2);
                _scratch[p++] = (ushort)b;
                _scratch[p++] = (ushort)(b + 2);
                _scratch[p++] = (ushort)(b + 3);
            }
            if (_dynamicEbo == -1)
                _dynamicEbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _dynamicEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexCount * sizeof(ushort)), _scratch, BufferUsageHint.StreamDraw);
            GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }
#endif

        private static OpenTK.Graphics.OpenGL.PrimitiveType ToGlPrimitive(SonicOrca.Graphics.PrimitiveType engineType)
        {
            switch (engineType)
            {
                case SonicOrca.Graphics.PrimitiveType.Points:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.Points;
                case SonicOrca.Graphics.PrimitiveType.Lines:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.Lines;
                case SonicOrca.Graphics.PrimitiveType.LineStrip:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip;
                case SonicOrca.Graphics.PrimitiveType.LineLoop:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop;
                case SonicOrca.Graphics.PrimitiveType.Triangles:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
                case SonicOrca.Graphics.PrimitiveType.TriangleStrip:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.TriangleStrip;
                case SonicOrca.Graphics.PrimitiveType.TriangleFan:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.TriangleFan;
                case SonicOrca.Graphics.PrimitiveType.Quads:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.Quads;
                case SonicOrca.Graphics.PrimitiveType.QuadStrip:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.QuadStrip;
                case SonicOrca.Graphics.PrimitiveType.Polygon:
                    return OpenTK.Graphics.OpenGL.PrimitiveType.Polygon;
                default:
                    throw new ArgumentOutOfRangeException(nameof(engineType));
            }
        }
    }
}
