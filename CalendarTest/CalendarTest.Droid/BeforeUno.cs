using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;
using System.Linq;

namespace BeforeUno
{

	#region "myDumps"

	class MyDUmps
	{
		static bool _bWasTypes = false;

		public static string DumpTableHeaderNames(Android.Database.ICursor _cursor)
		{
			string sDump = "";
			if (_cursor != null)
			{
				sDump = sDump + "Row count: " + _cursor.Count.ToString() + "\n";
				sDump = sDump + "Column count: " + _cursor.ColumnCount.ToString() + "\n\n";
				for (int i = 0; i < _cursor.ColumnCount; i++)
				{
					sDump = sDump + "|" + _cursor.GetColumnName(i);
				}
				sDump = sDump + "\n";
				//for (int i = 0; i < _cursor.ColumnCount; i++)
				//{
				//	sDump = sDump + _cursor.GetType(i);
				//}
				//sDump = sDump + "\n";
			}
			_bWasTypes = false;
			return sDump;
		}

		public static string DumpTableHeaderTypes(Android.Database.ICursor _cursor)
		{
			string sDump = "";

			if (_cursor.IsBeforeFirst)
				return sDump;   // jeszcze nie mozemy

			for (int i = 0; i < _cursor.ColumnCount; i++)
			{
				switch (_cursor.GetType(i))
				{
					case Android.Database.FieldType.Blob:
						sDump = sDump + "|blob";
						break;
					case Android.Database.FieldType.Float:
						sDump = sDump + "|float";
						break;
					case Android.Database.FieldType.Integer:
						sDump = sDump + "|int";
						break;
					case Android.Database.FieldType.Null:
						sDump = sDump + "|null";
						break;
					case Android.Database.FieldType.String:
						sDump = sDump + "|string";
						break;
					default:
						sDump = sDump + "|UNKNOWN";
						break;
				}
			}
			sDump = sDump + "\n\n";


			_bWasTypes = true;
			return sDump;
		}
		public static string DumpTableRows(Android.Database.ICursor _cursor)
		{
			string sDump = "";

			for (int pageGuard = 100; pageGuard > 0 && _cursor.MoveToNext(); pageGuard--)
			{

				if (!_bWasTypes)
					sDump = DumpTableHeaderTypes(_cursor);

				for (int i = 0; i < _cursor.ColumnCount; i++)
				{
					switch (_cursor.GetType(i))
					{
						case Android.Database.FieldType.Blob:
							sDump = sDump + "|<blob>";
							break;
						case Android.Database.FieldType.Float:
							try
							{
								sDump = sDump + "|" + _cursor.GetFloat(i).ToString();
							}
							catch
							{
								sDump = sDump + "|<error>";
							}
							break;
						case Android.Database.FieldType.Integer:
							try
							{
								sDump = sDump + "|" + _cursor.GetInt(i).ToString();
							}
							catch
							{
								sDump = sDump + "|<error>";
							}

							break;
						case Android.Database.FieldType.Null:
							sDump = sDump + "|null";
							break;
						case Android.Database.FieldType.String:
							try
							{
								sDump = sDump + "|" + _cursor.GetString(i);
							}
							catch
							{
								sDump = sDump + "|<error>";
							}

							break;
						default:
							sDump = sDump + "|UNKNOWN";
							break;
					}
				}
				sDump = sDump + "\n";

			}

			return sDump;

		}

	}
    #endregion


    #region "Calendar"



    // Windows.ApplicationModel.Appointments.AppointmentManager.RequestStoreAsync
    namespace Windows.ApplicationModel.Appointments
	{
		#region "Structs and enums"
		public enum AppointmentStoreAccessType
		{
			AppCalendarsReadWrite,
			AllCalendarsReadOnly,   // UZYWAM
			AllCalendarsReadWrite,
		}

		public partial interface IAppointmentParticipant
		{
			string Address { get; set; }
			string DisplayName { get; set; }
		}

		public partial class AppointmentOrganizer : IAppointmentParticipant
		{
			public string DisplayName;
			public string Address;
			public AppointmentOrganizer()
			{
				DisplayName = "";
				Address = "";
			}

