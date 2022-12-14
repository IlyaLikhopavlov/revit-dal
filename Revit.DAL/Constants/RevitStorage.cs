namespace Revit.DAL.Constants
{
    public static class RevitStorage
    {
        public const string FooSchemaGuid = @"B930CE04-70D2-49A4-9FA1-0A3B8BBC7568";

        public const string BarSchemaGuid = @"56BF018D-B9DE-4BCD-AE9A-2DFA1B843365";

        public const string SettingsExtensibleStorageSchemaGuid = @"3003AFD3-87E4-4424-A315-1312AA8C9A4E";

        public const string IdStorageSchemaGuid = @"6D90F8EE-1752-4BAE-BD34-215CDB31DA06";

        public const string SaveChangesTransactionName = @"Save Changes";

        public static class Settings
        {
            public const string SchemaName = @"SettingsSchema";

            public const string FieldName = @"SettingsDictionary";
        }

        public static class IdList
        {
            public const string SchemaName = @"IdListSchema";

            public const string FieldName = @"IdList";
        }
    }
}
