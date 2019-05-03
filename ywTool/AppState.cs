using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool
{
    public class AppState
    {
        public AppState()
        {
            User = null;
            ToolInitialized = false;
            isProcessingCommand = false;
        }
        public bool isProcessingCommand { get; set; }
        public bool isLogon
        {
            get
            {
                return User != null;
            }
        }
        public bool ToolInitialized { get; set; }

        public CurrentUser User { get; set; }


        private ywTool Observer;

        public void SetObserver(ywTool c)
        {
            Observer = c;
        }
        public void NotifyObserver()
        {
            if (Observer != null)
                Observer.UpdateUI(this);
        }
    }
}
