using System.Diagnostics;
using System.Xml;

namespace BeforeUno
{
    //    https://stackoverflow.com/questions/2570004/how-to-add-multiple-widgets-in-the-same-app

    //https://stackoverflow.com/questions/13563481/updating-multiple-app-widgets-in-android

    //https://stackoverflow.com/questions/28964029/how-to-avoid-to-creating-multiple-widgets-in-homescreen

    //https://developer.android.com/guide/topics/appwidgets

    //https://stackoverflow.com/questions/22829647/programmatically-add-the-widget-to-home-screen-in-android
    //(od Android O, tj. API26
    //https://stackoverflow.com/questions/23196053/set-app-widget-to-home-screen-programmatically

    //https://developer.android.com/reference/android/appwidget/AppWidgetManager#requestPinAppWidget(android.content.ComponentName,%20android.os.Bundle,%20android.app.PendingIntent)



    //https://stackoverflow.com/questions/16100926/how-to-add-a-widget-to-the-android-home-screen-from-my-app

    // podpinanie LiveTile
    // Windows.ApplicationModel.Package.Current.GetAppListEntriesAsync()
    // bool Windows.UI.StartScreen.StartScreenManager.GetDefault().SupportsAppListEntry(oEntry)
    // bool Windows.UI.StartScreen.StartScreenManager.GetDefault().ContainsAppListEntryAsync(oEntry)
    // Windows.UI.StartScreen.StartScreenManager.GetDefault().RequestAddAppListEntryAsync(oEntry)

    // kolejny trigger
    // Windows.ApplicationModel.Background.AppointmentStoreNotificationTrigger
    // ale mozna bez niego, tylko na userpresent

    // LaunchActivatedEventArgs.TileActivatedInfo - ze z Tile
    // e.TileActivatedInfo

    // ze niby jest:
    // taskInstance.GetDeferral()
    // ale nie ma
    // deferral.Complete();

    // samo ustawianie live tile
    // new Windows.UI.Notifications.TileNotification(oXml) <- przechowanie stringu XML? :)
    // Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Update(oTile)


    // sprobowac:
    // 1. zrobic widget, typu licznik co 10 sekund +1
    // 2. odczyt wielkosci current widget, i od tego wybierac 'template' - to wymaga Dom.Xml, wiec dobrze ze to jest
    // 2a. <binding template ='TileMedium' <-- to wybierac 
    // 3. ignorowac group, subgroup
    // 4. <text hint-style='" + p.k.GetSettingsString("sFontSize", "title") + "' hint-align='left'
    // 4a.     //sTmp = "<text hint-style='body'>" & SafeString(sTitle) & "</text>" & vbCrLf &
    //       "<text hint-style='captionSubtle'>" & sTmp & "</text>" & vbCrLf
    // 5. jako separator:  "<text hint-style='caption' hint-align='right'> </text>";
    // 6. <image src='Dni\" + DateTime.Now.Day.ToString("d2") + ".png' placement='background'/>"; - jako image button? jako background?


    public class TileNotification
    {
        public global::Windows.Data.Xml.Dom.XmlDocument Content { get; }
        public TileNotification(global::Windows.Data.Xml.Dom.XmlDocument content)
        {
            Content = content;
        }
    }

    public class TileUpdateManager
    {
        public static TileUpdater CreateTileUpdaterForApplication()
        {
            return new TileUpdater(null);
        }

        // for future implementation
        //public static TileUpdater CreateTileUpdaterForSecondaryTile(string tileId)
        //{
        //    return new TileUpdater(tileId);
        //}
    }

    public class TileUpdater
    {
        private string _tileName;  // null: primary tile; not empty: name of secondary tile
        static internal int _IdTileLayout = 0;  // per Tile..
        static internal int[] _IdTxtLines = new int[10]; // per Tile...
        static internal string _TileTemplate = "";

        public static void AndroidInitIds(int layout, int line0, int line1, int line2, int line3, int line4, int line5, int line6, int line7, int line8, int line9)
        {
            // this must be called from Android head before using LiveTile 
            _IdTileLayout = layout;

            // IDs should be consecutive, but...
            _IdTxtLines[0] = line0;
            _IdTxtLines[1] = line1;
            _IdTxtLines[2] = line2;
            _IdTxtLines[3] = line3;
            _IdTxtLines[4] = line4;
            _IdTxtLines[5] = line5;
            _IdTxtLines[6] = line6;
            _IdTxtLines[7] = line7;
            _IdTxtLines[8] = line8;
            _IdTxtLines[9] = line9;
        }

