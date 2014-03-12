using System.Collections.Generic;

namespace TeamCityExtension
{
    public class TcBuildResponse
    {
        public int count { get; set; }
        public string nextHref { get; set; }
        public IList<TcBuild> build { get; set; }
    }
}