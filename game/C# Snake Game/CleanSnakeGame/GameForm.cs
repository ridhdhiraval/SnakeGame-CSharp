using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CleanSnakeGame
{
    public partial class GameForm : Form
    {
        private const int CellSize = 20;
        private const int GridWidth = 40;
        private const int GridHeight = 30;

        private List<Point> snake;
        private Point food;
        private List<Point> powerups;
        private List<Point> obstacles;
        private Dictionary<Point, DateTime> powerupTimers;
        private Dictionary<Point, DateTime> obstacleTimers;
        private Direction direction;
        private Direction nextDirection;
        private Timer gameTimer;
        private Random random;

        private int score;
        private int level;
        private int speed;
        private bool gameRunning;
        private bool gamePaused;

        // UI Controls
        private Label lblScore;
        private Label lblBest;
        private Label lblLevel;
        private Label lblSpeed;
        private Label lblPlayer;
        private Label lblControls;
        private Panel gamePanel;
        private Panel pausePanel;
        private Button btnResume;
        private Button btnRestart;
        private Button btnMainMenu;

        public GameForm()
        {
            // Initialize fields first
            random = new Random();
            snake = new List<Point>();
            powerups = new List<Point>();
            obstacles = new List<Point>();
            powerupTimers = new Dictionary<Point, DateTime>();
            obstacleTimers = new Dictionary<Point, DateTime>();
            gameTimer = new Timer();
            
            SetupForm();
            CreateControls();
            InitializeGame();
        }

        private void SetupForm()
        {
            this.Text = "Ultimate Snake Game";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 35, 45);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.KeyPreview = true;
            this.KeyDown += GameForm_KeyDown;
            
            // Apply fullscreen setting
            ApplyFullscreenSetting();
            
            // Enable double buffering to prevent flickering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.UpdateStyles();
        }

        private void ApplyFullscreenSetting()
        {
            if (SettingsManager.Settings.Fullscreen)
            {
                // Go fullscreen
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = true;
            }
            else
            {
                // Exit fullscreen
                this.TopMost = false;
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;
                this.Size = new Size(1024, 768);
                this.CenterToScreen();
            }
            
            // Refresh the form to apply changes
            this.Refresh();
        }

        private void CreateControls()
        {
            // Status bar with clean layout
            int statusY = 15;
            lblScore = CreateStatusLabel("Score: 0", 20, statusY, Color.White);
            lblBest = CreateStatusLabel("Best: 0", 120, statusY, Color.FromArgb(76, 175, 80));
            lblLevel = CreateStatusLabel("Level: 1", 400, statusY, Color.FromArgb(33, 150, 243));
            lblSpeed = CreateStatusLabel("Speed: 10", 500, statusY, Color.Gray);
            lblPlayer = CreateStatusLabel("Player: Player", 750, statusY, Color.White);
            lblControls = CreateStatusLabel("WASD/Arrows: Move | ESC: Menu", 750, statusY + 20, Color.Gray);

            // Game panel with proper sizing and positioning
            gamePanel = new Panel
            {
                Size = new Size(GridWidth * CellSize, GridHeight * CellSize),
                Location = new Point((this.ClientSize.Width - GridWidth * CellSize) / 2, 70),
                BackColor = Color.FromArgb(20, 25, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Enable double buffering for game panel to eliminate glitches
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, gamePanel, new object[] { true });
            
            gamePanel.Paint += GamePanel_Paint;
            this.Controls.Add(gamePanel);

            // Pause panel (initially hidden)
            CreatePausePanel();
        }

        private Label CreateStatusLabel(string text, int x, int y, Color color)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(x, y)
            };
            this.Controls.Add(label);
            return label;
        }

        private void CreatePausePanel()
        {
            pausePanel = new Panel
            {
                Size = new Size(300, 200),
                BackColor = Color.FromArgb(200, Color.Black),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            var lblPaused = new Label
            {
                Text = "GAME PAUSED",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(80, 20)
            };
            pausePanel.Controls.Add(lblPaused);

            btnResume = CreatePauseButton("RESUME", 50, Color.FromArgb(76, 175, 80));
            btnResume.Click += BtnResume_Click;
            pausePanel.Controls.Add(btnResume);

            btnRestart = CreatePauseButton("RESTART", 90, Color.FromArgb(255, 152, 0));
            btnRestart.Click += BtnRestart_Click;
            pausePanel.Controls.Add(btnRestart);

            btnMainMenu = CreatePauseButton("MAIN MENU", 130, Color.FromArgb(244, 67, 54));
            btnMainMenu.Click += BtnMainMenu_Click;
            pausePanel.Controls.Add(btnMainMenu);

            this.Controls.Add(pausePanel);
            CenterPausePanel();
        }

        private Button CreatePauseButton(string text, int y, Color color)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(200, 30),
                Location = new Point(50, y),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void CenterPausePanel()
        {
            pausePanel.Location = new Point(
                (this.ClientSize.Width - pausePanel.Width) / 2,
                (this.ClientSize.Height - pausePanel.Height) / 2
            );
        }

        private void InitializeGame()
        {
            random = new Random();
            snake = new List<Point> 
            { 
                new Point(10, 10),  // Head
                new Point(9, 10),   // Body segment 1
                new Point(8, 10)    // Body segment 2
            };
            direction = Direction.Right;
            nextDirection = Direction.Right;
            GenerateFood();
            
            // Clear and generate powerups/obstacles based on settings
            powerups.Clear();
            obstacles.Clear();
            powerupTimers.Clear();
            obstacleTimers.Clear();
            
            score = 0;
            level = 1;
            speed = SettingsManager.Settings.GetGameSpeed(); // Use settings for speed
            gameTimer.Interval = speed; // Apply the speed immediately
            gameRunning = true;
            gamePaused = false;

            // Setup game timer properly
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Dispose();
            }
            
            gameTimer = new Timer();
            gameTimer.Interval = speed;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
            
            // Ensure form has focus for key handling
            this.Focus();
            this.KeyPreview = true;
            this.Activate();

            UpdateUI();
        }

        private void SetupTimer()
        {
            gameTimer = new Timer { Interval = speed };
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!gameRunning || gamePaused || snake == null || snake.Count == 0) return;

            // Update direction
            direction = nextDirection;

            // Move snake
            Point head = snake[0];
            Point newHead = direction switch
            {
                Direction.Up => new Point(head.X, head.Y - 1),
                Direction.Down => new Point(head.X, head.Y + 1),
                Direction.Left => new Point(head.X - 1, head.Y),
                Direction.Right => new Point(head.X + 1, head.Y),
                _ => head
            };

            // Handle wall wrapping (snake goes through walls)
            if (newHead.X < 0)
                newHead.X = GridWidth - 1;
            else if (newHead.X >= GridWidth)
                newHead.X = 0;
            
            if (newHead.Y < 0)
                newHead.Y = GridHeight - 1;
            else if (newHead.Y >= GridHeight)
                newHead.Y = 0;

            snake.Insert(0, newHead);

            // Check only self collision (not wall collision)
            if (CheckSelfCollision())
            {
                GameOver();
                return;
            }

            // Check food consumption
            if (newHead.Equals(food))
            {
                score += 10;
                GenerateFood();

                // Level up every 100 points
                if (score / 100 + 1 > level)
                {
                    level++;
                    speed = Math.Max(50, speed - 10);
                    gameTimer.Interval = speed;
                }
            }
            // Check powerup consumption
            else if (SettingsManager.Settings.PowerupsEnabled && powerups.Contains(newHead))
            {
                powerups.Remove(newHead);
                powerupTimers.Remove(newHead);
                score += 25; // Powerup gives more points
            }
            // Check obstacle collision
            else if (SettingsManager.Settings.ObstaclesEnabled && obstacles.Contains(newHead))
            {
                GameOver();
                return;
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }

            // Manage dynamic powerups and obstacles
            ManageDynamicItems();

            // Check self-collision (only after snake has grown)
            if (snake.Count > 1 && snake.GetRange(1, snake.Count - 1).Contains(newHead))
            {
                GameOver();
                return;
            }

            UpdateUI();
            gamePanel.Invalidate();
        }

        private bool CheckSelfCollision()
        {
            if (snake == null || snake.Count <= 1) return false;
            
            Point head = snake[0];
            // Check only self collision (snake hitting itself)
            return snake.Skip(1).Any(segment => segment.Equals(head));
        }

        private void GenerateFood()
        {
            if (random == null || snake == null) return;
            
            do
            {
                food = new Point(random.Next(GridWidth), random.Next(GridHeight));
            }
            while (snake.Contains(food) || powerups.Contains(food) || obstacles.Contains(food));
        }

        private void GeneratePowerups()
        {
            if (random == null || snake == null) return;
            
            // Generate 2-3 powerups
            int powerupCount = random.Next(2, 4);
            for (int i = 0; i < powerupCount; i++)
            {
                Point powerup;
                do
                {
                    powerup = new Point(random.Next(GridWidth), random.Next(GridHeight));
                }
                while (snake.Contains(powerup) || powerups.Contains(powerup) || obstacles.Contains(powerup) || powerup.Equals(food));
                
                powerups.Add(powerup);
            }
        }

        private void GenerateObstacles()
        {
            if (random == null || snake == null) return;
            
            // Generate 3-5 obstacles
            int obstacleCount = random.Next(3, 6);
            for (int i = 0; i < obstacleCount; i++)
            {
                Point obstacle;
                do
                {
                    obstacle = new Point(random.Next(GridWidth), random.Next(GridHeight));
                }
                while (snake.Contains(obstacle) || powerups.Contains(obstacle) || obstacles.Contains(obstacle) || obstacle.Equals(food));
                
                obstacles.Add(obstacle);
            }
        }

        private void ManageDynamicItems()
        {
            DateTime currentTime = DateTime.Now;

            // Remove expired powerups (disappear after 8 seconds)
            if (SettingsManager.Settings.PowerupsEnabled)
            {
                var expiredPowerups = powerupTimers.Where(kvp => (currentTime - kvp.Value).TotalSeconds > 8).Select(kvp => kvp.Key).ToList();
                foreach (var expiredPowerup in expiredPowerups)
                {
                    powerups.Remove(expiredPowerup);
                    powerupTimers.Remove(expiredPowerup);
                }

                // Spawn powerups extremely rarely and only one at a time (0.5% chance each tick)
                if (powerups.Count == 0 && random.Next(0, 1000) < 5)
                {
                    Point newPowerup;
                    int attempts = 0;
                    do
                    {
                        newPowerup = new Point(random.Next(GridWidth), random.Next(GridHeight));
                        attempts++;
                    }
                    while ((snake.Contains(newPowerup) || powerups.Contains(newPowerup) || obstacles.Contains(newPowerup) || newPowerup.Equals(food)) && attempts < 50);
                    
                    if (attempts < 50)
                    {
                        powerups.Add(newPowerup);
                        powerupTimers[newPowerup] = currentTime;
                    }
                }
            }

            // Remove expired obstacles (disappear after 12 seconds)
            if (SettingsManager.Settings.ObstaclesEnabled)
            {
                var expiredObstacles = obstacleTimers.Where(kvp => (currentTime - kvp.Value).TotalSeconds > 12).Select(kvp => kvp.Key).ToList();
                foreach (var expiredObstacle in expiredObstacles)
                {
                    obstacles.Remove(expiredObstacle);
                    obstacleTimers.Remove(expiredObstacle);
                }

                // Only spawn obstacles after score reaches 60-70 range
                // And spawn them rarely (2% chance each tick, max 2 at a time)
                if (score >= 60 && obstacles.Count < 2 && random.Next(0, 100) < 2) // Score 60+ means good gameplay progress
                {
                    Point newObstacle;
                    int attempts = 0;
                    do
                    {
                        newObstacle = new Point(random.Next(GridWidth), random.Next(GridHeight));
                        attempts++;
                    }
                    while ((snake.Contains(newObstacle) || powerups.Contains(newObstacle) || obstacles.Contains(newObstacle) || newObstacle.Equals(food)) && attempts < 50);
                    
                    if (attempts < 50)
                    {
                        obstacles.Add(newObstacle);
                        obstacleTimers[newObstacle] = currentTime;
                    }
                }
            }
        }

        private void UpdateUI()
        {
            lblScore.Text = $"Score: {score}";
            lblBest.Text = $"Best: {SettingsManager.Settings.BestScore}";
            lblLevel.Text = $"Level: {level}";
            lblSpeed.Text = $"Speed: {speed}ms";
            lblPlayer.Text = $"Player: {SettingsManager.Settings.PlayerName}";
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            
            // Optimize rendering to reduce glitches
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;

            // Clear background
            g.Clear(Color.FromArgb(20, 25, 35));

            // Draw grid (based on settings)
            if (SettingsManager.Settings.ShowGrid)
            {
                using (var pen = new Pen(Color.FromArgb(40, Color.Gray), 1))
                {
                    for (int x = 0; x <= GridWidth; x++)
                    {
                        g.DrawLine(pen, x * CellSize, 0, x * CellSize, GridHeight * CellSize);
                    }
                    for (int y = 0; y <= GridHeight; y++)
                    {
                        g.DrawLine(pen, 0, y * CellSize, GridWidth * CellSize, y * CellSize);
                    }
                }
            }

            // Draw snake with clean modern design
            if (snake != null && snake.Count > 0)
            {
                for (int i = 0; i < snake.Count; i++)
                {
                    Point segment = snake[i];
                    Rectangle rect = new Rectangle(
                        segment.X * CellSize + 1,
                        segment.Y * CellSize + 1,
                        CellSize - 2,
                        CellSize - 2
                    );

                    if (i == 0) // Head - Use settings color
                    {
                        var headColor = SettingsManager.Settings.GetSnakeColor();
                        
                        // Draw head with rounded corners effect
                        using (var brush = new SolidBrush(headColor))
                        {
                            g.FillRectangle(brush, rect);
                        }
                        
                        // Add border to head
                        using (var pen = new Pen(Color.FromArgb(
                            Math.Max(0, headColor.R - 50), 
                            Math.Max(0, headColor.G - 50), 
                            Math.Max(0, headColor.B - 50)), 2))
                        {
                            g.DrawRectangle(pen, rect);
                        }
                        
                        // Draw simple eyes
                        using (var eyeBrush = new SolidBrush(Color.Black))
                        {
                            int eyeSize = 3;
                            g.FillRectangle(eyeBrush, rect.X + 4, rect.Y + 4, eyeSize, eyeSize);
                            g.FillRectangle(eyeBrush, rect.X + rect.Width - 7, rect.Y + 4, eyeSize, eyeSize);
                        }
                        
                        // Add small white highlight
                        using (var highlightBrush = new SolidBrush(Color.FromArgb(150, Color.White)))
                        {
                            g.FillRectangle(highlightBrush, rect.X + 2, rect.Y + 2, 4, 2);
                        }
                    }
                    else // Body - Use darker version of settings color
                    {
                        var headColor = SettingsManager.Settings.GetSnakeColor();
                        var bodyColor = Color.FromArgb(
                            Math.Max(0, headColor.R - 80),
                            Math.Max(0, headColor.G - 80),
                            Math.Max(0, headColor.B - 80)
                        );
                        
                        // Draw body segments with clean design
                        using (var brush = new SolidBrush(bodyColor))
                        {
                            g.FillRectangle(brush, rect);
                        }
                        
                        // Add subtle border
                        using (var pen = new Pen(Color.FromArgb(
                            Math.Max(0, bodyColor.R - 40),
                            Math.Max(0, bodyColor.G - 40),
                            Math.Max(0, bodyColor.B - 40)), 1))
                        {
                            g.DrawRectangle(pen, rect);
                        }
                        
                        // Add small highlight for depth
                        using (var highlightBrush = new SolidBrush(Color.FromArgb(80, Color.White)))
                        {
                            g.FillRectangle(highlightBrush, rect.X + 1, rect.Y + 1, 3, 1);
                        }
                    }
                }
            }

            // Draw food as attractive circular apple-like design
            Rectangle foodRect = new Rectangle(
                food.X * CellSize + 2,
                food.Y * CellSize + 2,
                CellSize - 4,
                CellSize - 4
            );

            // Draw main food circle (red apple)
            using (var brush = new SolidBrush(Color.FromArgb(255, 60, 60)))
            {
                g.FillEllipse(brush, foodRect);
            }
            using (var pen = new Pen(Color.FromArgb(200, 40, 40), 2))
            {
                g.DrawEllipse(pen, foodRect);
            }
            
            // Add shine effect to food (makes it look juicy)
            using (var shineBrush = new SolidBrush(Color.FromArgb(120, Color.White)))
            {
                Rectangle shineRect = new Rectangle(foodRect.X + 3, foodRect.Y + 3, 6, 6);
                g.FillEllipse(shineBrush, shineRect);
            }
            
            // Add small highlight dot
            using (var highlightBrush = new SolidBrush(Color.FromArgb(180, Color.White)))
            {
                Rectangle highlightRect = new Rectangle(foodRect.X + 4, foodRect.Y + 4, 3, 3);
                g.FillEllipse(highlightBrush, highlightRect);
            }

            // Draw powerups if enabled
            if (SettingsManager.Settings.PowerupsEnabled && powerups != null)
            {
                foreach (var powerup in powerups)
                {
                    Rectangle powerupRect = new Rectangle(
                        powerup.X * CellSize + 1,
                        powerup.Y * CellSize + 1,
                        CellSize - 2,
                        CellSize - 2
                    );

                    // Draw powerup as golden star
                    using (var brush = new SolidBrush(Color.FromArgb(255, 215, 0)))
                    {
                        g.FillEllipse(brush, powerupRect);
                    }
                    using (var pen = new Pen(Color.FromArgb(255, 165, 0), 2))
                    {
                        g.DrawEllipse(pen, powerupRect);
                    }
                    
                    // Add sparkle effect
                    using (var sparkleBrush = new SolidBrush(Color.White))
                    {
                        g.FillEllipse(sparkleBrush, powerupRect.X + 4, powerupRect.Y + 4, 4, 4);
                    }
                }
            }

            // Draw obstacles if enabled
            if (SettingsManager.Settings.ObstaclesEnabled && obstacles != null)
            {
                foreach (var obstacle in obstacles)
                {
                    Rectangle obstacleRect = new Rectangle(
                        obstacle.X * CellSize + 1,
                        obstacle.Y * CellSize + 1,
                        CellSize - 2,
                        CellSize - 2
                    );

                    // Draw obstacle as dark red block
                    using (var brush = new SolidBrush(Color.FromArgb(139, 69, 19)))
                    {
                        g.FillRectangle(brush, obstacleRect);
                    }
                    using (var pen = new Pen(Color.FromArgb(101, 67, 33), 2))
                    {
                        g.DrawRectangle(pen, obstacleRect);
                    }
                    
                    // Add texture lines
                    using (var texturePen = new Pen(Color.FromArgb(160, 82, 45), 1))
                    {
                        g.DrawLine(texturePen, obstacleRect.X + 2, obstacleRect.Y + 2, obstacleRect.Right - 2, obstacleRect.Y + 2);
                        g.DrawLine(texturePen, obstacleRect.X + 2, obstacleRect.Bottom - 2, obstacleRect.Right - 2, obstacleRect.Bottom - 2);
                    }
                }
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle F11 for fullscreen toggle (works anytime)
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullscreen();
                return;
            }

            if (!gameRunning || gamePaused) 
            {
                if (e.KeyCode == Keys.Escape && gamePaused)
                {
                    ShowPauseMenu();
                }
                return;
            }

            Direction newDirection = direction;

            switch (e.KeyCode)
            {
                case Keys.W:
                case Keys.Up:
                    if (direction != Direction.Down) newDirection = Direction.Up;
                    break;
                case Keys.S:
                case Keys.Down:
                    if (direction != Direction.Up) newDirection = Direction.Down;
                    break;
                case Keys.A:
                case Keys.Left:
                    if (direction != Direction.Right) newDirection = Direction.Left;
                    break;
                case Keys.D:
                case Keys.Right:
                    if (direction != Direction.Left) newDirection = Direction.Right;
                    break;
                case Keys.Space:
                    TogglePause();
                    return;
                case Keys.Escape:
                    ShowPauseMenu();
                    return;
            }

            nextDirection = newDirection;
        }

        private void ToggleFullscreen()
        {
            SettingsManager.Settings.Fullscreen = !SettingsManager.Settings.Fullscreen;
            SettingsManager.SaveSettings();
            ApplyFullscreenSetting();
        }

        private void TogglePause()
        {
            if (!gameRunning) return;

            gamePaused = !gamePaused;
            pausePanel.Visible = gamePaused;

            if (gamePaused)
            {
                gameTimer.Stop();
            }
            else
            {
                gameTimer.Start();
                this.Focus(); // Restore focus to form for key handling
                this.KeyPreview = true; // Ensure key preview is enabled
            }
        }

        private void ShowPauseMenu()
        {
            if (!gameRunning) return;

            gamePaused = true;
            gameTimer.Stop();
            
            // Ensure form has focus and key preview is enabled
            this.Focus();
            this.KeyPreview = true;

            // Create pause menu panel
            var pauseMenuPanel = new Panel
            {
                Size = new Size(350, 400),
                BackColor = Color.FromArgb(40, 45, 55),
                BorderStyle = BorderStyle.None,
                Location = new Point((this.Width - 350) / 2, (this.Height - 400) / 2)
            };

            // Add border effect
            pauseMenuPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // Draw background
                using (var brush = new SolidBrush(Color.FromArgb(40, 45, 55)))
                {
                    var rect = new Rectangle(0, 0, pauseMenuPanel.Width, pauseMenuPanel.Height);
                    g.FillRectangle(brush, rect);
                }
                
                // Draw border
                using (var pen = new Pen(Color.FromArgb(70, 130, 255), 3))
                {
                    var rect = new Rectangle(1, 1, pauseMenuPanel.Width - 2, pauseMenuPanel.Height - 2);
                    g.DrawRectangle(pen, rect);
                }
            };

            // Title
            var titleLabel = new Label
            {
                Text = "GAME PAUSED",
                Font = new Font("Arial", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 130, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(330, 40),
                Location = new Point(10, 20)
            };

            // Resume button
            var resumeButton = new Button
            {
                Text = "RESUME",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(250, 50),
                Location = new Point(50, 80),
                BackColor = Color.FromArgb(50, 200, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            resumeButton.FlatAppearance.BorderSize = 0;
            resumeButton.Click += (s, e) =>
            {
                this.Controls.Remove(pauseMenuPanel);
                gamePaused = false;
                gameTimer.Start();
                this.Focus(); // Restore focus to form for key handling
                this.KeyPreview = true; // Ensure key preview is enabled
            };

            // Settings button
            var settingsButton = new Button
            {
                Text = "SETTINGS",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(250, 50),
                Location = new Point(50, 150),
                BackColor = Color.FromArgb(70, 130, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            settingsButton.FlatAppearance.BorderSize = 0;
            settingsButton.Click += (s, e) =>
            {
                this.Controls.Remove(pauseMenuPanel);
                var settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
                // Resume game after settings
                gamePaused = false;
                gameTimer.Start();
                this.Focus(); // Restore focus to form for key handling
                this.KeyPreview = true; // Ensure key preview is enabled
            };

            // Main Menu button
            var mainMenuButton = new Button
            {
                Text = "MAIN MENU",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(250, 50),
                Location = new Point(50, 220),
                BackColor = Color.FromArgb(255, 165, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            mainMenuButton.FlatAppearance.BorderSize = 0;
            mainMenuButton.Click += (s, e) =>
            {
                this.Controls.Remove(pauseMenuPanel);
                ReturnToMainMenu();
            };

            // Quit button
            var quitButton = new Button
            {
                Text = "QUIT GAME",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(250, 50),
                Location = new Point(50, 290),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            quitButton.FlatAppearance.BorderSize = 0;
            quitButton.Click += (s, e) =>
            {
                Application.Exit();
            };

            // Add hover effects
            resumeButton.MouseEnter += (s, e) => resumeButton.BackColor = Color.FromArgb(70, 220, 70);
            resumeButton.MouseLeave += (s, e) => resumeButton.BackColor = Color.FromArgb(50, 200, 50);
            
            settingsButton.MouseEnter += (s, e) => settingsButton.BackColor = Color.FromArgb(90, 150, 255);
            settingsButton.MouseLeave += (s, e) => settingsButton.BackColor = Color.FromArgb(70, 130, 255);
            
            mainMenuButton.MouseEnter += (s, e) => mainMenuButton.BackColor = Color.FromArgb(255, 185, 20);
            mainMenuButton.MouseLeave += (s, e) => mainMenuButton.BackColor = Color.FromArgb(255, 165, 0);
            
            quitButton.MouseEnter += (s, e) => quitButton.BackColor = Color.FromArgb(220, 70, 70);
            quitButton.MouseLeave += (s, e) => quitButton.BackColor = Color.FromArgb(200, 50, 50);

            // Add controls to panel
            pauseMenuPanel.Controls.Add(titleLabel);
            pauseMenuPanel.Controls.Add(resumeButton);
            pauseMenuPanel.Controls.Add(settingsButton);
            pauseMenuPanel.Controls.Add(mainMenuButton);
            pauseMenuPanel.Controls.Add(quitButton);

            // Add panel to form and bring to front
            this.Controls.Add(pauseMenuPanel);
            pauseMenuPanel.BringToFront();
        }

        private void BtnResume_Click(object sender, EventArgs e)
        {
            TogglePause();
            this.Focus(); // Restore focus to form for key handling
            this.KeyPreview = true; // Ensure key preview is enabled
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            RestartGame();
        }

        private void BtnMainMenu_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RestartGame()
        {
            gameTimer?.Stop();
            InitializeGame();
            pausePanel.Visible = false;
            gamePaused = false;
        }

        private void GameOver()
        {
            gameRunning = false;
            gameTimer.Stop();
            
            // Update best score in settings
            if (score > SettingsManager.Settings.BestScore)
            {
                SettingsManager.Settings.BestScore = score;
                SettingsManager.SaveSettings();
            }
            
            // Record score in database (non-blocking in case of errors)
            try
            {
                Database.AddScore(SettingsManager.Settings.PlayerName, score, level, DateTime.Now);
            }
            catch (Exception ex)
            {
                // Log to console; avoid interrupting UX
                Console.WriteLine($"Failed to save score: {ex.Message}");
            }
            
            UpdateUI();
            
            // Show custom game over dialog
            ShowGameOverDialog();
        }

        private void ShowGameOverDialog()
        {
            // Create custom game over dialog
            var gameOverPanel = new Panel
            {
                Size = new Size(400, 300),
                BackColor = Color.FromArgb(40, 45, 55),
                BorderStyle = BorderStyle.None,
                Location = new Point((this.Width - 400) / 2, (this.Height - 300) / 2)
            };

            // Add rounded border effect
            gameOverPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // Draw background with rounded corners
                using (var brush = new SolidBrush(Color.FromArgb(40, 45, 55)))
                {
                    var rect = new Rectangle(0, 0, gameOverPanel.Width, gameOverPanel.Height);
                    g.FillRectangle(brush, rect);
                }
                
                // Draw border
                using (var pen = new Pen(Color.FromArgb(70, 130, 255), 3))
                {
                    var rect = new Rectangle(1, 1, gameOverPanel.Width - 2, gameOverPanel.Height - 2);
                    g.DrawRectangle(pen, rect);
                }
            };

            // Game Over title
            var titleLabel = new Label
            {
                Text = "GAME OVER",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 100, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(380, 50),
                Location = new Point(10, 20)
            };

            // Score info
            var scoreLabel = new Label
            {
                Text = $"Score: {score}\nBest Score: {SettingsManager.Settings.BestScore}\nLevel Reached: {level}",
                Font = new Font("Arial", 14, FontStyle.Regular),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(380, 80),
                Location = new Point(10, 80)
            };

            // Restart question
            var questionLabel = new Label
            {
                Text = "Do you want to play again?",
                Font = new Font("Arial", 16, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 200, 200),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(380, 30),
                Location = new Point(10, 170)
            };

            // Yes button
            var yesButton = new Button
            {
                Text = "YES",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(120, 50),
                Location = new Point(80, 220),
                BackColor = Color.FromArgb(50, 200, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            yesButton.FlatAppearance.BorderSize = 0;
            yesButton.Click += (s, e) =>
            {
                this.Controls.Remove(gameOverPanel);
                RestartGame();
            };

            // No button
            var noButton = new Button
            {
                Text = "NO",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(120, 50),
                Location = new Point(220, 220),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            noButton.FlatAppearance.BorderSize = 0;
            noButton.Click += (s, e) =>
            {
                this.Controls.Remove(gameOverPanel);
                ReturnToMainMenu();
            };

            // Add hover effects
            yesButton.MouseEnter += (s, e) => yesButton.BackColor = Color.FromArgb(70, 220, 70);
            yesButton.MouseLeave += (s, e) => yesButton.BackColor = Color.FromArgb(50, 200, 50);
            noButton.MouseEnter += (s, e) => noButton.BackColor = Color.FromArgb(220, 70, 70);
            noButton.MouseLeave += (s, e) => noButton.BackColor = Color.FromArgb(200, 50, 50);

            // Add controls to panel
            gameOverPanel.Controls.Add(titleLabel);
            gameOverPanel.Controls.Add(scoreLabel);
            gameOverPanel.Controls.Add(questionLabel);
            gameOverPanel.Controls.Add(yesButton);
            gameOverPanel.Controls.Add(noButton);

            // Add panel to form and bring to front
            this.Controls.Add(gameOverPanel);
            gameOverPanel.BringToFront();
        }

        private void ReturnToMainMenu()
        {
            this.Hide();
            var mainMenu = new MainMenuForm();
            mainMenu.Show();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            gameTimer?.Stop();
            gameTimer?.Dispose();
            base.OnFormClosed(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1024, 768);
            this.Name = "GameForm";
            this.Text = "Ultimate Snake Game - Playing";
            this.ResumeLayout(false);
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
