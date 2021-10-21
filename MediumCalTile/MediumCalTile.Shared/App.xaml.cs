using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MediumCalTile
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        #region "automat"
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            ConfigureFilters(global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory);

            this.InitializeComponent();
            // this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif
            Frame rootFrame = Windows.UI.Xaml.Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                //if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                //{
                //    //TODO: Load state from previously suspended application
                //}

                // Place the frame in the current Window
                Windows.UI.Xaml.Window.Current.Content = rootFrame;
            }

#if NETFX_CORE
            if (e.PrelaunchActivated != false)
                return;
#endif 
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                // Ensure the current window is active
                // ale tylko gdy nie jest z ikonki
                if (e.TileActivatedInfo != null)
                    Window.Current.Activate();
                else
                    Windows.ApplicationModel.Appointments.AppointmentManager.ShowTimeFrameAsync(DateTime.Now, TimeSpan.FromHours(24));
            
                Windows.UI.Xaml.Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        //private void OnSuspending(object sender, SuspendingEventArgs e)
        //{
        //    var deferral = e.SuspendingOperation.GetDeferral();
        //    //TODO: Save application state and stop any background activity
        //    deferral.Complete();
        //}


        /// <summary>
        /// Configures global logging
        /// </summary>
        /// <param name="factory"></param>
        static void ConfigureFilters(ILoggerFactory factory)
        {
            factory
                .WithFilter(new FilterLoggerSettings
                    {
                        { "Uno", LogLevel.Warning },
                        { "Windows", LogLevel.Warning },

						// Debug JS interop
						// { "Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug },

						// Generic Xaml events
						// { "Windows.UI.Xaml", LogLevel.Debug },
						// { "Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug },
						// { "Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.UIElement", LogLevel.Debug },

						// Layouter specific messages
						// { "Windows.UI.Xaml.Controls", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.Panel", LogLevel.Debug },
						// { "Windows.Storage", LogLevel.Debug },

						// Binding related messages
						// { "Windows.UI.Xaml.Data", LogLevel.Debug },

						// DependencyObject memory references tracking
						// { "ReferenceHolder", LogLevel.Debug },

						// ListView-related messages
						// { "Windows.UI.Xaml.Controls.ListViewBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.ListView", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.GridView", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.VirtualizingPanelLayout", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.NativeListViewBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.ListViewBaseSource", LogLevel.Debug }, //iOS
						// { "Windows.UI.Xaml.Controls.ListViewBaseInternalContainer", LogLevel.Debug }, //iOS
						// { "Windows.UI.Xaml.Controls.NativeListViewBaseAdapter", LogLevel.Debug }, //Android
						// { "Windows.UI.Xaml.Controls.BufferViewCache", LogLevel.Debug }, //Android
						// { "Windows.UI.Xaml.Controls.VirtualizingPanelGenerator", LogLevel.Debug }, //WASM
					}
                )
#if DEBUG
				.AddConsole(LogLevel.Debug);
#else
                .AddConsole(LogLevel.Information);
#endif
        }
#endregion


