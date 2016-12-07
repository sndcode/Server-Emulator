using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ampedServerEmu;

namespace emulator
{
	// Token: 0x02000002 RID: 2
	public partial class frmMain : Form
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public frmMain()
		{
			this.InitializeComponent();
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002110 File Offset: 0x00000310
		private void Form1_Load(object sender, EventArgs e)
		{
            this.serverEmu_0 = new ServerEmu(EmuTarget.TheNoobBot, "pstmerger.4team.biz", false, false, new string[]
            {
                "/script.php?r=",
                "?botOnline=true"
            }, 2015, false, null);

            //this.serverEmu_0 = new ServerEmu(EmuTarget.TheNoobBot, "pstmerger.4team.biz", false, false, new string[] { "thenoobbot.de" } , 2015, false, null);
			this.serverEmu_0.EventLogger += new ServerEmu.EventLoggerHandler(this.method_0);
			base.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000205E File Offset: 0x0000025E
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.serverEmu_0.stop();
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002188 File Offset: 0x00000388
		private void method_0(object sender, ServerEmuEventArgs e)
		{
			if (base.InvokeRequired)
			{
				base.Invoke(new Action<object, ServerEmuEventArgs>(this.method_0), new object[]
				{
					sender,
					e
				});
				return;
			}
			this.listBox1.Items.Add(e.CurrentEvent);
			this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000021F8 File Offset: 0x000003F8
		private void button1_Click(object sender, EventArgs e)
		{
			this.serverEmu_0.isDetailed = this.checkBox1.Checked;
			if (!this.serverEmu_0.running)
			{
				this.serverEmu_0.start(5050, 100, false);
				this.button1.Text = ".stop server.";
				return;
			}
			this.serverEmu_0.stop();
			this.button1.Text = ".start server.";
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002076 File Offset: 0x00000276
		private void button2_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000207E File Offset: 0x0000027E
		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			this.serverEmu_0.isDetailed = this.checkBox1.Checked;
		}

		// Token: 0x04000001 RID: 1
		private ServerEmu serverEmu_0;

        private void button3_Click(object sender, EventArgs e)
        {
            
        }
    }
}
