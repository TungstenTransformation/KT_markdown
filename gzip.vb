Imports System.IO
Imports System.IO.Compression
Friend Class gzip
    Function gzip2xmlstring(ByVal filename As String) As String
        Dim xmlstring As String = Decompress(filename)
        While Not xmlstring.EndsWith(">") 'Trim final nulls from end of xml
            xmlstring = xmlstring.Substring(0, xmlstring.Length - 1)
        End While
        Return xmlstring
    End Function
    Function gzip2xml(ByVal filename As String) As Xml.XmlDocument
        gzip2xml = New Xml.XmlDocument
        gzip2xml.LoadXml(Decompress(filename))
    End Function

    Public Sub Compress(ByVal text As String, ByVal filename As String)
        Dim outFilestream As FileStream
        Dim gzipStream As GZipStream
        Dim utf8 As New System.Text.UTF8Encoding
        Dim buffer As Byte() = utf8.GetBytes(text)
        'Dim memoryBuffer As New MemoryStream()
        Try
            outFilestream = New FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write)
            gzipStream = New GZipStream(outFilestream, CompressionMode.Compress, True)
            gzipStream.Write(buffer, 0, buffer.Length)
            gzipStream.Flush()
            gzipStream.Close()
            outFilestream.Close()
        Catch ex As Exception
            Throw New Exception("Unable to save '" & filename & "." & ControlChars.NewLine & ex.Message)
        End Try
        gzipStream = Nothing
        outFilestream = Nothing
    End Sub

    Public Function Decompress(filename As String) As String
        Dim gz As New GZipStream(File.OpenRead(filename), CompressionMode.Decompress)
        Dim sr As New StreamReader(gz)
        Return sr.ReadToEnd
    End Function

End Class