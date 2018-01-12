using System;
using System.Collections;
using System.Collections.Generic;

namespace Draught
{
    class Program
    {
        static void Main(string[] args)
        {   /*In the board 1 means First player figures and 9 stands for First PLayer King figure(damke)*/
            /*Vice versa with Player 2 - its figures 2 and its King figure 4 */
            Game Draught = new Game(new Board(8, 12)); /*This helps to make it all flexible */
             /*The first parameter - how wide the borad is, ant the secong parameter how many figures are we going have */
            Draught.PlayGame();
        }
    }
    public class Board
    {
        public byte Width { get; private set; }
        public byte Figure { get; private set; }
        private int[,] BoardValue = null; /*the values of every board's  square*/
        public byte WhiteFigures { get; private set; } = 0;// - 1
        public byte BlackFigures { get; private set; } = 0;// - 2
        public byte ChangePlayer { get; set; } = 0;
        public int LayMovement { get; private set; } = 0;

        private IList<Tuple<int, int>> Matches = new List<Tuple<int, int>>(); /*Fill with the square coordinates */

        public int RecursionKing { get; set; } = 0;

        public bool KingMust = false;

        public int CrossCount { get; private set; } = 0;

        public Board(byte size, byte figure)
        {
            Width = size;
            Figure = figure;
            FillSquareInitialize(BoardValue, Width); /*Filling the board with figures */
        }

        public int IfKing(int Player, int row)
        {
            if (Player == 1 && row == Width) { return 9; }
            if (Player == 2 && row == 1) { return 4; }
            return Player;
        }

        private void FillSquareInitialize(int[,] Board, byte Width)  /*BLACK - 2, WHITE - 1 */
        {
            BoardValue = new int[Width + 2, Width + 2];
            bool Has = false;
            for (var i = 1; i <= Width; i++)
            {
                for (var j = 1; j <= Width; j++)
                {
                    if (WhiteFigures == Figure)
                    {
                        Has = true;
                        break;
                    }
                    BoardValue[i, j] = (i + j) % 2 == 0 ? 1 : 0;
                    if (BoardValue[i, j] == 1) { WhiteFigures++; }
                }
                if (Has == true) { Has = false; break; }
            }

            for (var i = Width; i >= 1; i--)
            {
                for (var j = Width; j >= 1; j--)
                {
                    if (BlackFigures == Figure)
                    {
                        Has = true;
                        break;
                    }
                    BoardValue[i, j] = (i + j) % 2 == 0 ? 2 : 0;
                    if (BoardValue[i, j] == 2) { BlackFigures++; }
                }
                if (Has == true) { break; }
            }

            for (var i = 1; i <= Width; i++)
            {
                BoardValue[i, 0] = -1;
                BoardValue[0, i] = -1;
                BoardValue[Width+1, i] = -1;
            }
            BoardValue[6, 6] = 9;
        }
        private void CheckIfGameIsOver()
        {
            if (BlackFigures == 0 || WhiteFigures == 0)
            {
                ShowSquares();
                int Player = BlackFigures != 0 ? 2 : 1;
                Console.WriteLine("The Game is Over, Player {0} won the game!", Player);
                Environment.Exit(0);
            }
        }
        public bool CheckIfIsItAllowedToMove(int FigureNumber, Tuple<int, int> From, Tuple<int, int> To)
        {
            if (CheckForcedCross(FigureNumber)) { return false; }
            if (KingCrossing(From, To, BoardValue[From.Item1, From.Item2])) { return true; }
            if (To.Item1 > Width || To.Item2 > Width || To.Item1 < 1 || To.Item2 < 1) { return false; }

            if (BoardValue[From.Item1, From.Item2] == FigureNumber)
            {
                if (Math.Abs(From.Item1 - To.Item1) == 2)
                {
                    CrossingFigure(FigureNumber, From, To, 0);
                }
                if ((From.Item1 + 1 == To.Item1 && From.Item2 + 1 == To.Item2 ||
                   From.Item1 + 1 == To.Item1 && From.Item2 - 1 == To.Item2)
                   && BoardValue[To.Item1, To.Item2] == 0 && FigureNumber == 1)
                {
                    BoardValue[From.Item1, From.Item2] = 0;
                    BoardValue[To.Item1, To.Item2] = IfKing(FigureNumber, To.Item1);
                    return true;
                }
                if ((From.Item1 - 1 == To.Item1 && From.Item2 - 1 == To.Item2 ||
                 From.Item1 - 1 == To.Item1 && From.Item2 + 1 == To.Item2)
                 && BoardValue[To.Item1, To.Item2] == 0 && FigureNumber == 2)
                {
                    BoardValue[From.Item1, From.Item2] = 0;
                    BoardValue[To.Item1, To.Item2] = IfKing(FigureNumber, To.Item1);
                    return true;
                }
                return false;
            }
            return false;
        }
        private bool CheckForcedCross(int FigureNumber) /*If there are forced cross, for intstance you allways have to cross the square */
        {                                               /* if there is a movement with who you can to get an opponent's figure*/
            for (var i = 1; i <= Width; i++)
            {
                for (var j = 1; j <= Width; j++)
                {
                    if (BoardValue[i, j] == 4 || BoardValue[i, j] == 9
                        || BoardValue[i, j] == 0
                        || BoardValue[i, j] != FigureNumber) { continue; }
                    if (CrossingFigure(FigureNumber, new Tuple<int, int>(i, j), new Tuple<int, int>(i + 2, j + 2), 1)) { return true; }
                    if (CrossingFigure(FigureNumber, new Tuple<int, int>(i, j), new Tuple<int, int>(i - 2, j + 2), 1)) { return true; }
                    if (CrossingFigure(FigureNumber, new Tuple<int, int>(i, j), new Tuple<int, int>(i + 2, j - 2), 1)) { return true; }
                    if (CrossingFigure(FigureNumber, new Tuple<int, int>(i, j), new Tuple<int, int>(i - 2, j - 2), 1)) { return true; }
                }
            }
            return false;
        }

