// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.SDL2WindowContext
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using SDL2;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SonicOrca.SDL2
{

    public class SDL2WindowContext : WindowContext
    {
      public const int DEFAULT_HICON_RESOURCE_ID = 32512;
      private const int HideMouseIdleTimeMs = 3000;
      private readonly SDL2Platform _platform;
      private readonly ConcurrentQueue<Action> _dispatchedActions = new ConcurrentQueue<Action>();
      private readonly GLGraphicsContext _glGraphicsContext;
      private IntPtr _windowHandle;
      private IntPtr _glContext;
      private Vector2i _clientSize;
      private Vector2i _aspectRatio;
      private VideoMode _mode;
      private string _windowTitle;
      private uint _lastMouseMovementTickCount;
      private bool _showingCursor = true;
      private SDL.SDL_Event[] events = new SDL.SDL_Event[512 /*0x0200*/];

      public IntPtr WindowHandle => this._windowHandle;

      public Thread ContextThread { get; private set; }

      public override SonicOrca.Graphics.IGraphicsContext GraphicsContext
      {
        get => (SonicOrca.Graphics.IGraphicsContext) this._glGraphicsContext;
      }

      public bool IsMaxPerformance { get; set; }

      public override bool FullScreen
      {
        get => this.Mode != 0;
        set
        {
          if (value)
          {
            if (this.Mode != VideoMode.Windowed)
              return;
            this.Mode = VideoMode.WindowedBorderless;
          }
          else
            this.Mode = VideoMode.Windowed;
        }
      }

      public override VideoMode Mode
      {
        get => this._mode;
        set
        {
          if (this._mode == value)
            return;
          this._mode = value;
          uint flags = 0;
          switch (value)
          {
            case VideoMode.Fullscreen:
              flags = 1U;
              break;
            case VideoMode.WindowedBorderless:
              flags = 4097U;
              break;
          }
          SDL.SDL_SetWindowFullscreen(this._windowHandle, flags);
          if (value != VideoMode.Windowed)
            return;
          this.UpdateWindowSize(new Vector2i(1920, 1080));
        }
      }

      public override string WindowTitle
      {
        get => this._windowTitle;
        set
        {
          this._windowTitle = value;
          if (!(this._windowHandle != IntPtr.Zero))
            return;
          SDL.SDL_SetWindowTitle(this._windowHandle, value);
        }
      }

      public override Vector2i ClientSize
      {
        get => this._clientSize;
        set => this.UpdateWindowSize(value);
      }

      public override Vector2i AspectRatio
      {
        get => this._aspectRatio;
        set
        {
          if (!(this._aspectRatio != value))
            return;
          this._aspectRatio = value;
          this.UpdateWindowSize();
        }
      }

      public override bool HideCursorIfIdle
      {
        get => base.HideCursorIfIdle;
        set
        {
          base.HideCursorIfIdle = value;
          if (!value)
            return;
          this.ShowCursor = true;
        }
      }

      public bool ShowCursor
      {
        get => this._showingCursor;
        set
        {
          if (this._showingCursor == value)
            return;
          this._showingCursor = value;
          SDL.SDL_ShowCursor(value ? 1 : 0);
        }
      }

      public SDL2WindowContext(SDL2Platform platform)
      {
        this._platform = platform;
        Trace.WriteLine("Initialising SDL2 video");
        if (SDL.SDL_InitSubSystem(32U /*0x20*/) != 0)
          throw SDL2Exception.FromError("Unable to initialise a video driver.");
        SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_MULTISAMPLESAMPLES, 1);
        SDL.SDL_GL_SetSwapInterval(1);
        SDL.SDL_DisplayMode mode;
        SDL.SDL_GetDesktopDisplayMode(0, out mode);
        int num1 = 1920;
        int num2;
        for (num2 = 1080; num1 > mode.w || num2 > mode.h; num2 -= 270)
          num1 -= 480;
        Trace.WriteLine("Creating window");
        this._windowHandle = SDL.SDL_CreateWindow(this._windowTitle, 805240832, 805240832, num1, num2, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        if (this._windowHandle == IntPtr.Zero)
          throw SDL2Exception.FromError("Unable to create window.");
        this._clientSize = new Vector2i(num1, num2);
        this.SetIconToAssemblyResource();
        Trace.WriteLine("Creating OpenGL context");
        this._glContext = SDL.SDL_GL_CreateContext(this._windowHandle);
        if (this._glContext == IntPtr.Zero)
          throw SDL2Exception.FromError("Unable to create OpenGL context.");
        this.SetOpenTKOpenGLHandle(this._glContext, this.GetWindowHWND(this._windowHandle));
        this.ContextThread = Thread.CurrentThread;
        this._glGraphicsContext = new GLGraphicsContext(this);
        this.ShowWindowWithBlackBackground();
      }

      private void SetIconToAssemblyResource()
      {
        IntPtr lParam = SDL2WindowContext.User32.LoadIcon(Marshal.GetHINSTANCE(Assembly.GetEntryAssembly().ManifestModule), new IntPtr(32512));
        if (!(lParam != IntPtr.Zero))
          return;
        SDL2WindowContext.User32.SendMessage(this.GetWindowHWND(this._windowHandle), 128U /*0x80*/, new IntPtr(0), lParam);
      }

      private void SetOpenTKOpenGLHandle(IntPtr glHandle, IntPtr windowHandle)
      {
        Toolkit.Init();
        Utilities.CreateWindowsWindowInfo(windowHandle);
        ContextHandle ch = new ContextHandle(this._glContext);
        ((IGraphicsContextInternal) new OpenTK.Graphics.GraphicsContext(ch, (OpenTK.Graphics.GraphicsContext.GetAddressDelegate) (proc => SDL.SDL_GL_GetProcAddress(proc)), (OpenTK.Graphics.GraphicsContext.GetCurrentContextDelegate) (() => ch))).Implementation.LoadAll();
      }

      private IntPtr GetWindowHWND(IntPtr window)
      {
        SDL.SDL_SysWMinfo info = new SDL.SDL_SysWMinfo();
        int windowWmInfo = (int) SDL.SDL_GetWindowWMInfo(window, ref info);
        return info.info.win.window;
      }

      public override void Dispose()
      {
        Trace.WriteLine("Deleting OpenGL context");
        SDL.SDL_GL_DeleteContext(this._glContext);
        Trace.WriteLine("Closing window");
        SDL.SDL_DestroyWindow(this._windowHandle);
        Trace.WriteLine("Quitting SDL2 video");
        SDL.SDL_QuitSubSystem(32U /*0x20*/);
      }

      public override void Update()
      {
        uint ticks = SDL.SDL_GetTicks();
        SDL.SDL_PumpEvents();
        int num = SDL.SDL_PeepEvents(this.events, this.events.Length, SDL.SDL_eventaction.SDL_PEEKEVENT, SDL.SDL_EventType.SDL_FIRSTEVENT, SDL.SDL_EventType.SDL_LASTEVENT);
        for (int index = 0; index < num; ++index)
        {
          SDL.SDL_Event sdlEvent = this.events[index];
          if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
            this.Finished = true;
          switch (sdlEvent.type)
          {
            case SDL.SDL_EventType.SDL_WINDOWEVENT:
              switch (sdlEvent.window.windowEvent)
              {
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                  this.UpdateWindowSize(new Vector2i(sdlEvent.window.data1, sdlEvent.window.data2));
                  continue;
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                  this.Finished = true;
                  continue;
                default:
                  continue;
              }
            case SDL.SDL_EventType.SDL_KEYDOWN:
              if ((sdlEvent.key.keysym.mod & SDL.SDL_Keymod.KMOD_ALT) != SDL.SDL_Keymod.KMOD_NONE && sdlEvent.key.keysym.sym == SDL.SDL_Keycode.SDLK_F4)
              {
                this.Finished = true;
                break;
              }
              break;
            case SDL.SDL_EventType.SDL_MOUSEMOTION:
              if (this.HideCursorIfIdle)
              {
                this._lastMouseMovementTickCount = ticks;
                this.ShowCursor = true;
                break;
              }
              break;
          }
        }
        Action result;
        while (this._dispatchedActions.TryDequeue(out result))
          result();
        this._glGraphicsContext.Update();
        if (!this.HideCursorIfIdle || ticks - this._lastMouseMovementTickCount <= 3000U)
          return;
        this.ShowCursor = false;
      }

      public override void BeginRender()
      {
        GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
        this._glGraphicsContext.BlendMode = BlendMode.Alpha;
        this._glGraphicsContext.RenderToBackBuffer();
      }

      public override void EndRender()
      {
        if (!SonicOrcaGameContext.IsMaxPerformance)
          GL.Finish();
        SDL.SDL_GL_SwapWindow(this._windowHandle);
      }

      public void Dispatch(Action action) => this._dispatchedActions.Enqueue(action);

      private void ShowWindowWithBlackBackground()
      {
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1f);
        SDL.SDL_GL_SwapWindow(this._windowHandle);
        SDL.SDL_ShowWindow(this._windowHandle);
      }

      private void UpdateWindowSize() => this.UpdateWindowSize(this._clientSize);

      private void UpdateWindowSize(Vector2i size)
      {
        if (this._aspectRatio.X != 0 || this._aspectRatio.Y != 0)
        {
          double num = (double) this._aspectRatio.X / (double) this._aspectRatio.Y;
          if (this._clientSize.X != size.X)
            size.Y = (int) ((double) size.X / num);
          else if (this._clientSize.Y != size.Y)
            size.X = (int) ((double) size.Y * num);
        }
        this._clientSize = size;
        SDL.SDL_SetWindowSize(this._windowHandle, size.X, size.Y);
        GL.Viewport(0, 0, size.X, size.Y);
      }

      private static class User32
      {
        public const int WM_SETICON = 128 /*0x80*/;
        public const int ICON_SMALL = 0;

        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
      }
    }
}
