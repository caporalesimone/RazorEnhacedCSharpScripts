using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

//#forcedebug
namespace RazorEnhanced
{
    internal class RemoveTrapsTurboTrainer
    {
        const bool STORE_UNKNOWN_SOLUTIONS_ON_FILE = true; // If this flag is true, a file called RemoveTraps.txt will be created in the Data folder with any unknown solution found. Just for debug. All solutions should be known

        private enum Dir
        {
            Invalid = 0,
            Up = 1,
            Right = 2,
            Down = 3,
            Left = 4
        }
        private static string DirectionToString(Dir direction)
        {
            return direction switch
            {
                Dir.Up => "↑",
                Dir.Right => "→",
                Dir.Down => "↓",
                Dir.Left => "←",
                _ => "Unknown",
            };
        }
        private static string DirectionListToString(List<Dir> directions)
        {
            string result = "";
            foreach (Dir direction in directions)
            {
                result += DirectionToString(direction);
            }
            return result;
        }
        /*
        private static Dir StringToDirection(char direction)
        {
            return direction switch
            {
                '↑' => Dir.Up,
                '→' => Dir.Right,
                '↓' => Dir.Down,
                '←' => Dir.Left,
                _ => Dir.Invalid,
            };
        }
        */
        private enum MoveResult
        {
            Disarmed = 0,
            ValidTry = 2,
            WrongTry = 1,
            SomethingWentWrong = 3
        }

        private static readonly List<List<Dir>> known_solutions_3x3 = new()
        {
            new () { Dir.Down, Dir.Right, Dir.Right, Dir.Down },
            new () { Dir.Down, Dir.Right, Dir.Down, Dir.Right },
            new () { Dir.Down, Dir.Down, Dir.Right, Dir.Right },
            new () { Dir.Down, Dir.Down, Dir.Right, Dir.Up, Dir.Up, Dir.Right, Dir.Down, Dir.Down },
            new () { Dir.Right, Dir.Right, Dir.Down, Dir.Down },
            new () { Dir.Right, Dir.Down, Dir.Left, Dir.Down, Dir.Right, Dir.Right },
            new () { Dir.Right, Dir.Down, Dir.Right, Dir.Down },
            new () { Dir.Right, Dir.Down, Dir.Down, Dir.Right }
        };

        private static readonly List<List<Dir>> known_solutions_4x4 = new()
        {
            new () {Dir.Down, Dir.Right, Dir.Down, Dir.Right, Dir.Right, Dir.Down },
            new () {Dir.Down, Dir.Right, Dir.Right, Dir.Up, Dir.Right, Dir.Down, Dir.Down, Dir.Down },
            new () {Dir.Down, Dir.Right, Dir.Right, Dir.Right, Dir.Down, Dir.Down },
            new () {Dir.Down, Dir.Down, Dir.Right, Dir.Right, Dir.Up, Dir.Up, Dir.Right, Dir.Down, Dir.Down, Dir.Down },
            new () {Dir.Down, Dir.Down, Dir.Right, Dir.Up, Dir.Right, Dir.Right, Dir.Down, Dir.Down },
            new () {Dir.Down, Dir.Down, Dir.Down, Dir.Right, Dir.Right, Dir.Right },
            new () {Dir.Right, Dir.Down, Dir.Right, Dir.Right, Dir.Down, Dir.Down },
            new () {Dir.Right, Dir.Down, Dir.Left, Dir.Down, Dir.Right, Dir.Right, Dir.Down, Dir.Right },
            new () {Dir.Right, Dir.Down, Dir.Left, Dir.Down, Dir.Down, Dir.Right, Dir.Right, Dir.Right },
            new () {Dir.Right, Dir.Right, Dir.Down, Dir.Right, Dir.Down, Dir.Down },
            new () {Dir.Right, Dir.Right, Dir.Right, Dir.Down, Dir.Down, Dir.Down },
            new () {Dir.Right, Dir.Right, Dir.Right, Dir.Down, Dir.Left, Dir.Left, Dir.Left, Dir.Down, Dir.Right, Dir.Right, Dir.Right, Dir.Down },
        };

