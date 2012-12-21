Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports MicrodataNet.Core.Parsing
Imports MicrodataNet.Lexicon
Imports MicrodataNet.Core
Imports MicrodataNet.Lexicon.Extensions


<TestClass()> Public Class UnitTest1

    Private parser_no_lex As Parser
    Private parsed_no_lex_doc As SchemaDocument

    Private parser_lex As Parser
    Private parser_lex_doc As SchemaDocument

    Sub New()

        parser_lex = New Parser(GetType(MicrodataNet.Lexicon.AboutPage).Assembly)
        parser_lex.load("http://www.imdb.com/title/tt0306414/")
        parser_lex_doc = parser_lex.parse()

        parser_no_lex = New Parser()
        parser_no_lex.load("http://www.imdb.com/title/tt0306414/")
        parsed_no_lex_doc = parser_no_lex.parse()

        

    End Sub

    <TestMethod()> Public Sub DifferentAncestors()

        Dim accountingService As New AccountingService

        Assert.IsTrue(GetType(IFinancialService).IsInstanceOfType(accountingService))
        Assert.IsTrue(GetType(IProfessionalService).IsInstanceOfType(accountingService))

    End Sub

    <TestMethod()> Public Sub TestLoadingDocumentWithoutLexicon()

        Dim movie As SchemaDefinitionBase = parsed_no_lex_doc.GetBySchemaUrl("http://schema.org/TVSeries").FirstOrDefault

        Assert.IsNotNull(movie)

        Assert.IsTrue(movie.HasProperty("name"))

        Assert.AreEqual(Of String)("The Wire (2002–2008)", movie("name"))

    End Sub

    <TestMethod()> _
    Public Sub TestLoadingDocumentWithLexicon()

        Dim movie As TVSeries = parser_lex_doc.Items.OfType(Of TVSeries)().FirstOrDefault

        Assert.IsNotNull(movie)

        Assert.AreEqual(Of String)("The Wire (2002–2008)", movie.name)

    End Sub

    <TestMethod()> _
    Public Sub TestInheritance()
        Dim movie As CreativeWork = parser_lex_doc.Items.FindAs(Of CreativeWork)().FirstOrDefault()

        Assert.IsNotNull(movie)

        Assert.AreEqual(Of String)("The Wire (2002–2008)", movie.name)
    End Sub

End Class