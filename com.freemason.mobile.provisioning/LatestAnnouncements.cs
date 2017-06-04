
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IdentityModel.OidcClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.freemason.mobile.provisioning
{
    [Activity(Label = "Ultimas Noticias")]
    public class LatestAnnouncements : Activity
    {
        HttpClient _apiClient;
        List<LatestAnnouncementsModel> LatestAnnouncementList { get; set; } 
            = new List<LatestAnnouncementsModel>();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
            SetContentView(Resource.Layout.LatestAnnouncements);

            ISharedPreferences prefs =
                    PreferenceManager.GetDefaultSharedPreferences(this);

            var access_token =
                prefs.GetString("fm_access_token", string.Empty);
            var refresh_token =
                prefs.GetString("fm_refresh_token", string.Empty);

            var listView = FindViewById<ListView>(Resource.Id.lvAnnouncements);

            var baseAddress = "http://services.juliofranciscodeiriarte166.org/";
            var svcEndpoint = 
                "api/v1.0/manage/notification/date/" + 
                DateTime.Now.Date.ToString("yyyy-MM-dd");

            _apiClient = new HttpClient();
            // added since not using result.Handler above
            _apiClient.SetBearerToken(access_token);
            _apiClient.BaseAddress =
                new Uri(baseAddress);

            var result =
                await _apiClient.GetAsync(svcEndpoint);

            //Http client does not throw an exception instead it sets
            //this boolean
            if (!result.IsSuccessStatusCode)
            {
                //Let's try again the call but first let's refresh the token
                /*await RefreshAccessToken(
                    prefs,
                    refresh_token);

                result =
                    await _apiClient.GetAsync(svcEndpoint);

                if (!result.IsSuccessStatusCode)
                {
                    //If fails again, let's set fm_registered to false and
                    //redirect the user back to MainActivity to login again

                    UpdatePreferenceValues(
                        preferences: prefs,
                        access_token: string.Empty,
                        refresh_token: string.Empty,
                        isRegistered: false);

                    StartActivity(typeof(MainActivity));
                }*/
                StartActivity(typeof(MainActivity));
            }
            else
            {
				var response = await
                    result.Content.ReadAsStringAsync();
                //Let's display the data

                var allMessages =
                    JsonConvert
                        .DeserializeObject<List<BroadcastedMessage>>(response);

                foreach(var message in allMessages)
                {
                    LatestAnnouncementList
                        .Add(
                            new LatestAnnouncementsModel(
                                message.Title, 
                                message.Message));
                }

                //Initializing listview
                listView.ItemClick += (sender, e) =>
                {

                };

				listView.Adapter = 
                    new CustomListAdapter(this, LatestAnnouncementList);
            }
        }

        void UpdatePreferenceValues(
            ISharedPreferences preferences,
            string access_token, 
            string refresh_token, 
            bool isRegistered)
        {
			var editor = preferences.Edit();
			editor.PutString("fm_access_token", access_token);
			editor.PutString("fm_refresh_token", refresh_token);
			editor.PutBoolean("fm_registered", isRegistered);
			editor.Apply();
        }

        async Task<string> RefreshAccessToken(
            ISharedPreferences preferences,
            string refresh_token)
        {
			//Let's try refreshing the token
			var options = new OidcClientOptions
			{
				Authority = "https://identity.provider.cotillo-corp.com",
				ClientId = "android-app",
				ClientSecret = "VUdPR5HIlKLe4sVmMe6JbZk8v/JMZC5qy8VY2Chdfrg=",
				Scope = "openid profile api1",
				RedirectUri = "freemason://callback",
				Browser = new BrowserCustomTabsWebView(this)
			};

			var client = new OidcClient(options);

			var token = await client.RefreshTokenAsync(refresh_token);

			//Let's update preference values

			UpdatePreferenceValues(
					preferences: preferences,
					access_token: token.AccessToken,
					refresh_token: token.RefreshToken,
					isRegistered: true);

            //Let's update the Bearer Token
            _apiClient.SetBearerToken(token.AccessToken);

            return token.AccessToken;
        }
    }
}
