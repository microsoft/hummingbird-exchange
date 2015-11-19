using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Hummingbird.Models;
using Hummingbird.ViewModels;

namespace Hummingbird.Core
{
    internal class CredentialManager
    {
        private readonly byte[] _container =
        {
            0x45, 0x09, 0x36, 0xd2, 0x8e, 0x1a, 0xce, 0x5d, 0x3f, 0x03, 0xe2, 0x0b, 0xde, 0x37, 0x9f, 0x31,
            0x91, 0xea, 0x2a, 0xfc, 0x57, 0x04, 0x9a, 0xf1, 0xe8, 0xcc, 0xaa, 0xff, 0xfb, 0x7b, 0x1d, 0x68,
            0x6f, 0xb7, 0x5f, 0x58, 0x36, 0x34, 0x4f, 0xf9, 0x4e, 0xae, 0x7c, 0xd4, 0x4d, 0x83, 0xcc, 0x50,
            0xb8, 0x3c, 0xe7, 0xb8, 0x93, 0xb6, 0xf1, 0xb4, 0x75, 0xf2, 0x0e, 0x54, 0x10, 0x55, 0x0b, 0x3c,
            0xab, 0x50, 0xb2, 0x3d, 0xe2, 0x5b, 0x05, 0xda, 0xcf, 0xfe, 0x88, 0xa1, 0xb4, 0xce, 0xdd, 0x0f,
            0xd5, 0xde, 0x15, 0x25, 0x7b, 0xcf, 0xc1, 0x34, 0x37, 0x82, 0xaa, 0x26, 0x0a, 0x71, 0xae, 0x28,
            0x3b, 0x11, 0xa9, 0xa4, 0x58, 0xca, 0xf8, 0xe8, 0x6b, 0xc2, 0xda, 0xee, 0xc2, 0xe0, 0x08, 0xd3,
            0x5f, 0x45, 0xaf, 0xd7, 0xa9, 0xbb, 0xc9, 0xa0, 0xaa, 0xe4, 0x25, 0x67, 0x76, 0x34, 0x95, 0x7d
        };

        internal CredentialManager()
        {
            var localPath = AppDomain.CurrentDomain.BaseDirectory;
            AppPath = Directory.CreateDirectory(Path.Combine(localPath, AppSetup.SettingsContainerName)).FullName;
        }

        private string AppPath { get; }

        /// <summary>
        ///     Decrypts the user credentials stored locally.
        /// </summary>
        /// <returns></returns>
        internal UserCredentials GetUserCredentials()
        {
            UserCredentials credentials = null;

            try
            {
                var rawContent = ReadFromLocalAppdata();
                if (rawContent != null)
                {
                    var decryptedContent = ProtectedData.Unprotect(rawContent, _container,
                        DataProtectionScope.CurrentUser);

                    IFormatter objectFormatter = new BinaryFormatter();
                    using (Stream stream = new MemoryStream(decryptedContent))
                    {
                        credentials = (UserCredentials) objectFormatter.Deserialize(stream);
                    }
                }
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("GetUserCredentials:Error ", exception.Message,
                    Environment.NewLine,
                    exception.StackTrace));
            }

            return credentials;
        }

        /// <summary>
        ///     Encrypts and stores the user credentials locally.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal bool StoreUserCredentials(string userName, string password)
        {
            byte[] credentialsData;

            IFormatter objectFormatter = new BinaryFormatter();
            var credentials = new UserCredentials {Username = userName, Password = password};

            using (var stream = new MemoryStream())
            {
                objectFormatter.Serialize(stream, credentials);
                credentialsData = stream.ToArray();
            }

            var encryptedData = ProtectedData.Protect(credentialsData, _container, DataProtectionScope.CurrentUser);

            return WriteToLocalAppData(encryptedData);
        }

        /// <summary>
        ///     Writes the encrypted binary content to the Local App Data folder.
        /// </summary>
        /// <param name="content">Content that is already encrypted.</param>
        /// <returns></returns>
        private bool WriteToLocalAppData(byte[] content)
        {
            try
            {
                File.WriteAllBytes(Path.Combine(AppPath, AppSetup.CredentialContainerName), content);

                return true;
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("WriteToLocalAppData:Error ", exception.Message,
                    Environment.NewLine,
                    exception.StackTrace));

                return false;
            }
        }

        /// <summary>
        ///     Read teh binary representation of the credentials file.
        /// </summary>
        /// <returns></returns>
        private byte[] ReadFromLocalAppdata()
        {
            try
            {
                var fileContent = File.ReadAllBytes(Path.Combine(AppPath, "container"));
                return fileContent;
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("ReadFromLocalAppdata:Error ", exception.Message,
                    Environment.NewLine,
                    exception.StackTrace));

                return null;
            }
        }
    }
}