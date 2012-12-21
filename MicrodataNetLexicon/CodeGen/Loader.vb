Imports System.Net
Imports Newtonsoft
Imports Newtonsoft.Json.Linq
Imports System.IO
Imports System.ComponentModel

Namespace CodeGeneration

    Public Class Loader

        Public Const SchemaJSONUrl = "http://schema.rdfs.org/all.json"

        Public Shared Function getJSON(Optional root As String = "") As SchemaRoot

            Dim responseValue As String

            If Not String.IsNullOrEmpty(root) AndAlso File.Exists(root & "schema.json") Then
                responseValue = File.ReadAllText(root & "schema.json")
            Else

                Dim req As HttpWebRequest = WebRequest.CreateHttp(SchemaJSONUrl)
                Dim res As HttpWebResponse = req.GetResponse
                Dim dataStream As IO.Stream = res.GetResponseStream
                Dim reader As New IO.StreamReader(dataStream)

                responseValue = reader.ReadToEnd

                If Not String.IsNullOrEmpty(root) Then File.WriteAllText(root & "schema.json", responseValue)

                reader.Close()
                dataStream.Close()
                res.Close()

            End If



            Dim j As JObject = JObject.Parse(responseValue)
            Dim types As JObject = j.Property("types").Value
            Dim properties As JObject = j.Property("properties").Value
            Dim datatypes As JObject = j.Property("datatypes").Value

            Dim sc As New SchemaRoot

            sc.Types = (From i In types.Properties Select i.Value.ToObject(Of ThingCreatorDescriptor)()).ToList
            sc.Properties = (From i In properties.Properties Select i.Value.ToObject(Of SchemaPropertyDescriptor)()).ToList
            sc.DataTypes = (From i In datatypes.Properties Select i.Value.ToObject(Of DataTypeDescriptor)()).ToList

            sc.FormatData()



            Return sc

        End Function

        Shared Function F(ByVal value As String) As String
            Return value.Replace(Microsoft.VisualBasic.Strings.Chr(10), "").Replace("&", "").Replace("<", "").Replace(">", "")
        End Function
    End Class

    Public Class SchemaRoot

        Public Types As List(Of ThingCreatorDescriptor)
        Public Properties As List(Of SchemaPropertyDescriptor)
        Public DataTypes As List(Of DataTypeDescriptor)

        Public TypeMap As Dictionary(Of String, SchemaTypeDescriptor)
        Public PropMap As Dictionary(Of String, SchemaPropertyDescriptor)

        Public ClassInherits As New Dictionary(Of String, SchemaTypeDescriptor())

        Public TypeProperties As New Dictionary(Of String, SchemaPropertyDescriptor())

        Public GroupedProperties As New Dictionary(Of String, Dictionary(Of String, SchemaTypeDescriptor()))


        Sub FormatData()

            TypeMap = (From i In Types.Cast(Of SchemaTypeDescriptor).Concat(DataTypes.Cast(Of SchemaTypeDescriptor)) Select i.ID, i).ToDictionary(Function(i) i.ID, Function(i) i.i)
            PropMap = (From i In Properties Select i).ToDictionary(Function(i) i.ID, Function(i) i)

            For Each item In Types
                Dim anc = (From i In item.Ancestors.Union(item.Supertypes) Select TypeMap(i))
                Dim ancProp = (From i In anc From p In i.Properties Select p)
                Dim props = item.Properties.Except(ancProp).Select(Function(i) PropMap(i))
                TypeProperties(item.ID) = props.ToArray
            Next

            For Each item In Types
                Dim ih = (From i In item.Supertypes.Union(item.Ancestors) Let map = TypeMap(i) Select map).ToArray
                ClassInherits(item.ID) = ih
                Dim l = ih.ToList
                l.Add(item)

                Dim q = (From i In l From p In TypeProperties(i.ID) Select New With {.PropId = p.ID, .Item = i})
                Dim props = (From i In q Group By i.PropId Into Group Select PropId, Group).ToDictionary(Function(i) i.PropId, Function(i) i.Group.Select(Function(_inner) _inner.Item).ToArray)

                GroupedProperties(item.ID) = props

            Next

        End Sub

    End Class

    Public Class SchemaItemDescriptor

        Property ID As String
        Property Comment As String
        Property Comment_Plain As String
        Property Label As String

    End Class

    Public Class SchemaTypeDescriptor
        Inherits SchemaItemDescriptor

        Property Ancestors As List(Of String)
        Property Properties As List(Of String)
        Property Specific_Properties As List(Of String)
        Property Subtypes As List(Of String)
        Property Supertypes As List(Of String)

        Property Url As String

    End Class

    Public Class SchemaPropertyDescriptor
        Inherits SchemaItemDescriptor

        Property Domains As List(Of String)
        Property Ranges As List(Of String)

    End Class

    Public Class ThingCreatorDescriptor
        Inherits SchemaTypeDescriptor

    End Class

    Public Class DataTypeDescriptor
        Inherits SchemaTypeDescriptor

    End Class

End Namespace