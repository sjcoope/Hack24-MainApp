using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hack24.MainApp.Models
{
    public class Statement
    {
        public string SpeechText { get; set; }
        public List<Response> Responses { get; set; }
    }
}
