namespace BottleColorSolver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Hi User, welcome to the bottle solver program. Hopefully with this cheat code you can always beat your girlfriend");

                UIStuff.DisplaySomeSpaces();
                BottleLevelState bottleLevelState = new BottleLevelState();

                bottleLevelState.AmountOfBottles = UIStuff.AskBottleCount();

                bottleLevelState.BottleCapacity = UIStuff.AskBottleCapacity();

                InquireAndFillTheBottles(bottleLevelState);

                bottleLevelState.MakeResponse();

                bottleLevelState.LogResponse();

                Console.WriteLine("press a key to start over");
                Console.ReadKey();
                Console.Clear();
            }
        }

        public static void InquireAndFillTheBottles(BottleLevelState bottleLevelState)
        {
            string input;
            for (int i = 0; i < bottleLevelState.AmountOfBottles; i++)
            {
                bottleLevelState.AddBottle(new Bottle(bottleLevelState.BottleCapacity));

                for (int j = 0; j < bottleLevelState.BottleCapacity; j++)
                {
                    input = UIStuff.AskBottleColorAtPosition(i + 1, j, bottleLevelState.BottleCapacity);
                    if (input.Equals(String.Empty))
                    {
                        bottleLevelState.PrintBottles();
                        break;
                    }
                    else
                    {
                        bottleLevelState.Bottles.ElementAt(i).PushColor(input);
                        bottleLevelState.PrintBottles();
                    }
                }
            }
        }
    }

    public class Bottle
    {
        public Stack<string> Colors { get; set; }
        public int BottleCapacity { get; set; }

        public Bottle(int capacity)
        {
            BottleCapacity = capacity;
            Colors = new Stack<string>();
        }

        public void PushColor(string color)
        {
            Colors.Push(color);
        }

        public string DeleteColorEntry()
        {
            if (Colors.Count > 0)
            {
                return Colors.Pop();
            }
            return String.Empty;
        }

        public string PeekTopColor()
        {
            if (Colors.Count > 0)
            {
                return Colors.Peek();
            }
            return String.Empty;
        }

        public int AmountOfColorsInBottle()
        {
            return Colors.Count;
        }

        public string LookForColorAtPosition(int index)
        {
            return Colors.ElementAtOrDefault(index);
        }

        public void PrintColorAtPosition(int index)
        {
            Console.WriteLine(LookForColorAtPosition(index));
        }

        public void PrintBottle()
        {
            foreach (string color in Colors)
            {
                Console.WriteLine(color);
            }
        }

        public string BottleStateInStringFormat()
        {
            string result = String.Empty;
            for (int i = 0; i < BottleCapacity; i++)
            {
                if (LookForColorAtPosition(i) == null)
                {
                    result += " ";
                }
                else
                {
                    result += LookForColorAtPosition(i);
                }
            }
            return result;
        }
    }

    public class BottleLevelState
    {

        public HashSet<string> VisitedBottleStates { get; set; }
        public List<(int, int)> Response { get; set; }

        public BottleLevelState()
        {
            Bottles = new List<Bottle>();
            VisitedBottleStates = new HashSet<string>();
            Response = new List<(int, int)>();
        }

        public List<Bottle> Bottles { get; set; }
        public int BottleCapacity { get; set; }
        public int AmountOfBottles { get; set; }



        public void DeleteBottle(Bottle bottle)
        {
            Bottles.Remove(bottle);
        }

        public void PrintBottles()
        {



            for (int bottleNr = 0; bottleNr < Bottles.Count(); bottleNr++)
            {


                PrintBottleAtNr(bottleNr);
                Console.WriteLine();
            }
        }

        public void PrintBottleAtNr(int bottleNr)
        {
            int printedNr = bottleNr + 1;
            Console.WriteLine("Bottle " + printedNr);
            Bottles.ElementAt(bottleNr).PrintBottle();
        }

        public void PrintBottles(List<Bottle> list)
        {
            for (int bottleNr = 0; bottleNr < Bottles.Count(); bottleNr++)
            {
                PrintBottleAtNr(bottleNr, list);
                Console.WriteLine();
            }
        }

        public void PrintBottleAtNr(int bottleNr, List<Bottle> list)
        {
            Console.WriteLine("Bottle " + bottleNr);
            list.ElementAt(bottleNr).PrintBottle();
        }

        public void LogResponse()
        {
            Random random = new Random();

            foreach ((int, int) response in Response)
            {
                int colorCode = random.Next(0, 16);
                ConsoleColor color = (ConsoleColor)colorCode;

                Console.ForegroundColor = color;
                Console.WriteLine("Move color from bottle " + (response.Item1 + 1) + " to bottle " + (response.Item2 + 1));
            }

            // Reset the console color to the default
            Console.ResetColor();
        }

        public void MakeResponse()
        {
            (int, int) tryMove;
            VisitedBottleStates.Add(StateInStringFormat());
            while (!IsFinished())
            {
                tryMove = TryGenerateBottleValidMove();
                if (tryMove.Item1 != -1) //  move further in the tree? attach it my Response and to visitedMoves
                {
                    Response.Add(tryMove);
                    VisitedBottleStates.Add(StateInStringFormat());
                }
                else
                {
                    VisitedBottleStates.Add(StateInStringFormat());
                    ExecuteMove(Bottles, Response.Last().Item2, Response.Last().Item1); /* no move to try from this point?, go back one move */
                    Response.RemoveAt(Response.Count - 1); // remove this move from the Response
                }
            }
            ExecuteFinishingMoves();
            Console.WriteLine("Response generated:");
            Console.WriteLine();
        }

        private bool IsFinished()
        {
            bool isFinished = true;
            foreach (Bottle bottle in Bottles)
            {
                if (bottle.PeekTopColor().Equals(String.Empty))
                {
                    continue;
                }
                foreach (string color in bottle.Colors)
                {
                    if (color != bottle.PeekTopColor())
                    {
                        isFinished = false;
                        break;
                    }
                }
            }
            return isFinished;
        }

        private (int, int) TryGenerateBottleValidMove()
        {
            int bottleNr = 0;

            foreach (Bottle bottle in Bottles) // loop over all bottles
            {
                if (bottle.Colors.Count > 0)
                {
                    for (int i = 0; i < Bottles.Count; i++) // loop over all bottles but the current bottle from the above loop is excluded in the if below
                    {
                        if (bottleNr == i)
                        {
                            continue;
                        }
                        if (ValidateMove(Bottles.ElementAt(bottleNr), Bottles.ElementAt(i)))
                        {
                            ExecuteMove(Bottles, bottleNr, i);
                            if (VisitedBottleStates.Contains(StateInStringFormat())) //  state already visited?,
                            {
                                ExecuteMove(Bottles, i, bottleNr); // Reverse the move 
                            }
                            else
                            {
                                return (bottleNr, i);
                            }
                        }
                    }
                    bottleNr++;
                }
            }
            return (-1, -1);
        }

        private void ExecuteMove(List<Bottle> bottles, int bottle1, int bottle2) // a color from bottle1 will be moved to bottle2
        {
            bottles.ElementAt(bottle2).PushColor(bottles.ElementAt(bottle1).DeleteColorEntry());
        }

        private bool ValidateMove(Bottle bottle1, Bottle bottle2) // bottle1 is trying to move a color from its bottle to bottle2
        {
            if ((bottle1.PeekTopColor().Equals(bottle2.PeekTopColor()) || bottle2.Colors.Count == 0) && bottle2.AmountOfColorsInBottle() < BottleCapacity)
            {
                return true;
            }

            return false;
        }

        public string StateInStringFormat()
        {
            string result = String.Empty;
            foreach (Bottle bottle in Bottles)
            {
                result += bottle.BottleStateInStringFormat();
            }
            return result;
        }

        private void ExecuteFinishingMoves()
        {
            int bottlenumber = 0;
            foreach (Bottle bottle in Bottles)
            {
                if (bottle.PeekTopColor().Equals(String.Empty) || bottle.AmountOfColorsInBottle() == BottleCapacity)
                {
                    continue;
                }
                for (int i = 0; i < Bottles.Count; i++)
                {
                    if (bottlenumber == i)
                    {
                        continue;
                    }
                    if (Bottles[bottlenumber].PeekTopColor() == Bottles[i].PeekTopColor())
                    {
                        while (Bottles[i].AmountOfColorsInBottle() != BottleCapacity)
                        {
                            ExecuteMove(Bottles, bottlenumber, i);
                            VisitedBottleStates.Add(StateInStringFormat());
                            Response.Add((bottlenumber, i));
                        }
                    }
                }
                bottlenumber++;
            }

        }
        public void AddBottle(Bottle bottle)
        {
            Bottles.Add(bottle);
        }
    }

    public static class UIStuff
    {



        public static void DisplaySomeSpaces()
        {

            Console.WriteLine();
            Console.WriteLine("*****");
            Console.WriteLine();
        }



        public static int AskBottleCount()
        {
            string input = GetBottleCountInput();
            (int value, bool isParsable) = VerifyInputParsability(input);

            while (!isParsable)
            {
                Console.WriteLine("Invalid input! Please enter a valid int (a number without commas).");
                (value, isParsable) = VerifyInputParsability(GetBottleCountInput());
            }

            Console.WriteLine("Valid input: " + value);
            Console.WriteLine();
            return value;
        }

        private static string GetBottleCountInput()
        {
            Console.WriteLine("How many bottles do you have in your level?");
            string Response = Console.ReadLine();
            return Response;
        }

        public static int AskBottleCapacity()
        {
            string input = GetBottleCapacityInput();
            (int value, bool isParsable) = VerifyInputParsability(input);

            while (!isParsable)
            {
                Console.WriteLine("Please");
                (value, isParsable) = VerifyInputParsability(GetBottleCapacityInput());
            }

            Console.WriteLine("Valid input: " + value);
            Console.WriteLine();
            return value;
        }

        private static string GetBottleCapacityInput()
        {
            Console.WriteLine("Please enter the bottle capacity (number of color entries per bottle):");

            Console.WriteLine();
            string input = Console.ReadLine();
            return input;
        }


        private static (int, bool) VerifyInputParsability(string input)
        {
            int value;
            bool isParsable = int.TryParse(input, out value);
            return (value, isParsable);
        }
        public static string AskBottleColorAtPosition(int bottleNumber, int position, int maxPosition)
        {
            Console.WriteLine("Enter the color at position " + position + " (floor = 0, ceiling = " + maxPosition + ") for bottle " + bottleNumber + ":");
            Console.WriteLine("No color at this position? Press any key if there is none");
            string input = Console.ReadLine();
            Console.WriteLine();
            return input;
        }
    }
}
