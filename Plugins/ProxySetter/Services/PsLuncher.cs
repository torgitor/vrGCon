using ProxySetter.Resources.Langs;
using System;
using System.Windows.Forms;
using VgcApis.Libs.Sys;

namespace ProxySetter.Services
{
    class PsLuncher
    {
        PsSettings setting;
        PacServer pacServer;
        ServerTracker serverTracker;

        VgcApis.Models.IServices.IApiService vgcApi;
        Model.Data.ProxySettings orgSysProxySetting;
        Views.WinForms.FormMain formMain;

        ToolStripMenuItem[] subMenuItemCache = null;


        public PsLuncher() { }

        #region public methods

        public void Run(VgcApis.Models.IServices.IApiService api)
        {
            orgSysProxySetting = Lib.Sys.ProxySetter.GetProxySetting();
            FileLogger.Info("ProxySetter: save sys proxy settings");

            this.vgcApi = api;

            var vgcSetting = api.GetSettingService();
            var vgcServer = api.GetServersService();
            var vgcNotifier = api.GetNotifierService();

            pacServer = new PacServer();
            setting = new PsSettings();
            serverTracker = new ServerTracker();

            // dependency injection
            setting.Run(vgcSetting);
            pacServer.Run(setting);

            serverTracker.OnSysProxyChanged += UpdateMenuItemCheckedStatHandler;
            serverTracker.Run(setting, pacServer, vgcServer, vgcNotifier);
        }

        public void Show()
        {
            if (formMain != null)
            {
                return;
            }

            formMain = new Views.WinForms.FormMain(
                setting,
                pacServer,
                serverTracker);
            formMain.FormClosed += (s, a) => formMain = null;
            formMain.Show();
        }

        public void Cleanup()
        {
            setting.DebugLog("call Luncher.cleanup");
            setting.isCleaning = true;

            serverTracker.OnSysProxyChanged -= UpdateMenuItemCheckedStatHandler;
            formMain?.Close();
            serverTracker.Cleanup();
            pacServer.Cleanup();
            setting.Cleanup();
            Lib.Sys.ProxySetter.UpdateProxySettingOnDemand(orgSysProxySetting);
            FileLogger.Info("ProxySetter: restore sys proxy settings");

        }

        public ToolStripMenuItem[] GetSubMenu() => GenSubMenu();
        #endregion

        #region private methods
        ToolStripMenuItem miProxyModeDirect = null;
        ToolStripMenuItem miProxyModeGlobal = null;
        ToolStripMenuItem miProxyModePac = null;

        ToolStripMenuItem[] GenSubMenu()
        {
            if (subMenuItemCache != null)
            {
                return subMenuItemCache;
            }

            miProxyModePac = new ToolStripMenuItem(
                I18N.PAC, null, (s, a) => SetProxyMode(Model.Data.Enum.SystemProxyModes.PAC));
            miProxyModeDirect = new ToolStripMenuItem(
                I18N.Direct, null, (s, a) => SetProxyMode(Model.Data.Enum.SystemProxyModes.Direct));
            miProxyModeGlobal = new ToolStripMenuItem(
                I18N.Global, null, (s, a) => SetProxyMode(Model.Data.Enum.SystemProxyModes.Global));

            subMenuItemCache = new ToolStripMenuItem[] {
                miProxyModeDirect,
                miProxyModeGlobal,
                miProxyModePac,
            };

            UpdateMenuItemCheckedStatHandler(this, EventArgs.Empty);

            return subMenuItemCache;
        }

        void UpdateMenuItemCheckedStatHandler(
            object sender, EventArgs events)
        {
            var bs = setting.GetBasicSetting();
            var pm = bs.sysProxyMode;

            if (miProxyModeDirect != null)
            {
                miProxyModeDirect.Checked =
                    (int)Model.Data.Enum.SystemProxyModes.Direct == pm;
            }

            if (miProxyModeGlobal != null)
            {
                miProxyModeGlobal.Checked =
                    (int)Model.Data.Enum.SystemProxyModes.Global == pm;
            }

            if (miProxyModePac != null)
            {
                miProxyModePac.Checked =
                    (int)Model.Data.Enum.SystemProxyModes.PAC == pm;
            }
        }

        void SetProxyMode(Model.Data.Enum.SystemProxyModes proxyMode)
        {
            var bs = setting.GetBasicSetting();
            bs.sysProxyMode = (int)proxyMode;
            setting.SaveBasicSetting(bs);
            serverTracker?.Restart();
        }

        #endregion
    }

}
