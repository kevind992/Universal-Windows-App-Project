using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace appDevProject
{
    
    public class RootBusStopobject
    {
        public string errorcode { get; set; }
        public string errormessage { get; set; }
        public int numberofresults { get; set; }
        public string timestamp { get; set; }
        public Result[] results { get; set; }
    }

    public class Result
    {
        public string stopid { get; set; }
        public string displaystopid { get; set; }
        public string shortname { get; set; }
        public string shortnamelocalized { get; set; }
        public string fullname { get; set; }
        public string fullnamelocalized { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string lastupdated { get; set; }
        public Operator[] operators { get; set; }
    }

    public class Operator
    {
        public string name { get; set; }
        public string[] routes { get; set; }
    }


}
