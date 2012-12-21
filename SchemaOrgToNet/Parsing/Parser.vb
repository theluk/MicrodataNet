Imports HtmlAgilityPack
Imports System.ComponentModel
Imports System.Web

Namespace Parsing

    Public Class Parser

        Private doc As New HtmlDocument
        Private web As New HtmlAgilityPack.HtmlWeb

        Private Shared _lock As New Object

        Private Shared definitions As Dictionary(Of Type, SchemaDefinitionAttribute)
        Private Shared urls As Dictionary(Of String, Type)
        Private Shared areDefInitialized = False

        Private Shared lexSourceLib As Reflection.Assembly

        Sub New()
            lexSourceLib = GetType(Parser).Assembly
            InitTypes()
        End Sub

        Sub New(lexicon As Reflection.Assembly)
            lexSourceLib = lexicon
            InitTypes()
        End Sub

        Sub load(url As String)
            doc = web.Load(url)
        End Sub

        Public Shared Function ResolveDefinition(type As Type) As SchemaDefinitionAttribute
            Parser.CheckTypes()
            Return Parser.definitions(type)
        End Function

        Public Shared Function CreateBySchemaUrl(url As String) As SchemaDefinitionBase
            Parser.CheckTypes()
            Return Tools.FastObjectFactory.CreateObjectFactory(urls(url)).Invoke
        End Function


        Private Shared Sub CheckTypes()
            If Not areDefInitialized Then
                Throw New Exception("Parser must be initalized at least once before using shared Function")
            End If
        End Sub

        Private Shared Sub InitTypes()

            If areDefInitialized Then Return

            SyncLock _lock
                If areDefInitialized Then Return

                Dim asmt = lexSourceLib.GetTypes
                Dim sp = (From i In asmt.AsParallel
                          Where i.IsSubclassOf(GetType(SchemaDefinitionBase))
                          Select i).ToDictionary(Of Type, SchemaDefinitionAttribute)(Function(i) i, Function(i) CType(TypeDescriptor.GetAttributes(i)(GetType(SchemaDefinitionAttribute)), SchemaDefinitionAttribute))

                Parser.definitions = sp
                Parser.urls = (From i In Parser.definitions).ToDictionary(Function(i) i.Value.Url, Function(i) i.Key)

                areDefInitialized = True

            End SyncLock

        End Sub

        Function parse() As SchemaDocument

            Dim scopes = doc.DocumentNode.SelectNodes("//*[@itemscope]")
            Dim instances As New Dictionary(Of HtmlNode, SchemaDefinitionBase)
            For Each item In scopes
                Dim itemParsed = ParseScopeRaw(item)
                instances.Add(itemParsed.Key, itemParsed.Value)
            Next

            For Each item In instances
                ParseProperties(item.Key, item.Value, instances)
            Next

            Return New SchemaDocument With {._items = instances.Values.ToList, .raw = instances}

        End Function

        Private Function ParseScopeRaw(node As HtmlNode) As KeyValuePair(Of HtmlNode, SchemaDefinitionBase)
            Dim url As String = node.GetAttributeValue("itemtype", "")
            Dim t As Type = Nothing
            If urls.ContainsKey(url) Then
                t = urls(url)
            Else
                t = GetType(SchemaDefinitionBase)
            End If
            Dim inst = CType(Tools.FastObjectFactory.CreateObjectFactory(t).Invoke(), SchemaDefinitionBase)
            Return New KeyValuePair(Of HtmlNode, SchemaDefinitionBase)(node, inst)
        End Function

        Sub ParseProperties(root As HtmlNode, instance As SchemaDefinitionBase, context As Dictionary(Of HtmlNode, SchemaDefinitionBase))

            Dim props = root.Descendants.AsParallel.Where(Function(i) i.Attributes.Contains("itemprop"))
            If props Is Nothing Then Return
            Dim ignoreNodes As New List(Of HtmlNode)
            For Each item In props
                If Not ignoreNodes.Contains(item) Then
                    ParseProperty(item, instance, context)
                    If IsScope(item) Then
                        ignoreNodes.AddRange(item.Descendants.AsParallel.Where(Function(i) i.Attributes.Contains("itemprop")))
                    End If
                End If
            Next
        End Sub

        Function IsScope(item As HtmlNode) As Boolean
            Return item.Attributes.Contains("itemscope")
        End Function

        Private regex As New Text.RegularExpressions.Regex("[\s|\t|\r|\n]{2,}")
        Private regex_gaps As New Text.RegularExpressions.Regex("[\t|\r|\n]+")
        Function GetValue(item As HtmlNode) As String
            Dim result As String
            Select Case item.Name.ToLower
                Case "img"
                    result = (item.GetAttributeValue("src", ""))
                Case "a", "link"
                    result = (item.GetAttributeValue("href", ""))
                Case "meta"
                    result = (item.GetAttributeValue("content", ""))
                Case Else
                    result = (item.InnerText)
            End Select

            result = HttpUtility.HtmlDecode(Trim(regex.Replace(regex_gaps.Replace(result, ""), " ")))
            Return result

        End Function

        Sub ParseProperty(item As HtmlNode, instance As SchemaDefinitionBase, context As Dictionary(Of HtmlNode, SchemaDefinitionBase))
            Dim name As String = item.GetAttributeValue("itemprop", String.Empty)
            Dim isScope As Boolean = item.Attributes.Contains("itemscope")
            Dim result As Object = Nothing

            If isScope And context.ContainsKey(item) Then
                result = context(item)
            Else
                result = GetValue(item)
            End If

            instance._propertyValues(name) = result

        End Sub


    End Class

    Public Class SchemaDocument
        Implements IDisposable

        Friend raw As Dictionary(Of HtmlNode, SchemaDefinitionBase)

        Friend _items As List(Of SchemaDefinitionBase)
        ReadOnly Property Items As IEnumerable(Of SchemaDefinitionBase)
            Get
                Return _items.ToArray
            End Get
        End Property

        Function GetByID(id As String) As IEnumerable(Of SchemaDefinitionBase)
            Return From i In raw Let idval As String = i.Key.GetAttributeValue("itemid", String.Empty)
                   Where Not String.IsNullOrEmpty(idval) Select i.Value
        End Function

        Function GetBySchemaUrl(url As String) As IEnumerable(Of SchemaDefinitionBase)
            Return From i In raw Let urlval As String = i.Key.GetAttributeValue("itemtype", String.Empty)
                   Where Not String.IsNullOrEmpty(urlval) Select i.Value
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    raw.Clear()
                    _items.Clear()
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

End Namespace