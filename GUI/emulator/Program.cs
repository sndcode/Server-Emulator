using System;
using System.Windows.Forms;

namespace emulator
{
	// Token: 0x02000004 RID: 4
	internal static class Class0
	{
		// Token: 0x0600000B RID: 11 RVA: 0x000020BD File Offset: 0x000002BD
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new frmMain());
		}
	}
}