        public bool CrossingFigure(int FigureNumber, Tuple<int, int> From, Tuple<int, int> To, int End) /* 0 - for putting, 1 - for cheking if there is forced cross*/
        {
            var Number = FigureNumber == 1 ? 9 : 4; /*Check a movement with having simple figures, not kings */
            if (BoardValue[From.Item1, From.Item2] == 4 || BoardValue[From.Item1, From.Item2] == 9) { return false; }
            if (From.Item1 == 0 || From.Item2 == 0) { return false; }
            if (From.Item1 + 2 == To.Item1 || From.Item1 - 2 == To.Item1)
                for (var i = -1; i <= 1; i++)
                {
                    for (var j = -1; j <= 1; j++)
                    {
                        if ((From.Item1 + i < Width + 1 && From.Item1 + i >= 1) && (From.Item2 + j < Width + 1 && From.Item2 + j >= 1))
                        {
                            if (BoardValue[From.Item1 + i, From.Item2 + j] != FigureNumber
                               && BoardValue[From.Item1 + i, From.Item2 + j] != 0
                                && BoardValue[From.Item1 + i, From.Item2 + j] != -1
                               && BoardValue[From.Item1 + (i * 2), From.Item2 + (j * 2)] == 0)
                            {
                                if (CrossCount == 0)/*The Counter time crossing a figure*/
                                {
                                    if (BoardValue[From.Item1 + i, From.Item2 + j] == Number) { return false; }
                                    if (End == 1) { return true; }
                                    IfKing(FigureNumber, From.Item1 + (i * 2));
                                    BoardValue[From.Item1 + (i * 2), From.Item2 + (j * 2)] = IfKing(FigureNumber, From.Item1 + (i * 2));
                                    BoardValue[From.Item1, From.Item2] = 0;
                                    BoardValue[From.Item1 + i, From.Item2 + j] = 0;

                                    if (FigureNumber == 1 || FigureNumber == 9) { BlackFigures--; }
                                    else { WhiteFigures--; }

                                    CheckIfGameIsOver(); /*If the game is not over yet */

                                    CrossCount++;
                                    if (CrossCount > 1) { return false; }
                                    for (int d = 1; d <= Width; d++) /*CHECKING IF THERE ARE MORE CROSSINGS */
                                    {
                                        if (CrossingFigure(FigureNumber, To, new Tuple<int, int>(To.Item1 + 2, d), 0)
                                             || CrossingFigure(FigureNumber, To, new Tuple<int, int>(To.Item1 + (-2), d), 0))
                                        {
                                            InsertNewMove(FigureNumber, To);
                                            CrossCount = 0;
                                            break;
                                        }
                                    }
                                    CrossCount = 0;
                                }
                                else
                                {
                                    if (End == 1) { return false; }
                                    return true;
                                }
                                return true;
                            }
                        }
                    }
                }
            return false;
        }

