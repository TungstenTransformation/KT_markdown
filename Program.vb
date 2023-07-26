Imports System
Imports System.IO

Module Program
    Private MarkDownFolder As String = "md" ' this is the subfolder to hold the markdown files
    Private HelpText As String = "\nKT_Markdown.exe [fprFileName] [subfolderName]\n If [fprFileName] is missing then look in current folder.\n If [subFolderName] is missing then output to folder 'md'."
    Sub Main(args As String())
        Dim fprFilename As String = ""
        Console.WriteLine("Kofax Transformation Markdown Version 1.0.3")
        Console.WriteLine("https://github.com/KofaxTransformation/KT_Markdown")
        Dim Dir As New DirectoryInfo(System.IO.Path.GetDirectoryName(Reflection.Assembly.GetEntryAssembly().Location))
        If args.Length > 1 Then 'both fpr and folder given
            fprFilename = args(0)
            MarkDownFolder = args(1)
        ElseIf args.Length > 0 AndAlso args(0).EndsWith(".fpr") Then 'fpr filename only
            fprFilename = args(0)
        ElseIf args.Length > 0 Then 'outputfoldername only
            MarkDownFolder = args(0)
        End If
        If fprFilename = "" Then
            'look in current folder for fpr file
            For Each FileInfo In Dir.GetFiles("*.fpr")
                fprFilename = FileInfo.FullName
            Next
            If fprFilename = "" Then
                Console.WriteLine("give path to fpr file or put exe into project folder" & HelpText)
                Exit Sub
            End If
        End If
        If Not File.Exists(fprFilename) Then
            Console.WriteLine("Cannot find " & fprFilename & HelpText)
            Exit Sub
        End If
        Dim xdoc As New XDoc(fprFilename)
        Dim MarkDown As New MarkDown(xdoc)
        MarkDown.WriteAll(IO.Path.GetDirectoryName(fprFilename) & IO.Path.DirectorySeparatorChar & MarkDownFolder)
    End Sub
End Module
