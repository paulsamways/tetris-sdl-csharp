using Sdl3Sharp.Video.Coloring;

namespace Tetris;

public static class ColorPalette
{
  public static readonly Color<byte> Accent = (97, 175, 239, 1);
  public static readonly Color<byte> Cursor = (171, 178, 191, 1);

  public static readonly Color<byte> Foreground = (171, 178, 191, 1);
  public static readonly Color<byte> Background = (18, 19, 20, 1);

  public static readonly Color<byte> SelectionForeground = (28, 29, 31, 1);
  public static readonly Color<byte> SelectionBackground = (171, 178, 191, 1);

  public static readonly Color<byte> Black = (18, 19, 20, 1);
  public static readonly Color<byte> Red = (224, 108, 117, 1);
  public static readonly Color<byte> Green = (152, 195, 121, 1);
  public static readonly Color<byte> Yellow = (229, 192, 123, 1);
  public static readonly Color<byte> Blue = (97, 175, 239, 1);
  public static readonly Color<byte> Magenta = (198, 120, 221, 1);
  public static readonly Color<byte> Cyan = (86, 182, 194, 1);
  public static readonly Color<byte> White = (171, 178, 191, 1);

  public static readonly Color<byte> BrightBlack = (25, 26, 27, 1);
  public static readonly Color<byte> BrightRed = (224, 108, 117, 1);
  public static readonly Color<byte> BrightGreen = (152, 195, 121, 1);
  public static readonly Color<byte> BrightYellow = (229, 192, 123, 1);
  public static readonly Color<byte> BrightBlue = (97, 175, 239, 1);
  public static readonly Color<byte> BrightMagenta = (198, 120, 221, 1);
  public static readonly Color<byte> BrightCyan = (86, 182, 194, 1);
  public static readonly Color<byte> BrightWhite = (255, 255, 255, 1);

  public static Color<byte> GetTetrominoColor(Tetromino t) => t switch
  {
    Tetromino.None => throw new NotImplementedException(),
    Tetromino.I => Cyan,
    Tetromino.J => Blue,
    Tetromino.L => White,
    Tetromino.O => Yellow,
    Tetromino.S => Green,
    Tetromino.T => Magenta,
    Tetromino.Z => Red,
    _ => throw new NotImplementedException(),
  };
}
