using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CleanSnakeGame
{
    public class HighScoresForm : Form
    {
        private ListView listView;
        private Button btnClose;

        public HighScoresForm()
        {
            InitializeComponent();
            LoadScores();
        }

        private void InitializeComponent()
        {
            this.Text = "High Scores - Ultimate Snake Game";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 35, 45);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            var title = new Label
            {
                Text = "🏆 HIGH SCORES",
                Font = new Font("Arial", 28, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(210, 20)
            };
            this.Controls.Add(title);

            listView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                Location = new Point(30, 90),
                Size = new Size(630, 320),
                BackColor = Color.FromArgb(40, 45, 55),
                ForeColor = Color.White,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            listView.Columns.Add("#", 40, HorizontalAlignment.Left);
            listView.Columns.Add("Player", 220, HorizontalAlignment.Left);
            listView.Columns.Add("Score", 120, HorizontalAlignment.Left);
            listView.Columns.Add("Level", 80, HorizontalAlignment.Left);
            listView.Columns.Add("Played At", 150, HorizontalAlignment.Left);
            this.Controls.Add(listView);

            btnClose = new Button
            {
                Text = "CLOSE",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(100, 36),
                Location = new Point((700 - 100) / 2 - 8, 425),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);

            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private void LoadScores()
        {
            List<HighScore> top = Database.GetTopScores(50);
            listView.BeginUpdate();
            listView.Items.Clear();

            int rank = 1;
            foreach (var hs in top)
            {
                var item = new ListViewItem(rank.ToString());
                item.SubItems.Add(hs.PlayerName);
                item.SubItems.Add(hs.Score.ToString());
                item.SubItems.Add(hs.Level.ToString());
                item.SubItems.Add(hs.PlayedAt.ToString("g"));
                listView.Items.Add(item);
                rank++;
            }

            listView.EndUpdate();
        }
    }
}
