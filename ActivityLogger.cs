using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbotGUI
{
    public class ActivityLogger
    {
        private readonly List<string> _log = new List<string>();
        private const int MaxVisible = 10;

        public void Log(string description)
        {
            _log.Add($"[{DateTime.Now:HH:mm:ss}] {description}");
        }

        public string GetLog()
        {
            if (_log.Count == 0)
                return "📋 No actions recorded yet. Start chatting, adding tasks, or taking the quiz!";

            var sb = new StringBuilder();
            sb.AppendLine("📋 RECENT ACTIVITY LOG:\n");
            int start = Math.Max(0, _log.Count - MaxVisible);
            int count = 1;
            for (int i = start; i < _log.Count; i++)
            {
                sb.AppendLine($"{count}. {_log[i]}");
                count++;
            }
            if (_log.Count > MaxVisible)
                sb.AppendLine($"\n(Showing last {MaxVisible} of {_log.Count} total actions.)");
            return sb.ToString().TrimEnd();
        }
    }
}
