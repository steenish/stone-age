using System.Collections.Generic;

namespace Utility {
    public class Logger {

        private readonly List<LogEntry> log = new List<LogEntry>();

        private class LogEntry {
            private readonly System.DateTime timestamp = System.DateTime.Now;
            private readonly string text;

            public LogEntry(string text) {
                this.text = text;
            }

            public override string ToString() {
                return $"[{timestamp.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("sv-SE"))}] {text}";
            }
        }

        public void Log(string text) {
            log.Add(new LogEntry(text));
        }

        public void LogTime(string text, System.DateTime startTime) {
            System.TimeSpan timeDifference = System.DateTime.Now - startTime;
            log.Add(new LogEntry(text + " (" + (timeDifference.Hours * 3600 + timeDifference.Minutes * 60 + timeDifference.Seconds + timeDifference.Milliseconds * 0.001) + " s)."));
        }

        public void WriteToFile(string path) {
            using System.IO.StreamWriter writer = System.IO.File.CreateText(path); foreach (LogEntry entry in log) {
                writer.WriteLine(entry.ToString());
            }
        }
    }
}