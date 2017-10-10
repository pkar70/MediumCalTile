' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

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

    Public Shared Function TriggersUsun(Optional bAll As Boolean = False) As Integer
        ' zwraca b0..b3 stan triggerów PO uruchomieiu funkcji
        ' gdy nie zostal zaden trigger, robi RemoveAccess (dla ServicingComplete)
        Dim iRet As Integer = 0

        For Each oTask In BackgroundTaskRegistration.AllTasks
            ' uzywane w historii (do wersji z 21 IX 2017)
            If oTask.Value.Name = "MediumCalTileBackgroundTimer" Then oTask.Value.Unregister(True)

            If oTask.Value.Name = "MediumCalTileBackgroundUser" Then
                If bAll Then
                    oTask.Value.Unregister(True)
                Else
                    iRet = iRet Or 2
                End If
            End If
            If oTask.Value.Name = "MediumCalTileBackgroundApp" Then
                ' jesli jest, to i tak zrob na nowo - jest potrzebna zmienna oAppTrig
                oTask.Value.Unregister(True)
            End If

            If oTask.Value.Name = "MediumCalTileServicing" Then oTask.Value.Unregister(True)
            If oTask.Value.Name = "MediumCalTileCalendarNotification" Then oTask.Value.Unregister(True)

        Next

        If iRet = 0 Then BackgroundExecutionManager.RemoveAccess()

        TriggersUsun = iRet
    End Function

    Public Shared Async Function UstawTriggery(Optional bAll As Boolean = False) As Task

        Dim oBAS As BackgroundAccessStatus
        oBAS = Await BackgroundExecutionManager.RequestAccessAsync()

        If oBAS = BackgroundAccessStatus.AlwaysAllowed Or oBAS = BackgroundAccessStatus.AllowedSubjectToSystemPolicy Then

            Dim iTriggers = TriggersUsun(bAll)

            ' https://docs.microsoft.com/en-us/windows/uwp/launch-resume/create-And-register-an-inproc-background-task
            Dim builder As BackgroundTaskBuilder = New BackgroundTaskBuilder
            Dim oRet As BackgroundTaskRegistration

            ' usuniete 21 IX 2017
            'If Not CBool(iTriggers And 1) Then
            '    builder.SetTrigger(New TimeTrigger(15, False))  ' nie moze byc mniej niz 15 minut!
            '    builder.Name = "MediumCalTileBackgroundTimer"
            '    builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            '    oRet = builder.Register()
            'End If

            ' user sie pojawia - po to, zeby pokazac (obracaniem tile) ze coś sie dzieje
            If Not CBool(iTriggers And 2) Then
                builder.SetTrigger(New SystemTrigger(SystemTriggerType.UserPresent, False))
                builder.Name = "MediumCalTileBackgroundUser"
                builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
                oRet = builder.Register()

            End If

            ' ten jest na pewno usuniety w TriggersUsun, odtworzenie oAppTrig
            oAppTrig = New ApplicationTrigger
            builder.SetTrigger(oAppTrig)  ' do wywolywania metody
            builder.Name = "MediumCalTileBackgroundApp"
            builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            oRet = builder.Register()

            ' ten jest na pewno usuniety
            builder.SetTrigger(New SystemTrigger(SystemTriggerType.ServicingComplete, True))  ' user sie pojawia
            builder.Name = "MediumCalTileServicing"
            builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            oRet = builder.Register()

            ' ten jest na pewno usuniety
            builder.SetTrigger(New AppointmentStoreNotificationTrigger)
            builder.Name = "MediumCalTileCalendarNotification"
            builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            oRet = builder.Register()
        End If

    End Function

    Private Sub UstawDefaulty()
        ApplicationData.Current.LocalSettings.Values("sRunLog") = ""

        oForcePL.IsOn = App.GetSettingsBool("bForcePL", True)
        oEventsNo.Value = App.GetSettingsInt("iEventNo", 2)
        oFontSize.Value = App.GetSettingsInt("iFontSize", 3)
        oNextEvent.IsOn = App.GetSettingsBool("bNextEvent", True)
        oPictDay.IsOn = App.GetSettingsBool("bPictDay", True)

    End Sub
    Private Async Sub OnLoaded_Main(sender As Object, e As RoutedEventArgs)

        UstawDefaulty()
        Await UstawTriggery()

        If App.GetSettingsBool("bCallFromTile") Then
            Await AppointmentManager.ShowTimeFrameAsync(Date.Now, TimeSpan.FromHours(24))
        Else

            tbModif.Text = "Build " & Package.Current.Id.Version.Major & "." &
            Package.Current.Id.Version.Minor & "." & Package.Current.Id.Version.Build

            ' wedle dokumentacji, Loaded event will always fire after OnNavigatedTo 
            ' ale na wszelki wypadek - bo najpierw jest to!
            If Not (oAppTrig Is Nothing) Then
                Await oAppTrig.RequestAsync()
            End If

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
End Class
