using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Windows.Forms;

namespace Snake
{
    internal class SnakeGame
    {
        private const int BODYSIZE = 20;

        private readonly Brush bodyBrush = new SolidBrush(Color.LimeGreen);
        private readonly Brush foodBrush = new SolidBrush(Color.Yellow);
        private readonly Brush headBrush = new SolidBrush(Color.Red);
        private readonly Brush infoBrush = new SolidBrush(Color.White);
        private readonly Brush scoreBrush = new SolidBrush(Color.Gray);

        private readonly Font infoFont = new Font("Verdana", 16, FontStyle.Bold);
        private readonly Font scoreFont = new Font("Verdana", 12, FontStyle.Bold);

        //связь между "тиками" и скоростью игры
        private readonly Dictionary<int, int> gameSpeeds;
        private readonly List<Sector> grid;        
        private readonly List<Direction> turnQueue;
        private readonly Snake snake;

        //здесь отрисовывается змейка
        private readonly PictureBox gamePanel;
        private readonly Timer gameTimer;
        private readonly Random random;

        private Sector food;

        private int gameSpeed;
        private int score;

        private bool gameEnded;
        private bool gamePaused;
        private bool gameStarted;

        public SnakeGame(PictureBox gamePanel)
        {
            this.gamePanel = gamePanel;
            this.gamePanel.Paint += GamePanelPaint;

            random = new Random(Environment.TickCount);

            grid = new List<Sector>();
            turnQueue = new List<Direction>();
            gameSpeeds = new Dictionary<int, int>();

            FillSpeeds(gameSpeeds);
            gameSpeed = 3;

            gameStarted = false;

            InitGrid(grid);
            snake = new Snake(BODYSIZE, new Point(200, 200));

            NewGame(true);

            gameTimer = new Timer();
            gameTimer.Interval = gameSpeeds[gameSpeed];
            gameTimer.Tick += GameTimerTick;

            gameTimer.Start();
        }

        private void NewGame(bool startingGame)
        {
            //в начале игры змейка не двигается
            if (startingGame)
            {
                snake.MovingDirection = Direction.NotMoving;
            }
            else
            {
                snake.MovingDirection = Direction.Right;
                if (!gameStarted)
                    gameStarted = true;
            }
            snake.Reset();
            food = GetRandomSector();

            // Начинаем новую игру и все очищаем
            turnQueue.Clear();
            score = 0;
            gamePaused = false;
            gameEnded = false;
        }

        //перерисовка змейки
        private void GamePanelPaint(object sender, PaintEventArgs e)
        {
            lock (this)
            {
                var image = new Bitmap(gamePanel.Width, gamePanel.Height);
                Graphics graphics = Graphics.FromImage(image);

                try
                {
                    // сглаживание для лучшего восприятия глазом
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, gamePanel.Width, gamePanel.Height);

                    DrawSnake(graphics);
                    DrawFood(graphics);
                    DrawInfo(graphics);

                    // отрисовка зависит от состояния
                    if (!gameStarted)
                        DrawGameBegin(graphics);
                    if (gameEnded)
                        DrawGameEnded(graphics);
                    if (gamePaused)
                        DrawGamePaused(graphics);

                    e.Graphics.DrawImage(image, 0, 0);
                }
                finally
                {
                    e.Dispose();
                    graphics.Dispose();
                }
            }
        }

        //в момент тика перерисовываем змейку и проверяем не закончена ли игра
        private void GameTimerTick(object sender, EventArgs e)
        {
            if (!gamePaused && !gameEnded)
            {
                if (turnQueue.Count >= 1)
                {
                    if (IsCompatibleTurn(turnQueue[0]))
                    {
                        snake.MovingDirection = turnQueue[0];
                    }
                    turnQueue.RemoveAt(0);
                }
                snake.Move();

                if (IsFoodAte())
                {
                    food = GetRandomSector();
                    snake.Grow(1);
                    score += 10;
                    PlaySound(SoundToPlay.AteFood);
                }
                if (IsGameOver() && snake.MovingDirection != Direction.NotMoving)
                {
                    gameEnded = true;
                    PlaySound(SoundToPlay.GameOver);
                }
            }
            gamePanel.Invalidate();
        }

        private void DrawGameBegin(Graphics graphics)
        {
            graphics.DrawString("Press enter to start...", infoFont, infoBrush, gamePanel.Width / 2 - 120, gamePanel.Height / 2 - 20);
        }

        private void DrawGamePaused(Graphics graphics)
        {
            graphics.DrawString("Game Paused", infoFont, infoBrush, gamePanel.Width / 2 - 100, gamePanel.Height / 2 - 20);
        }

        private void DrawGameEnded(Graphics graphics)
        {
            graphics.DrawString("Game Over!", infoFont, infoBrush, gamePanel.Width / 2 - 70, gamePanel.Height / 2 - 30);
            graphics.DrawString("Press enter to try again", infoFont, infoBrush, gamePanel.Width / 2 - 130, gamePanel.Height / 2);
        }

        private void DrawInfo(Graphics graphics)
        {
            graphics.DrawString("Score: " + score + "    Speed: " + gameSpeed + "    Pause: press P", scoreFont, scoreBrush, 2, 2);
        }

