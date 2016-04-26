namespace AnswerFactory
{
    using System.Collections.Generic;

    public class  ResponseData
    {
        public List<Datum> data { get; set; }
        public int shards { get; set; }
    }


    public class Datum
    {
        public string name { get; set; }
        public int count { get; set; }
    }
}