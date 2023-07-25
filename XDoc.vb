Public Class XDoc
    Private ReadOnly XML As Xml.XmlDocument
    Const eol = "  " & vbCrLf
    Private FileName As String
    Sub New(FileName As String)
        Dim gz As New gzip
        XML = gz.gzip2xml(FileName)
        Me.FileName = FileName
    End Sub
    Private Function Attribute(Node As Xml.XmlNode, att As String, DefaultValue As String) As String
        'return "" if missing attribute. XDOC has missing attributes if empty or not default
        Dim A As Xml.XmlAttribute = Node.Attributes(att)
        If A Is Nothing Then Return DefaultValue Else Return A.InnerText
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
            ProjectProperties &= "## Recognition Profiles" & eol & RecogProfiles() & eol
            ProjectProperties &= "## Script Variables" & eol & ScriptVariables() & eol
        End Get
    End Property

    Private ReadOnly Property RecogProfiles() As String
        Get
            Dim Profiles As Xml.XmlNode = XML.SelectSingleNode("/project/RecogProfiles")
            Dim DefOMR As String = Profiles.Attributes("DefZrOmr").InnerText
            Dim DefOCR As String = Profiles.Attributes("DefZrOcr").InnerText
            Dim DefPage As String = Profiles.Attributes("DefPr").InnerText
            RecogProfiles = ""
            For Each n As Xml.XmlNode In XML.SelectNodes("//RecogProfile")
                RecogProfiles &= IIf(n.Attributes("Type").InnerText = "1", "page", "zonal") & " recognition profile : " & n.Attributes("Name").InnerText
                Dim GUID As String = n.Attributes("guid").InnerText
                If GUID = DefOMR Or GUID = DefOCR Or GUID = DefPage Then RecogProfiles &= "*"
                RecogProfiles &= eol
            Next
        End Get
    End Property

    Private ReadOnly Property ScriptVariables() As String
        Get
            ScriptVariables = ""
            Dim vars As New Xml.XmlDocument
            vars.Load(FileName.Replace(".fpr", "_ScriptVariables.xml"))
            For Each var As Xml.XmlNode In vars.SelectNodes("//var")
                Dim key As String = var.Attributes("key").InnerText
                Dim value As String = var.Attributes("value").InnerText
                If key.ToLower.Contains("key") Or key.ToLower.Contains("token") Then
                    value = "*****"  'obscure sensitive data
                End If
                ScriptVariables &= String.Format("* **{0}** : {1}" & eol, key, value)
            Next
        End Get
    End Property

    Private ReadOnly Property Fields(cl As Xml.XmlNode) As String
        Get
            Fields = "|Group| Field | Locator | SubField | Formatter | Copy Conf|Valid Conf|Min Distance|" & eol
            Fields &= "|----|-------|---------|----------|-----------|----------|----------|------------|" & eol
            For Each f As Xml.XmlNode In cl.SelectNodes("field")
                Fields &= Field(f)
            Next
        End Get
    End Property
    Private ReadOnly Property Field(f As Xml.XmlNode) As String
        Get
            Dim groupName As String = f.Attributes("gname").InnerText
            Dim name As String = f.Attributes("name").InnerText
            Dim loc As String = f.Attributes("locator").InnerText
            Dim sf As String = f.Attributes("locsubf").InnerText
            Dim formatter As String = f.Attributes("fldfrmt").InnerText
            Dim confidence As String = Percent(Attribute(f, "conf", "0.80"))
            Dim distance As String = Percent(Attribute(f, "dist", "0.10"))
            Dim confidencecpy As String = Percent(Attribute(f, "confcpy", "0.10"))
            Field = String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|" & eol, groupName, name, loc, sf, formatter, confidence, distance, confidencecpy)
        End Get
    End Property

    Private Function Percent(p As String) As String
        Return FormatPercent(Double.Parse(p), 0)
    End Function

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
            Const ProjectLabels As String = "Fields Locators Formatters Databases Dictionaries Tables Recognition-Profiles Script-Variables Script"
            MarkDown = ""
            If IsProjectClass(cl) Then
                MarkDown = "# Kofax Transformation Auto-documentation" & vbCrLf
                MarkDown &= "*created by [KT markdown](https://github.com/KofaxRTransformation/KT_markdown#kt_markdown)*  " & vbCrLf
                MarkDown &= "project version= " & XML.SelectSingleNode("/project").Attributes("version").InnerText & vbCrLf
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