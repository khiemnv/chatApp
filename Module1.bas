Attribute VB_Name = "Module1"
Sub Publish(zPub, zProj)
    'Dim fso As FileSystemObject
    'Dim tFile As File
    'Dim tFolder As folder
    '//crt folder
    '//copy file
    '//zip
    Set fso = CreateObject("Scripting.FileSystemObject")
    zPub = fso.GetAbsolutePathName(zPub)
    zProj = fso.GetAbsolutePathName(zProj)
    
    zTgt = zPub & "\dangky"
    zRls = zProj & "\bin\x64\Release"
    
    arr = Split(zTgt, "\")
    zPath = ""
    For Each zTxt In arr
        zPath = zPath & zTxt
        If Not fso.FolderExists(zPath) Then
            fso.CreateFolder zPath
        End If
        zPath = zPath & "\"
    Next
    
    '//mycopy zRls, zTgt
    Set tLst = CreateObject("Scripting.Dictionary")
    tLst.Add 0, Array(zRls, zTgt)
    nCount = 0
    For i = 0 To 1000
        If tLst.Count = i Then Exit For
        
        rec = tLst(i)
        
        zSrc = rec(0)
        zDst = rec(1)
        For Each tFile In fso.GetFolder(zSrc).Files
            zTxt = zDst & "\" & tFile.Name
            If fso.FileExists(zTxt) Then
                If compareFile(zTxt, tFile.Path) Then
                    '//same
                Else
                    fso.CopyFile tFile.Path, zTxt, True
                    nCount = nCount + 1
                End If
            Else
                fso.CopyFile tFile.Path, zTxt
                nCount = nCount + 1
            End If
        Next
        For Each tFolder In fso.GetFolder(zSrc).SubFolders
            zTxt = zDst & "\" & tFolder.Name
            If Not fso.FolderExists(zTxt) Then
                fso.CreateFolder zTxt
            End If
            tLst.Add tLst.Count, Array(tFolder.Path, zTxt)
        Next
    Next
    If (tLst.Count > 1000) Then
        MsgBox "[err] too long"
    End If
    MsgBox "copied " & nCount
    
    'zip zTgt
    'ArchiveFolder zPub & "\search"
End Sub

Sub ArchiveFolder(oFile)

    Set oShell = CreateObject("WScript.Shell")
    oShell.Run "%comspec% /c ""C:\Program Files\7-Zip\7z.exe"" a " & oFile & ".zip " & oFile & " -tzip", , True

End Sub

'if same return true
Function compareFile(zFile1, zFile2)
    'Dim tFile1 As File
    'Dim tFile2 As File
    'Dim fso As FileSystemObject
    
    'get file modify date time
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set tFile1 = fso.GetFile(zFile1)
    Set tFile2 = fso.GetFile(zFile2)
    
    'check
    compareFile = tFile1.DateLastModified = tFile2.DateLastModified
    
End Function
Sub main()
txt1 = ActiveSheet.Range("A1").Value
txt2 = ActiveSheet.Range("A2").Value
Publish txt1, txt2

End Sub
