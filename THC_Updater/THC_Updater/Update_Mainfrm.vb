﻿Imports System.Net
Imports System.IO
Imports System.Xml

''' Internal Version: 0.0.0.5 (this Form)
''' 
''' <summary>
''' 
''' Too use this Form utilize a Textfile called "LocalVersion". and enter the version nr like '0.0.0.1'
''' Then change the Public variables to match your Project.
''' Tell your Programm to udpate this file according to it's version Nr.
''' If this does not match with a textfile you have to host online - it will notice a new version. (if server vers. is newer)
''' otherwise it will just terminate.
''' 
''' </summary>
''' 
''' <remarks>
''' 
''' Not sure if it works with Hexadecimal version nr's!
''' 
'''</remarks>

Public Class Update_Mainfrm
    'global var's
    Public Overridable Property AllowAutoRedirect As Boolean = True
    Public PayloadsourceURL As String = "https://github.com/SnipeLike/WinBackupper/releases/download/v0.0.1.4/Winbackupper_v0.0.2.1.exe"
    Public ChangelogURL As String = "https://raw.githubusercontent.com/SnipeLike/WinBackupper/master/Changelog.txt"    'since there is a different URL for the "Release" part i needed another var for that
    'now it downloads everything correctly =)
    Public Domain As String = "raw.githubusercontent.com" 'your domainname - nslookup needs to be able to get the IP from it!
    Public Domainprojectdir As String = "/SnipeLike/WinBackupper/master/" 'The Directory of the Webserver
    'Example Root/Application/UpdateInfo would look like =>  Public Domainprojectdir As String = " / Application / Portable_Helper / "
    'Dont forget ending " / " !!!
    Public Deployname As String = "WinBackupper_v" ' without version nr!
    'Example Portable_Helper_v0.0.0.1  (while 0.0.0.1 is a changing variable - the version nr)
    Public Deployfiletype As String = ".exe" 'Fileending
    Public DomainIP As String = "" 'DONT MANIPULATE!  If you want to bypass see next Variable.
    Public DomainIPbypass As String = "" 'if entered it will bypass DNS - if there is need for it. (Sometime resolveFQDN seems to get the wrong ip)
    Shared fullpathofoldvers As String 'needed later to store IP of full path of old version
    Public lastdownloadstatupdate As DateTime 'date of last check
    Public Logfile As String = "Changelog.txt"
    Public Logfolder As String = "/Logs/" 'Directory of Logfile (Empty if same directory as exe)
    'Dont forget ending "/" !!!
    Public dns1 As String = "8.8.8.8" 'google's public dns server
    Public dns2 As String = "8.8.4.4" 'google's 2nd public dns server

    ' Public downloadspeedHR As String = "N/A" 'not used yet
    Public alreadydownloadedHR As String = "N/A" 'default value for already downloaded label
    Public localversnr = "uknown"

    'Delegate-stuff to update download status
    ' declare update download delegate
    Private Delegate Sub UpdateDelegate(ByVal Megabytes As String, ByVal Speed As String)
    ' declare an implmentation with matching signature
    Private Sub UpdatedownloadStatus(ByVal Megabytes As String, ByVal Speed As String)
        'speed = bytes/second
        Me.lbl_alreadydownloaded.Text = Megabytes
        Me.lbl_Downloadrate.Text = Speed
    End Sub

    Private Sub Update_Mainfrm_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Start BW to to Read Verison XML File
        bw_getProgVersion.RunWorkerAsync()

        ' check for cmdline arguments
        ' If 1 Element is passed (its always 1, the "start" command) ignore it
        ' if 2 arguments are passed, assume update.exe should delete a file (self destory?)
        ' 2 arguments will delete the fassed file!
        ' if 3 are entered assume that update.exe should rename a file
        ' 3 arguments will delete the target file (second argument passed) and rename the first one to the second one. 

        Try
            'get IP for Domain (Bypass Proxies)
            If DomainIPbypass = "" Then
                DomainIP = resolveFQDN(Domain)
            Else
                DomainIP = DomainIPbypass
            End If

            Dim renamesrcfile As String
            Dim targetfile As String
            'attention! "count" gives a locigal "human" value, but info is stored in an array which begins from 0
            If Environment.GetCommandLineArgs.Count <> 1 Then

                If Environment.GetCommandLineArgs.Count = 2 Then
                    '1 second timeout so i can access the exe which has called this program
                    System.Threading.Thread.Sleep(1000)
                    fullpathofoldvers = Environment.GetCommandLineArgs(1)
                End If
                If Environment.GetCommandLineArgs.Count = 3 Then
                    '1 second timeout so i can access the exe which has called this program
                    System.Threading.Thread.Sleep(1000)
                    renamesrcfile = Environment.GetCommandLineArgs(1)
                    targetfile = Environment.GetCommandLineArgs(2)
                    'check if targetfile exists already (prevent exeptions)
                    If System.IO.File.Exists(targetfile) = True Then
                        System.IO.File.Delete(targetfile)
                    End If
                    Rename(renamesrcfile, targetfile)
                    Application.Exit()
                End If
            End If

            'delete old changelog if exists
            If File.Exists(Logfolder & Logfile) Then
                Try
                    File.Delete(Logfolder & Logfile)
                Catch ex As Exception
                    MessageBox.Show(ex.Message & vbNewLine & "Error While deleting the old changelog - Is the file open?", "FATAL Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
            'download new changelog
            downloadfileoverhttp((ChangelogURL), getexedir() & Logfolder, Logfile)
            'initialize changelog textbox
            changelogtxtbox.Text = My.Computer.FileSystem.ReadAllText(getexedir() & Logfolder & Logfile)
        Catch ex As Exception
            MessageBox.Show(ex.Message & vbNewLine & "Error while loading Update_Mainfrm class.", "FATAL ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Function resolveFQDN(FQDN As String)
        'Hardcode dns servers
        Dim dnsServer1 As IPAddress = IPAddress.Parse(dns1)
        Dim dnsServer2 As IPAddress = IPAddress.Parse(dns2)
        'Error handler to handle unexpected errors, if works fine will not land on catch.
        Try
            Dim domain As IPHostEntry
            domain = Dns.GetHostEntry(FQDN)
            Dim resolvedIP As String = domain.AddressList(0).ToString()
            Return resolvedIP
        Catch ex As Exception
            MessageBox.Show(ex.Message, "FATAL ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return -1
        End Try
    End Function

    Private Function getexedir()
        Dim path As String
        path = System.IO.Path.GetDirectoryName(
           System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
        Return path.Substring(6, path.Length - 6)
    End Function
    Private Sub Update_Mainfrm_close(sender As Object, e As EventArgs) Handles Me.FormClosed
        ' Do cleanupwork
    End Sub
    Private Function downloadfileoverhttp(sourceturl As String, targetdir As String, targetname As String)
        Try
            Dim fullfilepath As String = targetdir & "\" & targetname
            ' Check if file exists
            If My.Computer.FileSystem.FileExists(fullfilepath) Then
                'If it's there, delete it
                My.Computer.FileSystem.DeleteFile(fullfilepath)
            End If
            Dim wclient As New System.Net.WebClient()
            wclient.DownloadFile(New Uri(sourceturl), fullfilepath)

            While (wclient.IsBusy)
                Application.DoEvents()
            End While
            Return 0
            ' Download file again, with the link for the file
            '  My.Computer.Network.DownloadFile(sourceturl, fullfilepath)
        Catch ex As Exception
            If ex.Message IsNot "" Then
                Return -1
            End If
        End Try
        Return 0
    End Function

    Private Function readtxtfileline(fullfilepath As String, returnline As Integer)
        Try
            ' Read the File content
            Dim Stream As New IO.StreamReader(fullfilepath)
            'set ValueType to "" (0)
            Dim txtline As String = ""
            For i = 0 To returnline
                'save a line of the document
                txtline = Stream.ReadLine
            Next
            'close the stream
            Stream.Close()
            Return txtline
        Catch ex As Exception
            If ex.Message IsNot "" Then
                Return -1
            End If
        End Try
        Return 0
    End Function
    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Try
            'start download process
            'inform user
            loglbl.Text = "Checking for new version!"
            'get new Version Number
            downloadfileoverhttp("https://" & Domain & Domainprojectdir & "NewVersion.txt", getexedir() & Logfolder, "NewVersion.txt")

            ' Get "new" Version Number fom txt File
            Dim versnr As String = readtxtfileline(getexedir() & Logfolder & "NewVersion.txt", 0)
            ' localversnr1 getting in BW_getProgVersion out of XML File

            'check if local version is equal to "new" version
            If localversnr < versnr Then
                'inform user
                loglbl.Text = "newer version detected!"
                'Newer version availavle, start download?
                If MessageBox.Show("A newer Version is detected. Start download now?", "New Version", MessageBoxButtons.YesNo) = Windows.Forms.DialogResult.Yes Then
                    '  Dim NewFile As String = getexedir() & "\" & "Portable_Helper_v" & versnr & ".exe"
                    'download new exe
                    downloadfilewithprogress(New Uri(PayloadsourceURL), getexedir() & " \ " & Deployname & versnr & Deployfiletype)
                End If
            Else
                loglbl.Text = "up-To-date! repair?"
                If MessageBox.Show("Version is up-to-date!" &vbNewLine & "Want To repair (redownload) Application?", "redownload?", MessageBoxButtons.YesNo) = vbYes Then
                    'user said yes, redownload
                    ' Dim NewFile As String = getexedir() & "\" & "portable_helper_v" & versnr & ".exe"
                    'download new exe
                    downloadfilewithprogress(New Uri(PayloadsourceURL), getexedir() & "\" & Deployname & versnr & Deployfiletype)
                End If
            End If
        Catch ex As Exception
            If ex.Message IsNot "" Then
                MessageBox.Show(ex.Message)
            End If
        End Try
        'no new version available, ask for repair (redownload)?
        'inform user
    End Sub
    Function downloadfilewithprogress(URL As Uri, targetname As String)
        Try
            Dim fclient As New System.Net.WebClient()
            AddHandler fclient.DownloadProgressChanged, AddressOf DownloadProgressChanged
            AddHandler fclient.DownloadFileCompleted, AddressOf DownloadCompleted
            loglbl.Text = "Starting Download!"
            fclient.DownloadFileAsync(URL, targetname)

            While (fclient.IsBusy)
                Application.DoEvents()
            End While
            Return 0
        Catch ex As Exception
                Return -1
        End Try
    End Function

    Public Sub DownloadProgressChanged(ByVal sender As Object, ByVal e As DownloadProgressChangedEventArgs)

        Dim bytesIn As Double = Double.Parse(e.BytesReceived.ToString())
        Dim totalBytes As Double = Double.Parse(e.TotalBytesToReceive.ToString())
        Dim percentage As Double = bytesIn / totalBytes * 100
        ProgressBar.Value = Int32.Parse(Math.Truncate(percentage).ToString())
        Dim alreadydownloadedinMB = Math.Round((bytesIn / 1024 / 1024), 3)
        Dim now As DateTime = DateTime.Now
        Dim timesincelastdownloadstateupdate = now - lastdownloadstatupdate
        Dim timesincelastdownloadstateupdateinseconds = timesincelastdownloadstateupdate.Seconds
        Dim downloadspeed = Math.Round(alreadydownloadedinMB / timesincelastdownloadstateupdateinseconds, 3)
        Dim downloadspeedHR = downloadspeed & " MB/s"
        alreadydownloadedHR = alreadydownloadedinMB & " MB"
        'delegate stuff to update UI in async thread
        Dim del As UpdateDelegate = AddressOf UpdatedownloadStatus
        Me.Invoke(del, alreadydownloadedHR, downloadspeedHR)
    End Sub
    Public Sub DownloadCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs)
        ProgressBar.Value = 100
        loglbl.Text = "Download complete!"
        'delete old version if argument 1 was specified.
        If fullpathofoldvers <> Nothing Then
            'MessageBox.Show("tryng to delete -" & fullpathofoldvers)
            If System.IO.File.Exists(fullpathofoldvers) = True Then
                System.IO.File.Delete(fullpathofoldvers)
            End If
        End If
        'start SW if user wished so
        Dim versnr As String = readtxtfileline(getexedir() & Logfolder & "NewVersion.txt", 0)
        Dim NewFile As String = getexedir() & "\" & Deployname & versnr & Deployfiletype
        Dim startswafterwards = MessageBox.Show("Update finished! Start Software now?", "Update finished!", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If startswafterwards = Windows.Forms.DialogResult.Yes Then
            Diagnostics.Process.Start(NewFile)
            Application.Exit()
        End If

    End Sub

    Private Sub btn_closefrm_Click(sender As Object, e As EventArgs) Handles btn_closefrm.Click
        Application.Exit()
    End Sub

    ' Read XML File to get WinBackupper.exe Version
    Private Sub bw_getProgVersion_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles bw_getProgVersion.DoWork
        If Not Dir("version.xml") = "" Then
            ' Read XML File to check if it was written
            Dim xmlReader As XmlReader = New XmlTextReader("version.xml")

            ' Loop through XML File
            While (xmlReader.Read())
                Dim type = xmlReader.NodeType

                ' Find version in XML File and write them into Var
                If (type = XmlNodeType.Element) Then
                    ' Looking for "Version"
                    If (xmlReader.Name = "Version") Then
                        localversnr = xmlReader.ReadInnerXml.ToString
                    End If
                End If

            End While
            xmlReader.Close()
        Else
            MessageBox.Show("Can not find the version.xml File. Please restart WinBackupper.exe and try again", "Upps...")
        End If
    End Sub
End Class
