// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.SDL2InputContext
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using SDL2;
using SonicOrca.Geometry;
using SonicOrca.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SonicOrca.SDL2
{

    public class SDL2InputContext : InputContext, IDisposable
    {
      private readonly SDL2Platform _platform;
      private readonly IntPtr _windowHandle;
      private readonly List<IntPtr> _gameControllers = new List<IntPtr>();
      private readonly List<IntPtr> _haptic = new List<IntPtr>();
      private SDL.SDL_Event[] events = new SDL.SDL_Event[512 /*0x0200*/];

      public SDL2InputContext(SDL2Platform platform)
      {
        this._platform = platform;
        this._windowHandle = ((SDL2WindowContext) this._platform.Window).WindowHandle;
        Trace.WriteLine("Initialising SDL2 joystick");
        if (SDL.SDL_InitSubSystem(512U /*0x0200*/) != 0)
          throw SDL2Exception.FromError("Unable to initialise SDL2 joystick subsystem.");
        Trace.WriteLine("Initialising SDL2 game controller");
        if (SDL.SDL_InitSubSystem(8192U /*0x2000*/) != 0)
          throw SDL2Exception.FromError("Unable to initialise SDL2 game controller subsystem.");
        Trace.WriteLine("Initialising SDL2 haptic");
        if (SDL.SDL_InitSubSystem(4096U /*0x1000*/) != 0)
          throw SDL2Exception.FromError("Unable to initialise SDL2 haptic subsystem.");
        this.FindGameControllers();
      }

      public void Dispose()
      {
        Trace.WriteLine("Quitting SDL2 haptic");
        SDL.SDL_QuitSubSystem(4096U /*0x1000*/);
        Trace.WriteLine("Quitting SDL2 game controller");
        SDL.SDL_QuitSubSystem(8192U /*0x2000*/);
        Trace.WriteLine("Quitting SDL2 joystick");
        SDL.SDL_QuitSubSystem(512U /*0x0200*/);
      }

      private void FindGameControllers()
      {
        this._gameControllers.Clear();
        for (int joystick_index = 0; joystick_index < SDL.SDL_NumJoysticks(); ++joystick_index)
        {
          if (SDL.SDL_IsGameController(joystick_index) != SDL.SDL_bool.SDL_FALSE)
          {
            IntPtr gamecontroller = SDL.SDL_GameControllerOpen(joystick_index);
            if (gamecontroller != IntPtr.Zero)
            {
              IntPtr zero = IntPtr.Zero;
              IntPtr joystick = SDL.SDL_GameControllerGetJoystick(gamecontroller);
              this._gameControllers.Add(gamecontroller);
              IntPtr haptic = SDL.SDL_HapticOpenFromJoystick(joystick);
              if (haptic != IntPtr.Zero && SDL.SDL_HapticRumbleInit(haptic) != 0)
                haptic = IntPtr.Zero;
              this._haptic.Add(haptic);
            }
          }
        }
      }

      private void VibrateController(int index, double left, double right, int duration)
      {
        if (this._haptic.Count <= index)
          return;
        IntPtr haptic = this._haptic[index];
        if (haptic == IntPtr.Zero)
          return;
        SDL.SDL_HapticRumblePlay(haptic, (float) ((left + right) / 2.0), (uint) duration);
      }

      public override unsafe void UpdateCurrentState()
      {
        int num1 = 0;
        this.TextInput = (string) null;
        SDL.SDL_StartTextInput();
        int num2 = SDL.SDL_PeepEvents(this.events, this.events.Length, SDL.SDL_eventaction.SDL_PEEKEVENT, SDL.SDL_EventType.SDL_FIRSTEVENT, SDL.SDL_EventType.SDL_LASTEVENT);
        SDL.SDL_Event _event;
        for (int index1 = 0; index1 < num2; ++index1)
        {
          _event = this.events[index1];
          switch (_event.type)
          {
            case SDL.SDL_EventType.SDL_TEXTINPUT:
              IntPtr text = (IntPtr) (void*) _event.text.text;
              byte[] bytes = new byte[32 /*0x20*/];
              int index2 = 0;
              while (index2 < 32 /*0x20*/ && (bytes[index2] = Marshal.ReadByte(text, index2)) != (byte) 0)
                ++index2;
              this.TextInput = Encoding.UTF8.GetString(bytes, 0, index2);
              break;
            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
              num1 = _event.wheel.y;
              break;
          }
        }
        do
          ;
        while (SDL.SDL_PollEvent(out _event) != 0);
        int numkeys;
        IntPtr keyboardState1 = SDL.SDL_GetKeyboardState(out numkeys);
        bool[] keys = new bool[numkeys];
        for (int ofs = 0; ofs < numkeys; ++ofs)
          keys[ofs] = Marshal.ReadByte(keyboardState1, ofs) > (byte) 0;
        int x1;
        int y;
        uint mouseState1 = SDL.SDL_GetMouseState(out x1, out y);
        SDL.SDL_GetWindowSize(this._windowHandle, out int _, out int _);
        MouseState mouseState2 = new MouseState();
        mouseState2.X = x1;
        mouseState2.Y = y;
        mouseState2.Left = (mouseState1 & SDL.SDL_BUTTON_LMASK) > 0U;
        mouseState2.Middle = (mouseState1 & SDL.SDL_BUTTON_MMASK) > 0U;
        mouseState2.Right = (mouseState1 & SDL.SDL_BUTTON_RMASK) > 0U;
        mouseState2.Wheel = (double) num1;
        KeyboardState keyboardState2 = new KeyboardState(keys);
        GamePadInputState[] array = Enumerable.Range(0, 4).Select<int, GamePadInputState>((Func<int, GamePadInputState>) (x => new GamePadInputState())).ToArray<GamePadInputState>();
        for (int index = 0; index < this._gameControllers.Count; ++index)
        {
          IntPtr gameController = this._gameControllers[index];
          SDL.SDL_GameControllerGetJoystick(gameController);
          GamePadInputState gamePadInputState = array[index];
          double minTolerance = 0.2;
          double maxTolerance = 0.75;
          gamePadInputState.South = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A);
          gamePadInputState.East = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B);
          gamePadInputState.West = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X);
          gamePadInputState.North = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y);
          gamePadInputState.Start = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START);
          gamePadInputState.Select = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK);
          gamePadInputState.LeftBumper = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER);
          gamePadInputState.RightBumper = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER);
          gamePadInputState.LeftAxisButton = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK);
          gamePadInputState.RightAxisButton = this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK);
          Vector2i vector2i = new Vector2i();
          if (this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT))
            vector2i.X = -1;
          if (this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT))
            vector2i.X = 1;
          if (this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP))
            vector2i.Y = -1;
          if (this.IsGameControllerButtonDown(gameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN))
            vector2i.Y = 1;
          gamePadInputState.POV = vector2i;
          gamePadInputState.LeftAxis = this.GetGameControllerAxis(gameController, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY, minTolerance, maxTolerance);
          gamePadInputState.RightAxis = this.GetGameControllerAxis(gameController, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY, minTolerance, maxTolerance);
          gamePadInputState.LeftTrigger = this.GetGameControllerAxis(gameController, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT, minTolerance, maxTolerance);
          gamePadInputState.RightTrigger = this.GetGameControllerAxis(gameController, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT, minTolerance, maxTolerance);
          array[index] = gamePadInputState;
        }
        this.CurrentState = new InputState(mouseState2, keyboardState2, array);
        GamePadOutputState[] gamePad = this.OutputState.GamePad;
        if (!this.IsVibrationEnabled || this.OutputState.GamePad == null)
          return;
        for (int index = 0; index < gamePad.Length; ++index)
          this.VibrateController(index, gamePad[index].LeftVibration, gamePad[index].RightVibration, 4000);
      }

      private bool IsGameControllerButtonDown(IntPtr controller, SDL.SDL_GameControllerButton button)
      {
        return SDL.SDL_GameControllerGetButton(controller, button) > (byte) 0;
      }

      private Vector2 GetGameControllerAxis(
        IntPtr controller,
        SDL.SDL_GameControllerAxis x,
        SDL.SDL_GameControllerAxis y,
        double minTolerance,
        double maxTolerance)
      {
        return new Vector2(this.GetGameControllerAxis(controller, x, minTolerance, maxTolerance), this.GetGameControllerAxis(controller, y, minTolerance, maxTolerance));
      }

      private double GetGameControllerAxis(
        IntPtr controller,
        SDL.SDL_GameControllerAxis axis,
        double minTolerance,
        double maxTolerance)
      {
        double num1 = (double) SDL.SDL_GameControllerGetAxis(controller, axis) / (double) short.MaxValue;
        double num2 = Math.Abs(num1);
        if (num2 < minTolerance)
          return 0.0;
        return num2 > maxTolerance ? 1.0 * (double) Math.Sign(num1) : (num2 - minTolerance) / (maxTolerance - minTolerance) * (double) Math.Sign(num1);
      }

      public override char GetKeyCode(int scancode)
      {
        return (char) SDL.SDL_GetKeyFromScancode((SDL.SDL_Scancode) scancode);
      }
    }
}
