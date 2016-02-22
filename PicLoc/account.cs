using System;
using Windows.Web.Http;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace PicLoc
{
    class account
    {

        private HttpClient httpClient = new HttpClient();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private ProgressBar _progressBar;
        helper h = new helper();

        private void resetProgressBar()
        {
            _progressBar.IsIndeterminate = true;
            _progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        public async Task<String> login(String username, String password, ProgressBar progressBar, Boolean useToken = false)
        {
            HttpResponseMessage response = null;
            String responseString = null;

            _progressBar = progressBar;

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
                _progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
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

        private void ProgressHandler(HttpProgress progress)
        {
            //Debug.WriteLine("Stage: " + progress.Stage.ToString());
            //Debug.WriteLine("Retires: " + progress.Retries.ToString(CultureInfo.InvariantCulture));
            //Debug.WriteLine("Bytes Sent: " + progress.BytesSent.ToString(CultureInfo.InvariantCulture));
            //Debug.WriteLine("Bytes Received: " + progress.BytesReceived.ToString(CultureInfo.InvariantCulture));

            ulong totalBytesToSend = 0;
            if (progress.TotalBytesToSend.HasValue)
            {
                totalBytesToSend = progress.TotalBytesToSend.Value;
                //Debug.WriteLine("Total Bytes To Send: " + totalBytesToSend.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                //Debug.WriteLine("Total Bytes To Send: " + "unknown");
            }

            ulong totalBytesToReceive = 0;
            if (progress.TotalBytesToReceive.HasValue)
            {
                totalBytesToReceive = progress.TotalBytesToReceive.Value;
                //Debug.WriteLine("Total Bytes To Receive: " + totalBytesToReceive.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                //Debug.WriteLine("Total Bytes To Receive: " + "unknown");
            }

            double requestProgress = 0;
            if (progress.Stage == HttpProgressStage.SendingContent && totalBytesToSend > 0)
            {
                requestProgress = progress.BytesSent * 50 / totalBytesToSend;
            }
            else if (progress.Stage == HttpProgressStage.ReceivingContent)
            {
                // Start with 50 percent, request content was already sent.
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

            if (_progressBar != null)
            {
                if (requestProgress != 0)
                {
                    _progressBar.IsIndeterminate = false;
                    _progressBar.Value = requestProgress;
                    if (requestProgress == 100)
                    {
                        _progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                } else
                {
                    _progressBar.IsIndeterminate = true;
                }
            }

            Debug.WriteLine("Progress: " + requestProgress);
            //snap_progress.Value = requestProgress;
        }
    }

}

