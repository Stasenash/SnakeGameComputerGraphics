using System;
using System.Drawing;

namespace Snake
{
    internal class Sector : ICloneable
    {
        private Rectangle rect;

        public Sector(int x, int y, int width, int height)
        {
            rect = new Rectangle(x, y, width, height);
        }

        public Sector(Sector sector)
        {
            rect = sector.rect;
        }

        public int X
        {
            get { return rect.X; }
            set { rect.X = value; }
        }

        public int Y
        {
            get { return rect.Y; }
            set { rect.Y = value; }
        }

        public int Width
        {
            get { return rect.Width; }
            set { rect.Width = value; }
        }

        public int Height
        {
            get { return rect.Height; }
            set { rect.Height = value; }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void SetLocation(int x, int y)
        {
            rect.X = x;
            rect.Y = y;
        }

        public static bool Equals(Sector a, Sector b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
    }
}