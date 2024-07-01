using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        private int bestScore = 0;
        GameState gameState;
        private readonly int rows = 15, cols = 15;
        private PictureBox[,] pictureBoxes;
        private Timer gameTickTimer;
        private Label gameOverLabel;
        private Button startOverButton;
        private Label winLabel;
        private bool won=false;
        public Form1()
        {
            InitializeComponent();
            InitializeGameResources();
            gameState = new GameState(rows, cols);
            InitializeTableLayoutPanel();
            pictureBoxes = SetupGrid();  
            UpdateGrid();
            UpdateScore();

            gameTickTimer = new Timer();
            gameTickTimer.Interval = 300;  
            gameTickTimer.Tick += GameTick_Tick;
            gameTickTimer.Start();

            this.KeyDown += new KeyEventHandler(Form1_KeyDown);

            this.Resize += Form1_Resize;

            gameOverLabel = new Label
            {
                Text = "GAME OVER",
                ForeColor = Color.Red,
                Font = new Font("Arial", 24, FontStyle.Bold),
                BackColor = Color.Transparent,
                AutoSize = true,
                Visible = false  
            };
            winLabel = new Label
            {
                Text = "YOU WON :)!!!",
                ForeColor = Color.Green,
                Font = new Font("Arial", 24, FontStyle.Bold),
                BackColor = Color.Transparent,
                AutoSize = true,
                Visible = false
            };
            this.Controls.Add(gameOverLabel);
            this.Controls.Add(winLabel);

            //startOver
            startOverButton = new Button
            {
                Text = "Start Over",
                ForeColor = Color.Red,
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.LightGray,
                AutoSize = true,
                Visible = false
            };
            startOverButton.Click += StartOverButton_Click;
            this.Controls.Add(startOverButton);

            this.Controls.SetChildIndex(gameOverLabel, 0);
            this.Controls.SetChildIndex(startOverButton, 1);
            this.Controls.SetChildIndex(winLabel, 2);
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
                gameState.ChangeDirection(direction);
                moveOnTick();
            }
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
            PictureBox[,] pictureBoxes = new PictureBox[rows, cols];
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

                    pictureBoxes[r, c] = pictureBox;
                }
            }

            return pictureBoxes;
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

        private void moveOnTick()
        {
            gameState.Move();
            UpdateGrid();
            UpdateScore();
            if (!gameState.GameOver)
            {
                if (gameState.CheckWinCondition())
                {
                    gameTickTimer.Stop();
                    won = true;
                    ShowGameOverText();
                }
            }
            else
            {
                gameTickTimer.Stop();
                won = false;
                ShowGameOverText();
            }
        }

        private void GameTick_Tick(object sender, EventArgs e)
        {
            moveOnTick();
        }

        private void ShowGameOverText()
        {
            if(!won){
                gameOverLabel.Visible = true;
                gameOverLabel.Left = (this.ClientSize.Width - gameOverLabel.Width) / 2;
                gameOverLabel.Top = (this.ClientSize.Height - gameOverLabel.Height) / 2;
            }
            else
            {
                winLabel.Visible = true;
                winLabel.Left = (this.ClientSize.Width - gameOverLabel.Width) / 2;
                winLabel.Top = (this.ClientSize.Height - gameOverLabel.Height) / 2;
            }

            startOverButton.Visible = true;
            startOverButton.Left = (this.ClientSize.Width - startOverButton.Width) / 2;
            startOverButton.Top = gameOverLabel.Bottom + 10;

            startOverButton.BringToFront();
        }
        private void StartOverButton_Click(object sender, EventArgs e)
        {
            gameState = new GameState(rows, cols);

            UpdateGrid();
            UpdateScore();

            gameOverLabel.Visible = false;
            startOverButton.Visible = false;
            winLabel.Visible = false;
            this.Focus();
            gameTickTimer.Start();

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            tableGrid.Invalidate(); 

            if (gameState.GameOver)
            {
                ShowGameOverText();
            }
        }
    }
}
