using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public class DataReleatedSignalConstants
{
    //Entity related
    public const string Signal_New = "New";
    public const string Signal_Validate = "Validate";
    public const string Signal_BeforeSave = "BeforeSave";
    public const string Signal_AfterSave = "AfterSave";
    public const string Signal_AfterCommit = "AfterCommit";
    public const string Signal_BeforeDelete = "BeforeDelete";
    public const string Signal_AfterDelete = "AfterDelete";
    public const string Signal_Editing = "Editing";
    public const string Signal_FileAttached = "FileAttached";
    public const string Signal_NoteAttached = "NoteAttached";
}