			string IAppointmentParticipant.Address
			{
				get => Address;
				set
				{
					Address = value;
				}
			}
			string IAppointmentParticipant.DisplayName
			{
				get => DisplayName;
				set
				{
					DisplayName = value;
				}
			}

		}


		public partial class Appointment
		{
			// użyte przeze mnie
			public string Location;  // EVENT_LOCATION, string
			public System.TimeSpan Duration; // DURATION, w smiesznym formacie; ale moze trzeba wyliczac z DTSTART/DTEND
			public string Details;  // DESCRIPTION
			public string Subject; // TITLE
			public System.DateTimeOffset StartTime;  // DTSTART, EVENT_TIMEZONE?

			// simple to add
			public bool AllDay;  // ALL_DAY, int
			public string CalendarId; // CALENDAR_ID
			public string LocalId; // chyba _ID - nie widzę tego!
			public AppointmentOrganizer Organizer; // ORGANIZER, string = email organizatora; Win: Address (email), DisplayName (optional), 

			// required additional structs

			// public Windows.ApplicationModel.Appointments.AppointmentRecurrence Recurrence; //RRULE, a Win: Daily/Monthly/MonthlyOnDay/Weekly/Yearly/YearlyOnDay

			// subtables etc.
			// public global::System.TimeSpan? Reminder
			// public global::System.Collections.Generic.IList<global::Windows.ApplicationModel.Appointments.AppointmentInvitee> Invitees
			// public bool HasInvitees

			// Android has no such data
			// public AppointmentBusyStatus BusyStatus; // AVAILABILITY, Win: Busy, Free, OutOfOffice, Tentative, WorkingElsewhere
			// public global::System.Uri Uri
			// public global::Windows.ApplicationModel.Appointments.AppointmentSensitivity Sensitivity
			// public bool AllowNewTimeProposal
			// public global::Windows.ApplicationModel.Appointments.AppointmentParticipantResponse UserResponse
			// public string RoamingId
			// public global::System.DateTimeOffset? ReplyTime
			// public bool IsResponseRequested
			// public bool IsOrganizedByUser
			// public bool IsCanceledMeeting
			// public string OnlineMeetingLink
			// public global::System.DateTimeOffset? OriginalStartTime
			// public ulong RemoteChangeNumber
			// public ulong ChangeNumber
			// public global::Windows.ApplicationModel.Appointments.AppointmentDetailsKind DetailsKind	// HTML / PlainText
			public Appointment()
			{
				Location = "";
				Duration = System.TimeSpan.FromSeconds(0); // this is how UWP initialize this property
				Details = "";
				Subject = "";
				StartTime = System.DateTimeOffset.Now;  // this is how UWP initialize this property
				AllDay = false;
				CalendarId = "";
				LocalId = "";
				Organizer = null;
			}
		}


		public partial class FindAppointmentsOptions
		{
			public uint MaxCount { get; set; }
			public bool IncludeHidden { get; set; }
			public IList<string> CalendarIds { get; }
			public IList<string> FetchProperties { get; }	// UZYWAM
			public FindAppointmentsOptions()
			{
				MaxCount = uint.MaxValue;
				IncludeHidden = false;
				CalendarIds = new List<string>();
				FetchProperties = new List<string>();
			}
		}


		public partial class AppointmentProperties
		{
			public static string AllDay => "Appointment.AllDay";
			public static string Location => "Appointment.Location";
			public static string StartTime => "Appointment.StartTime";
			public static string Duration => "Appointment.Duration";
			public static string Subject => "Appointment.Subject";
			public static string Organizer => "Appointment.Organizer";
			public static string Details => "Appointment.Details";
			public System.Collections.Generic.IList<string> DefaultProperties
			{
				get
				{
					var properties = new System.Collections.Generic.List<string>();
					properties.Add(AllDay);
					properties.Add(Location);
					properties.Add(StartTime);
					properties.Add(Duration);
					properties.Add(Subject);
					return properties;
					// UWP version returns this list:
					// "Appointment.Subject", "Appointment.Location", "Appointment.StartTime", "Appointment.Duration",
					// "Appointment.BusyStatus", "Appointment.AllDay", "Appointment.ParentFolderId", "Appointment.Recurrence"
					// "Appointment.RemoteId", "Appointment.OriginalStartTime", "Appointment.ChangeNumber"
					// but we doesn't implement only some of this
				}

			}

#if false
#region "not implemented Appointment properties"
			public static string HasInvitees
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.HasInvitees is not implemented in Uno.");
				}
			}

