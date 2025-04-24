using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClockV2
{
    public partial class Form1: Form
    {

        public DateTime AlarmTime { get; private set; }
        public bool IsEnabled { get; private set; }

        public DateTimePicker dateTimePicker;
        public CheckBox checkBox;
        public Button saveBtn;
        public Button cancelBtn;
        public Form1()
        {
            InitializeComponent();

            this.Text = "Set Alarm";
            this.Size = new System.Drawing.Size(300, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            //DateTimePicker
            dateTimePicker = new DateTimePicker();
            dateTimePicker.Format = DateTimePickerFormat.Custom;
            dateTimePicker.CustomFormat = "HH:mm";
            dateTimePicker.ShowUpDown = true;
            dateTimePicker.Location = new System.Drawing.Point(50,30);
            this.Controls.Add(dateTimePicker);

            //CheckBox
            checkBox = new CheckBox();
            checkBox.Text = "Enable Alarm";
            checkBox.Location = new System.Drawing.Point(50, 70);
            this.Controls.Add(checkBox);

            //SaveBtn
            saveBtn = new Button();
            saveBtn.Text = "Save";
            saveBtn.Location = new System.Drawing.Point(50, 110);

            saveBtn.MouseEnter += (s, e) => saveBtn.BackColor = Color.Green;
            saveBtn.MouseLeave += (s, e) => saveBtn.BackColor = Color.FromArgb(30,30,30);

            saveBtn.FlatStyle = FlatStyle.Flat;
            saveBtn.Click += SaveBtn_Click;
            this.Controls.Add(saveBtn);

            //CancelBtn
            cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.Location = new System.Drawing.Point(150, 110);

            cancelBtn.MouseEnter += (s, e) => cancelBtn.BackColor = Color.Red;
            cancelBtn.MouseLeave += (s, e) => cancelBtn.BackColor = Color.FromArgb(30, 30, 30);

            cancelBtn.FlatStyle = FlatStyle.Flat;
            cancelBtn.Click += CancelBtn_Click;
            this.Controls.Add(cancelBtn);

        }

        public void SaveBtn_Click(object sender, EventArgs e)
        {
            AlarmTime = dateTimePicker.Value;
            IsEnabled = checkBox.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public void CancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    public class Alarm
    {
        public DateTime Time { get; set; }
        public bool IsEnabled { get; set; }

        public Alarm(DateTime time, bool isEnabled)
        {
            Time = time;
            IsEnabled = isEnabled;
        }

        public override string ToString()
        {
            return $"{Time:HH:mm} - {(IsEnabled ? "Enabled" : "Disabled")}";
        }
    }
}
