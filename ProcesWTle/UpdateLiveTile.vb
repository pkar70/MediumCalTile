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

    Public Async Sub ResetTriggers()
        ' 20171023: do osobnej funkcji, bo sa w niej crash

        For Each oTask In BackgroundTaskRegistration.AllTasks

            ' kiedys uzywane - kasujemy na wszelki wypadek, jakby cos gdzies zostalo
            If oTask.Value.Name = "MediumCalTileBackgroundTimer" Then oTask.Value.Unregister(True)

            ' aktualnie uzywane
            If oTask.Value.Name = "MediumCalTileBackgroundUser" Then oTask.Value.Unregister(True)
            ' tego nie ma jak przywrócic (zmienna globalna w App)
            'If oTask.Value.Name = "MediumCalTileBackgroundApp" Then oTask.Value.Unregister(True)
            If oTask.Value.Name = "MediumCalTileCalendarNotification" Then oTask.Value.Unregister(True)

            ' a ten nie potrzeba - jest OneShot, i wlasnie sie zdarzyl :)
            ' 20171023: a wlasnie ze potrzeba, bo dalej istnieje?? Podczas debug wchodzi do unregister!
            If oTask.Value.Name = "MediumCalTileServicing" Then oTask.Value.Unregister(False)
        Next
        'BackgroundExecutionManager.RemoveAccess()

        ' wklejone, a powinno byc: MainPage.UstawTriggery
        Dim oBAS As BackgroundAccessStatus
        oBAS = Await BackgroundExecutionManager.RequestAccessAsync()

        If oBAS = BackgroundAccessStatus.AlwaysAllowed Or oBAS = BackgroundAccessStatus.AllowedSubjectToSystemPolicy Then

            Dim builder As BackgroundTaskBuilder = New BackgroundTaskBuilder
            Dim oRet As BackgroundTaskRegistration

            ' user sie pojawia - po to, zeby pokazac (obracaniem tile) ze coś sie dzieje
            builder.SetTrigger(New SystemTrigger(SystemTriggerType.UserPresent, False))
            builder.Name = "MediumCalTileBackgroundUser"
            builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            oRet = builder.Register()

            ' ten jest na pewno usuniety...
            ' 20171023: wylaczenie oTrig i warunek, ale crash raczej zwiazany z tym ze nie bylo unregister?
            Dim oTrig = New SystemTrigger(SystemTriggerType.ServicingComplete, True)
            If oTrig IsNot Nothing Then
                builder.SetTrigger(oTrig)
                builder.Name = "MediumCalTileServicing"
                builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
                oRet = builder.Register()
            End If

            ' ten jest na pewno usuniety
            builder.SetTrigger(New AppointmentStoreNotificationTrigger) ' <-- crash?
            builder.Name = "MediumCalTileCalendarNotification"
            builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
            oRet = builder.Register()
        End If

    End Sub

    Public Async Sub Run(taskInstance As IBackgroundTaskInstance) Implements IBackgroundTask.Run
        If taskInstance.Task.Name = "MediumCalTileServicing" Then
            ' Await ResetTriggers
        End If

        Dim deferral As BackgroundTaskDeferral = taskInstance.GetDeferral
        Await Calendar2TileAsync()
        deferral.Complete()
    End Sub


    ' http://johnrah.com.au/2017/01/16/update-live-tiles-by-using-a-background-task/

    Private Function GetDTygName(iDay As Integer) As String
        ' 0-6, przy dacie nastepnego wydarzenia
        ' 8-9, dodatkowe msg
        ' 10-16, w dolnej linii

        If GetSettingsBool("bForcePL", True) Or
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
                Case 19
                    GetDTygName = "… "
                Case 20
                    GetDTygName = "n"
                Case 21
                    GetDTygName = "pn"
                Case 22
                    GetDTygName = "wt"
                Case 23
                    GetDTygName = "śr"
                Case 24
                    GetDTygName = "czw"
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
                    Case 19
                        GetDTygName = "… "
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

    Private Shared Function SafeString(sTxt As String) As String
        Dim sTmp = sTxt
        sTmp = sTmp.Replace("&", "&amp;")   ' to musi byc pierwsze!
        ' dopiero teraz pozostale
        sTmp = sTmp.Replace("<", "&gt;")
        sTmp = sTmp.Replace(">", "&lt;")
        Return sTmp
    End Function

    Private Function CreateOpisWhen(sWhen As Date, sToWhen As Date, iDni As Integer) As String
        If sWhen.DayOfYear = Date.Now.DayOfYear Then
            If sWhen <= Date.Now Then
                Return GetDTygName(8) & " " & GodzMin2Txt(sToWhen.Hour, sToWhen.Minute)
            Else
                Return GodzMin2Txt(sWhen.Hour, sWhen.Minute)
            End If
        ElseIf sWhen.DayOfYear = Date.Now.DayOfYear + 1 Then
            Return GetDTygName(9) & ", " & GodzMin2Txt(sWhen.Hour, sWhen.Minute)
        Else
            Return GetDTygName(sWhen.DayOfWeek + iDni) & ", " & GodzMin2Txt(sWhen.Hour, sWhen.Minute)
        End If

    End Function
    Private Function CreateEventDescr(sTitle As String, sWhen As Date, sToWhen As Date, sWhere As String) As String
        Dim sTmp As String
        If sWhere <> "" Then sWhere = SafeString("  (" & sWhere & ")")

        sTmp = CreateOpisWhen(sWhen, sToWhen, 0) & sWhere

        sTmp = "<text hint-style='body'>" & SafeString(sTitle) & "</text>" & vbCrLf &
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

    Private Function CreateEventHalfDescr(sTitle As String, sWhen As Date, sToWhen As Date, sWhere As String) As String
        Dim sTmp As String
        sTmp = "<text hint-style='caption'>" & GetDTygName(19) & CreateOpisWhen(sWhen, sToWhen, 20)
        If GetSettingsBool("bNextTitle", False) Then sTmp = sTmp & ": " & SafeString(sTitle)
        sTmp = sTmp & "</text>" & vbCrLf
        CreateEventHalfDescr = sTmp
    End Function
    Private Async Function Calendar2TileAsync() As Task

        Dim oStore As AppointmentStore
        oStore = Await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly)

        Dim oCalendars As IReadOnlyList(Of Appointment)
        oCalendars = Await oStore.FindAppointmentsAsync(Date.Now, TimeSpan.FromDays(7))
        Dim oApp As Appointment
        Dim iCnt As Integer
        Dim sEvents, sEvents1, sEventsHalf As String
        sEvents = ""
        sEvents1 = ""
        sEventsHalf = ""

        iCnt = GetSettingsInt("iEventNo", 2)

        For Each oApp In oCalendars
            If iCnt < 0 Then Exit For
            If iCnt = 0 Then
                sEvents1 = CreateEventDescr(oApp.Subject, oApp.StartTime.DateTime, oApp.StartTime.DateTime + oApp.Duration, oApp.Location)
                sEventsHalf = CreateEventHalfDescr(oApp.Subject, oApp.StartTime.DateTime, oApp.StartTime.DateTime + oApp.Duration, oApp.Location)
            Else
                ' If oApp.AllDay Then
                sEvents = sEvents & CreateEventDescr(oApp.Subject, oApp.StartTime.DateTime, oApp.StartTime.DateTime + oApp.Duration, oApp.Location)
            End If
            iCnt = iCnt - 1
        Next

        Dim sXml As String
        sXml = "<tile><visual>"

        sXml = sXml & "<binding template ='TileMedium' branding='none'>"
        sXml = sXml & sEvents
        If GetSettingsBool("bNextEvent", True) Then sXml = sXml & sEventsHalf

        sXml = sXml & "<text hint-style='captionSubtle'></text>"

        ' gdy jest obrazek, to  domyslna jest mniejsza czcionka oraz nie ma skrotow dni tygodnia
        If GetSettingsBool("bPictDay", True) Then
            sXml = sXml & "<text hint-style='" & GetSettingsString("sFontSize", "title") & "' hint-align='left'>"
            sXml = sXml & GetDTygName(Date.Now.DayOfWeek) & "</text>"
            sXml = sXml & "<image src='Dni\" & Date.Now.Day.ToString("d2") & ".png' placement='background'/>"
        Else
            ' gdy nie ma obrazka w tle, to wieksza czcionka i ze skrotami
            sXml = sXml & "<text hint-style='" & GetSettingsString("sFontSize", "subheader") & "' hint-align='right'>"
            ' "wtorek, 12" się nie miesci, więc scinam spacje - moze bedzie lepiej
            ' "sobota, 16" takze sie nie miesci
            If (Date.Now.DayOfWeek = DayOfWeek.Tuesday Or Date.Now.DayOfWeek = DayOfWeek.Saturday) And Date.Now.Day > 9 Then
                sXml = sXml & GetDTygName(Date.Now.DayOfWeek + 10) & "," & Date.Now.Day & "</text>"
            Else
                sXml = sXml & GetDTygName(Date.Now.DayOfWeek + 10) & ", " & Date.Now.Day & "</text>"
            End If
        End If
        sXml = sXml & "</binding>"

        sXml = sXml & "<binding template ='TileWide' branding='none'>"
        sXml = sXml & "<group><subgroup hint-weight='5'>"
        sXml = sXml & sEvents & sEvents1
        If GetSettingsBool("bNextEvent", True) Then sXml = sXml & sEventsHalf
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

    ' powtórka wszystkich funkcji, bo stąd nie widać App.!

    Public Shared Function GetSettingsBool(sName As String, iDefault As Boolean) As Boolean
        Dim sTmp As Boolean

        sTmp = iDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = CBool(ApplicationData.Current.RoamingSettings.Values(sName).ToString)
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = CBool(ApplicationData.Current.LocalSettings.Values(sName).ToString)
        End If

        Return sTmp

    End Function

    Public Shared Function GetSettingsInt(sName As String, iDefault As Integer) As Integer
        Dim sTmp As Integer

        sTmp = iDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = CInt(ApplicationData.Current.RoamingSettings.Values(sName).ToString)
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = CInt(ApplicationData.Current.LocalSettings.Values(sName).ToString)
        End If

        Return sTmp

    End Function

    Public Shared Function GetSettingsString(sName As String, sDefault As String) As String
        Dim sTmp As String

        sTmp = sDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = ApplicationData.Current.RoamingSettings.Values(sName).ToString
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = ApplicationData.Current.LocalSettings.Values(sName).ToString
        End If

        Return sTmp

    End Function

End Class