			public static string AllowNewTimeProposal
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.AllowNewTimeProposal is not implemented in Uno.");
				}
			}
			public static string BusyStatus
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.BusyStatus is not implemented in Uno.");
				}
			}
			public static string Recurrence
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.Recurrence is not implemented in Uno.");
				}
			}
			public static string Invitees
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.Invitees is not implemented in Uno.");
				}
			}
			public static string IsCanceledMeeting
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.IsCanceledMeeting is not implemented in Uno.");
				}
			}
			public static string IsOrganizedByUser
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.IsOrganizedByUser is not implemented in Uno.");
				}
			}
			public static string IsResponseRequested
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.IsResponseRequested is not implemented in Uno.");
				}
			}

			public static string OnlineMeetingLink
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.OnlineMeetingLink is not implemented in Uno.");
				}
			}
			public static string OriginalStartTime
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.OriginalStartTime is not implemented in Uno.");
				}
			}

			public static string Reminder
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.Reminder is not implemented in Uno.");
				}
			}
			public static string ReplyTime
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.ReplyTime is not implemented in Uno.");
				}
			}
			public static string Sensitivity
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.Sensitivity is not implemented in Uno.");
				}
			}

			public static string Uri
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.Uri is not implemented in Uno.");
				}
			}
			public static string UserResponse
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.UserResponse is not implemented in Uno.");
				}
			}
			public static string DetailsKind
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.DetailsKind is not implemented in Uno.");
				}
			}
			public static string RemoteChangeNumber
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.RemoteChangeNumber is not implemented in Uno.");
				}
			}
			public static string ChangeNumber
			{
				get
				{
					throw new global::System.NotImplementedException("The member string AppointmentProperties.ChangeNumber is not implemented in Uno.");
				}
			}

#endregion
#endif 
        }

