using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Fiddler;

namespace ampedServerEmu
{
	// Token: 0x02000005 RID: 5
	public class ServerEmu
	{
		// Token: 0x06000005 RID: 5 RVA: 0x0000210C File Offset: 0x0000030C
		public ServerEmu(EmuTarget setTargetToEmu, string hostToIntercept, bool isLogDetailed = default(bool), bool shouldLogFiddler = default(bool), string[] specificUrlsToSkip = null, int fiddlerPortToUse = default(int), bool shouldSaveSessionLog = default(bool), string sessionLogFilename = null)
		{
			this.emuTarget_0 = setTargetToEmu;
			this.setHostToIntercept = hostToIntercept;
			this.isDetailed = isLogDetailed;
			this.bool_2 = shouldLogFiddler;
			if (specificUrlsToSkip.Length > 0)
			{
				this.setSkipSpecificUrls = specificUrlsToSkip;
			}
			if (fiddlerPortToUse > -1)
			{
				this.setFiddlerPort = fiddlerPortToUse;
			}
			if (shouldSaveSessionLog)
			{
				if (sessionLogFilename.Length > 0)
				{
					this.setSaveSessionLog = new Tuple<bool, string>(shouldSaveSessionLog, sessionLogFilename);
					return;
				}
				this.setSaveSessionLog = new Tuple<bool, string>(shouldSaveSessionLog, this.string_7);
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002218 File Offset: 0x00000418
		private string method_0(Exception exception_0)
		{
			StackTrace stackTrace = new StackTrace(exception_0);
			StackFrame frame = stackTrace.GetFrame(0);
			MethodBase method = frame.GetMethod();
			return method.DeclaringType.FullName + "." + method.Name;
		}

		// Token: 0x14000001 RID: 1
		public event ServerEmu.EventLoggerHandler EventLogger
		{
			// Token: 0x06000007 RID: 7 RVA: 0x00002258 File Offset: 0x00000458
			add
			{
				ServerEmu.EventLoggerHandler eventLoggerHandler = this.eventLoggerHandler_0;
				ServerEmu.EventLoggerHandler eventLoggerHandler2;
				do
				{
					eventLoggerHandler2 = eventLoggerHandler;
					ServerEmu.EventLoggerHandler value2 = (ServerEmu.EventLoggerHandler)Delegate.Combine(eventLoggerHandler2, value);
					eventLoggerHandler = Interlocked.CompareExchange<ServerEmu.EventLoggerHandler>(ref this.eventLoggerHandler_0, value2, eventLoggerHandler2);
				}
				while (eventLoggerHandler != eventLoggerHandler2);
			}
			// Token: 0x06000008 RID: 8 RVA: 0x00002290 File Offset: 0x00000490
			remove
			{
				ServerEmu.EventLoggerHandler eventLoggerHandler = this.eventLoggerHandler_0;
				ServerEmu.EventLoggerHandler eventLoggerHandler2;
				do
				{
					eventLoggerHandler2 = eventLoggerHandler;
					ServerEmu.EventLoggerHandler value2 = (ServerEmu.EventLoggerHandler)Delegate.Remove(eventLoggerHandler2, value);
					eventLoggerHandler = Interlocked.CompareExchange<ServerEmu.EventLoggerHandler>(ref this.eventLoggerHandler_0, value2, eventLoggerHandler2);
				}
				while (eventLoggerHandler != eventLoggerHandler2);
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000022C8 File Offset: 0x000004C8
		private string method_1()
		{
			WebRequest webRequest = WebRequest.Create("http://icanhazip.com/");
			WebResponse response = webRequest.GetResponse();
			StreamReader streamReader = new StreamReader(response.GetResponseStream());
			return streamReader.ReadToEnd().Trim();
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002300 File Offset: 0x00000500
		private string method_2(byte[] byte_0)
		{
			string text = "";
			for (int i = 0; i < byte_0.Length; i++)
			{
				byte b = byte_0[i];
				text += b.ToString("X2");
			}
			return text;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000233C File Offset: 0x0000053C
		private string method_3(string string_11)
		{
			byte[] array = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(string_11));
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}
        

        //      this.method_4(socket_1, Encoding.UTF8.GetBytes("Lifetime Subscription"), "Life Time", " 200 OK", "text/html");
        private void method_4(Socket socket_1, byte[] byte_0, string string_11, string string_12, string string_13)
		{
			try
			{
				byte[] bytes = this.encoding_0.GetBytes(string.Concat(new string[]
				{
					"HTTP/1.1",
					string_12,
					"\r\nServer: AMPED ServerEmu\r\nAccept-Ranges: bytes\r\nretn: ",
					string_11,
					"\r\nContent-Length: ",
					byte_0.Length.ToString(),
					"\r\nContent-Type: ",
					string_13,
					"\r\n\r\n"
				}));
				socket_1.Send(bytes);
				socket_1.Send(byte_0);
				socket_1.Close();
				if (this.bool_0)
				{
					this.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new string[]
					{
						"[#",
						DateTime.Now.ToShortTimeString(),
						"# ServerEmu] Custom AuthResponse Sent: ",
						Environment.NewLine,
						"retn: ",
						string_11,
						Environment.NewLine,
						"Content: ",
						Encoding.UTF8.GetString(byte_0)
					}));
					this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
					if (this.bool_1)
					{
						if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
						{
							File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
						else
						{
							File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new string[]
				{
					"[#",
					DateTime.Now.ToShortTimeString(),
					"# ServerEmu] Error: ",
					MethodBase.GetCurrentMethod().Name,
					Environment.NewLine,
					ex.Message
				}));
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002624 File Offset: 0x00000824
		private void HandleStuff(string string_11, Socket socket_1)
		{
			string text = this.method_1();
			if (this.bool_0)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] RequestString: " + string_11.Substring(0, string_11.IndexOf("HTTP/1.1")));
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
			}

            ///
            //Handle request: 
            
            
            if (string_11.StartsWith("GET /_activate.asp"))
            {
                this.method_18(socket_1, Encoding.UTF8.GetBytes("0"), "0");
                //this.method_4(socket_1, Encoding.UTF8.GetBytes("Lifetime Subscription"), "Life Time", " 200 OK", "text/html");

                /*
                byte[] bytes = this.encoding_0.GetBytes(string.Concat(new string[]
                {
                    "HTTP / 1.1 200 OK",
                    "Cache - Control: private",
                    "Content-Length: 1",
                    "Content-Type: text/html",
                    "Expires: Tue, 03 Jan 2017 10:50:54 GMT",
                    "Server: Microsoft-IIS/8.5",
                    "Set-Cookie: settings=server%5Flanguage=English; expires=Tue, 03 Jan 2017 10:50:54 GMT; domain=.4team.biz; path=/",
                    "Set-Cookie: ASPSESSIONIDSSRBRABS=HIOEEFIBHHKBNMMKOPLLDCHD; path=/",
                    "Date: Tue, 06 Dec 2016 10:50:54 GMT",
                    "0"

                }));
                socket_1.Send(bytes);
                socket_1.Close();
                this.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new object[]
                {
                    "[#",
                    DateTime.Now.ToShortTimeString(),
                    "# ServerEmu] Sent custom html"
                }));
                */
                
                /*
                HTTP / 1.1 200 OK
                Cache - Control: private
                Content-Length: 1
                Content-Type: text/html
                Expires: Tue, 06 Dec 2016 10:50:54 GMT
                Server: Microsoft-IIS/8.5
                Set-Cookie: settings=server%5Flanguage=English; expires=Wed, 07-Dec-2016 05:00:00 GMT; domain=.4team.biz; path=/
                Set-Cookie: ASPSESSIONIDSSRBRABS=HIOEEFIBHHKBNMMKOPLLDCHD; path=/
                Date: Tue, 06 Dec 2016 10:50:54 GMT

                2
                */

            }
            ///

            if (string_11.StartsWith("GET /isOnline.php"))
			{
				this.method_18(socket_1, Encoding.UTF8.GetBytes("true"), "text/html");
			}
			if (string_11.StartsWith("GET /myIp.php"))
			{
				this.method_18(socket_1, Encoding.UTF8.GetBytes(text), "text/html");
				if (this.bool_0)
				{
					this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] MyIP: " + text);
					this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
					if (this.bool_1)
					{
						if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
						{
							File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
						else
						{
							File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
					}
				}
			}
			if (string_11.StartsWith("GET /auth.php?TimeSubscription=true"))
			{
				this.method_4(socket_1, Encoding.UTF8.GetBytes("Lifetime Subscription"), "Life Time", " 200 OK", "text/html");
			}
			if (string_11.StartsWith("GET /auth.php?"))
			{
				if (string_11.StartsWith("GET /auth.php?create=true"))
				{
					string text2 = string_11.Substring(38);
					this.string_2 = text2.Substring(0, text2.IndexOf("HTTP/1.1")).Trim();
					if (this.bool_0)
					{
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] HardwareID: " + this.string_2);
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
					}
					this.string_3 = Encoding.UTF8.GetString(Convert.FromBase64String(text2.Split(new string[]
					{
						Environment.NewLine
					}, StringSplitOptions.RemoveEmptyEntries)[1].Replace("Authorization: Basic ", "")));
					if (this.bool_0)
					{
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] User: " + this.string_3.Substring(0, this.string_3.IndexOf(':')));
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
					}
					this.string_4 = "0e8897c8c73772e72d81dd28ebf57ac3" + text + this.string_3.Substring(0, this.string_3.IndexOf(':')) + this.string_2.Trim();
					this.string_5 = this.method_3(this.string_4);
					if (this.bool_0)
					{
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] KeyToSendBack: " + this.string_4);
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
					}
					this.method_4(socket_1, Encoding.UTF8.GetBytes("OKConnect"), this.string_5, " 200 OK", "text/html");
				}
				if (string_11.StartsWith("GET /auth.php?random=true"))
				{
					string text2 = string_11.Substring(36);
					if (this.bool_0)
					{
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Random: " + text2.Split(new string[]
						{
							Environment.NewLine
						}, StringSplitOptions.RemoveEmptyEntries)[0].Replace("Authorization: Basic ", ""));
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
					}
					int num = int.Parse(Encoding.ASCII.GetString(Convert.FromBase64String(text2.Split(new string[]
					{
						Environment.NewLine
					}, StringSplitOptions.RemoveEmptyEntries)[0].Replace("Authorization: Basic ", ""))).Split(new char[]
					{
						':'
					})[1]);
					string text3 = this.method_3(num * 4 + "0e8897c8c73772e72d81dd28ebf57ac3");
					if (this.bool_0)
					{
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Random Num Response: " + text3);
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
					}
					this.method_4(socket_1, Encoding.UTF8.GetBytes("CRACKED BY MAMMON"), text3, " 200 OK", "text/html");
				}
				if (string_11.StartsWith("GET /auth.php?level="))
				{
					if (this.bool_0)
					{
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Secondary AuthCheck!");
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
					}
					this.method_4(socket_1, Encoding.UTF8.GetBytes("OKConnect"), this.string_5, " 200 OK", "text/html");
				}
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002EB0 File Offset: 0x000010B0
		private void method_6(string string_11, Socket socket_1)
		{
			string_11 = string_11.Replace("/Validate.aspx?", "").Substring(4);
			string text = this.method_8(string_11.Substring(0, string_11.IndexOf("&em=")).Replace("pv=", "").Replace("&em=", ""));
			string_11 = string_11.Remove(0, string_11.IndexOf("&em="));
			text += this.method_8(string_11.Substring(0, string_11.IndexOf("&vk=")).Replace("&em=", "").Replace("&vk=", ""));
			string_11 = string_11.Remove(0, string_11.IndexOf("&vk="));
			text += this.method_8(string_11.Substring(0, string_11.IndexOf("&sg=")).Replace("&vk=", "").Replace("&sg=", ""));
			string_11 = string_11.Remove(0, string_11.IndexOf("&sg="));
			text += this.method_8(string_11.Substring(0, string_11.IndexOf("&nn=")).Replace("&sg=", "").Replace("&nn=", ""));
			string_11 = string_11.Remove(0, string_11.IndexOf("&nn="));
			string s = this.method_7("Validation Success_" + this.method_7(text));
			this.method_18(socket_1, Encoding.ASCII.GetBytes(s), "text/html");
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00003054 File Offset: 0x00001254
		private string method_7(string string_11)
		{
			byte[] rgbKey = new byte[]
			{
				116,
				114,
				117,
				101,
				71,
				82,
				73,
				84
			};
			Rijndael rijndael = Rijndael.Create();
			rijndael.Mode = CipherMode.ECB;
			string result;
			try
			{
				byte[] bytes = Encoding.ASCII.GetBytes(string_11);
				result = Convert.ToBase64String(rijndael.CreateEncryptor(rgbKey, null).TransformFinalBlock(bytes, 0, bytes.Length));
			}
			catch (Exception ex)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new object[]
				{
					"[#",
					DateTime.Now.ToShortTimeString(),
					"# ServerEmu] Error: ",
					ex.TargetSite,
					Environment.NewLine,
					ex.Message
				}));
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				result = "";
			}
			return result;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00003130 File Offset: 0x00001330
		private string method_8(string string_11)
		{
			byte[] rgbKey = new byte[]
			{
				116,
				114,
				117,
				101,
				71,
				82,
				73,
				84
			};
			if (string_11.Length == 0)
			{
				return string.Empty;
			}
			Rijndael rijndael = Rijndael.Create();
			rijndael.Mode = CipherMode.ECB;
			string result;
			try
			{
				byte[] array = Convert.FromBase64String(HttpUtility.UrlDecode(string_11));
				result = Encoding.ASCII.GetString(rijndael.CreateDecryptor(rgbKey, null).TransformFinalBlock(array, 0, array.Length));
			}
			catch (Exception ex)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new object[]
				{
					"[#",
					DateTime.Now.ToShortTimeString(),
					"# ServerEmu] Error: ",
					ex.TargetSite,
					Environment.NewLine,
					ex.Message
				}));
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				result = "";
			}
			return result;
		}

