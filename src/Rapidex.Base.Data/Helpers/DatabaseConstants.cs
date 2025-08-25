using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public static class DatabaseConstants
    {
        public const string MASTER_DB_NAME = "Master";
        public const string DEFAULT_SCHEMA_NAME = "Base";

        public const string PREFIX_ENUMERATION = "enum";
        public const string PREFIX_DEFAULT = "ent";

        public const long MASTER_TENANT_ID = 1L;
        public const long DEFAULT_EMPTY_ID = -1L;

        public static string[] FIELDS_INTERNAL = new string[] { CommonConstants.FIELD_ID, CommonConstants.FIELD_EXTERNAL_ID, CommonConstants.FIELD_VERSION, "ModuleName", "WorkspaceName" };

        public static string[] FIELDS_PRIMARY = new string[] { "Name", "Title", "Subject", "Target", "Responsible", "Assigned", "AssignedTo" };
        public static string[] FIELDS_SECONDARY = new string[] { "StartTime", "EndTime", "Requester", "Owner", "Status" };
        //public static string[] FIELDS_SECONDARY = new string[] { "Description", "Details", "Notes", "Comments" };
        public static string KEY_OVERRIDE = "_override";

    }
}
