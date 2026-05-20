using System.Drawing;
using System.Runtime.InteropServices;
using Sdl3Sharp;
using Sdl3Sharp.Events;
using Sdl3Sharp.Input;
using Sdl3Sharp.Video.Coloring;
using Sdl3Sharp.Video.Drawing;
using Sdl3Sharp.Video.Rendering;
using Sdl3Sharp.Video.Windowing;

namespace Tetris;

public class App : AppBase
{
  private Window _window = default!;
  private Renderer _renderer = default!;

  private readonly static Color<byte> _backgroundColor = (18, 19, 20, 1);

  private int _windowHeight = 600;
  private int _windowWidth = 600;

  private bool _paused = false;

  private readonly GameState _state = new();

  protected override AppResult OnInitialize(Sdl sdl, string[] args)
  {
    if (!Window.TryCreateWithRenderer("Hello World", _windowWidth, _windowHeight, out _window!, out _renderer!, WindowFlags.Resizable))
    {
      return Failure;
    }

    return Continue;
  }

  private ulong _lastNanoTicks = 0;



  protected override AppResult OnIterate(Sdl sdl)
  {
    var ticks = Sdl3Sharp.Timing.Timer.NanosecondTicks;
    var delta = ticks - _lastNanoTicks;
    _lastNanoTicks = ticks;
    var delta_seconds = (float)delta / Sdl3Sharp.Timing.Time.NanosecondsPerSecond;

    OnUpdate(delta, delta_seconds);

    // Render
    OnDraw();

    return Continue;
  }

  private ulong _counter = 0;

  protected virtual void OnUpdate(ulong deltaTicks, float deltaSeconds)
  {
    _counter += deltaTicks;
    if (_counter > (Sdl3Sharp.Timing.Time.NanosecondsPerSecond / 2))
    {
      _counter = 0;
      if (!_paused)
        _state.Step();
    }
  }
  protected virtual void OnDraw()
  {
    _renderer.DrawColor = _backgroundColor;
    _ = _renderer.TryClear();

    var boardSize = GetBoardSize();
    var boardCellSize = boardSize.Width / (float)_state.Board.GetLength(1);
    var previewGap = (int)MathF.Max(12, boardCellSize * .6f);
    var previewWidth = (int)MathF.Max(120, boardCellSize * 5.5f);

    var totalWidth = boardSize.Width + previewGap + previewWidth;
    var boardX = (_windowWidth - totalWidth) / 2;
    if (boardX < 12)
      boardX = 12;

    var boardY = (_windowHeight - boardSize.Height) / 2;

    _renderer.Viewport = new Rect<int>(
      boardX,
      boardY,
      boardSize.Width,
      boardSize.Height
    );

    OnDrawBoard(boardSize.Width, boardSize.Height);

    _renderer.ResetViewport();

    OnDrawScore(
      boardX + boardSize.Width + previewGap,
      boardY,
      previewWidth,
      boardSize.Height / 10,
      _state.Score
    );

    OnDrawUpcomingPieces(
      boardX + boardSize.Width + previewGap,
      boardY + ((boardSize.Height / 10) * 2),
      previewWidth,
      (boardSize.Height / 10) * 8,
      5
    );

    _ = _renderer.TryRenderPresent();
  }

  private void OnDrawScore(int x, int y, int width, int height, int score)
  {
    _renderer.DrawColor = ColorPalette.Foreground;
    _ = _renderer.TryRenderRect(new Rect<float>(x, y, width, height));

    OnDrawString(string.Format("{0:d5}", score), x + 10, y + 10, (width - 30) / 5, height - 20, 2f, new Color<float>(.8f, .1f, .1f, 1f), Justify.Left);
  }

  private void OnDrawUpcomingPieces(int x, int y, int width, int height, int count)
  {
    _renderer.DrawColor = ColorPalette.Foreground;
    _ = _renderer.TryRenderRect(new Rect<float>(x, y, width, height));

    _renderer.DrawColor = ColorPalette.White;
    _ = _renderer.TryRenderDebugText(x + 8, y + 8, "Next");

    var upcoming = _state.GetUpcomingTetrominoes(count);
    if (upcoming.Length == 0)
      return;

    var topPadding = 28f;
    var verticalPadding = 8f;
    var slotHeight = (height - topPadding - (verticalPadding * 2f)) / count;

    for (var i = 0; i < upcoming.Length; i++)
    {
      var tetromino = upcoming[i];
      var piece = Piece.Pieces[(int)tetromino];

      var slotY = y + topPadding + (i * slotHeight);
      var blockSize = MathF.Min((width - 20f) / 4f, (slotHeight - 10f) / 4f);
      var innerSize = blockSize * .9f;
      var blockMargin = (blockSize - innerSize) / 2f;

      var gridWidth = blockSize * 4f;
      var gridHeight = blockSize * 4f;
      var originX = x + ((width - gridWidth) / 2f);
      var originY = slotY + ((slotHeight - gridHeight) / 2f);

      foreach (var (gridX, gridY) in piece.GetPositions(0))
      {
        var drawX = originX + (gridX * blockSize) + blockMargin;
        var drawY = originY + (gridY * blockSize) + blockMargin;

        _renderer.DrawColor = ColorPalette.GetTetrominoBackgroundColor(tetromino);
        _ = _renderer.TryRenderFilledRect(new Rect<float>(drawX, drawY, innerSize, innerSize));
      }
    }
  }