        private static readonly List<List<Dir>> known_solutions_5x5 = new()
        {
            new () { Dir.Down, Dir.Down, Dir.Down, Dir.Down, Dir.Right, Dir.Right, Dir.Right, Dir.Right },
            new () { Dir.Down, Dir.Down, Dir.Right, Dir.Right, Dir.Right, Dir.Up, Dir.Left, Dir.Left, Dir.Up, Dir.Right, Dir.Right, Dir.Right, Dir.Down, Dir.Down, Dir.Down, Dir.Down },
            new () { Dir.Down, Dir.Down, Dir.Right, Dir.Down, Dir.Right, Dir.Right, Dir.Down, Dir.Right },
            new () { Dir.Down, Dir.Right, Dir.Down, Dir.Down, Dir.Down, Dir.Right, Dir.Up, Dir.Up, Dir.Up, Dir.Right, Dir.Right, Dir.Down, Dir.Down, Dir.Down },
            new () { Dir.Down, Dir.Right, Dir.Down, Dir.Right, Dir.Right, Dir.Down, Dir.Right, Dir.Down },
            new () { Dir.Down, Dir.Right, Dir.Right, Dir.Up, Dir.Right, Dir.Right, Dir.Down, Dir.Down, Dir.Down, Dir.Down },
            new () { Dir.Right, Dir.Down, Dir.Left, Dir.Down, Dir.Down, Dir.Right, Dir.Right, Dir.Up, Dir.Up, Dir.Right, Dir.Right, Dir.Down, Dir.Down, Dir.Down },
            new () { Dir.Right, Dir.Down, Dir.Left, Dir.Down, Dir.Right, Dir.Right, Dir.Down, Dir.Right, Dir.Right, Dir.Down },
            new () { Dir.Right, Dir.Down, Dir.Right, Dir.Right, Dir.Down, Dir.Left, Dir.Down, Dir.Right, Dir.Right, Dir.Down },
            new () { Dir.Right, Dir.Right, Dir.Right, Dir.Right, Dir.Down, Dir.Down, Dir.Down, Dir.Down },
            new () { Dir.Right, Dir.Right, Dir.Right, Dir.Right, Dir.Down, Dir.Down, Dir.Left, Dir.Left, Dir.Down, Dir.Down, Dir.Right, Dir.Right },
            new () { Dir.Right, Dir.Right, Dir.Down, Dir.Right, Dir.Right, Dir.Down, Dir.Left, Dir.Left, Dir.Left, Dir.Left, Dir.Down, Dir.Down, Dir.Right, Dir.Right, Dir.Right, Dir.Right },
        };

