Imports System.IO
Public Class XDocument
    Public ReadOnly filename As String
    Private _XFolder As XFolder = Nothing
    Public xml As Xml.XmlDocument = Nothing
    Private culture As Globalization.CultureInfo
    Private xmlstring As String = ""
    Public ReadOnly fields As New List(Of XdocField)
    Private locators As List(Of XdocField)
    ReadOnly Property XFolder() As XFolder
        Get
            If _XFolder Is Nothing Then FindXFolder()
            Return _XFolder
        End Get
    End Property

    Private Sub FindXFolder()
        Dim files() As String = IO.Directory.GetFiles(IO.Directory.GetParent(Me.filename).Parent.FullName, "*.xfd")
        If files.Length = 0 Then Return
        _XFolder = New XFolder(files(0))
    End Sub
    Property extractionClass() As String
        Get
            Try
                extractionClass = xml.SelectSingleNode("xdoc/@extclass").Value
            Catch ex As Exception
                extractionClass = ""
            End Try
        End Get
        Set(ByVal value As String)
            xml.SelectSingleNode("xdoc/@extclass").Value = value
        End Set
    End Property

    Public Sub New(ByVal XDocfilename As String, Optional ByVal culture As Globalization.CultureInfo = Nothing)
        Me.culture = culture
        filename = XDocfilename
    End Sub



    Private Sub parseXML()
        If xmlstring = "" Then loadXML()
        xml = New Xml.XmlDocument
        xml.LoadXml(xmlstring)
    End Sub

    Private Sub loadXML()
        Dim gz As New gzip
        xmlstring = gz.gzip2xmlstring(filename)
        'getFields()
    End Sub

    Public Sub New(ByVal XDocfilename As String, ByVal deleteXML As Boolean)
        'Used for saving memory
        Me.New(XDocfilename)
        parseXML()
        parseFields()
        If deleteXML Then
            Me.xml = Nothing
            For Each f As XdocField In fields
                f.xml = Nothing
            Next
        End If
    End Sub

    Public Function getTIFFs() As List(Of String)
        'we do this without parsing xml for speed
        If xmlstring = "" Then loadXML()
        getTIFFs = New List(Of String)
        Dim i As Integer = 0
        While xmlstring.IndexOf("sourcefile fname=", i) > 0
            Dim startoftiff As Integer = xmlstring.IndexOf("fname=", i) + 7
            i = xmlstring.IndexOf("ftype=", startoftiff)
            If i < 0 Then Exit While
            If xmlstring.IndexOf("</sourcefiles>", i) < 0 Then Exit While
            Dim tiff As String = xmlstring.Substring(startoftiff, i - 2 - startoftiff)
            getTIFFs.Add(tiff)
        End While
    End Function

    Public Shared Function readErrFile(ByVal xdocFileName As String) As String
        'This can get called without having even unzipped the xdoc
        Dim errfile As String = IO.Path.GetFileNameWithoutExtension(xdocFileName) & ".err"
        If Not System.IO.File.Exists(errfile) Then Return ""
        Dim tr As New IO.StreamReader(errfile, System.Text.Encoding.Default)
        readErrFile = tr.ReadToEnd
    End Function

    Public Shared Function getConflictFiles(ByVal xdocFileName As String) As List(Of String)
        Dim list As New List(Of String)
        Dim lines As String() = readErrFile(xdocFileName).Split(ControlChars.Cr)
        For Each line As String In lines
            If line.Contains("conflict with") Then
                Dim f As String = line.Split(""""c)(1)
                list.Add(System.IO.Path.GetFileName(f))
            End If
        Next
        Return list
    End Function

    Public ReadOnly Property PageCount() As Integer
        Get
            If xml Is Nothing Then parseXML()
            Return xml.SelectNodes("/xdoc/cdoc/pages/page").Count
        End Get
    End Property

    Public ReadOnly Property WordCount(Optional ByVal pageNo As Integer = -1) As Integer
        Get
            If xml Is Nothing Then parseXML()
            Dim xpath As String
            If pageNo = -1 Then
                xpath = "/xdoc/representations/representation/words/word"
            Else
                xpath = "/xdoc/representations/representation/words/word[@pindex=""" & pageNo & """]"
            End If
            Return xml.SelectNodes(xpath).Count
        End Get
    End Property

    Public ReadOnly Property LineCount() As Integer
        Get
            Dim xpath As String = "/xdoc/representations/representation/textlines/tline"
            If xml Is Nothing Then parseXML()
            Return xml.SelectNodes(xpath).Count
        End Get
    End Property

    Public Sub CountWords(ByRef words As Dictionary(Of String, Integer), ByVal pageMin As Integer, ByVal pageMax As Integer, _
                                  ByVal minWordlength As Integer, _
                                  Optional ByVal cultureName As String = "en-US", _
                                  Optional ByVal ignores As String = ".,;:?!-`0123456789#+~*^")
        If xml Is Nothing Then parseXML()
        Dim culture As New Globalization.CultureInfo(cultureName)
        Dim xpath As String = "/xdoc/representations/representation/words/word[@pindex>=""" & pageMin & """ and @pindex<=""" & pageMax & """]"
        For Each node As Xml.XmlNode In xml.SelectNodes(xpath)
            Dim word As String = node.InnerText
            'word = word.ToLower(culture)
            For Each ch As Char In ignores
                word = word.Replace(ch, "")
            Next
            If word.Length > minWordlength Then
                If words.ContainsKey(word) Then
                    words(word) += 1
                Else
                    words(word) = 1
                End If
            End If
        Next
    End Sub

    Public Function GetWords(ByVal pageMin As Integer, ByVal pageMax As Integer, ByVal minWordlength As Integer, ByVal ignores As String) As Dictionary(Of String, Dictionary(Of Integer, Integer))
        Dim LongBits As Integer = Math.Log(Long.MaxValue) / Math.Log(2)
        If xml Is Nothing Then parseXML()
        GetWords = New Dictionary(Of String, Dictionary(Of Integer, Integer))
        Dim xpath As String = "/xdoc/representations/representation/words/word[@pindex>=""" & pageMin & """ and @pindex<=""" & pageMax & """]"
        For Each node As Xml.XmlNode In xml.SelectNodes(xpath)
            Dim word As String = node.InnerText
            word = word.ToLower()
            For Each ch As Char In ignores
                word = word.Replace(ch, "")
            Next
            Dim pageNo As Integer = Integer.Parse(node.Attributes("pindex").Value)
            If GetWords.ContainsKey(word) Then
                If GetWords.Item(word).ContainsKey(pageNo) Then
                    GetWords.Item(word)(pageNo) += 1
                Else
                    GetWords.Item(word)(pageNo) = 1
                End If
            Else
                GetWords.Item(word) = New Dictionary(Of Integer, Integer)
                GetWords.Item(word)(pageNo) = 1
            End If
        Next
    End Function

    Public Function findField(ByVal fieldname As String) As XdocField
        If xml Is Nothing Then parseXML()
        For Each f As XdocField In fields
            If f.name = fieldname Then Return f
        Next
        Return Nothing
    End Function


    Public Sub eraseXML()
        xml = Nothing
    End Sub

    Public Sub parseFields()
        If xml Is Nothing Then parseXML()
        If fields.Count > 0 Then Exit Sub
        For Each node As Xml.XmlNode In xml.SelectNodes("/xdoc/fields/field")
            fields.Add(New XdocField(node, xml))
        Next
    End Sub

    Public Sub parseLocators()
        If xml Is Nothing Then parseXML()
        If locators IsNot Nothing Then Exit Sub
        locators = New List(Of XdocField)
        For Each node As Xml.XmlNode In xml.SelectNodes("/xdoc/locators/locator")
            locators.Add(New XdocField(node, xml, culture))
        Next
    End Sub

    Public Function Fieldexists(ByVal fieldname As String) As Boolean
        If xml Is Nothing Then parseXML()
        parseFields()
        For Each f As XdocField In fields
            If f.name = fieldname Then Return True
        Next
        Return False
    End Function

    Public Function addField(ByVal fieldName As String, ByVal valid As Boolean, ByVal confidence As Double, ByVal text As String) As XdocField
        If xml Is Nothing Then parseXML()
        If Not findField(fieldName) Is Nothing Then
            Throw New Exception("Can't add field '" & fieldName & "' to " & Me.filename & " as it already exists!!")
        End If
        Dim doc As New Xml.XmlDocument
        Dim field As Xml.XmlElement = doc.CreateElement("field")
        Dim t As Xml.XmlElement = doc.CreateElement("text")
        field.AppendChild(t)
        t.InnerText = text
        field.SetAttribute("valid", Math.Abs(CInt(valid)).ToString)
        field.SetAttribute("name", fieldName)
        field.SetAttribute("conf", confidence.ToString(New System.Globalization.CultureInfo("en-US")))
        addField = New XdocField(field, xml)
        fields.Add(addField)
    End Function

    Public Function getField(ByVal fieldname As String) As XdocField
        If xml Is Nothing Then parseXML()
        parseFields()
        For Each getField In fields
            If getField.name = fieldname Then Exit Function
        Next
        Throw New Exception("Asked for field '" + fieldname + "' which doesn't exist in xdoc '" + Me.filename + "'")
    End Function

    Public Function getLocator(ByVal locatorName As String) As XdocField
        If xml Is Nothing Then parseXML()
        parseLocators()
        For Each getLocator In locators
            If getLocator.name = locatorName Then Exit Function
        Next
        Throw New Exception("Asked for locator '" + locatorName + "' which doesn't exist in xdoc '" + Me.filename + "'")
    End Function

    Public Sub Save()
        Dim st As New MemoryStream()
        'xml.Save(newpath & Path.DirectorySeparatorChar & filename)
        xml.Save(st)
        Dim xmlstring As String
        xmlstring = xml.OuterXml()
        Dim gz As New gzip
        gz.utf82gzip(xmlstring, filename)
    End Sub
    Public Function getLines(ByVal minWordlength As Integer, Optional ByVal ignores As String = ".,;:?!-`0123456789#«+~*^""") As List(Of List(Of String))
        Dim lines As New List(Of List(Of String))
        Dim wordNodes As Xml.XmlNodeList = xml.SelectNodes("/xdoc/representations/representation/words/word")
        Dim words(0) As String
        Array.Resize(words, wordNodes.Count)
        For i As Integer = 0 To words.Length - 1
            words(i) = wordNodes(i).InnerText
            For Each ch As Char In ignores
                words(i) = words(i).Replace(ch, "")
            Next
        Next
        Dim lineNodes As Xml.XmlNodeList = xml.SelectNodes("/xdoc/representations/representation/textlines/tline/words")
        For Each linenode As Xml.XmlNode In lineNodes
            lines.Add(New List(Of String))
            Dim wordIDs As String() = linenode.InnerText.Split(";"c)
            For Each wordID As String In wordIDs
                If wordID <> "" Then
                    Dim word As String = words(CInt(wordID))
                    If word.Length >= minWordlength Then lines(lines.Count - 1).Add(word)
                End If
            Next
        Next
        Return lines
    End Function

