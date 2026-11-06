using System.Collections;
using System.Diagnostics;


Char[] moves = { 's', 'd', 'w', 'a' };
(int, int)[] diffculties = {(11, 11), (11,21), (101,11), (101,101), (521,101) };
Console.CursorVisible = false;
Console.WriteLine("Move with \"wasd\"\nPress h to enable hint\nPress any key to start");
Console.ReadKey();
double score = 0;
while (true)
{
    Board b;
    while (true)
    {
        Console.Clear();
        Console.WriteLine("Tutoral(11x11): 0\nEasy(11x21): 1\nMedium(101x11): 2\nHard(101x101): 3\nDemon(521x101):4\ncustom: 5");
        string input = Console.ReadKey().KeyChar.ToString();
        if (byte.TryParse(input, out byte idx))
        {
            if (idx <= 4)
            {
                b = new Board(diffculties[idx]);
                score += getScore(diffculties[idx].Item1, diffculties[idx].Item2);
                break;
            }
            if (idx == 5)
            {
                char[] dims = { 'h', 'w' };
                int[] dimentions = new int[2];
                for (int i = 0; i < 2; i++)
                {
                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine($"Enter an odd number for {dims[i]}");
                        string size = Console.ReadLine();
                        if (int.TryParse(size, out int dim) && 5 <= dim && (dim & 1) == 1)
                        {
                            dimentions[i] = dim;
                            break;
                        }
                    }
                }
                b = new Board((dimentions[0], dimentions[1]));
                score += getScore(dimentions[0], dimentions[1]);
                break;
            }
        }
    }
    ;
    while (true)
    {
        if (score == 0)
            b.hint = false;
        Console.Clear();
        Console.WriteLine(b);
        Console.WriteLine($"Score: {score}");
        char c = Console.ReadKey().KeyChar;
        if (b.hint)
            score--;
        if (c == 'h')
            b.hint = !b.hint;
        else if (!(moves.Contains(c) && b.makeMove(Board.moves[moves.IndexOf(c)])))
        {
            if (b.hint)
                score++;
            Console.Beep();
        }
        else if (b.Won)
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Congratulations! You won!");
            Console.ResetColor();
            Console.ReadKey();
            break;
        }
    }
}

double getScore(int h, int w)
{
    return Math.Round(Math.Sqrt(h*w*(h+w*.5)));
}

class Board
{
    private BitArray board;
    private (int, int) playerPos = (1, 0);
    private int[,] path;
    private int height;
    private int width;
    private static Random rand = new Random();
    public static readonly (int, int)[] moves = { (1, 0), (0, 1), (-1, 0), (0, -1) };
    public bool hint = false;
    public bool Won
    {
        get{return playerPos == (height - 2, width - 1); }
    }

