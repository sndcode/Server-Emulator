using System;

namespace ampedServerEmu
{
	// Token: 0x02000004 RID: 4
	public class ServerEmuEventArgs : EventArgs
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002060 File Offset: 0x00000260
		public ServerEmuEventArgs(string cEvent)
		{
			this.currentEvent = cEvent;
		}

		// Token: 0x17000001 RID: 1
		public string CurrentEvent
		{
			// Token: 0x06000004 RID: 4 RVA: 0x00002078 File Offset: 0x00000278
			get
			{
				return this.currentEvent;
			}
			// Token: 0x06000003 RID: 3 RVA: 0x0000206F File Offset: 0x0000026F
			set
			{
				this.currentEvent = value;
			}
		}

		// Token: 0x04000004 RID: 4
		private string currentEvent;
	}
}