		// Token: 0x17000002 RID: 2
		public string[] setSkipSpecificUrls
		{
			// Token: 0x06000011 RID: 17 RVA: 0x00002080 File Offset: 0x00000280
			get
			{
				return this.string_6;
			}
			// Token: 0x06000012 RID: 18 RVA: 0x00002088 File Offset: 0x00000288
			set
			{
				this.string_6 = value;
			}
		}

		// Token: 0x17000003 RID: 3
		public bool isDetailed
		{
			// Token: 0x06000013 RID: 19 RVA: 0x00002091 File Offset: 0x00000291
			get
			{
				return this.bool_0;
			}
			// Token: 0x06000014 RID: 20 RVA: 0x00002099 File Offset: 0x00000299
			set
			{
				this.bool_0 = value;
			}
		}

		// Token: 0x17000004 RID: 4
		public int setFiddlerPort
		{
			// Token: 0x06000015 RID: 21 RVA: 0x000020A2 File Offset: 0x000002A2
			get
			{
				return this.int_0;
			}
			// Token: 0x06000016 RID: 22 RVA: 0x000020AA File Offset: 0x000002AA
			set
			{
				this.int_0 = value;
			}
		}

		// Token: 0x17000005 RID: 5
		public Tuple<bool, string> setSaveSessionLog
		{
			// Token: 0x06000017 RID: 23 RVA: 0x000020B3 File Offset: 0x000002B3
			get
			{
				return new Tuple<bool, string>(this.bool_1, this.string_7);
			}
			// Token: 0x06000018 RID: 24 RVA: 0x000020C6 File Offset: 0x000002C6
			set
			{
				this.bool_1 = value.Item1;
				this.string_7 = value.Item2;
			}
		}

