using System.Windows.Forms;

namespace TeamCityExtension
{
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
        }

        public string BaseUrl
        {
            get { return baseUrl.Text; }
            set { baseUrl.Text = value; }
        }
    }
}
