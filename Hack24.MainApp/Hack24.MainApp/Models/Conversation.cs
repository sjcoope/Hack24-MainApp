using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hack24.MainApp.Models
{
    public class Conversation
    {
        public string Name { get; set; }
        public List<Statement> Statements { get; set; }
    }
}
