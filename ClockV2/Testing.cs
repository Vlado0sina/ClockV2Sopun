using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PriorityQueue;
using static ClockV2.ClockView;

namespace ClockV2
{
    class Testing
    {
        [TestFixture]

        public class AlarmTests
        {
            /// <summary>
            /// Test alarm creationg with specific time and check if alarm is enabled
            /// </summary>
            [Test]
            public void alarmCreation()
            {
                var time = new DateTime(2025, 1, 1, 6, 30, 0);
                var alarm = new ClockView.Alarm(time, true);

                Assert.That(time, Is.EqualTo(alarm.Time));
                Assert.That(alarm.IsEnabled, Is.True);
            }

            /// <summary>
            /// Verify the priority calculation for an alarm based on time
            /// </summary>
            [Test]
            public void priorityCalculatedCorrectly()
            {
                var alarm = new ClockView.Alarm(new DateTime(1, 1, 1, 7, 45, 0), true);

                Assert.That(alarm.Priority, Is.EqualTo(465));
            }

            /// <summary>
            /// Test the representation of an alarm
            /// </summary>
            [Test]
            public void alarmToString()
            {
                var alarm = new ClockView.Alarm(new DateTime(1, 1, 1, 10, 5, 0), true, true);

                string expectedRes = "10:05 - Enabled(Saved)";

                Assert.That(alarm.ToString(), Is.EqualTo(expectedRes));
            }

            /// <summary>
            /// Check if converted correctly to iCalendar format
            /// </summary>
            [Test]
            public void alarmToCalendarFormat()
            {
                var time = new DateTime(2025, 1, 1, 6, 30, 0);
                var alarm = new ClockView.Alarm(time, true);
                var ics = alarm.ToICalendarFormat();

                Assert.That(ics, Does.Contain("BEGIN:VEVENT"));
                Assert.That(ics, Does.Contain("SUMMARY:Alarm"));
                Assert.That(ics, Does.Contain("DTSTART:20250101T063000Z"));

                var dtstampLine = ics.Split('\n').FirstOrDefault(l => l.StartsWith("DTSTAMP:"));

                Assert.That(dtstampLine, Is.Not.Null);
                //Assert.That(dtstampLine.Length, Is.EqualTo("DTSTAMP:yyyyMMddTHHmmssZ".Length));
                Assert.That(dtstampLine.Trim(), Does.Match(@"DTSTAMP:\d{8}T\d{6}Z$"));
            }

            private SortedArrayPriorityQueue<ClockView.Alarm> queue;

            /// <summary>
            /// Set up method to initialize priority queue before each test
            /// </summary>
            [SetUp]
            public void setUp()
            {
                queue = new SortedArrayPriorityQueue<ClockView.Alarm>(10);
            }

            /// <summary>
            /// Test adding an alarm to ther queue
            /// </summary>
            [Test]
            public void addAlarmToQueue()
            {
                var alarm = new ClockView.Alarm(DateTime.Now.AddMinutes(10), true);
                queue.Add(alarm, alarm.Priority);

                Assert.That(queue.size, Is.EqualTo(1));
            }

            /// <summary>
            /// Testing removing an alarm
            /// </summary>
            [Test]
            public void removeAlarm()
            {
                var alarm1 = new ClockView.Alarm(DateTime.Now.AddMinutes(10), true);
                var alarm2 = new ClockView.Alarm(DateTime.Now.AddMinutes(20), true);

                queue.Add(alarm1, alarm1.Priority);
                queue.Add(alarm2, alarm2.Priority);

                queue.Remove(0);

                Assert.That(queue.size, Is.EqualTo(1));
                Assert.That(queue.getAt(0).Time, Is.EqualTo(alarm1.Time));
            }

            /// <summary>
            /// Test editing alarm
            /// </summary>
            [Test]
            public void EditAlarm()
            {
                var alarm = new ClockView.Alarm(DateTime.Now, true);
                queue.Add(alarm, alarm.Priority);

                var updated = new ClockView.Alarm(DateTime.Now.AddHours(1), false);
                queue.Remove(0);

                queue.Add(updated, updated.Priority);

                Assert.That(queue.size, Is.EqualTo(1));
                Assert.That(queue.getAt(0).IsEnabled, Is.False);
            }
        }

        /// <summary>
        /// Testing exporting and loading an alarm to ICS fromat
        /// </summary>
        public class ICSExportTest
        {
            [Test]
            public void Export()
            {
                var alarm = new ClockView.Alarm(DateTime.UtcNow.AddMinutes(5), true);

                string content = "BEGIN:VCALENDAR\nVERSION:2.0\n" + alarm.ToICalendarFormat() + "END:VCALENDAR";

                var tempFile = Path.Combine(Path.GetTempPath(), "event.ics");
                File.WriteAllText(tempFile, content);

                Assert.That(File.Exists(tempFile), Is.True);

                string[] lines = File.ReadAllLines(tempFile);

                Assert.That(lines, Has.Some.Contains("BEGIN:VEVENT"));
            }

            [Test]
            public void LoadSavedAlarmsFromICS()
            {
                string ics = @"
BEGIN:VCALENDAR
VERSION:2.0
BEGIN:VEVENT
DTSTART:20250414T103000Z
DTEND:20250414t103100Z
SUMMARY:Alarm
BEGIN:VALARM
TRIGGER:-PT0M
DESCRIPTION: Time to wake up!
ACTION:DISPLAY
END:VALARM
END:VEVENT
END:VCALENDAR
";
                string tempPath = Path.Combine(Path.GetTempPath(), "event.ics");
                File.WriteAllText(tempPath, ics);

                var alarms = new SortedArrayPriorityQueue<ClockView.Alarm>(10);
                string[] lines = File.ReadAllLines(tempPath);
                bool inEve = false;
                DateTime startTime = DateTime.MinValue;


                foreach (var line in lines)
                {
                    if (line.StartsWith("BEGIN:VEVENT")) inEve = true;
                    if (line.StartsWith("END:VEVENT")) inEve = false;

                    if (inEve && line.StartsWith("DTSTART:"))
                    {
                        string dt = line.Substring("DTSTART:".Length);
                        if (DateTime.TryParseExact(dt, "yyyyMMdd'T'HHmmss'Z'", null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out startTime))
                        {
                            var newAlarm = new ClockView.Alarm(startTime, true, true);
                            alarms.Add(newAlarm, newAlarm.Priority);
                        }
                    }
                }

                Assert.That(alarms.size, Is.EqualTo(1));
                Assert.That(alarms.getAt(0).Time.Hour, Is.EqualTo(10));
                Assert.That(alarms.getAt(0).Time.Minute, Is.EqualTo(30));
            }
        }
    }
}
