using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Polly;
using Cardrly.Mode_s.ApplicationUser;
using Cardrly.Helpers;



namespace Cardrly.Helpers
{
    public interface IGenericRepository
    {
        Task<T> GetAsync<T>(string uri, string authToken = "");
        Task<string> GetStrAsync<T>(string uri, string authToken = "");
        Task<ApplicationUserResponse> GetLoginAsync<T>(string uri, string authToken = "");
        Task<T> PostAsync<T>(string uri, T data, string authToken = "");
        Task<string> PostEAsync(string uri, string authToken = "");
        Task<(TR, ErrorResult?)> PostTRAsync<T, TR>(string uri, T data, string authToken = "");
        Task<string> PostStrAsync<T>(string uri, T data, string authToken = "");
        Task<string> PostDataAsync<T>(string uri, T data, string authToken = "");
        Task<string> PostMData<T>(string uri, T data, string authToken = "");
        Task<string> PostMultiPicAsync<T>(string uri, T data, string authToken = "");
        Task<T> PutAsync<T>(string uri, T data, string authToken = "");
        Task<string> PutStrAsync<T>(string uri, T data, string authToken = "");
        Task<string> PutDataAsync<T>(string uri, T data, string authToken = "");
        Task DeleteAsync(string uri, string authToken = "");
        Task<string> DeleteStrItemAsync(string uri, string authToken = "");
        Task<R> PostAsync<T, R>(string uri, T data, string authToken = "");
    }


