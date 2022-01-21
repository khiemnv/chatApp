Sub Unzip(strFile)
' This routine unzips a file. NOTE: The files are extracted to a folder '
' in the same location using the name of the file minus the extension.  '
' EX. C:\Test.zip will be extracted to C:\Test '
'strFile (String) = Full path and filename of the file to be unzipped. '
    Set fso = CreateObject("Scripting.FileSystemObject")
    strFile = fso.GetAbsolutePathName(strFile)
    path = fso.GetParentFolderName(strFile) & "\" & fso.GetBaseName(strFile)
    fso.CreateFolder(path)
    pathToZipFile = strFile
    extractTo= path
    set objShell = CreateObject("Shell.Application")
    set filesInzip=objShell.NameSpace(pathToZipFile).items
    objShell.NameSpace(extractTo).CopyHere(filesInzip)
    'fso.DeleteFile pathToZipFile, True
    Set fso = Nothing
    Set objShell = Nothing
End Sub 'Unzip
Sub crtShortcut(Path)
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set tShell = CreateObject("WScript.Shell")
    Path = fso.GetAbsolutePathName(Path)
    zfile = findFile(Path,"register.exe")
    Set link = tShell.CreateShortcut(Path & "\register.lnk")
    link.Description = "register tool"
    link.TargetPath = "" & zfile & ""
    link.WindowStyle = 2
    link.WorkingDirectory = fso.GetParentFolderName(zfile)
    link.IconLocation = zfile
    link.Save
    Set fso = Nothing
    Set objShell = Nothing
End Sub
Function findFile(zdir, zname)
    Set fso = CreateObject("Scripting.FileSystemObject")
    zPath = ""
    For Each tfile In fso.GetFolder(zdir).Files
        If tfile.Name = zname Then
            zPath = tfile.Path
            Exit For
        End If
    Next
    If zPath = "" Then
        For Each tfolder In fso.GetFolder(zdir).SubFolders
            zPath = findFile(tfolder.Path, zname)
            If zPath <> "" Then Exit For
        Next
    End If
    findFile = zPath
    Set fso = Nothing
End Function

'Unzip(".\search.zip")
crtShortCut(".")