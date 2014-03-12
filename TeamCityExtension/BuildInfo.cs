using System;

namespace TeamCityExtension
{
    public enum BuildStatus
    {
        Success,
        Failure,
        Unknown
    }

    internal class BuildInfo
    {
        public string Project { get; set; }
        public BuildStatus Status { get; set; }
        public DateTimeOffset Date { get; set; }
        public string BuildName { get; set; }
        public string Branch { get; set; }
        public string Summary { get; set; }
        public string Title { get; set; }
        public int Label { get; set; }
    }
}