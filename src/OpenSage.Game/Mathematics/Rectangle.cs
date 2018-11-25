﻿using System;

namespace OpenSage.Mathematics
{
    public readonly struct Rectangle
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public Point2D Location => new Point2D(X, Y);
        public Size Size => new Size(Width, Height);

        public int Left => X;
        public int Right => X + Width;
        public int Top => Y;
        public int Bottom => Y + Height;

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(in Point2D location, in Size size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.Width;
            Height = size.Height;
        }

        public bool Intersects(in Rectangle value)
        {
            return value.Left < Right &&
                Left < value.Right &&
                value.Top < Bottom &&
                Top < value.Bottom;
        }

        public static Rectangle Intersect(in Rectangle value1, in Rectangle value2)
        {
            if (value1.Intersects(value2))
            {
                var rightSide = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                var leftSide = Math.Max(value1.X, value2.X);
                var topSide = Math.Max(value1.Y, value2.Y);
                var bottomSize = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                return new Rectangle(leftSide, topSide, rightSide - leftSide, bottomSize - topSide);
            }
            else
            {
                return new Rectangle(0, 0, 0, 0);
            }
        }

        public static Rectangle FromCorners(in Point2D topLeft, in Point2D bottomRight)
        {
            return new Rectangle(topLeft, (bottomRight - topLeft).ToSize());
        }

        public bool Contains(in Point2D point)
        {
            return point.X >= X
                && point.X <= Right
                && point.Y >= Y
                && point.Y <= Bottom;
        }

        public RectangleF ToRectangleF()
        {
            return new RectangleF(X, Y, Width, Height);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Rectangle))
            {
                return false;
            }

            var rectangle = (Rectangle) obj;
            return X == rectangle.X &&
                   Y == rectangle.Y &&
                   Width == rectangle.Width &&
                   Height == rectangle.Height;
        }

        public override int GetHashCode()
        {
            var hashCode = 466501756;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Rectangle rectangle1, Rectangle rectangle2)
        {
            return rectangle1.Equals(rectangle2);
        }

        public static bool operator !=(Rectangle rectangle1, Rectangle rectangle2)
        {
            return !(rectangle1 == rectangle2);
        }
    }
}
