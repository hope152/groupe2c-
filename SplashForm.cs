using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KeyceWordLite
{
    public class SplashForm : Form
    {
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Timer fadeTimer;

        private ProgressBar progressBar;
        private Label lblTitle;

        private int progress = 0;
        private int fadeDirection = +1; // +1 = fade-in, -1 = fade-out
        private const int DURATION_MS = 3500;
        private const int TIMER_INTERVAL = 50;

        public SplashForm()
        {
            InitializeComponent();
            StartTimer();
            StartFadeAnimation();
        }

        private void InitializeComponent()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(420, 250);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.RoyalBlue;

            lblTitle = new Label()
            {
                Text = "WORD KEYCE",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 90
            };

            // Commence invisible
            lblTitle.ForeColor = Color.FromArgb(0, 255, 255, 255);

            // ProgressBar
            progressBar = new ProgressBar()
            {
                Style = ProgressBarStyle.Continuous,
                Location = new Point(30, 180),
                Size = new Size(360, 22),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(progressBar);
        }

        private void StartTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = TIMER_INTERVAL;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void StartFadeAnimation()
        {
            fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Interval = 30; // vitesse animation
            fadeTimer.Tick += Fade_Tick;
            fadeTimer.Start();
        }

        private void Fade_Tick(object sender, EventArgs e)
        {
            Color c = lblTitle.ForeColor;

            int alpha = c.A + (fadeDirection * 10); // intensité du fade

            // limite 0 → 255
            alpha = Math.Max(0, Math.Min(255, alpha));

            lblTitle.ForeColor = Color.FromArgb(alpha, 255, 255, 255);

            // Quand complètement visible → commence fade-out
            if (alpha == 255)
                fadeDirection = -1;

            // Quand complètement invisible → recommence fade-in
            if (alpha == 0)
                fadeDirection = +1;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int increments = DURATION_MS / TIMER_INTERVAL;
            progress++;
            int val = (int)(progress * (100.0 / increments));
            progressBar.Value = Math.Min(progressBar.Maximum, Math.Max(progressBar.Minimum, val));

            if (progressBar.Value >= progressBar.Maximum)
            {
                timer.Stop();
                fadeTimer.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
