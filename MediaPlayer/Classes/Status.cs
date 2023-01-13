using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Classes
{
    public class Status { 
        public bool TrueValue { get; set; }
        public string Message { get; set; }

        public Status()
        {
            TrueValue = true;
            Message = "";
        }

        public Status(bool result, string message)
        {
            TrueValue = result;
            Message = message;
        }
    }

}
