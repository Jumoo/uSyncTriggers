namespace uSync.Triggers.Auth
{
    public class HmacParamaters
    {
        public string Signature { get; set; }
        public string Nonce { get; set; }
        
        public long Timestamp { get; set; }
        
    }
}
