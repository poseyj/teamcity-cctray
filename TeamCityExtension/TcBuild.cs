namespace TeamCityExtension
{
    public class TcBuild
    {
        public int id { get; set; }
        public string number { get; set; }
        public string status { get; set; }
        public string branchName { get; set; }
        public string buildTypeId { get; set; }
        public string percentageComplete { get; set; }
        public string webUrl { get; set; }

        public BuildStatus buildStatus
        {
            get
            {
                if (status == "SUCCESS")
                    return BuildStatus.Success;
                else if (status == "FAILURE")
                    return BuildStatus.Failure;
                else
                    return BuildStatus.Unknown;
            }
        }
    }
}