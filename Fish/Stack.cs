using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fish
{
    class Stack
    {
        Fish fish;

        double[] stack;
        int index = -1;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="size">Size of the stack</param>
        /// <param name="fish">Reference</param>
        public Stack(int size, Fish fish)
        {
            stack = new double[size];
            this.fish = fish;
        }

        /// <summary>
        /// Push a value to the stack
        /// </summary>
        /// <param name="x">Value to push</param>
        public void Push(double x)
        {
            if (index >= stack.Length - 1)
                throw new IndexOutOfRangeException("Can't push byond the stack m8");

            stack[++index] = x;
        }

        /// <summary>
        /// Pop a value from the stack
        /// </summary>
        /// <returns>The top value</returns>
        public double Pop()
        {
            if (index <= -1)
                throw new IndexOutOfRangeException("Stack empty, can't pop that sir");

            double x = stack[index];
            stack[index] = 0;
            index--;
            return x;
        }

        /// <summary>
        /// Reverse a stack
        /// </summary>
        public void Reverse()
        {
            for (int left = 0, right = index; left < right; left++, right--)
            {
                double x = stack[left];
                stack[left] = stack[right];
                stack[right] = x;
            }
        }

        public int Lenght()
        {
            return index + 1;
        }


        public void Action(char c)
        {
            double x, y, z;

            switch (c)
            {
                case ':':
                    x = Pop();
                    Push(x);
                    Push(x);
                    break;

                case '~':
                    x = Pop();
                    break;

                case '$':
                    x = Pop();
                    y = Pop();
                    Push(x);
                    Push(y);

                    break;
                case '@':
                    x = Pop();
                    y = Pop();
                    z = Pop();
                    Push(x);
                    Push(z);
                    Push(y);
                    break;

                case '}':
                    x = stack[index];
                    for (int a = index; a > 0; a--)
                        stack[a] = stack[a - 1];
                    stack[0] = x;
                    break;

                case '{':
                    x = stack[0];
                    for (int a = 0; a < index; a++)
                        stack[a] = stack[a + 1];
                    stack[index] = x;
                    break;

                case 'r':
                    Reverse();
                    break;

                case 'l':
                    Push(Lenght());
                    break;
                default:
                    break;
            }
        }

      
    }
}
