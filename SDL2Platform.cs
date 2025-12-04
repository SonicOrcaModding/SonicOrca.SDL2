// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.SDL2Platform
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using OpenTK.Graphics.OpenGL;
using SDL2;
using SonicOrca.Audio;
using SonicOrca.Graphics;
using SonicOrca.Input;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SonicOrca.SDL2
{

    public class SDL2Platform : IPlatform, IDisposable
    {
      private static readonly SDL2Platform _singleton = new SDL2Platform();
      private bool _initialised;
      private SDL2WindowContext _window;
      private SDL2InputContext _input;
      private SDL2AudioContext _audio;

      [DllImport("kernel32.dll")]
      private static extern IntPtr LoadLibrary(string path);

      public WindowContext Window => (WindowContext) this._window;

      public InputContext Input => (InputContext) this._input;

      public AudioContext Audio => (AudioContext) this._audio;

      public static SDL2Platform Instance => SDL2Platform._singleton;

      private SDL2Platform()
      {
      }

      public void Initialise()
      {
        this._initialised = !this._initialised ? true : throw new InvalidOperationException("Platform already initialised.");
        Trace.WriteLine("Initialising SDL2 platform");
        Trace.WriteLine("-- SDL2 " + (object) this.GetVersion());
        Trace.Indent();
        this._window = new SDL2WindowContext(this);
        this._input = new SDL2InputContext(this);
        this._audio = new SDL2AudioContext(this);
        Trace.Unindent();
      }

      public void Dispose()
      {
        this._initialised = this._initialised ? false : throw new InvalidOperationException("Platform not initialised.");
        Trace.WriteLine("Disposing SDL2 platform");
        Trace.Indent();
        if (this._audio != null)
          this._audio.Dispose();
        if (this._input != null)
          this._input.Dispose();
        if (this._window != null)
          this._window.Dispose();
        Trace.Unindent();
      }

      public Version GetVersion()
      {
        SDL.SDL_version x;
        SDL.SDL_VERSION(out x);
        return new Version((int) x.major, (int) x.minor, (int) x.patch);
      }

      public Version GetOpenGLVersion()
      {
        if (!this._initialised)
          throw new InvalidOperationException("Platform not initialised.");
        int data1;
        GL.GetInteger(GetPName.MajorVersion, out data1);
        int data2;
        GL.GetInteger(GetPName.MinorVersion, out data2);
        return new Version(data1, data2);
      }

      public string GraphicsAPI
      {
        get
        {
          Version openGlVersion = this.GetOpenGLVersion();
          return $"OpenGL {openGlVersion.Major}.{openGlVersion.Minor}";
        }
      }

      public string GraphicsVendor => GL.GetString(StringName.Vendor);

      public int TotalMemory => SDL.SDL_GetSystemRAM();
    }
}
