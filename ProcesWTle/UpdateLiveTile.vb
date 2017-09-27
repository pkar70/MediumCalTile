Imports Windows.ApplicationModel.Background
Imports Windows.Data.Xml.Dom
Imports Windows.UI.Notifications
Imports Windows.ApplicationModel.Appointments
Imports Windows.Storage

' 21 IX 2017
' dodatkowy trigger na Servicing (przy upgrade) - usuniecie triggerów
' wyREMowanie triggera Timer (bo jest AppointmentStoreNotificationTrigger)
' usuniecie sleep(5 min) przy triggerach

Public NotInheritable Class UpdateLiveTile
    Implements IBackgroundTask


    Public Async Sub Run(taskInstance As IBackgroundTaskInstance) Implements IBackgroundTask.Run

        If taskInstance.Task.Name = "MediumCalTileServicing" Then
            For Each oTask In BackgroundTaskRegistration.AllTasks

                ' kiedys uzywane - kasujemy na wszelki wypadek, jakby cos gdzies zostalo
                If oTask.Value.Name = "MediumCalTileBackgroundTimer" Then oTask.Value.Unregister(True)

                ' aktualnie uzywane
                If oTask.Value.Name = "MediumCalTileBackgroundUser" Then oTask.Value.Unregister(True)
                If oTask.Value.Name = "MediumCalTileBackgroundApp" Then oTask.Value.Unregister(True)
                If oTask.Value.Name = "MediumCalTileCalendarNotification" Then oTask.Value.Unregister(True)

                ' a ten nie potrzeba - jest OneShot, i wlasnie sie zdarzyl :)
                'If oTask.Value.Name = "MediumCalTileServicing" Then oTask.Value.Unregister(False)
            Next
            'BackgroundExecutionManager.RemoveAccess()

            ' wklejone, a powinno byc: MainPage.UstawTriggery
            ' error 0xC000027B chyba tu jest, linia z Register (pierwszego triggera)
            'Dim oBAS As BackgroundAccessStatus

            'Dim deferralDodawanie As BackgroundTaskDeferral = taskInstance.GetDeferral

            'oBAS = Await BackgroundExecutionManager.RequestAccessAsync()

            'If oBAS = BackgroundAccessStatus.AlwaysAllowed Or oBAS = BackgroundAccessStatus.AllowedSubjectToSystemPolicy Then
            '    Dim builder As BackgroundTaskBuilder = New BackgroundTaskBuilder
            '    Dim oRet As BackgroundTaskRegistration
            '    builder.SetTrigger(New SystemTrigger(SystemTriggerType.UserPresent, False))
            '    builder.Name = "MediumCalTileBackgroundUser"
            '    builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            '    oRet = builder.Register()
            '    builder.SetTrigger(New SystemTrigger(SystemTriggerType.ServicingComplete, False))  ' user sie pojawia
            '    builder.Name = "MediumCalTileServicing"
            '    builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            '    oRet = builder.Register()
            '    builder.SetTrigger(New AppointmentStoreNotificationTrigger)
            '    builder.Name = "MediumCalTileCalendarNotification"
            '    builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            '    oRet = builder.Register()
            'End If

            'deferralDodawanie.Complete()

            Exit Sub
        End If

        'reset zmiennych - moze tester sklepowy wylatuje z errorem dlatego ze Exception w .ToString z NULLa
        If Not ApplicationData.Current.LocalSettings.Values.ContainsKey("bForcePL") Then
            ApplicationData.Current.LocalSettings.Values("bForcePL") = "1"
        End If
        If Not ApplicationData.Current.LocalSettings.Values.ContainsKey("iEventNo") Then
            ApplicationData.Current.LocalSettings.Values("iEventNo") = "2"
        End If
        If Not ApplicationData.Current.LocalSettings.Values.ContainsKey("sFontSize") Then
            ApplicationData.Current.LocalSettings.Values("sFontSize") = "subheader"
        End If

        'ApplicationData.Current.LocalSettings.Values("sRunLog") = Date.Now.ToString

        Dim deferral As BackgroundTaskDeferral = taskInstance.GetDeferral
        Await Calendar2TileAsync()
        deferral.Complete()
    End Sub


    ' http://johnrah.com.au/2017/01/16/update-live-tiles-by-using-a-background-task/

    Private Function GetDTygName(iDay As Integer) As String
        ' 0-6, przy dacie nastepnego wydarzenia
        ' 8-9, dodatkowe msg
        ' 10-16, w dolnej linii

        If CBool(Windows.Storage.ApplicationData.Current.LocalSettings.Values("bForcePL").ToString) Or
            Globalization.CultureInfo.CurrentCulture.Name.Substring(0, 2) = "pl" Then
            Select Case iDay
                Case 0
                    GetDTygName = "niedziela"
                Case 1
                    GetDTygName = "poniedziałek"
                Case 2
                    GetDTygName = "wtorek"
                Case 3
                    GetDTygName = "środa"
                Case 4
                    GetDTygName = "czwartek"
                Case 5
                    GetDTygName = "piątek"
                Case 6
                    GetDTygName = "sobota"
                Case 8
                    GetDTygName = "Do"
                Case 9
                    GetDTygName = "jutro"
                Case 10
                    GetDTygName = "ndz"
                Case 11
                    GetDTygName = "pon"
                Case 12
                    GetDTygName = "wtorek"
                Case 13
                    GetDTygName = "środa"
                Case 14
                    GetDTygName = "czw"
                Case 15
                    GetDTygName = "piątek"
                Case 16
                    GetDTygName = "sobota"
                Case 20
                    GetDTygName = "n"
                Case 21
                    GetDTygName = "pn"
                Case 22
                    GetDTygName = "wt"
                Case 23
                    GetDTygName = "śr"
                Case 24
                    GetDTygName = "cz"
                Case 25
                    GetDTygName = "pt"
                Case 26
                    GetDTygName = "s"
                Case Else
                    GetDTygName = "(dztyg)"
            End Select
        Else
            ' Dim oResLoad As New Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView()
            ' Dim Text As String = loader.GetString("Farewell")
            With Globalization.CultureInfo.CurrentCulture
                ' pełne dni tygodnia: działa dla dowolnego jezyka
                ' pozostale robi na angielskie niestety
                Select Case iDay
                    Case 0
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Sunday)
                    Case 1
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Monday)
                    Case 2
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Tuesday)
                    Case 3
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Wednesday)
                    Case 4
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Thursday)
                    Case 5
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Friday)
                    Case 6
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Saturday)
                    Case 8
                        'GetDTygName = ResourceManager.GetString("GetDTygName_Do")
                        GetDTygName = "till"
                    Case 9
                        'GetDTygName = ResourceManager.GetString("GetDTygName_Jutro")
                        GetDTygName = "tomorrow"
                    Case 10
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Sunday)
                    Case 11
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Monday)
                    Case 12
                        GetDTygName = "tue"
                    Case 13
                        GetDTygName = "wed"
                    Case 14
                        GetDTygName = "thu"
                    Case 15
                        GetDTygName = .DateTimeFormat.GetDayName(DayOfWeek.Friday)
                    Case 16
                        GetDTygName = "sat"
                    Case 20
                        GetDTygName = "sun"
                    Case 21
                        GetDTygName = "mon"
                    Case 22
                        GetDTygName = "tue"
                    Case 23
                        GetDTygName = "wed"
                    Case 24
                        GetDTygName = "thu"
                    Case 25
                        GetDTygName = "fri"
                    Case 26
                        GetDTygName = "sat"
                    Case Else
                        GetDTygName = "(dztyg)"
                End Select

            End With

        End If
    End Function
    Private Function GodzMin2Txt(iHr As Integer, iMin As Integer) As String
        Dim sTmp As String
        sTmp = iHr & ":" & iMin
        If iMin = 0 Then sTmp = sTmp & "0"
        GodzMin2Txt = sTmp
    End Function
    Private Function CreateEventDescr(sTitle As String, sWhen As Date, sToWhen As Date, sWhere As String) As String
        Dim sTmp As String
        If sWhere <> "" Then sWhere = "  (" & sWhere & ")"
        If sWhen.DayOfYear = Date.Now.DayOfYear Then
            If sWhen <= Date.Now Then
                sTmp = GetDTygName(8) & " " & GodzMin2Txt(sToWhen.Hour, sToWhen.Minute) & sWhere
            Else
                sTmp = GodzMin2Txt(sWhen.Hour, sWhen.Minute) & sWhere
            End If
        ElseIf sWhen.DayOfYear = Date.Now.DayOfYear + 1 Then
            sTmp = GetDTygName(9) & ", " & GodzMin2Txt(sWhen.Hour, sWhen.Minute) & sWhere
        Else
            sTmp = GetDTygName(sWhen.DayOfWeek) & ", " & GodzMin2Txt(sWhen.Hour, sWhen.Minute) & sWhere
        End If

        sTmp = "<text hint-style='body'>" & sTitle & "</text>" & vbCrLf &
               "<text hint-style='captionSubtle'>" & sTmp & "</text>" & vbCrLf
        '        "<text hint-style='captionSubtle'></text>"

        CreateEventDescr = sTmp

        ' rozmiary czcionki:
        ' caption   12 regular
        ' body      15 regular
        ' base      15 semibold
        ' subtitle  20 regular
        ' title     24 semilight
        ' subheader 34 light
        ' header    46 light


    End Function
    Private Async Function Calendar2TileAsync() As Task

        Dim oStore As AppointmentStore
        oStore = Await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly)

        Dim oCalendars As IReadOnlyList(Of Appointment)
        oCalendars = Await oStore.FindAppointmentsAsync(Date.Now, TimeSpan.FromDays(7))
        Dim oApp As Appointment
        Dim iCnt As Integer
        Dim sEvents, sEvents1 As String
        sEvents = ""
        sEvents1 = ""

        iCnt = CInt(Windows.Storage.ApplicationData.Current.LocalSettings.Values("iEventNo").ToString)

        For Each oApp In oCalendars
            If iCnt < 0 Then Exit For
            If iCnt = 0 Then
                sEvents1 = CreateEventDescr(oApp.Subject, oApp.StartTime.DateTime, oApp.StartTime.DateTime + oApp.Duration, oApp.Location)
            Else
                ' If oApp.AllDay Then
                sEvents = sEvents & CreateEventDescr(oApp.Subject, oApp.StartTime.DateTime, oApp.StartTime.DateTime + oApp.Duration, oApp.Location)
            End If
            iCnt = iCnt - 1
        Next

        Dim sXml As String
        sXml = "<tile><visual>"

        sXml = sXml & "<binding template ='TileMedium' branding='none'>"
        sXml = sXml & sEvents & "<text hint-style='captionSubtle'></text>"
        sXml = sXml & "<text hint-style='" &
            Windows.Storage.ApplicationData.Current.LocalSettings.Values("sFontSize").ToString &
            "' hint-align='right'>"

        ' "wtorek, 12" się nie miesci, więc scinam spacje - moze bedzie lepiej
        ' "sobota, 16" takze sie nie miesci
        If (Date.Now.DayOfWeek = DayOfWeek.Tuesday Or Date.Now.DayOfWeek = DayOfWeek.Saturday) And Date.Now.Day > 9 Then
            sXml = sXml & GetDTygName(Date.Now.DayOfWeek + 10) & "," & Date.Now.Day & "</text>"
        Else
            sXml = sXml & GetDTygName(Date.Now.DayOfWeek + 10) & ", " & Date.Now.Day & "</text>"
        End If
        sXml = sXml & "</binding>"

        sXml = sXml & "<binding template ='TileWide' branding='none'>"
        sXml = sXml & "<group><subgroup hint-weight='5'>"
        sXml = sXml & sEvents & sEvents1
        sXml = sXml & "</subgroup>"
        ' dla Wide:
        ' bylo" subtitle+title+header, 20+24+46, mozna ciut obnizyc
        ' jest: caption+title+caption+header, 12+24+12+46 = 4 wiecej niz bylo
        sXml = sXml & "<subgroup hint-weight='1'>"
        sXml = sXml & "<text hint-style='caption' hint-align='right'> </text>"
        sXml = sXml & "<text hint-style='title' hint-align='right'>" & GetDTygName(Date.Now.DayOfWeek + 20) & "</text>"
        sXml = sXml & "<text hint-style='caption' hint-align='right'> </text>"
        sXml = sXml & "<text hint-style='header' hint-align='right'>" & Date.Now.Day & "</text>"
        sXml = sXml & "</subgroup></group>"
        sXml = sXml & "</binding>"

        sXml = sXml & "</visual></tile>"

        Dim oXml As New XmlDocument
        oXml.LoadXml(sXml)

        Dim oTile = New Windows.UI.Notifications.TileNotification(oXml)
        TileUpdateManager.CreateTileUpdaterForApplication().Update(oTile)

    End Function
End Class
