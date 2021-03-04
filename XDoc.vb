Public Class XDoc
    Private ReadOnly XML As Xml.XmlDocument
    Const eol = "  " & vbCrLf
    Sub New(filename As String)
        Dim gz As New gzip
        XML = gz.gzip2xml(filename)
    End Sub
    Private Function Attribute(xpath As String, att As String) As String
        Return XML.SelectSingleNode(xpath).Attributes(att).InnerText
    End Function

    ReadOnly Property Markdown As String
        Get
            Markdown = "# KT Project auto documentation" & vbCrLf
            Markdown &= "project version= " & Attribute("/project", "version") & vbCrLf
            Markdown &= ClassTree
            Markdown &= Scripts
            Markdown &= Properties
        End Get
    End Property
    Private ReadOnly Property Scripts As String
        Get
            Scripts = ""
            'project level script
            Dim script As String = XML.SelectSingleNode("/project/script").InnerText
            If script <> "" Then Scripts &= "## Project Level Script" & vbCrLf & "```vb" & vbCrLf & script & "```" & vbCrLf
            For Each cl As Xml.XmlNode In XML.SelectNodes("//class")
                script = cl.SelectSingleNode("script").InnerText
                If script <> "" Then Scripts &= "## Script for class '" & cl.Attributes("name").InnerText & "'" & vbCrLf & "```vb" & vbCrLf & script & "```" & vbCrLf
            Next
        End Get
    End Property
    Private ReadOnly Property Properties As String
        Get
            Properties = "## Formatters" & eol
            For Each dict As Xml.XmlNode In XML.SelectNodes("//dictionary")
                Properties &= "* " & dict.Attributes("name").InnerText & eol
            Next
            For Each n As Xml.XmlNode In XML.SelectNodes("project/format")
                Properties &= n.Attributes("type").InnerText & " : " & n.Attributes("type").InnerText & eol
            Next
            Dim Settings As Xml.XmlNode = XML.SelectSingleNode("/project/settings")
            Properties &= "Default Date   Formatter: " & Settings.Attributes("DefDate").InnerText & eol
            Properties &= "Default Amount Formatter: " & Settings.Attributes("DefAmnt").InnerText & eol
            Properties &= "## Databases and Dictionaries" & eol
            For Each n As Xml.XmlNode In XML.SelectNodes("/project/dict")
                Properties &= "Dictionary: " & n.Attributes("name").InnerText & eol
            Next
            Properties &= "## Table Settings" & eol
            For Each n As Xml.XmlNode In XML.SelectNodes("project/glbcol")
                Properties &= "Global Column: " & n.Attributes("gcid").InnerText & " : " & n.Attributes("dcolheaderlocalization").InnerText & eol
            Next
            For Each n As Xml.XmlNode In XML.SelectNodes("project/tablemod")
                Properties &= "Table Model: " & n.Attributes("name").InnerText & eol
            Next

            For Each n As Xml.XmlNode In XML.SelectNodes("RecogProfile")
                Properties &= IIf(n.Attributes("Type").InnerText = "1", "page", "zonal") & " recognition profile : " &
                n.Attributes("Name").InnerText & " : " & n.Attributes("Class").InnerText & eol
            Next
        End Get
    End Property
    Private ReadOnly Property ClassTree As String
        Get
            ClassTree = ""
            For Each cl As Xml.XmlNode In XML.SelectNodes("//class")
                Dim classname As String = cl.Attributes("name").InnerText
                If classname = "" Then classname = "project"
                ClassTree &= "# Class: " & classname & vbCrLf
                For Each field As Xml.XmlNode In cl.SelectNodes("field")
                    ClassTree &= " * field: " & field.Attributes("name").InnerText & ":" & vbCrLf
                Next
                For Each locator As Xml.XmlNode In cl.SelectNodes("locator")
                    ClassTree &= " * locator: " & locator.Attributes("name").InnerText & ":"
                    Try
                        ClassTree &= locator.SelectSingleNode("extrname").InnerText
                    Catch ex As Exception

                    End Try
                    ClassTree &= vbCrLf
                Next

            Next
        End Get
    End Property
End Class