        internal TileUpdater(string tileName)
        {
            _tileName = tileName;
        }

        public void Update(TileNotification notification)
        {
            var context = Android.App.Application.Context;
            var appWidgetManager = Android.Appwidget.AppWidgetManager.GetInstance(context);
            var component = new Android.Content.ComponentName(context,
                Java.Lang.Class.FromType(typeof(AndroidBroadcastReceiverForUWPLiveTile)).Name);

            int[] views = appWidgetManager.GetAppWidgetIds(component);
            AndroidBroadcastReceiverForUWPLiveTile.UpdateAllIds(context, appWidgetManager, views);
            //appWidgetManager.NotifyAppWidgetViewDataChanged(views, )

            //for(int viewNo = 0; viewNo < views.GetLength(0); viewNo++)
            //{
            //    var remViews = new Android.Widget.RemoteViews(context.PackageName, viewNo);
            //    appWidgetManager.UpdateAppWidget(component, remViews);
            //}
        }
    }

    
    // new Windows.UI.Notifications.TileNotification(oXml) <- przechowanie stringu XML? :)
    // Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Update(oTile)

    [Android.Content.BroadcastReceiver()]  // Name = "UnoLiveTileEmulator"
    [Android.App.IntentFilter(new[] { Android.Appwidget.AppWidgetManager.ActionAppwidgetUpdate , Android.Appwidget.AppWidgetManager.ActionAppwidgetOptionsChanged  })]
    [Android.App.MetaData(Android.Appwidget.AppWidgetManager.MetaDataAppwidgetProvider, Resource = "@xml/livetileinfo") ]
    public class AndroidBroadcastReceiverForUWPLiveTile : Android.Appwidget.AppWidgetProvider
    {

        private static string size2templateName(Android.OS.Bundle options)
        { // get Live Tile template name (small|medium|wide|large) from current widget size
            int minW = options.GetInt(Android.Appwidget.AppWidgetManager.OptionAppwidgetMinWidth);
            int maxW = options.GetInt(Android.Appwidget.AppWidgetManager.OptionAppwidgetMaxWidth);
            int minH = options.GetInt(Android.Appwidget.AppWidgetManager.OptionAppwidgetMinHeight);
            int maxH = options.GetInt(Android.Appwidget.AppWidgetManager.OptionAppwidgetMaxHeight);

            int tileWidth = minW;   // should be == maxW
            int tileHeight = minH;  // should be == maxH
            if ((minW != maxW))
            {
                // shouldn't occur, but use average
                tileWidth = (minW + maxW) / 2;
            }

            if ((minH != maxH))
            {
                // shouldn't occur, but use average
                tileHeight = (minH + maxH) / 2;
            }

            // maybe exchange tileHeight/tileWidth when screen flipped, but at least SquareHome doesn't rotate 

            // maybe conversion between "display points" and "logical points" (using DPI)

            // There are 4 tile sizes: small (71 x 71), medium (150 x 150), wide (310 x 150), and large (310 x 310)

            if (tileHeight < 140)
                return "TileSmall";

            if (tileHeight < 300)
                return "TileMedium";

            if (tileWidth < 300)
                return "TileWide";

            return "TileLarge";
        }

        private static System.Xml.XmlNode EmptyTileTemplate()
        {// default template 
            var xmlLT = new System.Xml.XmlDocument();
            string sXml = "<tile><visual>";
            sXml = sXml + "<binding template ='TileMedium' branding='none'>";
            sXml = sXml + "<text hint-style='caption'>" + Android.App.Application.Context.PackageName + "</text>";
            sXml = sXml + "</binding>";
            sXml = sXml + "</visual></tile>";

            xmlLT.LoadXml(sXml);

            var template = xmlLT.SelectSingleNode("//binding");
            return template;
        }

