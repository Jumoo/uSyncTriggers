namespace uSyncTrigger
{
    public class TriggerOptions
    {
        public string? Group { get; set; }
        public string? Set { get; set; }
        public string? Folder { get; set; }
        public bool Force { get; set; }

        public bool Verbose { get; set; }

        public string? HmacKey { get; set; }
    }
}