#region "Moje - z ProcesWtle"
        public async void Run(Windows.ApplicationModel.Background.IBackgroundTaskInstance taskInstance)
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

            if (p.k.GetSettingsBool("bForcePL", true) | (System.Globalization.CultureInfo.CurrentCulture.Name.Substring(0, 2) ?? "") == "pl")
            {
                switch (iDay)
                {
                    case 0:
                        GetDTygNameRet = "niedziela";
                        break;
                    case 1:
                        GetDTygNameRet = "poniedziałek";
                        break;
                    case 2:
                        GetDTygNameRet = "wtorek";
                        break;
                    case 3:
                        GetDTygNameRet = "środa";
                        break;
                    case 4:
                        GetDTygNameRet = "czwartek";
                        break;

                    case 5:
                        GetDTygNameRet = "piątek";
                        break;
                    case 6:
                        GetDTygNameRet = "sobota";
                        break;
                    case 8:
                        GetDTygNameRet = "Do";
                        break;
                    case 9:
                        GetDTygNameRet = "jutro";
                        break;
                    case 10:
                        GetDTygNameRet = "ndz";
                        break;
                    case 11:
                        GetDTygNameRet = "pon";
                        break;
                    case 12:
                        GetDTygNameRet = "wtorek";
                        break;
                    case 13:
                        GetDTygNameRet = "środa";
                        break;
                    case 14:
                        GetDTygNameRet = "czw";
                        break;
                    case 15:
                        GetDTygNameRet = "piątek";
                        break;
                    case 16:
                        GetDTygNameRet = "sobota";
                        break;
                    case 19:
                        GetDTygNameRet = "… ";
                        break;
                    case 20:
                        GetDTygNameRet = "n";
                        break;
                    case 21:
                        GetDTygNameRet = "pn";
                        break;
                    case 22:
                        GetDTygNameRet = "wt";
                        break;
                    case 23:
                        GetDTygNameRet = "śr";
                        break;
                    case 24:
                        GetDTygNameRet = "czw";
                        break;
                    case 25:
                        GetDTygNameRet = "pt";
                        break;
                    case 26:
                        GetDTygNameRet = "s";
                        break;
                    default:
                        GetDTygNameRet = "(dztyg)";
                        break;
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
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Sunday);
                            break;
                        case 1:
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Monday);
                            break;
                        case 2:
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Tuesday);
                            break;
                        case 3:
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Wednesday);
                            break;
                        case 4:
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Thursday);
                            break;
                        case 5:
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Friday);
                            break;
                        case 6:
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Saturday);
                            break;
                        case 8:
                            // GetDTygName = ResourceManager.GetString("GetDTygName_Do")
                            GetDTygNameRet = "till";
                            break;
                        case 9:
                            // GetDTygName = ResourceManager.GetString("GetDTygName_Jutro")
                            GetDTygNameRet = "tomorrow";
                            break;
                        case 10:
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Sunday);
                            break;
                        case 11:
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Monday);
                            break;
                        case 12:
                            GetDTygNameRet = "tue";
                            break;
                        case 13:
                            GetDTygNameRet = "wed";
                            break;
                        case 14:
                            GetDTygNameRet = "thu";
                            break;
                        case 15:
                            GetDTygNameRet = withBlock.DateTimeFormat.GetDayName(DayOfWeek.Friday);
                            break;
                        case 16:
                            GetDTygNameRet = "sat";
                            break;
                        case 19:
                            GetDTygNameRet = "… ";
                            break;
                        case 20:
                            GetDTygNameRet = "sun";
                            break;
                        case 21:
                            GetDTygNameRet = "mon";
                            break;
                        case 22:
                            GetDTygNameRet = "tue";
                            break;
                        case 23:
                            GetDTygNameRet = "wed";
                            break;
                        case 24:
                            GetDTygNameRet = "thu";
                            break;
                        case 25:
                            GetDTygNameRet = "fri";
                            break;
                        case 26:
                            GetDTygNameRet = "sat";
                            break;
                        default:
                            GetDTygNameRet = "(dztyg)";
                            break;
                    }
                }
            }

            return GetDTygNameRet;
        }

        private string GodzMin2Txt(int iHr, int iMin)
        {
            string sTmp;
            sTmp = iHr + ":" + iMin;
            if (iMin == 0)
                sTmp = sTmp + "0";
            return sTmp;
        }

        private static string SafeString(string sTxt)
        {
            string sTmp = sTxt;
            sTmp = sTmp.Replace("&", "&amp;");   // to musi byc pierwsze!
                                                 // dopiero teraz pozostale
            sTmp = sTmp.Replace("<", "&gt;");
            sTmp = sTmp.Replace(">", "&lt;");
            sTmp = sTmp.Replace("\n", " ");   // 20171114: bo miałem subject z <cr> i zepsuło to układ na Tile
            sTmp = sTmp.Replace("\r", " ");
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
            string sTmp;
            if (!string.IsNullOrEmpty(sWhere))
                sWhere = SafeString("  (" + sWhere + ")");
            sTmp = CreateOpisWhen(sWhen, sToWhen, 0) + sWhere;
            sTmp = "<text hint-style='body'>" + SafeString(sTitle) + "</text>\n<text hint-style='captionSubtle'>" + sTmp + "</text>\n";
            // "<text hint-style='captionSubtle'></text>"

            return sTmp;

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
            string sTmp;
            sTmp = "<text hint-style='caption'>" + GetDTygName(19) + CreateOpisWhen(sWhen, sToWhen, 20);
            if (p.k.GetSettingsBool("bNextTitle", false))
                sTmp = sTmp + ": " + SafeString(sTitle);
            sTmp = sTmp + "</text>\n";
            return sTmp;
        }

        private async System.Threading.Tasks.Task Calendar2TileAsync()
        {
            // 20171201: w tej funkcji bywa Crash, dodaje test na Nothing oStore i oCalendars
            // bo XML powinien byc poprawny - jest w koncu SafeString na tym co moze miec dziwne znaki

            Windows.ApplicationModel.Appointments.AppointmentStore oStore = null;
            // If GetSettingsBool("convertHtmlTxt", False) Then
            // Try
            // oStore = Await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite)
            // Catch ex As Exception
            // oStore = Nothing
            // End Try
            // End If
            if (oStore is null)
            {
                oStore = await Windows.ApplicationModel.Appointments.AppointmentManager.RequestStoreAsync(Windows.ApplicationModel.Appointments.AppointmentStoreAccessType.AllCalendarsReadOnly);
            }

            if (oStore is null)
                return;
            IReadOnlyList<Windows.ApplicationModel.Appointments.Appointment> oCalendars;

            // przekopiowane z MyCameras
            var oCalOpt = new Windows.ApplicationModel.Appointments.FindAppointmentsOptions();
            oCalOpt.IncludeHidden = true;
            // nie użyte: oCalOpt.FetchProperties.Add(Windows.ApplicationModel.Appointments.AppointmentProperties.AllDay);
            oCalOpt.FetchProperties.Add(Windows.ApplicationModel.Appointments.AppointmentProperties.Location);
            // nie użyte: oCalOpt.FetchProperties.Add(Windows.ApplicationModel.Appointments.AppointmentProperties.Reminder);
            oCalOpt.FetchProperties.Add(Windows.ApplicationModel.Appointments.AppointmentProperties.StartTime);   // default
            oCalOpt.FetchProperties.Add(Windows.ApplicationModel.Appointments.AppointmentProperties.Duration);
            oCalOpt.FetchProperties.Add(Windows.ApplicationModel.Appointments.AppointmentProperties.Subject);
            // If GetSettingsBool("convertHtmlTxt", False) Then
            // oCalOpt.FetchProperties.Add(Appointments.AppointmentProperties.Details)
            // oCalOpt.FetchProperties.Add(Appointments.AppointmentProperties.DetailsKind)
            // End If
            oCalOpt.MaxCount = 20;
            oCalendars = await oStore.FindAppointmentsAsync(DateTime.Now, TimeSpan.FromDays(7), oCalOpt);
            if (oCalendars is null)
                return;
            int iCnt;
            string sEvents, sEvents1, sEventsHalf;
            sEvents = "";
            sEvents1 = "";
            sEventsHalf = "";
            string sOldEvent = "";
            iCnt = p.k.GetSettingsInt("iEventNo", 2);
            foreach (Windows.ApplicationModel.Appointments.Appointment oApp in oCalendars)
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
            if (p.k.GetSettingsBool("bNextEvent", true))
                sXml = sXml + sEventsHalf;
            sXml = sXml + "<text hint-style='captionSubtle'></text>";

            // gdy jest obrazek, to  domyslna jest mniejsza czcionka oraz nie ma skrotow dni tygodnia
            if (p.k.GetSettingsBool("bPictDay", true))
            {
                sXml = sXml + "<text hint-style='" + p.k.GetSettingsString("sFontSize", "title") + "' hint-align='left'>";
                sXml = sXml + GetDTygName((int)DateTime.Now.DayOfWeek) + "</text>";
                sXml = sXml + @"<image src='Dni\" + DateTime.Now.Day.ToString("d2") + ".png' placement='background'/>";
            }
            else
            {
                // gdy nie ma obrazka w tle, to wieksza czcionka i ze skrotami
                sXml = sXml + "<text hint-style='" + p.k.GetSettingsString("sFontSize", "subheader") + "' hint-align='right'>";
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
            if (p.k.GetSettingsBool("bNextEvent", true))
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
            var oXml = new Windows.Data.Xml.Dom.XmlDocument();
            oXml.LoadXml(sXml);
            var oTile = new Windows.UI.Notifications.TileNotification(oXml);
            if (oTile is object)
                Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Update(oTile);
        }

        // powtórka wszystkich funkcji, bo stąd nie widać App.!

#endregion
    }
}
