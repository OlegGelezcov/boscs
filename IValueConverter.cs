public interface IValueConverter
{
    int ConvertTo(double value, int multiplier = 1000);
    string ConvertFrom(int value, int multiplier = 1000);
}

public class IdentityConverter : IValueConverter
{
    public string ConvertFrom(int value, int multiplier = 1000)
    {
        return value.ToString();
    }

    public int ConvertTo(double value, int multiplier = 1000)
    {
        return (int)value;
    }
}
