using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data;

public static class DatabaseConstants
{
    public const string MASTER_DB_ALIAS_NAME = "Master";
    public const string DEFAULT_SCHEMA_NAME = "Base";

    public const string PREFIX_ENUMERATION = "enum";
    public const string PREFIX_DEFAULT = "ent";

    public const long MASTER_TENANT_ID = 1L;
    public const long DEFAULT_EMPTY_ID = -1L;

    public const string FIELD_ID = "Id";
    public const string FIELD_EXTERNAL_ID = "ExternalId";
    public const string FIELD_VERSION = "DbVersion";

    public const string DATA_FIELD_ID = "_id";
    public const string DATA_FIELD_CAPTION = "_caption";
    public const string DATA_FIELD_TYPENAME = "_entity";
    public const string DATA_FIELD_PICTURE = "_picture";
    public const string DATA_FIELD_OPERATION = "_type";



    public static string[] FIELDS_INTERNAL = new string[] { FIELD_ID, FIELD_EXTERNAL_ID, FIELD_VERSION, DATA_FIELD_CAPTION, DATA_FIELD_OPERATION, "ModuleName", "WorkspaceName" };

    public static string[] FIELDS_PRIMARY = new string[] { "Name", "Title", "Subject", "Target", "Responsible", "Assigned", "AssignedTo" };
    public static string[] FIELDS_SECONDARY = new string[] { "StartTime", "EndTime", "Requester", "Owner", "Status" };
    //public static string[] FIELDS_SECONDARY = new string[] { "Description", "Details", "Notes", "Comments" };
    public static string KEY_OVERRIDE = "_override";

}
