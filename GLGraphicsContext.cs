// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.GLGraphicsContext
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using OpenTK.Graphics.OpenGL;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SonicOrca.SDL2
{

    internal class GLGraphicsContext : IGraphicsContext
    {
      private readonly SDL2WindowContext _videoAdapter;
      private BlendMode _blendMode;
      private readonly List<ITexture> _textures = new List<ITexture>();
      private readonly List<IShaderProgram> _shaderPrograms = new List<IShaderProgram>();
      private readonly List<VertexBuffer> _vertexBuffers = new List<VertexBuffer>();
      private readonly List<IFramebuffer> _renderTargets = new List<IFramebuffer>();

      internal ICollection<ITexture> Textures => (ICollection<ITexture>) this._textures;

      internal ICollection<IShaderProgram> ShaderPrograms
      {
        get => (ICollection<IShaderProgram>) this._shaderPrograms;
      }

      internal ICollection<VertexBuffer> VertexBuffers
      {
        get => (ICollection<VertexBuffer>) this._vertexBuffers;
      }

      internal ICollection<IFramebuffer> Framebuffers
      {
        get => (ICollection<IFramebuffer>) this._renderTargets;
      }

      IReadOnlyCollection<ITexture> IGraphicsContext.Textures
      {
        get => (IReadOnlyCollection<ITexture>) this._textures;
      }

      IReadOnlyCollection<IShaderProgram> IGraphicsContext.ShaderPrograms
      {
        get => (IReadOnlyCollection<IShaderProgram>) this._shaderPrograms;
      }

      IReadOnlyCollection<VertexBuffer> IGraphicsContext.VertexBuffers
      {
        get => (IReadOnlyCollection<VertexBuffer>) this._vertexBuffers;
      }

      IReadOnlyCollection<IFramebuffer> IGraphicsContext.RenderTargets
      {
        get => (IReadOnlyCollection<IFramebuffer>) this._renderTargets;
      }

      public bool DepthTesting
      {
        get => GL.IsEnabled(EnableCap.DepthTest);
        set
        {
          if (value)
            GL.Enable(EnableCap.DepthTest);
          else
            GL.Disable(EnableCap.DepthTest);
        }
      }

      public BlendMode BlendMode
      {
        get => this._blendMode;
        set
        {
          if (this._blendMode == value)
            return;
          this._blendMode = value;
          switch (value)
          {
            case BlendMode.Alpha:
              GL.Enable(EnableCap.Blend);
              GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
              break;
            case BlendMode.Additive:
              GL.Enable(EnableCap.Blend);
              GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
              break;
            default:
              GL.Disable(EnableCap.Blend);
              break;
          }
        }
      }

      public SonicOrca.Graphics.PolygonMode PolygonMode
      {
        get
        {
          int data;
          GL.GetInteger(GetPName.PolygonMode, out data);
          return (SonicOrca.Graphics.PolygonMode) (data - 6912);
        }
        set => GL.PolygonMode(MaterialFace.FrontAndBack, (OpenTK.Graphics.OpenGL.PolygonMode) (6912 + value));
      }

      public IFramebuffer CurrentFramebuffer { get; internal set; }

      public GLGraphicsContext(SDL2WindowContext videoAdapter) => this._videoAdapter = videoAdapter;

      public IShader CreateShader(SonicOrca.Graphics.ShaderType type, string sourceCode)
      {
        return (IShader) new GLShader(type, sourceCode);
      }

      public IShaderProgram CreateShaderProgram(params IShader[] shaders)
      {
        return this.CreateShaderProgram((IEnumerable<IShader>) shaders);
      }

      public IShaderProgram CreateShaderProgram(IEnumerable<IShader> shaders)
      {
        return (IShaderProgram) new GLShaderProgram(this, shaders.Select<IShader, GLShader>((Func<IShader, GLShader>) (x => (GLShader) x)));
      }

      public VertexBuffer CreateVertexBuffer(params int[] vectorCounts)
      {
        return this.CreateVertexBuffer((IEnumerable<int>) vectorCounts);
      }

      public VertexBuffer CreateVertexBuffer(IEnumerable<int> vectorCounts)
      {
        return (VertexBuffer) new GLVertexBuffer(this, vectorCounts);
      }

      public VertexBuffer CreateVertexBuffer(
        IShaderProgram shaderProgram,
        IEnumerable<string> names,
        IEnumerable<int> vectorCounts)
      {
        return (VertexBuffer) new GLVertexBuffer(this, shaderProgram, names, vectorCounts);
      }

      public ITexture CreateTexture(int width, int height)
      {
        return this.CreateTexture(width, height, 4, new byte[width * height * 4], false);
      }

      public ITexture CreateTexture(
        int width,
        int height,
        int channels,
        byte[] pixels,
        bool toCompress = false)
      {
        if (Thread.CurrentThread == this._videoAdapter.ContextThread)
          return (ITexture) new GLTexture(this, width, height, channels, pixels, toCompress);
        GLTexture texture = (GLTexture) null;
        using (AutoResetEvent autoResetEvent = new AutoResetEvent(false))
        {
          this._videoAdapter.Dispatch((Action) (() =>
          {
            texture = new GLTexture(this, width, height, channels, pixels, toCompress);
            autoResetEvent.Set();
          }));
          if (!autoResetEvent.WaitOne(5000))
            throw new TimeoutException("OpenGL texture creation timed out.");
        }
        return (ITexture) texture;
      }

      public void SetTexture(ITexture texture) => this.SetTexture(0, texture);

      public void SetTexture(int index, ITexture texture)
      {
        if (texture == null)
          return;
        GL.ActiveTexture((TextureUnit) (33984 + index));
        GL.BindTexture(TextureTarget.Texture2D, ((GLTexture) texture).Id);
      }

      public void SetTextures(IEnumerable<ITexture> textures)
      {
        int num = 0;
        foreach (ITexture texture in textures)
          this.SetTexture(num++, texture);
      }

      public IFramebuffer CreateFrameBuffer(int width, int height, int numTextures = 1)
      {
        return (IFramebuffer) new GLFramebuffer(this, width, height, numTextures);
      }

      public void RenderToBackBuffer()
      {
        Vector2i clientSize1 = this._videoAdapter.ClientSize;
        int x1 = clientSize1.X;
        clientSize1 = this._videoAdapter.ClientSize;
        int y1 = clientSize1.Y;
        this.CurrentFramebuffer = (IFramebuffer) GLFramebuffer.FromBackBuffer(this, x1, y1);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        Vector2i clientSize2 = this._videoAdapter.ClientSize;
        int x2 = clientSize2.X;
        clientSize2 = this._videoAdapter.ClientSize;
        int y2 = clientSize2.Y;
        GL.Viewport(0, 0, x2, y2);
      }

      public void Update()
      {
      }

      public void ClearBuffer()
      {
        GL.Clear(ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit);
      }

      public void ClearDepthBuffer() => GL.Clear(ClearBufferMask.DepthBufferBit);

      public void ClearColourBuffer(int index) => GL.ClearBuffer(OpenTK.Graphics.OpenGL.ClearBuffer.Color, index, new int[4]);

      public IBuffer CreateBuffer() => (IBuffer) new GLBuffer(this);

      public IVertexArray CreateVertexArray() => (IVertexArray) new GLVertexArray(this);
    }
}