        private bool KingCrossing(Tuple<int, int> From, Tuple<int, int> To, int King) /*See which squares is possible to move having a king(damke) */
        {
            if (!(BoardValue[From.Item1, From.Item2] == 4 || BoardValue[From.Item1, From.Item2] == 9))
            {
                return false; /*not a king */
            }
            if (!KingRecursion(From, To, King, 1))
            {
                return false;
            }
            if (LayMovement == 1) { LayMovement = 0; return true; }

            IList<Tuple<int, int>> Matches1 = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(1,1),
                new Tuple<int, int>(-1,1),
                new Tuple<int, int>(1,-1),
                new Tuple<int, int>(-1,-1)
            };
            foreach (var tuple in Matches1)
            {
                int Counter = 1;

                while (true)
                {
                    if (To.Item1 + (tuple.Item1 * Counter) < 1 || To.Item1 + (tuple.Item2 * Counter) < 1
                        || To.Item1 + (tuple.Item1 * Counter) > Width
                        || To.Item1 + (tuple.Item2 * Counter) > Width) { break; }
                    if (KingRecursion(To, new Tuple<int, int>(To.Item1 + (tuple.Item1 * Counter), To.Item1 + (tuple.Item2 * Counter)), King, 0))
                    {
                        ChangePlayer = 1;
                        return true;
                    }
                    Counter++;
                }
            }
            return true;
        }
        /*Needed var helps to indetify if what it has to do
         for instance Needed = 1, when if we want to cross the figure, we gonna make this action
         Needed = 0, when we checking if there are any situation for crossing an oppoenent's figure - if yes - we stop the function
         and user now must to input that coordinates. Similiar goes for cheking opportunities for crossing with not King figure
             */
        private bool KingRecursion(Tuple<int, int> from, Tuple<int, int> to, int King, int Needed) 
        {
            if (BoardValue[to.Item1, to.Item2] != 0) { return false; }
            if (to.Item1 > Width || to.Item2 > Width) { return false; }
            var count = Math.Abs(from.Item1 - to.Item1);
            int X = to.Item2 - from.Item2 < 0 ? -1 : 1;
            int Y = to.Item1 - from.Item1 < 0 ? -1 : 1;
            var BasicValue = King == 9 ? 1 : 2;
            bool EmptySquare = false;
            for (var a = 1; a < count; a++)
            {
                if (EmptySquare == true)
                {
                    if (BoardValue[from.Item1 + ((a + 1) * X), from.Item2 + ((a + 1) * Y)] != 0) { RecursionKing = 1; break; }
                }
                else
                {
                    if (from.Item1 + (a * X) < 1 || from.Item2 + (a * Y) < 1
                        || from.Item1 + (a * Y) > Width
                        || from.Item2 + (a * X) > Width) {
                        
                        return false;
                    }
                    if (BoardValue[from.Item1 + (a * Y), from.Item2 + (a * X)] == King
                        || BoardValue[from.Item1 + (a * Y), from.Item2 + (a * X)] == BasicValue) {/* RecursionKing = 1; break;*/ return false; }
                    if (BoardValue[from.Item1 + (a * Y), from.Item2 + (a * X)] != King
                        && BoardValue[from.Item1 + (a * Y), from.Item2 + (a * X)] != 0
                        && BoardValue[from.Item1 + (a * Y), from.Item2 + (a * X)] != BasicValue
                        && BoardValue[from.Item1 + ((a + 1) * Y), from.Item2 + ((a + 1) * X)] == 0)
                    {
                        Matches.Add(new Tuple<int, int>(from.Item1 + (a * Y), from.Item2 + (a * X)));
                        a++;
                        EmptySquare = true;
                    }
                }
               
            }
            

            if (EmptySquare == false && Needed == 0) { return false; }
            if (RecursionKing == 0)
            {
                if (Needed == 1)
                {
                    BoardValue[from.Item1, from.Item2] = 0;
                    BoardValue[to.Item1, to.Item2] = King;
                    LayMovement = 1;
                    foreach (var a in Matches)
                    {

                        BoardValue[a.Item1, a.Item2] = 0;
                        LayMovement = 0;
                        if (King == 9)
                        {
                            BlackFigures--;
                        }
                        if (King == 4)
                        {
                            WhiteFigures--;
                        }
                    }
                    Matches.Clear();
                    CheckIfGameIsOver();
                }
                return true;
            }
            RecursionKing = 0;
            return false;
        }

