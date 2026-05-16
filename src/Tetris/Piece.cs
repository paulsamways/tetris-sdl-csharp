using Sdl3Sharp.Video.Drawing;

namespace Tetris;

public record struct Piece
{
  private readonly static Point<int>[][] _clockwiseKicksJlstz =
  [
    [ (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) ],
    [ (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) ],
    [ (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) ],
    [ (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) ],
  ];

  private readonly static Point<int>[][] _clockwiseKicksI =
  [
    [ (0, 0), (-2, 0), (1, 0), (-2, 1), (1, -2) ],
    [ (0, 0), (-1, 0), (2, 0), (-1, -2), (2, 1) ],
    [ (0, 0), (2, 0), (-1, 0), (2, -1), (-1, 2) ],
    [ (0, 0), (1, 0), (-2, 0), (1, 2), (-2, -1) ],
  ];

  private readonly static Point<int>[][] _clockwiseKicksO = [[(0, 0)], [(0, 0)], [(0, 0)], [(0, 0)]];

  public required Tetromino Tetromino { get; init; }
  public required byte[][,] Rotations { get; init; }
  public required Point<int>[][] ClockwiseKickTests { get; init; }

  public IEnumerable<Point<int>> GetPositions(int rotation)
  {
    var value = Rotations[rotation];

    for (int y = 0; y < value.GetLength(0); y++)
    {
      for (int x = 0; x < value.GetLength(1); x++)
      {
        if (value[y, x] != 0)
          yield return (x, y);
      }
    }
  }

  public int GetRows(int rotation) => Rotations[rotation].GetLength(0);

  public int GetColumns(int rotation) => Rotations[rotation].GetLength(1);

  public readonly static Piece[] Pieces =
  [
      new()
      {
        Tetromino = Tetromino.None,
        ClockwiseKickTests = _clockwiseKicksO,
        Rotations =
        [
          new byte[,] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
        ]
      },
      new()
      {
        Tetromino = Tetromino.I,
        ClockwiseKickTests = _clockwiseKicksI,
        Rotations =
        [
          new byte[,] { { 0, 0, 0, 0 }, { 1, 1, 1, 1 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 1, 0 }, { 0, 0, 1, 0 }, { 0, 0, 1, 0 }, { 0, 0, 1, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 1, 1, 1, 1 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 } },
        ]
      },
      new()
      {
        Tetromino = Tetromino.J,
        ClockwiseKickTests = _clockwiseKicksJlstz,
        Rotations =
        [
          new byte[,] { { 1, 0, 0, 0 }, { 1, 1, 1, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 1, 1, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 1, 1, 1, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 1, 1, 0, 0 }, { 0, 0, 0, 0 } },
        ]
      },
      new()
      {
        Tetromino = Tetromino.L,
        ClockwiseKickTests = _clockwiseKicksJlstz,
        Rotations =
        [
          new byte[,] { { 0, 0, 1, 0 }, { 1, 1, 1, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 1, 1, 1, 0 }, { 1, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 1, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 } },
        ]
      },
      new()
      {
        Tetromino = Tetromino.O,
        ClockwiseKickTests = _clockwiseKicksO,
        Rotations =
        [
          new byte[,] { { 0, 0, 0, 0 }, { 0, 1, 1, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 0, 1, 1, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 0, 1, 1, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 0, 1, 1, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
        ]
      },
      new()
      {
        Tetromino = Tetromino.S,
        ClockwiseKickTests = _clockwiseKicksJlstz,
        Rotations =
        [
          new byte[,] { { 0, 1, 1, 0 }, { 1, 1, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 1, 0, 0 }, { 0, 1, 1, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 0, 1, 1, 0 }, { 1, 1, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 1, 0, 0, 0 }, { 1, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 } },
        ]
      },
      new()
      {
        Tetromino = Tetromino.T,
        ClockwiseKickTests = _clockwiseKicksJlstz,
        Rotations =
        [
          new byte[,] { { 0, 1, 0, 0 }, { 1, 1, 1, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 1, 0, 0 }, { 0, 1, 1, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 1, 1, 1, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 1, 0, 0 }, { 1, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 } },
        ]
      },
      new()
      {
        Tetromino = Tetromino.Z,
        ClockwiseKickTests = _clockwiseKicksJlstz,
        Rotations =
        [
          new byte[,] { { 1, 1, 0, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 1, 0 }, { 0, 1, 1, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 0, 0, 0 }, { 1, 1, 0, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
          new byte[,] { { 0, 1, 0, 0 }, { 1, 1, 0, 0 }, { 1, 0, 0, 0 }, { 0, 0, 0, 0 } },
        ]
      },
  ];
}
