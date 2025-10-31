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

    //Editing 
    public const string SIGNAL_EDITING = "Editing";
    //Importing
    public const string SIGNAL_IMPORTING = "Importing";
    //Imported
    public const string SIGNAL_IMPORTED = "Imported";
    //Exporting
    public const string SIGNAL_EXPORTED = "Exported";



    //Entity + Behavior releated
    //Archived
    public const string SIGNAL_ARCHIVED = "Archived";
    //Unarchived
    public const string SIGNAL_UNARCHIVED = "Unarchived";


    //Automation Releated
    //TimeArrived 
    public const string SIGNAL_TIMEARRIVED = "TimeArrived";

    //Authorization Releated
    //Login
    public const string SIGNAL_LOGIN = "Login";
    //Logout
    public const string SIGNAL_LOGOUT = "Logout";

    //System Releated
    //OnError
    public const string SIGNAL_ONERROR = "OnError";
    //SystemStarting
    public const string SIGNAL_SYSTEMSTARTING = "SystemStarting";
    //SystemStarted
    public const string SIGNAL_SYSTEMSTARTED = "SystemStarted";
    //SystemStopping
    public const string SIGNAL_SYSTEMSTOPPING = "SystemStopping";
    //WorkspaceCreated
    public const string SIGNAL_SCHEMAORWORKSPACECREATED = "SchemaOrWorkspaceCreated";
    //WorkspaceDeleted
    public const string SIGNAL_SCHEMAORWORKSPACEDELETED = "SchemaOrWorkspaceDeleted";
    //ModuleInstalled
    public const string SIGNAL_MODULEINSTALLED = "ModuleInstalled";
    //ModuleActivated
    public const string SIGNAL_MODULEACTIVATED = "ModuleActivated";
    //ModuleDeactivated
    public const string SIGNAL_MODULEDEACTIVATED = "ModuleDeactivated";


    //KPI releated?
}
