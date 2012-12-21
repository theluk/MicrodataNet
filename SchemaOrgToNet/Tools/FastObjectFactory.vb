Imports System.Reflection.Emit
Namespace Tools

    Public NotInheritable Class FastObjectFactory
        Private Sub New()
        End Sub
        Private Shared ReadOnly creatorCache As Hashtable = Hashtable.Synchronized(New Hashtable())
        Private Shared ReadOnly coType As Type = GetType(CreateObject)
        Public Delegate Function CreateObject() As Object

        Public Shared Function CreateObjectFactory(mt As Type) As CreateObject
            Dim c As FastObjectFactory.CreateObject = TryCast(creatorCache(mt), FastObjectFactory.CreateObject)
            If c Is Nothing Then
                SyncLock creatorCache.SyncRoot
                    c = TryCast(creatorCache(mt), FastObjectFactory.CreateObject)
                    If c IsNot Nothing Then
                        Return c
                    End If
                    Dim dynMethod As New DynamicMethod("DM$OBJ_FACTORY_" + mt.Name, GetType(Object), Nothing, mt)
                    Dim ilGen As ILGenerator = dynMethod.GetILGenerator()

                    ilGen.Emit(OpCodes.Newobj, mt.GetConstructor(Type.EmptyTypes))
                    ilGen.Emit(OpCodes.Ret)
                    c = DirectCast(dynMethod.CreateDelegate(coType), CreateObject)
                    creatorCache.Add(mt, c)
                End SyncLock
            End If
            Return c
        End Function

        ''' <summary>
        ''' Create an object that will used as a 'factory' to the specified type T 
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function CreateObjectFactory(Of T As Class)() As CreateObject
            Dim mt As Type = GetType(T)
            Return CreateObjectFactory(mt)
        End Function
    End Class

End Namespace