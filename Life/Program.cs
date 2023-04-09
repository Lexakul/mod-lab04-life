using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Text.Json;

namespace cli_life
{
    public class Cell // представление ячейки
    {
        public bool IsAlive; //жива не жива
        public readonly List<Cell> neighbors = new List<Cell>(); // список из соседей чтобы работать с окрестностью
        public bool IsAliveNext; // что произойдет с клеткой на след шаге (сост в след момент)
        public void DetermineNextLiveState() // правила клетки
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count(); //просмотр соседей живые или нет
            if (IsAlive) // клетка живая в след сост
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext; // расчитанное состояние в текущее
        }
    }
    public class Board //представление решетки
    {
        public readonly Cell[,] Cells; // двумерный массив
        public readonly int CellSize; // задает кол-во пикселей для отображения 1 клетки

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1) // liveDensity = .1 1 десятая от общего кол-ва клеток в начале
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize]; // создается массив пустой а потом прикрепляются ячейки
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors(); // соединяет всех соседей чтобы клетка знала соседей
            Randomize(liveDensity); // начальная расстановка рандом
        }

        readonly Random rand = new Random(); // выполнение рандомизации
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState(); // определяем состояние
            foreach (var cell in Cells)
                cell.Advance(); // переделывание состояние в текущее
        }
        private void ConnectNeighbors() // связывание ячеек
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }

        public static readonly char[,] glider = {
            { '0', '0', '1' },
            { '1', '0', '1' },
            { '0', '1', '1' }
        };

        public List<(int x, int y)> FindFigure(char[,] figure) // он принимает двумерный массив (маску чтобы найти фигуру) например glider
        {
            var positions = new List<(int x, int y)>(); // будем добавлять сюда найденные фигуры (их позиции)
            for (int x = 0; x < Columns - figure.GetLength(0); x++) // ходим по доске
            {
                for (int y = 0; y < Rows - figure.GetLength(1); y++)
                {
                    bool find = true; // если нашли то true
                    for (int i = 0; i < figure.GetLength(0); i++) // ходим по столбцам и строкам glider чтобы ее найти
                    {
                        for (int j = 0; j < figure.GetLength(1); j++)
                        {
                            if (figure[i, j] == '1' && !Cells[x + i, y + j].IsAlive || figure[i, j] == '0' && Cells[x + i, y + j].IsAlive) // проверяем соответствуют элементы figure элементам на Board
                            {
                                find = false;
                                break;
                            }
                        }
                        if (!find)
                        {
                           break; 
                        }
                    }
                    if (find)
                    {
                        positions.Add((x, y)); // добавляем позицию если нашли
                    }
                }
            }
            return positions;
        }

        public int Live()
        {
            return Cells.Cast<Cell>().Count(cell => cell.IsAlive); 
        }
        public int Sym()
        {
            int symcount = 0;
            for(int x = 0; x < Columns;x++)
            {
                for(int y = 0; y < Rows;y++)
                {
                    
                    if (Cells[x,y].IsAlive && Cells[Columns-1-x,Rows -1 -y].IsAlive)
                    {
                        symcount++;
                    }
                }
            }
            return symcount;
        }
        public static Board SettingsJson()
        {
            List<BoardJsonSet> item;
            using (StreamReader r = new StreamReader("imput.json"))
            {
                string json = r.ReadToEnd();
                item = JsonConvert.DeserializeObject<List<BoardJsonSet>>(json);
            }
            return new Board(item[0].Width, item[0].Height, item[0].CellSize, item[0].LiveDensity);
        }
    }

    public class BoardJsonSet
    {
        public int Width{get; set; }
        public int Height{get; set; }
        public int CellSize {get; set; }
        public double LiveDensity {get; set; }
    }

    public class Program //класс консольного приложения
    {
        static Board board;
        static public void Reset() // очистить и создать заного 
        {
           
            board = new Board( // массив 50 на 20
                width: 50,
                height: 20,
                cellSize: 1,
                liveDensity: 0.5);
        }
        static public void Render() // метод отрисовки
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)   
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        
        }

        public static void savejsonboard()
        {
            string path = "BoardSet.json";
            File.WriteAllText(path, string.Empty);
            int[,] data = new int[board.Columns, board.Rows];
            
            for (int row = 0; row < board.Rows;row++)
            {
                for(int col=0; col < board.Columns;col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        data[col,row] = 1; 
                    }
                    else
                    {
                        data[col,row] = 0;
                    }
                }
            }
            string jsonString = JsonConvert.SerializeObject(data);
            using (StreamWriter streamWriter = File.CreateText("BoardSet.json"))
            {
               streamWriter.Write(jsonString);
            }

        }

        public static int genstatictime(int lim)
        {
            int count = 0;
            bool st = true;
            int[,] data = new int[board.Columns, board.Rows];
            while(count < lim )
            {
                st = true;
                for(int row = 0;row < board.Rows;row++)
                {
                    for(int col = 0;col < board.Columns;col++)
                    {
                        var cell = board.Cells[col,row];
                        if(cell.IsAlive)
                        {
                            data[col,row] = 1;
                        }
                        else
                        {
                            data[col,row] = 0;
                        }
                    }
                }
                board.Advance();
                for(int row = 0;row < board.Rows;row++)
                {
                    for(int col = 0;col < board.Columns;col++)
                    {
                        var cell = board.Cells[col,row];
                        if(cell.IsAlive)
                        {
                            if(data[col,row] == 1)
                            {
                                
                            }
                            else
                            {
                                st = false;
                            }
                        }
                        else
                        {
                            if(data[col,row] == 0)
                            {
                                
                            }
                            else
                            {
                                st = false;
                            }
                        }
                    }
                }
                if(st == true)
                {
                    return count;
                }
                else
                {
                    count++;

                }

            }
            return count;
        }
        public static void BoardSetLoad()
        {
            string jsonString = File.ReadAllText("BoardSet.json"); // чтение JSON-строки из файла
            int[,] array = JsonConvert.DeserializeObject<int[,]>(jsonString);
            for (int row = 0; row < board.Rows;row++)
            {
                for(int col=0; col < board.Columns;col++)
                {
                    var cell = board.Cells[col, row];
                    if (array[col,row] == 1)
                    {
                        cell.IsAlive = true; 
                    }
                    else
                    {
                        cell.IsAlive = false;
                    }
                }
            }

        }
        
        static void Main(string[] args)
        {
            
            Console.WriteLine("select an item \n 1 - start program \n 2 - read settings file \n 3 - load Board \n press button S during generation to save the state of the board");
            int yu = Convert.ToInt32(Console.ReadLine());
            switch(yu)
            {
            case 1:
                Console.Clear();
                Reset();
                Console.WriteLine(board);
                break;
            case 2:
                Console.Clear();
                board = Board.SettingsJson();
                Console.WriteLine(board);
                break ;
            case 3:
                Console.Clear();
                Reset();
                BoardSetLoad();
                Console.WriteLine(board);
                break;
            }
            ConsoleKeyInfo save;
            while(true)
            {
                Console.Clear();
                Render();
                Console.Write("Live ");
                Console.Write(board.Live());
                Console.Write("\nSym ");
                Console.Write(board.Sym());
                if(Console.KeyAvailable == true)
                {
                save = Console.ReadKey();
                if(save.Key == ConsoleKey.Q)  // выход
                {
                    break;
                }
                if (save.Key == ConsoleKey.S)
                {
                    Console.Write("board save ");
                    savejsonboard();
                }
                }
                board.Advance(); // переход к след поколению
                Thread.Sleep(1000); // задержка секундная

            }
            
        }
    }
}