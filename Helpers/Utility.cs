
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Cardrly.Helpers
{
    public class ClsApiParamters
    {
        public string ParamterName { get; set; }
        public string ParamterValue { get; set; }
    }

    public static class Utility
    {
        static string MGestToken;
        static bool InWork = false;
        //#if DEBUG
        //        public static readonly string ServerUrl = "https://www.dt-works.com/amd/horse";
        //#else
        //        //public static readonly string ServerUrl = "https://www.psaiahf.com/horse";
        //#endif
        public static readonly string ServerUrlIIS = "http://192.168.1.4:8012/";

        public static readonly string PathServerImages = "https://projectservices.engprosoft.com/ScheduleAttachments/";

        public static readonly string PathServerProfileImages = "https://projectservices.engprosoft.com/EmployeePic/";

        public static readonly string ServerUrl = "https://apicards.engprosoft.net/";
        //public static readonly string ServerUrl = "https://dhm.engprosoft.net/";


        public static readonly string FolderImageProfile = "EmployeePic";


        public async static Task<List<string>> GetListDataFromPath(string url, List<string> Path)
        {
            HttpClient _Client = new HttpClient();
            var _Data = await _Client.GetStringAsync(url);

            JObject jo = JObject.Parse(_Data);
            List<string> Data = new List<string>();
            for (int i = 0; i < Data.Count; i++)
            {
                Data.Add((string)jo.SelectToken(Path[i]));
            }
            return Data;
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static string FormatTextWithoutnewLin(string input)
        {
            return input.Replace("\r", " ").Replace("\n", ".");
        }


        /// <summary>
        /// Converts unix date to C# date time
        /// </summary>
        /// <param name="UnixDate"></param>
        /// <returns></returns>
        public static DateTime ConvertUnixToDateTime(string UnixDate)
        {

            if (string.IsNullOrEmpty(UnixDate))
                return new DateTime();

            // Example of a UNIX timestamp for 11-29-2013 4:58:30
            double timestamp = double.Parse(UnixDate);

            // Format our new DateTime object to start at the UNIX Epoch
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

            // Add the timestamp (number of seconds since the Epoch) to be converted
            dateTime = dateTime.AddSeconds(timestamp);

            return dateTime;

        }


        /// <summary>
        /// Return the HTTP client for the service address 
        /// </summary>
        /// <returns>HTTP Client Object </returns>
        public static HttpClient GetClient()
        {
            CookieContainer cookieContainer = new CookieContainer();
            Cookie cookie = new Cookie();
            Uri baseAddress = new Uri(Utility.ServerUrl);
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            var client = new HttpClient(handler) { BaseAddress = baseAddress };

            if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
            {
                cookieContainer.Add(baseAddress, cookie);
            }

            return client;
        }

        public static async Task<string> CallWebApi(string url)
        {
            InWork = true;

            try
            {
                using (var client = GetClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    try
                    {
                        HttpResponseMessage loginResponse = await client.GetAsync(url);
                        if (loginResponse.IsSuccessStatusCode)
                        {
                            using (var responseContent = loginResponse.Content)
                            {
                                var result = responseContent.ReadAsStringAsync().Result;
                                return result;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            catch (WebException ex)
            {
                //await App.Current.MainPage.DisplayAlert("message from server", sResult, "");
                //Log.Report(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //await App.Current.MainPage.DisplayAlert("message from server", sResult, "");
                //Log.Report(ex);
            }
            finally
            {
                InWork = false;
            }
            return null;
        }

        public static async Task<string> CallWebApi(string url, string TokenGest)
        {
            try
            {
                var httpLient = new HttpClient();
                httpLient.DefaultRequestHeaders.Accept.Clear();
                httpLient.DefaultRequestHeaders.Add("Authorization", "Basic " + TokenGest);

                HttpResponseMessage loginResponse = await httpLient.GetAsync(Utility.ServerUrl + url);
                if (loginResponse.IsSuccessStatusCode)
                {
                    using (var responseContent = loginResponse.Content)
                    {
                        var result = responseContent.ReadAsStringAsync().Result;
                        return result;
                    }
                }
            }
            catch (WebException ex)
            {
                //await App.Current.MainPage.DisplayAlert("message from server", sResult, "");
                //Log.Report(ex);
                throw ex;
            }

            return null;
        }

        public static async Task<string> CallWebApi(ClsApiParamters Pramater, string url)
        {
            InWork = true;

            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>(Pramater.ParamterName, Pramater.ParamterValue));

            HttpContent content = new FormUrlEncodedContent(parameters);
            try
            {
                using (var client = GetClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        string URL = (url[0] == 'h') ? url : Utility.ServerUrl + url;
                        HttpResponseMessage loginResponse = await client.GetAsync(URL);
                        if (loginResponse.IsSuccessStatusCode)
                        {
                            using (var responseContent = loginResponse.Content)
                            {
                                var result = responseContent.ReadAsStringAsync().Result;
                                return result;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return "WebException";
                    }
                }
            }
            catch (WebException ex)
            {
                //await App.Current.MainPage.DisplayAlert("message from server", sResult, "");
                //Log.Report(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //await App.Current.MainPage.DisplayAlert("message from server", sResult, "");
                //Log.Report(ex);
            }
            finally
            {
                InWork = false;
            }
            return null;
        }

        public static async Task<string> PostData(string url, string content, string TokenGest)
        {
            var httpLient = new HttpClient();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            //httpLient.DefaultRequestHeaders.Add("authorization", "Basic " + TokenGest);
            httpLient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenGest);
            httpLient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpContent Mycontent = new StringContent(content);
                MediaTypeHeaderValue mValue = new MediaTypeHeaderValue("application/json");
                Mycontent.Headers.ContentType = mValue;
                var response = await httpLient.PostAsync(ServerUrl + url, Mycontent);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return responseData.ToString();
                }
                else
                {
                    switch (response.ReasonPhrase.ToString())
                    {
                        case "Multiple Choices":
                            {
                                result = response.ReasonPhrase;
                            }
                            break;
                        case "Bad Request":
                            {
                                result = response.ReasonPhrase;
                            }
                            break;
                        default:
                            {

                            }
                            break;
                    }
                }
                //return "api not responding";
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static async Task<string> PostPicData(string url, HttpContent content, string TokenGest)
        {
            var httpLient = new HttpClient();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Add("authorization", "Basic " + TokenGest);
            httpLient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                MediaTypeHeaderValue mValue = new MediaTypeHeaderValue("application/json");
                var response = await httpLient.PostAsync(ServerUrl + url, content);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return responseData.ToString();
                }
                else
                    return "api not responding";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static async Task<string> PostData(string url, string content)
        {
            var httpLient = new HttpClient();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpContent Mycontent = new StringContent(content);
                MediaTypeHeaderValue mValue = new MediaTypeHeaderValue("application/json");
                Mycontent.Headers.ContentType = mValue;
                var response = await httpLient.PostAsync(ServerUrl + url, Mycontent);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return responseData.ToString();
                }
                else if (response.ReasonPhrase == "Multiple Choices")
                {
                    return "Multiple Choices";
                }
                else
                {
                    return "api not responding";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public static async Task<string> PostMData(string url, string content)
        {
            var httpLient = new HttpClient();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpContent Mycontent = new StringContent(content);
                MediaTypeHeaderValue mValue = new MediaTypeHeaderValue("application/json");
                Mycontent.Headers.ContentType = mValue;
                var response = await httpLient.PostAsync(ServerUrl + url, Mycontent);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return responseData.ToString();
                }
                else if (response.ReasonPhrase == "Multiple Choices")
                {
                    return "Multiple Choices";
                }
                else
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    if (responseData.Contains("Not_Enough") == true)
                    {
                        return responseData.Replace("\"","").Replace("Message","").Replace(":","").Replace("{","").Replace("}","").Trim(); 
                    }
                    else if(responseData.Contains("already exists") == true)
                    {
                        return responseData.Replace("\"", "").Replace("Message", "").Replace(":", "").Replace("{", "").Replace("}", "").Trim();
                    }
                    else if (responseData.Contains("The_invoice_exists") == true)
                    {
                        return "This Invoice Already Exist From Estimate Or Schedule";
                    }
                    else
                    {
                        return "api not responding";
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public static async Task<string> PutData(string url, string TokenGest)
        {
            var httpLient = new HttpClient();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Add("authorization", "Basic " + TokenGest);
            try
            {
                var response = await httpLient.PutAsync(ServerUrl + url, null);

                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return responseData.ToString();
                }
                else
                {
                    return "api not responding";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static async Task<string> PutPosData(string url, string content)
        {
            var httpLient = new HttpClient();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //httpLient.DefaultRequestHeaders.Add("authorization", "Basic ");
            try
            {
                HttpContent Mycontent = new StringContent(content);
                MediaTypeHeaderValue mValue = new MediaTypeHeaderValue("application/json");
                Mycontent.Headers.ContentType = mValue;

                var response = await httpLient.PutAsync(ServerUrl + url, Mycontent);

                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return responseData.ToString();
                }
                else
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    if (responseData.Contains("Not_Enough") == true)
                    {
                        return responseData.Replace("\"", "").Replace("Message", "").Replace(":", "").Replace("{", "").Replace("}", "").Trim();
                    }
                    else
                    {
                        return "api not responding";
                    }
                }
                    //return "api not responding";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static async Task<string> PutData(string url, string content, string TokenGest)
        {
            var httpLient = new HttpClient();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Add("authorization", "Basic " + TokenGest);
            try
            {
                HttpContent Mycontent = new StringContent(content);
                MediaTypeHeaderValue mValue = new MediaTypeHeaderValue("application/json");
                Mycontent.Headers.ContentType = mValue;

                var response = await httpLient.PutAsync(ServerUrl + url, Mycontent);

                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return responseData.ToString();
                }
                else
                    return "api not responding";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static async Task<string> PutDataJson(string url, string content, string TokenGest)
        {
            var httpLient = new HttpClient();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Add("authorization", "Basic " + TokenGest);
            httpLient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpContent Mycontent = new StringContent(content);
                MediaTypeHeaderValue mValue = new MediaTypeHeaderValue("application/json");
                Mycontent.Headers.ContentType = mValue;
                var response = await httpLient.PutAsync(ServerUrl + url, Mycontent);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return responseData.ToString();
                }
                else
                    return "api not responding";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static async Task<HttpResponseMessage> DeleteAsync(string url, string TokenGest)
        {
            var httpLient = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Add("authorization", "Basic " + TokenGest);

            try
            {
                return response = await httpLient.DeleteAsync(ServerUrl + url);
            }
            catch (Exception ex)
            {
                return response;
            }
        }

        public static async Task<string> DeleteItem(string url)
        {
            var httpLient = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Add("authorization", "Basic ");

            try
            {
                response = await httpLient.DeleteAsync(ServerUrl + url);
                //return response.ToString();
                if (response.IsSuccessStatusCode)
                {
                    return response.ToString();
                }
                else
                    return "api not responding";
            }
            catch (Exception ex)
            {
                return "api not responding";
            }
        }

        public static async Task<string> DeleteData(string url, string content)
        {
            var httpLient = new HttpClient();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            httpLient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpContent Mycontent = new StringContent(content);
                MediaTypeHeaderValue mValue = new MediaTypeHeaderValue("application/json");
                Mycontent.Headers.ContentType = mValue;
                var response = await httpLient.DeleteAsync(ServerUrl + url);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return responseData.ToString();
                }
                else
                    return "api not responding";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public static void ClearNavigationStack()
        {
            var existingPages = Application.Current.MainPage.Navigation.NavigationStack.ToList();
            for (int i = 1; i < existingPages.Count; i++)
            {
                Application.Current.MainPage.Navigation.RemovePage(existingPages[i]);
            }
        }

    }
}

