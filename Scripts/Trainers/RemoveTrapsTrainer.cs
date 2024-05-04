using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

//#forcedebug
namespace RazorEnhanced
{
    internal class RemoveTrapsTrainer
    {
        private enum Direction
        {
            Invalid = 0,
            Up = 1,
            Right = 2,
            Down = 3,
            Left = 4
        }
        private string DirectionToString(Direction direction)
        {
            return direction switch
            {
                Direction.Up => "↑",
                Direction.Right => "→",
                Direction.Down => "↓",
                Direction.Left => "←",
                _ => "Unknown",
            };
        }

        private enum MoveResult
        {
            Disarmed = 0,
            ValidTry = 2,
            WrongTry = 1,
            SomethingWentWrong = 3
        }

        public RemoveTrapsTrainer()
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

                Misc.SendMessage($"============================================", 33);
                Misc.SendMessage($"Solved Trap #{++counter:D4} in {trap_elapsed:D3} seconds", 33);
                Misc.SendMessage($"Total training session elapsed {training_elapsed:D4} seconds", 33);
                Misc.SendMessage($"Average trap resolution time: {((float)training_elapsed / (float)counter):000.0} seconds", 33);
                Misc.SendMessage($"============================================", 33);
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
            List<Direction> path = new();
            List<Direction> failedDirections = new();

            MoveResult attemp = MoveResult.ValidTry; // First try is always valid
            Direction TryDirection;

            int SafeCounter = 0;
            while (SafeCounter++ < 50)
            {
                // First: I repeat all the previous moves if needed
                if (attemp == MoveResult.WrongTry)
                {
                    foreach (Direction step in path)
                    {
                        var check = MoveTo(gumpID, (int)step);
                        if (check == MoveResult.WrongTry || check == MoveResult.SomethingWentWrong) return false; // Something went wrong
                    }
                }

                TryDirection = NextDirection(size, path, failedDirections);
                attemp = MoveTo(gumpID, (int)TryDirection);

                switch (attemp)
                {
                    case MoveResult.Disarmed:
                        path.Add(TryDirection);
                        StoreOnFileTheSolution(size, path);
                        return true;
                    case MoveResult.WrongTry:
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

                string pathString = "";
                foreach (Direction dir in path)
                {
                    pathString += DirectionToString(dir);
                }
                Misc.SendMessage($"Attempt #{SafeCounter}", 33);
                Misc.SendMessage($"Tried direction: {DirectionToString(TryDirection)}", 55);
                Misc.SendMessage($"Discovered Path: {pathString}", 33);
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
        private Direction NextDirection(int N, List<Direction> previousSteps, List<Direction> failedDirections)
        {
            int row = 0;
            int col = 0;

            // Calculates actual position in the matrix
            foreach (var step in previousSteps)
            {
                switch (step)
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
                    default:
                        Misc.SendMessage("Something Wrong");
                        return Direction.Invalid;
                }

                // Verifica che la posizione sia all'interno della matrice
                if (row < 0 || row >= N || col < 0 || col >= N)
                {
                    Misc.SendMessage("La posizione è fuori dalla matrice.");
                    return Direction.Invalid;
                }
            }

            // Verify next possible steps
            List<Direction> possibleSteps = new List<Direction>();
            if (row > 0 && previousSteps.LastOrDefault() != Direction.Down) possibleSteps.Add(Direction.Up);
            if (row < N - 1 && previousSteps.LastOrDefault() != Direction.Up) possibleSteps.Add(Direction.Down);
            if (col > 0 && previousSteps.LastOrDefault() != Direction.Right) possibleSteps.Add(Direction.Left);
            if (col < N - 1 && previousSteps.LastOrDefault() != Direction.Left) possibleSteps.Add(Direction.Right);

            // Removes all alredy failed directions
            possibleSteps.RemoveAll(step => failedDirections.Contains(step));

            // Restituisce il primo passo valido, se presente
            if (possibleSteps.Count > 0)
            {
                if (possibleSteps.Count == 2)
                {
                    if (possibleSteps.Contains(Direction.Up) && possibleSteps.Contains(Direction.Down)) return Direction.Down;
                    if (possibleSteps.Contains(Direction.Left) && possibleSteps.Contains(Direction.Right)) return Direction.Right;
                }
                return possibleSteps.First();
            }
            else
            {
                return Direction.Invalid;
            }
        }
        private void StoreOnFileTheSolution(int size, List<Direction> solution)
        {
            string stringToFile = "";
            foreach (Direction dir in solution)
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