namespace utility
{
    public class Limits
    {
        public readonly float Left;
        public readonly float Right;
        public readonly float Upper;
        public readonly float Lower;
        
        public Limits(float leftLimit, float rightLimit, float lowerLimit, float upperLimit)
        {
            Left = leftLimit;
            Right = rightLimit;
            Upper = upperLimit;
            Lower = lowerLimit;
        }
    }
}