    public Board((int, int) size) : this(size.Item1, size.Item2) { }
    public Board(int h, int w)
    {
        Debug.Assert(3 <= h && 3 <= w && (h & w & 1) == 1);
        board = new BitArray(h * w);
        height = h;
        width = w;
        GenerateMaze(-1,1, 0);
        board.Or(noise());
        this[height - 2, width - 1] = true;
        findPath();
    }
    private void GenerateMaze(int x, int y, byte dirIdx)
    {
        {
            (int, int) dir = moves[dirIdx];
            x += dir.Item1 << 1;
            y += dir.Item2 << 1;
            if (x <= 0 || y <= 0 || height <= y || width <= x || this[y, x])
                return;
            dir = (-dir.Item1, -dir.Item2);
            this[y, x] = true;
            this[y + dir.Item2, x + dir.Item1] = true;
        }
        List<byte> options = new List<byte>(4) { 0, 1, 2, 3 };
        options.RemoveAt(dirIdx);
        while (0 < options.Count)
        {
            byte index = (byte)rand.Next(options.Count);
            GenerateMaze(x, y, options[index]);
            options.RemoveAt(index);
        }
    }
    private void findPath()
    {
        path = new int[height, width];
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                path[i, j] = int.MaxValue;
        path[1, 0] = int.MaxValue-1;
        path[height - 2, width - 1] = 0;
        Queue<(int, int)> queue = new Queue<(int, int)>(board.Length/2);
        queue.Enqueue((height - 2, width - 2));
        while (0 < queue.Count)
        {
            (int, int) idx = queue.Dequeue();
            if (path[idx.Item1, idx.Item2] != int.MaxValue)
                continue;
            int min = int.MaxValue;
            foreach ((int, int) move in moves) {
                (int, int) newIdx = (idx.Item1 + move.Item1, idx.Item2 + move.Item2);
                if (this[newIdx.Item1, newIdx.Item2] && path[newIdx.Item1, newIdx.Item2] == int.MaxValue)
                    queue.Enqueue(newIdx);
                else
                    min = Math.Min(min, path[newIdx.Item1, newIdx.Item2]);
            }
            path[idx.Item1, idx.Item2] = min + 1;
        }
    }
    public bool makeMove ((int,int) move)
    {
        (int, int) newPos = (playerPos.Item1 + move.Item1, playerPos.Item2 + move.Item2);
        if (newPos.Item1 < 0 || newPos.Item2 < 0 || height < newPos.Item1 || width < newPos.Item2 || !this[newPos.Item1, newPos.Item2] || !moves.Contains(move))
            return false;
        playerPos = newPos;
        return true;
    }

    public bool this[int i, int j]
    {
        get
        {
            return board[i * width + j];
        }
        set
        {
            board[i * width + j] = value;
        }
    }
    public override string ToString()
    {
        return hint ? getPath() : getBoard();

    }
    public  string getBoard()
    {
        if ((1, 0) == playerPos)
            return ($"###\n#@.\n##{(this[2, 1] ? '.' : '#')}");
        string s = "";
        for (int i = playerPos.Item1-1; i <= playerPos.Item1+1; i++)
        {
            for (int j = playerPos.Item2-1; j <= playerPos.Item2+1; j++)
            {
                s += (i, j) == playerPos ? "@" : (this[i, j] ? "." : "#");
            }
            s += "\n";
        }
        return s;
    }
    private string getPath()
    {
        if ((1, 0) == playerPos)
            return ($"###\n#@+\n##{(this[2, 1] ? (path[2,1] < path[1,1]? '+' : '.') : '#')}");
        string s = "";
        int min = Math.Min(path[playerPos.Item1-1, playerPos.Item2], Math.Min(path[playerPos.Item1, playerPos.Item2-1], Math.Min(path[playerPos.Item1, playerPos.Item2+1], path[playerPos.Item1+1, playerPos.Item2])));
        for (int i = playerPos.Item1 - 1; i <= playerPos.Item1 + 1; i++)
        {
            for (int j = playerPos.Item2 - 1; j <= playerPos.Item2 + 1; j++)
            {
                s += (j < 0 || width <= j) ? "#" : ((i, j) == playerPos ? "@" : (this[i, j] ? (path[i, j] <= min?"+": "."): "#"));
            }
            s += "\n";
        }
        return s;
    }
    private BitArray noise(int cycles = 5)
    {
        BitArray construction = new BitArray(board.Length);
        for (int i = width+1; i < width * 2 -1; i++)
            construction[i] = true;
        BitArray output = new BitArray(board.Length);
        for (int i = 0; i < height - 3; i++)
        {
            output.Or(construction);
            construction.LeftShift(width);
        }
        output.Or(construction);

        for (int i = 0; i < cycles; i++)
        {
            byte[] randomBytes = new byte[board.Length / 8 + 1];
            rand.NextBytes(randomBytes);
            BitArray randomBits = new BitArray(randomBytes);
            randomBits.Length = board.Length;
            output.And(randomBits);
        }
        return output;
    }
}