        private static System.Xml.XmlNode GetLTTemplate(Android.OS.Bundle options)
        {
            string liveTile = BeforeUno.TileUpdater._TileTemplate;
            if(string.IsNullOrEmpty(liveTile))
                return EmptyTileTemplate();

            var xmlLT = new System.Xml.XmlDocument();
            xmlLT.LoadXml(liveTile);

            string templateName = size2templateName(options);
            // <tile><visual><binding template ='TileWide' branding='none'>";'

            // return this, or smaller template
            do
            {
                var template = xmlLT.SelectSingleNode("//binding[@template='" + templateName + "']");
                if (template != null)
                    return template;

                // we don't have template for this size, so try smaller template
                switch (templateName)
                {
                    case "TileMedium":
                        templateName = "TileSmall";
                        break;
                    case "TileWide":
                        templateName = "TileMedium";
                        break;
                    case "TileLarge":
                        templateName = "TileWide";
                        break;
                    default:
                        templateName = "";
                        break;
                }

            } while (templateName != "");

            // we don't have requested template, nor smaller template - try to get bigger template
            templateName = size2templateName(options);
            do
            {
                var template = xmlLT.SelectSingleNode("//binding[@template='" + templateName + "']");
                if (template != null)
                    return template;

                switch (templateName)
                {
                    case "TileSmall":
                        templateName = "TileMedium";
                        break;
                    case "TileMedium":
                        templateName = "TileWide";
                        break;
                    case "TileWide":
                        templateName = "TileLarge";
                        break;
                    default:
                        templateName = "";
                        break;
                }


            } while (templateName != "");


            // cannot find ANY template
            // maybe widget is added, before call to Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Update()
            return EmptyTileTemplate();

        }

        private static int UwpHintStyle2Points(string hintStyle)
        {
            if (string.IsNullOrEmpty(hintStyle))
                return 15;

            hintStyle = hintStyle.Replace("Numeral", "");   // smaller line height
            hintStyle = hintStyle.Replace("Subtle", "");    // opacity 60 %, *TODO* jako color=0x99rrggbb

            switch (hintStyle.ToLower())
            {
                case "caption":
                    return 12;
                case "body":
                    return 15;
                case "base":
                    return 15;
                case "subtitle":
                    return 20;
                case "title":
                    return 24;
                case "subheader":
                    return 34;
                case "header":
                    return 46;
            }
            return 15;

            // caption   12 regular
            // body      15 regular
            // base      15 semibold
            // subtitle  20 regular
            // title     24 semilight
            // subheader 34 light
            // header    46 light
        }

        private static int UwpHintAlign2Gravity(string hintAlign)
        {
            if (string.IsNullOrEmpty(hintAlign))
                return (int)Android.Views.GravityFlags.Left;

            switch (hintAlign.ToLower())
            {
                case "left":
                    return (int)Android.Views.GravityFlags.Left;
                case "center":
                    return (int)Android.Views.GravityFlags.Center;
                case "right":
                    return (int)Android.Views.GravityFlags.Right;
            }
            return (int)Android.Views.GravityFlags.Left;
        }


