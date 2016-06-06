using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hummingbird.Models;
using Hummingbird.ViewModels;

namespace Hummingbird.Core.Common
{
    internal class FileSystemOperator
    {
        internal FileSystemOperator()
        {
            var localPath = AppDomain.CurrentDomain.BaseDirectory;
            AppPath = Directory.CreateDirectory(Path.Combine(localPath, "Output")).FullName;
        }

        private string AppPath { get; set; }

        /// <summary>
        /// Stores the DL information in a local file.
        /// </summary>
        /// <param name="distributionList">Existing distribution list model.</param>
        /// <returns></returns>
        internal string StoreDistributionListInformation(DistributionList distributionList)
        {
            string path;

            try
            {
                var directory = Directory.CreateDirectory(AppPath);

                var serializer = new XmlSerializer(typeof(DistributionList));
                path = Path.Combine(directory.FullName, distributionList.Name + ".xmldl");

                using (var writer = new StreamWriter(path))
                {
                    serializer.Serialize(writer, distributionList);
                }

                LoggingViewModel.Instance.Logger.Write(string.Concat("StoreDistributionListInformation:OK ", path,
                    Environment.NewLine,
                    distributionList.Name));
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("StoreDistributionListInformation:Error ",
                    exception.Message, Environment.NewLine,
                    exception.StackTrace, Environment.NewLine, string.Join(",", distributionList.Members.ToArray()),
                    Environment.NewLine,
                    distributionList.Owner));

                path = string.Empty;
            }

            return path;
        }

        /// <summary>
        /// Stores the DL invalid members info in a local file.
        /// </summary>
        /// <param name="distributionList">Existing distribution list model.</param>
        /// <returns></returns>
        internal string StoreDistributionListFailures(DistributionList distributionList, string action, AddMembersErrorDetails error)
        {
            string path;

            try
            {
                var directory = Directory.CreateDirectory(AppPath);

                var serializer = new XmlSerializer(typeof(AddMembersErrorDetails));
                path = Path.Combine(directory.FullName, distributionList.Name + "_" + action + "_Failures.xmldl");

                using (var writer = new StreamWriter(path))
                {
                    serializer.Serialize(writer, error);
                }

                LoggingViewModel.Instance.Logger.Write(string.Concat("StoreDistributionListInvalidMembers:OK ", path,
                    Environment.NewLine,
                    distributionList.Name));
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("StoreDistributionListInvalidMembers:Error ",
                    exception.Message, Environment.NewLine,
                    exception.StackTrace,
                    Environment.NewLine,
                    distributionList.Owner));

                path = string.Empty;
            }

            return path;
        }

        /// <summary>
        /// Loads distribution list information from an external file.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns></returns>
        internal DistributionList GetDistributionListInformation(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    var serializer = new XmlSerializer(typeof(DistributionList));
                    var distributionList = (DistributionList)serializer.Deserialize(reader);

                    return distributionList;
                }
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("GetDistributionListInformation:Error ",
                    exception.Message, Environment.NewLine,
                    exception.StackTrace, Environment.NewLine, filePath));

                return null;
            }
        }
    }
}