    public class GenericRepository : IGenericRepository
    {
        private HttpClient CreateHttpClient(string authToken)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }
            return httpClient;
        }


        public async Task<T> GetAsync<T>(string uri, string authToken = "")
        {

            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () =>
                    await httpClient.GetAsync(Utility.ServerUrl + uri));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult =
                        await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json!;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    //throw new ServiceAuthenticationException(jsonResult);
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                    return default(T)!;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    return default(T)!;
                }

                if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 404. System.Net.HttpStatusCode.NotFound indicates\r\nthat the requested resource requires Data.", "OK");
                    return default(T)!;
                    
                }

                //throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);   
                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                var json1 = JsonConvert.DeserializeObject<T>(jsonResult);
                return json1!;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<string> GetStrAsync<T>(string uri, string authToken = "")
        {

            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () =>
                    await httpClient.GetAsync(Utility.ServerUrl + uri));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult =
                        await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    //var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return jsonResult;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    //throw new ServiceAuthenticationException(jsonResult);
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                }

                //throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);
                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                //var json1 = JsonConvert.DeserializeObject<T>(jsonResult);
                return jsonResult;

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<ApplicationUserResponse> GetLoginAsync<T>(string uri, string authToken = "")
        {

            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () =>
                    await httpClient.GetAsync(Utility.ServerUrl + uri));

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                }

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<ApplicationUserResponse>(jsonResult);
                    return json;
                }
                else
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<ApplicationUserResponse>(jsonResult);
                    return json;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<string> PostEAsync(string uri, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(Utility.ServerUrl + uri, null));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return "";
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                    return "No Content";
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    return "No Content";
                }

                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (responseMessage.ReasonPhrase == "No Content")
                {
                    return "No Content";
                }
                else
                {
                    return "No Content";
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }


        public async Task<T> PostAsync<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var cc = JsonConvert.SerializeObject(data);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(Utility.ServerUrl + uri, content, CancellationToken.None));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                    return default(T)!;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                }

                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                var json2 = JsonConvert.DeserializeObject<T>(jsonResult);
                return json2;
                //throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<(TR, ErrorResult?)> PostTRAsync<T, TR>(string uri, T data, string authToken = "")
        {

            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var cc = JsonConvert.SerializeObject(data);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(Utility.ServerUrl + uri, content, CancellationToken.None));

                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                { 
                    var json = JsonConvert.DeserializeObject<TR>(jsonResult);
                    return (json!, null);
                }
                else
                {
                    if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                    {
                        await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                        
                    }

                    if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                        //await StartData.UserLogout();
                    }
                    var model = JsonConvert.DeserializeObject<TR>("");
                    var json = JsonConvert.DeserializeObject<ErrorResult>(jsonResult);
                    return (model!, json);
                }  
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                await App.Current!.MainPage!.DisplayAlert("Info", "Found_Problem_Internal_Server", "Ok");
                var model = JsonConvert.DeserializeObject<TR>(""); 
                return (model!, null); 
            }
        }

        public async Task<string> PostStrAsync<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var cc = JsonConvert.SerializeObject(data);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(Utility.ServerUrl + uri, content, CancellationToken.None));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    //var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return jsonResult;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                    return "";
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                    return "";
                }

                return "";

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                return "";
            }
        }

        public async Task<string> PostDataAsync<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var cc = JsonConvert.SerializeObject(data);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                //var responseMessage= await httpClient.PostAsync(uri, content);

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(Utility.ServerUrl + uri, content, CancellationToken.None));

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                    return "";
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                }

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<object>(jsonResult);
                    return json.ToString();
                }
                else
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    if (responseData.Contains("Not_Enough") == true)
                    {
                        return responseData.Replace("\"", "").Replace("Message", "").Replace(":", "").Replace("{", "").Replace("}", "").Trim();
                    }
                    else if (responseData.Contains("Already Exist For This Schedule Date#") == true)//Duplicate Schedule Dates for Estimate or Invoice
                    {
                        return responseData.Replace("\"", "").Replace("Message", "").Replace(":", "").Replace("{", "").Replace("}", "").Trim();
                    }
                    else if (responseMessage.ReasonPhrase == "Multiple Choices")
                    {
                        return "Multiple Choices";
                    }
                    else
                    {
                        return "api not responding";
                    }
                }



                //throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<string> PostMData<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(Utility.ServerUrl + uri, content, CancellationToken.None));

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                }

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<object>(jsonResult);
                    return json.ToString();
                }
                else if (responseMessage.ReasonPhrase == "Multiple Choices")
                {
                    return "Multiple Choices";
                }
                else
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    if (responseData.Contains("Not_Enough") == true)
                    {
                        return responseData.Replace("\"", "").Replace("Message", "").Replace(":", "").Replace("{", "").Replace("}", "").Trim();
                    }
                    else if (responseData.Contains("already exists") == true)
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


                //throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<string> PostMultiPicAsync<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                string cc = JsonConvert.SerializeObject(data, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                var content = new StringContent(JsonConvert.SerializeObject(data, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));


                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(Utility.ServerUrl + uri, content, CancellationToken.None));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json.ToString();
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                }

                //if (responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                //    responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                //{
                //    //throw new ServiceAuthenticationException(jsonResult);
                //    var jsonResult2 = await responseMessage.Content.ReadAsStringAsync();
                //    var json2 = JsonConvert.DeserializeObject<T>(jsonResult2);
                //    return json2.ToString();
                //}

                var jsonResult3 = await responseMessage.Content.ReadAsStringAsync();
                var json3 = JsonConvert.DeserializeObject<T>(jsonResult3);
                return json3.ToString();

                //throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<T> PutAsync<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                string cc = JsonConvert.SerializeObject(data);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PutAsync(Utility.ServerUrl + uri, content, CancellationToken.None));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return data;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                }

                return JsonConvert.DeserializeObject<T>(await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false));

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<string> PutStrAsync<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                string cc = JsonConvert.SerializeObject(data);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PutAsync(Utility.ServerUrl + uri, content, CancellationToken.None));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    //var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return jsonResult;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                    return "";
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                    return "";
                }

                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                return jsonResult;

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                return "";
            }
        }

        public async Task<string> PutDataAsync<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                string cc = JsonConvert.SerializeObject(data);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PutAsync(Utility.ServerUrl + uri, content, CancellationToken.None));

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                }

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<object>(jsonResult);
                    return json.ToString();
                }
                else
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    if (responseData.Contains("Not_Enough") == true)
                    {
                        return responseData.Replace("\"", "").Replace("Message", "").Replace(":", "").Replace("{", "").Replace("}", "").Trim();
                    }
                    else if (responseData.Contains("Already Exist For This Schedule Date#") == true)//Duplicate Schedule Dates for Estimate or Invoice
                    {
                        return responseData.Replace("\"", "").Replace("Message", "").Replace(":", "").Replace("{", "").Replace("}", "").Trim();
                    }
                    else
                    {
                        return "api not responding";
                    }
                }

                //throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(string uri, string authToken = "")
        {
            HttpClient httpClient = CreateHttpClient(authToken);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var res = await httpClient.DeleteAsync(Utility.ServerUrl + uri);
        }

        public async Task DeleteItemAsync(string uri, string authToken = "")
        {
            HttpClient httpClient = CreateHttpClient(authToken);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            await httpClient.DeleteAsync(Utility.ServerUrl + uri);
        }

        public async Task<string> DeleteStrItemAsync(string uri, string authToken = "")
        {
            HttpClient httpLient = CreateHttpClient(authToken);
            HttpResponseMessage response = new HttpResponseMessage();
            httpLient.DefaultRequestHeaders.Accept.Clear();
            //httpLient.DefaultRequestHeaders.Add("authorization", "Basic ");
            httpLient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            try
            {
                response = await httpLient.DeleteAsync(Utility.ServerUrl + uri);
                var responseData = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates\r\nthat the server refuses to fulfill the request.", "OK");
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await App.Current!.MainPage!.DisplayAlert("Warning", "Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates\r\nthat the requested resource requires authentication.", "OK");
                    //await StartData.UserLogout();
                }

                if (response.IsSuccessStatusCode)
                {
                    return responseData;
                }
                else if (responseData.Contains("Is Not Deleted") == true)
                {
                    return responseData.Replace("\"", "").Replace("Message", "").Replace(":", "").Replace("{", "").Replace("}", "").Trim();
                }
                else
                    return "api not responding";
            }
            catch (Exception ex)
            {
                return "api not responding";
            }
        }

        public async Task<TR> PostAsync<T, TR>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(Utility.ServerUrl + uri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(Utility.ServerUrl + uri, content));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<TR>(jsonResult);
                    return json;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                    responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //throw new ServiceAuthenticationException(jsonResult);
                }

                //throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);
                return default(TR);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }
    }
}
