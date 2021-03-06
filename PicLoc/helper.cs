﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace PicLoc
{
    class helper
    {
        public void hideStatusBar()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var i = Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            }
        }

        public async Task<StorageFolder> getImageFolder()
        {
            try
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("images");
            }
            catch (FileNotFoundException)
            {
                StorageFolder new_images = await ApplicationData.Current.LocalFolder.CreateFolderAsync("images", CreationCollisionOption.ReplaceExisting);
            }
            return await ApplicationData.Current.LocalFolder.GetFolderAsync("images");
        }

        public async void createImageFolder()
        {
            try
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("images");
            }
            catch (FileNotFoundException)
            {
                StorageFolder new_images = await ApplicationData.Current.LocalFolder.CreateFolderAsync("images", CreationCollisionOption.ReplaceExisting);
            }
        }

        public Boolean isStatusBarShowing()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public string getOSVersion()
        {
            string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong version = ulong.Parse(deviceFamilyVersion);
            ulong major = (version & 0xFFFF000000000000L) >> 48;
            ulong minor = (version & 0x0000FFFF00000000L) >> 32;
            ulong build = (version & 0x00000000FFFF0000L) >> 16;
            ulong revision = (version & 0x000000000000FFFFL);
            return $"{major}.{minor}.{build}.{revision}";
        }

        public async Task<Boolean> requestDeviceID(ProgressBar progressBar)
        {
            account a = new account();
            var returnvar = await a.device_id(progressBar);
            return true;
        }

        public string getDeviceID()
        {
            var vault = new PasswordVault();
            try
            {
                PasswordCredential cred = vault.Retrieve("PicLoc", "device_id");
                if (cred != null)
                {
                    return cred.Password;
                } else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string getAutoLogin()
        {
            var vault = new PasswordVault();
            try
            {
                PasswordCredential cred = vault.Retrieve("PicLoc", "auto_login");
                if (cred != null)
                {
                    return cred.Password;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public Boolean setAutoLogin(String username)
        {
            try {
                var vault = new PasswordVault();
                var cred = new PasswordCredential("PicLoc", "auto_login", username);
                vault.Add(cred);
                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        public Boolean clearAutoLogin()
        {
            var vault = new PasswordVault();
            try
            {
                PasswordCredential cred = vault.Retrieve("PicLoc", "auto_login");
                if (cred != null)
                {
                    vault.Remove(cred);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string getPasswordFromUsername(String username)
        {
            var vault = new PasswordVault();
            try
            {
                PasswordCredential cred = vault.Retrieve("PicLoc_accounts", username);
                if (cred != null)
                {
                    return cred.Password;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public Boolean setUsernamePassword(String username, String password)
        {
            try
            {
                var vault = new PasswordVault();
                var cred = new PasswordCredential("PicLoc_accounts", username, password);
                vault.Add(cred);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Boolean setDeviceID(String did)
        {
            try {
                var vault = new PasswordVault();
                var cred = new PasswordCredential("PicLoc", "device_id", did);
                vault.Add(cred);
                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        public Boolean clearDeviceID()
        {
            var vault = new PasswordVault();
            try
            {
                PasswordCredential cred = vault.Retrieve("PicLoc", "device_id");
                if (cred != null)
                {
                    vault.Remove(cred);
                    return true;
                } else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string getHardwareIdentifier()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
            {
                var deviceInformation = new EasClientDeviceInformation();
                return deviceInformation.Id.ToString();
            }
            else
            {
                return "No Identifier Present";
            }
        }

        public string sha512(String text)
        {
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            String strAlgNameUsed = objAlgProv.AlgorithmName;
            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);
            if (buffHash.Length != objAlgProv.HashLength)
            {
                throw new Exception("There was an error creating the hash");
            }
            String strHashBase64 = CryptographicBuffer.EncodeToBase64String(buffHash);
            return strHashBase64;
        }

        public Boolean isMobile()
        {
            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
            return (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile");
        }

        public async void showSingleButtonDialog(String title, String message, String buttonOne)
        {
            var dialog = new MessageDialog(message);
            dialog.Title = title;
            dialog.Commands.Add(new UICommand(buttonOne, null, "1"));
            var show_dialog = await dialog.ShowAsync();
        }

        public async Task<int> showDialog2Buttons(String title, String message, String buttonOne, String buttonTwo)
        {
            var dialog = new MessageDialog(message);
            dialog.Title = title;
            dialog.Commands.Add(new UICommand(buttonOne, null, "1"));
            dialog.Commands.Add(new UICommand(buttonTwo, null, "2"));
            var show_dialog = await dialog.ShowAsync();

            if (show_dialog.Label == buttonOne)
            {
                return 1;
            } else if (show_dialog.Label == buttonTwo)
            {
                return 2;
            } else
            {
                return 0;
            }
        }

    }

    }
