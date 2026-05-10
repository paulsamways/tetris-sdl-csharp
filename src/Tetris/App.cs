using System.Diagnostics;
using System.Drawing;
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

  private GameState State = new();

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
    if (_counter > (Sdl3Sharp.Timing.Time.NanosecondsPerSecond / 3))
    {
      _counter = 0;
      if (!_paused)
        State.Step();
    }
  }
  protected virtual void OnDraw()
  {
    _renderer.DrawColor = _backgroundColor;
    _ = _renderer.TryClear();

    var boardDimensions = GetBoardDimensions();
    var boardX = (_windowWidth - boardDimensions.Item1) / 2;
    var boardY = (_windowHeight - boardDimensions.Item2) / 2;

    _ = _renderer.DrawColor = ColorPalette.Foreground;
    _ = _renderer.TryRenderRect(new Rect<float>(boardX, boardY, boardDimensions.Item1, boardDimensions.Item2));

    var rows = State.Board.GetLength(0);
    var columns = State.Board.GetLength(1);

    var tetrominoHeight = boardDimensions.Item2 / rows;
    var tetrominoWidth = boardDimensions.Item1 / columns;

    for (var y = 0; y < rows; y++)
    {
      for (var x = 0; x < columns; x++)
      {
        var tetromino = State.Board[y, x];
        if (tetromino != Tetromino.None)
        {
          _ = _renderer.DrawColor = ColorPalette.GetTetrominoColor(tetromino);

          float innerWidth = (float)(tetrominoWidth * .95);
          float margin = (float)(tetrominoWidth * .025);

          var tetrominoX = boardX + (x * tetrominoWidth) + margin;
          var tetrominoY = boardY + (y * tetrominoHeight) + margin;

          _ = _renderer.TryRenderRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));
        }
      }
    }

    foreach (var (x, y) in State.CurrentPiece.GetPositions(State.CurrentRotation))
    {
      _ = _renderer.DrawColor = ColorPalette.GetTetrominoColor(State.CurrentPiece.Tetromino);

      float innerWidth = (float)(tetrominoWidth * .95);
      float margin = (float)(tetrominoWidth * .025);

      var tetrominoX = boardX + ((x + State.CurrentPosition.X) * tetrominoWidth) + margin;
      var tetrominoY = boardY + ((y + State.CurrentPosition.Y) * tetrominoHeight) + margin;

      _ = _renderer.TryRenderRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));
    }

    _ = _renderer.TryRenderPresent();
  }

  private (float, float) GetBoardDimensions()
  {
    float width = (float)(_windowWidth * .9);
    float height = (float)(_windowHeight * .9);
    float halfHeight = height / 2;

    if (width > halfHeight)
      return (halfHeight, height);

    return (width, width * 2);
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
          State.MoveLeft();
          break;
        case Keycode.Right:
          State.MoveRight();
          break;
        case Keycode.Down:
          State.Step();
          break;
        case Keycode.Up:
          State.Rotate();
          break;
        case Keycode.Escape:
          State.Reset();
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