End Class

Public Class XdocField
    Private doc As Xml.XmlDocument
    Public xml As Xml.XmlNode
    Private enUS As New Globalization.CultureInfo("en-US")
    Private culture As Globalization.CultureInfo
    Public ReadOnly name As String
    Public ReadOnly valid As Boolean
    Public ReadOnly orgvalid As Boolean
    Public ReadOnly orgtext As String
    Private _alternatives As List(Of XdocAlternative)
    Public Property alternatives() As List(Of XdocAlternative)
        Get
            If _alternatives Is Nothing Then getAlternatives()
            Return _alternatives
        End Get
        Set(ByVal value As List(Of XdocAlternative))
            _alternatives = value
        End Set
    End Property

    Private Sub getAlternatives()
        alternatives = New List(Of XdocAlternative)()
        For Each altXML As Xml.XmlNode In xml.SelectNodes("alts/alt")
            alternatives.Add(New XdocAlternative(altXML, culture))
        Next
    End Sub
    Property text() As String
        Get
            Try
                text = xml.SelectSingleNode("text").InnerText
            Catch ex As Exception
                text = ""
            End Try
        End Get
        Set(ByVal value As String)
            Try
                xml.SelectSingleNode("text").InnerText = value
            Catch ex As Exception
                Throw New Exception("This xdoc contains no text for field '" & name & "'")
            End Try
            Try
                xml.SelectSingleNode("orgtext").InnerText = value
            Catch ex As Exception
            End Try
            Try
                xml.SelectSingleNode("valtext").InnerText = value
            Catch ex As Exception
            End Try
            Try
                xml.SelectSingleNode("alts/alt[1]/text").InnerText = value
            Catch ex As Exception
            End Try
        End Set
    End Property
    Property confidence() As Double
        Get
            Try
                confidence = Double.Parse(xml.SelectSingleNode("@conf").Value, enUS)
            Catch ex As Exception
                confidence = 0
            End Try
        End Get
        Set(ByVal value As Double)
            Try
                xml.SelectSingleNode("@conf").Value = value.ToString(enUS)
            Catch ex As Exception
                Dim att As Xml.XmlAttribute
                att = doc.CreateAttribute("conf")
                att.Value = value.ToString(enUS)
                xml.Attributes.Append(att)
            End Try
        End Set
    End Property
    Public ReadOnly confidenceNext As Double = 0


    Public ReadOnly doublevalue As Double

    ReadOnly Property Classname() As String
        Get
            Return xml.ParentNode.Attributes("name").Value
        End Get
    End Property
    Public Sub New(ByVal node As Xml.XmlNode, ByVal doc As Xml.XmlDocument, Optional ByVal culture As Globalization.CultureInfo = Nothing)
        Me.culture = culture
        Me.doc = doc
        xml = node
        name = node.SelectSingleNode("@name").Value
        Try
            confidenceNext = Double.Parse(node.SelectSingleNode("alts/alt[2]/@conf").Value, enUS)
        Catch ex As Exception
            confidenceNext = 0
        End Try
        Try
            doublevalue = Double.Parse(node.SelectSingleNode("@DblVal").Value, enUS)
        Catch ex As Exception
            doublevalue = Double.NaN
        End Try
        Dim v As String = node.SelectSingleNode("@valid").Value
        If v = "1" Then valid = True Else valid = False
        Try
            orgtext = node.SelectSingleNode("orgtext").InnerXml
            v = node.SelectSingleNode("@orgvalid").Value
            If v = "1" Then orgvalid = True Else orgvalid = False
        Catch ex As Exception
            orgtext = Nothing
            orgvalid = Nothing
        End Try
    End Sub
