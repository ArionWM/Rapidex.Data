using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class CommonConstants
    {
        //public static string[] NULLTEXTS = new string[] { "(null)", "null" };
        public static DateTimeOffset NULL_DATE = new DateTimeOffset(1800, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static DateTimeOffset MIN_DATE = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static DateTimeOffset MAX_DATE = new DateTimeOffset(2200, 12, 31, 0, 0, 0, TimeSpan.Zero);
        public const string NULLTEXT = "(null)";

        public const string FIELD_ID = "Id";
        public const string FIELD_EXTERNAL_ID = "ExternalId";
        public const string FIELD_VERSION = "DbVersion";

        public const string DATA_FIELD_ID = "_id";
        public const string DATA_FIELD_CAPTION = "_caption";
        public const string DATA_FIELD_TYPENAME = "_entity";
        public const string DATA_FIELD_PICTURE = "_picture";

        public const string ENV_DEVELOPMENT = "Development";
        public const string ENV_UNITTEST = "Test";
        public const string ENV_PRODUCTION = "Production";

        public const string MODULE_COMMON = "Common";
        public const string LIB_COMMON_NAVIGATION = "applicationBaseLibrary";


    }
}