		// Token: 0x17000006 RID: 6
		public string setHostToIntercept
		{
			// Token: 0x06000019 RID: 25 RVA: 0x000020E0 File Offset: 0x000002E0
			get
			{
				return this.string_8;
			}
			// Token: 0x0600001A RID: 26 RVA: 0x000020E8 File Offset: 0x000002E8
			set
			{
				this.string_8 = value;
			}
		}

		// Token: 0x17000007 RID: 7
		public bool shouldLogFiddler
		{
			// Token: 0x0600001B RID: 27 RVA: 0x000020F1 File Offset: 0x000002F1
			get
			{
				return this.bool_2;
			}
			// Token: 0x0600001C RID: 28 RVA: 0x000020F9 File Offset: 0x000002F9
			set
			{
				this.bool_2 = value;
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00003220 File Offset: 0x00001420
		public bool start(int port, int maxNOfCon, bool isConsole)
		{
			string string_ = this.string_8;
			if (isConsole)
			{
				Console.Title = "[ServerEmu]";
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("   _____      _____ _____________________________   ");
				Console.WriteLine("  /  _  \\    /     \\\\______   \\_   _____/\\______ \\  ");
				Console.WriteLine(" /  /_\\  \\  /  \\ /  \\|     ___/|    __)_  |    |  \\ ");
				Console.WriteLine("/    |    \\/    Y    \\    |    |        \\ |    `   \\");
				Console.WriteLine("\\____|__  /\\____|__  /____|   /_______  //_______  /");
				Console.WriteLine("        \\/         \\/                 \\/         \\/ ");
				Console.WriteLine();
				Console.WriteLine("              -=( Version " + this.string_0 + " )=-");
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("(c)2015 " + this.string_1);
				Console.WriteLine();
				Console.WriteLine();
			}
			IPAddress address = IPAddress.Parse("127.0.0.1");
			if (this.running)
			{
				return false;
			}
			bool result;
			try
			{
				this.socket_0 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				this.socket_0.Bind(new IPEndPoint(address, port));
				this.socket_0.Listen(1);
				this.socket_0.ReceiveTimeout = this.int_1;
				this.socket_0.SendTimeout = this.int_1;
				this.running = true;
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Server Online: 127.0.0.1:" + port.ToString());
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
				this.method_9(string_, "127.0.0.1:" + port.ToString());
				goto IL_2CA;
			}
			catch (Exception ex)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new string[]
				{
					"[#",
					DateTime.Now.ToShortTimeString(),
					"# ServerEmu] Error: ",
					MethodBase.GetCurrentMethod().Name,
					Environment.NewLine,
					ex.Message
				}));
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
				result = false;
			}
			return result;
			IL_2CA:
			Thread thread = new Thread(new ThreadStart(this.method_21));
			thread.Start();
			this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] RequestListener Started...");
			this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
			if (this.bool_1)
			{
				if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
				{
					File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
				}
				else
				{
					File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
				}
			}
			return true;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000035D8 File Offset: 0x000017D8
		public void stop()
		{
			if (this.running)
			{
				this.running = false;
				try
				{
					this.socket_0.Close();
					this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Server Socket Closed...");
					this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
					if (this.bool_1)
					{
						if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
						{
							File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
						else
						{
							File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
					}
				}
				catch (Exception ex)
				{
					this.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new string[]
					{
						"[#",
						DateTime.Now.ToShortTimeString(),
						"# ServerEmu] Error: ",
						MethodBase.GetCurrentMethod().Name,
						Environment.NewLine,
						ex.Message
					}));
					this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
					if (this.bool_1)
					{
						if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
						{
							File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
						else
						{
							File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
					}
				}
				this.socket_0 = null;
				this.method_14();
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Server Socket has been reset...");
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						return;
					}
					File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
				}
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00003858 File Offset: 0x00001A58
		private void method_9(string string_11, string string_12)
		{
			FiddlerApplication.BeforeRequest += new SessionStateHandler(this.method_13);
			FiddlerApplication.BeforeResponse += new SessionStateHandler(this.method_12);
			FiddlerApplication.AfterSessionComplete += new SessionStateHandler(this.method_10);
			this.string_9 = string_11;
			this.string_10 = string_12;
			CONFIG.IgnoreServerCertErrors = true;
			CONFIG.sHookConnectionNamed = string_11;
			FiddlerApplication.Log.OnLogString += new EventHandler<LogEventArgs>(this.method_11);
			FiddlerApplication.Startup(this.int_0, FiddlerCoreStartupFlags.RegisterAsSystemProxy | FiddlerCoreStartupFlags.MonitorAllConnections | FiddlerCoreStartupFlags.OptimizeThreadPool);
			this.bool_3 = true;
			this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Fiddler2 Started!");
			this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
			if (this.bool_1)
			{
				if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
				{
					File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					return;
				}
				File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002102 File Offset: 0x00000302
		private void method_10(Session session_0)
		{
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000398C File Offset: 0x00001B8C
		private void method_11(object sender, LogEventArgs e)
		{
			if (this.bool_0)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# Fiddler]: " + e.LogString);
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000039E0 File Offset: 0x00001BE0
		private void method_12(Session session_0)
		{
			if (session_0.HostnameIs(this.string_10) && this.bool_0 && this.bool_2)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] InterceptSendBody: " + session_0.GetResponseBodyAsString());
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] InterceptSendHeaders: " + session_0.oResponse.headers.ToString());
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						return;
					}
					File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
				}
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003B80 File Offset: 0x00001D80
		private void method_13(Session session_0)
		{
			if (session_0.HostnameIs(this.string_9))
			{
				if (!this.string_6.Any(new Func<string, bool>(session_0.url.Contains)))
				{
					session_0.bypassGateway = false;
					if (this.bool_0 && this.bool_2)
					{
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Host Intercepted: " + session_0.host);
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
					}
					session_0.host = this.string_10;
					if (this.bool_0 && this.bool_2)
					{
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] InterceptGetBody: " + session_0.GetRequestBodyAsString());
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] InterceptGetHeaders: " + session_0.oRequest.headers.ToString());
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] InterceptGetURL: " + session_0.url);
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
								return;
							}
							File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							return;
						}
					}
				}
				else
				{
					session_0.bypassGateway = true;
					if (this.bool_0 && this.bool_2)
					{
						this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] URL Bypassed: " + session_0.url);
						this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
						if (this.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", session_0.url + Environment.NewLine);
								return;
							}
							File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", session_0.url + Environment.NewLine);
							return;
						}
					}
				}
			}
			else
			{
				session_0.bypassGateway = true;
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003F9C File Offset: 0x0000219C
		private void method_14()
		{
			if (this.bool_3)
			{
				FiddlerApplication.Shutdown();
				this.bool_3 = false;
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Fiddler2 Stopped!");
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						return;
					}
					File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
				}
			}
		}

		// Token: 0x06000025 RID: 37 RVA: 0x0000406C File Offset: 0x0000226C
		private void method_15(Socket socket_1)
		{
			byte[] array = new byte[10240];
			int count = socket_1.Receive(array);
			string @string = this.encoding_0.GetString(array, 0, count);
			if (this.bool_0)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new string[]
				{
					"[#",
					DateTime.Now.ToShortTimeString(),
					"# ServerEmu] RequestSize: ",
					count.ToString(),
					"bytes"
				}));
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
			}
			string text = @string.Substring(0, @string.IndexOf(" "));
			int num = @string.IndexOf(text) + text.Length + 1;
			int length = @string.LastIndexOf("HTTP") - num - 1;
			string str = @string.Substring(num, length);
			if (this.bool_0)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] RequestedURL: " + str);
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
			}
			switch (this.emuTarget_0)
			{
			case EmuTarget.VoiceAttack:
				this.method_6(@string, socket_1);
				return;
			case EmuTarget.TheNoobBot:
				this.HandleStuff(@string, socket_1);
				return;
			default:
				return;
			}
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000042A4 File Offset: 0x000024A4
		private void method_16(Socket socket_1)
		{
			if (this.bool_0)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Response not implemented!");
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
			}
			this.method_19(socket_1, "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>Atasoy Simple Web Server</h2><div>501 - Method Not Implemented</div></body></html>", "501 Not Implemented", "text/html");
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00004380 File Offset: 0x00002580
		private void method_17(Socket socket_1)
		{
			if (this.bool_0)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Response not found!");
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
			}
			this.method_19(socket_1, "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>Atasoy Simple Web Server</h2><div>404 - Not Found</div></body></html>", "404 Not Found", "text/html");
		}

		// Token: 0x06000028 RID: 40 RVA: 0x0000445C File Offset: 0x0000265C
		private void method_18(Socket socket_1, byte[] byte_0, string string_11)
		{
			if (this.bool_0)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Response is sent as OK (Code: 200)");
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
			}
			this.method_20(socket_1, byte_0, " 200 OK", string_11);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00004530 File Offset: 0x00002730
		private void method_19(Socket socket_1, string string_11, string string_12, string string_13)
		{
			byte[] bytes = this.encoding_0.GetBytes(string_11);
			this.method_20(socket_1, bytes, string_12, string_13);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00004558 File Offset: 0x00002758
		private void method_20(Socket socket_1, byte[] byte_0, string string_11, string string_12)
		{
			try
			{
              
				byte[] bytes = this.encoding_0.GetBytes(string.Concat(new string[]
				{
					"HTTP/1.1",
					string_11,
					"\r\nServer: AMPED ServerEmu\r\nContent-Length: ",
					byte_0.Length.ToString(),
					"\r\nAccept-Ranges: bytes\r\nContent-Type: ",
					string_12,
					"\r\n\r\n"
				}));
				socket_1.Send(bytes);
				socket_1.Send(byte_0);
				socket_1.Close();
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Response Sent...");
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
			}
			catch (Exception ex)
			{
				this.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new string[]
				{
					"[#",
					DateTime.Now.ToShortTimeString(),
					"# ServerEmu] Error: ",
					MethodBase.GetCurrentMethod().Name,
					Environment.NewLine,
					ex.Message
				}));
				this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
				if (this.bool_1)
				{
					if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
					{
						File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
					}
				}
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00004780 File Offset: 0x00002980
		[CompilerGenerated]
		private void method_21()
		{
			while (this.running)
			{
				ThreadStart threadStart = null;
				ServerEmu.Class0 @class = new ServerEmu.Class0();
				@class.serverEmu_0 = this;
				try
				{
					@class.socket_0 = this.socket_0.Accept();
					if (threadStart == null)
					{
						threadStart = new ThreadStart(@class.method_0);
					}
					Thread thread = new Thread(threadStart);
					thread.Start();
					this.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] RequestHandler Started...");
					this.eventLoggerHandler_0(this, this.serverEmuEventArgs_0);
					if (this.bool_1)
					{
						if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
						{
							File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
						else
						{
							File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x04000005 RID: 5
		private string string_0 = "0.2.4";

		// Token: 0x04000006 RID: 6
		private string string_1 = "mammon // AMPED";

		// Token: 0x04000007 RID: 7
		private ServerEmu.EventLoggerHandler eventLoggerHandler_0;

		// Token: 0x04000008 RID: 8
		private ServerEmuEventArgs serverEmuEventArgs_0;

		// Token: 0x04000009 RID: 9
		private string string_2 = "";

		// Token: 0x0400000A RID: 10
		private string string_3 = "";

		// Token: 0x0400000B RID: 11
		private string string_4 = "";

		// Token: 0x0400000C RID: 12
		private string string_5 = "";

		// Token: 0x0400000D RID: 13
		private EmuTarget emuTarget_0;

		// Token: 0x0400000E RID: 14
		private string[] string_6 = new string[]
		{
			""
		};

		// Token: 0x0400000F RID: 15
		private bool bool_0;

		// Token: 0x04000010 RID: 16
		private int int_0 = 8877;

		// Token: 0x04000011 RID: 17
		private bool bool_1;

		// Token: 0x04000012 RID: 18
		private string string_7 = "ServerEmu.log";

		// Token: 0x04000013 RID: 19
		private string string_8;

		// Token: 0x04000014 RID: 20
		private bool bool_2;

		// Token: 0x04000015 RID: 21
		public bool running;

		// Token: 0x04000016 RID: 22
		private bool bool_3;

		// Token: 0x04000017 RID: 23
		private int int_1 = 8;

		// Token: 0x04000018 RID: 24
		private Encoding encoding_0 = Encoding.ASCII;

		// Token: 0x04000019 RID: 25
		private Socket socket_0;

		// Token: 0x0400001A RID: 26
		private string string_9;

		// Token: 0x0400001B RID: 27
		private string string_10;

		// Token: 0x02000006 RID: 6
		// Token: 0x0600002D RID: 45
		public delegate void EventLoggerHandler(object sender, ServerEmuEventArgs e);

		// Token: 0x02000007 RID: 7
		[CompilerGenerated]
		private sealed class Class0
		{
			// Token: 0x06000031 RID: 49 RVA: 0x0000489C File Offset: 0x00002A9C
			public void method_0()
			{
				this.socket_0.ReceiveTimeout = this.serverEmu_0.int_1;
				this.socket_0.SendTimeout = this.serverEmu_0.int_1;
				try
				{
					this.serverEmu_0.method_15(this.socket_0);
					this.serverEmu_0.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Request Handled...");
					this.serverEmu_0.eventLoggerHandler_0(this.serverEmu_0, this.serverEmu_0.serverEmuEventArgs_0);
					if (this.serverEmu_0.bool_1)
					{
						if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
						{
							File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmu_0.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
						else
						{
							File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmu_0.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
					}
				}
				catch (Exception ex)
				{
					this.serverEmu_0.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new string[]
					{
						"[#",
						DateTime.Now.ToShortTimeString(),
						"# ServerEmu] Error: ",
						MethodBase.GetCurrentMethod().Name,
						Environment.NewLine,
						ex.Message
					}));
					this.serverEmu_0.eventLoggerHandler_0(this.serverEmu_0, this.serverEmu_0.serverEmuEventArgs_0);
					if (this.serverEmu_0.bool_1)
					{
						if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
						{
							File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmu_0.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
						else
						{
							File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmu_0.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
						}
					}
					try
					{
						this.socket_0.Close();
						this.serverEmu_0.serverEmuEventArgs_0 = new ServerEmuEventArgs("[#" + DateTime.Now.ToShortTimeString() + "# ServerEmu] Client Socket - Closed during error in: " + MethodBase.GetCurrentMethod().Name);
						this.serverEmu_0.eventLoggerHandler_0(this.serverEmu_0, this.serverEmu_0.serverEmuEventArgs_0);
						if (this.serverEmu_0.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmu_0.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmu_0.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
					}
					catch (Exception ex2)
					{
						this.serverEmu_0.serverEmuEventArgs_0 = new ServerEmuEventArgs(string.Concat(new string[]
						{
							"[#",
							DateTime.Now.ToShortTimeString(),
							"# ServerEmu] Error: ",
							MethodBase.GetCurrentMethod().Name,
							Environment.NewLine,
							ex2.Message
						}));
						this.serverEmu_0.eventLoggerHandler_0(this.serverEmu_0, this.serverEmu_0.serverEmuEventArgs_0);
						if (this.serverEmu_0.bool_1)
						{
							if (!File.Exists(Application.StartupPath + "\\SessionBypass.log"))
							{
								File.WriteAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmu_0.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
							else
							{
								File.AppendAllText(Application.StartupPath + "\\SessionBypass.log", this.serverEmu_0.serverEmuEventArgs_0.CurrentEvent + Environment.NewLine);
							}
						}
					}
				}
			}

			// Token: 0x0400001C RID: 28
			public Socket socket_0;

			// Token: 0x0400001D RID: 29
			public ServerEmu serverEmu_0;
		}
	}
}