        internal static void updateWidget(Android.Content.Context context, Android.Appwidget.AppWidgetManager appWidgetManager, int appWidgetId, Android.OS.Bundle options)
        {
            Debug.WriteLine("PKAR updateWidget(x,x,appWidgetId=" + appWidgetId.ToString() + ",x)");

            var liveTileTemplate = GetLTTemplate(options);
            if (liveTileTemplate is null)
                return;

            int layoutId = BeforeUno.TileUpdater._IdTileLayout;
            Debug.WriteLine("PKAR BeforeUno.TileUpdater._IdTileLayout = " + layoutId.ToString());

            if (layoutId == 0 )
            {
                throw new System.NullReferenceException("LiveTile: missing Ids, check if TileUpdater.AndroidInitIds was called from app");
            }
            
            var views = new Android.Widget.RemoteViews(context.PackageName, layoutId);

            int txtLine;

            // iterating texts from template
            var txtNodes = liveTileTemplate.SelectNodes("text");
            for (txtLine = 0; txtLine < txtNodes.Count; txtLine++)
            {
                var txtNode = txtNodes[txtLine];
                XmlAttributeCollection txtAttribs = txtNode.Attributes;

                //views.SetTextViewText(BeforeUno.TileUpdater._IdTxtLines[txtLine], txtNode.InnerText);

                //views.SetViewVisibility(BeforeUno.TileUpdater._IdTxtLines[txtLine], Android.Views.ViewStates.Visible);

                //views.SetTextViewTextSize(BeforeUno.TileUpdater._IdTxtLines[txtLine], (int)Android.Util.ComplexUnitType.Pt,
                //    UwpHintStyle2Points(txtAttribs.GetNamedItem("hint-style")?.InnerText));

                //// hint-align? = "left" | "center" | "right"
                //views.SetInt(BeforeUno.TileUpdater._IdTxtLines[txtLine], "setGravity",
                //    UwpHintAlign2Gravity(txtAttribs.GetNamedItem("hint-align")?.InnerText));

                //string hint;

                //// hint-wrap? = boolean
                //hint = txtAttribs.GetNamedItem("hint-wrap")?.InnerText;
                //if (hint is null)
                //{
                //    hint = "false";
                //}
                //views.SetBoolean(BeforeUno.TileUpdater._IdTxtLines[txtLine], "setSingleLine", (hint.ToLower() == "false"));

                //// line sizes
                //int lineCount = 1;

                //// hint-maxLines? = integer setMaxLines(int)
                //hint = txtAttribs.GetNamedItem("hint-maxLines")?.InnerText;
                //if (hint is null)
                //{
                //    hint = "1";
                //}
                //if (!int.TryParse(hint, out lineCount))
                //{
                //    lineCount = 1;
                //}
                //views.SetInt(BeforeUno.TileUpdater._IdTxtLines[txtLine], "setMaxLines", lineCount);


                //// hint-minLines? = integer setMinLines(int)
                //hint = txtAttribs.GetNamedItem("hint-minLines")?.InnerText;
                //if (hint is null)
                //{
                //    hint = "1";
                //}
                //if (!int.TryParse(hint, out lineCount))
                //{
                //    lineCount = 1;
                //}
                //views.SetInt(BeforeUno.TileUpdater._IdTxtLines[txtLine], "setMinLines", lineCount);

            }
            // and hide all next lines
            //for (; txtLine < 10; txtLine++)
            //{
            //    views.SetViewVisibility(BeforeUno.TileUpdater._IdTxtLines[txtLine], Android.Views.ViewStates.Gone);
            //}

            // Couldn't add widget
            Debug.WriteLine("PKAR BEFORE calling appWidgetManager.UpdateAppWidget");
            //appWidgetManager.UpdateAppWidget(appWidgetId, views);


        }

        public override void OnAppWidgetOptionsChanged(Android.Content.Context context, Android.Appwidget.AppWidgetManager appWidgetManager, int appWidgetId, Android.OS.Bundle newOptions)
        {
            Debug.WriteLine("PKAR OnAppWidgetOptionsChanged, appWidgetId=" + appWidgetId.ToString());
            //updateWidget(context, appWidgetManager, appWidgetId, newOptions);
        }

        internal static void UpdateAllIds(Android.Content.Context context, Android.Appwidget.AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            if (appWidgetIds is null || appWidgetIds.Length < 1)
            {
                Debug.WriteLine("PKAR UpdateAllIds(), but nothing to do!");
                return;
            }

            Debug.WriteLine("PKAR UpdateAllIds(), przed pętlą");
            foreach (int appWidgetId in appWidgetIds)
            {
                Android.OS.Bundle opcje = appWidgetManager.GetAppWidgetOptions(appWidgetId);    // since API 16
                updateWidget(context, appWidgetManager, appWidgetId, opcje);
            }

        }

        public override void OnUpdate(Android.Content.Context context, Android.Appwidget.AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            Debug.WriteLine("PKAR OnUpdate(x,x,appWidgetIds), appWidgetIds.len=" + appWidgetIds.Length.ToString());
            UpdateAllIds(context, appWidgetManager, appWidgetIds);
            Debug.WriteLine("PKAR OnUpdate(), pomiedzy UpdateAllIds a base.OnUpdate");
            base.OnUpdate(context, appWidgetManager, appWidgetIds);
            Debug.WriteLine("PKAR OnUpdate(), po base.OnUpdate");
        }


    }


}
