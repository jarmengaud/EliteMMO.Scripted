using System.Windows.Forms;
using EliteMMO.API;

namespace EliteMMO.Scripted.Views
{
    public partial class ScriptHealing : UserControl
    {
        public ScriptHealing(EliteAPI core)
        {
            InitializeComponent();
            api = core;
        }
    }
}