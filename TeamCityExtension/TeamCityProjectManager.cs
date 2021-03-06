using System;
using System.Collections.Generic;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Parameters;

namespace TeamCityExtension
{
    public class TeamCityProjectManager : ICruiseProjectManager
    {
        public TeamCityProjectManager(string projectName)
        {
            ProjectName = projectName;
        }

        public void ForceBuild(string sessionToken, Dictionary<string, string> parameters, string userName)
        {
            //throw new NotImplementedException();
        }

        public void FixBuild(string sessionToken, string fixingUserName)
        {
            //throw new NotImplementedException();
        }

        public void AbortBuild(string sessionToken, string userName)
        {
            //throw new NotImplementedException();
        }

        public void StopProject(string sessionToken)
        {
            //throw new NotImplementedException();
        }

        public void StartProject(string sessionToken)
        {
            //throw new NotImplementedException();
        }

        public void CancelPendingRequest(string sessionToken)
        {
        }

        public string ProjectName { get; private set; }
        public ProjectStatusSnapshot RetrieveSnapshot()
        {
            throw new NotImplementedException();
        }

        public PackageDetails[] RetrievePackageList()
        {
            throw new NotImplementedException();
        }

        public IFileTransfer RetrieveFileTransfer(string fileName)
        {
            throw new NotImplementedException();
        }

        public List<ParameterBase> ListBuildParameters()
        {
            throw new NotImplementedException();
        }
    }
}