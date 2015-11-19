namespace GBD
{
    public class Header
    {
        public string serviceName { get; set; }
        public string messageID { get; set; }
        public string serviceOperation { get; set; }
        public int serviceVersion { get; set; }
        public string serviceProvider { get; set; }
        public string serviceConsumer { get; set; }
        public long messageDateTimeStamp { get; set; }
        public string sendRuntime { get; set; }
        public string classification { get; set; }
    }
}