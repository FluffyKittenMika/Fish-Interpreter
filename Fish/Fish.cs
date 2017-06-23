using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Fish
{
    class Fish
    {
        public int stackSize = 65536; //64*1024 the size of the stack that we get to work with
        public int[] pos = { -1, 0 }; //starting position
        public int stackID = 0;
        public double register = 0d; //valueof the register
        
        public bool Skipping { get; set; }  = false ; //this will trigger if a command needs to be 'jumped'
        public bool REG { get; set; }       = false ;
        public bool IsRunning { get; set; } = true  ;
        public bool IsString { get; set; }  = false ;
        public char Quote { get; set; }
        Stack[] stacks = new Stack[1024];
        char[][] grid = null;//must be loaded inn
        Directions dir = Directions.RIGHT;
        StringBuilder sb = new StringBuilder();
        Random rng = new Random();

        
        private string input;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="input">file path</param>
        public Fish(string input)
        {
            this.input = input;
            stacks[stackID] = new Stack(stackSize, this);
            string[] FileContent;

            try
            {
                FileContent = System.IO.File.ReadAllLines(input);
                grid = new char[FileContent.Length][];

                //convert s[] to c[][]
                int i = 0;
                foreach (var line in FileContent)
                    grid[i++] = line.ToCharArray();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //get maximum x
            int top = grid[0].Length;
            foreach (char[] line in grid)
                if (line.Length > top)
                    top = line.Length;

            //top long
            for (int a = 0; a < grid.Length; a++)
            {
                char[] line = grid[a];
                char[] newline = new char[top];
                int b = 0;

                foreach (char c in line)
                    newline[b++] = c;
                grid[a] = newline;
            }

            try
            {
                while (IsRunning)
                {
                    //move the position in the right direction
                    pos = dir.Move(pos);

                    //check for bounds
                    Check();

                    if (Skipping)
                    {
                        Skipping = false;
                        continue;
                    }

                    //check current char
                    Parse();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// loop back if we're out of bounds
        /// </summary>
        public void Check()
        {
            int y = pos[1], maxy = grid.Length - 1;
            if (y < 0)
                y = maxy;

            if (y > maxy)
                y = 0;

            int x = pos[0], maxx = grid[y].Length - 1;

            if (x < 0)
                x = maxx;

            if (x > maxx)
                x = 0;

            pos = new int[] { x, y };
        }

        /// <summary>
        /// chack if input is a valid number
        /// </summary>
        /// <param name="s">input string</param>
        /// <returns>bool</returns>
        public bool IsNumber(string s)
        {
            if (int.TryParse(s, out int result)) 
                return true;
            if (double.TryParse(s, out double dresult))
                return true;

            return false;
        }

        /// <summary>
        /// Checks double if it's a valid int
        /// </summary>
        /// <param name="x">input double</param>
        /// <returns>bool</returns>
        public bool IsInterger(double x)
        {
            return Math.Floor(x) == Math.Ceiling(x) && x > int.MinValue && x < int.MaxValue;
        }

        /// <summary>
        /// Merge two stacks together to make a new and improved stack, a better stack, one that you can contribute to, a new stack and better stack.
        /// </summary>
        /// <param name="A">first stack</param>
        /// <param name="B">other stack</param>
        /// <returns>result stack</returns>
        public Stack Merge(Stack A, Stack B)
        {
            Stack r = new Stack(stackSize, this);
            while (B.Lenght() > 0)
                r.Push(B.Pop());
            while (A.Lenght() > 0)
                r.Push(A.Pop());
            r.Reverse();
            return r;
        }

        public void Parse()
        {

        char c = grid[pos[1]][pos[0]];

            // ^C
            if (c == 3) {
                Console.WriteLine("^C");
                return;
            }

            // String
            else if (c == '"' || c == '\'') {
                if (IsString) {
                    if (Quote == c) {
                        // Get the built string
                        string s = sb.ToString();
                        // Reset the builder
                        sb = new StringBuilder();
                        
                        // Push every character from the string to the current stack
                        foreach (char x in s.ToCharArray()) 
                            stacks[stackID].Push(x);

                        // Reset values
                        IsString = false;
                        Quote = '0';
                    } else 
                        sb.Append(c); // Append quote to the builder
                } else {
                    // Enable string'ing
                    IsString = true;
                    Quote = c;
                }
                return;
            }

            // Append character to the builder when string'ing is turned on
            else if (IsString) {
                sb.Append(c);
                return;
            }

            // NOP
            else if (c == '0' || c == ' ') {
                return;
            }

            // Movement
            else if (c == '>' || c == '<' || c == '^' || c == 'v' || c == '/' || c == '\\' || c == '|' || c == '_' || c == 'x' || c == '#') {
                // Let Directions handle this
                dir = (Directions)Get(c);
                return;
            }

            // Trampoline
            else if (c == '!') {
                Skipping = true;
            }

            // Conditional trampoline
            else if (c == '?') {
                double x = stacks[stackID].Pop();
                Skipping = (x == 0);
            }

            // Jump
            else if (c == '.') {
                double y = stacks[stackID].Pop();
                double x = stacks[stackID].Pop();

                if (!IsInterger(y) || !IsInterger(x)) { // Integer check
                    throw new NotFiniteNumberException("X or Y is no integer!");
                }

                // Safe to cast
                pos = new int[] { (int)x, (int)y };
            }

            // Numbers
            else if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')) {

                // Parse in hexadecimal (base 16)
                int i = Convert.ToInt32(c + "", 16);
                stacks[stackID].Push(i);
                return;
            }

            // Expressions
            else if (c == '+' || c == '-' || c == '*' || c == ',' || c == '%' || c == '=' || c == '(' || c == ')') {
                double y = stacks[stackID].Pop();
                double x = stacks[stackID].Pop();
                double z = 0;

                // Calculate
                switch (c) {
                    case '+':
                        z = x + y;
                        break;
                    case '-':
                        z = x - y;
                        break;
                    case '*':
                        z = x * y;
                        break;
                    case ',':
                        z = x / y;
                        break;
                    case '%':
                        z = x % y;
                        break;
                    case '=':
                        z = (x == y) ? 1 : 0;
                        break;
                    case '(':
                        z = (y > x) ? 1 : 0;
                        break;
                    case ')':
                        z = (y < x) ? 1 : 0;
                        break;
                    default:
                        break;
                }

                stacks[stackID].Push(z);
            }

            // Stack manipulation
            else if (c == ':' || c == '~' || c == '$' || c == '@' || c == '}' || c == '{' || c == 'r' || c == 'l') {
                // Let the stack handle this
                stacks[stackID].Action(c);
            }

            // New stacks
            else if (c == '[' || c == ']') {
                if (c == '[') {
                    double x = stacks[stackID].Pop();
                    if (Math.Floor(x) != Math.Ceiling(x)) 
                        throw new NotFiniteNumberException("X is no Integer!");

                    stackID++;
                    stacks[stackID] = new Stack(stackSize, this);

                    for (int a = 0; a < x; a++) {
                        double item = stacks[stackID - 1].Pop();
                        stacks[stackID].Push(item);
                    }
                    stacks[stackID].Reverse();
                }

                else if (c == ']') {
                    stackID--;
                    stacks[stackID] = Merge(stacks[stackID], stacks[stackID + 1]);
                }
            }

            else if (c == 'n') {
                double i = stacks[stackID].Pop();
                if (IsInterger(i)) 
                    Console.Write((int)i);
                else if (i >= Double.MinValue && i <= Double.MaxValue) 
                    Console.Write(i);
                else 
                    throw new NotFiniteNumberException("Number too large");
                return;
            }

            else if (c == 'o') {
                double i = stacks[stackID].Pop();
                Console.Write((char)i);
                return;
            }

            // Input
            else if (c == 'i') {
                int i = int.Parse(Console.ReadLine());
                stacks[stackID].Push(i);
                return;
            }

            // Register 
            else if (c == '&') {
                if (REG) stacks[stackID].Push(register);
                else register = stacks[stackID].Pop();
                REG = !REG;
                return;
            }

            // Get character
            else if (c == 'g') {

                double y = stacks[stackID].Pop();
                double x = stacks[stackID].Pop();

                if (!IsInterger(y) || !IsInterger(x)) {
                    throw new NotFiniteNumberException("X or Y is no integer!");
                }

                char v = grid[(int)y][(int)x];
                stacks[stackID].Push(v);

                return;
            }

            // Change character
            else if (c == 'p') {
                double y = stacks[stackID].Pop();
                double x = stacks[stackID].Pop();
                double v = stacks[stackID].Pop();

                if (!IsInterger(y) || !IsInterger(x) || !IsInterger(v)) {
                    throw new NotFiniteNumberException("X, Y or V is no integer!");
                }

                grid[(int)x][(int)y] = (char)v;

                return;
            }

            // Terminater
            else if (c == ';') {
                IsRunning = false;
                return;
            }

            // Unknown
            else 
                return;
	}


        public Directions? Get(char c)
        {
            switch (c)
            {
                case '>':
                    return Directions.RIGHT;
                case '<':
                    return Directions.LEFT;
                case '^':
                    return Directions.UP;
                case 'v':
                    return Directions.DOWN;
                case '/':
                    switch (dir)
                    {
                        case Directions.RIGHT:
                            return Directions.UP;
                        case Directions.LEFT:
                            return Directions.DOWN;
                        case Directions.DOWN:
                            return Directions.LEFT;
                        case Directions.UP:
                            return Directions.RIGHT;
                        default:
                            return null;
                    }
                case '\\':
                    switch (dir)
                    {
                        case Directions.RIGHT:
                            return Directions.DOWN;
                        case Directions.LEFT:
                            return Directions.UP;
                        case Directions.DOWN:
                            return Directions.RIGHT;
                        case Directions.UP:
                            return Directions.LEFT;
                        default:
                            return null;
                    }
                case '|':
                    if (dir == Directions.UP || dir == Directions.DOWN)
                        return dir;
                    else if (dir == Directions.RIGHT)
                        return Directions.LEFT;
                    else if (dir == Directions.LEFT)
                        return Directions.RIGHT;
                    return null;
                case '_':
                    if (dir == Directions.RIGHT || dir == Directions.LEFT)
                        return dir;
                    else if (dir == Directions.UP)
                        return Directions.DOWN;
                    else if (dir == Directions.DOWN)
                        return Directions.UP;
                    return null;
                case 'x':
                    switch (rng.Next(1, 4))
                    {
                        case 1:
                            return Directions.RIGHT;
                        case 2:
                            return Directions.LEFT;
                        case 3:
                            return Directions.UP;
                        case 4:
                            return Directions.DOWN;
                        default:
                            Console.WriteLine("wtf?");
                            return null;
                    }
                case '#':
                    if (dir == Directions.RIGHT)
                        return Directions.LEFT;
                    else if (dir == Directions.LEFT)
                        return Directions.RIGHT;
                    else if (dir == Directions.UP)
                        return Directions.DOWN;
                    else if (dir == Directions.DOWN)
                        return Directions.UP;
                    return null;
                default:
                    return null;
            }
        }
    }



    public enum Directions
    {
        RIGHT, LEFT, DOWN, UP
    }

    static class DirectionsMethods
    {


        public static int[] Move(this Directions d, int[] pos)
        {
            switch (d)
            {
                case Directions.RIGHT:
                    pos[0] += 1;
                    break;
                case Directions.LEFT:
                    pos[0] -= 1;
                    break;
                case Directions.DOWN:
                    pos[1] += 1;
                    break;
                case Directions.UP:
                    pos[1] -= 1;
                    break;
                default:
                    break;
            }

            return pos;
        }
    }
}

