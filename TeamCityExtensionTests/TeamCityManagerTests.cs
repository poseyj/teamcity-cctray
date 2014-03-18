using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SirenOfShame.Lib.Device;
using TeamCityExtension;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.Remote;
using UsbLib;

namespace TeamCityExtensionTests
{
    [TestClass]
    public class TeamCityManagerTests
    {
        #region Siren of shame tests

        [TestMethod]
        public void IfAllBuildsGoodStopLights()
        {
            const string baseUrl = "http://server/teamcity";
            const string response1 = "{" +
                                   "'count': 1," +
                                   "'nextHref': '/guestAuth/app/rest/builds?locator=count:100,start:100,branch:(default:any)'," +
                                   "'build': [{" +
                                   "'id': 2052," +
                                   "'number': '811'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/auth'," +
                                   "'startDate': '20140312T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2052'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2052&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";
            const string response2 = "{" +
                        "'count': 0}";

            var mockSiren = MockRepository.GenerateMock<ISirenOfShameDevice>();
            mockSiren.Expect(x => x.TryConnect()).Return(true);
            mockSiren.Expect(x => x.StopLightPattern());

            var responses = new Stack<string>();
            responses.Push(response1);
            responses.Push(response2);
            var httpClient = BuildHttpClient(responses);

            var sosManager = new SirenOfShameManager(mockSiren);

            var client = new TeamCityManager(new BuildServer(baseUrl), httpClient, sosManager);
            var result = client.GetCruiseServerSnapshot();

            Assert.AreEqual(ProjectActivity.Sleeping, result.ProjectStatuses[0].Activity);
            mockSiren.VerifyAllExpectations();
        }

        [TestMethod]
        public void IfAnyBuildsAreRunningStartLightPattern()
        {
            const string baseUrl = "http://server/teamcity";
            const string response1 = "{" +
                                   "'count': 1," +
                                   "'nextHref': '/guestAuth/app/rest/builds?locator=count:100,start:100,branch:(default:any)'," +          
                                   "'build': [{" +
                                   "'id': 2052," +
                                   "'number': '811'," +
                                   "'status': 'FAILURE'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/auth'," +
                                   "'startDate': '20140312T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2052'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2052&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";
            const string response2 = "{" +
                                   "'count': 1," +
                                   "'build': [{" +
                                   "'id': 2088," +
                                   "'number': '247'," +
                                   "'running': 'true'," +
                                   "'percentageComplete': '24'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/auth'," +
                                   "'defaultBranch': 'false'," +
                                   "'startDate': '20140313T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2088'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2088&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";

            var mockSiren = MockRepository.GenerateMock<ISirenOfShameDevice>();
            mockSiren.Expect(x => x.TryConnect()).Return(true);
            mockSiren.Expect(x => x.PlayLightPattern(Arg<LedPattern>.Is.Anything, Arg<TimeSpan>.Is.Anything));

            var responses = new Stack<string>();
            responses.Push(response1);
            responses.Push(response2);
            var httpClient = BuildHttpClient(responses);

            var sosManager = new SirenOfShameManager(mockSiren);

            var client = new TeamCityManager(new BuildServer(baseUrl), httpClient, sosManager);
            var result = client.GetCruiseServerSnapshot();

            Assert.AreEqual(ProjectActivity.Building, result.ProjectStatuses[0].Activity);
            mockSiren.VerifyAllExpectations();
        }

        #endregion

        #region Get running builds

        [TestMethod]
        public void GetExecutingBuilds()
        {
            const string expectedKey = "TestApi_Continuous-[feature/settings]";
            const string baseUrl = "http://server/teamcity";
            const string content = "{" +
                                   "'count': 1," +
                                   "'build': [{" +
                                   "'id': 2088," +
                                   "'number': '247'," +
                                   "'running': 'true'," +
                                   "'percentageComplete': '14'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/settings'," +
                                   "'defaultBranch': 'false'," +
                                   "'startDate': '20140312T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2088'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2088&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";

            var httpClient = BuildHttpClient(content);
            var client = new TeamCityManager(new BuildServer(baseUrl), httpClient);
            var result = client.GetExecutingBuilds();

            Assert.AreEqual(1, result.Keys.Count);
            Assert.IsTrue(result.ContainsKey(expectedKey));
            Assert.AreEqual(2088, result[expectedKey].id);
            Assert.AreEqual("247", result[expectedKey].number);
            Assert.AreEqual("14", result[expectedKey].percentageComplete);
            Assert.AreEqual("SUCCESS", result[expectedKey].status);
            Assert.AreEqual(BuildStatus.Success, result[expectedKey].buildStatus);
            Assert.AreEqual("feature/settings", result[expectedKey].branchName);
            Assert.AreEqual("https://server/teamcity/viewLog.html?buildId=2088&buildTypeId=TestApi_Continuous", result[expectedKey].webUrl);
        }

