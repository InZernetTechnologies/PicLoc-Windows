using System;
using Windows.Web.Http;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Newtonsoft.Json.Linq;
using Windows.Storage.Streams;

namespace PicLoc
{
    class account
    {

        private HttpClient httpClient = new HttpClient();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private ProgressBar _progressBar = null;
        helper h = new helper();

        private void resetProgressBar()
        {
            //_progressBar.IsIndeterminate = true;
            if (_progressBar == null)
            {
                Debug.WriteLine("***** PROGRESS BAR NULL *******");
                return;
            }
            _progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        public async Task<String> login(String username, String password, ProgressBar progressBar, Boolean useToken = false)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;
            _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (!useToken)
            {
                password = h.sha512(password);
            }

            try
            {
                String device_id = h.getDeviceID();
                var values = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password },
                { "device_id", device_id }
            };

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                response = await httpClient.PostAsync(new Uri(settings.API + "/login/"), formContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("login | responseString: " + responseString);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("login | Canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("login | Error: " + ex.StackTrace);
            }
            finally
            {
                Debug.WriteLine("login | Completed");
                resetProgressBar();
            }

            return responseString;
        }

        public async Task<String> signup(String username, String email, String password, ComboBox gender, DatePicker birthdate, ProgressBar progressBar)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;

            try
            {
                String formatted_gender = "";
                if (gender.SelectedIndex != -1)
                {
                    formatted_gender = ((ComboBoxItem)gender.SelectedItem).Content.ToString();
                }

                if (password != "")
                {
                    password = h.sha512(password);
                }

                String year = birthdate.Date.Year.ToString();
                String month = birthdate.Date.Month.ToString().PadLeft(2, '0');
                String day = birthdate.Date.Day.ToString().PadLeft(2, '0');
                String formatted_birthdate = year + "-" + month + "-" + day;
                var values = new Dictionary<string, string>
            {
                    { "username", username },
                    { "email", email },
                    { "password", password },
                    { "gender", formatted_gender },
                    { "birthdate", formatted_birthdate }
            };

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                response = await httpClient.PostAsync(new Uri(settings.API + "/register/"), formContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("register | responseString: " + responseString);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("register | Canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("register | Error: " + ex.StackTrace);
            }
            finally {
                Debug.WriteLine("register | Completed");
                resetProgressBar();
            }

            return responseString;
        }

        public async Task<String> device_id(ProgressBar progressBar)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;

            try
            {
                String device_id = h.getDeviceID();
                var values = new Dictionary<string, string>
            {
                { "device", "PicLoc UWP/" + settings.APP_version }
            };

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                response = await httpClient.PostAsync(new Uri(settings.API + "/device_id/"), formContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("device_id | responseString: " + responseString);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("device_id | Canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("device_id | Error: " + ex.StackTrace);
            }
            finally
            {
                Debug.WriteLine("device_id | Completed");
            }

            return responseString;
        }

        public async Task<String> getSnaps(String username, String password, ProgressBar progressBar)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;

            try
            {
                String device_id = h.getDeviceID();
                var values = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password },
                { "device_id", device_id }
            };

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                response = await httpClient.PostAsync(new Uri(settings.API + "/get_snaps/"), formContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("getSnaps | responseString: " + responseString);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("getSnaps | Canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("getSnaps | Error: " + ex.StackTrace);
            }
            finally
            {
                Debug.WriteLine("getSnaps | Completed");
                resetProgressBar();
            }

            return responseString;
        }

        public async Task<String> getFriends(String username, String password, ProgressBar progressBar)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;
            _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            try
            {
                String device_id = h.getDeviceID();
                var values = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password },
                { "device_id", device_id }
            };

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                response = await httpClient.PostAsync(new Uri(settings.API + "/get_friends/"), formContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("getFriends | responseString: " + responseString);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("getFriends | Canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("getFriends | Error: " + ex.StackTrace);
            }
            finally
            {
                Debug.WriteLine("getFriends | Completed");
                resetProgressBar();
            }

            return responseString;
        }

