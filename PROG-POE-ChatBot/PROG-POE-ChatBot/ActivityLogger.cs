using System;
using System.Collections.Generic;
using System.Linq;

public static class ActivityLogger
{
    private static readonly List<string> activityLog = new List<string>();

    public static void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("g"); // set time it was logged
        activityLog.Add($"{message} at {timestamp}");

        //Keep it to the latest 50 actions
        if (activityLog.Count > 50)
            activityLog.RemoveAt(0);
    }

    public static List<string> GetRecentLog(int count = 10)
    {
        return activityLog.Skip(Math.Max(0, activityLog.Count - count)).Reverse().ToList();
    }
}
