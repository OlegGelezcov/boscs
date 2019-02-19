using System;

public class TimePlayedConverter : IValueConverter
{
    public int ConvertTo(double value, int multiplier = 1000)
    {
        throw new NotImplementedException(); // not needed
    }
    public string ConvertFrom(int timeMilis, int multiplier = 1000)
    {
        TimeSpan t = TimeSpan.FromMilliseconds(timeMilis);

        if (t.TotalDays != 0)
        {
            return string.Format("{0} days\r\n{1} hours", t.Days, t.Hours);
        }

        if (t.TotalHours != 0)
        {
            return string.Format("{0} hours\r\n{1} minutes", t.Hours, t.Minutes);
        }

        if (t.TotalMinutes != 0)
        {
            return string.Format("{0} minutes\r\n{1} seconds", t.Minutes, t.Seconds);
        }

        return "----";
    }
}
