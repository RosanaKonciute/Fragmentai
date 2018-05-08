using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XESApdorojimas
{
    class Log
    {
        public string FileName { get; set; }
        public List<Trace> Traces { get; private set; } = new List<Trace>(); 
    }

    class Trace
    {
        public string Id { get; set; }
        public List<Event> Events { get; private set; } = new List<Event>();
    }

    class Event
    {
        public string Name { get; set; }
    }
}