        public bool InsertNewMove(int FigureNumber, Tuple<int, int> To)
        {
            while (true) /*Check if there are probabilities or possibilities to cross the square who has opponent figure */
            {
                Console.WriteLine("---------" + FigureNumber + "Player  turn again------/n");
                ShowSquares();

                Console.WriteLine("Where to move?");
                var NextMove = Game.GetNumbers(Convert.ToInt32(Console.ReadLine()));

                CrossCount = 0;
                if (CrossingFigure(FigureNumber, To, NextMove, 0)) { return true; }
            }
        }
        public void ShowSquares()
        {
            Console.WriteLine("Remain " + BlackFigures + " BLACK(2) ######");
            Console.WriteLine("Remain " + WhiteFigures + " WHITE(1) ######");
            Console.WriteLine("##############################");
            Console.Write("  ");
            for (var i = 1; i <= Width; i++) { Console.Write(i + " "); };
            Console.WriteLine("\n__________________");
            for (var i = Width; i >= 1; i--)
            {
                Console.Write(i + "|");
                for (var j = 1; j <= Width; j++)
                {
                    Console.Write(BoardValue[i, j] + " ");
                }
                Console.WriteLine("|" + i);
            }
            Console.WriteLine("__________________");
            Console.Write("  ");
            for (var i = 1; i <= Width; i++) { Console.Write(i + " "); };
            Console.WriteLine();
            Console.WriteLine("##############################");
        }
    }
    public class Game
    {
        public Board Board { get; private set; }
        public Game(Board Board)
        {
            this.Board = Board;
        }

        public static Tuple<int, int> GetNumbers(int Number)
        {
            int First = Number / 10;
            int Second = Number - (First * 10);
            return new Tuple<int, int>(First, Second);
        }

        private void MoveFigure(int from, int to) /*Get coordinates of a square from where to where */
        {
            var fromLocation = GetNumbers(from);
            var toLocation = GetNumbers(to);
        }

        private bool CheckIfIfExist(int nubmer)
        {
            return true;
        }
        public void PlayGame()
        {
            bool State = true;
            while (State)
            {
                Console.WriteLine("LET'S START A NEW GAME! Please insert 444 to start/EXIT - 555");
                try
                {
                    int Start = Convert.ToInt32(Console.ReadLine());
                    if (Start == 555)
                    {
                        Environment.Exit(0);
                    }
                    else if (Start == 444)
                    {
                        State = false;
                    }
                    continue;

                }
                catch (Exception)
                {
                    Console.WriteLine("Please insert numbers!");
                    continue;
                }
            }
            Console.WriteLine("Lets Begin the Game hahaha");
            int Player = 1;
            while (true)
            {
                Board.ShowSquares();
                Console.WriteLine("##### Player" + Player + " #####\nWhich figure to move");
                int FromCoordinates;
                try
                {
                    FromCoordinates = Convert.ToInt32(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("Please insert numbers!");
                    continue;
                }

                var fromTuple = GetNumbers(FromCoordinates);

                Console.WriteLine("Where to move?");
                int ToCoordinates;
                try
                {
                    ToCoordinates = Convert.ToInt32(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("Please insert numbers!");
                    continue;
                }

                var toTuple = GetNumbers(ToCoordinates);


                if (!Board.CrossingFigure(Player, fromTuple, toTuple, 0)) /*if not true - there are no ways of crossing any figure */
                { /*moving to check other directions */
                    if (Board.CheckIfIsItAllowedToMove(Player, fromTuple, toTuple)) { Console.WriteLine("Success"); }
                    else
                    {
                        Console.WriteLine("Wrong coordinates");
                        Player = Player == 1 ? 2 : 1;
                    }
                }
                else { Console.WriteLine("You just thrown out an opponent's figure"); }
                if (Board.ChangePlayer == 1) { Player = Player == 1 ? 2 : 1; Board.ChangePlayer = 0; }
                Player = Player == 1 ? 2 : 1; /* Channge player it the previous has done with his movement*/
            }
        }
    }
}
