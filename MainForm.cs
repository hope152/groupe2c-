using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KeyceWordLite
{
    public partial class MainForm : Form
    {
        private RichTextBox rtb;
        private MenuStrip menu;
        private ToolStrip tool;
        private StatusStrip status;
        private ToolStripStatusLabel lblStatus;
        private string currentFilePath = null;
        private OpenFileDialog openDlg;
        private SaveFileDialog saveDlg;
        private FontDialog fontDlg;
        private ColorDialog colorDlg;

        public MainForm()
        {
            InitializeComponent();
            LoadPossibleFileFromSplash();
            UpdateStatus();
        }

        private void InitializeComponent()
        {
            // ---- Form ----
            this.Text = "Keyce Word Lite";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(800, 500);
            this.BackColor = Color.FromArgb(245, 245, 245); // Fond gris clair

            // ---- MenuStrip ----
            menu = new MenuStrip
            {
                BackColor = Color.FromArgb(30, 144, 255), // Bleu dodger
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                RenderMode = ToolStripRenderMode.System
            };

            var fileMenu = new ToolStripMenuItem("Fichier");
            var newItem = new ToolStripMenuItem("Nouveau", null, (s, e) => NewFile());
            var openItem = new ToolStripMenuItem("Ouvrir...", null, (s, e) => OpenFile());
            var saveItem = new ToolStripMenuItem("Enregistrer", null, (s, e) => SaveFile());
            var saveAsItem = new ToolStripMenuItem("Enregistrer sous...", null, (s, e) => SaveAs());
            var exitItem = new ToolStripMenuItem("Quitter", null, (s, e) => this.Close());
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { newItem, openItem, saveItem, saveAsItem, new ToolStripSeparator(), exitItem });

            var editMenu = new ToolStripMenuItem("Édition");
            editMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Copier", null, (s,e) => rtb.Copy()),
                new ToolStripMenuItem("Couper", null, (s,e) => rtb.Cut()),
                new ToolStripMenuItem("Coller", null, (s,e) => rtb.Paste())
            });

            var formatMenu = new ToolStripMenuItem("Format");
            formatMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Police...", null, BtnFont_Click),
                new ToolStripMenuItem("Couleur du texte...", null, BtnTextColor_Click),
                new ToolStripMenuItem("Couleur du fond...", null, BtnBackColor_Click)
            });

            menu.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, formatMenu });
            this.MainMenuStrip = menu;
            this.Controls.Add(menu);

            // ---- ToolStrip ----
            tool = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = Color.FromArgb(240, 248, 255) // Blanc cassé
            };

            var tNew = new ToolStripButton("Nouveau") { ToolTipText = "Nouveau (Ctrl+N)" };
            tNew.Click += (s, e) => NewFile();
            var tOpen = new ToolStripButton("Ouvrir") { ToolTipText = "Ouvrir (Ctrl+O)" };
            tOpen.Click += (s, e) => OpenFile();
            var tSave = new ToolStripButton("Enregistrer") { ToolTipText = "Enregistrer (Ctrl+S)" };
            tSave.Click += (s, e) => SaveFile();

            var sep = new ToolStripSeparator();

            var boldBtn = new ToolStripButton("B") { CheckOnClick = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            boldBtn.Click += (s, e) => ToggleStyle(FontStyle.Bold, boldBtn.Checked);
            var italicBtn = new ToolStripButton("I") { CheckOnClick = true, Font = new Font("Segoe UI", 9F, FontStyle.Italic) };
            italicBtn.Click += (s, e) => ToggleStyle(FontStyle.Italic, italicBtn.Checked);
            var underBtn = new ToolStripButton("U") { CheckOnClick = true, Font = new Font("Segoe UI", 9F, FontStyle.Underline) };
            underBtn.Click += (s, e) => ToggleStyle(FontStyle.Underline, underBtn.Checked);

            var fontBtn = new ToolStripButton("Police...");
            fontBtn.Click += BtnFont_Click;

            var sizeCombo = new ToolStripComboBox() { Width = 60 };
            sizeCombo.Items.AddRange(new object[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72" });
            sizeCombo.TextChanged += (s, e) =>
            {
                if (float.TryParse(sizeCombo.Text, out float sz))
                    ApplyFontSize(sz);
            };

            var textColorBtn = new ToolStripButton("A") { ToolTipText = "Couleur du texte" };
            textColorBtn.Click += BtnTextColor_Click;
            var backColorBtn = new ToolStripButton("Bg") { ToolTipText = "Couleur du fond" };
            backColorBtn.Click += BtnBackColor_Click;

            var alignLeft = new ToolStripButton("G") { ToolTipText = "Aligner à gauche" };
            alignLeft.Click += (s, e) => rtb.SelectionAlignment = HorizontalAlignment.Left;
            var alignCenter = new ToolStripButton("C") { ToolTipText = "Centrer" };
            alignCenter.Click += (s, e) => rtb.SelectionAlignment = HorizontalAlignment.Center;
            var alignRight = new ToolStripButton("D") { ToolTipText = "Aligner à droite" };
            alignRight.Click += (s, e) => rtb.SelectionAlignment = HorizontalAlignment.Right;

            tool.Items.AddRange(new ToolStripItem[]
            {
                tNew, tOpen, tSave, sep,
                boldBtn, italicBtn, underBtn, fontBtn, sizeCombo, textColorBtn, backColorBtn, sep,
                alignLeft, alignCenter, alignRight
            });

            this.Controls.Add(tool);

            // ---- RichTextBox ----
            rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Black
            };
            rtb.TextChanged += (s, e) => UpdateStatus();
            this.Controls.Add(rtb);

            // ---- StatusStrip ----
            status = new StatusStrip
            {
                BackColor = Color.FromArgb(240, 248, 255)
            };
            lblStatus = new ToolStripStatusLabel("Lignes: 0    Caractères: 0") { ForeColor = Color.Black };
            status.Items.Add(lblStatus);
            this.Controls.Add(status);

            // ---- Dock ----
            menu.Dock = DockStyle.Top;
            tool.Dock = DockStyle.Top;
            status.Dock = DockStyle.Bottom;

            // ---- Dialogs ----
            openDlg = new OpenFileDialog { Filter = "RTF (*.rtf)|*.rtf|Texte (*.txt)|*.txt|Tous (*.*)|*.*" };
            saveDlg = new SaveFileDialog { Filter = "RTF (*.rtf)|*.rtf|Texte (*.txt)|*.txt|Tous (*.*)|*.*" };
            fontDlg = new FontDialog();
            colorDlg = new ColorDialog();

            // ---- Shortcuts ----
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N) NewFile();
            if (e.Control && e.KeyCode == Keys.O) OpenFile();
            if (e.Control && e.KeyCode == Keys.S) SaveFile();
        }

        // ---- Méthodes principales ----
        private void NewFile() { if (ConfirmSaveIfNeeded()) { rtb.Clear(); currentFilePath = null; this.Text = "Keyce Word Lite - Nouveau document"; UpdateStatus(); } }
        private void OpenFile() { if (!ConfirmSaveIfNeeded()) return; if (openDlg.ShowDialog() == DialogResult.OK) LoadFile(openDlg.FileName); }
        private void SaveFile() { if (string.IsNullOrEmpty(currentFilePath)) { SaveAs(); return; } SaveToPath(currentFilePath); }
        private void SaveAs() { if (saveDlg.ShowDialog() == DialogResult.OK) SaveToPath(saveDlg.FileName); }

        private void LoadFile(string path)
        {
            try
            {
                if (Path.GetExtension(path).ToLower() == ".rtf")
                    rtb.LoadFile(path, RichTextBoxStreamType.RichText);
                else
                    rtb.LoadFile(path, RichTextBoxStreamType.PlainText);
                currentFilePath = path;
                this.Text = $"Keyce Word Lite - {Path.GetFileName(path)}";
                UpdateStatus();
            }
            catch (Exception ex) { MessageBox.Show("Impossible d'ouvrir le fichier:\n" + ex.Message); }
        }

        private void SaveToPath(string path)
        {
            try
            {
                if (Path.GetExtension(path).ToLower() == ".rtf")
                    rtb.SaveFile(path, RichTextBoxStreamType.RichText);
                else
                    rtb.SaveFile(path, RichTextBoxStreamType.PlainText);
                currentFilePath = path;
                this.Text = $"Keyce Word Lite - {Path.GetFileName(path)}";
            }
            catch (Exception ex) { MessageBox.Show("Impossible d'enregistrer le fichier:\n" + ex.Message); }
        }

        private bool ConfirmSaveIfNeeded()
        {
            if (!string.IsNullOrEmpty(rtb.Text))
            {
                var dr = MessageBox.Show("Voulez-vous enregistrer le document actuel ?", "Enregistrer", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Yes) { SaveFile(); return true; }
                if (dr == DialogResult.No) return true;
                return false;
            }
            return true;
        }

        private void ToggleStyle(FontStyle style, bool enable)
        {
            Font current = rtb.SelectionFont ?? rtb.Font;
            FontStyle newStyle = current.Style;
            newStyle = enable ? newStyle | style : newStyle & ~style;
            try { rtb.SelectionFont = new Font(current.FontFamily, current.Size, newStyle); } catch { }
        }

        private void BtnFont_Click(object sender, EventArgs e)
        {
            fontDlg.Font = rtb.SelectionFont ?? rtb.Font;
            if (fontDlg.ShowDialog() == DialogResult.OK)
                rtb.SelectionFont = fontDlg.Font;
        }

        private void ApplyFontSize(float newSize)
        {
            Font current = rtb.SelectionFont ?? rtb.Font;
            try { rtb.SelectionFont = new Font(current.FontFamily, newSize, current.Style); } catch { }
        }

        private void BtnTextColor_Click(object sender = null, EventArgs e = null)
        {
            if (colorDlg.ShowDialog() == DialogResult.OK)
                rtb.SelectionColor = colorDlg.Color;
        }

        private void BtnBackColor_Click(object sender = null, EventArgs e = null)
        {
            if (colorDlg.ShowDialog() == DialogResult.OK)
                rtb.BackColor = colorDlg.Color;
        }

        private void UpdateStatus()
        {
            int lines = rtb.Lines.Length;
            int chars = rtb.Text.Length;
            lblStatus.Text = $"Lignes: {lines}    Caractères: {chars}";
        }

        private void LoadPossibleFileFromSplash()
        {
            try
            {
                string tmp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "open_file.path");
                if (File.Exists(tmp))
                {
                    string path = File.ReadAllText(tmp);
                    if (File.Exists(path))
                        LoadFile(path);
                    File.Delete(tmp);
                }
            }
            catch { }
        }
    }
}
