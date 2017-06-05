﻿using System.Net.Http;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Android.Widget;
using IdentityModel.Client;
using IdentityModel.OidcClient;

namespace com.freemason.mobile.provisioning
{
    [Activity(Label = "Freemason Notifications", 
              LaunchMode = Android.Content.PM.LaunchMode.SingleTask,
              MainLauncher = true,
              Icon = "@mipmap/masonic_symbol")]
    public class MainActivity : Activity
    {
        ProgressBar _progressBar;
        TextView _textLoading;
        TextView _textSuccess;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _progressBar = FindViewById<ProgressBar>(Resource.Id.pbLoading);
            _textLoading = FindViewById<TextView>(Resource.Id.tvLoading);
            _textSuccess = FindViewById<TextView>(Resource.Id.tvSuccessMsg);

            DisplayOrHideSuccessControl(display: false);

            /*buttonClose.Click += (sender, e) => 
            {
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            };*/

            ISharedPreferences prefs =
                    PreferenceManager.GetDefaultSharedPreferences(this);

            // Register with the Google Cloud Service
            RegisterWithGCM();

            _progressBar.Indeterminate = true;

            var options = new OidcClientOptions
            {
                Authority = "https://identity.provider.cotillo-corp.com",
                ClientId = "android-app",
                ClientSecret = "VUdPR5HIlKLe4sVmMe6JbZk8v/JMZC5qy8VY2Chdfrg=",
                Scope = "openid profile api1",
                RedirectUri = "freemason://callback",
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                //Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                //Policy = new Policy() { Discovery = new IdentityModel.Client.DiscoveryPolicy() { RequireHttps = false }},
                Browser = new BrowserCustomTabsWebView(this)
            };

            var client = new OidcClient(options);

            var result = await client.LoginAsync();

            if (!string.IsNullOrEmpty(result.Error))
            {
                return;
            }
            else
            {
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutString("fm_access_token", result.AccessToken);
                //TODO: There is no refresh token, have to verify the reason.
                editor.PutString("fm_refresh_token", result.RefreshToken);
                editor.Apply();

                _progressBar.Indeterminate = false;

                DisplayOrHideSuccessControl(display: true);

                LaunchLatestAnnouncements();
            }
        }

		/// <summary>
		/// Registers the with GCM.
		/// </summary>
		void RegisterWithGCM()
		{
			if (IsPlayServicesAvailable())
			{
				var intent = new Intent(this, typeof(RegistrationIntentService));
				StartService(intent);
			}
		}

		bool IsPlayServicesAvailable()
		{
			int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
			if (resultCode != ConnectionResult.Success)
			{
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    Log.Info("IsPlayServicesAvailable", GoogleApiAvailability.Instance.GetErrorString(resultCode));
                }
				else
				{
                    Log.Info("IsPlayServicesAvailable", "Sorry, this device is not supported");
					Finish();
				}
				return false;
			}
			else
			{
                Log.Info("IsPlayServicesAvailable", "Google Play Services is available.");
				return true;
			}
		}

        void DisplayOrHideSuccessControl(bool display)
        {
            if(!display)
            {
				_textSuccess.Visibility = Android.Views.ViewStates.Gone;
				_progressBar.Visibility = Android.Views.ViewStates.Visible;
				_textLoading.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {
				_progressBar.Visibility = Android.Views.ViewStates.Gone;
				_textLoading.Visibility = Android.Views.ViewStates.Gone;
				_textSuccess.Visibility = Android.Views.ViewStates.Visible;
            }
        }

        void LaunchLatestAnnouncements()
        {
            StartActivity(typeof(LatestAnnouncements));
        }
    }
}

