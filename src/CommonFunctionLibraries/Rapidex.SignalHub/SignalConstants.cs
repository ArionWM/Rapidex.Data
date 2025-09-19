using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;
public class SignalConstants
{
    public static readonly char[] WildcardChars = TopicParser.WildcardChars;
    public static readonly string[] WildcardStrs = TopicParser.WildcardStrs;

    ////Entity related
    //public const string Signal_New = DataReleatedSignalConstants.Signal_New;
    //public const string Signal_BeforeSave  = DataReleatedSignalConstants.Signal_BeforeSave;
    //public const string Signal_AfterSave = DataReleatedSignalConstants.Signal_AfterSave;
    //public const string Signal_AfterCommit = DataReleatedSignalConstants.Signal_AfterCommit;
    //public const string Signal_BeforeDelete  = DataReleatedSignalConstants.Signal_BeforeDelete;
    //public const string Signal_AfterDelete = DataReleatedSignalConstants.Signal_AfterDelete;


    //Editing 
    public const string Signal_Editing = "Editing";
    //Importing
    public const string Signal_Importing = "Importing";
    //Imported
    public const string Signal_Imported = "Imported";
    //Exporting
    public const string Signal_Exported = "Exported";



    //Entity + Behavior releated
    //Archived
    public const string Signal_Archived = "Archived";
    //Unarchived
    public const string Signal_Unarchived = "Unarchived";


    //Automation Releated
    //TimeArrived 
    public const string Signal_TimeArrived = "TimeArrived";

    //Authorization Releated
    //Login
    public const string Signal_Login = "Login";
    //Logout
    public const string Signal_Logout = "Logout";

    //System Releated
    //OnError
    public const string Signal_OnError = "OnError";
    //SystemStarting
    public const string Signal_SystemStarting = "SystemStarting";
    //SystemStarted
    public const string Signal_SystemStarted = "SystemStarted";
    //SystemStopping
    public const string Signal_SystemStopping = "SystemStopping";
    //WorkspaceCreated
    public const string Signal_SchemaOrWorkspaceCreated = "SchemaOrWorkspaceCreated";
    //WorkspaceDeleted
    public const string Signal_SchemaOrWorkspaceDeleted = "SchemaOrWorkspaceDeleted";
    //ModuleInstalled
    public const string Signal_ModuleInstalled = "ModuleInstalled";
    //ModuleActivated
    public const string Signal_ModuleActivated = "ModuleActivated";
    //ModuleDeactivated
    public const string Signal_ModuleDeactivated = "ModuleDeactivated";


    //KPI releated?
}
