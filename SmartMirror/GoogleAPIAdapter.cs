using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Core;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace SmartMirror
{
    class GoogleAPIAdapter
    {
        private const string googleMapsKey = "AIzaSyCf7aTd-ZPp6eEKeLNRIKgKPOj0oBMCd2M";
        private static String client_id = "227708707917-giuqblflgspittrvo1hgn411b0urbbg7.apps.googleusercontent.com";
        private static String client_secret = "V9vfSYUkkhqWgZR_fh-IfQaR";
        private static String scope = "https://www.googleapis.com/auth/calendar.readonly";
        private static String redirect_uri = "https://github.com/aWilson41/Smart-Mirror";
        private static string[] formats = new string[] { "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.ffK", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.ffZ" };

        public GoogleAPIAdapter()
        {
        }

        public static async void authorize(String code)
        {
            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            Uri requestUri = new Uri("https://www.googleapis.com/oauth2/v4/token");
            HttpResponseMessage response = new HttpResponseMessage();
            HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new Dictionary<String, String> {
                    { "client_id", client_id },
                    { "client_secret", client_secret },
                    { "code", code },
                    { "scope", scope },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", redirect_uri }
                });
            string body = "";

            try
            {
                response = await httpClient.PostAsync(requestUri, content);
                body = await response.Content.ReadAsStringAsync();

                JsonObject obj = JsonValue.Parse(body).GetObject();
                String accessToken = obj.GetNamedString("access_token");
                String refreshToken = obj.GetNamedString("refresh_token");
                int expiresIn = (int)obj.GetNamedNumber("expires_in");

                UserAccount.saveSetting("googleAccessToken", accessToken);
                UserAccount.saveSetting("googleRefreshToken", refreshToken);
                UserAccount.saveSetting("googleFreshness", DateTime.Now.Ticks.ToString());
                UserAccount.saveSetting("googleExpires", expiresIn.ToString());
                Debug.WriteLine(accessToken);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.HResult.ToString("X") + " Message: " + e.Message);
            }
        }

        public static async void Refresh()
        {
            String refresh_token = (String)UserAccount.getSetting("googleRefreshToken");

            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            Uri requestUri = new Uri("https://www.googleapis.com/oauth2/v4/token");
            HttpResponseMessage response = new HttpResponseMessage();
            HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new Dictionary<String, String> {
                    { "client_id", client_id },
                    { "client_secret", client_secret },
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refresh_token }
                });
            string body = "";

            try
            {
                response = await httpClient.PostAsync(requestUri, content);
                body = await response.Content.ReadAsStringAsync();

                JsonObject obj = JsonValue.Parse(body).GetObject();
                String accessToken = obj.GetNamedString("access_token");
                int expiresIn = (int)obj.GetNamedNumber("expires_in");

                UserAccount.saveSetting("googleAccessToken", accessToken);
                UserAccount.saveSetting("googleFreshness", DateTime.Now.Ticks.ToString());
                UserAccount.saveSetting("googleExpires", expiresIn.ToString());
                Debug.WriteLine(accessToken);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.HResult.ToString("X") + " Message: " + e.Message);
            }
        }

        public static async void GetCalendars()
        {
            DateTime freshness = new DateTime(Convert.ToInt64(UserAccount.getSetting("googleFreshness")));
            int expiresIn = Convert.ToInt32(UserAccount.getSetting("googleExpires"));
            int elapsedTime = (int)DateTime.Now.Subtract(freshness).TotalSeconds;
            if (expiresIn > 0 && elapsedTime > expiresIn)
            {
                await Task.Run(() => Refresh());
            }

            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            Debug.WriteLine(UserAccount.getSetting("googleAccessToken"));
            headers.Authorization = new HttpCredentialsHeaderValue("Bearer", (String)UserAccount.getSetting("googleAccessToken"));
            Uri requestUri = new Uri("https://www.googleapis.com/calendar/v3/users/me/calendarList");
            HttpResponseMessage response = new HttpResponseMessage();
            string body = "";

            try
            {
                response = await httpClient.GetAsync(requestUri);
                body = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(body);
                Debug.WriteLine(response.RequestMessage.Headers.Authorization);
                JsonObject obj = JsonValue.Parse(body).GetObject();
                JsonArray calendars = obj.GetNamedArray("items");
                foreach (JsonValue element in calendars)
                {
                    JsonObject calendar = element.GetObject();
                    String id = calendar.GetNamedString("id");
                    Debug.WriteLine(calendar.GetNamedString("summary"));
                    ApplicationDataCompositeValue cals = (ApplicationDataCompositeValue)UserAccount.getSetting("calendars");
                    Debug.WriteLine("cals: " + cals);
                    if (cals == null)
                    {
                        cals = new ApplicationDataCompositeValue();
                    }
                    if (!cals.ContainsKey(id))
                    {
                        cals.Add(calendar.GetNamedString("id"), true);
                        UserAccount.saveSetting("calendars", cals);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.HResult.ToString("X") + " Message: " + e.Message);
            }
        }

        public static async Task<List<Tuple<int, string>>> GetEvents(String calendarId)
        {
            DateTime freshness = new DateTime(Convert.ToInt64(UserAccount.getSetting("googleFreshness")));
            int expiresIn = Convert.ToInt32(UserAccount.getSetting("googleExpires"));
            int elapsedTime = (int)DateTime.Now.Subtract(freshness).TotalSeconds;
            if (expiresIn > 0 && elapsedTime > expiresIn)
            {
                await Task.Run(() => Refresh());
            }

            DateTime now = DateTime.Now;
            DateTime timeMin = new DateTime(now.Year, now.Month, 1);
            DateTime timeMax = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            String min = timeMin.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
            String max = timeMax.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
            List<Tuple<int, string>> calEvents = new List<Tuple<int, string>>();

            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            headers.Authorization = new HttpCredentialsHeaderValue("Bearer", (String)UserAccount.getSetting("googleAccessToken"));
            Uri requestUri = new Uri("https://www.googleapis.com/calendar/v3/calendars/" + calendarId + "/events?timeMin=" + min + "&timeMax=" + max);
            HttpResponseMessage response = new HttpResponseMessage();
            string body = "";

            response = await httpClient.GetAsync(requestUri);
            body = await response.Content.ReadAsStringAsync();

            JsonObject obj = JsonValue.Parse(body).GetObject();
            IJsonValue test;
            if (obj.TryGetValue("items", out test))
            {
                JsonArray events = obj.GetNamedArray("items");
                Debug.WriteLine(calendarId + " " + events.Count);
                if (events.Count > 0)
                {
                    foreach (JsonValue element in events)
                    {
                        JsonObject evt = element.GetObject();
                        JsonObject start = evt.GetNamedObject("start");

                        if (start.TryGetValue("dateTime", out test))
                        {
                            String time = start.GetNamedString("dateTime");
                            DateTime dateTime;
                            DateTime.TryParseExact(time, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateTime);

                            if (dateTime.Month == now.Month)
                            {
                                String title = dateTime.ToString("HH:mm") + " " + evt.GetNamedString("summary");
                                Debug.WriteLine(dateTime.Day + title);
                                calEvents.Add(new Tuple<int, string>(dateTime.Day, title));
                            }
                        }
                        else
                        {
                            DateTime date;
                            DateTime.TryParse(start.GetNamedString("date"), out date);

                            if (date.Month == now.Month)
                            {
                                String title = evt.GetNamedString("summary");
                                Debug.WriteLine(date.Day + title);
                                calEvents.Add(new Tuple<int, string>(date.Day, title));
                            }
                        }
                    }
                }
            }

            return calEvents;
        }

        public static async Task<List<Tuple<int, string>>> GetAllEvents()
        {
            List<Tuple<int, string>> events = new List<Tuple<int, string>>();
            Debug.WriteLine(UserAccount.getSetting("calendars"));
            ApplicationDataCompositeValue cals = (ApplicationDataCompositeValue)UserAccount.getSetting("calendars");
            if (cals == null)
            {
                cals = new ApplicationDataCompositeValue();
            }
            foreach (KeyValuePair<String, Object> cal in cals)
            {
                if ((bool)cal.Value && cal.Key != "#contacts@group.v.calendar.google.com" && cal.Key != "family03301070956538335300@group.calendar.google.com" && cal.Key != "en.usa#holiday@group.v.calendar.google.com")
                {
                    events.AddRange(await GetEvents(cal.Key));
                }
            }
            return events;
        }

        public static async Task<String> zipcode(String zipcode)
        {
            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            Uri requestUri = new Uri("https://maps.googleapis.com/maps/api/geocode/json?address=" + zipcode + "&key=" + googleMapsKey);
            HttpResponseMessage response = new HttpResponseMessage();
            string body = "";

            try
            {
                response = await httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Error: " + e.HResult.ToString("X") + " Message: " + e.Message;
            }

            JsonObject obj = JsonValue.Parse(body).GetObject();
            JsonObject j = obj.GetNamedArray("results").GetObjectAt(0);
            double lat = j.GetNamedObject("geometry").GetNamedObject("location").GetNamedNumber("lat");
            double lng = j.GetNamedObject("geometry").GetNamedObject("location").GetNamedNumber("lng");
            string locality = "";
            JsonArray address = j.GetNamedArray("address_components");
            foreach (var comp in address)
            {
                JsonArray types = comp.GetObject().GetNamedArray("types");
                foreach (var type in types)
                {
                    if (type.GetString() == "locality")
                    {
                        locality = comp.GetObject().GetNamedString("long_name");
                    }
                }
            }

            JsonObject zipResult = new JsonObject();
            zipResult.SetNamedValue("locality", JsonValue.CreateStringValue(locality));

            Int32 timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            requestUri = new Uri("https://maps.googleapis.com/maps/api/timezone/json?location=" +
                lat + ',' + lng + "&timestamp=" + timestamp + "&key=" + googleMapsKey);
            System.Diagnostics.Debug.WriteLine(requestUri);
            try
            {
                response = await httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Error: " + e.HResult.ToString("X") + " Message: " + e.Message;
            }

            obj = JsonValue.Parse(body).GetObject();
            String tzName = obj.GetNamedString("timeZoneName");
            zipResult.SetNamedValue("timeZoneName", JsonValue.CreateStringValue(tzName));

            return zipResult.ToString();
        }
    }
}
