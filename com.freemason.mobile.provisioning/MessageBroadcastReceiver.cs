using System;
using System.Diagnostics;
using System.Text;
using WindowsAzure.Messaging;
using Android.App;
using Android.Content;
using Android.Util;
using Gcm.Client;
using System.Threading.Tasks;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

//GET_ACCOUNTS is only needed for android versions 4.0.3 and below
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]

namespace com.freemason.mobile.provisioning
{
	[BroadcastReceiver(Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
	[IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE },
		Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK },
		Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY },
		Categories = new string[] { "@PACKAGE_NAME@" })]
	public class MessageBroadcastReceiver : 
        GcmBroadcastReceiverBase<PushHandlerService>
	{
		public static string[] SENDER_IDS = new string[] { Constants.SenderID };

		public const string TAG = "Freemason-JulioFcoDeIriarte166";
	}


	/// <summary>
	/// Registers for notifications
	/// </summary>
	/// <remarks>
	/// Must use the service tag
	/// </remarks>
	[Service]
	public class PushHandlerService : GcmServiceBase
	{
		public static string RegistrationID { get; private set; }

		private NotificationHub Hub { get; set; }

		// Transforming Template (Into a data notification)
		// The $(notificationMessage) maps in the date from the generic template notification's 
		// notificationMessage value.
		public const string GoogleTemplateMessage = 
            @"{ ""data"" : {""message"":""$(notificationMessage)""}}";

		/// <summary>
		/// Initializes a new instance of the <see cref="PushHandlerService"/> class.
		/// </summary>
		public PushHandlerService()
			: base(Constants.SenderID)
		{
			Log
                .Info(
                    MessageBroadcastReceiver.TAG, 
                    "PushHandlerService() constructor");
		}

		/// <summary>
		/// Called to register for notifications
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="registrationId">The registration identifier.</param>
		protected override void OnRegistered(
            Context context, 
            string registrationId)
		{
			Log
                .Verbose(
                    MessageBroadcastReceiver.TAG, 
                    "GCM Registered: " + registrationId);
            
			RegistrationID = registrationId;

			CreateNotification(
                title: "Info de Registro...", 
                desc: "El dispostivo ha sido registrado satisfactoriamente!");

			Hub = 
                new NotificationHub(
                    Constants.NotificationHubPath, 
                    Constants.ConnectionString, 
                    this);
			try
			{
				Hub.UnregisterAll(registrationId);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			//Register for native messages
			try
			{
				var nativeHubRegistration = 
                    Hub.Register(
                        pnsHandle: registrationId, 
                        tags: "GoogleApp");
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			// Register for template messages
			try
			{
				var templateHubRegistration = 
                    Hub.RegisterTemplate(
                        pnsHandle: registrationId, 
                        templateName: "data", 
                        template: GoogleTemplateMessage, 
                        tags: MessageBroadcastReceiver.TAG);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			CreateNotification(
                title: "Notification de Subscripcion...", 
                desc: "Completado!");
		}


		/// <summary>
		/// Called when a message is received
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="intent">The intent.</param>
		protected override void OnMessage(Context context, Intent intent)
		{
			Log.Info(MessageBroadcastReceiver.TAG, "GCM Message Received!");

			var msg = new StringBuilder();

			if (intent != null && intent.Extras != null)
			{
                foreach (var key in intent.Extras.KeySet())
                {
                    msg
                        .AppendLine(
                            $"{key}={intent.Extras.Get(key).ToString()}");
                }
			}

			string messageText = intent.Extras.GetString("message");
			if (!string.IsNullOrEmpty(messageText))
			{
				CreateNotification("Nuevo Mensaje!", messageText);
				return;
			}

			CreateNotification("Mensaje desconocido", msg.ToString());
		}

		/// <summary>
		/// Creates the notification and displays it
		/// </summary>
		/// <param name="title">The title.</param>
		/// <param name="desc">The desc.</param>
		void CreateNotification(string title, string desc)
		{
			//Create notification
			var notificationManager = 
                GetSystemService(NotificationService) as NotificationManager;

			//Create an intent to show ui
			var uiIntent = new Intent(this, typeof(MainActivity));

            //Create the notification
            var notification =
                new Notification(Resource.Drawable.masonic_symbol, title)
                {

                    //Auto cancel will remove the notification once the user touches it
                    Flags = NotificationFlags.AutoCancel
                };

            //Set the notification info
            //we use the pending intent, passing our ui intent over which will get called
            //when the notification is tapped.
            notification
                .SetLatestEventInfo(
                    this, 
                    title, 
                    desc, 
                    PendingIntent.GetActivity(this, 0, uiIntent, 0));

			//Show the notification
			notificationManager.Notify(1, notification);
		}

		/// <summary>
		/// Called when [un registered].
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="registrationId">The registration identifier.</param>
		protected override void OnUnRegistered(
            Context context, 
            string registrationId)
		{
			CreateNotification(
                title: "GcmService Unregistered!", 
                desc: "Device has been unregistered");
		}

		/// <summary>
		/// Called when [recoverable error].
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="errorId">The error identifier.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		protected override bool OnRecoverableError(
            Context context, 
            string errorId)
		{
			Log
                .Warn(
                    MessageBroadcastReceiver.TAG, 
                    "Recoverable Error: " + errorId);
            
			return base.OnRecoverableError(context, errorId);
		}

		/// <summary>
		/// Called when [error].
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="errorId">The error identifier.</param>
		protected override void OnError(Context context, string errorId)
		{
			Log.Error(MessageBroadcastReceiver.TAG, "GCM Error: " + errorId);
		}
	}


}