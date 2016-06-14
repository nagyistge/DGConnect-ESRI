namespace AnswerFactory
{
    public class NamedBuffer
    {
        public string name { get; set; } 

        /// <summary>
        /// GeoJson of the buffer
        /// </summary>
        public string buffer { get; set; }

        public override string ToString()
        {
            return string.Format("NamedBuffer{name='{0}', buffer={1}}", this.name, this.buffer);
        }
    }
}