        private void DrawFood(Graphics graphics)
        {
            graphics.FillEllipse(foodBrush, food.X, food.Y, food.Width, food.Height);
        }

        private void DrawSnake(Graphics graphics)
        {
            Sector head = snake.GetHeadSector();
            graphics.FillEllipse(headBrush, head.X, head.Y, head.Width, head.Height);

            for (int i = 1; i < snake.Length; i++)
            {
                Sector sector = snake.GetSectorAt(i);
                graphics.FillEllipse(bodyBrush, sector.X, sector.Y, sector.Width, sector.Height);
            }
        }

        private Sector GetRandomSector()
        {
            Sector randomSector = null;
            bool foundSuitableSector = false;

            while (!foundSuitableSector)
            {
                randomSector = grid[random.Next(0, grid.Count)];

                for (int i = 0; i < snake.Length; i++)
                {
                    foundSuitableSector =  !Sector.Equals(snake.GetSectorAt(i), randomSector);
                }
            }
            return randomSector;
        }

        private static void FillSpeeds(IDictionary<int, int> speeds)
        {
            speeds.Add(1, 500);
            speeds.Add(2, 300);
            speeds.Add(3, 150);
            speeds.Add(4, 80);
            speeds.Add(5, 40);
            speeds.Add(6, 1);
        }

        private void InitGrid(ICollection<Sector> gameGrid)
        {
            int x = gamePanel.Width - BODYSIZE;
            int y = gamePanel.Height - BODYSIZE;

            for (int i = 0; i <= x; i += BODYSIZE)
            {
                for (int j = 0; j <= y; j += BODYSIZE)
                {
                    var gridSector = new Sector(i, j, BODYSIZE, BODYSIZE);
                    gameGrid.Add(gridSector);
                }
            }
        }

        private bool IsSelfHarm()
        {
            Sector head = snake.GetHeadSector();

            for (int i = 1; i < snake.Length; i++)
            {
                if (Sector.Equals(head, snake.GetSectorAt(i)))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsFoodAte()
        {
            return Sector.Equals(snake.GetHeadSector(), food);
        }

        private bool IsGameOver()
        {
            Sector headSector = snake.GetHeadSector();

            if ((headSector.X < 0 && snake.MovingDirection == Direction.Left) ||
                (headSector.Y < 0 && snake.MovingDirection == Direction.Up) ||
                (headSector.X > gamePanel.Width - BODYSIZE && snake.MovingDirection == Direction.Right) ||
                (headSector.Y > gamePanel.Height - BODYSIZE && snake.MovingDirection == Direction.Down))
            {
                return true;
            }
            return IsSelfHarm();
        }

        private bool IsCompatibleTurn(Direction newDirection)
        {
            return !((snake.MovingDirection == newDirection) ||
                (snake.MovingDirection == Direction.Left && newDirection == Direction.Right) ||
                (snake.MovingDirection == Direction.Right && newDirection == Direction.Left) ||
                (snake.MovingDirection == Direction.Up && newDirection == Direction.Down) ||
                (snake.MovingDirection == Direction.Down && newDirection == Direction.Up));
        }

        public void HandleKeyPress(Keys key)
        {
            if (key == Keys.Enter)
            {
                NewGame(false);
            }

            if (gameStarted)
            {
                switch (key)
                {
                    case Keys.Left:
                        SetDirection(Direction.Left);
                        break;
                    case Keys.Down:
                        SetDirection(Direction.Down);
                        break;
                    case Keys.Right:
                        SetDirection(Direction.Right);
                        break;
                    case Keys.Up:
                        SetDirection(Direction.Up);
                        break;
                    case Keys.Oemplus: 
                        if (gameSpeed + 1 <= gameSpeeds.Count)
                        {
                            gameTimer.Interval = gameSpeeds[++gameSpeed];
                        }
                        break;
                    case Keys.OemMinus: 
                        if (gameSpeed - 1 > 0)
                        {
                            gameTimer.Interval = gameSpeeds[--gameSpeed];
                        }
                        break;
                    case Keys.P:
                        if (!gameEnded)
                            gamePaused = !gamePaused;
                        break;
                }
            }
        }

        public void SetDirection(Direction newDirection)
        {
            if (!gamePaused && !gameEnded)
            {
                if (turnQueue.Count > 0)
                {
                    if (turnQueue[turnQueue.Count - 1] != newDirection)
                    {
                        turnQueue.Add(newDirection);
                        return;
                    }
                }
                turnQueue.Add(newDirection);
            }
        }

        enum SoundToPlay
        {
            AteFood,
            GameOver
        }

        private void PlaySound(SoundToPlay sound)
        {
            SoundPlayer player;

            switch (sound)
            {
                case SoundToPlay.AteFood:
                    player = new SoundPlayer(@"sounds\109662__grunz__success.wav");
                    break;
                case SoundToPlay.GameOver:
                    player = new SoundPlayer(@"sounds\159408__noirenex__life-lost-game-over.wav");
                    break;
                default:
                    player = new SoundPlayer();
                    break;
            }
            player.Play();
        }
    }
}