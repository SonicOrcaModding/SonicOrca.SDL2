// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.GLTexture
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using OpenTK.Graphics.OpenGL;
using SonicOrca.Graphics;
using SonicOrca.Resources;
using System;

namespace SonicOrca.SDL2
{

    internal class GLTexture : ITexture, IDisposable, ILoadedResource
    {
      private readonly GLGraphicsContext _context;
      private int _glId;
      private TextureFiltering _filtering;
      private TextureWrapping _wrapping;

      public Resource Resource { get; set; }

      public int Width { get; private set; }

      public int Height { get; private set; }

      public TextureFiltering Filtering
      {
        get => this._filtering;
        set
        {
          if (value == this._filtering)
            return;
          TextureMinFilter textureMinFilter = TextureMinFilter.Linear;
          TextureMagFilter textureMagFilter = TextureMagFilter.Linear;
          if ((this._filtering = value) == TextureFiltering.NearestNeighbour)
          {
            textureMinFilter = TextureMinFilter.Nearest;
            textureMagFilter = TextureMagFilter.Nearest;
          }
          GL.BindTexture(TextureTarget.Texture2D, this._glId);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) textureMinFilter);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) textureMagFilter);
        }
      }

      public TextureWrapping Wrapping
      {
        get => this._wrapping;
        set
        {
          if (value == this._wrapping)
            return;
          this._wrapping = value;
          TextureWrapMode textureWrapMode;
          switch (value)
          {
            case TextureWrapping.Clamp:
              textureWrapMode = TextureWrapMode.Clamp;
              break;
            case TextureWrapping.RepeatMirrored:
              textureWrapMode = TextureWrapMode.MirroredRepeat;
              break;
            default:
              textureWrapMode = TextureWrapMode.Repeat;
              break;
          }
          GL.BindTexture(TextureTarget.Texture2D, this._glId);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) textureWrapMode);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) textureWrapMode);
        }
      }

      public int Id => this._glId;

      public GLTexture(
        GLGraphicsContext context,
        int width,
        int height,
        int channels,
        byte[] argb,
        bool toCompress = false)
      {
        this._context = context;
        this.Width = width;
        this.Height = height;
        this._glId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, this._glId);
        if (toCompress)
          GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, PixelInternalFormat.CompressedRgbaS3tcDxt1Ext, width, height, 0, channels == 3 ? PixelFormat.Rgb : PixelFormat.Rgba, PixelType.UnsignedByte, argb);
        else
          GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, channels == 3 ? PixelFormat.Rgb : PixelFormat.Rgba, PixelType.UnsignedByte, argb);
        this._filtering = TextureFiltering.Linear;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 9729);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 9729);
        this._wrapping = TextureWrapping.Clamp;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, 10496);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, 10496);
        this._context.Textures.Add((ITexture) this);
      }

      public void Dispose()
      {
        GL.DeleteTexture(this._glId);
        this._context.Textures.Remove((ITexture) this);
      }

      public void OnLoaded()
      {
      }

      public byte[] GetArgbData()
      {
        byte[] pixels = new byte[this.Width * this.Height * 4];
        GL.GetTexImage<byte>(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        int num1 = this.Width * 4;
        for (int index1 = 0; index1 < this.Height / 2; ++index1)
        {
          for (int index2 = 0; index2 < this.Width; ++index2)
          {
            int index3 = index1 * num1 + index2 * 4;
            int index4 = (this.Height - index1 - 1) * num1 + index2 * 4;
            byte num2 = pixels[index3];
            byte num3 = pixels[index3 + 1];
            byte num4 = pixels[index3 + 2];
            byte num5 = pixels[index3 + 3];
            pixels[index3] = pixels[index4];
            pixels[index3 + 1] = pixels[index4 + 1];
            pixels[index3 + 2] = pixels[index4 + 2];
            pixels[index3 + 3] = pixels[index4 + 3];
            pixels[index4] = num2;
            pixels[index4 + 1] = num3;
            pixels[index4 + 2] = num4;
            pixels[index4 + 3] = num5;
          }
        }
        for (int index = 0; index < pixels.Length; index += 4)
        {
          byte num6 = pixels[index];
          byte num7 = pixels[index + 1];
          byte num8 = pixels[index + 2];
          byte num9 = pixels[index + 3];
          pixels[index] = num8;
          pixels[index + 1] = num7;
          pixels[index + 2] = num6;
          pixels[index + 3] = num9;
        }
        return pixels;
      }

      public void SetArgbData(int width, int height, byte[] data)
      {
        PixelFormat format = PixelFormat.Bgra;
        if (data.Length < width * height * 4)
          format = PixelFormat.Bgr;
        GL.BindTexture(TextureTarget.Texture2D, this._glId);
        GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, format, PixelType.UnsignedByte, data);
      }
    }
}
