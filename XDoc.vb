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

    Private ReadOnly Property Script(cl As Xml.XmlNode) As String
        Get
            Script = cl.InnerText
            Return "## Script" & eol & "```vb" & vbCrLf & Script & "```" & eol
        End Get
    End Property
    Private ReadOnly Property ProjectProperties As String
        Get
            ProjectProperties = "## Formatters" & eol
            For Each dict As Xml.XmlNode In XML.SelectNodes("//dictionary")
                ProjectProperties &= "* " & dict.Attributes("name").InnerText & eol
            Next
            For Each n As Xml.XmlNode In XML.SelectNodes("project/format")
                ProjectProperties &= "* **" & n.Attributes("name").InnerText & "** *" & n.Attributes("type").InnerText & "*" & eol
            Next
            Dim Settings As Xml.XmlNode = XML.SelectSingleNode("/project/settings")
            ProjectProperties &= "*Default Date   Formatter*: " & Settings.Attributes("DefDate").InnerText & eol
            ProjectProperties &= "*Default Amount Formatter*: " & Settings.Attributes("DefAmnt").InnerText & eol
            ProjectProperties &= "## Databases" & eol
            For Each n As Xml.XmlNode In XML.SelectNodes("/project/database")
                ProjectProperties &= "Database: " & n.Attributes("name").InnerText & eol
            Next
            ProjectProperties &= "## Dictionaries" & eol
            For Each n As Xml.XmlNode In XML.SelectNodes("/project/dict")
                ProjectProperties &= "Dictionary: " & n.Attributes("name").InnerText & eol
            Next
            ProjectProperties &= "## Table Settings" & eol
            For Each n As Xml.XmlNode In XML.SelectNodes("project/glbcol")
                ProjectProperties &= "Global Column " & n.Attributes("gcid").InnerText & " : " & n.Attributes("dcolheaderlocalization").InnerText & eol
            Next
            For Each n As Xml.XmlNode In XML.SelectNodes("project/tablemod")
                ProjectProperties &= "Table Model: " & n.Attributes("name").InnerText & eol
            Next
            ProjectProperties &= "## Recognition Profiles" & eol
            For Each n As Xml.XmlNode In XML.SelectNodes("//RecogProfile")
                ProjectProperties &= IIf(n.Attributes("Type").InnerText = "1", "page", "zonal") & " recognition profile : " &
                n.Attributes("Name").InnerText & " : " & n.Attributes("Class").InnerText & eol
            Next
        End Get
    End Property
    Private ReadOnly Property Fields(cl As Xml.XmlNode) As String
        Get
            Fields = "## Fields" & vbCrLf
            For Each f As Xml.XmlNode In cl.SelectNodes("field")
                Fields &= Field(f)
            Next
        End Get
    End Property
    Private ReadOnly Property Field(f As Xml.XmlNode) As String
        Get
            Field = "* " & f.Attributes("name").InnerText
            Dim loc As String = f.Attributes("locator").InnerText
            If loc <> "" Then Field &= String.Format(" *{0}*", loc)
            Dim sf As String = f.Attributes("locsubf").InnerText
            If sf <> "" Then Field &= String.Format(":*{0}*", sf)
            Field &= eol
        End Get
    End Property

    Private ReadOnly Property Locators(cl As Xml.XmlNode) As String
        Get
            Locators = "## Locators" & vbCrLf
            For Each loc As Xml.XmlNode In cl.SelectNodes("locator")
                Locators &= "* " & Locator(loc) & eol
            Next
        End Get
    End Property

    Private ReadOnly Property Locator(loc As Xml.XmlNode) As String
        Get
            Locator = "**" & loc.Attributes("name").InnerText & "** "
            Dim locType As String = ""
            Dim extrname As Xml.XmlNode = loc.SelectSingleNode("extrname")
            If extrname IsNot Nothing Then
                locType = loc.SelectSingleNode("extrname").InnerText
            ElseIf loc.Attributes IsNot Nothing AndAlso loc.Attributes("script") IsNot Nothing Then
                If loc.Attributes("script").InnerText = "1" Then locType = "Script Locator [[Script](#Script)]"
            End If
            Locator &= "*" & locType & "*" & eol
            If loc.Attributes("sfcount") IsNot Nothing Then
                Dim subfieldcount As Long = Long.Parse(loc.Attributes("sfcount").InnerText)
                If subfieldcount > 0 Then
                    For sf = 0 To subfieldcount - 1
                        Locator &= "  * " & loc.Attributes("sbfld" & sf.ToString).InnerText & eol
                    Next
                End If

            End If
        End Get
    End Property

    Public Iterator Function Classes() As System.Collections.IEnumerable
        'Yield XML.SelectSingleNode("/project")
        For Each cl As Xml.XmlNode In XML.SelectNodes("//class")
            Yield cl
        Next
    End Function

    Public ReadOnly Property MarkDown(cl As Xml.XmlNode) As String
        Get
            Dim Labels As String = "Fields Locators Script"
            Const ProjectLabels As String = "Fields Locators Formatters Databases Dictionaries Tables Recognition-Profiles Script"
            MarkDown = ""
            If IsProjectClass(cl) Then
                MarkDown = "# Kofax Transformation Auto-documentation" & vbCrLf
                MarkDown &= "*created by [KT markdown](https://github.com/KofaxRTransformation/KT_markdown#kt_markdown)*  " & vbCrLf
                MarkDown &= "project version= " & Attribute("/project", "version") & vbCrLf
            End If
            MarkDown &= "# Class: " & ClassName(cl) & vbCrLf
            If IsProjectClass(cl) Then Labels = ProjectLabels
            For Each label As String In Split(Labels)
                MarkDown &= String.Format("[[{0}](#{1})] ", label.Replace("-", " "), label)
            Next
            MarkDown &= eol
            MarkDown &= Fields(cl)
            MarkDown &= Locators(cl)
            If IsProjectClass(cl) Then MarkDown &= ProjectProperties
            MarkDown &= Script(cl)
        End Get
    End Property
    Public ReadOnly Property IsProjectClass(cl As Xml.XmlNode) As Boolean
        Get
            Return ClassName(cl) = "Project"
        End Get
    End Property

    Public ReadOnly Property ClassName(cl As Xml.XmlNode) As String
        Get
            ClassName = cl.Attributes("name").InnerText
            If ClassName = "" Then ClassName = "Project"
            Return ClassName
        End Get
    End Property
End Class