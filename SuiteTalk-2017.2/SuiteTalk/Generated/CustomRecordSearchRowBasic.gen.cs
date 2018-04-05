//~ Generated by SearchStubs/SearchRowBasic

using System;
using System.Collections.Generic;

namespace SuiteTalk
{
    partial class CustomRecordSearchRowBasic
    {
        private static readonly Lazy<ColumnFactory> columnFactoryLoader = new Lazy<ColumnFactory>(() => new ColumnFactory());

        public override void SetColumns(string[] columnNames)
        {
            var factory = columnFactoryLoader.Value;
            for (int i = 0; i < columnNames.Length; i++)
            {
                factory.BuildColumn(this, columnNames[i]);
            }
        }

        class ColumnFactory:  ColumnFactory<CustomRecordSearchRowBasic>
        {
            protected override Dictionary<string, Action<CustomRecordSearchRowBasic>> InitializeColumnBuilders()
            {
                return new Dictionary<string, Action<CustomRecordSearchRowBasic>> {
                    { "altName", r => r.@altName = new [] { new SearchColumnStringField { customLabel = "altName" } } },
                    { "availableOffline", r => r.@availableOffline = new [] { new SearchColumnBooleanField { customLabel = "availableOffline" } } },
                    { "created", r => r.@created = new [] { new SearchColumnDateField { customLabel = "created" } } },
                    { "externalId", r => r.@externalId = new [] { new SearchColumnSelectField { customLabel = "externalId" } } },
                    { "id", r => r.@id = new [] { new SearchColumnLongField { customLabel = "id" } } },
                    { "internalId", r => r.@internalId = new [] { new SearchColumnSelectField { customLabel = "internalId" } } },
                    { "isInactive", r => r.@isInactive = new [] { new SearchColumnBooleanField { customLabel = "isInactive" } } },
                    { "lastModified", r => r.@lastModified = new [] { new SearchColumnDateField { customLabel = "lastModified" } } },
                    { "lastModifiedBy", r => r.@lastModifiedBy = new [] { new SearchColumnSelectField { customLabel = "lastModifiedBy" } } },
                    { "name", r => r.@name = new [] { new SearchColumnStringField { customLabel = "name" } } },
                    { "owner", r => r.@owner = new [] { new SearchColumnSelectField { customLabel = "owner" } } },
                    { "parent", r => r.@parent = new [] { new SearchColumnSelectField { customLabel = "parent" } } },
                };
            }
        }
    }
}