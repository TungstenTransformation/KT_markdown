Imports System
Imports System.IO


Module Program
    Sub Main(args As String())
        Dim gz As New gzip
        Dim XML As Xml.XmlDocument
        Dim out As String = ""
        If args.Length = 0 Then Console.WriteLine("give path to fpr file") : Exit Sub
        If Not File.Exists(args(0)) Then
            Console.WriteLine("Cannot find " + args(0)) : Exit Sub
        End If
        XML = gz.gzip2xml(args(0))
        out += "# KT Project auto documentation" + vbCrLf
        out += "project version= " + XML.SelectSingleNode("/project").Attributes("version").InnerText + vbCrLf
        For Each cl As Xml.XmlNode In XML.SelectNodes("//class")
            Dim classname As String = cl.Attributes("name").InnerText
            If classname = "" Then classname = "project"
            out += "# " + classname + vbCrLf
            Dim script As String = cl.SelectSingleNode("script").InnerText
            If script <> "" Then out += "script" + vbCrLf + "```vb" + vbCrLf + +"```" + vbCrLf
        Next
        System.Console.Write(out)
    End Sub
End Module
