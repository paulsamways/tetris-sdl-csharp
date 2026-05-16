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
    if (_counter > (Sdl3Sharp.Timing.Time.NanosecondsPerSecond / 2))
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

    var boardSize = GetBoardSize();

    _renderer.Viewport = new Rect<int>(
      (_windowWidth - boardSize.Width) / 2,
      (_windowHeight - boardSize.Height) / 2,
      boardSize.Width,
      boardSize.Height
    );

    OnDrawBoard(boardSize.Width, boardSize.Height);

    _renderer.ResetViewport();

    _ = _renderer.TryRenderPresent();
  }

  private void OnDrawBoard(float width, float height)
  {
    _ = _renderer.DrawColor = ColorPalette.Foreground;
    _ = _renderer.TryRenderRect(new Rect<float>(0, 0, width, height));

    var tetrominoWidth = width / State.Board.GetLength(1);
    var tetrominoHeight = height / State.Board.GetLength(0);

    for (var y = 0; y < State.Board.GetLength(0); y++)
    {
      for (var x = 0; x < State.Board.GetLength(1); x++)
      {
        var tetromino = State.Board[y, x];
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

    foreach (var (x, y) in State.CurrentPiece.GetPositions(State.CurrentRotation))
    {
      float innerWidth = (float)(tetrominoWidth * .95);
      float margin = (float)(tetrominoWidth * .025);

      var tetrominoX = ((x + State.CurrentPosition.X) * tetrominoWidth) + margin;
      var tetrominoY = ((y + State.CurrentPosition.Y) * tetrominoHeight) + margin;

      _ = _renderer.DrawColor = ColorPalette.GetTetrominoBackgroundColor(State.CurrentPiece.Tetromino);
      _ = _renderer.TryRenderFilledRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));

      // _ = _renderer.DrawColor = ColorPalette.GetTetrominoBorderColor(State.CurrentPiece.Tetromino);
      // _ = _renderer.TryRenderRect(new Rect<float>(tetrominoX, tetrominoY, innerWidth, innerWidth));
    }

    foreach (var (x, y) in State.CurrentPiece.GetPositions(State.CurrentRotation))
    {
      float innerWidth = (float)(tetrominoWidth * .95);
      float margin = (float)(tetrominoWidth * .025);

      var tetrominoX = ((x + State.GhostPosition.X) * tetrominoWidth) + margin;
      var tetrominoY = ((y + State.GhostPosition.Y) * tetrominoHeight) + margin;

      _ = _renderer.DrawColor = ColorPalette.GetTetrominoBackgroundColor(State.CurrentPiece.Tetromino);
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
        case Keycode.Space:
          State.Drop();
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
