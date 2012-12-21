Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports MicrodataNet.MongoDB
Imports MicrodataNet.Core
Imports MicrodataNet.Core.Parsing
Imports MicrodataNet.Lexicon.Extensions
Imports MicrodataNet.Lexicon
Imports MongoDB.Driver
Imports MongoDB.Bson
Imports MongoDB.Driver.Linq
Imports MongoDB.Driver.Builders

<TestClass()> Public Class BsonTests
    Dim parser As New Parser(GetType(MicrodataNet.Lexicon.AboutPage).Assembly)

    Friend Function GetMovie() As TVSeries

        parser.load("http://www.imdb.com/title/tt0306414/")

        Dim doc = parser.parse()

        Dim movie = doc.Items.FindAs(Of TVSeries)().FirstOrDefault
        Return movie

    End Function

    <TestMethod> _
    Public Sub Extensions()

        Dim movie = GetMovie()

        Dim bsonDoc = movie.AsBsonDocument

        Assert.IsTrue(bsonDoc.GetValue("name").AsString = "The Wire (2002–2008)")

        Dim movieFromBson = CType(bsonDoc.ToSchemaItem(), TVSeries)

        Assert.IsInstanceOfType(movieFromBson.aggregateRating, GetType(AggregateRating))

    End Sub

End Class

<TestClass()> _
Public Class DBTests

    Dim BsonTest As New BsonTests

    <TestMethod> _
    Public Sub DB()

        Dim srv As MongoServer = MongoServer.Create("mongodb://localhost")

        Dim db = srv.GetDatabase("test")
        Dim col = db.GetCollection(Of BsonDocument)("test")

        Dim movie = BsonTest.GetMovie
        Dim url = movie.SchemaDefinition.Url

        col.Save(movie.AsBsonDocument)

        Assert.IsTrue(col.Count = 1)

        Dim q = Query.SchemaUrlEQ(url)
        Dim result = col.FindOne(q)

        Dim dbMovie = CType(result.ToSchemaItem(), TVSeries)


        Assert.IsTrue(dbMovie.name = "The Wire (2002–2008)")

        col.RemoveAll()

    End Sub

End Class