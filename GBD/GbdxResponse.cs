using System.Collections.Generic;

namespace GBD
{
    public class GbdxResponse
    {
        public string identifier { get; set; }
        public List<string> type { get; set; }
        public Properties properties { get; set; }
    }
}