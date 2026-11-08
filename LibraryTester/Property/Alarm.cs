using Lincsolution.Core.Base.Alarms;
using System;

namespace LibraryTester.Property
{
    public class Alarm : AlarmBase
    {
        public Alarm(string msg, AlarmSeverity sev) : base(msg, sev)
        {
            
        }
    }
}
