// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.GLFramebuffer
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

    internal class GLFramebuffer : IFramebuffer, IDisposable
    {
      private readonly GLGraphicsContext _context;
      private readonly int _width;
      private readonly int _height;
      private readonly uint _name;
      private readonly GLTexture[] _textures;
      private readonly int _depthBufferName;

      public int Width => this._width;

      public int Height => this._height;

      public IReadOnlyList<ITexture> Textures => (IReadOnlyList<ITexture>) this._textures;

      public static GLFramebuffer FromBackBuffer(GLGraphicsContext context, int width, int height)
      {
        return new GLFramebuffer(context, width, height, 0U);
      }

      private GLFramebuffer(GLGraphicsContext context, int width, int height, uint name)
      {
        this._context = context;
        this._width = width;
        this._height = height;
        this._name = name;
      }

      public GLFramebuffer(GLGraphicsContext context, int width, int height, int numTextures = 1)
      {
        this._context = context;
        this._width = width;
        this._height = height;
        GL.GenFramebuffers(1, out this._name);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, this._name);
        this._textures = new GLTexture[numTextures];
        for (int index = 0; index < numTextures; ++index)
        {
          GLTexture glTexture = this._textures[index] = (GLTexture) context.CreateTexture(width, height);
          GL.FramebufferTexture(FramebufferTarget.Framebuffer, (FramebufferAttachment) (36064 + index), glTexture.Id, 0);
        }
        GL.DrawBuffers(numTextures, Enumerable.Range(0, numTextures).Select<int, DrawBuffersEnum>((Func<int, DrawBuffersEnum>) (i => (DrawBuffersEnum) (36064 + i))).ToArray<DrawBuffersEnum>());
        this._depthBufferName = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, this._depthBufferName);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, this._depthBufferName);
        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
          throw new SDL2Exception("Unable to create framebuffer.");
        this._context.Framebuffers.Add((IFramebuffer) this);
      }

      public void Dispose()
      {
        foreach (GLTexture texture in this._textures)
          texture.Dispose();
        GL.DeleteFramebuffer(this._name);
        if (this._context == null)
          return;
        this._context.Framebuffers.Remove((IFramebuffer) this);
      }

      public void Activate()
      {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, this._name);
        GL.Viewport(0, 0, this._width, this._height);
        this._context.CurrentFramebuffer = (IFramebuffer) this;
      }
    }
}
