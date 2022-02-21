namespace uSync.Triggers.Auth
{
    internal class HmacParameters
    {
        public string Signature { get; set; }
        public string Nonce { get; set; }

        public long Timestamp { get; set; }
    }
}
