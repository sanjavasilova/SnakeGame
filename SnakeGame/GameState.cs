using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnakeGame
{
    public class GameState
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public GridValue[,] Grid {  get; set; }
        public Direction Dir { get; set; }
        public int Score { get; set; }
        public bool GameOver { get; set; }

        private LinkedList<Position> snakePosition = new LinkedList<Position>();
        private Random Random = new Random();
        public GameState(int rows, int cols) 
        { 
            Rows = rows;
            Columns = cols;
            Grid = new GridValue[Rows, Columns];
            Dir = Direction.Right;
        }
    }
}
