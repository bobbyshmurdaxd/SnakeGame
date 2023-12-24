using System.Diagnostics;
using System.Timers;

namespace Snake
{
    public record Position
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }

    public class State
    {
        public State(int rows, int cols, int foodCount = 1)
        {
            _rand = new Random();
            _rows = rows;
            _cols = cols;
            _foodCount = foodCount;
            _gameOver = false;
            _crashBorder = false;
            _crashSnake = false;
            var head = GenRandomPos();
            _snake.Add(head);
            _directionVert = 0;
            _directionHorz = head.Col > _cols / 2 ? -1 : 1;
            AddFood();
            _eating = 0;
        }

        private Position GenRandomPos()
        {
            return new Position { Row = _rand.Next(0, _rows), Col = _rand.Next(0, _cols) };
        }

        private void AddFood()
        {
            while (_food.Count < _foodCount)
            {
                var pos = GenRandomPos();
                if (_snake.Contains(pos) == false && _food.Contains(pos) == false)
                    _food.Add(pos);
            }
        }

        public void UpdateDirection(ConsoleKey input)
        {
            switch (input)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    _directionHorz = 0;
                    _directionVert = -1;
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    _directionHorz = 0;
                    _directionVert = 1;
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    _directionHorz = -1;
                    _directionVert = 0;
                    break;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    _directionHorz = 1;
                    _directionVert = 0;
                    break;
            }
        }

        public void Move()
        {
            if (_gameOver == false)
            {
                _prevSnake = _snake.ToList();
                var head = _snake.Last();
                var pos = new Position();
                pos.Row += head.Row + _directionVert;
                pos.Col += head.Col + _directionHorz;
                _snake.Add(pos);
                if (_eating == 0)
                    _snake.RemoveAt(0);
                else
                    _eating--;
            }
        }


        private bool SnakeCrashed()
        {
            var head = _snake.Last();
            // check if snake is out of boundaries
            if (head.Col < 0 || head.Col >= _cols || head.Row < 0 || head.Row >= _rows)
            {
                _crashBorder = true;
                return true;
            }
            // check if snake crashed into itself
            for (int i = 0; i < _snake.Count - 2; i++)
            {
                if (head == _snake[i])
                {
                    _crashSnake = true;
                    return true;
                }
            }
            return false;
        }

        public bool Evaluate()
        {
            if (SnakeCrashed() == true)
            {
                _gameOver = true;
                return false;
            }
            var head = _snake.Last();
            int index = _food.IndexOf(head);
            if (index != -1)
            {
                _food.RemoveAt(index);
                AddFood();
                _score++;
                _eating = 2;
            }
            return true;
        }

        public void Render(bool crashed = false)
        {
            if (crashed)
                _snake = _prevSnake.ToList();
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            Console.WriteLine("Score = " + _score);
            Console.ForegroundColor = _crashBorder ? ConsoleColor.Red : ConsoleColor.White;
            Console.WriteLine('╔' + new String('═', _cols) + '╗');
            var pos = new Position();
            for (pos.Row = 0; pos.Row < _rows; pos.Row++)
            {
                Console.Write('║');
                for (pos.Col = 0; pos.Col < _cols; pos.Col++)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    if (_snake.Contains(pos))
                    {
                        Console.ForegroundColor = _crashSnake ? ConsoleColor.Red : ConsoleColor.Green;
                        Console.Write('█');
                    }
                    else if (_food.Contains(pos))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write('■');
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write('-');
                    }
                }
                Console.ForegroundColor = _crashBorder ? ConsoleColor.Red : ConsoleColor.White;
                Console.WriteLine("║");
            }
            Console.WriteLine("╚" + new String('═', _cols) + "╝");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private Random _rand;
        private int _rows;
        private int _cols;
        private int _foodCount;
        private int _directionVert;
        private int _directionHorz;
        private int _eating;
        private bool _gameOver;
        private List<Position> _snake = new List<Position>();
        private List<Position> _food = new List<Position>();
        private List<Position> _prevSnake = new List<Position>();
        private bool _crashBorder;
        private bool _crashSnake;
        private int _score;

        public bool GameDone { get => _gameOver; }
        public int Score { get => _score; }
    }

    class Game
    {
        public Game(int rows, int cols)
        {
            var state = new State(rows, cols, rows / 4);
            state.Render();
            const long interval = 150;
            var sw = new Stopwatch();
            while (state.GameDone == false)
            {
                sw.Restart();
                if (Console.KeyAvailable)
                {
                    var keyinfo = Console.ReadKey(true);
                    state.UpdateDirection(keyinfo.Key);
                }
                state.Move();
                if (state.Evaluate())
                {
                    state.Render();
                    sw.Stop();
                    if (interval > sw.ElapsedMilliseconds)
                        System.Threading.Thread.Sleep((int)(interval - sw.ElapsedMilliseconds));
                }
            }
            state.Render(true);
            _score = state.Score;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e, State state)
        {
            lock (state)
            {
                if (Console.KeyAvailable)
                {
                    var keyinfo = Console.ReadKey(true);
                    state.UpdateDirection(keyinfo.Key);
                }
                state.Move();
                if (state.Evaluate())
                    state.Render();
            }
        }

        private int _score;

        public int Score { get => _score; }


    }

    class Program
    {
        static int Introduction(bool start)
        {
            if (start)
                Console.WriteLine("Welcome to Snake#\n");
            Console.WriteLine("Control your snake by using the arrow keys or AWSD. Press one of the following to start or Q to quit:");
            Console.WriteLine("1: Small Grid");
            Console.WriteLine("2: Medium grid");
            Console.WriteLine("3: Large grid");
            Console.CursorVisible = true;
            for (; ; )
            {
                var keyinfo = Console.ReadKey(true);
                switch (keyinfo.Key)
                {
                    case ConsoleKey.D1:
                        return 10;
                    case ConsoleKey.D2:
                        return 15;
                    case ConsoleKey.D3:
                        return 19;
                    case ConsoleKey.Q:
                        return -1;
                }
            }
        }

        static void Main()
        {
            bool firstGame = true;
            int highScore = 0;

            for (; ; )
            {
                int input = Introduction(firstGame);
                if (input < 0)
                    return;

                Console.Clear();
                var game = new Game(input, input * 2);
                var score = game.Score;

                if (highScore < score)
                    highScore = score;

                Console.WriteLine(String.Format("\nGame Over! High score = {0}\n", highScore));
                firstGame = false;
            }
        }
    }
}