using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gbdx.Vector_Index
{
    public class PagedData
    {
        public object data { get; set; }
        public string next_paging_id { get; set; }
        public string item_count { get; set; }
    }
}
