Imports MongoDB.Driver

Public Class Query
    Public Shared Function SchemaUrlEQ(url As String) As IMongoQuery
        Return Builders.Query.EQ(BSONDOCUMENT_SCHEMA_COLUMN_NAME & ".Url", url)
    End Function
End Class
