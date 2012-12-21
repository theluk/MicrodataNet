Imports System.Runtime.CompilerServices
Imports MicrodataNet.Core

Namespace Extensions


    Public Module Extensions

        <Extension> _
        Public Function FindAs(Of T)(items As IEnumerable(Of SchemaDefinitionBase)) As IEnumerable(Of T)
            If GetType(T).IsInterface Then
                Return items.OfType(Of T)()
            Else
                Dim inf = GetType(T).GetInterfaces()
                Dim result = From i In items
                            Let newType As Object = Tools.FastObjectFactory.CreateObjectFactory(GetType(T)).Invoke()
                            Where (From ty In inf Where ty.IsInstanceOfType(i)).Any
                            Select i.CopyTo(newType)

                Return result.AsEnumerable.Cast(Of T)()
            End If
        End Function

    End Module

End Namespace