Imports System.ComponentModel.DataAnnotations

Public Class SchemaDefinitionBase
    Implements IDisposable

    Friend _propertyValues As New Dictionary(Of String, Object)
    Private _schemaDefinition As SchemaDefinitionAttribute
    Private _schemaDefinitionFound As Boolean = False

    Public ReadOnly Property SchemaDefinition As SchemaDefinitionAttribute
        Get
            If Not _schemaDefinitionFound AndAlso _schemaDefinition Is Nothing Then
                _schemaDefinition = Parsing.Parser.ResolveDefinition(Me.GetType)
                _schemaDefinitionFound = True
            End If
            Return _schemaDefinition
        End Get
    End Property

    Default Property PropertyValue(name As String) As Object
        Get
            Return _propertyValues(name)
        End Get
        Set(value As Object)
            _propertyValues(name) = value
        End Set
    End Property

    ReadOnly Property AllProperties As IEnumerable(Of KeyValuePair(Of String, Object))
        Get
            Return _propertyValues.ToArray
        End Get
    End Property

    Function HasProperty(name As String) As Boolean
        Return _propertyValues.ContainsKey(name)
    End Function

    Function CopyTo(obj As SchemaDefinitionBase) As SchemaDefinitionBase
        obj._propertyValues = Me._propertyValues
        Return obj
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                _propertyValues.Clear()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

Public Class SchemaDefinitionAttribute
    Inherits Attribute

    Sub New(ID As String, Url As String)
        Me.ID = ID
        Me.Url = Url
    End Sub

    Property ID As String
    Property Url As String

End Class
