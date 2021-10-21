using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace BackgroundTasks
{

    // 21 IX 2017
    // dodatkowy trigger na Servicing (przy upgrade) - usuniecie triggerów
    // wyREMowanie triggera Timer (bo jest AppointmentStoreNotificationTrigger)
    // usuniecie sleep(5 min) przy triggerach

    public sealed class UpdateLiveTile : IBackgroundTask
    {
        public async void ResetTriggers()
        {
            // 20171023: do osobnej funkcji, bo sa w niej crash

            foreach (var oTask in BackgroundTaskRegistration.AllTasks)
            {

                // kiedys uzywane - kasujemy na wszelki wypadek, jakby cos gdzies zostalo
                if ((oTask.Value.Name ?? "") == "MediumCalTileBackgroundTimer")
                    oTask.Value.Unregister(true);

                // aktualnie uzywane
                if ((oTask.Value.Name ?? "") == "MediumCalTileBackgroundUser")
                    oTask.Value.Unregister(true);
                // tego nie ma jak przywrócic (zmienna globalna w App)
                // If oTask.Value.Name = "MediumCalTileBackgroundApp" Then oTask.Value.Unregister(True)
                if ((oTask.Value.Name ?? "") == "MediumCalTileCalendarNotification")
                    oTask.Value.Unregister(true);

                // a ten nie potrzeba - jest OneShot, i wlasnie sie zdarzyl :)
                // 20171023: a wlasnie ze potrzeba, bo dalej istnieje?? Podczas debug wchodzi do unregister!
                if ((oTask.Value.Name ?? "") == "MediumCalTileServicing")
                    oTask.Value.Unregister(false);
            }
            // BackgroundExecutionManager.RemoveAccess()

            // wklejone, a powinno byc: MainPage.UstawTriggery
            BackgroundAccessStatus oBAS;
            oBAS = await BackgroundExecutionManager.RequestAccessAsync();
            if (oBAS == BackgroundAccessStatus.AlwaysAllowed | oBAS == BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
            {
                var builder = new BackgroundTaskBuilder();
                BackgroundTaskRegistration oRet;

                // user sie pojawia - po to, zeby pokazac (obracaniem tile) ze coś sie dzieje
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.UserPresent, false));
                builder.Name = "MediumCalTileBackgroundUser";
                builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile";
                oRet = builder.Register();

                // ten jest na pewno usuniety...
                // 20171023: wylaczenie oTrig i warunek, ale crash raczej zwiazany z tym ze nie bylo unregister?
                var oTrig = new SystemTrigger(SystemTriggerType.ServicingComplete, true);
                if (oTrig is object)
                {
                    builder.SetTrigger(oTrig);
                    builder.Name = "MediumCalTileServicing";
                    builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile";
                    oRet = builder.Register();
                }

                // ten jest na pewno usuniety
                builder.SetTrigger(new AppointmentStoreNotificationTrigger()); // <-- crash?
                builder.Name = "MediumCalTileCalendarNotification";
                builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile";
                oRet = builder.Register();
            }
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            if ((taskInstance.Task.Name ?? "") == "MediumCalTileServicing")
            {
                // Await ResetTriggers
            }

            await Calendar2TileAsync();
            deferral.Complete();
        }


        // http://johnrah.com.au/2017/01/16/update-live-tiles-by-using-a-background-task/

        private string GetDTygName(int iDay)
        {
            string GetDTygNameRet = default;
            // 0-6, przy dacie nastepnego wydarzenia
            // 8-9, dodatkowe msg
            // 10-16, w dolnej linii

            if (GetSettingsBool("bForcePL", true) | (System.Globalization.CultureInfo.CurrentCulture.Name.Substring(0, 2) ?? "") == "pl")
            {
                switch (iDay)
                {
                    case 0:
                        {
                            GetDTygNameRet = "niedziela";
                            break;
                        }

                    case 1:
                        {
                            GetDTygNameRet = "poniedziałek";
                            break;
                        }

                    case 2:
                        {
                            GetDTygNameRet = "wtorek";
                            break;
                        }

                    case 3:
                        {
                            GetDTygNameRet = "środa";
                            break;
                        }

                    case 4:
                        {
                            GetDTygNameRet = "czwartek";
                            break;
                        }

                    case 5:
                        {
                            GetDTygNameRet = "piątek";
                            break;
                        }

                    case 6:
                        {
                            GetDTygNameRet = "sobota";
                            break;
                        }

                    case 8:
                        {
                            GetDTygNameRet = "Do";
                            break;
                        }

                    case 9:
                        {
                            GetDTygNameRet = "jutro";
                            break;
                        }

                    case 10:
                        {
                            GetDTygNameRet = "ndz";
                            break;
                        }

                    case 11:
                        {
                            GetDTygNameRet = "pon";
                            break;
                        }

                    case 12:
                        {
                            GetDTygNameRet = "wtorek";
                            break;
                        }

                    case 13:
                        {
                            GetDTygNameRet = "środa";
                            break;
                        }

                    case 14:
                        {
                            GetDTygNameRet = "czw";
                            break;
                        }

                    case 15:
                        {
                            GetDTygNameRet = "piątek";
                            break;
                        }

                    case 16:
                        {
                            GetDTygNameRet = "sobota";
                            break;
                        }

                    case 19:
                        {
                            GetDTygNameRet = "… ";
                            break;
                        }

                    case 20:
                        {
                            GetDTygNameRet = "n";
                            break;
                        }

                    case 21:
                        {
                            GetDTygNameRet = "pn";
                            break;
                        }

                    case 22:
                        {
                            GetDTygNameRet = "wt";
                            break;
                        }

                    case 23:
                        {
                            GetDTygNameRet = "śr";
                            break;
                        }

                    case 24:
                        {
                            GetDTygNameRet = "czw";
                            break;
                        }

                    case 25:
                        {
                            GetDTygNameRet = "pt";
                            break;
                        }

                    case 26:
                        {
                            GetDTygNameRet = "s";
                            break;
                        }

                    default:
                        {
                            GetDTygNameRet = "(dztyg)";
                            break;
                        }
                }
            }
            else
            {
                // Dim oResLoad As New Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView()
                // Dim Text As String = loader.GetString("Farewell")
                {
                    var withBlock = System.Globalization.CultureInfo.CurrentCulture;
                    // pełne dni tygodnia: działa dla dowolnego jezyka
                    // pozostale robi na angielskie niestety
                    switch (iDay)
                    {
                        case 0:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Sunday);
                                break;
                            }

                        case 1:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Monday);
                                break;
                            }

                        case 2:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Tuesday);
                                break;
                            }

                        case 3:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Wednesday);
                                break;
                            }

                        case 4:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Thursday);
                                break;
                            }

                        case 5:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Friday);
                                break;
                            }

                        case 6:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Saturday);
                                break;
                            }

                        case 8:
                            {
                                // GetDTygName = ResourceManager.GetString("GetDTygName_Do")
                                GetDTygNameRet = "till";
                                break;
                            }

                        case 9:
                            {
                                // GetDTygName = ResourceManager.GetString("GetDTygName_Jutro")
                                GetDTygNameRet = "tomorrow";
                                break;
                            }

                        case 10:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Sunday);
                                break;
                            }

                        case 11:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Monday);
                                break;
                            }

                        case 12:
                            {
                                GetDTygNameRet = "tue";
                                break;
                            }

                        case 13:
                            {
                                GetDTygNameRet = "wed";
                                break;
                            }

                        case 14:
                            {
                                GetDTygNameRet = "thu";
                                break;
                            }

                        case 15:
                            {
                                GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Friday);
                                break;
                            }

                        case 16:
                            {
                                GetDTygNameRet = "sat";
                                break;
                            }

                        case 19:
                            {
                                GetDTygNameRet = "… ";
                                break;
                            }

                        case 20:
                            {
                                GetDTygNameRet = "sun";
                                break;
                            }

                        case 21:
                            {
                                GetDTygNameRet = "mon";
                                break;
                            }

                        case 22:
                            {
                                GetDTygNameRet = "tue";
                                break;
                            }

                        case 23:
                            {
                                GetDTygNameRet = "wed";
                                break;
                            }

                        case 24:
                            {
                                GetDTygNameRet = "thu";
                                break;
                            }

                        case 25:
                            {
                                GetDTygNameRet = "fri";
                                break;
                            }

                        case 26:
                            {
                                GetDTygNameRet = "sat";
                                break;
                            }

                        default:
                            {
                                GetDTygNameRet = "(dztyg)";
                                break;
                            }
                    }
                }
            }

            return GetDTygNameRet;
        }

        private string GodzMin2Txt(int iHr, int iMin)
        {
            string GodzMin2TxtRet = default;
            string sTmp;
            sTmp = iHr + ":" + iMin;
            if (iMin == 0)
                sTmp = sTmp + "0";
            GodzMin2TxtRet = sTmp;
            return GodzMin2TxtRet;
        }

        private static string SafeString(string sTxt)
        {
            string sTmp = sTxt;
            sTmp = sTmp.Replace("&", "&amp;");   // to musi byc pierwsze!
                                                 // dopiero teraz pozostale
            sTmp = sTmp.Replace("<", "&gt;");
            sTmp = sTmp.Replace(">", "&lt;");
            sTmp = sTmp.Replace(Constants.vbCr, " ");   // 20171114: bo miałem subject z <cr> i zepsuło to układ na Tile
            sTmp = sTmp.Replace(Constants.vbLf, " ");
            return sTmp;
        }

        private string CreateOpisWhen(DateTime sWhen, DateTime sToWhen, int iDni)
        {
            if (sWhen.DayOfYear == DateTime.Now.DayOfYear)
            {
                if (sWhen <= DateTime.Now)
                {
                    return GetDTygName(8) + " " + GodzMin2Txt(sToWhen.Hour, sToWhen.Minute);
                }
                else
                {
                    return GodzMin2Txt(sWhen.Hour, sWhen.Minute);
                }
            }
            else if (sWhen.DayOfYear == DateTime.Now.DayOfYear + 1)
            {
                return GetDTygName(9) + ", " + GodzMin2Txt(sWhen.Hour, sWhen.Minute);
            }
            else
            {
                return GetDTygName((int)sWhen.DayOfWeek + iDni) + ", " + GodzMin2Txt(sWhen.Hour, sWhen.Minute);
            }
        }

        private string CreateEventDescr(string sTitle, DateTime sWhen, DateTime sToWhen, string sWhere)
        {
            string CreateEventDescrRet = default;
            string sTmp;
            if (!string.IsNullOrEmpty(sWhere))
                sWhere = SafeString("  (" + sWhere + ")");
            sTmp = CreateOpisWhen(sWhen, sToWhen, 0) + sWhere;
            sTmp = "<text hint-style='body'>" + SafeString(sTitle) + "</text>" + Constants.vbCrLf + "<text hint-style='captionSubtle'>" + sTmp + "</text>" + Constants.vbCrLf;
            // "<text hint-style='captionSubtle'></text>"

            CreateEventDescrRet = sTmp;
            return CreateEventDescrRet;

            // rozmiary czcionki:
            // caption   12 regular
            // body      15 regular
            // base      15 semibold
            // subtitle  20 regular
            // title     24 semilight
            // subheader 34 light
            // header    46 light

        }

        private string CreateEventHalfDescr(string sTitle, DateTime sWhen, DateTime sToWhen, string sWhere)
        {
            string CreateEventHalfDescrRet = default;
            string sTmp;
            sTmp = "<text hint-style='caption'>" + GetDTygName(19) + CreateOpisWhen(sWhen, sToWhen, 20);
            if (GetSettingsBool("bNextTitle", false))
                sTmp = sTmp + ": " + SafeString(sTitle);
            sTmp = sTmp + "</text>" + Constants.vbCrLf;
            CreateEventHalfDescrRet = sTmp;
            return CreateEventHalfDescrRet;
        }

        private async Task Calendar2TileAsync()
        {
            // 20171201: w tej funkcji bywa Crash, dodaje test na Nothing oStore i oCalendars
            // bo XML powinien byc poprawny - jest w koncu SafeString na tym co moze miec dziwne znaki

            AppointmentStore oStore = null;
            // If GetSettingsBool("convertHtmlTxt", False) Then
            // Try
            // oStore = Await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite)
            // Catch ex As Exception
            // oStore = Nothing
            // End Try
            // End If
            if (oStore is null)
            {
                oStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);
            }

            if (oStore is null)
                return;
            IReadOnlyList<Appointment> oCalendars;

            // przekopiowane z MyCameras
            var oCalOpt = new FindAppointmentsOptions();
            oCalOpt.IncludeHidden = true;
            oCalOpt.FetchProperties.Add(AppointmentProperties.AllDay);
            oCalOpt.FetchProperties.Add(AppointmentProperties.Location);
            oCalOpt.FetchProperties.Add(AppointmentProperties.Reminder);
            oCalOpt.FetchProperties.Add(AppointmentProperties.StartTime);   // default
            oCalOpt.FetchProperties.Add(AppointmentProperties.Duration);
            oCalOpt.FetchProperties.Add(AppointmentProperties.Subject);
            // If GetSettingsBool("convertHtmlTxt", False) Then
            // oCalOpt.FetchProperties.Add(Appointments.AppointmentProperties.Details)
            // oCalOpt.FetchProperties.Add(Appointments.AppointmentProperties.DetailsKind)
            // End If
            oCalOpt.MaxCount = 20;
            oCalendars = await oStore.FindAppointmentsAsync(DateTime.Now, TimeSpan.FromDays(7), oCalOpt);
            if (oCalendars is null)
                return;
            Appointment oApp;
            int iCnt;
            string sEvents, sEvents1, sEventsHalf;
            sEvents = "";
            sEvents1 = "";
            sEventsHalf = "";
            string sOldEvent = "";
            iCnt = GetSettingsInt("iEventNo", 2);
            foreach (var oApp in oCalendars)
            {
                string sCurr;


                // If GetSettingsBool("convertHtmlTxt", False) Then
                // ' przetworzenie - tylko gdy naprawde tego chcemy
                // If oApp.DetailsKind = AppointmentDetailsKind.PlainText AndAlso
                // oApp.Details.Contains("<!-- Converted") Then

                // Dim sTxt As String = oApp.Details
                // sTxt = sTxt.Replace("<BR>", vbCrLf)
                // Dim iInd, iInd2 As Integer

                // For iLoop As Integer = 0 To 100  ' guard
                // iInd = sTxt.IndexOf("<")
                // If iInd < 0 Then Exit For
                // ' czyli mamy do usuniecia cos
                // iInd2 = sTxt.IndexOf(">", iInd)
                // sTxt = sTxt.Remove(iInd, iInd2 - iInd)
                // Next

                // oApp.Details = sTxt
                // End If
                // End If

                if (iCnt < 0)
                    break;
                if (iCnt == 0)
                {
                    sEvents1 = CreateEventDescr(oApp.Subject, oApp.StartTime.DateTime, oApp.StartTime.DateTime + oApp.Duration, oApp.Location);
                    if ((sOldEvent ?? "") == (sEvents1 ?? ""))
                        continue;
                    sEventsHalf = CreateEventHalfDescr(oApp.Subject, oApp.StartTime.DateTime, oApp.StartTime.DateTime + oApp.Duration, oApp.Location);
                }
                else
                {
                    sCurr = CreateEventDescr(oApp.Subject, oApp.StartTime.DateTime, oApp.StartTime.DateTime + oApp.Duration, oApp.Location);
                    if ((sOldEvent ?? "") == (sCurr ?? ""))
                        continue;
                    sOldEvent = sCurr;
                    // If oApp.AllDay Then
                    sEvents = sEvents + sCurr;
                }

                iCnt = iCnt - 1;
            }

            string sXml;
            sXml = "<tile><visual>";
            sXml = sXml + "<binding template ='TileMedium' branding='none'>";
            sXml = sXml + sEvents;
            if (GetSettingsBool("bNextEvent", true))
                sXml = sXml + sEventsHalf;
            sXml = sXml + "<text hint-style='captionSubtle'></text>";

            // gdy jest obrazek, to  domyslna jest mniejsza czcionka oraz nie ma skrotow dni tygodnia
            if (GetSettingsBool("bPictDay", true))
            {
                sXml = sXml + "<text hint-style='" + GetSettingsString("sFontSize", "title") + "' hint-align='left'>";
                sXml = sXml + GetDTygName((int)DateTime.Now.DayOfWeek) + "</text>";
                sXml = sXml + @"<image src='Dni\" + DateTime.Now.Day.ToString("d2") + ".png' placement='background'/>";
            }
            else
            {
                // gdy nie ma obrazka w tle, to wieksza czcionka i ze skrotami
                sXml = sXml + "<text hint-style='" + GetSettingsString("sFontSize", "subheader") + "' hint-align='right'>";
                // "wtorek, 12" się nie miesci, więc scinam spacje - moze bedzie lepiej
                // "sobota, 16" takze sie nie miesci
                if ((DateTime.Now.DayOfWeek == DayOfWeek.Tuesday | DateTime.Now.DayOfWeek == DayOfWeek.Saturday) & DateTime.Now.Day > 9)
                {
                    sXml = sXml + GetDTygName((int)DateTime.Now.DayOfWeek + 10) + "," + DateTime.Now.Day + "</text>";
                }
                else
                {
                    sXml = sXml + GetDTygName((int)DateTime.Now.DayOfWeek + 10) + ", " + DateTime.Now.Day + "</text>";
                }
            }

            sXml = sXml + "</binding>";
            sXml = sXml + "<binding template ='TileWide' branding='none'>";
            sXml = sXml + "<group><subgroup hint-weight='5'>";
            sXml = sXml + sEvents + sEvents1;
            if (GetSettingsBool("bNextEvent", true))
                sXml = sXml + sEventsHalf;
            sXml = sXml + "</subgroup>";
            // dla Wide:
            // bylo" subtitle+title+header, 20+24+46, mozna ciut obnizyc
            // jest: caption+title+caption+header, 12+24+12+46 = 4 wiecej niz bylo
            sXml = sXml + "<subgroup hint-weight='1'>";
            sXml = sXml + "<text hint-style='caption' hint-align='right'> </text>";
            sXml = sXml + "<text hint-style='title' hint-align='right'>" + GetDTygName((int)DateTime.Now.DayOfWeek + 20) + "</text>";
            sXml = sXml + "<text hint-style='caption' hint-align='right'> </text>";
            sXml = sXml + "<text hint-style='header' hint-align='right'>" + DateTime.Now.Day + "</text>";
            sXml = sXml + "</subgroup></group>";
            sXml = sXml + "</binding>";
            sXml = sXml + "</visual></tile>";
            var oXml = new XmlDocument();
            oXml.LoadXml(sXml);
            var oTile = new TileNotification(oXml);
            if (oTile is object)
                TileUpdateManager.CreateTileUpdaterForApplication().Update(oTile);
        }

        // powtórka wszystkich funkcji, bo stąd nie widać App.!

        public static bool GetSettingsBool(string sName, bool iDefault)
        {
            bool sTmp;
            sTmp = iDefault;
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName))
            {
                sTmp = Conversions.ToBoolean(ApplicationData.Current.RoamingSettings.Values[sName].ToString());
            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(sName))
            {
                sTmp = Conversions.ToBoolean(ApplicationData.Current.LocalSettings.Values[sName].ToString());
            }

            return sTmp;
        }

        public static int GetSettingsInt(string sName, int iDefault)
        {
            int sTmp;
            sTmp = iDefault;
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName))
            {
                sTmp = Conversions.ToInteger(ApplicationData.Current.RoamingSettings.Values[sName].ToString());
            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(sName))
            {
                sTmp = Conversions.ToInteger(ApplicationData.Current.LocalSettings.Values[sName].ToString());
            }

            return sTmp;
        }

        public static string GetSettingsString(string sName, string sDefault)
        {
            string sTmp;
            sTmp = sDefault;
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName))
            {
                sTmp = ApplicationData.Current.RoamingSettings.Values[sName].ToString();
            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(sName))
            {
                sTmp = ApplicationData.Current.LocalSettings.Values[sName].ToString();
            }

            return sTmp;
        }
    }
}