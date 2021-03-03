Imports System
Imports System.IO


Module Program
    Sub Main(args As String())
        If args.Length = 0 Then Console.WriteLine("give path to fpr file") : Exit Sub
        If Not File.Exists(args(0)) Then
            Console.WriteLine("Cannot find " & args(0)) : Exit Sub
        End If
        Dim xdoc As New XDoc(args(0))
        System.Console.Write(xdoc.Markdown)
    End Sub
End Module
