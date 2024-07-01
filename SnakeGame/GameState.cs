using SnakeGame;
using System.Collections.Generic;
using System;

public class GameState
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public GridValue[,] Grid { get; set; }
    public Direction Dir { get; set; }
    public int Score { get; set; }
    public bool GameOver { get; set; }

    private LinkedList<Position> snakePosition = new LinkedList<Position>();
    private Random random = new Random();

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

        for (int c = 1; c <= 3; c++)
        {
            Grid[r, c] = GridValue.Snake;
            snakePosition.AddFirst(new Position(r, c));
        }
    }

    private IEnumerable<Position> EmptyPositions()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
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
        List<Position> empty = new List<Position>(EmptyPositions());

        if (empty.Count == 0)
        {
            return;
        }

        Position position = empty[random.Next(empty.Count)];
        Grid[position.Row, position.Column] = GridValue.Apple;

        var timer = new System.Timers.Timer(5000);
        timer.Elapsed += (sender, e) =>
        {
            if (Grid[position.Row, position.Column] == GridValue.Apple)
            {
                Grid[position.Row, position.Column] = GridValue.Empty;
                AddFood();
            }
        };
        timer.Start();
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
        if (Dir.IsOpposite(direction)) return;

        Dir = direction;
    }

    private bool OutsideGrid(Position position)
    {
        return position.Row < 0 || position.Row >= Rows || position.Column < 0 || position.Column >= Columns;
    }

    private GridValue Hit(Position newHeadPosition)
    {
        if (OutsideGrid(newHeadPosition))
        {
            return GridValue.Out;
        }

        if (newHeadPosition == TailPosition())
        {
            return GridValue.Empty;
        }

        return Grid[newHeadPosition.Row, newHeadPosition.Column];
    }

    public void Move()
    {
        if (GameOver) return;

        Position newHeadPos = HeadPosition().Translate(Dir);
        GridValue hit = Hit(newHeadPos);

        if (hit == GridValue.Out || hit == GridValue.Snake)
        {
            Grid[HeadPosition().Row, HeadPosition().Column] = GridValue.DeadHead; 
            foreach (var position in snakePosition)
            {
                Grid[position.Row, position.Column] = GridValue.DeadBody; 
            }
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


    public bool CheckWinCondition()
    {
        int totalCells = Rows * Columns;
        int occupiedCells = 0;

        foreach (var value in Grid)
        {
            if (value == GridValue.Snake || value == GridValue.Apple)
            {
                occupiedCells++;
            }
        }

        double occupiedPercentage = (double)occupiedCells / totalCells;
        return occupiedPercentage >= 0.05; 
    }
}
