Public Class XDoc
    Private ReadOnly XML As Xml.XmlDocument
    Sub New(filename As String)
        Dim gz As New gzip
        XML = gz.gzip2xml(filename)
    End Sub
    ReadOnly Property Markdown As String
        Get
            Markdown = "# KT Project auto documentation" & vbCrLf
            Markdown &= "project version= " & XML.SelectSingleNode("/project").Attributes("version").InnerText & vbCrLf
            Markdown &= Classes
            Markdown &= Scripts
            Markdown &= Properties
        End Get
    End Property
    Private ReadOnly Property Scripts As String
        Get
            Throw New NotImplementedException
        End Get
    End Property
    Private ReadOnly Property Properties As String
        Get
            Throw New NotImplementedException
        End Get
    End Property
    Private ReadOnly Property Classes As String
        Get
            Classes = ""
            For Each cl As Xml.XmlNode In XML.SelectNodes("//class")
                Dim classname As String = cl.Attributes("name").InnerText
                If classname = "" Then classname = "project"
                Classes &= "# Class: " & classname & vbCrLf
                For Each field As Xml.XmlNode In cl.SelectNodes("field")
                    Classes &= " * field: " & field.Attributes("name").InnerText & ":" & vbCrLf
                Next
                For Each locator As Xml.XmlNode In cl.SelectNodes("locator")
                    Classes &= " * locator: " & locator.Attributes("name").InnerText & ":"
                    Try
                        Classes &= locator.SelectSingleNode("extrname").InnerText
                    Catch ex As Exception

                    End Try
                    Classes &= vbCrLf
                Next
                Dim script As String = cl.SelectSingleNode("script").InnerText
                If script <> "" Then Classes &= "script" & vbCrLf & "```vb" & vbCrLf & script & "```" & vbCrLf
            Next
        End Get
    End Property
End Class