        #endregion

        #region Get snapshot tests

        [TestMethod]
        public void GetSnapshotWithNoRunningBuilds()
        {
            const string baseUrl = "http://server/teamcity";
            const string response1 = "{" +
                                   "'count': 1," +
                                   "'nextHref': '/guestAuth/app/rest/builds?locator=count:100,start:100,branch:(default:any)'," +
                                   "'build': [{" +
                                   "'id': 2051," +
                                   "'number': '810'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/settings'," +
                                   "'startDate': '20140311T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2051'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2051&buildTypeId=TestApi_Continuous'" +
                                   "}]," +
                                   "'build': [{" +
                                   "'id': 2052," +
                                   "'number': '811'," +
                                   "'status': 'FAILURE'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/auth'," +
                                   "'startDate': '20140312T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2052'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2052&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";
            const string response2 = "{'count': 0}";

            var responses = new Stack<string>();
            responses.Push(response1);
            responses.Push(response2);
            var httpClient = BuildHttpClient(responses);

            var client = new TeamCityManager(new BuildServer(baseUrl), httpClient);
            var result = client.GetCruiseServerSnapshot();

            Assert.AreEqual(2, result.ProjectStatuses.Length);
        }

        [TestMethod]
        public void GetSnapshotWithRunningBuild()
        {
            const string baseUrl = "http://server/teamcity";
            const string response1 = "{" +
                                   "'count': 1," +
                                   "'nextHref': '/guestAuth/app/rest/builds?locator=count:100,start:100,branch:(default:any)'," +
                                   "'build': [{" +
                                   "'id': 2051," +
                                   "'number': '810'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/settings'," +
                                   "'startDate': '20140311T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2051'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2051&buildTypeId=TestApi_Continuous'" +
                                   "}]," +
                                   "'build': [{" +
                                   "'id': 2052," +
                                   "'number': '811'," +
                                   "'status': 'FAILURE'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/auth'," +
                                   "'startDate': '20140312T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2052'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2052&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";
            const string response2 = "{" +
                                   "'count': 1," +
                                   "'build': [{" +
                                   "'id': 2088," +
                                   "'number': '247'," +
                                   "'running': 'true'," +
                                   "'percentageComplete': '24'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/auth'," +
                                   "'defaultBranch': 'false'," +
                                   "'startDate': '20140313T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2088'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2088&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";

            var responses = new Stack<string>();
            responses.Push(response1);
            responses.Push(response2);
            var httpClient = BuildHttpClient(responses);

            var client = new TeamCityManager(new BuildServer(baseUrl), httpClient);
            var result = client.GetCruiseServerSnapshot();
            
            Assert.AreEqual(2, result.ProjectStatuses.Length);
            Assert.AreEqual("TestApi_Continuous-[feature/auth]", result.ProjectStatuses[0].Name);
            Assert.AreEqual(IntegrationStatus.Failure, result.ProjectStatuses[0].BuildStatus);
            Assert.AreEqual("Failure", result.ProjectStatuses[0].Category);
            Assert.AreEqual("247", result.ProjectStatuses[0].LastBuildLabel);
            Assert.AreEqual("https://server/teamcity/viewLog.html?buildId=2088&buildTypeId=TestApi_Continuous", result.ProjectStatuses[0].WebURL);
            Assert.AreEqual(ProjectActivity.Building, result.ProjectStatuses[0].Activity);
            Assert.AreEqual("Percent Complete: 24", result.ProjectStatuses[0].CurrentMessage);

            Assert.AreEqual("TestApi_Continuous-[feature/settings]", result.ProjectStatuses[1].Name);
            Assert.AreEqual(IntegrationStatus.Success, result.ProjectStatuses[1].BuildStatus);
            Assert.AreEqual("Success", result.ProjectStatuses[1].Category);
            Assert.AreEqual("810", result.ProjectStatuses[1].LastBuildLabel);
            Assert.AreEqual("https://server/teamcity/viewLog.html?buildId=2051&buildTypeId=TestApi_Continuous", result.ProjectStatuses[1].WebURL);
            Assert.AreEqual(ProjectActivity.Sleeping, result.ProjectStatuses[1].Activity);
            Assert.AreEqual("", result.ProjectStatuses[1].CurrentMessage);
        }

        #endregion

        #region Get project list tests

