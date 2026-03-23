using SonicOrca.Graphics;
using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace SonicOrca.SDL2
{
    internal static class GlPrimitiveDraw
    {
        public static void DrawArrays(SonicOrca.Graphics.PrimitiveType engineType, int first, int count)
        {
            if (count <= 0)
                return;
#if __ANDROID__
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

#if __ANDROID__
        private static void DrawQuadsAsTriangles(int first, int vertexCount)
        {
            int nq = vertexCount / 4;
            int indexCount = nq * 6;
            ushort[] indices = new ushort[indexCount];
            int w = 0;
            for (int i = 0; i < nq; i++)
            {
                int b = first + i * 4;
                indices[w++] = (ushort)b;
                indices[w++] = (ushort)(b + 1);
                indices[w++] = (ushort)(b + 2);
                indices[w++] = (ushort)b;
                indices[w++] = (ushort)(b + 2);
                indices[w++] = (ushort)(b + 3);
            }
            GCHandle handle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            try
            {
                GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedShort, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
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
