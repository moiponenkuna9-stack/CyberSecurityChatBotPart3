using System;
using System.Drawing;
using System.Windows.Forms;

namespace CybersecurityChatbotGUI
{
    public class TaskForm : Form
    {
        public string TaskTitle { get; private set; }
        public string TaskDescription { get; private set; }
        public string ReminderDate { get; private set; }

        private TextBox txtTitle, txtDescription;
        private DateTimePicker dtpReminder;
        private CheckBox chkSetReminder;
        private Button btnAdd, btnCancel;

        public TaskForm(string prefilledTitle = "")
        {
            this.Text = "Add Cybersecurity Task";
            this.Size = new Size(460, 330);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.BackColor = Color.White;

            var lblTitle = new Label { Text = "Task Title:", Location = new Point(20, 20), Size = new Size(100, 22), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtTitle = new TextBox { Location = new Point(20, 45), Size = new Size(405, 26), Font = new Font("Segoe UI", 10f), Text = prefilledTitle };
            var lblDesc = new Label { Text = "Description:", Location = new Point(20, 82), Size = new Size(200, 22), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtDescription = new TextBox { Location = new Point(20, 107), Size = new Size(405, 26), Font = new Font("Segoe UI", 10f) };
            chkSetReminder = new CheckBox { Text = "Set a reminder date:", Location = new Point(20, 150), Size = new Size(200, 24), Font = new Font("Segoe UI", 9.5f) };
            dtpReminder = new DateTimePicker { Location = new Point(225, 148), Size = new Size(200, 26), Format = DateTimePickerFormat.Short, MinDate = DateTime.Today, Enabled = false };
            chkSetReminder.CheckedChanged += (s, e) => dtpReminder.Enabled = chkSetReminder.Checked;

            btnAdd = new Button { Text = "Add Task", Location = new Point(225, 210), Size = new Size(100, 36), BackColor = Color.FromArgb(18, 140, 126), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnCancel = new Button { Text = "Cancel", Location = new Point(335, 210), Size = new Size(90, 36), BackColor = Color.FromArgb(200, 200, 200), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10f), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblTitle, txtTitle, lblDesc, txtDescription, chkSetReminder, dtpReminder, btnAdd, btnCancel });
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please enter a task title.", "Missing Title", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            TaskTitle = txtTitle.Text.Trim();
            TaskDescription = string.IsNullOrWhiteSpace(txtDescription.Text) ? "No description provided." : txtDescription.Text.Trim();
            ReminderDate = chkSetReminder.Checked ? dtpReminder.Value.ToString("yyyy-MM-dd") : "";
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
