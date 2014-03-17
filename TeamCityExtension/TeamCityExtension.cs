using System.Reflection;
using System.Windows.Forms;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace TeamCityExtension
{
    public class TeamCityExtension : ITransportExtension
    {
        public string DisplayName { get; private set; }
        public string Settings { get; set; }
        public BuildServer Configuration { get; set; }

        public TeamCityExtension()
        {
            DisplayName = "Team City";
        }

        public CCTrayProject[] GetProjectList(BuildServer server)
        {
            var manager = new TeamCityManager(server);
            return manager.GetProjectList();
        }

        public ICruiseProjectManager RetrieveProjectManager(string projectName)
        {
            return new TeamCityProjectManager(projectName);
        }

        public ICruiseServerManager RetrieveServerManager()
        {
            return new TeamCityManager(Configuration);
        }

        public bool Configure(IWin32Window owner)
        {
            var config = new Configuration();
            if(Configuration != null)
                config.BaseUrl = Configuration.Url;

            config.ShowDialog(owner);

            if (config.DialogResult == DialogResult.OK)
            {
                Configuration = new BuildServer(FormatBaseUrl(config.BaseUrl), BuildServerTransport.Extension, DisplayName,
                    string.Empty);
            }
            return true;
        }

        public string FormatBaseUrl(string url)
        {
            return url.EndsWith("/") ? url.Remove(url.Length - 1) : url;
        }
    }
}
