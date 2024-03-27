using System;
using System.Collections.Generic;
using System.Linq;

//#forcedebug
namespace RazorEnhanced
{
    internal class RemoveTrapsLoop
    {
        private readonly uint gumpID = 368468644;

        private class Cell
        {
            public Cell(int x, int y, CellType type)
            {
                X = x;
                Y = y;
                Type = type;
            }
            public enum CellType
            {
                EmptyCell,
                TraversedCell,
                DestinationCell,
                InvalidCell
            }
            public int X { get; set; }
            public int Y { get; set; }
            public CellType Type { get; set; }
        }

        private enum Direction
        {
            Unknown = 0,
            Up = 1,
            Right = 2,
            Down = 3,
            Left = 4,
            Invalid = 5
        }

        public RemoveTrapsLoop()
        {
        }

        public void Run()
        {
            Target target = new Target();
            int trapSerial = target.PromptTarget("Select the trap to disarm");

            int counter = 0;
            while (true)
            {
                RemoveTrap(trapSerial);
                Misc.SendMessage($"Solved Traps: {++counter}", 33);
                Misc.Pause(3000);
            }

        }

        private void RemoveTrap(int trapSerial)
        {
            Player.UseSkill("Remove Trap");
            Target.WaitForTarget(1000, false);
            Target.TargetExecute(trapSerial);

            Gumps.WaitForGump(gumpID, 5000);

            string game = Gumps.GetGumpRawData(gumpID);
            Cell.CellType[,] gameBoard = CalculateGameMatrix(ParseGameGump(game));
            Direction[,] visited = new Direction[gameBoard.GetLength(0), gameBoard.GetLength(1)];

            int safeCounter = 30;
            while (true)
            {
                bool gameResult = PlayGame(gameBoard, ref visited);

                if (gameResult) break;
                else
                {
                    if (safeCounter-- <= 0)
                    {
                        Misc.SendMessage("Safe counter reached. Something went wrong", 33);
                        Misc.SendMessage("Going to reset the visited matrix", 33);
                        for (int i = 0; i < visited.GetLength(0); i++)
                        {
                            for (int j = 0; j < visited.GetLength(1); j++)
                            {
                                visited[i, j] = Direction.Unknown;
                            }
                        }
                        safeCounter = 30;
                    }

                    if (Gumps.HasGump())
                    {
                        continue;
                    }
                    while (true)
                    {
                        Misc.Pause(5500); // 4000 from wait for gump fail on moveTo + 5500
                        Player.UseSkill("Remove Trap");
                        Target.WaitForTarget(1000, false);
                        Target.TargetExecute(trapSerial);
                        bool bRet = Gumps.WaitForGump(gumpID, 5000);
                        if (bRet) break;
                    }
                }
            }
            PrintSolution(visited);
            Player.HeadMessage(33, "WIN!!");
        }

        private void PrintSolution(Direction[,] visited)
        {
            string result = "Solution:\n";

            // The last cell is always the destination cell and I add it with a fake value
            visited[visited.GetLength(0) - 1, visited.GetLength(1) - 1] = Direction.Up;

            for (int i = 0; i < visited.GetLength(0); i++)
            {
                for (int j = 0; j < visited.GetLength(1); j++)
                {
                    if (visited[i, j] == Direction.Invalid) visited[i, j] = Direction.Unknown;
                    result += visited[i, j] != Direction.Unknown ? "X" : "0";
                }
                result += "\n";
            }

            Misc.SendMessage(result, 33);
        }

        private List<Cell> ParseGameGump(string game)
        {
            List<string> neededGraphicsElements = new List<string> {
                    "9720", // gray diamond
                    "2152", // azure diamond
                    "2472", // rosso diamond
                };

            // Create a list of strings that contains the gumppic elements of the gump that I'm interested only
            List<string> gump_elements = game
                .Split('{')
                .Select(element => element.Replace("}", "").Trim())
                .Where(element => element.StartsWith("gumppic"))
                .Where(element => neededGraphicsElements.Any(useful => element.Contains(useful)))
                .ToList();

            List<Cell> cells = new List<Cell>();

            foreach (string element in gump_elements)
            {
                string[] parts = element.Split(' ');
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                string type = parts[3];

                Cell.CellType cellType = Cell.CellType.EmptyCell;
                switch (type)
                {
                    case "9720":
                        cellType = Cell.CellType.EmptyCell;
                        break;
                    case "2152":
                        cellType = Cell.CellType.TraversedCell;
                        break;
                    case "2472":
                        cellType = Cell.CellType.DestinationCell;
                        break;
                    default:
                        break;
                }
                cells.Add(new Cell(x, y, cellType));
            }

            return cells;
        }

