// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.SDL2AudioContext
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using SDL2;
using SonicOrca.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SonicOrca.SDL2
{

    public class SDL2AudioContext : AudioContext, IDisposable
    {
      private readonly SDL2Platform _platform;
      private readonly Lockable<List<ISampleProvider>> _registeredSampleProviders = new Lockable<List<ISampleProvider>>(new List<ISampleProvider>());
      private uint _outputDevice;
      private SDL.SDL_AudioSpec _outputAudioSpec;

      public SDL2AudioContext(SDL2Platform platform)
      {
        this._platform = platform;
        Trace.WriteLine("Initialising SDL2 audio");
        if (SDL.SDL_InitSubSystem(16U /*0x10*/) != 0)
          throw SDL2Exception.FromError("Unable to initialise an audio driver.");
        SDL.SDL_AudioSpec desired = new SDL.SDL_AudioSpec()
        {
          channels = 2,
          format = 32784,
          freq = 44100,
          samples = 2048 /*0x0800*/,
          callback = new SDL.SDL_AudioCallback(this.ReadDataCallback)
        };
        this._outputDevice = SDL.SDL_OpenAudioDevice((string) null, 0, ref desired, out this._outputAudioSpec, 0);
        if (this._outputDevice == 0U)
          throw SDL2Exception.FromError("Unable to create output buffer.");
        SDL.SDL_PauseAudioDevice(this._outputDevice, 0);
      }

      public void Dispose()
      {
        SDL.SDL_CloseAudioDevice(this._outputDevice);
        Trace.WriteLine("Quitting SDL2 video");
        SDL.SDL_QuitSubSystem(16U /*0x10*/);
      }

      public override void RegisterSampleProvider(ISampleProvider sampleProvider)
      {
        lock (this._registeredSampleProviders.Sync)
          this._registeredSampleProviders.Instance.Add(sampleProvider);
      }

      public override void UnregisterSampleProvider(ISampleProvider sampleProvider)
      {
        lock (this._registeredSampleProviders.Sync)
          this._registeredSampleProviders.Instance.Remove(sampleProvider);
      }

      private void ReadDataCallback(IntPtr userdata, IntPtr stream, int length)
      {
        byte[] numArray = new byte[length];
        if (this.Mixer != null)
        {
          ISampleProvider[] array;
          lock (this._registeredSampleProviders.Sync)
            array = this._registeredSampleProviders.Instance.ToArray();
          this.Mixer.Mix(numArray, 0, length, (IEnumerable<ISampleProvider>) array);
        }
        Marshal.Copy(numArray, 0, stream, length);
      }
    }
}
