Imports System.Runtime.CompilerServices
Imports MongoDB.Bson
Imports MicrodataNet.Core

Public Module Extensions

    Public Const BSONDOCUMENT_SCHEMA_COLUMN_NAME = "__MicroDataSchemaDefinition"

    <Extension> _
    Private Function MapValue(item As KeyValuePair(Of String, Object)) As Object
        If (GetType(SchemaDefinitionBase).IsInstanceOfType(item.Value)) Then
            Return CType(item.Value, SchemaDefinitionBase).AsBsonDocument()
        Else
            Return item.Value
        End If
    End Function

    <Extension> _
    Public Function MapDictionary(items As IEnumerable(Of KeyValuePair(Of String, Object))) As Dictionary(Of String, Object)
        Return items.ToDictionary(Function(i) i.Key, Function(i) i.MapValue())
    End Function

    <Extension> _
    Public Function AsBsonDocument(item As SchemaDefinitionBase) As BsonDocument
        Dim dict = item.AllProperties.MapDictionary
        dict.Add(BSONDOCUMENT_SCHEMA_COLUMN_NAME, item.SchemaDefinition.AsBsonDocument)
        Return New BsonDocument(dict)
    End Function

    <Extension> _
    Public Function AsBsonDocument(item As SchemaDefinitionAttribute) As BsonDocument
        Return New BsonDocument(
            New BsonElement("ID", item.ID),
            New BsonElement("Url", item.Url)
            )
    End Function

    <Extension> _
    Public Function ToSchemaItem(bson As BsonDocument) As SchemaDefinitionBase
        If Not bson.Contains(BSONDOCUMENT_SCHEMA_COLUMN_NAME) Then
            Throw New InvalidCastException("The specified BsonDocument is not a valid Microdata. (Has no valid '" & BSONDOCUMENT_SCHEMA_COLUMN_NAME & "' column")
        End If
        Dim item As SchemaDefinitionBase = Parsing.Parser.CreateBySchemaUrl(bson.GetValue(BSONDOCUMENT_SCHEMA_COLUMN_NAME).AsBsonDocument.GetValue("Url"))

        For Each prop In bson.Elements.Where(Function(i) i.Name <> BSONDOCUMENT_SCHEMA_COLUMN_NAME)
            If prop.Value.IsBsonDocument Then
                item(prop.Name) = prop.Value.AsBsonDocument.ToSchemaItem()
            Else
                item(prop.Name) = prop.Value.RawValue
            End If
        Next

        Return item
    End Function

    <Extension> _
    Public Function ToSchemaItems(bson As IEnumerable(Of BsonDocument)) As IEnumerable(Of SchemaDefinitionBase)
        Return bson.Select(Function(i) i.ToSchemaItem)
    End Function

    <Extension> _
    Public Function FindBySchemaUrl(col As Global.MongoDB.Driver.MongoCollection(Of BsonDocument), url As String) As Global.MongoDB.Driver.MongoCursor(Of BsonDocument)
        Dim q = Global.MongoDB.Driver.Builders.Query.EQ(BSONDOCUMENT_SCHEMA_COLUMN_NAME & ".Url", url)
        Return col.Find(q)
    End Function

End Module
