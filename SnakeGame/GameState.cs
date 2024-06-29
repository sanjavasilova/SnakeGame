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

            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;

            for(int c=1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePosition.AddFirst(new Position(r, c));
            }
        }

        private IEnumerable<Position> emptyPositions()
        {
            for (int r=0;r<Rows; r++)
            {
                for(int c = 0; c < Columns; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(emptyPositions());

            if(empty.Count == 0)
            {
                return;
            }

            Position Position = empty[Random.Next(empty.Count)];
            Grid[Position.Row, Position.Column] = GridValue.Apple;
        }

        public Position HeadPosition()
        {
            return snakePosition.First.Value;
        }

        public Position TailPosition()
        {
            return snakePosition.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePosition;
        }

        private void AddHead(Position position)
        {
            snakePosition.AddFirst(position);
            Grid[position.Row, position.Column] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePosition.Last.Value;
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            snakePosition.RemoveLast();
        }

        public void ChangeDirection(Direction direction)
        {
            Dir = direction;
        }

        private bool OutsideGrid(Position position)
        {
            return position.Row < 0 || position.Row >= Rows || position.Column < 0 || position.Column >=Columns;
        }

        private GridValue Hit(Position newHeadPosition)
        {
            if (OutsideGrid(newHeadPosition))
            {
                return GridValue.Out;
            }

            if(newHeadPosition == TailPosition())
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPosition.Row, newHeadPosition.Column];
        }

        public void Move()
        {
            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = Hit(newHeadPos);

            if (hit == GridValue.Out || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if (hit == GridValue.Apple)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }
        }
    }
}
