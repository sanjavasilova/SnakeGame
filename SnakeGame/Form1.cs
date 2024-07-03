using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        private int bestScore = 0;
        private readonly int rows = 15, cols = 15;
        private GameState gameState;
        private PictureBox[,] pictureBoxes;
        private Timer gameTickTimer;
        private Label gameOverLabel;
        private Label gamePausedLabel;
        private Label winLabel;
        private Button startOverButton;
        private Button startGameButton;
        private bool won = false;
        private int snakeSpeed = 350;
        private bool gameIsPaused = true;
        private bool gameIsRunning = false;
        private ToolStripMenuItem selectedDifficulty;

        public Form1()
        {
            InitializeComponent();
            selectedDifficulty = easyToolStripMenuItem;
            InitializeGameResources();
            InitializeGame();
        }

        private void InitializeGameResources()
        {
            this.BackColor = ColorTranslator.FromHtml(Properties.Resources.BackgroundColor);
            this.ForeColor = ColorTranslator.FromHtml(Properties.Resources.TextColor);
            this.Font = new Font("Droid Sans Mono", 12, FontStyle.Regular);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.menuStrip1.ForeColor = ColorTranslator.FromHtml(Properties.Resources.TextColor);
            this.menuStrip1.Renderer = new MyRenderer(selectedDifficulty);

            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                AddCursorEvents(item);
                AddCursorEventsToDropDownItems(item);
            }


            string iconFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icon.ico");
            if (File.Exists(iconFilePath))
            {
                this.Icon = new Icon(iconFilePath);
            }
            else
            {
                MessageBox.Show("Icon file not found.");
            }
        }

        private class MyRenderer : ToolStripProfessionalRenderer
        {
            private ToolStripMenuItem selectedDifficulty;

            public MyRenderer(ToolStripMenuItem selectedDifficulty)
            {
                this.selectedDifficulty = selectedDifficulty;
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                base.OnRenderMenuItemBackground(e);

                Color backgroundColor = (e.Item.Selected || e.Item == selectedDifficulty) ? Color.FromArgb(60, 60, 90) : Color.FromArgb(49, 44, 64);

                using (SolidBrush brush = new SolidBrush(backgroundColor))
                {
                    e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
                }
                selectedDifficulty.BackColor = Color.FromArgb(60, 60, 90);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                base.OnRenderItemText(e);

                ToolStripMenuItem item = e.Item as ToolStripMenuItem;
                if (item != null)
                {
                    e.Item.ForeColor = Color.White;
                }
            }
        }

        private void AddCursorEvents(ToolStripMenuItem item)
        {
            item.MouseEnter += MenuItem_MouseEnter;
            item.MouseLeave += MenuItem_MouseLeave;
        }

        private void AddCursorEventsToDropDownItems(ToolStripMenuItem item)
        {
            foreach (ToolStripItem subItem in item.DropDownItems)
            {
                if (subItem is ToolStripMenuItem dropdownItem)
                {
                    AddCursorEvents(dropdownItem);
                    AddCursorEventsToDropDownItems(dropdownItem);
                }
            }
        }

        private void MenuItem_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void MenuItem_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void InitializeGame()
        {
            gameState = new GameState(rows, cols);
            InitializeTableLayoutPanel();
            pictureBoxes = SetupGrid();
            UpdateGrid();
            UpdateScore();

            InitializeTimers();
            InitializeLabelsAndButtons();

            startGameButton.Visible = true;
            startGameButton.Left = (this.ClientSize.Width - startGameButton.Width) / 2;
            startGameButton.Top = (this.ClientSize.Height - startGameButton.Height) / 2;

            startGameButton.BringToFront();
        }

        private void InitializeTableLayoutPanel()
        {
            tableGrid.BackColor = ColorTranslator.FromHtml(Properties.Resources.GridBackgroundColor);
            tableGrid.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableGrid.ColumnStyles.Clear();
            tableGrid.RowStyles.Clear();

            for (int i = 0; i < cols; i++)
            {
                tableGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
            }
            for (int i = 0; i < rows; i++)
            {
                tableGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            }
        }

        private PictureBox[,] SetupGrid()
        {
            PictureBox[,] grid = new PictureBox[rows, cols];
            tableGrid.RowCount = rows;
            tableGrid.ColumnCount = cols;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    PictureBox pictureBox = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.Transparent,
                        Image = Images.Empty
                    };

                    tableGrid.Controls.Add(pictureBox, c, r);
                    grid[r, c] = pictureBox;
                }
            }

            return grid;
        }
        private void InitializeTimers()
        {
            this.DoubleBuffered = true;
            gameTickTimer = new Timer();
            gameTickTimer.Interval = snakeSpeed;
            gameTickTimer.Tick += GameTick_Tick;
            gameTickTimer.Start();
        }

        private void InitializeLabelsAndButtons()
        {
            gameOverLabel = CreateLabel("GAME OVER", Color.Red);
            gamePausedLabel = CreateLabel("GAME PAUSED", Color.Red);
            winLabel = CreateLabel("YOU WON :)!!!", Color.Green);
            startOverButton = CreateButton("Start Over", StartOverButton_Click);
            startGameButton = CreateButton("Start Game", StartGameButton_Click);

            this.Controls.Add(gameOverLabel);
            this.Controls.Add(winLabel);
            this.Controls.Add(startOverButton);
            this.Controls.Add(startGameButton);
            this.Controls.Add(gamePausedLabel);

            this.Controls.SetChildIndex(gameOverLabel, 0);
            this.Controls.SetChildIndex(startOverButton, 1);
            this.Controls.SetChildIndex(winLabel, 2);
            this.Controls.SetChildIndex(gamePausedLabel, 3);
            this.Controls.SetChildIndex(startGameButton, 4);

            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.Resize += Form1_Resize;
        }

        private Label CreateLabel(string text, Color color)
        {
            return new Label
            {
                Text = text,
                ForeColor = color,
                Font = new Font("Arial", 24, FontStyle.Bold),
                BackColor = Color.Transparent,
                AutoSize = true,
                Visible = false
            };
        }

        private Button CreateButton(string text, EventHandler clickHandler)
        {
            Button button = new Button
            {
                Text = text,
                ForeColor = Color.Red,
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.LightGray,
                AutoSize = true,
                Visible = false
            };
            button.Click += clickHandler;
            return button;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameIsPaused && gameIsRunning)
            {
                Direction direction = null;

                if (e.KeyCode == Keys.Left)
                {
                    direction = Direction.Left;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    direction = Direction.Right;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    direction = Direction.Up;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    direction = Direction.Down;
                }

                if (direction != null && direction != gameState.Dir.Opposite() && direction != gameState.Dir)
                {
                    gameTickTimer.Stop();
                    gameState.ChangeDirection(direction);
                    MoveOnTick();
                    gameTickTimer.Start();
                }
            }
        }

        private void GameTick_Tick(object sender, EventArgs e)
        {
            MoveOnTick();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            tableGrid.Invalidate();

            if (gameState.GameOver)
            {
                ShowGameOverText();
            }
        }

        private void StartOverButton_Click(object sender, EventArgs e)
        {
            ResetGame();
        }
        private void StartGameButton_Click(Object sender, EventArgs e)
        {
            if (!gameIsRunning)
            {
                startGameButton.Visible = false;
                gameIsRunning = true;
                gameIsPaused = false;
                this.Focus();

            }
        }
        private void UpdateGrid()
        {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        GridValue value = gameState.Grid[r, c];
                        PictureBox pictureBox = pictureBoxes[r, c];

                        if (value == GridValue.Snake)
                        {
                            if (gameState.HeadPosition().Row == r && gameState.HeadPosition().Column == c)
                            {
                                pictureBox.Image = Images.Head;
                            }
                            else
                            {
                                pictureBox.Image = Images.Body;
                            }
                        }
                        else if (value == GridValue.Apple)
                        {
                            pictureBox.Image = Images.Food;
                        }
                        else if (value == GridValue.Out)
                        {
                            pictureBox.Image = Images.DeadBody;
                        }
                        else if (value == GridValue.DeadHead)
                        {
                            pictureBox.Image = Images.DeadHead;
                        }
                        else if (value == GridValue.DeadBody)
                        {
                            pictureBox.Image = Images.DeadBody;
                        }
                        else
                        {
                            pictureBox.Image = Images.Empty;
                        }
                    }
                }

                if (gameState.GameOver)
                {
                    ShowGameOverText();
                    gameTickTimer.Stop();
                }
        }

        private void UpdateScore()
        {
            if (gameState.Score > bestScore)
            {
                bestScore = gameState.Score;
            }
            lblScore.Text = $"SCORE: {gameState.Score}";
            lblBestScore.Text = $"BEST SCORE: {bestScore}";
        }

        private void MoveOnTick()
        {
            if (!gameIsPaused && gameIsRunning)
            {
                gameState.Move();
                UpdateGrid();
                UpdateScore();

                if (!gameState.GameOver && gameState.CheckWinCondition())
                {
                    gameTickTimer.Stop();
                    won = true;
                    ShowGameOverText();
                }
                else if (gameState.GameOver)
                {
                    gameTickTimer.Stop();
                    won = false;
                    ShowGameOverText();
                }
            }
        }

        private void ShowGameOverText()
        {
            Label label;
            if (won)
            {
                label = winLabel;
            }
            else
            {
                label = gameOverLabel;
            }
            label.Visible = true;
            label.Left = (this.ClientSize.Width - label.Width) / 2;
            label.Top = (this.ClientSize.Height - label.Height) / 2;

            startOverButton.Visible = true;
            startOverButton.Left = (this.ClientSize.Width - startOverButton.Width) / 2;
            startOverButton.Top = label.Bottom + 10;
            startOverButton.BringToFront();
        }

        private void ShowGamePausedText()
        {
            Label label;
            if (gameIsPaused)
            {
                label = gamePausedLabel;
                label.Visible = true;
            }
            else
            {
                label = gamePausedLabel;
                label.Visible = false;
            }
            label.Left = (this.ClientSize.Width - label.Width) / 2;
            label.Top = (this.ClientSize.Height - label.Height) / 2;
        }

        private void startOverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopGame();
            ResetGame();
        }

        private void easyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            snakeSpeed = 350;
            selectedDifficulty = easyToolStripMenuItem;
            ResetGame();
            menuStrip1.Renderer = new MyRenderer(selectedDifficulty);
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            snakeSpeed = 200;
            selectedDifficulty = mediumToolStripMenuItem;
            ResetGame();
            menuStrip1.Renderer = new MyRenderer(selectedDifficulty);
        }

        private void hardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            snakeSpeed = 50;
            selectedDifficulty = hardToolStripMenuItem;
            ResetGame();
            menuStrip1.Renderer = new MyRenderer(selectedDifficulty);
        }

        private void ResetGame()
        {
            gameState = new GameState(rows, cols);
            UpdateGrid();
            UpdateScore();

            gameOverLabel.Visible = false;
            winLabel.Visible = false;
            startOverButton.Visible = false;
            gamePausedLabel.Visible = false;
            startGameButton.Visible = true;
            won = false;

            gameIsPaused = true;
            gameIsRunning = false;
            this.Focus();
            gameTickTimer.Interval = snakeSpeed;
            gameTickTimer.Start();
        }

        private void pauseGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gameIsRunning)
            {
                if (gameIsPaused)
                {
                    gameIsPaused = false;
                    gameTickTimer.Start();
                }
                else
                {
                    gameIsPaused = true;
                    gameTickTimer.Stop();
                }
                pauseGameToolStripMenuItem.Text = gameIsPaused ? "Resume Game" : "Pause Game";

                pauseGameToolStripMenuItem.Invalidate();
                if (!gameState.GameOver)
                {
                    ShowGamePausedText();

                }
            }
        }

        private void changeModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedDifficulty.BackColor = Color.FromArgb(60, 60, 60);
        }

        private void StopGame()
        {
            gameTickTimer.Stop();
        }
    }
}
