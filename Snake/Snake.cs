using System.Collections.Generic;
using System.Drawing;

namespace Snake
{    internal class Snake
    {
        private readonly List<Sector> segments;

        private readonly int size;

        private Point defaultPos;

        public Snake(int size) : this(size, new Point(100, 100))
        {
        }

        public Snake(int size, Point defaultPos)
        {
            this.size = size;
            this.defaultPos = defaultPos;
            segments = new List<Sector>();
        }

        public Direction MovingDirection { get; set; }

        public int Length
        {
            get { return segments.Count; }
        }

        public void Reset()
        {
            segments.Clear();
            segments.Add(new Sector(defaultPos.X, defaultPos.Y, size, size));
            Grow(3);
        }

        public void Grow(int howMany)
        {
            for (int i = howMany; i > 0; i--)
            {
                segments.Add(new Sector(segments[segments.Count - 1]));
            }
        }

        public void Move()
        {
            //имитируем движение за головой змеи
            for (int n = Length - 1; n >= 1; n--)
            {
                segments[n] = segments[n - 1].Clone() as Sector;
            }

            // затем уже сдвигаем голову в нужном направлении
            switch (MovingDirection)
            {
                case Direction.Left:
                    segments[0].X -= size;
                    break;
                case Direction.Right:
                    segments[0].X += size;
                    break;
                case Direction.Up:
                    segments[0].Y -= size;
                    break;
                case Direction.Down:
                    segments[0].Y += size;
                    break;
            }
        }

        public Sector GetHeadSector()
        {
            return segments.Count >= 1 ? segments[0] : null;
        }

        public Sector GetSectorAt(int index)
        {
            return segments[index];
        }
    }
}