// Decompiled with JetBrains decompiler
// Type: SonicOrca.SDL2.SDL2Exception
// Assembly: SonicOrca.SDL2, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 8E58CAA6-91C2-4B5A-9120-3E146868C58C
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.SDL2.dll

using SDL2;
using System;
using System.Runtime.Serialization;

namespace SonicOrca.SDL2
{

    [Serializable]
    public class SDL2Exception : Exception
    {
      public SDL2Exception()
      {
      }

      public SDL2Exception(string message)
        : base(message)
      {
      }

      public SDL2Exception(string message, Exception inner)
        : base(message, inner)
      {
      }

      protected SDL2Exception(SerializationInfo info, StreamingContext context)
        : base(info, context)
      {
      }

      public static SDL2Exception FromError() => new SDL2Exception(SDL.SDL_GetError());

      public static SDL2Exception FromError(string basicMessage)
      {
        return new SDL2Exception(basicMessage + Environment.NewLine + SDL.SDL_GetError());
      }
    }
}
