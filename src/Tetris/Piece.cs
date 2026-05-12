using Sdl3Sharp.Video.Drawing;

namespace Tetris;

public record struct Piece
{
  public required Tetromino Tetromino { get; init; }
  public required byte[][,] Rotations { get; init; }

  public IEnumerable<Point<int>> GetPositions(byte rotation)
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

  public int GetRows(byte rotation) => Rotations[rotation].GetLength(0);

  public int GetColumns(byte rotation) => Rotations[rotation].GetLength(1);


  public static Piece FromArray(Tetromino tetromino, params byte[][,] rotations)
  {
    if (rotations.Length == 1)
      rotations = [rotations[0], rotations[0], rotations[0], rotations[0]];
    else if (rotations.Length == 2)
      rotations = [rotations[0], rotations[1], rotations[0], rotations[1]];
    else if (rotations.Length != 4)
      throw new ArgumentException("Must provide either a single rotation, two rotations or four rotations");


    return new() { Tetromino = tetromino, Rotations = rotations };
  }

  public readonly static Piece[] Pieces =
  [
      FromArray(Tetromino.None,
        new byte[,] { { 0 } }
      ),
      FromArray(Tetromino.I,
        new byte[,] {{1, 1, 1, 1}},
        new byte[,] {{1}, {1}, {1}, {1}}
      ),
      FromArray(Tetromino.J,
        new byte[,]
        {
          { 0, 1 },
          { 0, 1 },
          { 1, 1 },
        },
        new byte[,]
        {
          { 1, 0, 0 },
          { 1, 1, 1 },
        },
        new byte[,]
        {
          { 1, 1 },
          { 1, 0 },
          { 1, 0 },
        },
        new byte[,]
        {
          { 1, 1, 1 },
          { 0, 0, 1 },
        }
      ),
      FromArray(Tetromino.L,
        new byte[,]
        {
          { 1, 0 },
          { 1, 0 },
          { 1, 1 },
        },
        new byte[,]
        {
          { 1, 1, 1 },
          { 1, 0, 0 },
        },
        new byte[,]
        {
          { 1, 1 },
          { 0, 1 },
          { 0, 1 },
        },
        new byte[,]
        {
          { 0, 0, 1 },
          { 1, 1, 1 },
        }
      ),
      FromArray(Tetromino.O,
        new byte[,]
        {
          { 1, 1 },
          { 1, 1 }
        }
      ),
      FromArray(Tetromino.S,
        new byte[,]
        {
          { 0, 1, 1 },
          { 1, 1, 0 },
        },
        new byte[,]
        {
          { 1, 0 },
          { 1, 1 },
          { 0, 1 },
        }
      ),
      FromArray(Tetromino.T,
        new byte[,]
        {
          { 0, 1, 0 },
          { 1, 1, 1 },
        },
        new byte[,]
        {
          { 1, 0 },
          { 1, 1 },
          { 1, 0 },
        },
        new byte[,]
        {
          { 1, 1, 1 },
          { 0, 1, 0 },
        },
        new byte[,]
        {
          { 0, 1 },
          { 1, 1 },
          { 0, 1 },
        }
      ),
      FromArray(Tetromino.Z,
        new byte[,]
        {
          { 1, 1, 0 },
          { 0, 1, 1 },
        },
        new byte[,]
        {
          { 0, 1 },
          { 1, 1 },
          { 1, 0 },
        }
      ),
  ];
}
