﻿Imports System.Xml
Imports System.Threading

Public Class Settings

#Region "Variables"
    '*-----------------*'
    '*----Variables----*'
    '*-----------------*'
    Dim defaultsourcePath As String 'set via code - read from settings and set like that.
    Public sourcepatharray As ArrayList = WinBackupper.home.sourcepatharray 'reference to form1.vb array. (which is loaded first)
    Dim defaultbackupPath As String 'set via code - read from settings and set like that.
    Public backupPatharray As ArrayList = WinBackupper.home.backupPatharray 'reference to form1.vb array. (which is loaded first)
    Dim tempSourcePath As String
    Dim tempBackupPath As String
    Dim GlobalSeperator As String = WinBackupper.home.GlobalSeperator ' seperatir used to combine/cut strings (like allsourcepaths)
    Dim Allsourcepaths As String 'this is the whole string "path1;path2;path2" etc
    Dim Allbackuppaths As String ' this is the whole string see above
    Dim formfullyloaded As Boolean = False

#End Region

#Region "MainCode"
    '*-----------------*'
    '*----Main Code----*'
    '*-----------------*'

    ' Settings Form
    Private Sub Settings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'get Values from "home"-class (form1.vb) to get already loaded settings (to display/manipulate)
        backupPatharray = WinBackupper.home.backupPatharray
        sourcepatharray = WinBackupper.home.sourcepatharray
        'after getting current values - update displayed settings 
        Settings_Reload()
    End Sub

    'function to set autostart
    Function Application_Autostart(enable As Boolean, Optional startupparameters As String = "")
        Try
            If enable = True Then
                'Create value in the "Run" key within the current user hive
                'set name "Winbackupper" witht he full path to the exe file which should be called after startup
                'first check the startupparameters argument - if not supplied dont do anything, if supplied - check how parameters were entered:
                If Not startupparameters = "" Then
                    'some kind of parameter was specified - check it
                    If startupparameters.Substring(0, 1) = " " Then
                        'the entered parameter starts with a space - no need to manipulate it!
                    ElseIf startupparameters.Substring(0, 1) = "-" Then
                        'no space in begining - add it (otherwise it will produce errors since the string would be written incorrectly into the registry
                        startupparameters = " " & startupparameters
                    End If
                End If
                My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "Winbackupper_Autostart", getexedir() & "\WinBackupper.exe" & startupparameters)
            Else
                'define the run key
                Dim runkey = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
                'delete the value 
                runkey.DeleteValue("Winbackupper_Autostart")
                'DeleteValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run\Winbackupper_Autostart")
            End If
            'function didn't return a excpected value - return -1 as error code 
            Return -1
        Catch ex As Exception
            Return -1
        End Try
    End Function

    'function to check if autostart is correctly written into registry
    'needs to be public so it can be called from startform (home.vb)
    Public Function Check_Autostart()
        Try
            'for the moment just rewrite it - for the future chekc it and if it's a bit corrupted recreate it for the user
            'check if no reg key is there - and exit sub if so
            If (My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "Winbackupper_Autostart", Nothing)) Is Nothing Then
                'there is no such value - meaybe a wrong key? (exit - nothing to compare)
                Return -2
            Else
                'there is some value

                'delete it
                Application_Autostart(False)
                'rewrite it (currently without parameters
                My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "Winbackupper_Autostart", _
                                             getexedir().ToString & System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.ToString)
            End If




            Return 0
            Exit Function
            'the code down won't fire - because of the "return" statement
            'temporarily disable checking while developing it  - simply rewrite it if existing



            'define the "RUN" key of windows in the CU hive
            Dim runkey = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
            'get our valuename is within the run key
            Dim Autostartvalue = runkey.GetValue("Winbackupper_Autostart").ToString

            Dim writtenpath = "not used yet"

            ' checks if our valuename is within the run key
            If writtenpath = vbNull Or writtenpath = "" Then
                'there is no suck value - meaybe a wrong key? (exit - nothing to compare)
                Return -1
            Else
                Dim OK As Boolean = True
                Dim params As Boolean = False
                Dim paramsstring As String 'used if there are some to store them

                If Not (Autostartvalue.Contains(writtenpath)) Then : OK = False : End If

                'there are some params
                'now there is only 1 - so it s easy to check (for future check if temparray.contains(param) )
                '  paramsstring = temparray(1)
                If paramsstring = "-s" Or paramsstring = "-silent" Or paramsstring = "/s" Or paramsstring = "/silent" Then
                    params = True
                End If
                If Not OK = True Then
                    'recreate the settings
                    'delete the value 
                    '   runkey.DeleteValue("Winbackupper_Autostart")
                Else
                    'return 0 to indicate success
                    Return 0
                End If

            End If
            'function didn't return a excpected value - return -1 as error code 
            Return -1
        Catch ex As Exception
            MsgBox(ex.Message)
            Return -1
        End Try
    End Function

    'function to reload all settings displayed in the form. Only use this one!
    Public Function Settings_Reload()
        Try
            'temp test
            Check_Autostart()
            'run through Array and get needed Values
            For i = 0 To sourcepatharray.Count - 1 Step 1
                'also fill RTB_Source! (richtextbox)
                RTB_Sourcepath.AppendText(sourcepatharray(i) & vbNewLine)
                'also fill RTB_Backup! (richtextbox)
                RTB_Backuppath.AppendText(backupPatharray(i) & vbNewLine)
                'to get the time, fill a richtextbox with all starttimes FOR THE SELECTED ENTRY! (no idea how to display it otherwise currently)
            Next
            'check if registry key for autostart exists -also set in the settings form
            Dim runkey = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run")
            'checks if our valuename is within the run key
            If (runkey.GetValueNames.Contains("Winbackupper_Autostart")) Then
                'reg key exists - enable the autostart checkbox
                cb_Autostart.Checked = True
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return -1
        End Try

    End Function

    ' TextBox for default Source Path
    Private Sub tb_defaultSourcePath_TextChanged(sender As Object, e As EventArgs)
        'if something is pasted in, add the globalsperator sign at the end!
    End Sub

    ' Button Search default Source Path
    Private Sub b_searchDefaultSource_Click(sender As Object, e As EventArgs)
        ' Dialog to select Source Path
        fbd_searchDefaultSource.Description = "Select Folder"
        fbd_searchDefaultSource.RootFolder = Environment.SpecialFolder.LocalizedResources
        DialogResult = fbd_searchDefaultSource.ShowDialog
        Dim SourcePathtresult As String = fbd_searchDefaultSource.SelectedPath.ToString 'maybe get multiple paths? (ad ask user if he wants to backup them to same place)
        'do sanity check before adding (check if already existing?)
        'does string contain "desktop"?
        If sourcepatharray.Contains(SourcePathtresult) Then
            Dim userchoice = MessageBox.Show("Folder is already getting backupped, add it anyway?", "Already getting Backupped!", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If userchoice = vbYes Then
                'add it anyway, even if already existing in source list
                'write value into Array!
                sourcepatharray.Add(SourcePathtresult)
            End If
        Else
            'sane string ...add it
            'write value into Array!
            sourcepatharray.Add(SourcePathtresult)
        End If
    End Sub

    ' FolderBrowserDialog to select default Source Path
    Private Sub fbd_searchDefaultSource_HelpRequest(sender As Object, e As EventArgs) Handles fbd_searchDefaultSource.HelpRequest

    End Sub

    ' TextBox for default Backup Path
    Private Sub tb_defaultBackupPath_TextChanged(sender As Object, e As EventArgs)
        'if something is pasted in, add the globalsperator sign at the end!
    End Sub

    ' Button Search default Backup Path
    Private Sub b_searchDefaultBackup_Click(sender As Object, e As EventArgs)
        ' Dialog to select Backup Path
        fbd_searchDefaultBackup.Description = "Select Folder"
        fbd_searchDefaultBackup.RootFolder = Environment.SpecialFolder.LocalizedResources
        'i notice here is a bug - you can click ok without selecting anything i guess? not sure what s in var then...
        DialogResult = fbd_searchDefaultBackup.ShowDialog
        Dim BackupPathresult As String = fbd_searchDefaultBackup.SelectedPath.ToString
        'do sanity check before adding (check if already existing?)
        If sourcepatharray.Contains(BackupPathresult) Then
            Dim userchoice = MessageBox.Show("You want to save into a Folder which is getting back-uppen itself" & vbNewLine & "Do you want to continue?", "Destination getting Backupped!", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If userchoice = vbYes Then
                'add it anyway, even if user is saving data into a directory which is getting backed-up
                'write value into Array!
                backupPatharray.Add(BackupPathresult)
            Else
                Exit Sub
            End If
        Else
            'sane string ...add it
            'write value into Array!
            backupPatharray.Add(BackupPathresult)
        End If
    End Sub

    ' FolderBrowserDialog to select default Backup Path
    Private Sub fbd_searchDefaultBackup_HelpRequest(sender As Object, e As EventArgs) Handles fbd_searchDefaultBackup.HelpRequest

    End Sub

    ' Button Save defaults to own XML File
    Private Sub b_save_Click(sender As Object, e As EventArgs) Handles b_save.Click
        'delete default.xml if it exists already
        If System.IO.File.Exists(getexedir() & "\default.xml") Then
            'delete it 
            System.IO.File.Delete(getexedir() & "\default.xml")
        End If
        'start bw_writer which writes default.xml in backgournd (other thread)
        bw_writer.RunWorkerAsync()

    End Sub

    Private Sub b_reset_Click(sender As Object, e As EventArgs) Handles b_reset.Click
        Dim resetchoice = MessageBox.Show("Do you really want to reset ALL your configurations?", "Reset EVERYTHING?", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If resetchoice = vbYes Then
            'delete xml file - reset arrays
            If System.IO.File.Exists(getexedir() & "\default.xml") Then
                'delete it 
                System.IO.File.Delete(getexedir() & "\default.xml")
            End If
            sourcepatharray.Clear()
            backupPatharray.Clear()
        Else
            'user aborted - maybe misclicked 
            MessageBox.Show("Reseting Configuration Aborted!", "Aborted", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If


    End Sub

    'sub called when mouse button is clicked (rtb refers to the clicked richtextbox!)
    Private Sub RTB_Sourcepath_MouseDown(sender As Object, e As MouseEventArgs) Handles RTB_Sourcepath.MouseDown
        Try
            'get mouseposition
            Dim rtb = DirectCast(sender, RichTextBox)
            'then get the char where the mouse is
            Dim index = rtb.GetCharIndexFromPosition(e.Location)
            'get the line where this char is (with it's index in the char array of the rtb)
            Dim line = rtb.GetLineFromCharIndex(index)
            'define the first char of line
            Dim lineStart = rtb.GetFirstCharIndexFromLine(line)
            'define the last one
            Dim lineEnd = rtb.GetFirstCharIndexFromLine(line + 1) - 1
            'start selection
            rtb.SelectionStart = lineStart
            'define the length of it
            rtb.SelectionLength = lineEnd - lineStart
            'define color to set
            Dim tempselectionfont
            If (rtb.SelectionFont.Style = FontStyle.Regular) Then
                tempselectionfont = New Font(rtb.SelectionFont, FontStyle.Bold)
            Else
                tempselectionfont = New Font(rtb.SelectionFont, FontStyle.Regular)
            End If
            rtb.SelectionFont = tempselectionfont
            'after that,make shure to make same with other rtb's to select similar entries! (at least in backuppathrtb too - time rtb can be ignored)
            Dim backuppathrtb = DirectCast(Me.RTB_Backuppath, RichTextBox)
            'repeat above steps for other rtbox...
            Dim backupline = backuppathrtb.GetLineFromCharIndex(index)
            'define the first char of line
            Dim backuplineStart = backuppathrtb.GetFirstCharIndexFromLine(line)
            'define the last one
            Dim backuplineEnd = backuppathrtb.GetFirstCharIndexFromLine(line + 1) - 1
            'start selection
            backuppathrtb.SelectionStart = backuplineStart
            'define the length of it
            backuppathrtb.SelectionLength = backuplineEnd - backuplineStart
            If (backuppathrtb.SelectionFont.Style = FontStyle.Regular) Then
                tempselectionfont = New Font(backuppathrtb.SelectionFont, FontStyle.Bold)
            Else
                tempselectionfont = New Font(backuppathrtb.SelectionFont, FontStyle.Regular)
            End If
            backuppathrtb.SelectionFont = tempselectionfont
            'after setting bold font in both boxes, select "nothing" so no text is blue.
            rtb.SelectionStart = 0
            rtb.SelectionLength = 0
        Catch ex As Exception

        End Try

    End Sub

    'sub called when mouse button is clicked (rtb refers to the clicked richtextbox!)
    Private Sub RTB_Backuppath_MouseDown(sender As Object, e As MouseEventArgs) Handles RTB_Backuppath.MouseDown
        Try
            'get mouseposition
            Dim rtb = DirectCast(sender, RichTextBox)
            'then get the char where the mouse is
            Dim index = rtb.GetCharIndexFromPosition(e.Location)
            'get the line where this char is (with it's index in the char array of the rtb)
            Dim line = rtb.GetLineFromCharIndex(index)
            'define the first char of line
            Dim lineStart = rtb.GetFirstCharIndexFromLine(line)
            'define the last one
            Dim lineEnd = rtb.GetFirstCharIndexFromLine(line + 1) - 1
            'start selection
            rtb.SelectionStart = lineStart
            'define the length of it
            rtb.SelectionLength = lineEnd - lineStart
            'define color to set
            Dim tempselectionfont
            If (rtb.SelectionFont.Style = FontStyle.Regular) Then
                tempselectionfont = New Font(rtb.SelectionFont, FontStyle.Bold)
            Else
                tempselectionfont = New Font(rtb.SelectionFont, FontStyle.Regular)
            End If
            rtb.SelectionFont = tempselectionfont
            'after that,make shure to make same with other rtb's to select similar entries! (at least in backuppathrtb too - time rtb can be ignored)
            Dim sourcepathrtb = DirectCast(Me.RTB_Sourcepath, RichTextBox)
            'repeat above steps for other rtbox...
            Dim sourceline = sourcepathrtb.GetLineFromCharIndex(index)
            'define the first char of line
            Dim sourcelineStart = sourcepathrtb.GetFirstCharIndexFromLine(sourceline)
            'define the last one
            Dim sourcelineEnd = sourcepathrtb.GetFirstCharIndexFromLine(sourceline + 1) - 1
            'start selection => seems not to work well with 2 boxes at the same time (only marks blue in one)
            'now try to make it bold - maybe it0s enough
            sourcepathrtb.SelectionStart = sourcelineStart
            'define the length of it
            sourcepathrtb.SelectionLength = sourcelineEnd - sourcelineStart
            'define color to set
            If (sourcepathrtb.SelectionFont.Style = FontStyle.Regular) Then
                tempselectionfont = New Font(sourcepathrtb.SelectionFont, FontStyle.Bold)
            Else
                tempselectionfont = New Font(sourcepathrtb.SelectionFont, FontStyle.Regular)
            End If
            sourcepathrtb.SelectionFont = tempselectionfont
            'after setting bold font in both boxes, select "nothing" so no text is blue.
            rtb.SelectionStart = 0
            rtb.SelectionLength = 0
        Catch ex As Exception

        End Try
    End Sub

    'sub called when mouse button is clicked (rtb refers to the clicked richtextbox!)
    Private Sub rtb_backupstarttimes_MouseDown(sender As Object, e As MouseEventArgs) Handles rtb_backupstarttimes.MouseDown
        Try
            'this time - only select entry of timebox - to delete entries in future or edit them one for one.
            'get mouseposition
            Dim rtb = DirectCast(sender, RichTextBox)
            'then get the char where the mouse is
            Dim index = rtb.GetCharIndexFromPosition(e.Location)
            'get the line where this char is (with it's index in the char array of the rtb)
            Dim line = rtb.GetLineFromCharIndex(index)
            'define the first char of line
            Dim lineStart = rtb.GetFirstCharIndexFromLine(line)
            'define the last one
            Dim lineEnd = rtb.GetFirstCharIndexFromLine(line + 1) - 1
            'start selection
            rtb.SelectionStart = lineStart
            'define the length of it
            rtb.SelectionLength = lineEnd - lineStart
            'define color to set
            Dim tempselectionfont
            If (rtb.SelectionFont.Style = FontStyle.Regular) Then
                tempselectionfont = New Font(rtb.SelectionFont, FontStyle.Bold)
            Else
                tempselectionfont = New Font(rtb.SelectionFont, FontStyle.Regular)
            End If
            rtb.SelectionFont = tempselectionfont
            'after setting bold font in both boxes, select "nothing" so no text is blue.
            rtb.SelectionStart = 0
            rtb.SelectionLength = 0
        Catch ex As Exception

        End Try
    End Sub

    Private Sub b_addfolderpair_Click(sender As Object, e As EventArgs) Handles b_addfolderpair.Click
        'only execute once so user is forced to enter a backuppath too - old functions still exist so still changeable!
        ' Dialog to select Source Path
        fbd_searchDefaultSource.Description = "Select Source Folder!"
        fbd_searchDefaultSource.RootFolder = Environment.SpecialFolder.LocalizedResources
        DialogResult = fbd_searchDefaultSource.ShowDialog
        Dim SourcePathtresult As String = fbd_searchDefaultSource.SelectedPath.ToString 'maybe get multiple paths? (ad ask user if he wants to backup them to same place)
        'do sanity check before adding (check if already existing?)
        If sourcepatharray.Contains(SourcePathtresult) Then
            Dim userchoice = MessageBox.Show("Folder is already getting backupped, add it anyway?", "Already getting Backupped!", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If userchoice = vbYes Then
                'add it anyway, even if already existing in source list
                'write value into Array!
                sourcepatharray.Add(SourcePathtresult)
            Else
                Exit Sub
            End If
        Else
            'sane string ...add it
            'write value into Array!
            sourcepatharray.Add(SourcePathtresult)
        End If

        ' Dialog to select Backup Path
        fbd_searchDefaultBackup.Description = "Select Destination Folder"
        fbd_searchDefaultBackup.RootFolder = Environment.SpecialFolder.LocalizedResources
        DialogResult = fbd_searchDefaultBackup.ShowDialog
        Dim BackupPathresult As String = fbd_searchDefaultBackup.SelectedPath.ToString
        'do sanity check before adding (check if already existing?)
        If sourcepatharray.Contains(BackupPathresult) Then
            Dim userchoice = MessageBox.Show("You want to save into a Folder which is getting back-upped itself" & vbNewLine & "Do you want to continue?", "Destination getting Backupped!", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If userchoice = vbYes Then
                'add it anyway, even if user is saving data into a directory which is getting backed-up
                'write value into Array!
                backupPatharray.Add(BackupPathresult)
            Else
                Exit Sub
            End If
        Else
            'sane string ...add it
            'write value into Array!
            backupPatharray.Add(BackupPathresult)
        End If
    End Sub

    'executed when settingsform is fully loaded (and therefore shown to the user)
    Private Sub Settings_Shown(sender As Object, e As EventArgs) _
         Handles Me.Shown
        formfullyloaded = True
    End Sub

    'executed when the cb_autostart is clicked- will write/delete autostart reg key depending on arguments supplied
    Private Sub cb_Autostart_CheckedChanged(sender As Object, e As EventArgs) Handles cb_Autostart.CheckedChanged
        If formfullyloaded Then
        If cb_Autostart.Checked = True Then
            'ask user if he want to start silently ....
                Dim startsilent = MessageBox.Show("Want to start on startup in SILENT mode?" & vbNewLine & _
                                             "This will hide all forms and do all work in the background!", _
                                             "Startup Silently in the Future?", _
                                             MessageBoxButtons.YesNoCancel, _
                                             MessageBoxIcon.Question)

            'Application_Autostart sets autostart - accepts arguments "enabled" which is a boolean
            'and accepts a second argument "Startupparameters" as a string like "- silent"
            If startsilent = vbYes Then
                'if user wants to start silent - add parameter
                Application_Autostart(True, " -s")
                ElseIf startsilent = vbNo Then
                    'start normally
                    Application_Autostart(True)
                Else
                    'if canceled, cancel the whole sub - nothing has changed (and reset the checkbox)
                    cb_Autostart.Checked = False
                    Exit Sub
                End If
        Else
            'start normally (delete reg key )
            Application_Autostart(False)
            End If
        Else
            'don't execute the code since the checkbox is changen on the LOAD event! This would execute this code too - and we don't want that!
        End If
    End Sub

    'Function to get Directory of current .exe-file
    Private Function getexedir()
        Dim path As String
        path = System.IO.Path.GetDirectoryName( _
           System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
        Return path.Substring(6, path.Length - 6)
    End Function

#End Region

#Region "Workers"
    '*-----------------*'
    '*-----Workers-----*'
    '*-----------------*'

    ' BackgroundWorker Writes settings into XML File
    Private Sub bw_writer_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles bw_writer.DoWork
        ' Create XML Writer
        Dim writerOption As New XmlWriterSettings
        writerOption.Indent = True
        Dim writerSettings As XmlWriter = XmlWriter.Create("default.xml", writerOption)
        'check if array's have unequal nr of members
        'maybe user closed box to choose backup path or didnt open it 
        If Not (sourcepatharray.Count = backupPatharray.Count) Then
            Dim cleanchoice = MessageBox.Show("Troubles with local settings detected - want to auto-clean?")
            If cleanchoice = vbYes Then
                If (sourcepatharray.Count > backupPatharray.Count) Then
                    For i = 0 To (sourcepatharray.Count - backupPatharray.Count) Step 1
                        'remove last entry - not in sync (what if others were entered?)
                        sourcepatharray.RemoveAt(sourcepatharray.Count - 1)
                    Next
                ElseIf (sourcepatharray.Count < backupPatharray.Count) Then
                    For i = 0 To (backupPatharray.Count - sourcepatharray.Count) Step 1
                        'remove last entry - not in sync (what if others were entered?)
                        backupPatharray.RemoveAt(backupPatharray.Count - 1)
                    Next
                End If
            End If
        End If

        With writerSettings
            .WriteStartDocument()
            .WriteStartElement("defaults")
            For Each sourcepath As String In sourcepatharray
                'start writing each sourcepath of the array
                .WriteStartElement("Source")
                .WriteString(sourcepath)
                'write ending "<Source>" tag
                .WriteEndElement()

            Next

            For Each backuppath As String In backupPatharray
                'start writing each sourcepath of the array
                .WriteStartElement("Backup")
                .WriteString(backuppath)
                'write ending "<Backup>" tag
                .WriteEndElement()
            Next
            'write ending "<default>" tag
            .WriteEndElement()
            .WriteEndDocument()
            .Close()
            .Dispose()
        End With
        'close file again - prevent file IO exceptions
        writerSettings.Close()
        writerSettings.Dispose()



        'start checking if settings are saved correctly!
        If Not Dir("default.xml") = "" Then
            ' Read XML File to check if it was written
            Dim xmlReader As XmlReader = New XmlTextReader("default.xml")
            'define var's used to compare saveddata to supposed data
            Dim sourcetargetdata As ArrayList = sourcepatharray
            Dim backuptargetdata As ArrayList = backupPatharray
            Dim Savedsourcedata As New ArrayList
            Dim Savedbackupdata As New ArrayList
            ' Loop through XML File
            While (xmlReader.Read())
                Dim type = xmlReader.NodeType

                ' Find selected Paths in XML File and write them into Var
                If (type = XmlNodeType.Element) Then
                    ' Looking for "Source" Path
                    If (xmlReader.Name = "Source") Then
                        'add current string (read from xml) to the array
                        Savedsourcedata.Add(xmlReader.ReadInnerXml.ToString)
                    End If
                    'Looking for "Backup" Path
                    If (xmlReader.Name = "Backup") Then
                        'add current string (read from xml) to the array
                        Savedbackupdata.Add(xmlReader.ReadInnerXml.ToString)
                    End If
                End If

            End While
            'the first if part seems to fail- real check is within the loop
            If (sourcetargetdata.Equals(Savedsourcedata)) And (backuptargetdata.Equals(Savedbackupdata)) Then
                MessageBox.Show("Paths saved!")
            Else
                Dim mismatches As Integer = 0
                For i = 0 To sourcepatharray.Count Step 1
                    If i = sourcepatharray.Count Then
                        Exit For
                    End If
                    If Not (sourcetargetdata(i).ToString = Savedsourcedata(i).ToString) Then
                        mismatches += 1
                    End If
                Next
                If mismatches > 0 Then 'if there are no mismatches the array's are equal!
                    MessageBox.Show("Unable to save Configuration!", "Error while saving Configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Else
                    MessageBox.Show("Configuration Saved succesfully!", "Configuration Saved!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If
            End If

            'close reader again
            xmlReader.Close()
            xmlReader.Dispose()
        End If
    End Sub

#End Region

End Class