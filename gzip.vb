Imports System.IO
Imports System.IO.Compression
Friend Class gzip
    Function gzip2xmlstring(ByVal filename As String) As String
        Dim xmlstring As String = gzip2utf8(filename)
        While Not xmlstring.EndsWith(">") 'Trim final nulls from end of xml
            xmlstring = xmlstring.Substring(0, xmlstring.Length - 1)
        End While
        Return xmlstring
    End Function
    Function gzip2xml(ByVal filename As String) As Xml.XmlDocument
        gzip2xml = New Xml.XmlDocument
        gzip2xml.LoadXml(gzip2xmlstring(filename))
    End Function

    Public Sub utf82gzip(ByVal text As String, ByVal filename As String)
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

    Public Function gzip2utf8(ByVal filename As String) As String
        Dim inputFile As FileStream
        Dim compressedZipStream As GZipStream
        ' Determine the uncompressed size of the file;
        Try
            inputFile = New FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)
            compressedZipStream = New GZipStream(inputFile, CompressionMode.Decompress)
        Catch ex As Exception
            Throw New Exception("Failed to load filename! " & ex.Message)
        End Try
        Dim offset, totalBytes As Integer
        offset = 0
        totalBytes = 0
        Dim smallBuffer(1024) As Byte
        While (True)
            Dim bytesRead As Integer
            bytesRead = compressedZipStream.Read(smallBuffer, 0, 1024)
            If bytesRead = 0 Then
                Exit While
            End If
            offset += bytesRead
            totalBytes += bytesRead
        End While
        compressedZipStream.Close()
        'Open and read the contents of the file now that
        'we know the uncompressed size
        inputFile = New FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)

        'Decompress the file contents
        compressedZipStream = New GZipStream(inputFile, CompressionMode.Decompress)
        Dim buffer(totalBytes) As Byte
        compressedZipStream.Read(buffer, 0, totalBytes)
        compressedZipStream.Close()
        inputFile.Close()
        Dim enc As New System.Text.UTF8Encoding
        Return enc.GetString(buffer)
    End Function

End Class