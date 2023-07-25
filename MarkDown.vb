Imports System.IO

Public Class MarkDown
    Private xdoc As XDoc

    Public Sub New(xdoc As XDoc)
        Me.xdoc = xdoc
    End Sub

    Friend Sub WriteAll(OutPutFolder As String)
        If Not Directory.Exists(OutPutFolder) Then Directory.CreateDirectory(OutPutFolder)
        For Each cl As Xml.XmlNode In xdoc.Classes
            Dim FileName As String = OutPutFolder & Path.DirectorySeparatorChar & xdoc.ClassName(cl) & ".md"
            File.WriteAllText(FileName, xdoc.MarkDown(cl))
        Next
    End Sub
End Class