        public RemoveTrapsTurboTrainer()
        {
        }
        public void Run()
        {
            Target target = new();
            int trapSerial = target.PromptTarget("Select the trap to disarm");

            int counter = 0;
            var training_session_start = DateTime.Now;
            while (true)
            {
                var trap_start = DateTime.Now;
                RemoveTrap(trapSerial);
                var trap_elapsed     = (int)(DateTime.Now - trap_start).TotalSeconds;
                var training_elapsed = (int)(DateTime.Now - training_session_start).TotalSeconds;

                Misc.SendMessage($"============================================", 55);
                Misc.SendMessage($"Solved Trap #{++counter:D4} in {trap_elapsed:D3} seconds", 55);
                Misc.SendMessage($"Total training session elapsed {training_elapsed:D4} seconds", 55);
                Misc.SendMessage($"Average trap resolution time: {((float)training_elapsed / (float)counter):000.0} seconds", 55);
                Misc.SendMessage($"============================================", 55);
                Misc.Pause(5000); // TODO: calculate the right time
            }
        }
        private void RemoveTrap(int trapSerial)
        {
            uint gumpID = OpenTrap(trapSerial);
            if (gumpID == 0) return;

            int size = CalculateTrapSize(gumpID);
            Misc.SendMessage($"Trap size: {size}x{size}", 33);

            PlayGame(gumpID, size, trapSerial);
        }
        private uint OpenTrap(int trapSerial)
        {
            while (true)
            {
                Player.UseSkill("Remove Trap");
                if (Target.WaitForTarget(1000, false)) break;
                Misc.Pause(1000);
            }
            Target.TargetExecute(trapSerial);

            return WaitForRemoveTrapGump();
        }
        private uint WaitForRemoveTrapGump()
        {
            int safeCounter = 5000;
            while (safeCounter-- > 0)
            {
                foreach (uint i in Gumps.AllGumpIDs())
                {
                    if (Gumps.GetGumpRawData(i).Contains("1159005")) return i;  // 1159005 is the cliloc text string of the title of the gump
                }
                Misc.Pause(1);
            }
            Misc.SendMessage("Gump not found", 33);
            return 0;
        }
        private int CalculateTrapSize(uint gumpID)
        {
            string gump_data = Gumps.GetGumpRawData(gumpID);

            int count = Regex.Matches(gump_data, Regex.Escape("9720")).Count; // Counts the number of gray diamonds

            int size = 5;
            if (count <= 7) size = 3;
            if (count > 7 && count < 23) size = 4;

            return size;
        }
        private bool PlayGame(uint gumpID, int size, int trapSerial)
        {
            List<Dir> path = new();
            List<Dir> failedDirections = new();

            MoveResult attemp = MoveResult.ValidTry; // First try is always valid
            Dir TryDirection = Dir.Invalid;

            int SafeCounter = 0;
            while (SafeCounter++ < 50)
            {
                Misc.SendMessage($"Attempt #{SafeCounter}", 33);
                float solutionFitness = CalculatePathFitness(size, path, out List<Dir> foundSolution);
                if (solutionFitness > 0)
                {
                    Misc.SendMessage($"Found a match in solution database with {solutionFitness:00.0}% match", 33);
                    // I continue with the missing steps of the solution
                    for (int i = path.Count; i < foundSolution.Count; i++)
                    {
                        TryDirection = foundSolution[i];
                        attemp = MoveTo(gumpID, (int)TryDirection);

                        // Check if the step is valid. Expected result should be always valid
                        if (attemp == MoveResult.WrongTry || attemp == MoveResult.SomethingWentWrong)
                        {
                            Misc.SendMessage("The found solution is not valid!!", 33);
                            break; // The found solution is not valid
                        }
                    }
                    // Rebuild all the path but not last move because will be added later
                    path = foundSolution.GetRange(0, foundSolution.Count - 1);
                }
                else
                {
                    // First: I repeat all the previous moves if needed
                    if (attemp == MoveResult.WrongTry)
                    {
                        foreach (Dir step in path)
                        {
                            var check = MoveTo(gumpID, (int)step);
                            if (check == MoveResult.WrongTry || check == MoveResult.SomethingWentWrong) return false; // Something went wrong
                        }
                    }

                    TryDirection = NextDirection(size, path, failedDirections);
                    Misc.SendMessage($"Try: {DirectionToString(TryDirection)}", 33);
                    attemp = MoveTo(gumpID, (int)TryDirection);
                }

                switch (attemp)
                {
                    case MoveResult.Disarmed:
                        path.Add(TryDirection);
                        if (STORE_UNKNOWN_SOLUTIONS_ON_FILE) StoreOnFileTheSolution(size, path);
                        return true;
                    case MoveResult.WrongTry:
                        Misc.SendMessage($"Wrong: {DirectionToString(TryDirection)}", 33);
                        failedDirections.Add(TryDirection);
                        if (OpenTrap(trapSerial) != gumpID) return false;
                        break;
                    case MoveResult.ValidTry:
                        path.Add(TryDirection);
                        failedDirections.Clear();
                        break;
                    case MoveResult.SomethingWentWrong:
                        return false;
                }

                string pathString = DirectionListToString(path);
                Misc.SendMessage($"Path: {pathString}", 33);
            }
            Misc.SendMessage("Failed: Too many tries", 33);
            return true;
        }
        private MoveResult MoveTo(uint gumpID, int direction)
        {
            Journal journal = new();
            journal.Clear();

            Gumps.SendAction(gumpID, direction);

            int safeCounter = 50; // 50 * 100 = 5000 ms
            while (safeCounter-- > 0)
            {
                bool success = journal.Search("successfully disarm");
                bool fail = journal.Search("fail to disarm the trap");
                if (success) return MoveResult.Disarmed;
                if (fail) return MoveResult.WrongTry;

                bool gump = Gumps.WaitForGump(gumpID, 100);
                if (gump) return MoveResult.ValidTry;
            }

            return MoveResult.SomethingWentWrong;
        }
        // Calculate the next direction to try
        private Dir NextDirection(int N, List<Dir> previousSteps, List<Dir> failedDirections)
        {
            int row = 0;
            int col = 0;

            // Calculates actual position in the matrix
            foreach (var step in previousSteps)
            {
                switch (step)
                {
                    case Dir.Up:
                        row--;
                        break;
                    case Dir.Right:
                        col++;
                        break;
                    case Dir.Down:
                        row++;
                        break;
                    case Dir.Left:
                        col--;
                        break;
                    default:
                        Misc.SendMessage("Something Wrong");
                        return Dir.Invalid;
                }

                // Verifica che la posizione sia all'interno della matrice
                if (row < 0 || row >= N || col < 0 || col >= N)
                {
                    Misc.SendMessage("La posizione è fuori dalla matrice.");
                    return Dir.Invalid;
                }
            }

            // Verify next possible steps
            List<Dir> possibleSteps = new List<Dir>();
            if (row > 0 && previousSteps.LastOrDefault() != Dir.Down) possibleSteps.Add(Dir.Up);
            if (row < N - 1 && previousSteps.LastOrDefault() != Dir.Up) possibleSteps.Add(Dir.Down);
            if (col > 0 && previousSteps.LastOrDefault() != Dir.Right) possibleSteps.Add(Dir.Left);
            if (col < N - 1 && previousSteps.LastOrDefault() != Dir.Left) possibleSteps.Add(Dir.Right);

            // Removes all alredy failed directions
            possibleSteps.RemoveAll(step => failedDirections.Contains(step));

            // Restituisce il primo passo valido, se presente
            if (possibleSteps.Count > 0)
            {
                if (possibleSteps.Count == 2)
                {
                    if (possibleSteps.Contains(Dir.Up) && possibleSteps.Contains(Dir.Down)) return Dir.Down;
                    if (possibleSteps.Contains(Dir.Left) && possibleSteps.Contains(Dir.Right)) return Dir.Right;
                }

                return CalculateBestNextDirection(N, previousSteps, possibleSteps);

                //return possibleSteps.First();
            }
            else
            {
                return Dir.Invalid;
            }
        }
        // Calculate the fitness of the path with known solutions
        // If only only solution is possible, also if the fitness is low, is not a problem because is the only one possible
        private static float CalculatePathFitness(int size, List<Dir> path, out List<Dir> pathSolution)
        {
            pathSolution = new List<Dir>();

            List<List<Dir>> available_solutions = new();
            if (size == 3) available_solutions = known_solutions_3x3;
            if (size == 4) available_solutions = known_solutions_4x4;
            if (size == 5) available_solutions = known_solutions_5x5;

            int cntMatch = 0;
            string strPath = DirectionListToString(path);

            foreach (List<Dir> solution in available_solutions)
            {
                string strSolution = DirectionListToString(solution);

                if (strSolution.StartsWith(strPath))
                {
                    cntMatch++;
                    pathSolution = solution;
                }
            }

            if (cntMatch == 0) return 0;
            if (cntMatch > 1) return 0;

            return (float)path.Count / pathSolution.Count * 100;
        }
        // Calculate the best next direction trying to match the actual path with known solutions
        // when the metch is found, suggests the next direction from that match if is possible
        private static Dir CalculateBestNextDirection(int size, List<Dir> path, List<Dir> possibleDirections)
        {
            if (path.Count < 2) return possibleDirections.First();

            List<List<Dir>> available_solutions;
            if (size == 3) available_solutions = known_solutions_3x3;
            else if (size == 4) available_solutions = known_solutions_4x4;
            else if (size == 5) available_solutions = known_solutions_5x5;
            else return Dir.Invalid;

            if (available_solutions.Count == 0) return possibleDirections.First();

            string strPath = DirectionListToString(path);

            List<Dir> pathSolution = new List<Dir>();

            foreach (List<Dir> solution in available_solutions)
            {
                string strSolution = DirectionListToString(solution);
                if (strSolution.StartsWith(strPath))
                {
                    pathSolution = solution;
                    break;
                }
            }

            Dir NextDir = pathSolution[path.Count - 1];
            if (possibleDirections.Contains(NextDir)) return NextDir;
            else return possibleDirections.First();
        }
        private void StoreOnFileTheSolution(int size, List<Dir> solution)
        {
            string stringToFile = "";
            foreach (Dir dir in solution)
            {
                stringToFile += DirectionToString(dir);
            }
            stringToFile = $"{size}:{stringToFile}";

            string data_folder = System.IO.Path.GetFullPath(System.IO.Path.Combine(Assistant.Engine.RootPath, "Data"));
            string path = new FileInfo(System.IO.Path.Combine(data_folder, "RemoveTraps.txt")).FullName;

            bool found = false;
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    if (line.Trim() == stringToFile.Trim())
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                using StreamWriter sw = File.AppendText(path);
                sw.WriteLine(stringToFile);
            }
        }
    }
}