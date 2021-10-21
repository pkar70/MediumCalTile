using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private static bool TriggersUsun()
        {
            try
            {

                foreach (var oTask in Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks)
                {
                    if (oTask.Value.Name.StartsWith("EnviroStat_"))
                        oTask.Value.Unregister(true);
                }

#if NETFX_CORE
                Windows.ApplicationModel.Background.BackgroundExecutionManager.RemoveAccess();
#endif 
                return true;
            }
            catch
            {
                return false;
            }

        }


        private static async System.Threading.Tasks.Task<bool> UstawTriggery()
        {
            Windows.ApplicationModel.Background.BackgroundAccessStatus oBAS;
            oBAS = await Windows.ApplicationModel.Background.BackgroundExecutionManager.RequestAccessAsync();

            if (oBAS == Windows.ApplicationModel.Background.BackgroundAccessStatus.AlwaysAllowed ||
                    oBAS == Windows.ApplicationModel.Background.BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
            {
                if (!TriggersUsun()) return false;
            }
            else
                return false;

            // czyli na pewno usunięte triggery, i możemy się nimi zająć
            try
            {
                var builder = new Windows.ApplicationModel.Background.BackgroundTaskBuilder();
                Windows.ApplicationModel.Background.BackgroundTaskRegistration oRet;

                // user sie pojawia - po to, zeby pokazac (obracaniem tile) ze coś sie dzieje
                builder.SetTrigger(new Windows.ApplicationModel.Background.SystemTrigger(
                        Windows.ApplicationModel.Background.SystemTriggerType.UserPresent, false));
                builder.Name = "MediumCalTileBackgroundUser";
                // builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
                oRet = builder.Register();

                //' odtworzenie oAppTrig
                //oAppTrig = New ApplicationTrigger
                //builder.SetTrigger(oAppTrig)  ' do wywolywania metody
                //builder.Name = "MediumCalTileBackgroundApp"
                //builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
                //oRet = builder.Register()

                builder.SetTrigger(new Windows.ApplicationModel.Background.SystemTrigger(
                    Windows.ApplicationModel.Background.SystemTriggerType.ServicingComplete, true));
                builder.Name = "MediumCalTileServicing";
                // builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
                oRet = builder.Register();

                builder.SetTrigger(new Windows.ApplicationModel.Background.AppointmentStoreNotificationTrigger());
                builder.Name = "MediumCalTileCalendarNotification";
                // builder.TaskEntryPoint = "BackgroundTasks.UpdateLiveTile"
                oRet = builder.Register();

                return true;
            }
            catch
            {
                return false;
            }

        }

        private void LoadSetting()
        {
            p.k.SetSettingsString("sRunLog", "");
            oForcePL.IsOn = p.k.GetSettingsBool("bForcePL");
            oEventsNo.Value = p.k.GetSettingsInt("iEventNo", 2);
            oFontSize.Value = p.k.GetSettingsInt("iFontSize", 3);
            oNextEvent.IsOn = p.k.GetSettingsBool("bNextEvent", true);
            oPictDay.IsOn = p.k.GetSettingsBool("bPictDay", true);
            oNextTitle.IsOn = p.k.GetSettingsBool("bNextTitle");
            uiDelDupl.IsOn = p.k.GetSettingsBool("bDelDupl");
            // uiConvertHtmlToggle.IsOn = false;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            LoadSetting();
            await UstawTriggery();

            if (p.k.GetSettingsBool("bCallFromTile"))
            {
                bOpenCal_Click(null, null);
            }
            else
            {
                tbModif.Text = "Build " + p.k.GetAppVers();

            //' wedle dokumentacji, Loaded event will always fire after OnNavigatedTo 
            //' ale na wszelki wypadek - bo najpierw jest to!
            //If oAppTrig IsNot Nothing Then Await oAppTrig.RequestAsync()

            }


        }

        private static string FontSizeNum2Txt(int iSize)
        {
            switch(iSize)
            {
                case 1:
                    return "base";
            case 2:
                    return "subtitle";
            case 3:
                    return "title";
            case 4:
                    return "subheader";
            case 5:
                    return "header";
                default:
                    return "subheader";
            }
        //' rozmiary czcionki:
        //' caption   12 regular
        //' body      15 regular
        //' base      15 semibold
        //' subtitle  20 regular
        //' title     24 semilight
        //' subheader 34 light
        //' header    46 light
    }
        private async void bUpdate_Click(object sender, RoutedEventArgs e)
        {

            p.k.SetSettingsBool("bForcePL", oForcePL);
            p.k.SetSettingsInt("iEventNo", (int)oEventsNo.Value);
            p.k.SetSettingsString("sFontSize", FontSizeNum2Txt((int)oFontSize.Value));
            p.k.SetSettingsInt("iFontSize", (int)oFontSize.Value);
            p.k.SetSettingsBool("bNextEvent", oNextEvent.IsOn);
            p.k.SetSettingsBool("bPictDay", oPictDay.IsOn);
            p.k.SetSettingsBool("bNextTitle", oNextTitle.IsOn);
            p.k.SetSettingsBool("bDelDupl", uiDelDupl.IsOn);
            //p.k.SetSettingsBool("convertHtmlTxt", uiConvertHtmlToggle.IsOn);

            //' *TODO* OnChange kazdego elementu sie ustawia dany element. Ale to pozniej. 

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.StartScreen.StartScreenManager"))
            {
                //' czyli od Windows 15063, Creators Update - Aska nie ma!
                //' https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-primary-tile-apis
                var oEntry = (await Windows.ApplicationModel.Package.Current.GetAppListEntriesAsync()).ElementAt(0);

                //' jako ze Pin dziala tylko w mobile i w desktop - na wszelki wypadek
                bool isSupported = Windows.UI.StartScreen.StartScreenManager.GetDefault().SupportsAppListEntry(oEntry);
                if (isSupported)
                {
                    bool isPinned = await Windows.UI.StartScreen.StartScreenManager.GetDefault().ContainsAppListEntryAsync(oEntry);
                    if (!isPinned)
                        await Windows.UI.StartScreen.StartScreenManager.GetDefault().RequestAddAppListEntryAsync(oEntry);
                }
            }

            // await oAppTrig.RequestAsync

        }

        private async void bOpenCal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.ApplicationModel.Appointments.AppointmentManager.ShowTimeFrameAsync(DateTime.Now, TimeSpan.FromHours(24));
            }
            catch
            {
                // ' bo w OnLoaded_Main.MoveNext jest jakis unknown error (dashboard)

            }
        }


        private void uiNextDay_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                oNextTitle.IsEnabled = oNextEvent.IsOn;
            }
            catch
            {
                // ' spodziewane - ktoregos moze jeszcze nie być
            }
        }

    }
}