#endregion
		public partial class AppointmentManager
		{
			// UZYWAM
			public static IAsyncOperation<AppointmentStore> RequestStoreAsync(AppointmentStoreAccessType options)
				=> RequestStoreAsyncTask(options).AsAsyncOperation<AppointmentStore>();

			public static async Task<AppointmentStore> RequestStoreAsyncTask(AppointmentStoreAccessType options)
			{
				// UWP: AppCalendarsReadWrite, AppCalendarsReadOnly,AppCalendarsReadWrite (cannot be used without special provisioning by Microsoft)
				// Android: Manifest has READ_CALENDAR and WRITE_CALENDAR, no difference between app/limited/full
				// using only AllCalendarsReadOnly, other: throw NotImplementedException

				if (options != AppointmentStoreAccessType.AllCalendarsReadOnly)
					throw new NotImplementedException("AppointmentManager:RequestStoreAsync - only AllCalendarsReadOnly is implemented");

				string[] testPermissions = { Android.Manifest.Permission.ReadCalendar };
				var missingPermissions = global::Windows.Extensions.PermissionsHelper.MissingPermissions(testPermissions, null);
				if (missingPermissions is null)
				{
					throw new InvalidOperationException("AppointmentManager:RequestStoreAsync - no ReadCalendar permission declared in Manifest");
				}

				bool granted = await global::Windows.Extensions.PermissionsHelper.AndroidPermissionAsync(testPermissions, null);
				if (!granted)
				{
					return null;
				}
				else
				{
					return new AppointmentStore();
				}

			}

			public static IAsyncAction ShowTimeFrameAsync(DateTimeOffset timeToShow, TimeSpan duration)
				=> ShowTimeFrameAsyncTask(timeToShow, duration).AsAsyncAction();
			private static async Task ShowTimeFrameAsyncTask(DateTimeOffset timeToShow, TimeSpan duration)
			{
				// https://stackoverflow.com/questions/4373074/how-to-launch-android-calendar-application-using-intent-froyo
				//  <uses-permission android:name="android.permission.READ_CALENDAR" />
				Android.Net.Uri.Builder builder = Android.Provider.CalendarContract.ContentUri.BuildUpon();
				builder.AppendPath("time");
				Android.Content.ContentUris.AppendId(builder, timeToShow.ToUniversalTime().ToUnixTimeMilliseconds());
				var intent = new Android.Content.Intent(Android.Content.Intent.ActionView).SetData(builder.Build());
				Android.App.Application.Context.StartActivity(intent);
			}


#if false
			#region "nieuzywam"
			public static global::Windows.ApplicationModel.Appointments.AppointmentManagerForUser GetForUser(global::Windows.System.User user)
			{
				throw new global::System.NotImplementedException("The member AppointmentManagerForUser AppointmentManager.GetForUser(User user) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncAction ShowAppointmentDetailsAsync(string appointmentId)
			{
				throw new global::System.NotImplementedException("The member IAsyncAction AppointmentManager.ShowAppointmentDetailsAsync(string appointmentId) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncAction ShowAppointmentDetailsAsync(string appointmentId, global::System.DateTimeOffset instanceStartDate)
			{
				throw new global::System.NotImplementedException("The member IAsyncAction AppointmentManager.ShowAppointmentDetailsAsync(string appointmentId, DateTimeOffset instanceStartDate) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncOperation<string> ShowEditNewAppointmentAsync(global::Windows.ApplicationModel.Appointments.Appointment appointment)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentManager.ShowEditNewAppointmentAsync(Appointment appointment) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncOperation<string> ShowAddAppointmentAsync(global::Windows.ApplicationModel.Appointments.Appointment appointment, global::Windows.Foundation.Rect selection)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentManager.ShowAddAppointmentAsync(Appointment appointment, Rect selection) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncOperation<string> ShowAddAppointmentAsync(global::Windows.ApplicationModel.Appointments.Appointment appointment, global::Windows.Foundation.Rect selection, global::Windows.UI.Popups.Placement preferredPlacement)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentManager.ShowAddAppointmentAsync(Appointment appointment, Rect selection, Placement preferredPlacement) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncOperation<string> ShowReplaceAppointmentAsync(string appointmentId, global::Windows.ApplicationModel.Appointments.Appointment appointment, global::Windows.Foundation.Rect selection)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentManager.ShowReplaceAppointmentAsync(string appointmentId, Appointment appointment, Rect selection) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncOperation<string> ShowReplaceAppointmentAsync(string appointmentId, global::Windows.ApplicationModel.Appointments.Appointment appointment, global::Windows.Foundation.Rect selection, global::Windows.UI.Popups.Placement preferredPlacement)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentManager.ShowReplaceAppointmentAsync(string appointmentId, Appointment appointment, Rect selection, Placement preferredPlacement) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncOperation<string> ShowReplaceAppointmentAsync(string appointmentId, global::Windows.ApplicationModel.Appointments.Appointment appointment, global::Windows.Foundation.Rect selection, global::Windows.UI.Popups.Placement preferredPlacement, global::System.DateTimeOffset instanceStartDate)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentManager.ShowReplaceAppointmentAsync(string appointmentId, Appointment appointment, Rect selection, Placement preferredPlacement, DateTimeOffset instanceStartDate) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncOperation<bool> ShowRemoveAppointmentAsync(string appointmentId, global::Windows.Foundation.Rect selection)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<bool> AppointmentManager.ShowRemoveAppointmentAsync(string appointmentId, Rect selection) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncOperation<bool> ShowRemoveAppointmentAsync(string appointmentId, global::Windows.Foundation.Rect selection, global::Windows.UI.Popups.Placement preferredPlacement)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<bool> AppointmentManager.ShowRemoveAppointmentAsync(string appointmentId, Rect selection, Placement preferredPlacement) is not implemented in Uno.");
			}
			public static global::Windows.Foundation.IAsyncOperation<bool> ShowRemoveAppointmentAsync(string appointmentId, global::Windows.Foundation.Rect selection, global::Windows.UI.Popups.Placement preferredPlacement, global::System.DateTimeOffset instanceStartDate)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<bool> AppointmentManager.ShowRemoveAppointmentAsync(string appointmentId, Rect selection, Placement preferredPlacement, DateTimeOffset instanceStartDate) is not implemented in Uno.");
			}
			#endregion
#endif
		}

		public partial class AppointmentStore
		{
			private bool _StartTimeRequested = false; // if it should be included in UWP output result set
			private bool _DurationRequested = false; // set if duration should be included in UWP output result

			private List<string> UWP2AndroColumnNames(IList<string> uwpColumns)
			{
				_StartTimeRequested = false;

				List<string> androColumns = new List<string>();
				foreach (var column in uwpColumns)
				{
					switch (column)
					{
						case "Appointment.AllDay":
							androColumns.Add(Android.Provider.CalendarContract.EventsColumns.AllDay);
							break;
						case "Appointment.Location":
							androColumns.Add(Android.Provider.CalendarContract.EventsColumns.EventLocation);
							break;
						case "Appointment.StartTime":
							_StartTimeRequested = true;
							break;
						case "Appointment.Duration":
							_DurationRequested = true;
							break;
						case "Appointment.Subject":
							androColumns.Add(Android.Provider.CalendarContract.EventsColumns.Title);
							break;
						case "Appointment.Organizer":
							androColumns.Add(Android.Provider.CalendarContract.EventsColumns.Organizer);
							break;
						case "Appointment.Details":
							androColumns.Add(Android.Provider.CalendarContract.EventsColumns.Description);
							break;
					}
				}
				return androColumns;
			}


			private TimeSpan CalculateDuration(string duration, long startTime, long endtime)
			{
				// duration maybe "" - no data
				// startTime is always present
				// endtime can be 0 - no data

				// scenario 1: we have duration
				if(!string.IsNullOrEmpty(duration))
				{
					// P1800S, P1D...
					// doc says that this is RFC2445 format https://tools.ietf.org/html/rfc2445#section-4.3.6
					// e.g. "P10H 5S...", letter: Weeks, Day, Hour, Minutes, Seconds
					// but I did not encounter such in my calendars, only S and D
					if (duration.StartsWith("P"))
					{
						int multiplier = 1;
						if (duration.EndsWith("D"))
						{
							multiplier = 60 * 60 * 24;  // seconds in day
						}
						if (duration.EndsWith("H"))
						{
							multiplier = 60 * 60;  // seconds in hour
						}
						if (duration.EndsWith("M"))
						{
							multiplier = 60;  // seconds in minute
						}
						int seconds = 0;
						if (int.TryParse(duration.Substring(1, duration.Length - 2), out seconds))
						{
							return TimeSpan.FromSeconds(seconds * multiplier);
						}
					}
					// no "P...", or error parsing integer: we try to use second scenario
				}

				// scenario 2: we have start and end
				if (endtime>0)
				{
					return TimeSpan.FromMilliseconds(endtime - startTime);
				}

				// else: we cannot compute duration
				return TimeSpan.FromSeconds(0);
			}

			// USED
			public IAsyncOperation<IReadOnlyList<Appointment>> FindAppointmentsAsync(DateTimeOffset rangeStart, TimeSpan rangeLength, FindAppointmentsOptions options)
				=> FindAppointmentsAsyncTask(rangeStart, rangeLength, options).AsAsyncOperation<IReadOnlyList<Appointment>>();
			private async Task<IReadOnlyList<Appointment>> FindAppointmentsAsyncTask(DateTimeOffset rangeStart, TimeSpan rangeLength, FindAppointmentsOptions options)
			{
				Android.Database.ICursor _cursor = null;
				Android.Content.ContentResolver _contentResolver = null;
				List<Appointment> entriesList = new List<Appointment>();

				if (options is null)
				{
					throw new ArgumentNullException();
				}

				Android.Net.Uri.Builder builder = Android.Provider.CalendarContract.Instances.ContentUri.BuildUpon();
				Android.Content.ContentUris.AppendId(builder, rangeStart.ToUniversalTime().ToUnixTimeMilliseconds());
				var rangeEnd = rangeStart + rangeLength;
				Android.Content.ContentUris.AppendId(builder, rangeEnd.ToUniversalTime().ToUnixTimeMilliseconds());
				Android.Net.Uri oUri = builder.Build();
				// it is simply: {content://com.android.calendar/instances/when/1588275364371/1588880164371}

				var androColumns = UWP2AndroColumnNames(options.FetchProperties);
				// some 'system columns' columns, cannot be switched off
				androColumns.Add(Android.Provider.CalendarContract.EventsColumns.CalendarId);
				androColumns.Add("_id");
				//androColumns.Add(Android.Provider.CalendarContract.EventsColumns.Dtstart);	// for sort
				//androColumns.Add(Android.Provider.CalendarContract.EventsColumns.Dtend);    // we need this, as Android sometimes has NULL duration, and it should be reconstructed from start/end
				androColumns.Add(Android.Provider.CalendarContract.Instances.Begin);	// for sort
				androColumns.Add(Android.Provider.CalendarContract.Instances.End);    // we need this, as Android sometimes has NULL duration, and it should be reconstructed from start/end

				// string sortMode = Android.Provider.CalendarContract.EventsColumns.Dtstart + " ASC";
				string sortMode = Android.Provider.CalendarContract.Instances.Begin + " ASC";
				_contentResolver = Android.App.Application.Context.ContentResolver;
				_cursor = _contentResolver.Query(oUri,
										androColumns.ToArray(),  // columns in result
										null,   // where
										null,   // where params
										sortMode); 

				if (_cursor is null || _cursor.IsAfterLast)
				{
					return entriesList;
				}

				string sTmp = MyDUmps.DumpTableHeaderNames(_cursor);
				System.Diagnostics.Debug.Write(sTmp);
				string sTmp1 = MyDUmps.DumpTableRows(_cursor);
				System.Diagnostics.Debug.Write(sTmp1);

				if (!_cursor.MoveToFirst())
				{
					return entriesList;
				}

				// optimization
				int _colAllDay, _colLocation, _colStartTime, _colDuration, _colSubject, _colOrganizer, _colDetails, _colCalId, _colId, _colEndTime;
				_colAllDay = _cursor.GetColumnIndex(Android.Provider.CalendarContract.EventsColumns.AllDay);
				_colLocation = _cursor.GetColumnIndex(Android.Provider.CalendarContract.EventsColumns.EventLocation);
				_colStartTime= _cursor.GetColumnIndex(Android.Provider.CalendarContract.Instances.Begin);
				_colEndTime = _cursor.GetColumnIndex(Android.Provider.CalendarContract.Instances.End);
				//_colDuration = _cursor.GetColumnIndex(Android.Provider.CalendarContract.EventsColumns.Duration);
				_colSubject= _cursor.GetColumnIndex(Android.Provider.CalendarContract.EventsColumns.Title);
				_colOrganizer= _cursor.GetColumnIndex(Android.Provider.CalendarContract.EventsColumns.Organizer);
				_colDetails= _cursor.GetColumnIndex(Android.Provider.CalendarContract.EventsColumns.Description);
				_colCalId = _cursor.GetColumnIndex(Android.Provider.CalendarContract.EventsColumns.CalendarId);
				 _colId = _cursor.GetColumnIndex("_id");


				// odczytanie

				for (uint pageGuard = options.MaxCount; pageGuard > 0; pageGuard--)
				{
					var entry = new Appointment();

					// two properties always present in result
					entry.CalendarId = _cursor.GetString(_colCalId);
					entry.LocalId = _cursor.GetString(_colId);

					// rest of properties can be switched off (absent in result set)
					if (_colAllDay > -1)
					{
						entry.AllDay = (_cursor.GetInt(_colAllDay) == 1);
					}
					if (_colDetails > -1)
					{
						entry.Details = _cursor.GetString(_colDetails);
					}
					if (_colLocation > -1)
					{
						entry.Location = _cursor.GetString(_colLocation);
					}

					if (_StartTimeRequested)
					{
						entry.StartTime = DateTimeOffset.FromUnixTimeMilliseconds(_cursor.GetLong(_colStartTime));
					}

					if (_DurationRequested)
					{
						// calculate duration from start/end, as Android Duration field sometimes is null, and is in hard to parse RFC2445 format 
						long startTime, endTime;
						startTime = _cursor.GetLong(_colStartTime);
						endTime = _cursor.GetLong(_colEndTime);
						entry.Duration = TimeSpan.FromMilliseconds(endTime - startTime);
					}


					if (_colSubject > -1)
					{
						entry.Subject = _cursor.GetString(_colSubject);
					}

					if (_colOrganizer > -1)
					{
						var organ = new AppointmentOrganizer();
						organ.Address = _cursor.GetString(_colOrganizer);
						entry.Organizer = organ;
					}

					entriesList.Add(entry);

					if (!_cursor.MoveToNext())
					{
						break;
					}
				}

				_cursor.Close();

				return entriesList;

			}


#if false
			#region "nieuzywam"
			public global::Windows.ApplicationModel.Appointments.AppointmentStoreChangeTracker ChangeTracker
			{
				get
				{
					throw new global::System.NotImplementedException("The member AppointmentStoreChangeTracker AppointmentStore.ChangeTracker is not implemented in Uno.");
				}
			}
			public global::Windows.Foundation.IAsyncOperation<global::Windows.ApplicationModel.Appointments.AppointmentCalendar> CreateAppointmentCalendarAsync(string name)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<AppointmentCalendar> AppointmentStore.CreateAppointmentCalendarAsync(string name) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<global::Windows.ApplicationModel.Appointments.AppointmentCalendar> GetAppointmentCalendarAsync(string calendarId)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<AppointmentCalendar> AppointmentStore.GetAppointmentCalendarAsync(string calendarId) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<global::Windows.ApplicationModel.Appointments.Appointment> GetAppointmentAsync(string localId)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<Appointment> AppointmentStore.GetAppointmentAsync(string localId) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<global::Windows.ApplicationModel.Appointments.Appointment> GetAppointmentInstanceAsync(string localId, global::System.DateTimeOffset instanceStartTime)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<Appointment> AppointmentStore.GetAppointmentInstanceAsync(string localId, DateTimeOffset instanceStartTime) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<global::System.Collections.Generic.IReadOnlyList<global::Windows.ApplicationModel.Appointments.AppointmentCalendar>> FindAppointmentCalendarsAsync()
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<IReadOnlyList<AppointmentCalendar>> AppointmentStore.FindAppointmentCalendarsAsync() is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<global::System.Collections.Generic.IReadOnlyList<global::Windows.ApplicationModel.Appointments.AppointmentCalendar>> FindAppointmentCalendarsAsync(global::Windows.ApplicationModel.Appointments.FindAppointmentCalendarsOptions options)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<IReadOnlyList<AppointmentCalendar>> AppointmentStore.FindAppointmentCalendarsAsync(FindAppointmentCalendarsOptions options) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<global::Windows.ApplicationModel.Appointments.AppointmentConflictResult> FindConflictAsync(global::Windows.ApplicationModel.Appointments.Appointment appointment)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<AppointmentConflictResult> AppointmentStore.FindConflictAsync(Appointment appointment) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<global::Windows.ApplicationModel.Appointments.AppointmentConflictResult> FindConflictAsync(global::Windows.ApplicationModel.Appointments.Appointment appointment, global::System.DateTimeOffset instanceStartTime)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<AppointmentConflictResult> AppointmentStore.FindConflictAsync(Appointment appointment, DateTimeOffset instanceStartTime) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncAction MoveAppointmentAsync(global::Windows.ApplicationModel.Appointments.Appointment appointment, global::Windows.ApplicationModel.Appointments.AppointmentCalendar destinationCalendar)
			{
				throw new global::System.NotImplementedException("The member IAsyncAction AppointmentStore.MoveAppointmentAsync(Appointment appointment, AppointmentCalendar destinationCalendar) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<string> ShowAddAppointmentAsync(global::Windows.ApplicationModel.Appointments.Appointment appointment, global::Windows.Foundation.Rect selection)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentStore.ShowAddAppointmentAsync(Appointment appointment, Rect selection) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<string> ShowReplaceAppointmentAsync(string localId, global::Windows.ApplicationModel.Appointments.Appointment appointment, global::Windows.Foundation.Rect selection)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentStore.ShowReplaceAppointmentAsync(string localId, Appointment appointment, Rect selection) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<string> ShowReplaceAppointmentAsync(string localId, global::Windows.ApplicationModel.Appointments.Appointment appointment, global::Windows.Foundation.Rect selection, global::Windows.UI.Popups.Placement preferredPlacement, global::System.DateTimeOffset instanceStartDate)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentStore.ShowReplaceAppointmentAsync(string localId, Appointment appointment, Rect selection, Placement preferredPlacement, DateTimeOffset instanceStartDate) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<bool> ShowRemoveAppointmentAsync(string localId, global::Windows.Foundation.Rect selection)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<bool> AppointmentStore.ShowRemoveAppointmentAsync(string localId, Rect selection) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<bool> ShowRemoveAppointmentAsync(string localId, global::Windows.Foundation.Rect selection, global::Windows.UI.Popups.Placement preferredPlacement, global::System.DateTimeOffset instanceStartDate)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<bool> AppointmentStore.ShowRemoveAppointmentAsync(string localId, Rect selection, Placement preferredPlacement, DateTimeOffset instanceStartDate) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncAction ShowAppointmentDetailsAsync(string localId)
			{
				throw new global::System.NotImplementedException("The member IAsyncAction AppointmentStore.ShowAppointmentDetailsAsync(string localId) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncAction ShowAppointmentDetailsAsync(string localId, global::System.DateTimeOffset instanceStartDate)
			{
				throw new global::System.NotImplementedException("The member IAsyncAction AppointmentStore.ShowAppointmentDetailsAsync(string localId, DateTimeOffset instanceStartDate) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<string> ShowEditNewAppointmentAsync(global::Windows.ApplicationModel.Appointments.Appointment appointment)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<string> AppointmentStore.ShowEditNewAppointmentAsync(Appointment appointment) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<global::System.Collections.Generic.IReadOnlyList<string>> FindLocalIdsFromRoamingIdAsync(string roamingId)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<IReadOnlyList<string>> AppointmentStore.FindLocalIdsFromRoamingIdAsync(string roamingId) is not implemented in Uno.");
			}
			public global::Windows.Foundation.IAsyncOperation<global::Windows.ApplicationModel.Appointments.AppointmentCalendar> CreateAppointmentCalendarAsync(string name, string userDataAccountId)
			{
				throw new global::System.NotImplementedException("The member IAsyncOperation<AppointmentCalendar> AppointmentStore.CreateAppointmentCalendarAsync(string name, string userDataAccountId) is not implemented in Uno.");
			}
			public global::Windows.ApplicationModel.Appointments.AppointmentStoreChangeTracker GetChangeTracker(string identity)
			{
				throw new global::System.NotImplementedException("The member AppointmentStoreChangeTracker AppointmentStore.GetChangeTracker(string identity) is not implemented in Uno.");
			}

			public IAsyncOperation<IReadOnlyList<Appointment>> FindAppointmentsAsync(DateTimeOffset rangeStart, TimeSpan rangeLength)
			{
				throw new global::System.NotImplementedException("The member FindAppointmentsAsync(DateTimeOffset rangeStart, TimeSpan rangeLength) is not implemented in Uno.");
			}

			public event global::Windows.Foundation.TypedEventHandler<global::Windows.ApplicationModel.Appointments.AppointmentStore, global::Windows.ApplicationModel.Appointments.AppointmentStoreChangedEventArgs> StoreChanged
			{
				[global::Uno.NotImplemented]
				add
				{
					global::Windows.Foundation.Metadata.ApiInformation.TryRaiseNotImplemented("Windows.ApplicationModel.Appointments.AppointmentStore", "event TypedEventHandler<AppointmentStore, AppointmentStoreChangedEventArgs> AppointmentStore.StoreChanged");
				}
				[global::Uno.NotImplemented]
				remove
				{
					global::Windows.Foundation.Metadata.ApiInformation.TryRaiseNotImplemented("Windows.ApplicationModel.Appointments.AppointmentStore", "event TypedEventHandler<AppointmentStore, AppointmentStoreChangedEventArgs> AppointmentStore.StoreChanged");
				}
			}

			#endregion
#endif
		}
    }




    // https://developer.android.com/guide/topics/providers/calendar-provider
    // <uses-permission android:name="android.permission.READ_CALENDAR" />



#endregion
}
