using System.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;

namespace TeamCityExtension
{
    public class TeamCityManager : ICruiseServerManager
    {
        private readonly HttpClient _httpClient;
        //private readonly ISirenOfShameManager _sosManager;

        public TeamCityManager(BuildServer buildServer) : this(buildServer, new HttpClient())
        {
        }

        public TeamCityManager(BuildServer buildServer, HttpClient httpClient)
        {
            DisplayName = "TeamCityManager";
            Configuration = buildServer;
            _httpClient = httpClient;
        }

        public string DisplayName { get; private set; }
        public BuildServer Configuration { get; private set; }
        public string SessionToken { get; private set; }
        public void CancelPendingRequest(string projectName)
        {
            throw new NotImplementedException();
        }

        public CruiseServerSnapshot GetCruiseServerSnapshot()
        {
            var projectList = GetBuildList();
            var currentlyExecutingBuilds = GetExecutingBuilds();

            var projectStatuses = new List<ProjectStatus>();
            foreach (var build in projectList)
            {
                var projectStatus = new ProjectStatus
                {
                    Name = build.Key,
                    BuildStatus = build.Value.buildStatus == BuildStatus.Success
                        ? IntegrationStatus.Success
                        : IntegrationStatus.Failure,
                    Category = build.Value.buildStatus.ToString(),
                    LastBuildLabel = build.Value.number,
                    WebURL = build.Value.webUrl
                };

                projectStatuses.Add(projectStatus);

                if (currentlyExecutingBuilds.ContainsKey(build.Key))
                {
                    projectStatus.Activity = ProjectActivity.Building;
                    projectStatus.LastBuildLabel = currentlyExecutingBuilds[build.Key].number;
                    projectStatus.Messages = new Message[1];
                    projectStatus.Messages[0] = new Message("Percent Complete: " + currentlyExecutingBuilds[build.Key].percentageComplete);
                    projectStatus.WebURL = currentlyExecutingBuilds[build.Key].webUrl;
                }
            }

            var snapshot = new CruiseServerSnapshot
            {
                ProjectStatuses = projectStatuses.ToArray()
            };

            ManageSiren(projectStatuses);
            return snapshot;
        }

        private void ManageSiren(IEnumerable<ProjectStatus> projectStatuses)
        {
            if (projectStatuses.Count(x => x.Activity == ProjectActivity.Building) > 0)
            {
                //_sosManager.Building();
                return;
            }

            if (projectStatuses.Count(x => x.BuildStatus == IntegrationStatus.Failure) == 0)
            {
                //_sosManager.AllBuildsGood();
            }

        }

        private SortedDictionary<string, TcBuild> GetBuildList()
        {
            var url = BuildFullUrl("/guestAuth/app/rest/builds?locator=branch:(default:any)");
            var result = MakeRequest(url);
            var buildResponse = JsonConvert.DeserializeObject<TcBuildResponse>(result);

            var buildList = new SortedDictionary<string, TcBuild>();
            foreach (var build in buildResponse.build)
            {
                var key = string.Format("{0}-[{1}]", build.buildTypeId, build.branchName);
                if (!buildList.ContainsKey(key))
                {
                    buildList.Add(key, build);
                }
            }
            return buildList;
        }

        public CCTrayProject[] GetProjectList()
        {
            var buildList = GetBuildList();
            return buildList.Select(build => new CCTrayProject(Configuration.Url, build.Key)).ToArray();
        }

        public bool Login()
        {
            return true;
        }

        public void Logout()
        {

        }

        public SortedDictionary<string, TcBuild> GetExecutingBuilds()
        {
            var url = BuildFullUrl("/guestAuth/app/rest/builds?locator=branch:(default:any),running:true");
            var result = MakeRequest(url);
            var buildResponse = JsonConvert.DeserializeObject<TcBuildResponse>(result);

            var currentlyExecutingBuilds = new SortedDictionary<string, TcBuild>();
            foreach (var build in buildResponse.build)
            {
                var key = string.Format("{0}-[{1}]", build.buildTypeId, build.branchName);
                if (!currentlyExecutingBuilds.ContainsKey(key))
                {
                    currentlyExecutingBuilds.Add(key, build);
                }
            }
            return currentlyExecutingBuilds;
        }

        private string BuildFullUrl(string url)
        {
            return string.Format("{0}{1}", Configuration.Url, url);
        }

        private string MakeRequest(string url)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = _httpClient.GetAsync(url).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return result;
        }
    }
}