using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public class DataReleatedSignalConstants
{
    //Entity related
    public const string SIGNAL_NEW = "New";
    public const string SIGNAL_VALIDATE = "Validate";
    public const string SIGNAL_BEFORESAVE = "BeforeSave";
    public const string SIGNAL_AFTERSAVE = "AfterSave";
    public const string SIGNAL_AFTERCOMMIT = "AfterCommit";
    public const string SIGNAL_BEFOREDELETE = "BeforeDelete";
    public const string SIGNAL_AFTERDELETE = "AfterDelete";
    public const string SIGNAL_EDITING = "Editing";
    public const string SIGNAL_FILEATTACHED = "FileAttached";
    public const string SIGNAL_NOTEATTACHED = "NoteAttached";
}