        public async Task<Boolean> getSnap(String username, String password, int snapID, ProgressBar progressBar)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;
            _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            try
            {
                String device_id = h.getDeviceID();
                var values = new Dictionary<string, string>
            {
                    { "username", username },
                    { "password", password },
                    { "device_id", device_id },
                    { "snap_id", snapID.ToString() }

            };
                Debug.WriteLine("getSnap | user: " + username);
                Debug.WriteLine("getSnap | pass: " + password);
                Debug.WriteLine("getSnap | did: " + device_id);
                Debug.WriteLine("getSnap | snap: " + snapID.ToString());

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                response = await httpClient.PostAsync(new Uri(settings.API + "/get_snap/"), formContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("getSnap | Canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("getSnap | Error: " + ex.StackTrace);
            }
            finally
            {
                Debug.WriteLine("getSnaps | Completed");
                resetProgressBar();
            }

            JObject jo = null;

            if (response.Content.Headers.ContentType.ToString() == "image/png")
            {
                Debug.WriteLine("Snap is good");

                try
                {
                    StorageFolder image = await h.getImageFolder();
                    StorageFile file = await image.CreateFileAsync(snapID + ".jpg", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBufferAsync(file, await response.Content.ReadAsBufferAsync());
                    return true;
                }
                catch (System.Exception ex)
                {
                    h.showSingleButtonDialog("Snap download error", "There was an error savind the snap: " + ex.Message, "Close");
                    return false;
                }
            } else
            {
                jo = JObject.Parse(responseString);
                h.showSingleButtonDialog("Server error [" + jo["code"] + "]", ((jo["message"] != null) ? jo["message"].ToString() : "No server message was provided"), "Dismiss");
                return false;
            }
        }

        public async Task<String> sendSnap(String username, String password, ProgressBar progressBar, String userTo, StorageFile file, int time)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;
            _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            try
            {
                IInputStream inputStream = await file.OpenAsync(FileAccessMode.Read);

                String device_id = h.getDeviceID();
                HttpMultipartFormDataContent multipartContent = new HttpMultipartFormDataContent();

                Debug.WriteLine("sendSnap | File name: " + file.Name);

                multipartContent.Add(
                    new HttpStreamContent(inputStream),
                    "snap",
                    file.Name);

                multipartContent.Add(new HttpStringContent(username), "username");
                multipartContent.Add(new HttpStringContent(password), "password");
                multipartContent.Add(new HttpStringContent(h.getDeviceID()), "device_id");
                multipartContent.Add(new HttpStringContent(userTo), "send_snap_to");
                multipartContent.Add(new HttpStringContent(time.ToString()), "send_snap_time");

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                response = await httpClient.PostAsync(new Uri(settings.API + "/send_snap/"), multipartContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("sendSnap | responseString: " + responseString);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("sendSnap | Canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("sendSnap | Error: " + ex.StackTrace);
            }
            finally
            {
                Debug.WriteLine("sendSnap | Completed");
                resetProgressBar();
            }

            return responseString;
        }

        public async Task<String> addFriend(String username, String password, ProgressBar progressBar, String userToAdd)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;

            try
            {
                String device_id = h.getDeviceID();
                var values = new Dictionary<string, string>
            {
                    { "username", username },
                    { "password", password },
                    { "device_id", device_id },
                    { "friend", userToAdd }
            };

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                response = await httpClient.PostAsync(new Uri(settings.API + "/add_friend/"), formContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("addFriend | responseString: " + responseString);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("addFriend | Canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("addFriend | Error: " + ex.StackTrace);
            }
            finally
            {
                Debug.WriteLine("addFriend | Completed");
                resetProgressBar();
            }

            return responseString;
        }

        public async Task<String> acceptFriend(String username, String password, ProgressBar progressBar, String friend)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;

            try
            {
                String device_id = h.getDeviceID();
                var values = new Dictionary<string, string>
            {
                    { "username", username },
                    { "password", password },
                    { "device_id", device_id },
                    { "friend", friend }
            };

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                response = await httpClient.PostAsync(new Uri(settings.API + "/accept_friend/"), formContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("acceptFriend | responseString: " + responseString);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("acceptFriend | Canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("acceptFriend | Error: " + ex.StackTrace);
            }
            finally
            {
                Debug.WriteLine("acceptFriend | Completed");
                resetProgressBar();
            }

            return responseString;
        }

        private void ProgressHandler(HttpProgress progress)
        {
            /* For some reason, the progress is so messed up and I have no clue why. Theoretically this should work, but it doesn't.
            The progress bar doesn't even show up and the value doesn't get set
            */

            // Debug.WriteLine("Stage: " + progress.Stage.ToString());
            // Debug.WriteLine("Retires: " + progress.Retries.ToString());
            // Debug.WriteLine("Bytes Sent: " + progress.BytesSent.ToString());
            // Debug.WriteLine("Bytes Received: " + progress.BytesReceived.ToString());

            ulong totalBytesToSend = 0;
            if (progress.TotalBytesToSend.HasValue)
            {
                totalBytesToSend = progress.TotalBytesToSend.Value;
            }

            ulong totalBytesToReceive = 0;
            if (progress.TotalBytesToReceive.HasValue)
            {
                totalBytesToReceive = progress.TotalBytesToReceive.Value;
            }

            double requestProgress = 0;
            if (progress.Stage == HttpProgressStage.SendingContent && totalBytesToSend > 0)
            {
                requestProgress = progress.BytesSent * 50 / totalBytesToSend;
            }
            else if (progress.Stage == HttpProgressStage.ReceivingContent)
            {
                requestProgress += 50;
                if (totalBytesToReceive > 0)
                {
                    requestProgress += progress.BytesReceived * 50 / totalBytesToReceive;
                }
            }
            else
            {
                return;
            }

            Debug.WriteLine(requestProgress.ToString());

            if (_progressBar != null)
            {
                Debug.WriteLine("not null");
                if (requestProgress != 0)
                {
                    Debug.WriteLine("progress not 0");
                    _progressBar.IsIndeterminate = false;
                    _progressBar.Value = requestProgress;
                    if (requestProgress == 100)
                    {
                        Debug.WriteLine("100% reset");
                        resetProgressBar();
                    }
                    Debug.WriteLine("done");
                } else
                {
                    Debug.WriteLine("Else reset");
                    resetProgressBar();
                }
            }
        }
    }
}

