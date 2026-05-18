using Sdl3Sharp.Video.Drawing;

namespace Tetris;


public class GameState
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

  public int Score { get; private set; }

  public Tetromino[,] Board { get; private set; }


  public Piece CurrentPiece { get; private set; }

  public Point<int> CurrentPosition { get; private set; }

  public Point<int> GhostPosition { get; private set; }

  public int CurrentRotation { get; private set; }


  public Tetromino[] TetrominoBag { get; }

  public int NextTetrominoIndex { get; private set; }

  public Tetromino[] GetUpcomingTetrominoes(int count)
  {
    if (count <= 0)
      return [];

    var available = TetrominoBag.Length - NextTetrominoIndex;
    if (available <= 0)
      return [];

    var length = Math.Min(count, available);
    var upcoming = new Tetromino[length];

    for (var i = 0; i < length; i++)
      upcoming[i] = TetrominoBag[NextTetrominoIndex + i];

    return upcoming;
  }

  private void LockCurrentTetromino()
  {
    foreach (var (x, y) in CurrentPiece.GetPositions(CurrentRotation))
      Board[CurrentPosition.Y + y, CurrentPosition.X + x] = CurrentPiece.Tetromino;
  }

  private void ClearRows()
  {
    // Use a two-pointer compaction: readRow scans upward from the bottom,
    // writeRow tracks where the next surviving row should land. Full rows are
    // skipped by readRow without advancing writeRow, which naturally compacts
    // surviving rows downward in a single O(rows × columns) pass.
    var writeRow = Board.GetLength(0) - 1;
    var clearedRows = 0;

    // Hoisted so its final value is visible after the loop: when the loop breaks
    // early on an empty row, readRow marks the boundary above which all rows are
    // already empty and do not need blanking.
    var readRow = Board.GetLength(0) - 1;
    for (; readRow >= 0; readRow--)
    {
      // Classify the row in one pass: track both full and completely empty.
      var filledCells = 0;
      for (var x = 0; x < Board.GetLength(1); x++)
      {
        if (Board[readRow, x] != Tetromino.None)
          filledCells++;
      }

      if (filledCells == 0)
      {
        // Pieces stack from the bottom, so an entirely empty row means every
        // row above is also empty — nothing left to compact.
        break;
      }

      if (filledCells == Board.GetLength(1))
      {
        // Skip this row; writeRow stays put so the gap gets overwritten.
        clearedRows++;
        continue;
      }

      // Copy the surviving row down to writeRow only when the pointers diverge.
      if (writeRow != readRow)
      {
        for (var x = 0; x < Board.GetLength(1); x++)
        {
          Board[writeRow, x] = Board[readRow, x];
        }
      }

      writeRow--;
    }

    if (clearedRows == 0)
      return;

    // Blank only the rows between writeRow and readRow: rows at readRow and above
    // were already empty when we broke out of the loop, so there is no work to do there.
    for (var y = writeRow; y > readRow; y--)
    {
      for (var x = 0; x < Board.GetLength(1); x++)
      {
        Board[y, x] = Tetromino.None;
      }
    }

    Score += clearedRows;
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
    RecalculateGhostPosition();
  }

  private void RecalculateGhostPosition()
  {
    var ghostPosition = CurrentPosition;

    while (!Collision((ghostPosition.X, ghostPosition.Y + 1), CurrentRotation))
      ghostPosition = (ghostPosition.X, ghostPosition.Y + 1);

    GhostPosition = ghostPosition;
  }

  private bool Collision(Point<int> position, int rotation)
  {
    var boardHeight = Board.GetLength(0);
    var boardWidth = Board.GetLength(1);

    // Check only occupied cells so padded 4x4 rotation matrices do not create
    // false collisions near walls.
    foreach (var (x, y) in CurrentPiece.GetPositions(rotation))
    {
      var boardX = position.X + x;
      var boardY = position.Y + y;

      if (boardX < 0 || boardX >= boardWidth || boardY < 0 || boardY >= boardHeight)
        return true;

      if (Board[boardY, boardX] != Tetromino.None)
        return true;
    }

    return false;
  }

  private bool TrySetPosition(Point<int>? position = null, int? rotation = null)
  {
    var nextPosition = position ?? CurrentPosition;
    var nextRotation = rotation ?? CurrentRotation;

    if (Collision(nextPosition, nextRotation))
      return false;

    var hasHorizontalChange = nextPosition.X != CurrentPosition.X;
    var hasRotationChange = nextRotation != CurrentRotation;

    CurrentPosition = nextPosition;
    CurrentRotation = nextRotation;

    // The landing row only depends on board, x and rotation. A pure Y move keeps
    // ghost landing unchanged, so we can skip recomputation in that hot path.
    if (hasHorizontalChange || hasRotationChange)
      RecalculateGhostPosition();

    return true;
  }

  private void LockClearAndAdvance()
  {
    LockCurrentTetromino();
    ClearRows();
    AdvanceToNextTetromino();
  }

  public void Reset()
  {
    for (var y = 0; y < Board.GetLength(0); y++)
    {
      for (var x = 0; x < Board.GetLength(1); x++)
      {
        Board[y, x] = Tetromino.None;
      }
    }

    Score = 0;
    NextTetrominoIndex = int.MaxValue;

    AdvanceToNextTetromino();
  }

  public void Step()
  {
    if (!TrySetPosition((CurrentPosition.X, CurrentPosition.Y + 1)))
      LockClearAndAdvance();
  }

  public void MoveLeft() =>
    _ = TrySetPosition((CurrentPosition.X - 1, CurrentPosition.Y));

  public void MoveRight() =>
    _ = TrySetPosition((CurrentPosition.X + 1, CurrentPosition.Y));

  public void Rotate() =>
    TryRotateWithWallKicks();

  private void TryRotateWithWallKicks()
  {
    var nextRotation = CurrentRotation < 3 ? CurrentRotation + 1 : 0;
    var kickTests = CurrentPiece.ClockwiseKickTests[CurrentRotation];

    foreach (var (x, y) in kickTests)
    {
      if (TrySetPosition((CurrentPosition.X + x, CurrentPosition.Y + y), nextRotation))
        return;
    }
  }

  public void Drop()
  {
    _ = TrySetPosition(GhostPosition);
    LockClearAndAdvance();
  }
}
