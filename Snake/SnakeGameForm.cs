using System.Windows.Forms;

namespace Snake
{
    public partial class SnakeGameForm : Form
    {
        private readonly SnakeGame game;

        public SnakeGameForm()
        {
            InitializeComponent();
            game = new SnakeGame(Canvas);
        }

        private void MainFormKeyDown(object sender, KeyEventArgs e)
        {
            game.HandleKeyPress(e.KeyCode);
        }
    }
}