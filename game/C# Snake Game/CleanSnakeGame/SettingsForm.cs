using System;
using System.Drawing;
using System.Windows.Forms;

namespace CleanSnakeGame
{
    public partial class SettingsForm : Form
    {
        private Label lblTitle;
        private Button btnDifficulty;
        private Button btnSnakeColor;
        private Button btnPlayerName;
        private Button btnPowerups;
        private Button btnObstacles;
        private Button btnSound;
        private Button btnShowGrid;
        private Button btnFullscreen;
        private Button btnBack;
        private Panel pnlSnakeColorPreview;

        private readonly Color[] snakeColors = { Color.Lime, Color.Red, Color.Blue, Color.Yellow, Color.Magenta, Color.Cyan };

        public SettingsForm()
        {
            InitializeComponent();
            SetupForm();
            CreateControls();
            UpdateStatusLabels();
            
            // Apply current fullscreen setting
            ApplyFullscreenToCurrentForm();
        }

        private void SetupForm()
        {
            this.Text = "Settings - Ultimate Snake Game";
            this.Size = new Size(1024, 800); // Increased height to prevent overlap
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 35, 45);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.KeyPreview = true;
            this.KeyDown += SettingsForm_KeyDown;
        }

        private void CreateControls()
        {
            // Title
            lblTitle = new Label
            {
                Text = "🐍 SETTINGS",
                Font = new Font("Arial", 48, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(350, 50)
            };
            this.Controls.Add(lblTitle);

            int yPos = 150;
            int spacing = 65;

            // Difficulty
            CreateSettingButton(ref btnDifficulty, "Difficulty", yPos, Color.FromArgb(33, 150, 243));
            btnDifficulty.Click += BtnDifficulty_Click;
            yPos += spacing;

            // Snake Color with preview
            CreateSettingButton(ref btnSnakeColor, "Snake Color", yPos, Color.FromArgb(76, 175, 80));
            btnSnakeColor.Click += BtnSnakeColor_Click;
            
            pnlSnakeColorPreview = new Panel
            {
                Size = new Size(60, 20),
                Location = new Point(550, yPos + 15),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlSnakeColorPreview.Paint += PnlSnakeColorPreview_Paint;
            this.Controls.Add(pnlSnakeColorPreview);
            yPos += spacing;

            // Player Name
            CreateSettingButton(ref btnPlayerName, "Player Name", yPos, Color.FromArgb(156, 39, 176));
            btnPlayerName.Click += BtnPlayerName_Click;
            yPos += spacing;

            // Powerups
            CreateSettingButton(ref btnPowerups, "Powerups", yPos, Color.FromArgb(255, 152, 0));
            btnPowerups.Click += BtnPowerups_Click;
            yPos += spacing;

            // Obstacles
            CreateSettingButton(ref btnObstacles, "Obstacles", yPos, Color.FromArgb(244, 67, 54));
            btnObstacles.Click += BtnObstacles_Click;
            yPos += spacing;

            // Sound
            CreateSettingButton(ref btnSound, "Sound", yPos, Color.FromArgb(255, 235, 59));
            btnSound.Click += BtnSound_Click;
            yPos += spacing;

            // Show Grid
            CreateSettingButton(ref btnShowGrid, "Show Grid", yPos, Color.FromArgb(33, 150, 243));
            btnShowGrid.Click += BtnShowGrid_Click;
            yPos += spacing;

            // Fullscreen
            CreateSettingButton(ref btnFullscreen, "Fullscreen", yPos, Color.FromArgb(76, 175, 80));
            btnFullscreen.Click += BtnFullscreen_Click;
            yPos += spacing; // Add spacing after fullscreen button

            // Back Button - positioned on right side for balanced layout
            btnBack = new Button
            {
                Text = "BACK",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(100, 40),
                Location = new Point(450, 730), // Centered at bottom
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += BtnBack_Click;
            this.Controls.Add(btnBack);

            // Instructions - positioned at bottom
            var lblInstructions = new Label
            {
                Text = "Click buttons to change settings • Use F11 for fullscreen • All settings are saved automatically",
                Font = new Font("Arial", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(150, 150, 150),
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(900, 40),
                Location = new Point(50, 720) // Bottom center
            };
            this.Controls.Add(lblInstructions);
        }

        private void CreateSettingButton(ref Button button, string text, int yPos, Color buttonColor)
        {
            button = new Button
            {
                Text = text,
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(200, 50),
                Location = new Point(50, yPos),
                BackColor = buttonColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            button.FlatAppearance.BorderSize = 0;
            this.Controls.Add(button);
        }

        private void UpdateStatusLabels()
        {
            // Remove existing status labels
            var labelsToRemove = new System.Collections.Generic.List<Control>();
            foreach (Control control in this.Controls)
            {
                if (control is Label lbl && (lbl.Text.StartsWith("Current:") || lbl.Text.StartsWith("Status:")))
                {
                    labelsToRemove.Add(control);
                }
            }
            foreach (var label in labelsToRemove)
            {
                this.Controls.Remove(label);
                label.Dispose();
            }

            // Create new status labels using SettingsManager
            CreateStatusLabel($"Current: {SettingsManager.Settings.Difficulty}", GetDifficultyDescription(), 150);
            CreateStatusLabel($"Current: {GetColorName(snakeColors[SettingsManager.Settings.SnakeColorIndex])}", "", 215);
            CreateStatusLabel($"Enter name: {SettingsManager.Settings.PlayerName}", "", 280);
            CreateStatusLabel($"Status: {(SettingsManager.Settings.PowerupsEnabled ? "ON" : "OFF")}", "", 345);
            CreateStatusLabel($"Status: {(SettingsManager.Settings.ObstaclesEnabled ? "ON" : "OFF")}", "", 410);
            CreateStatusLabel($"Status: {(SettingsManager.Settings.SoundEnabled ? "ON" : "OFF")}", "", 475);
            CreateStatusLabel($"Status: {(SettingsManager.Settings.ShowGrid ? "ON" : "OFF")}", "", 540);
            CreateStatusLabel($"Status: {(SettingsManager.Settings.Fullscreen ? "ON" : "OFF")}", "", 605);
        }

        private string GetDifficultyDescription()
        {
            return SettingsManager.Settings.Difficulty switch
            {
                "Easy" => "Relaxed pace",
                "Medium" => "Balanced challenge",
                "Hard" => "Fast & furious",
                _ => "Balanced challenge"
            };
        }

        private void CreateStatusLabel(string mainText, string subText, int yPos)
        {
            var lblMain = new Label
            {
                Text = mainText,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = mainText.Contains("OFF") ? Color.FromArgb(244, 67, 54) : Color.FromArgb(76, 175, 80),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(280, yPos + 10)
            };
            this.Controls.Add(lblMain);

            if (!string.IsNullOrEmpty(subText))
            {
                var lblSub = new Label
                {
                    Text = subText,
                    Font = new Font("Arial", 10, FontStyle.Regular),
                    ForeColor = Color.Gray,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(280, yPos + 30)
                };
                this.Controls.Add(lblSub);
            }
        }

        private string GetColorName(Color color)
        {
            if (color == Color.Lime) return "Emerald Green";
            if (color == Color.Red) return "Red";
            if (color == Color.Blue) return "Blue";
            if (color == Color.Yellow) return "Yellow";
            if (color == Color.Magenta) return "Magenta";
            if (color == Color.Cyan) return "Cyan";
            return "Custom";
        }

        private void PnlSnakeColorPreview_Paint(object sender, PaintEventArgs e)
        {
            int segmentWidth = pnlSnakeColorPreview.Width / 3;
            
            for (int i = 0; i < 3; i++)
            {
                Rectangle rect = new Rectangle(i * segmentWidth, 0, segmentWidth - 1, pnlSnakeColorPreview.Height - 1);
                using (var brush = new SolidBrush(snakeColors[SettingsManager.Settings.SnakeColorIndex]))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
                using (var pen = new Pen(Color.White))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        private void BtnDifficulty_Click(object sender, EventArgs e)
        {
            SettingsManager.Settings.Difficulty = SettingsManager.Settings.Difficulty switch
            {
                "Easy" => "Medium",
                "Medium" => "Hard",
                "Hard" => "Easy",
                _ => "Medium"
            };
            SettingsManager.SaveSettings();
            UpdateStatusLabels();
        }

        private void BtnSnakeColor_Click(object sender, EventArgs e)
        {
            SettingsManager.Settings.SnakeColorIndex = (SettingsManager.Settings.SnakeColorIndex + 1) % snakeColors.Length;
            SettingsManager.SaveSettings();
            pnlSnakeColorPreview.Invalidate();
            UpdateStatusLabels();
        }

        private void BtnPlayerName_Click(object sender, EventArgs e)
        {
            string newName = ShowInputDialog("Enter your player name:", "Player Name", SettingsManager.Settings.PlayerName);
            
            if (!string.IsNullOrWhiteSpace(newName) && newName != SettingsManager.Settings.PlayerName)
            {
                SettingsManager.Settings.PlayerName = newName;
                SettingsManager.SaveSettings();
                UpdateStatusLabels();
            }
        }

        private string ShowInputDialog(string text, string caption, string defaultValue)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(30, 35, 45)
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Width = 350, Text = text, ForeColor = Color.White };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 350, Text = defaultValue, BackColor = Color.DarkGray, ForeColor = Color.White };
            Button confirmation = new Button() { Text = "OK", Left = 250, Width = 100, Top = 90, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            Button cancel = new Button() { Text = "Cancel", Left = 130, Width = 100, Top = 90, DialogResult = DialogResult.Cancel, BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            confirmation.FlatAppearance.BorderSize = 0;
            cancel.FlatAppearance.BorderSize = 0;

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : defaultValue;
        }

        private void BtnPowerups_Click(object sender, EventArgs e)
        {
            SettingsManager.Settings.PowerupsEnabled = !SettingsManager.Settings.PowerupsEnabled;
            SettingsManager.SaveSettings();
            UpdateStatusLabels();
        }

        private void BtnObstacles_Click(object sender, EventArgs e)
        {
            SettingsManager.Settings.ObstaclesEnabled = !SettingsManager.Settings.ObstaclesEnabled;
            SettingsManager.SaveSettings();
            UpdateStatusLabels();
        }

        private void BtnSound_Click(object sender, EventArgs e)
        {
            SettingsManager.Settings.SoundEnabled = !SettingsManager.Settings.SoundEnabled;
            SettingsManager.SaveSettings();
            UpdateStatusLabels();
        }

        private void BtnShowGrid_Click(object sender, EventArgs e)
        {
            SettingsManager.Settings.ShowGrid = !SettingsManager.Settings.ShowGrid;
            SettingsManager.SaveSettings();
            UpdateStatusLabels();
        }

        private void BtnFullscreen_Click(object sender, EventArgs e)
        {
            SettingsManager.Settings.Fullscreen = !SettingsManager.Settings.Fullscreen;
            SettingsManager.SaveSettings();
            UpdateStatusLabels();
            
            // Apply fullscreen to this form immediately
            ApplyFullscreenToCurrentForm();
        }

        private void ApplyFullscreenToCurrentForm()
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
                this.Size = new Size(1024, 800); // Updated to match new form height
                this.CenterToScreen();
            }
            
            this.Refresh();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                BtnBack_Click(sender, e);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1024, 768);
            this.Name = "SettingsForm";
            this.Text = "Settings - Ultimate Snake Game";
            this.ResumeLayout(false);
        }
    }
}
