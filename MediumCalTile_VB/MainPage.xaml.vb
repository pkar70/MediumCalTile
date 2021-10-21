' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


' 2.5.0
' gdy to moje, to moze byc poprawianie konwersji html/txt... - ale nie, AppointmentStoreAccessType.AllCalendarsReadWrite daje błąd
' 2.2.0


Imports Windows.ApplicationModel.Appointments
Imports Windows.ApplicationModel.Background
Imports Windows.ApplicationModel.Core
Imports Windows.Data.Xml.Dom
Imports Windows.Foundation.Metadata
Imports Windows.Storage
Imports Windows.UI.Notifications
Imports Windows.UI.StartScreen

' 21 IX 2017
' dodatkowy trigger na Servicing (przy upgrade) 
' wydzielona funkcja TriggersUsun
' dodanie triggera AppointmentStoreNotificationTrigger
' wyREMowanie triggera Timer

''' <summary>
''' wedle
''' https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-create-adaptive-tiles
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Shared oAppTrig As ApplicationTrigger

    Public Shared Function TriggersUsun() As Boolean
        ' Ret: False, gdy błąd

        Try
            For Each oTask In BackgroundTaskRegistration.AllTasks
                ' uzywane w historii (do wersji z 21 IX 2017)
                If oTask.Value.Name = "MediumCalTileBackgroundTimer" Then oTask.Value.Unregister(True)

                ' aktualne
                If oTask.Value.Name = "MediumCalTileBackgroundUser" Then oTask.Value.Unregister(True)
                If oTask.Value.Name = "MediumCalTileServicing" Then oTask.Value.Unregister(True)
                If oTask.Value.Name = "MediumCalTileCalendarNotification" Then oTask.Value.Unregister(True)
                If oTask.Value.Name = "MediumCalTileBackgroundApp" Then oTask.Value.Unregister(True)

            Next

            BackgroundExecutionManager.RemoveAccess()
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Shared Async Function UstawTriggery() As Task(Of Boolean)
        ' Ret: False, gdy błąd

        Dim oBAS As BackgroundAccessStatus
        oBAS = Await BackgroundExecutionManager.RequestAccessAsync()

        If oBAS <> BackgroundAccessStatus.AlwaysAllowed AndAlso oBAS <> BackgroundAccessStatus.AllowedSubjectToSystemPolicy Then Return False

        If Not TriggersUsun() Then Return False

        Try
            ' https://docs.microsoft.com/en-us/windows/uwp/launch-resume/create-And-register-an-inproc-background-task
            Dim builder As BackgroundTaskBuilder = New BackgroundTaskBuilder
            Dim oRet As BackgroundTaskRegistration

            ' user sie pojawia - po to, zeby pokazac (obracaniem tile) ze coś sie dzieje
            builder.SetTrigger(New SystemTrigger(SystemTriggerType.UserPresent, False))
            builder.Name = "MediumCalTileBackgroundUser"
            builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            oRet = builder.Register()

            ' odtworzenie oAppTrig
            oAppTrig = New ApplicationTrigger
            builder.SetTrigger(oAppTrig)  ' do wywolywania metody
            builder.Name = "MediumCalTileBackgroundApp"
            builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            oRet = builder.Register()

            builder.SetTrigger(New SystemTrigger(SystemTriggerType.ServicingComplete, True))
            builder.Name = "MediumCalTileServicing"
            builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            oRet = builder.Register()

            builder.SetTrigger(New AppointmentStoreNotificationTrigger)
            builder.Name = "MediumCalTileCalendarNotification"
            builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            oRet = builder.Register()
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function GetHostName() As String
        Dim hostNames As IReadOnlyList(Of Windows.Networking.HostName) =
                Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
        For Each oItem As Windows.Networking.HostName In hostNames
            If oItem.DisplayName.Contains(".local") Then
                ' Return oItem.DisplayName.Replace(".local", "")
            End If
        Next
        Return ""
    End Function


    Public Function IsThisMoje() As Boolean
        Dim sTmp As String = GetHostName.ToLower
        If sTmp = "home-pkar" Then Return True
        If sTmp = "lumia_pkar" Then Return True
        If sTmp = "kuchnia_pk" Then Return True
        If sTmp = "ppok_pk" Then Return True
        'If sTmp.Contains("pkar") Then Return True
        'If sTmp.EndsWith("_pk") Then Return True
        Return False
    End Function


    Private Sub UstawDefaulty()
        ApplicationData.Current.LocalSettings.Values("sRunLog") = ""

        oForcePL.IsOn = App.GetSettingsBool("bForcePL", False)
        oEventsNo.Value = App.GetSettingsInt("iEventNo", 2)
        oFontSize.Value = App.GetSettingsInt("iFontSize", 3)
        oNextEvent.IsOn = App.GetSettingsBool("bNextEvent", True)
        oPictDay.IsOn = App.GetSettingsBool("bPictDay", True)
        oNextTitle.IsOn = App.GetSettingsBool("bNextTitle", False)
        uiDelDupl.IsOn = App.GetSettingsBool("bDelDupl", False)
        uiConvertHtmlToggle.IsOn = False
    End Sub

    'Private Sub JakbyMoje()
    '    uiSettingsTitle.Visibility = Visibility.Collapsed
    '    uiConvertHtmlText.Visibility = Visibility.Visible
    '    uiConvertHtmlToggle.Visibility = Visibility.Visible
    '    uiConvertHtmlToggle.IsOn = App.GetSettingsBool("convertHtmlTxt", True)
    'End Sub

    Private Async Sub OnLoaded_Main(sender As Object, e As RoutedEventArgs)

        'If IsThisMoje() Then
        '    JakbyMoje()
        'Else
        uiConvertHtmlToggle.IsOn = False
        'End If

        UstawDefaulty()
        Await UstawTriggery()

        If App.GetSettingsBool("bCallFromTile") Then
            Try
                Await AppointmentManager.ShowTimeFrameAsync(Date.Now, TimeSpan.FromHours(24))
            Catch ex As Exception
                ' bo w OnLoaded_Main.MoveNext jest jakis unknown error (dashboard)
            End Try
        Else

            tbModif.Text = "Build " & Package.Current.Id.Version.Major & "." &
            Package.Current.Id.Version.Minor & "." & Package.Current.Id.Version.Build

            ' wedle dokumentacji, Loaded event will always fire after OnNavigatedTo 
            ' ale na wszelki wypadek - bo najpierw jest to!
            If oAppTrig IsNot Nothing Then Await oAppTrig.RequestAsync()

        End If

    End Sub
    Private Shared Function FontSizeNum2Txt(iSize As Integer) As String
        FontSizeNum2Txt = "subheader"
        Select Case iSize
            Case 1
                Return "base"
            Case 2
                Return "subtitle"
            Case 3
                Return "title"
            Case 4
                Return "subheader"
            Case 5
                Return "header"
            Case Else
                Return "subheader"
        End Select

        ' rozmiary czcionki:
        ' caption   12 regular
        ' body      15 regular
        ' base      15 semibold
        ' subtitle  20 regular
        ' title     24 semilight
        ' subheader 34 light
        ' header    46 light
    End Function
    Private Async Sub bUpdate_Click(sender As Object, e As RoutedEventArgs)

        App.SetSettingsBool("bForcePL", oForcePL.IsOn)
        App.SetSettingsInt("iEventNo", CInt(oEventsNo.Value))
        App.SetSettingsString("sFontSize", FontSizeNum2Txt(CInt(oFontSize.Value)))
        App.SetSettingsInt("iFontSize", CInt(oFontSize.Value))
        App.SetSettingsBool("bNextEvent", oNextEvent.IsOn)
        App.SetSettingsBool("bPictDay", oPictDay.IsOn)
        App.SetSettingsBool("bNextTitle", oNextTitle.IsOn)
        App.SetSettingsBool("bDelDupl", uiDelDupl.IsOn)
        App.SetSettingsBool("convertHtmlTxt", uiConvertHtmlToggle.IsOn)

        ' *TODO* OnChange kazdego elementu sie ustawia dany element. Ale to pozniej. 

        If ApiInformation.IsTypePresent("Windows.UI.StartScreen.StartScreenManager") Then
            ' czyli od Windows 15063, Creators Update - Aska nie ma!
            ' https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-primary-tile-apis
            Dim oEntry As AppListEntry = (Await Package.Current.GetAppListEntriesAsync())(0)

            ' jako ze Pin dziala tylko w mobile i w desktop - na wszelki wypadek
            Dim isSupported As Boolean = StartScreenManager.GetDefault().SupportsAppListEntry(oEntry)
            If isSupported Then
                Dim isPinned As Boolean = Await StartScreenManager.GetDefault().ContainsAppListEntryAsync(oEntry)
                If Not isPinned Then Await StartScreenManager.GetDefault().RequestAddAppListEntryAsync(oEntry)
            End If
        End If

        Await oAppTrig.RequestAsync

    End Sub

    Private Sub bOpenCal_Click(sender As Object, e As RoutedEventArgs)
        AppointmentManager.ShowTimeFrameAsync(Date.Now, TimeSpan.FromHours(24))
    End Sub

    Private Sub uiNextDay_Toggled(sender As Object, e As RoutedEventArgs) Handles oNextEvent.Toggled
        Try
            oNextTitle.IsEnabled = oNextEvent.IsOn
        Catch ex As Exception
            ' spodziewane - ktoregos moze jeszcze nie być
        End Try
    End Sub

    'Private Sub uiTitle_DoubleTap(sender As Object, e As DoubleTappedRoutedEventArgs)
    '    If App.GetSettingsBool("bForcePL", False) Then Return
    '    JakbyMoje()
    'End Sub
End Class
