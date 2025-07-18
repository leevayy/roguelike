namespace utility
{
    public enum PlayerEventPayloadType
    {
        OnDash,
    }
    
    public class PlayerEventPayload
    {
        public PlayerEventPayloadType Type { get; private set; }
    }
        
    public class PlayerEvent<T>
    {
        private bool _isPrevented;
        public T payload { get; private set; }

        public PlayerEvent(bool isPrevented, T payload)
        {
            _isPrevented = isPrevented;
            this.payload = payload;
        }

        public void PreventDefault()
        {
            _isPrevented = true;
        }
    }
}