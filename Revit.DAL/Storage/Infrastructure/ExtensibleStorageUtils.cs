﻿using System.Text.Json;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Revit.DAL.Exceptions;
using Revit.DAL.Storage.Infrastructure.Model;
using Revit.DAL.Storage.Infrastructure.Model.Enums;
using Revit.DAL.Storage.Schemas;

namespace Revit.DAL.Storage.Infrastructure
{
    public static class ExtensibleStorageUtils
    {
        public const int SetMethodParametersCount = 2;
        public const string FieldNameParameterName = @"fieldName";
        
        private static readonly IDictionary<Type, Func<Entity, string, object>> AvailableTypesDictionary =
            new Dictionary<Type, Func<Entity, string, object>>
            {
                {typeof(int), (e, n) => e.Get<int>(n)},
                {typeof(short), (e, n) => e.Get<short>(n)},
                {typeof(byte), (e, n) => e.Get<byte>(n)},
                {typeof(double), (e, n) => e.Get<double>(n)},
                {typeof(float), (e, n) => e.Get<float>(n)},
                {typeof(bool), (e, n) => e.Get<bool>(n)},
                {typeof(string), (e, n) => e.Get<string>(n)},
                {typeof(Guid), (e, n) => e.Get<Guid>(n)},
                {typeof(ElementId), (e, n) => e.Get<ElementId>(n)},
                {typeof(XYZ), (e, n) => e.Get<XYZ>(n)},
                {typeof(UV), (e, n) => e.Get<UV>(n)},
            };

        public static void AssertExtensibleStorageCompatible(Type type)
        {
            if (AvailableTypesDictionary.Keys.All(x => x != type))
            {
                throw new RevitDataAccessException($"{type.Name} is not compatible type with Revit Extensible Storage");
            }
        }

        public static bool IsExtensibleStorageCompatible(Type type) =>
            AvailableTypesDictionary.Keys.Any(x => x == type);

        public static object GetEntityFieldValue(Entity entity, Type fieldType, string fieldName)
        {
            AssertExtensibleStorageCompatible(fieldType);

            var func = AvailableTypesDictionary[fieldType];

            return func.Invoke(entity, fieldName);
        }

        public static bool IsSchemaPresent(string schemaGuid)
        {
            if (string.IsNullOrWhiteSpace(schemaGuid))
            {
                return false;
            }

            var result = Schema.ListSchemas().FirstOrDefault(x => x.GUID == new Guid(schemaGuid));

            return result != null;
        }

        public static bool IsDataStoredWithSchema(Element element, string schemaGuid)
        {
            var schema = Schema.Lookup(new Guid(schemaGuid));
            var entity = element.GetEntity(schema);

            return entity.IsValid();
        }

        //todo move to extensible storage service
        public static IEnumerable<(string data, SchemaDescriptor descriptor)> GetProjectData(
            this Document document,
            IExtensibleStorageService extensibleStorageService)
        {
            var schemas = extensibleStorageService
                .Where(x => x.Kind == TargetObjectKindEnum.ProjectInfo)
                .Select(x => Schema.Lookup(x.Guid));
            
            return 
                schemas
                    .Where(x => x is not null)
                    .Select(x =>
                    {
                        var entity = document.ProjectInformation.GetEntity(x);

                        if (!entity.IsValid())
                        {
                            return (null, null);
                        }

                        var descriptor = extensibleStorageService[x.GUID];

                        var result = descriptor.GetData(entity);
                        using var jDoc = JsonDocument.Parse(result);

                        return (JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true }), descriptor);
                    })
                    .Where(x => x.Item1 is not null);
        }

        public static (string data, SchemaDescriptor descriptor) GetDataFromElement(
            this Element element,
            IExtensibleStorageService extensibleStorageService)
        {
            var schemas = Schema.ListSchemas()
                .Where(x => extensibleStorageService.IsSchemaExists(x.GUID));

            foreach (var schema in schemas)
            {
                var entity = element.GetEntity(schema);

                if (!entity.IsValid())
                {
                    continue;
                }

                var result = (string)GetEntityFieldValue(entity, typeof(string), nameof(IDataSchema.Data));
                using var jDoc = JsonDocument.Parse(result);

                return 
                    (JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true }),
                    extensibleStorageService[entity.SchemaGUID]);
            }

            return (null, null);
        }
    }
}
