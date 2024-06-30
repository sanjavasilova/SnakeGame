using System;
using System.Collections.Generic;

namespace SnakeGame
{
    public class Direction
    {
        public static readonly Direction Left = new Direction(0, -1);
        public static readonly Direction Right = new Direction(0, 1);
        public static readonly Direction Up = new Direction(-1, 0);
        public static readonly Direction Down = new Direction(1, 0);

        public int RowOffset { get; }
        public int ColumnOffset { get; }

        private Direction(int rowOffset, int columnOffset)
        {
            RowOffset = rowOffset;
            ColumnOffset = columnOffset;
        }

        public Direction Opposite()
        {
            return new Direction(-RowOffset, -ColumnOffset);
        }

        public bool IsOpposite(Direction other)
        {
            return this.Opposite() == other;
        }

        public override bool Equals(object obj)
        {
            return obj is Direction direction &&
                   RowOffset == direction.RowOffset &&
                   ColumnOffset == direction.ColumnOffset;
        }

        public override int GetHashCode()
        {
            int hashCode = -1983741850;
            hashCode = hashCode * -1521134295 + RowOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + ColumnOffset.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Direction left, Direction right)
        {
            return EqualityComparer<Direction>.Default.Equals(left, right);
        }

        public static bool operator !=(Direction left, Direction right)
        {
            return !(left == right);
        }
    }
}
