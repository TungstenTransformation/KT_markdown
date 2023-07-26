Imports System.IO

Public Class MarkDown
    Private xdoc As XDoc
    Private Files As New List(Of String)
    Public Sub New(xdoc As XDoc)
        Me.xdoc = xdoc
    End Sub

    Friend Sub WriteAll(OutPutFolder As String)
        If Not Directory.Exists(OutPutFolder) Then Directory.CreateDirectory(OutPutFolder)
        For Each cl As Xml.XmlNode In xdoc.Classes
            Dim FileName As String = OutPutFolder & Path.DirectorySeparatorChar & xdoc.ClassName(cl)
            WriteFileIfChanged(FileName & ".md", xdoc.MarkDown(cl))
            WriteFileIfChanged(FileName & ".vb", xdoc.Script(cl))
        Next
        DeleteUnneededFiles(OutPutFolder)
    End Sub

    Private Sub WriteFileIfChanged(FileName As String, Content As String)
        'Only write file if new or content changed
        Files.Add(FileName)
        If File.Exists(FileName) AndAlso File.ReadAllText(FileName) = Content Then Exit Sub
        File.WriteAllText(FileName, Content)
    End Sub

    Private Sub DeleteUnneededFiles(Folder As String)
        'This deletes files from classes that no longer exist in project
        For Each f As String In Directory.GetFiles(Folder)
            If Not Files.Contains(f) Then File.Delete(f)
        Next
    End Sub
End Class
