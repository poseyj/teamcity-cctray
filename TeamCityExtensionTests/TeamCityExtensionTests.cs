using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TeamCityExtensionTests
    {
        [TestMethod]
        public void FormatBaseUrlShouldNotModifyUrlWhenNoSlash()
        {
            const string url = "https://server.net/teamcity";

            var extension = new TeamCityExtension.TeamCityExtension();
            var formattedUrl = extension.FormatBaseUrl(url);

            Assert.AreEqual(url, formattedUrl);
        }

        [TestMethod]
        public void FormatBaseUrlShouldRemoveTrailingSlash()
        {
            const string url = "https://server.net/teamcity/";
            const string expected = "https://server.net/teamcity";

            var extension = new TeamCityExtension.TeamCityExtension();
            var formattedUrl = extension.FormatBaseUrl(url);

            Assert.AreEqual(expected, formattedUrl);
        }
    }
}