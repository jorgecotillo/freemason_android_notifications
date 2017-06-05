using System;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Gms.Gcm;
using Android.Gms.Gcm.Iid;
using WindowsAzure.Messaging;
using Android.Preferences;

namespace com.freemason.mobile.provisioning
{
	[Service(Exported = false)]
	class RegistrationIntentService : IntentService
	{
		static object locker = new object();

		public RegistrationIntentService() : base("RegistrationIntentService") { }

		protected override void OnHandleIntent(Intent intent)
		{
			try
			{
				Log.Info("RegistrationIntentService", "Calling InstanceID.GetToken");
				lock (locker)
				{
					var instanceID = InstanceID.GetInstance(this);
					var token = instanceID.GetToken(
                        Constants.SenderID, GoogleCloudMessaging.InstanceIdScope, null);

					Log.Info("RegistrationIntentService", "GCM Registration Token: " + token);
					SendRegistrationToAppServer(token);
					Subscribe(token);
                    RegisterNotificationHub(token);
				}
			}
			catch 
            {
				Log.Debug("RegistrationIntentService", "Failed to get a registration token");
				return;
			}
		}

		void SendRegistrationToAppServer(string token)
		{
			// Add custom implementation here as needed.
		}

		void Subscribe(string token)
		{
			var pubSub = GcmPubSub.GetInstance(this);
			pubSub.Subscribe(token, "/topics/global", null);
		}

        void RegisterNotificationHub(string token)
        {
            ISharedPreferences prefs =
                    PreferenceManager.GetDefaultSharedPreferences(this);

            string regID = string.Empty;
            string storedToken = string.Empty;

            if (((regID = prefs.GetString("registrationID", null)) == null))
            {
                regID = RegisterHub(token, prefs);
                Log.Info("RegisterNotificationHub", $"Successfull Registration- RegId = {regID}");
            }
            else if ((storedToken = prefs.GetString("gcmToken", string.Empty)) != token)
            {
                regID = RegisterHub(token, prefs);
                Log.Info("RegisterNotificationHub", $"Successfull Registration - RegId = {regID}");
            }
            else
            {
                Log.Info("RegisterNotificationHub", $"Previously Registered - RegId = {regID}");
            }
        }

        string RegisterHub(string token, ISharedPreferences prefs)
        {
			NotificationHub hub =
				new NotificationHub(
					Constants.NotificationHubPath,
					Constants.ConnectionString,
					this);

			string regID = hub.Register(token, "JFI166").RegistrationId;

			ISharedPreferencesEditor editor = prefs.Edit();
			editor.PutString("registrationID", regID);
			editor.PutString("gcmToken", token);
			editor.Apply();

            return regID;
        }

	}
}
