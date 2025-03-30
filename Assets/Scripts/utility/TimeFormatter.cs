namespace utility
{
    public class TimeFormatter
    {
        public static string FormatTime(float timeInSeconds)
        {
            var totalMilliseconds = (int)(timeInSeconds * 1000);
            var milliseconds = totalMilliseconds % 1000;
            var totalSeconds = totalMilliseconds / 1000;
            var seconds = totalSeconds % 60;
            var totalMinutes = totalSeconds / 60;
            var minutes = totalMinutes % 60;
            var hours = totalMinutes / 60;

            return hours > 0 ? $"{hours:D2}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}" : $"{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
        }
    }
}