using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public enum EditingMode
    {
        Read,
        Edit 
    }

    public enum ExceptionTarget
    {
        Unknown,
        ITDepartment_Infrastructure,
        ITDepartment_ApplicationManagement,
        ApplicationSupport,
        DevelopmentTeam,
    }

    public enum TextType
    {
        Plain,
        Html,
        Markdown
    }

    public enum RunnableComponentStatus
    {
        Idle = 0 ,
        Running = 20, 
        WaitingStop = 40,
        Stopped = 50,
        Error = 99
             
    }
}