        [TestMethod]
        public void MultipleBuildResultsOnDifferentBranchShouldParseATwoProjects()
        {
            const string baseUrl = "http://server/teamcity";
            const string content = "{" +
                                   "'count': 1," +
                                   "'nextHref': '/guestAuth/app/rest/builds?locator=count:100,start:100,branch:(default:any)'," +
                                   "'build': [{" +
                                   "'id': 2051," +
                                   "'number': '810'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/settings'," +
                                   "'startDate': '20140311T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2051'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2051&buildTypeId=TestApi_Continuous'" +
                                   "}]," +
                                   "'build': [{" +
                                   "'id': 2052," +
                                   "'number': '811'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/auth'," +
                                   "'startDate': '20140312T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2052'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2052&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";

            var httpClient = BuildHttpClient(content);
            var client = new TeamCityManager(new BuildServer(baseUrl), httpClient);
            var result = client.GetProjectList();

            Assert.IsTrue(result.Length == 2);
            Assert.AreEqual("TestApi_Continuous-[feature/auth]", result[0].ProjectName);
            Assert.AreEqual(baseUrl, result[0].ServerUrl);

            Assert.AreEqual("TestApi_Continuous-[feature/settings]", result[1].ProjectName);
            Assert.AreEqual(baseUrl, result[1].ServerUrl);
        }

        [TestMethod]
        public void MultipleBuildResultsOnSameBranchShouldParseASingleProject()
        {
            const string baseUrl = "http://server/teamcity";
            const string content = "{" +
                                   "'count': 1," +
                                   "'nextHref': '/guestAuth/app/rest/builds?locator=count:100,start:100,branch:(default:any)'," +
                                   "'build': [{" +
                                   "'id': 2051," +
                                   "'number': '810'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/settings'," +
                                   "'startDate': '20140311T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2051'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2051&buildTypeId=TestApi_Continuous'" +
                                   "}]," +
                                   "'build': [{" +
                                   "'id': 2052," +
                                   "'number': '811'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/settings'," +
                                   "'startDate': '20140312T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2052'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2052&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";

            var httpClient = BuildHttpClient(content);
            var client = new TeamCityManager(new BuildServer(baseUrl), httpClient);
            var result = client.GetProjectList();

            Assert.IsTrue(result.Length == 1);
            Assert.AreEqual("TestApi_Continuous-[feature/settings]", result[0].ProjectName);
            Assert.AreEqual(baseUrl, result[0].ServerUrl);
        }

        [TestMethod]
        public void SingleBuildResultShouldParseASingleProject()
        {
            const string baseUrl = "http://server/teamcity";
            const string content = "{" +
                                   "'count': 1," +
                                   "'nextHref': '/guestAuth/app/rest/builds?locator=count:100,start:100,branch:(default:any)'," +
                                   "'build': [{" +
                                   "'id': 2051," +
                                   "'number': '810'," +
                                   "'status': 'SUCCESS'," +
                                   "'buildTypeId': 'TestApi_Continuous'," +
                                   "'branchName': 'feature/settings'," +
                                   "'startDate': '20140311T170452+0000'," +
                                   "'href': '/guestAuth/app/rest/builds/id:2051'," +
                                   "'webUrl': 'https://server/teamcity/viewLog.html?buildId=2051&buildTypeId=TestApi_Continuous'" +
                                   "}]" +
                                   "}";

            var httpClient = BuildHttpClient(content);
            var client = new TeamCityManager(new BuildServer(baseUrl), httpClient);
            var result = client.GetProjectList();

            Assert.IsTrue(result.Length == 1);
            Assert.AreEqual("TestApi_Continuous-[feature/settings]", result[0].ProjectName);
            Assert.AreEqual(baseUrl, result[0].ServerUrl);
        }

        #endregion

        #region Fake handler tests

        [TestMethod]
        public void FakeHandlerTestSingleCall()
        {
            var content1 = "message1";

            var httpClient = BuildHttpClient(content1);
            var result1 = httpClient.GetStringAsync("http://server/request1");
            Assert.AreEqual(content1, result1.Result);
        }

        [TestMethod]
        public void FakeHandlerTestMultipleCalls()
        {
            var content1 = "message1";
            var content2 = "message2";

            var content = new Stack<string>();
            content.Push(content1);
            content.Push(content2);

            var httpClient = BuildHttpClient(content);
            var result1 = httpClient.GetStringAsync("http://server/request1");
            Assert.AreEqual(content1, result1.Result);

            var result2 = httpClient.GetStringAsync("http://server/request2");
            Assert.AreEqual(content2, result2.Result);
        }

        #endregion

        #region Private helpers

        private static HttpClient BuildHttpClient(IEnumerable<string> content)
        {
            var responses = new Stack<HttpResponseMessage>();
            foreach (var item in content)
            {
                responses.Push(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(item)
                });                
            }

            var httpClient = new HttpClient(new FakeHandler
            {
                Response = responses,
                InnerHandler = new HttpClientHandler()
            });
            return httpClient;
        }

        private static HttpClient BuildHttpClient(string content)
        {
            var response = new Stack<HttpResponseMessage>();
            response.Push(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
            
            var httpClient = new HttpClient(new FakeHandler
            {
                Response = response,
                InnerHandler = new HttpClientHandler()
            });
            return httpClient;
        }

        #endregion
    }
}
