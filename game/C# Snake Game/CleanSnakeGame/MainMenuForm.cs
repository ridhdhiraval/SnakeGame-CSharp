using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CleanSnakeGame
{
    public partial class MainMenuForm : Form
    {
        private Panel centerPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnStartGame;
        private Button btnHighScores;
        private Button btnSettings;
        private Button btnQuit;
        private Label lblInstructions;

        public MainMenuForm()
        {
            InitializeComponent();
            SetupForm();
            CreateControls();
        }

        private void SetupForm()
        {
            this.Text = "Ultimate Snake Game";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 35, 45);
            this.KeyPreview = true;
            this.KeyDown += MainMenuForm_KeyDown;
        }

        private void CreateControls()
        {
            // Center Panel with blue border
            centerPanel = new Panel
            {
                Size = new Size(600, 500),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None
            };
            centerPanel.Paint += CenterPanel_Paint;
            this.Controls.Add(centerPanel);

            // Title
            lblTitle = new Label
            {
                Text = "🐍 SNAKE",
                Font = new Font("Arial", 48, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            centerPanel.Controls.Add(lblTitle);

            // Subtitle
            lblSubtitle = new Label
            {
                Text = "Modern Snake Game",
                Font = new Font("Arial", 16, FontStyle.Regular),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            centerPanel.Controls.Add(lblSubtitle);

            // Start Game Button
            btnStartGame = CreateMenuButton("START GAME", Color.FromArgb(76, 175, 80));
            btnStartGame.Click += BtnStartGame_Click;
            centerPanel.Controls.Add(btnStartGame);

            // High Scores Button
            btnHighScores = CreateMenuButton("HIGH SCORES", Color.FromArgb(255, 215, 0));
            btnHighScores.Click += BtnHighScores_Click;
            centerPanel.Controls.Add(btnHighScores);

            // Settings Button
            btnSettings = CreateMenuButton("SETTINGS", Color.FromArgb(33, 150, 243));
            btnSettings.Click += BtnSettings_Click;
            centerPanel.Controls.Add(btnSettings);

            // Quit Button
            btnQuit = CreateMenuButton("QUIT", Color.FromArgb(244, 67, 54));
            btnQuit.Click += BtnQuit_Click;
            centerPanel.Controls.Add(btnQuit);

            // Instructions
            lblInstructions = new Label
            {
                Text = "Use WASD or Arrow Keys to navigate\nClick buttons or press Enter to select\nPress F11 for fullscreen, ESC to quit",
                Font = new Font("Arial", 11, FontStyle.Regular),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(600, 60)
            };
            this.Controls.Add(lblInstructions);

            PositionControls();
        }

        private Button CreateMenuButton(string text, Color baseColor)
        {
            var button = new Button
            {
                Text = text,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Size = new Size(200, 50),
                FlatStyle = FlatStyle.Flat,
                BackColor = baseColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                TabStop = true
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.BorderColor = baseColor;

            // Hover effects
            button.MouseEnter += (s, e) =>
            {
                button.BackColor = ControlPaint.Light(baseColor, 0.2f);
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = baseColor;
            };

            return button;
        }

        private void CenterPanel_Paint(object sender, PaintEventArgs e)
        {
            // Draw blue border around the center panel
            using (var pen = new Pen(Color.FromArgb(33, 150, 243), 3))
            {
                Rectangle rect = new Rectangle(0, 0, centerPanel.Width - 1, centerPanel.Height - 1);
                DrawRoundedRectangle(e.Graphics, pen, rect, 15);
            }
        }

        private void DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle rect, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                graphics.DrawPath(pen, path);
            }
        }

        private void PositionControls()
        {
            // Center the main panel
            centerPanel.Location = new Point(
                (this.ClientSize.Width - centerPanel.Width) / 2,
                (this.ClientSize.Height - centerPanel.Height) / 2 - 50
            );

            // Position controls within the center panel
            int centerX = centerPanel.Width / 2;
            
            lblTitle.Location = new Point(centerX - lblTitle.Width / 2, 60);
            lblSubtitle.Location = new Point(centerX - lblSubtitle.Width / 2, 130);

            btnStartGame.Location = new Point(centerX - btnStartGame.Width / 2, 200);
            btnHighScores.Location = new Point(centerX - btnHighScores.Width / 2, 270);
            btnSettings.Location = new Point(centerX - btnSettings.Width / 2, 340);
            btnQuit.Location = new Point(centerX - btnQuit.Width / 2, 410);

            // Position instructions at bottom of form
            lblInstructions.Location = new Point(
                (this.ClientSize.Width - lblInstructions.Width) / 2, 
                this.ClientSize.Height - 100
            );
        }

        private void MainMenuForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (btnStartGame.Focused)
                        BtnStartGame_Click(sender, e);
                    else if (btnHighScores.Focused)
                        BtnHighScores_Click(sender, e);
                    else if (btnSettings.Focused)
                        BtnSettings_Click(sender, e);
                    else if (btnQuit.Focused)
                        BtnQuit_Click(sender, e);
                    else
                        BtnStartGame_Click(sender, e);
                    break;
                case Keys.Escape:
                    BtnQuit_Click(sender, e);
                    break;
                case Keys.Up:
                case Keys.W:
                    SelectPreviousButton();
                    break;
                case Keys.Down:
                case Keys.S:
                    SelectNextButton();
                    break;
            }
        }

        private void SelectNextButton()
        {
            if (btnStartGame.Focused)
                btnHighScores.Focus();
            else if (btnHighScores.Focused)
                btnSettings.Focus();
            else if (btnSettings.Focused)
                btnQuit.Focus();
            else
                btnStartGame.Focus();
        }

        private void SelectPreviousButton()
        {
            if (btnQuit.Focused)
                btnSettings.Focus();
            else if (btnSettings.Focused)
                btnHighScores.Focus();
            else if (btnHighScores.Focused)
                btnStartGame.Focus();
            else
                btnQuit.Focus();
        }

        private void BtnHighScores_Click(object sender, EventArgs e)
        {
            using var scoresForm = new HighScoresForm();
            scoresForm.ShowDialog(this);
        }

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            var gameForm = new GameForm();
            gameForm.FormClosed += (s, args) => this.Show();
            this.Hide();
            gameForm.Show();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
            settingsForm.FormClosed += (s, args) => this.Show();
            this.Hide();
            settingsForm.Show();
        }

        private void BtnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1024, 768);
            this.Name = "MainMenuForm";
            this.Text = "Ultimate Snake Game";
            this.ResumeLayout(false);
        }
    }
}
