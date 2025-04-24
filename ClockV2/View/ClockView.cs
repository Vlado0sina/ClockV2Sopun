using Bunifu.UI.WinForms;
using ClockV2.Helpers;
using ClockV2.Presenter;
using PriorityQueue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Forms;
using static ClockV2.ClockView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace ClockV2
{
    /// <summary>
    /// The main form for the ClockV2
    /// </summary>
    public partial class ClockView : Form
    {
        //Variables related to alarm and UI compenents 
        private ClockPresenter presenter;
        private readonly ClockDrawingHelper drawingHelper = new ClockDrawingHelper();
        private DateTime currentTime;
        private Button btnSetAlarm;
        private Panel panelAlarmSettings;
        private DateTimePicker dateTimePicker;
        //
        private CheckBox checkBox;
        private Button toggleSwitch;
        private bool isToggleOn = false;
        //
        private ListBox listBoxAlarms;
        private Button deleteAlarmBtn;
        private Button saveBtn;
        private Button cancelBtn;
        private Button deleteButton;
        private int indexDeleteButton;
        private Button btnExportAlarms;
        private Timer alarmCheckTimer;
        private bool alarmsExported = false;
        private AlarmStopForm alarmStopForm = null;
        private bool isAlarmPlaying = false;
        //Priority queue for managing alarms in order we need 
        private SortedArrayPriorityQueue<Alarm> alarms = new SortedArrayPriorityQueue<Alarm>(100);

        /// <summary>
        /// Gets the currently selected alarm time
        /// </summary>
        public DateTime AlarmTime { get; private set; }

        /// <summary>
        /// Variable which store value if alarm is enabled
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Initializes the ClockView form and UI components
        /// </summary>
        public ClockView()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;


            //Button to export alarms to ics file
            btnExportAlarms = new Button
            {
                Text = "Export Alarms",
                Size = new Size(140, 30)
            };
            btnExportAlarms.Click += btnExportAlarms_Click;
            this.Controls.Add(btnExportAlarms);


            //Button "Set Alarm"
            btnSetAlarm = new Button
            {
                Text = "Set Alarm",
                Size = new Size(140, 30)
            };
            btnSetAlarm.Click += btnSetAlarm_Click;
            this.Controls.Add(btnSetAlarm);

            //ListBox to show alarms
            listBoxAlarms = new ListBox
            {
                Text = "Alarms",
                Size = new System.Drawing.Size(275, 275)
            };
            this.Controls.Add(listBoxAlarms);


            //Before the app load we are asking the user if they want to load saved alarms
            DialogResult res = MessageBox.Show("Do you want to load saved alarms?", "Load Alarms", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                LoadSavedAlarmsFromICS();
                RefreshAlarmListBox();
            }


            SetInitialWindowSize();

            // Enable double buffering to avoid flicker
            Panel_Clock.GetType()
                    .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(Panel_Clock, true, null);

            currentTime = DateTime.Now;

            alarmCheckTimer = new Timer();
            alarmCheckTimer.Interval = 1000;
            alarmCheckTimer.Tick += CheckAlarms;
            alarmCheckTimer.Start();


            //Initialize alarm settings pannel but hidden from beggining
            panelAlarmSettings = new Panel
            {
                Size = new Size(250, 180),
                //FormBorderStyle = FormBorderStyle.FixedDialog;
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Visible = false
            };
            this.Controls.Add(panelAlarmSettings);

            //DateTimePicker for alarm
            dateTimePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Location = new System.Drawing.Point(50, 30)
            };
            panelAlarmSettings.Controls.Add(dateTimePicker);

            //Check Box for detection if alarm is enabled
            checkBox = new CheckBox
            {
                Text = "Enable Alarm",
                Location = new System.Drawing.Point(50, 70)

            };
            panelAlarmSettings.Controls.Add(checkBox);

            //toggleSwitch = new Button
            //{
            //    Text = "OFF",
            //    BackColor = Color.DarkGray,
            //    ForeColor = Color.White,
            //    Location = new System.Drawing.Point(50, 70),
            //    Size = new Size(100, 30),
            //    FlatStyle = FlatStyle.Flat
            //};
            //toggleSwitch.Click += (s, e) =>
            //{
            //    isToggleOn = !isToggleOn;
            //    toggleSwitch.Text = isToggleOn ? "ON" : "OFF";
            //    toggleSwitch.BackColor = isToggleOn ? Color.Green : Color.DarkGray;
            //};
            //panelAlarmSettings.Controls.Add(toggleSwitch);

            //Save Button
            saveBtn = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(50, 110)
            };
            saveBtn.MouseEnter += (s, e) => saveBtn.BackColor = Color.Green;
            saveBtn.MouseLeave += (s, e) => saveBtn.BackColor = Color.FromArgb(30, 30, 30);

            saveBtn.FlatStyle = FlatStyle.Flat;
            saveBtn.Click += SaveBtn_Click;
            panelAlarmSettings.Controls.Add(saveBtn);

            //Cancel Button
            cancelBtn = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(150, 110)
            };
            cancelBtn.MouseEnter += (s, e) => cancelBtn.BackColor = Color.Red;
            cancelBtn.MouseLeave += (s, e) => cancelBtn.BackColor = Color.FromArgb(30, 30, 30);

            cancelBtn.FlatStyle = FlatStyle.Flat;
            cancelBtn.Click += CancelBtn_Click;
            panelAlarmSettings.Controls.Add(cancelBtn);





            this.Load += PositionSetAlaramButton;
            this.Load += PositionAlarmList;
            this.Resize += PositionSetAlaramButton;
            this.FormClosing += formClosing;
        }

        //private void SetInitialWindowSize(object sender, EventArgs e)
        private void SetInitialWindowSize()
        {

            int generalPaddingForm = 50;
            int clockHeight = Panel_Clock.Height;
            int clockWidth = Panel_Clock.Width;
            int btnHeight = btnSetAlarm.Height;
            int btnPadding = 20;

            int heightOFform = clockHeight + generalPaddingForm + btnHeight + btnPadding + listBoxAlarms.Height;
            int widthOFform = Math.Max(clockWidth, listBoxAlarms.Width) + generalPaddingForm * 2;

            this.ClientSize = new Size(widthOFform, heightOFform);

            PositionSetAlaramButton(null, null);
            PositionAlarmList(null, null);
        }
        public void SetPresenter(ClockPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void UpdateClock(DateTime currentTime)
        {
            this.currentTime = currentTime;
            Panel_Clock.Invalidate(); // Trigger a redraw of the panel
        }

        private void Panel_Clock_Paint(object sender, PaintEventArgs e)
        {
            if (presenter == null) return;

            var g = e.Graphics;
            drawingHelper.DrawClock(g, currentTime, Panel_Clock.Width, Panel_Clock.Height);
        }

        /// <summary>
        /// Handles the 'Set Alarm' button click event  
        /// </summary>
        private void btnSetAlarm_Click(object sender, EventArgs e)
        {
            Panel_Clock.Visible = false;
            panelAlarmSettings.Visible = true;
            btnSetAlarm.Visible = false;
        }

        /// <summary>
        /// Refreshes the ListBox with current alarms 
        /// </summary>
        //private void RefreshAlarmListBox()
        //{

        //    listBoxAlarms.Items.Clear();
        //    listBoxAlarms.Controls.Clear();

        //    //Creat a copy of all alarms to sort them without affectig on the original list
        //    var sortedAlarms = new List<Alarm>();
        //    for (int i = 0; i < alarms.size; i++)
        //    {
        //        sortedAlarms.Add(alarms.getAt(i));
        //    }

        //    //Sort alarms by priority
        //    sortedAlarms.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        //    //Add each element to the ListBox
        //    foreach (var alarm in sortedAlarms)
        //    {
        //        listBoxAlarms.Items.Add(alarm.ToString());              
        //    }
        //}

        private void RefreshAlarmListBox()
        {

            listBoxAlarms.Items.Clear();
            listBoxAlarms.Controls.Clear();

            //Creat a copy of all alarms to sort them without affectig on the original list
            var sortedAlarms = new List<(Alarm alarm, int originalIndex)>();
            for (int i = 0; i < alarms.size; i++)
            {
                sortedAlarms.Add((alarms.getAt(i), i));
            }

            //Sort alarms by priority
            sortedAlarms.Sort((a, b) => a.alarm.Priority.CompareTo(b.alarm.Priority));

            for (int i =0; i < sortedAlarms.Count;i++)
            {
                var alarm = sortedAlarms[i].alarm;
                int originalIndex = sortedAlarms[i].originalIndex;

                listBoxAlarms.Items.Add(alarm.ToString());

                var deleteButton = new Button
                {
                    Text = "Delete",
                    Tag = originalIndex
                };
                deleteButton.Click += deleteAlarm_Click;

                var editButton = new Button
                {
                    Text = "Edit",
                    Tag = originalIndex
                };
                editButton.Click += editAlarmBtn_Click;

                int yPos = i * deleteButton.Height;
                int deleteX = listBoxAlarms.Width - deleteButton.Width - 5;
                int editX = deleteX -editButton.Width -5;

                deleteButton.Location = new Point(deleteX, yPos);
                editButton.Location = new Point(editX, yPos);

                listBoxAlarms.Controls.Add(deleteButton);
                listBoxAlarms.Controls.Add(editButton);
            }

        }

        /// <summary>
        /// Handles saving a new alarm
        /// </summary>
        public void SaveBtn_Click(object sender, EventArgs e)
        {

            IsEnabled = checkBox.Checked;

            //IsEnabled = isToggleOn;

            //Save  the alarm data
            AlarmTime = dateTimePicker.Value;
            int priority = 1;


            Alarm newAlarm = new Alarm(AlarmTime, IsEnabled);
            alarms.Add(newAlarm, newAlarm.Priority);
            alarmsExported = false;

            //Update ListBox
            listBoxAlarms.Items.Add(newAlarm.ToString());

            //Delete Alarm Button
            var deleteAlarmBtn = new Button
            {
                Text = "Delete alarm",
                Visible = true
            };
            deleteAlarmBtn.Tag = alarms.size - 1;
            //deleteAlarmBtn.Tag = newAlarm;

            //Position of delete button
            int deleteAlarmBtnX = listBoxAlarms.Width - deleteAlarmBtn.Width - 5;
            int deleteAlarmBtnY = listBoxAlarms.Items.Count * deleteAlarmBtn.Height;
            deleteAlarmBtn.Location = new Point(deleteAlarmBtnX, deleteAlarmBtnY);

            deleteAlarmBtn.Click += deleteAlarm_Click;
            listBoxAlarms.Controls.Add(deleteAlarmBtn);


            //Edit Alarm Button
            var editAlarmBtn = new Button
            {
                Text = "Edit alarm",
                Visible = true,           
            };
            editAlarmBtn.Tag = alarms.size - 1;
            //editAlarmBtn.Tag = newAlarm;

            //Position of edit button
            int editAlarmBtnX = deleteAlarmBtnX - editAlarmBtn.Width - 5;
            int editAlarmBtnY = deleteAlarmBtnY;
            editAlarmBtn.Location = new Point(editAlarmBtnX, editAlarmBtnY);

            editAlarmBtn.Click += editAlarmBtn_Click;
            listBoxAlarms.Controls.Add(editAlarmBtn);

            RefreshAlarmListBox();

            //Hide the panel settings 
            IsEnabled = checkBox.Checked;

            //IsEnabled = isToggleOn;

            panelAlarmSettings.Visible = false;
            btnSetAlarm.Visible = true;
            Panel_Clock.Visible = true;


        }

        /// <summary>
        /// Cancels alarm settings and hides alarm setting panel 
        /// </summary>
        public void CancelBtn_Click(object sender, EventArgs e)
        {
            panelAlarmSettings.Visible = false;
            btnSetAlarm.Visible = true;
            Panel_Clock.Visible = true;
        }

        /// <summary>
        /// Positions the 'Set Alarm' and 'Export Alarms' buttons.
        /// </summary>
        private void PositionSetAlaramButton(object sender, EventArgs e)
        {
            int clockHeight = Panel_Clock.Height;
            int fromWidth = this.Width;

            int btnX = (fromWidth - btnSetAlarm.Width - btnExportAlarms.Width - 10) / 2;
            int btnY = clockHeight + 20; //Add 20px below the clock

            btnSetAlarm.Location = new Point(btnX, btnY);
            btnExportAlarms.Location = new Point(btnX + btnSetAlarm.Width + 10, btnY);

        }
        /// <summary>
        /// Positions the alarm ListBox relative to form size
        /// </summary>
        private void PositionAlarmList(object sender, EventArgs e)
        {
            int formWidth = this.Width;
            int listAlarmsX = (formWidth - listBoxAlarms.Width) / 2;
            int listAlarmsY = btnSetAlarm.Bottom + 10;

            listBoxAlarms.Location = new Point(listAlarmsX, listAlarmsY);

        }



        /// <summary>
        /// Handles deletetion of alarms
        /// </summary>
        ///
        private void deleteAlarm_Click(object sender, EventArgs e)
        {
            deleteButton = sender as Button;
            if (deleteButton == null || deleteButton.Tag == null) return;

            indexDeleteButton = (int)deleteButton.Tag;

            if (indexDeleteButton < 0 || indexDeleteButton >= alarms.size) return;

            Alarm alarmToDelete = alarms.getAt(indexDeleteButton);
            alarms.Remove(indexDeleteButton);

            alarmsExported = false;
            listBoxAlarms.Items.RemoveAt(indexDeleteButton);

            //Remove edit/delete button from UI
            List<System.Windows.Forms.Control> controlToRemove = new List<System.Windows.Forms.Control>();

            foreach (System.Windows.Forms.Control control in listBoxAlarms.Controls)
            {
                if (control is Button btn && btn.Tag != null)
                {
                    int btnIndex = (int)btn.Tag;

                    //Mark the button for the removal
                    if (btnIndex == indexDeleteButton)
                    {
                        controlToRemove.Add(btn);
                    }
                    //Shift the index to keep correct track of tags
                    else if (btnIndex > indexDeleteButton)
                    {
                        btn.Tag = btnIndex - 1;
                    }

                }
            }

            //Remove buttons from UI
            foreach (var control in controlToRemove)
            {
                control.Visible = false;
                control.Dispose();
            }

            RefreshAlarmListBox();
        }

        /// <summary>
        /// Check current time and compare it to time it was st up and if it is same then trigger 
        /// </summary>
        private void CheckAlarms(object sender, EventArgs e)
        {
            if (alarms.size == 0) return;
            DateTime now = DateTime.Now;
            List<int> alarmToRemove = new List<int>();

            for (int i = 0; i < alarms.size; i++)
            {
                Alarm alarm = alarms.getAt(i);

                //If alarm matches with current time is enabled  
                if (alarm.IsEnabled && alarm.Time.Hour == now.Hour && alarm.Time.Minute == now.Minute)
                {
                    //Strat to play a sound if its not already playing
                    if (!isAlarmPlaying)
                    {
                        isAlarmPlaying = true;
                        Task.Run(() =>
                        {
                            while (isAlarmPlaying)
                            {
                                System.Media.SystemSounds.Beep.Play();
                                //Waiting for  sec for sound
                                System.Threading.Thread.Sleep(1000);
                            }

                        });
                    }
                    //Showing the dilong for stoping alarm once
                    if (alarmStopForm == null || alarmStopForm.IsDisposed)
                    {
                        alarmStopForm = new AlarmStopForm();
                        var result = alarmStopForm.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            isAlarmPlaying = false;
                            alarmToRemove.Add(i);
                        }
                        alarmStopForm = null;
                    }


                }
            }

            //Remove all triggered alarms in des order to avoid index shifting
            foreach (int index in alarmToRemove.OrderByDescending(i => i))
            {
                if (index >= 0 && index < alarms.size)
                {
                    alarms.Remove(index);

                    if (index < listBoxAlarms.Items.Count)
                    {
                        listBoxAlarms.Items.RemoveAt(index);
                    }

                    List<System.Windows.Forms.Control> controlsRemove = new List<System.Windows.Forms.Control>();

                    foreach (System.Windows.Forms.Control control in listBoxAlarms.Controls)
                    {
                        if (control is Button btn && btn.Tag != null)
                        {
                            int btnIndex = (int)btn.Tag;
                            if (btnIndex == index)
                            {
                                controlsRemove.Add(btn);
                            }
                            else if (btnIndex > index)
                            {
                                btn.Tag = btnIndex - 1;
                            }
                        }
                    }
                    foreach (var control in controlsRemove)
                    {
                        control.Visible = false;
                        control.Dispose();
                    }
                }
            }

            RefreshAlarmListBox();
        }
        /// <summary>
        /// Edit an existing alarm 
        /// </summary>
            private void editAlarmBtn_Click(object sender, EventArgs e)
            {
                //Show the pannel settings but hide others
                panelAlarmSettings.Visible = true;
                btnSetAlarm.Visible = false;
                Panel_Clock.Visible = false;

                Button editButton = sender as Button;
                if (editButton?.Tag == null)
                {
                    return;
                }
                int indexEditButton = (int)editButton.Tag;

                if (indexEditButton < 0 || indexEditButton >= alarms.size) return;

                //Load selected alarms details into UI
                Alarm selectedAlarm = alarms.getAt(indexEditButton);
                dateTimePicker.Value = selectedAlarm.Time;
            checkBox.Checked = selectedAlarm.IsEnabled;
            //isToggleOn = selectedAlarm.IsEnabled;
            //toggleSwitch.Text = isToggleOn ? "ON" : "OFF";
            //toggleSwitch.BackColor = isToggleOn ? Color.Green : Color.DarkGray;


                //Ensure panels stay visible
                panelAlarmSettings.Visible = true;
                btnSetAlarm.Visible = false;
                Panel_Clock.Visible = false;

                saveBtn.Click -= SaveBtn_Click;

                //Replace Save button and update logic for the selected alarm
                saveBtn.Click += (s, args) =>
                    {
                        //Creat updated alarm and replace exiting one 
                        Alarm updatedAlarm = new Alarm(dateTimePicker.Value, checkBox.Checked);
                        //Alarm updatedAlarm = new Alarm(dateTimePicker.Value, isToggleOn);
                        alarms.Remove(indexEditButton);
                        alarms.Add(updatedAlarm, updatedAlarm.Priority);
                        alarmsExported = false;

                        //Update UI
                        listBoxAlarms.Items[indexEditButton] = alarms.getAt(indexEditButton).ToString();

                        //Hide setting panel
                        panelAlarmSettings.Visible = false;
                        btnSetAlarm.Visible = true;
                        Panel_Clock.Visible = true;
                    };
                if (indexDeleteButton < indexEditButton)
                {
                    editButton.Visible = false;
                }
            }
        /// <summary>
        /// Export current alarms to the ICS calendar file
        /// </summary>
        private void LoadSavedAlarmsFromICS()
        {
            string filePath = @"C:\Users\vladg\Source\Repos\ClockV2Sopun\ClockV2\event.ics";
            if (!File.Exists(filePath)) return;

            string [] lines = File.ReadAllLines(filePath);
            DateTime startTime = DateTime.MinValue;
            bool inEve = false;

            foreach (string line in lines) 
            {
                if(line.StartsWith("BEGIN:VEVENT")) inEve = true;
                if (line.StartsWith("END:VEVENT")) inEve = false;

                if (inEve && line.StartsWith("DTSTART:"))
                {
                    string dtstr = line.Substring("DTSTART:".Length);
                    if (DateTime.TryParseExact(dtstr,"yyyyMMdd'T'HHmmss'Z'", null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out startTime))
                    {
                        var newAlarm = new Alarm(startTime, true, true);
                        alarms.Add(newAlarm, newAlarm.Priority);
                    }
                }
            }
        }

        /// <summary>
        /// Represent and alarm itself
        /// </summary>
        public class Alarm
        {
         
            /// <summary>
            /// Gets or sets  the alarm time
            /// </summary>
            public DateTime Time { get; set; }
            /// <summary>
            /// Get or set the alarm is enabled
            /// </summary>
            public bool IsEnabled { get; set; }
            public bool IsFromFile { get; set; } = false;

            public int Priority => Time.Hour * 60 + Time.Minute;

            public Alarm(DateTime time, bool isEnabled, bool isFromFile = false)
            {
                Time = time;
                IsEnabled = isEnabled;
                IsFromFile = isFromFile;
            }

            public override string ToString()
            {
                return $"{Time:HH:mm} - {(IsEnabled ? "Enabled" : "Disabled")}" + (IsFromFile ? "(Saved)" : "");
            }

            /// <summary>
            /// Convert the alarm to an iCalendar format
            /// </summary>
            public string ToICalendarFormat()
            {

                string startTime = Time.ToString("yyyyMMdd'T'HHmmss'Z'");
                string endTime = Time.AddMinutes(1).ToString("yyyyMMdd'T'HHmmss'Z'");
                string dtStamp = DateTime.Now.ToString("yyyyMMdd'T'HHmmss'Z'");
                string uid = Guid.NewGuid().ToString();

                string trigger = "-PT0M";

                return $@"
BEGIN:VEVENT
UID:{uid}
DTSTAMP:{dtStamp}
SUMMARY:Alarm
DTSTART:{startTime}
DTEND:{endTime}
BEGIN:VALARM
TRIGGER:{trigger}
DESCRIPTION: Time to wake up!
ACTION:DISPLAY
END:VALARM
END:VEVENT
";

            }
        }

        /// <summary>
        /// Form wich will allow to stop an alarm
        /// </summary>
        public class AlarmStopForm : Form
        {
            private Button btnAlarmStop;

            /// <summary>
            /// A new istance of AlarmStopForm
            /// </summary>
            public AlarmStopForm()
            {
                this.Text = "AlarmAlert";

                btnAlarmStop = new Button
                {
                    Text = "Stop Alarm"
                };
                btnAlarmStop.Click += BtnStop_Click;
                this.Controls.Add(btnAlarmStop);

            }

            /// <summary>
            /// Stop the alaram when the button clicked
            /// </summary>
            private void BtnStop_Click(object sender, EventArgs e)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
       
        /// <summary>
        /// Closing from but before it asking if the user want to save alarms if they have not been exported yet
        /// </summary>
        private void formClosing(object sender, FormClosingEventArgs e)
        {
            if (alarms.size == 0 || alarmsExported) return;

            var res = MessageBox.Show("Do you want to save your alarms before closing?", "Save Alarms", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (res == DialogResult.Yes) 
            {
                ExportAlarmsToICS();
            }else if (res == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Export current list of alarms to iCalendar file
        /// Hsndle errors related to export
        /// </summary>
        public void ExportAlarmsToICS()
        {
            if (alarms.size == 0)
            {
                MessageBox.Show("No alarms to export. Please create an alarm first.", "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }


            var stringBuilder = new StringBuilder();
            string filePath = @"C:\Users\vladg\Source\Repos\ClockV2Sopun\ClockV2\event.ics";

            stringBuilder.AppendLine("BEGIN:VCALENDAR");
            stringBuilder.AppendLine("VERSION:2.0");

            for (int i = 0; i < alarms.size; i++)
            {
                var alarm = alarms.getAt(i);
                stringBuilder.Append(alarm.ToICalendarFormat());
            }

            stringBuilder.AppendLine("END:VCALENDAR");

            try
            {
                File.WriteAllText(filePath, stringBuilder.ToString());
                alarmsExported = true;
                if (File.Exists(filePath))
                {
                    alarmsExported = true;
                    MessageBox.Show("Alarms have been successfully exported to iCalendar format");
                }
                else
                {
                    MessageBox.Show("Failed");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error");
            }
        }


        /// <summary>
        /// Handles the export button click event 
        /// </summary>
        public void btnExportAlarms_Click(object sender, EventArgs e)
        {

            List<Alarm> enabledAlarms = new List<Alarm>();

            for (int i = 0; i < alarms.size; i++)
            {
                var alarm = alarms.getAt(i);

                if (alarm.IsEnabled)
                {
                    enabledAlarms.Add(alarm);
                }
            }


            ExportAlarmsToICS();
        }
    }
}
