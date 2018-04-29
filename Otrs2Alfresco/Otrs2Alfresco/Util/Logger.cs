﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Otrs2Alfresco
{
    public enum LogSeverity
    {
        Debug = 0,
        Verbose = 1,
        Info = 2,
        Notice = 3,
        Warning = 4,
        Error = 5,
        Critical = 6,
    }

    public class LogEntry
    {
        public LogSeverity Severity { get;  private set; }

        public string Text { get; private set;  }

        public DateTime DateTime { get; private set; }

        public LogEntry(LogSeverity severity, string text)
        {
            Severity = severity;
            Text = text;
            DateTime = DateTime.Now;
        }

        public string ToText()
        {
            return string.Format("{0} {1} {2}", 
                                 DateTime.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture).PadRight(20), 
                                 Severity.ToString().PadRight(8), 
                                 Text);
        }
    }

    public class Logger : IDisposable
    {
        public List<LogEntry> Entries { get; private set; }

        public LogSeverity ConsoleSeverity { get; set; }

        public LogSeverity FileSeverity { get; set; }

        private FileStream _logFile;

        private TextWriter _logWriter;

        private DateTime _logFileDate;

        private const string LogPath = "/var/log/o2a";

        public Logger()
        {
            Entries = new List<LogEntry>();
            ConsoleSeverity = LogSeverity.Info;
            FileSeverity = LogSeverity.Verbose;

            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
        }

        public void Dispose()
        {
            if (_logFile != null)
            {
                _logFile.Close();
                _logFile = null;
            }
        }

        private void CheckLogFile()
        {
            if (_logFile == null)
            {
                _logFileDate = DateTime.Now;
                var filename = string.Format("o2a_{0}.log", _logFileDate.ToString("yyyy-MM-dd-HH-mm-ss"));
                _logFile = File.OpenWrite(Path.Combine(LogPath, filename));
                _logWriter = new StreamWriter(_logFile);
            }
            else if (DateTime.Now > _logFileDate.AddDays(1))
            {
                _logFile.Close();
                _logFileDate = DateTime.Now;
                var filename = string.Format("o2a_{0}.log", _logFileDate.ToString("yyyy-MM-dd-HH-mm-ss"));
                _logFile = File.OpenWrite(Path.Combine(LogPath, filename));
                _logWriter = new StreamWriter(_logFile);
            }
        }

        public void Debug(string text, params object[] arguments)
        {
            Write(LogSeverity.Debug, text, arguments).ToList();
        }

        public void Verbose(string text, params object[] arguments)
        {
            Write(LogSeverity.Verbose, text, arguments).ToList();
        }

        public void Info(string text, params object[] arguments)
        {
            Write(LogSeverity.Info, text, arguments).ToList();
        }

        public void Notice(string text, params object[] arguments)
        {
            Write(LogSeverity.Notice, text, arguments).ToList();
        }

        public void Warning(string text, params object[] arguments)
        {
            Write(LogSeverity.Warning, text, arguments).ToList();
        }

        public void Error(string text, params object[] arguments)
        {
            Write(LogSeverity.Error, text, arguments).ToList();
        }

        public Exception Critical(string text, params object[] arguments)
        {
            var entry = Write(LogSeverity.Critical, text, arguments).ToList().First();
            return new Exception(entry.Text);
        }

        private IEnumerable<LogEntry> Write(LogSeverity severity, string text, object[] arguments)
        {
            var fullText = arguments.Length > 0 ? string.Format(text, arguments) : text;

            foreach (var line in fullText.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedLine = line.Trim();

                if (trimmedLine != string.Empty)
                {
                    yield return WriteLine(severity, trimmedLine);
                }
            }
        }

        private LogEntry WriteLine(LogSeverity severity, string text)
        {
            var entry = new LogEntry(severity, text);
            Entries.Add(entry);

            if (entry.Severity >= ConsoleSeverity)
            {
                Console.WriteLine(entry.ToText());
            }

            if (entry.Severity >= FileSeverity)
            {
                CheckLogFile();

                _logWriter.WriteLine(entry.ToText());
                _logWriter.Flush();
            }

            return entry;
        }

        public string ToText(LogSeverity severity)
        {
            var builder = new StringBuilder();

            foreach (var entry in Entries)
            {
                if (entry.Severity >= severity)
                {
                    builder.AppendLine(entry.ToText());
                }
            }

            return builder.ToString();
        }

        public void Clear()
        {
            Entries.Clear();
        }

        public LogSeverity HighestSeverity
        {
            get 
            {
                var severity = LogSeverity.Debug;

                foreach (var entry in Entries)
                {
                    if (entry.Severity > severity)
                    {
                        severity = entry.Severity;
                    }
                }

                return severity;
            }
        }
    }
}