End Class

Public Class XDocWord
    Public _text As String
    Public words As Integer()
    ReadOnly Property text() As String
        Get
            Return _text
        End Get
    End Property
    Public id As Integer
    Public count As Integer
    Public _page As Integer = -1
    ReadOnly Property page() As Integer
        Get
            Return _page
        End Get
    End Property
    Public _line As Integer
    Protected _left As Integer
    ReadOnly Property left() As Integer
        Get
            Return _left
        End Get
    End Property
    Protected _top As Integer
    ReadOnly Property top() As Integer
        Get
            Return _top
        End Get
    End Property
    Public ReadOnly Property right() As Integer
        Get
            Return _left + _width
        End Get
    End Property
    Public ReadOnly Property bottom() As Integer
        Get
            Return _top + _height
        End Get
    End Property

    Protected _width As Integer
    Public Property width() As Integer
        Get
            Return _width
        End Get
        Set(ByVal value As Integer)
            _width = value
        End Set
    End Property

    Protected _height As Integer
    Public Property height() As Integer
        Get
            Return _height
        End Get
        Set(ByVal value As Integer)
            _height = value
        End Set
    End Property
    Private _value As Double
    Public ReadOnly Property value() As Double
        Get
            Return _value
        End Get
    End Property

    Protected _confidence As Double
    Public Property confidence() As Double
        Get
            Return _confidence
        End Get
        Set(ByVal value As Double)
            _confidence = value
        End Set
    End Property

    ReadOnly Property squaredistance() As Integer
        Get
            Return _left + _width + _top + _height
        End Get
    End Property
    Public _typeWeight As Double
    Property typeWeight() As Double
        Get
            Return _typeWeight
        End Get
        Set(ByVal value As Double)
            _typeWeight = value
        End Set
    End Property
    Private Sub parseXML()
        _text = _xml.SelectSingleNode("text").InnerXml
        _left = CInt(_xml.SelectSingleNode("@left").Value)
        _width = CInt(_xml.SelectSingleNode("@width").Value)
        _top = CInt(_xml.SelectSingleNode("@top").Value)
        height = CInt(_xml.SelectSingleNode("@height").Value)
        _page = CInt(_xml.SelectSingleNode("@page").Value)
        Try
            _value = Double.Parse(_text.Replace("%"c, " "c).Trim, culture)
        Catch ex As Exception
            _value = Double.NaN
        End Try
        Try
            _confidence = Double.Parse(_xml.SelectSingleNode("@conf").Value, Globalization.CultureInfo.InvariantCulture)
        Catch ex As Exception
            Throw New Exception("Couldn't parse '" & _xml.SelectSingleNode("@conf").Value & "' as double")
        End Try
        Try
            Dim ws As String() = _xml.SelectSingleNode("words").InnerText.Split(";"c)
            ReDim words(ws.Length - 2)
            For w As Integer = 0 To ws.Length - 2
                words(w) = CInt(ws(w))
            Next
        Catch ex As Exception
        End Try
    End Sub
    Private _xml As Xml.XmlNode
    Private culture As New Globalization.CultureInfo("en-US")
    Public Sub New(ByVal altXML As Xml.XmlNode, _
                   Optional ByVal culture As Globalization.CultureInfo = Nothing)
        _xml = altXML
        If culture IsNot Nothing Then Me.culture = culture
        parseXML()
    End Sub
End Class

Public Class XdocAlternatives
    Public alternative As List(Of XdocAlternative)
End Class

Public Class XdocAlternative
    Inherits XDocword


    Public Sub New(ByVal altXML As Xml.XmlNode, _
                   Optional ByVal culture As Globalization.CultureInfo = Nothing)
        MyBase.New(altXML, culture)
    End Sub
End Class