  private enum Justify { Left, Right }

  private void OnDrawString(string s, float x, float y, float charWidth, float charHeight, float charSpacing, Color<float> color, Justify justify = Justify.Left)
  {
    for (var i = 0; i < s.Length; i++)
    {
      var x2 = justify == Justify.Left
        ? x + (i * (charWidth + charSpacing))
        : x - ((i + 1) * charWidth) + (i * charSpacing);
      OnDrawChar(s[i], x2, y, charWidth, charHeight, color);
    }
  }

  private void OnDrawChar(char c, float x, float y, float width, float height, Color<float> color)
  {
    var w = width / 3;
    var h = height / 7;

    void DrawCell(bool flip, UInt64 data, float x, float y)
    {
      var m = w / 2;
      var points = new List<Vertex>(9);

      var x1 = x;
      var x2 = x + m;
      var x3 = x + w;

      var y1 = y;
      var y2 = y + h;

      if (flip)
      {
        if ((data & 4) == 4)
        {
          points.Add(new(new(x1, y2), color, new(0, 0)));
          points.Add(new(new(x1, y1), color, new(0, 0)));
          points.Add(new(new(x2, y2), color, new(0, 0)));
        }

        if ((data & 2) == 2)
        {
          points.Add(new(new(x2, y2), color, new(0, 0)));
          points.Add(new(new(x1, y1), color, new(0, 0)));
          points.Add(new(new(x3, y1), color, new(0, 0)));
        }

        if ((data & 1) == 1)
        {
          points.Add(new(new(x2, y2), color, new(0, 0)));
          points.Add(new(new(x3, y1), color, new(0, 0)));
          points.Add(new(new(x3, y2), color, new(0, 0)));
        }
      }
      else
      {
        if ((data & 4) == 4)
        {
          points.Add(new(new(x1, y1), color, new(0, 0)));
          points.Add(new(new(x1, y2), color, new(0, 0)));
          points.Add(new(new(x2, y1), color, new(0, 0)));
        }

        if ((data & 2) == 2)
        {
          points.Add(new(new(x2, y1), color, new(0, 0)));
          points.Add(new(new(x1, y2), color, new(0, 0)));
          points.Add(new(new(x3, y2), color, new(0, 0)));
        }

        if ((data & 1) == 1)
        {
          points.Add(new(new(x2, y1), color, new(0, 0)));
          points.Add(new(new(x3, y2), color, new(0, 0)));
          points.Add(new(new(x3, y1), color, new(0, 0)));
        }
      }

      _ = _renderer.TryRenderGeometry(CollectionsMarshal.AsSpan(points));
    }

    var data = Tri63CharTable[c];

    for (var row = 0; row < 7; row++)
    {
      for (var column = 0; column < 3; column++)
      {
        var cell = 60 - ((3 * row) + column) * 3;

        var flip = row == 2 || row == 4 || row == 6;

        DrawCell(flip, data >> cell, x + (w * column), y + (h * row));
      }
    }
  }

