using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace ChooseC.MonacoEditor.Api.DataHelpers
{
    public static class DownloadNugetPackages
    {
        private static readonly string nuget = "https://api.nuget.org/";
        private static readonly string nuget_CN = "https://nuget.cdn.azure.cn";
        private static readonly string nuget_Index = "/v3/index.json";
        /// <summary>
        /// Nuget Package下载链接
        /// </summary>
        /// <param name="package"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        private static string NugetPackageUrl(string package, string version) => nuget_CN + $"/v3-flatcontainer/{package}/{version}/{package}.{version}.nupkg";
        /// <summary>
        /// Nuget Package Version Index Json Url
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        private static string NugetVersionIndexJsonUrl(string packageName) => nuget_CN + $"/v3-flatcontainer/{packageName}/index.json";
        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri(nuget_CN)
        };

        private static string installationDirectory = Directory.GetCurrentDirectory() + "//NugetPackages//packages";

        public static List<Assembly> LoadPackages(string packages)
        {
            List<Assembly> assemblies = new List<Assembly>();
            if (!String.IsNullOrWhiteSpace(packages))
            {

                string[] npackages = packages.Split(';');
                foreach (var item in npackages)
                {
                    string downloadItem = "";
                    string version = "";
                    if (item.Contains(','))
                    {
                        downloadItem = item.Split(',')[0];
                        version = item.Split(',')[1];
                    }
                    else
                    {
                        downloadItem = item;

                    }
                    var path = $"{installationDirectory}//{downloadItem}";

                    var files = System.IO.Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            var assembly = Assembly.LoadFile(file);
                            assemblies.Add(assembly);
                        }
                        catch (Exception)
                        {
                            // throw
                        }
                    }

                }
            }
            return assemblies;
        }
        public static void DownloadAllPackages(string packages)
        {
            if (!String.IsNullOrWhiteSpace(packages))
            {
                string[] npackages = packages.Split(';');
                foreach (var item in npackages)
                {
                    if (!String.IsNullOrWhiteSpace(item))
                    {
                        string downloadItem = "";
                        string version = "";
                        if (item.Contains(','))
                        {
                            downloadItem = item.Split(',')[0];
                            version = item.Split(',')[1];
                        }
                        else
                        {
                            downloadItem = item;

                        }
                        if (!String.IsNullOrWhiteSpace(version))
                        {
                            DownloadPackage(downloadItem, version);
                        }
                        else
                        {
                            DownloadPackage(downloadItem, null);
                        }
                    }
                }
            }
        }
        public static void DownloadPackage(string packageName, string version)
        {
            string packageInstallationDirectory = installationDirectory + $"//{packageName}";
            if (!System.IO.File.Exists($"{packageInstallationDirectory}//{packageName}.nuget"))
            {
                if (!System.IO.Directory.Exists(packageInstallationDirectory))
                {
                    System.IO.Directory.CreateDirectory(packageInstallationDirectory);
                }
                if (string.IsNullOrWhiteSpace(version))
                {
                    version = GetLatestVersion(packageName);
                }

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(NugetPackageUrl(packageName, version)),
                    Method = HttpMethod.Get,
                };
                using (var fs = new FileStream($"{packageInstallationDirectory}//{packageName}.nuget", FileMode.CreateNew))
                {
                    client.Send(request).Content.CopyToAsync(fs);
                }

                System.IO.Compression.ZipFile.ExtractToDirectory($"{packageInstallationDirectory}//{packageName}.nuget", packageInstallationDirectory, true);
            }

        }

        public static string GetLatestVersion(string packageName)
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(NugetVersionIndexJsonUrl(packageName)),
                    Method = HttpMethod.Get
                };
                JObject jsonobj = JObject.Parse(client.Send(request).Content.ReadAsStringAsync().Result);
                return ((IList<string>)jsonobj["versions"]).Last();
            }
            catch (Exception)
            {
                throw;
            }

        }

    }
}
