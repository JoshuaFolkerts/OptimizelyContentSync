using System.Configuration;

namespace Optimizely.ImportExport
{
    public interface IContentSyncOptions
    {
        bool ContentStagingEnabled { get; set; }

        string SettingPageFiles { get; set; }

        string SettingRecursively { get; set; }

        string SettingPageLink { get; set; }

        string SettingIncludeContentTypeDependencies { get; set; }

        string Username { get; set; }

        string Password { get; set; }

        string StagingUrl { get; set; }
    }

    public class ContentSyncOptions : IContentSyncOptions
    {
        public ContentSyncOptions()
        {
            this.StagingUrl = ConfigurationManager.AppSettings["contentStaging:ProductionUrlBase"];
            this.Username = ConfigurationManager.AppSettings["contentStaging:Username"];
            this.Password = ConfigurationManager.AppSettings["contentStaging:Password"];
        }

        public ContentSyncOptions(bool contentStagingEnabled,
            string settingPageFiles,
            string settingRecursively,
            string settingPageLink,
            string settingIncludeContentTypeDependencies,
            string stagingUrl,
            string username,
            string password) : this()
        {
            this.ContentStagingEnabled = contentStagingEnabled;
            this.SettingPageFiles = settingPageFiles;
            this.SettingRecursively = settingRecursively;
            this.SettingPageLink = settingPageLink;
            this.SettingIncludeContentTypeDependencies = settingIncludeContentTypeDependencies;
            this.StagingUrl = stagingUrl;
            this.Username = username;
            this.Password = password;
        }

        public bool ContentStagingEnabled { get; set; }

        public string SettingPageFiles { get; set; } = "ExportPageFiles";

        public string SettingRecursively { get; set; } = "ExportRecursively";

        public string SettingPageLink { get; set; } = "ExporterPageLink";

        public string SettingIncludeContentTypeDependencies { get; set; } = "ExportIncludeContentTypeDependencies";

        public string Username { get; set; }

        public string Password { get; set; }

        public string StagingUrl { get; set; }
    }
}