using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace sma
{
    class docBody
    {
        public string id { get; set; }
        public object gnip_matchingrules { get; set; }
        public object actor_langs { get; set; }
        public string ghfour { get; set; }
        public string ghsix { get; set; }
        public string gheight { get; set; }
        public string ghten { get; set; }
        public string cntry_code { get; set; }
        public string created { get; set; }
        public string device { get; set; }
        public string yy { get; set; }
        public string qq { get; set; }
        public string mm { get; set; }
        public string dd { get; set; }
        public string hh { get; set; }
        public string mi { get; set; }
        public string dis_name { get; set; }
        public int fav_count { get; set; }
        public int foll_count { get; set; }
        public int friend_count { get; set; }
        public string geotype { get; set; }
        public string gnip_lang { get; set; }
        public object gnip_urls { get; set; }
        public int listed_count { get; set; }
        public string pointgeom_rpt { get; set; }
        public double pointx { get; set; }
        public double pointy { get; set; }
        public string pref_un { get; set; }
        public string pubdate { get; set; }
        public int rtcnt { get; set; }
        public double pos_sentiment { get; set; }
        public double neg_sentiment { get; set; }
        public int status_count { get; set; }
        public string text { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string verb { get; set; }
        public long _version_ { get; set; }
    }

    class Response
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public double maxScore { get; set; }
        public docBody[] docs { get; set; }
    }

    class ResponseHeader
    {
        public int status { get; set; }
        public int qtime { get; set; }
        public Dictionary<string, object> Params { get; set; }
    }

    class ResponseSolr
    {
        public ResponseHeader ResponseHeader { get; set; }
        public Response Response { get; set; }
        public object facet_counts { get; set; }
    }
}
