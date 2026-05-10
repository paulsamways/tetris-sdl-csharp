using Sdl3Sharp;

namespace Tetris;

public static class Program
{
  public static int Main(string[] args)
  {
    using var sdl = new Sdl(static builder => builder
      .SetAppName("Simple SDL3# Triangle example")
      .InitializeSubSystems(SubSystems.Video));

    return sdl.Run(new App(), args);
  }
}
