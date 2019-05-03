using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ywTool
{
    public partial class ywTool : Form
    {
        public ywTool()
        {
            InitializeComponent();
        }

        public void UpdateUI(AppState appstate)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.Invoke((MethodInvoker)delegate { lblLogonUser.Visible = GlobalSettings.AppState.isLogon; });
                this.Invoke((MethodInvoker)delegate { lblUserName.Visible = GlobalSettings.AppState.isLogon; });
                this.Invoke((MethodInvoker)delegate { lblUserName.Text = GlobalSettings.AppState.isLogon ? GlobalSettings.AppState.User.UserName : ""; });
                this.Invoke((MethodInvoker)delegate { btnCancel.Visible = false; });
                btnSend.Enabled = !GlobalSettings.AppState.isProcessingCommand;
                this.ControlBox = !GlobalSettings.AppState.isProcessingCommand;
                wbMessage.Navigate("about:blank");
                wbMessage.Document.OpenNew(false);
                wbMessage.Document.Write(GlobalSettings.GetMessage());
                wbMessage.Refresh();
                if (wbMessage.Document != null)
                    wbMessage.Document.Window.ScrollTo(0, wbMessage.Document.Body.ScrollRectangle.Height);
                if (GlobalSettings.AppState.isProcessingCommand == false)
                    txtCmd.Select();
            });
        }

        private void ywTool_Load(object sender, EventArgs e)
        {
            if (GlobalSettings.LogoImage != null) { pictureBox1.Image = GlobalSettings.LogoImage; }
            UpdateUI(GlobalSettings.AppState);
            GlobalSettings.AppState.SetObserver(this);
            //txtCmd.Text = "command runbatch -i test.bat";
            txtCmd.Text = "Command RunBatch -i chart.bat";
        }

        private void FrozenMe()
        {
            txtCmd.Text = "";
            btnSend.Enabled = false;
            btnCancel.Enabled = false;
            this.ControlBox = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!btnSend.Enabled)
                return;
            if (string.IsNullOrWhiteSpace(txtCmd.Text))
                return;
            if (txtCmd.Text.ToLower() == "quit" || txtCmd.Text.ToLower() == "exit")
                this.Close();
            GlobalSettings.CmdQueue.Enqueue(txtCmd.Text);
            FrozenMe();
        }

        private void txtCmd_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && !string.IsNullOrWhiteSpace(txtCmd.Text))
            {
                Objects.CmdRouter.SetCmdCursor();
                btnSend_Click(sender, null);
            }
            else if (e.KeyCode == Keys.Up)
            {
                txtCmd.Text = Objects.CmdRouter.GetPreviousCmdFromHistory() ?? txtCmd.Text;
            }
            else if (e.KeyCode == Keys.Down)
            {
                txtCmd.Text = Objects.CmdRouter.GetNextCmdFromHistory() ?? txtCmd.Text;
            }
        }
    }
}
