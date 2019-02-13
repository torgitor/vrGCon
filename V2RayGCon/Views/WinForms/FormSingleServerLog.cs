﻿using System;
using System.Windows.Forms;
using V2RayGCon.Resource.Resx;

namespace V2RayGCon.Views.WinForms
{
    public partial class FormSingleServerLog : Form
    {
        long updateTimestamp = -1;
        VgcApis.Libs.Tasks.Routine logUpdater;
        VgcApis.Libs.Sys.QueueLogger qLogger;
        VgcApis.Libs.Views.RepaintCtrl repaintCtrl;

        public FormSingleServerLog(
            string title,
            VgcApis.Libs.Sys.QueueLogger logger)
        {
            this.qLogger = logger;
            logUpdater = new VgcApis.Libs.Tasks.Routine(
                RefreshUi,
                VgcApis.Models.Consts.Intervals.SiFormLogRefreshInterval);

            InitializeComponent();
            VgcApis.Libs.UI.AutoSetFormIcon(this);
            this.Text = I18N.Log + " - " + title;
        }

        private void RefreshUi()
        {
            var timestamp = qLogger.GetTimestamp();

            if (updateTimestamp == timestamp)
            {
                return;
            }

            updateTimestamp = timestamp;
            VgcApis.Libs.UI.RunInUiThread(this, UpdateLogBox);
        }

        void UpdateLogBox()
        {
            repaintCtrl.Disable();
            rtBoxLogger.Text = qLogger.GetLogAsString(true);
            rtBoxLogger.SelectionStart = rtBoxLogger.Text.Length;
            rtBoxLogger.ScrollToCaret();
            repaintCtrl.Enable();
        }

        private void FormSingleServerLog_Load(object sender, EventArgs e)
        {
            logUpdater.Run();
            repaintCtrl = new VgcApis.Libs.Views.RepaintCtrl(rtBoxLogger);
        }

        private void FormSingleServerLog_FormClosed(object sender, FormClosedEventArgs e)
        {
            logUpdater.Dispose();
            qLogger.Dispose();
        }
    }
}
