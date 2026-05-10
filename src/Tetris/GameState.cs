using Sdl3Sharp.Video.Drawing;

namespace Tetris;


public struct GameState
{
  public GameState()
  {
    TetrominoBag = new Tetromino[7];
    for (var i = 1; i < 8; i++)
      TetrominoBag[i - 1] = (Tetromino)i;

    Board = new Tetromino[20, 10];

    NextTetrominoIndex = int.MaxValue;
    AdvanceToNextTetromino();
  }

  public Tetromino[,] Board { get; private set; }


  public Piece CurrentPiece { get; private set; }

  public Point<ushort> CurrentPosition { get; private set; }

  public byte CurrentRotation { get; private set; }


  public Tetromino[] TetrominoBag { get; }

  public int NextTetrominoIndex { get; private set; }

  private void LockCurrentTetromino()
  {
    foreach (var (x, y) in CurrentPiece.GetPositions(CurrentRotation))
      Board[CurrentPosition.Y + y, CurrentPosition.X + x] = CurrentPiece.Tetromino;
  }

  private void AdvanceToNextTetromino()
  {
    if (NextTetrominoIndex >= TetrominoBag.Length)
    {
      Random.Shared.Shuffle(TetrominoBag);
      NextTetrominoIndex = 0;
    }

    var nextTetromino = TetrominoBag[NextTetrominoIndex++];

    CurrentPiece = Piece.Pieces[(int)nextTetromino];
    CurrentPosition = (3, 0);
    CurrentRotation = 0;
  }


  private bool Collision(Point<ushort> position, byte rotation)
  {
    // Game board collision
    if (position.X < 0 || position.X + CurrentPiece.GetColumns(rotation) > Board.GetLength(1) || position.Y + CurrentPiece.GetRows(rotation) > Board.GetLength(0))
      return true;

    // Tetromino collision
    foreach (var (x, y) in CurrentPiece.GetPositions(rotation))
    {
      if (Board[position.Y + y, position.X + x] != Tetromino.None)
        return true;
    }

    return false;
  }

  public void Reset()
  {
    for (var y = 0; y < Board.GetLength(0); y++)
      for (var x = 0; x < Board.GetLength(1); x++)
        Board[y, x] = Tetromino.None;

    NextTetrominoIndex = int.MaxValue;

    AdvanceToNextTetromino();
  }

  public void Step()
  {
    var nextPosition = new Point<ushort>(CurrentPosition.X, (ushort)(CurrentPosition.Y + 1));
    if (Collision(nextPosition, CurrentRotation))
    {
      LockCurrentTetromino();
      AdvanceToNextTetromino();
    }
    else
    {
      CurrentPosition = nextPosition;
    }
  }

  public void MoveLeft()
  {
    var nextPosition = ((ushort)(CurrentPosition.X - 1), CurrentPosition.Y);
    if (!Collision(nextPosition, CurrentRotation))
      CurrentPosition = nextPosition;
  }

  public void MoveRight()
  {
    var nextPosition = ((ushort)(CurrentPosition.X + 1), CurrentPosition.Y);
    if (!Collision(nextPosition, CurrentRotation))
      CurrentPosition = nextPosition;
  }

  public void Rotate()
  {
    var nextRotation = CurrentRotation < 3 ? (byte)(CurrentRotation + 1) : (byte)0;
    if (!Collision(CurrentPosition, nextRotation))
      CurrentRotation = nextRotation;
  }
}