  public static readonly Dictionary<char, ulong> Tri63CharTable = new()
  {
    // Digits
    { '0', 0b__011_111_110__111_000_111__111_000_111__111_000_111__111_000_111__111_000_111__011_111_110 },
    { '1', 0b__011_111_000__000_111_000__000_111_000__000_111_000__000_111_000__000_111_000__000_111_000 },
    { '2', 0b__011_111_110__111_000_111__000_001_110__000_011_100__001_110_000__011_100_000__111_111_111 },
    { '3', 0b__011_111_110__111_000_111__000_001_110__000_001_110__000_000_111__010_000_111__011_111_110 },
    { '4', 0b__011_000_111__111_000_111__111_000_111__111_111_111__000_000_111__000_000_111__000_000_111 },
    { '5', 0b__011_111_110__111_000_000__111_000_000__111_111_110__000_000_111__000_000_111__011_111_110 },
    { '6', 0b__011_111_110__111_000_000__111_100_000__111_111_110__111_000_111__111_000_111__011_111_110 },
    { '7', 0b__111_111_111__000_000_111__000_001_110__000_011_100__001_110_000__011_100_000__111_000_000 },
    { '8', 0b__011_111_110__111_000_111__011_000_110__011_111_110__111_000_111__111_000_111__011_111_110 },
    { '9', 0b__011_111_110__111_000_111__011_100_111__001_111_111__000_000_111__000_000_111__011_111_110 },

    { 'A', 0b__011_111_110__111_000_111__111_000_111__111_111_111__111_000_111__111_000_111__111_000_111 },
    { 'B', 0b__111_111_110__111_000_111__111_000_110__111_111_110__111_000_111__111_000_111__111_111_110 },
    { 'C', 0b__011_111_110__111_000_111__111_000_000__111_000_000__111_000_000__111_000_010__011_111_110 },
    { 'D', 0b__111_111_110__111_000_111__111_000_111__111_000_111__111_000_111__111_000_111__111_111_110 },
    { 'E', 0b__111_111_111__111_000_000__111_000_000__111_111_000__111_000_000__111_000_000__111_111_111 },
    { 'F', 0b__111_111_111__111_000_000__111_000_000__111_111_000__111_000_000__111_000_000__111_000_000 },
    { 'G', 0b__011_111_110__111_000_111__111_000_000__111_001_110__111_000_111__111_000_111__011_111_110 },
    { 'H', 0b__111_000_111__111_000_111__111_000_111__111_111_111__111_000_111__111_000_111__111_000_111 },
    { 'I', 0b__111_111_111__000_111_000__000_111_000__000_111_000__000_111_000__000_111_000__111_111_111 },
    { 'J', 0b__000_000_111__000_000_111__000_000_111__000_000_111__000_000_111__000_000_111__111_111_110 },
    { 'K', 0b__111_000_111__111_000_111__111_001_110__111_011_100__111_011_100__111_001_110__111_000_111 },
    { 'L', 0b__111_000_000__111_000_000__111_000_000__111_000_000__111_000_000__111_000_000__111_111_111 },
    { 'M', 0b__111_000_111__111_000_111__111_101_111__111_111_111__111_010_111__111_000_111__111_000_111 },
    { 'N', 0b__111_000_111__111_000_111__111_100_111__111_110_111__111_011_111__111_001_111__111_000_111 },
    { 'O', 0b__011_111_110__111_000_111__111_000_111__111_000_111__111_000_111__111_000_111__011_111_110 },
    { 'P', 0b__111_111_110__111_000_111__111_001_110__111_111_100__111_000_000__111_000_000__111_000_000 },
    { 'Q', 0b__011_111_110__111_000_111__111_000_111__111_000_111__111_001_110__111_011_100__011_110_001 },
    { 'R', 0b__111_111_110__111_000_111__111_001_110__111_111_100__111_011_100__111_001_110__111_000_111 },
    { 'S', 0b__011_111_111__111_000_000__011_100_000__001_111_110__000_000_111__000_000_111__111_111_110 },
    { 'T', 0b__111_111_111__000_111_000__000_111_000__000_111_000__000_111_000__000_111_000__000_111_000 },
    { 'U', 0b__111_000_111__111_000_111__111_000_111__111_000_111__111_000_111__111_000_111__011_111_110 },
    { 'V', 0b__111_000_111__111_000_111__111_000_111__111_000_111__111_001_110__111_011_100__111_110_000 },
    { 'W', 0b__111_000_111__111_000_111__111_000_111__111_010_111__111_111_111__111_101_111__111_000_111 },
    { 'X', 0b__111_000_111__111_000_111__011_101_110__001_111_100__001_111_100__011_101_110__111_000_111 },
    { 'Y', 0b__111_000_111__111_000_111__011_100_111__001_110_111__000_011_111__000_001_111__000_000_111 },
    { 'Z', 0b__111_111_111__000_000_111__000_001_110__000_011_100__001_110_000__011_100_000__111_111_111 },

    { ':', 0b__000_000_000__000_000_000__000_111_000__000_000_000__000_000_000__000_111_000__000_000_000 },
    { '!', 0b__000_111_000__000_111_000__000_111_000__000_111_000__000_111_000__000_000_000__000_111_000 },
    { '-', 0b__000_000_000__000_000_000__000_000_000__111_111_111__000_000_000__000_000_000__000_000_000 },
    { '_', 0b__000_000_000__000_000_000__000_000_000__000_000_000__000_000_000__000_000_000__111_111_111 },
  };

