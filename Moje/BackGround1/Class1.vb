Imports Windows.ApplicationModel.Background
Imports Windows.ApplicationModel.Appointments
Imports Windows.Data.Xml.Dom
Imports Windows.UI.Notifications

Public NotInheritable Class ClassDlaTla
    '    Public sealed Class BlogFeedBackgroundTask  : IBackgroundTask
    Inherits BackgroundWorker


    Public Async Sub Run(taskInstance As IBackgroundTaskInstance)
        Dim deferral As Windows.ApplicationModel.Background.BackgroundTaskDeferral
        '// Get a deferral, to prevent the task from closing prematurely
        '// while asynchronous code Is still running.
        deferral = taskInstance.GetDeferral

        ReadCal()
        '// Inform the system that the task Is finished.
        deferral.Complete()
    End Sub

    Function GodzMin2Txt(iHr As Integer, iMin As Integer)
        Dim sTmp As String
        sTmp = iHr & ":" & iMin
        If iMin = 0 Then sTmp = sTmp & "0"
        GodzMin2Txt = sTmp
    End Function
    Function CreateEventDescr(sTitle As String, sWhen As Date, sWhere As String)
        Dim sTmp As String
        If sWhen.DayOfYear = Date.Now.DayOfYear Then
            sTmp = sWhen.Hour & ":" & sWhen.Minute & "  (" & sWhere & ")"
        ElseIf sWhen.DayOfYear = Date.Now.DayOfYear + 1 Then
            sTmp = GetDTygName(9) & ", " & GodzMin2Txt(sWhen.Hour, sWhen.Minute) & "  (" & sWhere & ")"
        Else
            sTmp = GetDTygName(sWhen.DayOfWeek) & ", " & GodzMin2Txt(sWhen.Hour, sWhen.Minute) & "  (" & sWhere & ")"
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

    Function GetDTygName(iDay As Integer)
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
            Case Else
                GetDTygName = "(dztyg)"
        End Select
    End Function

    Private Async Sub ReadCal()

        Dim oStore As AppointmentStore
        oStore = Await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly)

        Dim oCalendars
        oCalendars = Await oStore.FindAppointmentsAsync(Date.Now, TimeSpan.FromDays(7))
        Dim oApp As Appointment
        Dim iCnt As Integer
        Dim sEvents As String
        iCnt = 0
        sEvents = ""

        For Each oApp In oCalendars
            If iCnt > 1 Then Exit For
            ' If oApp.AllDay Then
            sEvents = sEvents & CreateEventDescr(oApp.Subject, oApp.StartTime.DateTime, oApp.Location)
            iCnt = iCnt + 1
        Next

        Dim sXml As String
        sXml = "<tile><visual>"

        sXml = sXml & "<binding template ='TileMedium' branding='none'>"
        sXml = sXml & sEvents & "<text hint-style='captionSubtle'></text>"
        sXml = sXml & "<text hint-style='subheader' hint-align='right'>" &
            GetDTygName(Date.Now.DayOfWeek + 10) & ", " & Date.Now.Day & "</text>"
        sXml = sXml & "</binding>"

        sXml = sXml & "<binding template ='TileWide' branding='none'>"
        sXml = sXml & "<group><subgroup hint-weight='5'>"
        sXml = sXml & sEvents   ' mogloby byc jeszcze jedno event przy Wide
        sXml = sXml & "</subgroup>"
        sXml = sXml & "<subgroup hint-weight='1'>"
        sXml = sXml & "<text hint-style='title' hint-align='right'> </text>"    ' puste zeby data na dole
        sXml = sXml & "<text hint-style='title' hint-align='right'> </text>"
        sXml = sXml & "<text hint-style='header' hint-align='right'>" & Date.Now.Day & "</text>"
        sXml = sXml & "</subgroup></group>"
        sXml = sXml & "</binding>"

        sXml = sXml & "</visual></tile>"

        Dim oXml As XmlDocument
        oXml = New XmlDocument
        oXml.LoadXml(sXml)

        Dim oTile = New Windows.UI.Notifications.TileNotification(oXml)

        TileUpdateManager.CreateTileUpdaterForApplication().Update(oTile)

    End Sub
End Class
