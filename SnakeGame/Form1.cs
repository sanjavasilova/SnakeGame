using System;
using System.Drawing;
using System.IO;
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
        private Label winLabel;
        private Button startOverButton;
        private bool won = false;

        public Form1()
        {
            InitializeComponent();
            InitializeGameResources();
            InitializeGame();
        }

        private void InitializeGameResources()
        {
            this.BackColor = ColorTranslator.FromHtml(Properties.Resources.BackgroundColor);
            this.ForeColor = ColorTranslator.FromHtml(Properties.Resources.TextColor);
            this.Font = new Font("Droid Sans Mono", 12, FontStyle.Regular);
            this.StartPosition = FormStartPosition.CenterScreen;

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

        private void InitializeGame()
        {
            gameState = new GameState(rows, cols);
            InitializeTableLayoutPanel();
            pictureBoxes = SetupGrid();
            UpdateGrid();
            UpdateScore();

            InitializeTimers();
            InitializeLabelsAndButtons();
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
            gameTickTimer.Interval = 200;
            gameTickTimer.Tick += GameTick_Tick;
            gameTickTimer.Start();
        }

        private void InitializeLabelsAndButtons()
        {
            gameOverLabel = CreateLabel("GAME OVER", Color.Red);
            winLabel = CreateLabel("YOU WON :)!!!", Color.Green);
            startOverButton = CreateButton("Start Over", StartOverButton_Click);

            this.Controls.Add(gameOverLabel);
            this.Controls.Add(winLabel);
            this.Controls.Add(startOverButton);

            this.Controls.SetChildIndex(gameOverLabel, 0);
            this.Controls.SetChildIndex(startOverButton, 1);
            this.Controls.SetChildIndex(winLabel, 2);

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
                        if(gameState.HeadPosition().Row == r && gameState.HeadPosition().Column == c)
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

        private void ResetGame()
        {
            gameState = new GameState(rows, cols);
            UpdateGrid();
            UpdateScore();

            gameOverLabel.Visible = false;
            winLabel.Visible = false;
            startOverButton.Visible = false;
            this.Focus();
            gameTickTimer.Start();
        }
    }
}
