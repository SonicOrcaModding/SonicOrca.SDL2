using SDL2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SonicOrca.SDL2
{
    internal static class Sdl2NativeLoader
    {
        private static bool _registered;

        internal static void EnsureRegistered()
        {
            if (_registered)
                return;
            _registered = true;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;
            NativeLibrary.SetDllImportResolver(typeof(SDL).Assembly, Resolve);
        }

        private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            bool isSdlFamily = false;
            foreach (string candidate in GetCandidateNames(libraryName))
            {
                isSdlFamily = true;
                try
                {
                    if (NativeLibrary.TryLoad(candidate, assembly, searchPath, out IntPtr handle))
                        return handle;
                }
                catch
                {
                    // try next name
                }
            }

            if (!isSdlFamily)
                return IntPtr.Zero;

            return NativeLibrary.Load(libraryName, assembly, searchPath);
        }

        private static IEnumerable<string> GetCandidateNames(string libraryName)
        {
            if (string.Equals(libraryName, "SDL2.dll", StringComparison.OrdinalIgnoreCase))
            {
                yield return "libSDL2-2.0.so.0";
                yield return "libSDL2-2.0.so";
                yield return "libSDL2.so";
            }
            else if (string.Equals(libraryName, "SDL2_image.dll", StringComparison.OrdinalIgnoreCase))
            {
                yield return "libSDL2_image-2.0.so.0";
                yield return "libSDL2_image-2.0.so";
                yield return "libSDL2_image.so";
            }
            else if (string.Equals(libraryName, "SDL2_mixer.dll", StringComparison.OrdinalIgnoreCase))
            {
                yield return "libSDL2_mixer-2.0.so.0";
                yield return "libSDL2_mixer-2.0.so";
                yield return "libSDL2_mixer.so";
            }
            else if (string.Equals(libraryName, "SDL2_ttf.dll", StringComparison.OrdinalIgnoreCase))
            {
                yield return "libSDL2_ttf-2.0.so.0";
                yield return "libSDL2_ttf-2.0.so";
                yield return "libSDL2_ttf.so";
            }
        }
    }
}