        // Based on the cell position on the gump, calculate a matrix of the game
        private Cell.CellType[,] CalculateGameMatrix(List<Cell> game_elements)
        {
            int size = (int)Math.Sqrt(game_elements.Count);
            Player.HeadMessage(33, $"{size}x{size}");
            Cell.CellType[,] matrix = new Cell.CellType[size, size];
            game_elements = game_elements.OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList();
            for (int i = 0; i < game_elements.Count; i++)
            {
                int x = i / size;
                int y = i % size;
                matrix[x, y] = game_elements[i].Type;
            }
            return matrix;
        }

        private bool MoveTo(Direction direction)
        {
            Journal journal = new Journal();
            journal.Clear();

            Gumps.SendAction(gumpID, (int)direction);
            bool ret = Gumps.WaitForGump(gumpID, 3000);

            bool win = journal.Search("successfully disarm");
            bool fail = journal.Search("fail to disarm the trap");
            
            Misc.Pause(1000);

            if (win) return true;
            if (fail) return false;

            return ret;
        }

        private bool PlayGame(Cell.CellType[,] game_matrix, ref Direction[,] visited)
        {
            int N = game_matrix.GetLength(0);
            int row = 0;
            int col = 0;

            while (game_matrix[row, col] != Cell.CellType.DestinationCell)
            {
                Direction nextDirection = visited[row, col];

                if (nextDirection != Direction.Unknown)
                {
                    if (MoveTo(nextDirection))
                    {
                        switch (nextDirection)
                        {
                            case Direction.Up:
                                row--;
                                break;
                            case Direction.Right:
                                col++;
                                break;
                            case Direction.Down:
                                row++;
                                break;
                            case Direction.Left:
                                col--;
                                break;
                        }
                        Misc.Pause(700);
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                // Try moving down
                if (row + 1 < N && game_matrix[row + 1, col] != Cell.CellType.TraversedCell && visited[row + 1, col] == Direction.Unknown)
                {
                    if (MoveTo(Direction.Down))
                    {
                        visited[row, col] = Direction.Down;
                        row++;
                        continue;
                    }
                    else
                    {
                        visited[row + 1, col] = Direction.Invalid;
                        return false;
                    }
                }

                // Try moving right
                if (col + 1 < N && game_matrix[row, col + 1] != Cell.CellType.TraversedCell && visited[row, col + 1] == Direction.Unknown)
                {
                    if (MoveTo(Direction.Right))
                    {
                        visited[row, col] = Direction.Right;
                        col++;
                        continue;
                    }
                    else
                    {
                        visited[row, col + 1] = Direction.Invalid;
                        return false;
                    }
                }

                // Try moving left
                if (col - 1 >= 0 && game_matrix[row, col - 1] != Cell.CellType.TraversedCell && visited[row, col - 1] == Direction.Unknown)
                {
                    if (MoveTo(Direction.Left))
                    {
                        visited[row, col] = Direction.Left;
                        col--;
                        continue;
                    }
                    else
                    {
                        visited[row, col - 1] = Direction.Invalid;
                        return false;
                    }
                }

                // Try moving up
                if (row - 1 >= 0 && game_matrix[row - 1, col] != Cell.CellType.TraversedCell && visited[row - 1, col] == Direction.Unknown)
                {
                    if (MoveTo(Direction.Up))
                    {
                        visited[row, col] = Direction.Up;
                        row--;
                        continue;
                    }
                    else
                    {
                        visited[row - 1, col] = Direction.Invalid;
                        return false;
                    }
                }

                // If none of the moves are valid there is an Invalid tile that needs to be
                // removed. I try to remove all the Invalid tiles
                for (int i = 0; i < visited.GetLength(0); i++)
                {
                    for (int j = 0; j < visited.GetLength(1); j++)
                    {
                        if (visited[i, j] == Direction.Invalid)
                        {
                            visited[i, j] = Direction.Unknown;
                        }
                    }
                }
            }

            // Trap disarmed
            return true;
        }

    }
}