  private void OnDrawBoard(float width, float height)
  {
    _ = _renderer.DrawColor = ColorPalette.Foreground;
    _ = _renderer.TryRenderRect(new Rect<float>(0, 0, width, height));

    var tetrominoWidth = width / _state.Board.GetLength(1);
    var tetrominoHeight = height / _state.Board.GetLength(0);

    for (var y = 0; y < _state.Board.GetLength(0); y++)
    {
      for (var x = 0; x < _state.Board.GetLength(1); x++)
      {
        var tetromino = _state.Board[y, x];
        if (tetromino != Tetromino.None)
        {
          float innerWidth = (float)(tetrominoWidth * .95);
          float margin = (float)(tetrominoWidth * .025);

          var tetrominoX = (x * tetrominoWidth) + margin;
          var tetrominoY = (y * tetrominoHeight) + margin;

          _ = _renderer.DrawColor = ColorPalette.GetTetrominoBackgroundColor(tetromino);
          _ = _renderer.TryRenderFilledRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));

          // _ = _renderer.DrawColor = ColorPalette.GetTetrominoBorderColor(tetromino);
          // _ = _renderer.TryRenderRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));
        }
      }
    }

    foreach (var (x, y) in _state.CurrentPiece.GetPositions(_state.CurrentRotation))
    {
      float innerWidth = (float)(tetrominoWidth * .95);
      float margin = (float)(tetrominoWidth * .025);

      var tetrominoX = ((x + _state.CurrentPosition.X) * tetrominoWidth) + margin;
      var tetrominoY = ((y + _state.CurrentPosition.Y) * tetrominoHeight) + margin;

      _ = _renderer.DrawColor = ColorPalette.GetTetrominoBackgroundColor(_state.CurrentPiece.Tetromino);
      _ = _renderer.TryRenderFilledRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));

      // _ = _renderer.DrawColor = ColorPalette.GetTetrominoBorderColor(State.CurrentPiece.Tetromino);
      // _ = _renderer.TryRenderRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));
    }

    foreach (var (x, y) in _state.CurrentPiece.GetPositions(_state.CurrentRotation))
    {
      float innerWidth = (float)(tetrominoWidth * .95);
      float margin = (float)(tetrominoWidth * .025);

      var tetrominoX = ((x + _state.GhostPosition.X) * tetrominoWidth) + margin;
      var tetrominoY = ((y + _state.GhostPosition.Y) * tetrominoHeight) + margin;

      _ = _renderer.DrawColor = ColorPalette.GetTetrominoBackgroundColor(_state.CurrentPiece.Tetromino);
      _ = _renderer.TryRenderRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));

      // _ = _renderer.DrawColor = ColorPalette.GetTetrominoBorderColor(State.CurrentPiece.Tetromino);
      // _ = _renderer.TryRenderRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));
    }
  }

  private Size GetBoardSize()
  {
    var width = (int)(_windowWidth * .9);
    var height = (int)(_windowHeight * .9);
    var halfHeight = height / 2;

    if (width > halfHeight)
      return new(halfHeight, height);

    return new(width, width * 2);
  }

  protected override AppResult OnEvent(Sdl sdl, ref Event @event)
  {
    if (@event.Type is EventType.WindowCloseRequested)
    {
      return Success;
    }

    if (@event.Type is EventType.WindowResized && @event.TryAs<WindowEvent>(out var e))
    {
      _windowWidth = e.Target.Data1;
      _windowHeight = e.Target.Data2;
    }

    if (@event.Type is EventType.WindowFocusLost)
      _paused = true;
    if (@event.Type is EventType.WindowFocusGained)
      _paused = false;

    if (@event.Type is EventType.KeyDown && @event.TryAs<KeyboardEvent>(out var keyDownEvent))
    {
      var key = keyDownEvent.GetReferenceOrNull()!.Keycode;

      switch (key)
      {
        case Keycode.Left:
          _state.MoveLeft();
          break;
        case Keycode.Right:
          _state.MoveRight();
          break;
        case Keycode.Down:
          _state.Step();
          break;
        case Keycode.Up:
          _state.Rotate();
          break;
        case Keycode.Escape:
          _state.Reset();
          break;
        case Keycode.Space:
          _state.Drop();
          break;

      }
    }

    return Continue;
  }

  protected override void OnQuit(Sdl sdl, AppResult result)
  {
    _renderer?.Dispose();
    _renderer = default!;

    _window?.Dispose();
    _window = default!;
  }
}
