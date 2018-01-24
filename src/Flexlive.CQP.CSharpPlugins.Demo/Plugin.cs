using Flexlive.CQP.Framework;
using Flexlive.CQP.Framework.Utils;
using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace Wennx.CQP.CSharpPlugins.TRPGBot
{
	/// <summary>
	/// 酷Q C#版插件Demo
	/// </summary>
	public class Plugin : CQAppAbstract
	{
		Dictionary<long, GroupSession> SessionTable = new Dictionary<long, GroupSession>();
		static Random rd = new Random();

		static string CSPath = "C:\\酷Q Pro\\CSharpPlugins";
		static string CQPath = "C:\\酷Q Pro";

		/// <summary>
		/// 应用初始化，用来初始化应用的基本信息。
		/// </summary>
		public override void Initialize()
		{
			// 此方法用来初始化插件名称、版本、作者、描述等信息，
			// 不要在此添加其它初始化代码，插件初始化请写在Startup方法中。

			this.Name = "TRPG Bot";
			this.Version = new Version("0.0.2.5");
			this.Author = "Wennx";
			this.Description = "TRPG综合Bot";
		}

		/// <summary>
		/// 应用启动，完成插件线程、全局变量等自身运行所必须的初始化工作。
		/// </summary>
		public override void Startup()
		{
			//完成插件线程、全局变量等自身运行所必须的初始化工作。

		}

		/// <summary>
		/// 打开设置窗口。
		/// </summary>
		public override void OpenSettingForm()
		{
			// 打开设置窗口的相关代码。
			FormSettings frm = new FormSettings();
			frm.ShowDialog();
		}

		/// <summary>
		/// Type=21 私聊消息。
		/// </summary>
		/// <param name="subType">子类型，11/来自好友 1/来自在线状态 2/来自群 3/来自讨论组。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromQQ">来源QQ。</param>
		/// <param name="msg">消息内容。</param>
		/// <param name="font">字体。</param>
		public override void PrivateMessage(int subType, int msgID, long fromQQ, string msg, int font)
		{
			try
			{
				if (!PrivateSession.Sessions.ContainsKey(fromQQ))
				{
					PrivateSession.Sessions.Add(fromQQ, new PrivateSession(fromQQ));
				}
				PrivateSession.Sessions[fromQQ].PrivateMessageHandler(msg, msgID);
				
			}
			catch (Exception e)
			{
				Tools.SendDebugMessage(e.ToString());
			}
		}

		/// <summary>
		/// Type=2 群消息。
		/// </summary>
		/// <param name="subType">子类型，目前固定为1。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromGroup">来源群号。</param>
		/// <param name="fromQQ">来源QQ。</param>
		/// <param name="fromAnonymous">来源匿名者。</param>
		/// <param name="msg">消息内容。</param>
		/// <param name="font">字体。</param>
		public override void GroupMessage(int subType, int msgID, long fromGroup, long fromQQ, string fromAnonymous, string msg, int font)
		{
			try
			{
				if (!GroupSession.Sessions.ContainsKey(fromGroup))
				{
					GroupSession.Sessions.Add(fromGroup, new GroupSession(fromGroup));
				}
				GroupSession.Sessions[fromGroup].GroupMessageHandler(fromQQ, msg, msgID);
			}
			catch(Exception e)
			{
				Tools.SendDebugMessage(fromGroup + "--" + fromQQ + ":" + msg + "\n" + e.ToString());
			}
			
		}

		/// <summary>
		/// Type=4 讨论组消息。
		/// </summary>
		/// <param name="subType">子类型，目前固定为1。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromDiscuss">来源讨论组。</param>
		/// <param name="fromQQ">来源QQ。</param>
		/// <param name="msg">消息内容。</param>
		/// <param name="font">字体。</param>
		public override void DiscussMessage(int subType, int msgID, long fromDiscuss, long fromQQ, string msg, int font)
		{
			// 处理讨论组消息。
			CQ.SendDiscussMessage(fromDiscuss, String.Format("[{0}]{1}你发的讨论组消息是：{2}", CQ.ProxyType, CQ.CQCode_At(fromQQ), msg));
		}

		/// <summary>
		/// Type=11 群文件上传事件。
		/// </summary>
		/// <param name="subType">子类型，目前固定为1。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromGroup">来源群号。</param>
		/// <param name="fromQQ">来源QQ。</param>
		/// <param name="file">上传文件信息。</param>
		public override void GroupUpload(int subType, int sendTime, long fromGroup, long fromQQ, string file)
		{
			// 处理群文件上传事件。
			CQ.SendGroupMessage(fromGroup, String.Format("[{0}]{1}你上传了一个文件：{2}", CQ.ProxyType, CQ.CQCode_At(fromQQ), file));
		}

		/// <summary>
		/// Type=101 群事件-管理员变动。
		/// </summary>
		/// <param name="subType">子类型，1/被取消管理员 2/被设置管理员。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromGroup">来源群号。</param>
		/// <param name="beingOperateQQ">被操作QQ。</param>
		public override void GroupAdmin(int subType, int sendTime, long fromGroup, long beingOperateQQ)
		{
			// 处理群事件-管理员变动。
			//CQ.SendGroupMessage(fromGroup, String.Format("[{0}]{2}({1})被{3}管理员权限。", CQ.ProxyType, beingOperateQQ, CQE.GetQQGetName(beingOperateQQ), subType == 1 ? "取消了" : "设置为"));
		}

		/// <summary>
		/// Type=102 群事件-群成员减少。
		/// </summary>
		/// <param name="subType">子类型，1/群员离开 2/群员被踢 3/自己(即登录号)被踢。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromGroup">来源群。</param>
		/// <param name="fromQQ">来源QQ。</param>
		/// <param name="beingOperateQQ">被操作QQ。</param>
		public override void GroupMemberDecrease(int subType, int sendTime, long fromGroup, long fromQQ, long beingOperateQQ)
		{
			// 处理群事件-群成员减少。
			//CQ.SendGroupMessage(fromGroup, String.Format("[{0}]群员{2}({1}){3}", CQ.ProxyType, beingOperateQQ, CQE.GetQQGetName(beingOperateQQ), subType == 1 ? "退群。" : String.Format("被{0}({1})踢除。", CQE.GetQQGetName(fromQQ), fromQQ)));
		}

		/// <summary>
		/// Type=103 群事件-群成员增加。
		/// </summary>
		/// <param name="subType">子类型，1/管理员已同意 2/管理员邀请。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromGroup">来源群。</param>
		/// <param name="fromQQ">来源QQ。</param>
		/// <param name="beingOperateQQ">被操作QQ。</param>
		public override void GroupMemberIncrease(int subType, int sendTime, long fromGroup, long fromQQ, long beingOperateQQ)
		{
			// 处理群事件-群成员增加。
			//CQ.SendGroupMessage(fromGroup, String.Format("[{0}]群里来了新人{2}({1})，管理员{3}({4}){5}", CQ.ProxyType, beingOperateQQ, CQE.GetQQGetName(beingOperateQQ), CQE.GetQQGetName(fromQQ), fromQQ, subType == 1 ? "同意。" : "邀请。"));
		}

		/// <summary>
		/// Type=201 好友事件-好友已添加。
		/// </summary>
		/// <param name="subType">子类型，目前固定为1。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromQQ">来源QQ。</param>
		public override void FriendAdded(int subType, int sendTime, long fromQQ)
		{
			// 处理好友事件-好友已添加。
			CQ.SendPrivateMessage(fromQQ, String.Format("[{0}]你好，我的朋友！", CQ.ProxyType));
		}

		/// <summary>
		/// Type=301 请求-好友添加。
		/// </summary>
		/// <param name="subType">子类型，目前固定为1。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromQQ">来源QQ。</param>
		/// <param name="msg">附言。</param>
		/// <param name="responseFlag">反馈标识(处理请求用)。</param>
		public override void RequestAddFriend(int subType, int sendTime, long fromQQ, string msg, string responseFlag)
		{
			// 处理请求-好友添加。
			//CQ.SetFriendAddRequest(responseFlag, CQReactType.Allow, "新来的朋友");
		}

		/// <summary>
		/// Type=302 请求-群添加。
		/// </summary>
		/// <param name="subType">子类型，目前固定为1。</param>
		/// <param name="sendTime">发送时间(时间戳)。</param>
		/// <param name="fromGroup">来源群号。</param>
		/// <param name="fromQQ">来源QQ。</param>
		/// <param name="msg">附言。</param>
		/// <param name="responseFlag">反馈标识(处理请求用)。</param>
		public override void RequestAddGroup(int subType, int sendTime, long fromGroup, long fromQQ, string msg, string responseFlag)
		{
			// 处理请求-群添加。
			//CQ.SetGroupAddRequest(responseFlag, CQRequestType.GroupAdd, CQReactType.Allow, "新群友");
		}

	}

	class PrivateSession
	{
		static string CSPath = "C:\\酷Q Pro\\CSharpPlugins";
		static string CQPath = "C:\\酷Q Pro";
		static public Dictionary<long, PrivateSession> Sessions = new Dictionary<long, PrivateSession>();
		long QQid;
		RandomCreator rc;
		public bool rcInput = false;
		Random rd = new Random();
		Regex CQIMG = new Regex("\\[CQ:image,file=[\\S ]*?\\]");

		string CharBinding;

		public PrivateSession(long id)
		{
			QQid = id;
		}

		public void Send(string msg)
		{
			CQ.SendPrivateMessage(QQid, msg);
		}

		public void PrivateMessageHandler(string msg, int MsgID)
		{
			
			
			string[] msgstr = msg.Split(' ');
			switch (msgstr[0])
			{
				case ".reset":
					Send("已重置会话");
					Sessions.Remove(QQid);
					break;
				case ".r":
					Roll(msg);
					break;
				case ".s":
					Search(QQid, msg, MsgID);
					break;
				case ".draw":
					Draw(QQid, msg, MsgID);
					break;
				case ".rc":
					rc = new RandomCreator(msgstr[1], this);
					rc.Build();
					break;
				default:
					if (CQIMG.IsMatch(msg))
					{
						AddMap(msg);
					}
					else if (rcInput)
					{
						rc.Build(msg);
						return;
					}
					break;
			}
		}

		public void Roll(string rollstr)
		{
			string[] rsn = new Regex(".r\\s[\\s\\S]*").Match(rollstr).ToString().Split(' ');
			if (rsn.Length > 2)
			{
				Send(String.Format("{0}：{1}",  rsn[2], Tools.Dice(rollstr)));
			}
			else
			{
				Send(String.Format("{0}", Tools.Dice(rollstr)));
			}
		}

		public void CharSelection()
		{
			Dictionary<string, string> menu = new Dictionary<string, string>();
			DirectoryInfo d = new DirectoryInfo(CSPath + "\\CharSettings");
			foreach (FileInfo f in d.GetFiles("*.ini"))
			{

				if (IniFileHelper.GetStringValue(f.FullName, "CharInfo", "PlayerID", "") == QQid.ToString())
				{
					menu.Add(IniFileHelper.GetStringValue(f.FullName, "CharInfo", "CharID", "0"),
						IniFileHelper.GetStringValue(f.FullName, "CharInfo", "CharName", "Unnamed")
						+ "--" + IniFileHelper.GetStringValue(f.FullName, "CharInfo", "CharDesc", "Unknown"));
				}
			}
			string m = CQ.CQCode_At(QQid) + " 可用角色：\n";
			foreach (KeyValuePair<string, string> e in menu)
			{
				m += e.Key + ". " + e.Value + "\n";
			}
			m += "输入.csel+序号进行选择";
			menu.Clear();
			Send(m);
		}

		public void CharBind(string msg, int msgID)
		{
			string selection = msg.Replace(".csel ", "").Replace("。csel ", "");
			DirectoryInfo d = new DirectoryInfo(CSPath + "\\CharSettings");
			foreach (FileInfo f in d.GetFiles("*.ini"))
			{
				if (IniFileHelper.GetStringValue(f.FullName, "CharInfo", "CharID", "") == selection)
				{
					CharBinding = f.FullName;
				}
			}
			Send(string.Format("{0} 绑定了角色 {1}", CQ.CQCode_At(QQid),
				IniFileHelper.GetStringValue(CharBinding.ToString(), "CharInfo", "CharName", "")));
		}

		public void CharDisbinding()
		{
			CharBinding = "";
			Send(string.Format("{0} 解除绑定了当前角色", CQ.CQCode_At(QQid)));
		}

		public void AddMap(string msg)
		{
			Regex R = new Regex("R[.0-9]+");
			Regex C = new Regex("C[.0-9]+");
			string img = CQIMG.Match(msg).ToString().Replace("[CQ:image,file=", "").Replace("]", "");
			msg = CQIMG.Replace(msg, "");
			if (R.IsMatch(msg) && C.IsMatch(msg))
			{
				Image map = Tools.GetImage(img);
				string rdname = rd.Next(9999).ToString("0000");
				float row = float.Parse(Regex.Match(msg, "R[.0-9]+").ToString().Replace("R", ""));
				float col = float.Parse(Regex.Match(msg, "C[.0-9]+").ToString().Replace("C", ""));
				Bitmap bmp = new Bitmap((int)(150 * col), (int)(150 * row));
				Graphics g = Graphics.FromImage(bmp);
				g.DrawImage(map, 0, 0, (int)(150 * col), (int)(150 * row));
				bmp.Save(CSPath + "\\Maps\\TM" + rdname 
					+ "(" + R.Match(msg).ToString() + C.Match(msg).ToString() + (msg.Contains("NG") ? "NG" : "") + ").jpg");
				Send("临时地图：TM" + rdname + "已上传");
			}
			else
			{
				Send("格式不正确");
			}
		}

		Dictionary<long, List<FileInfo>> SearchMenu = new Dictionary<long, List<FileInfo>>();
		public void Search(long QQid, string msg, int msgID)
		{
			msg = msg.Replace(".s", "").Replace("。s", "");
			string[] msgs = msg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			DirectoryInfo d = new DirectoryInfo(CSPath + "\\Data");
			int res = 0;
			if (!SearchMenu.ContainsKey(QQid) || !Regex.IsMatch(msgs[0], "[0-9]+") || !int.TryParse(msgs[0], out res) || res < 1 || res > SearchMenu[QQid].Count)
			{
				SearchMenu.Remove(QQid);
				List<FileInfo> NewMenu = new List<FileInfo>();
				SearchMenu.Add(QQid, new List<FileInfo>(d.GetFiles("*.jpg", SearchOption.AllDirectories)));

				foreach (FileInfo fi in SearchMenu[QQid])
				{
					foreach (string str in msgs)
					{
						if (str.StartsWith("^"))
						{

							if (fi.Name.Contains(str.Substring(1)) && fi.Name.ToLower().Contains(str.Substring(1)))
							{
								goto bk;
							}
						}
						else
						{
							if (!fi.Name.Contains(str) && !fi.Name.ToLower().Contains(str))
							{
								goto bk;
							}
						}
					}
					NewMenu.Add(fi);
					bk: continue;
				}

				if (NewMenu.Count == 1)
				{
					File.Copy(NewMenu[0].FullName, CQPath + "\\data\\image\\" + NewMenu[0].Name, true);
					Send(CQ.CQCode_Image(NewMenu[0].Name.Replace("&", "&amp;").Replace(",", "&#44;").Replace("[", "&#91;").Replace("]", "&#93;")));
					SearchMenu.Remove(QQid);
				}
				else
				{
					string rtmsg = string.Format("[{0}]查找到了{1}项:", CQ.CQCode_At(QQid), NewMenu.Count);
					if (NewMenu.Count > 10)
					{
						rtmsg += "\n匹配项目过多，仅显示随机10项，建议更换或添加关键字";
						while (NewMenu.Count > 10)
						{
							NewMenu.RemoveAt(rd.Next(0, NewMenu.Count));
						}
						//NewMenu.RemoveRange(10, NewMenu.Count - 10);
					}
					SearchMenu[QQid] = NewMenu;
					foreach (FileInfo fi in NewMenu)
					{
						rtmsg += "\n" + (NewMenu.IndexOf(fi) + 1).ToString() + "." + fi.Name.Replace(fi.Extension, "");
					}
					if (NewMenu.Count > 0) rtmsg += "\n请输入.s+序号";
					else rtmsg += "\n请更换关键字后重试";
					Send(rtmsg);
				}
			}
			else
			{
				FileInfo sel = SearchMenu[QQid][res - 1];
				File.Copy(sel.FullName, CQPath + "\\data\\image\\" + sel.Name, true);
				Send(CQ.CQCode_Image(sel.Name.Replace("&", "&amp;").Replace(",", "&#44;").Replace("[", "&#91;").Replace("]", "&#93;")));
				SearchMenu.Remove(QQid);
			}
		}

		public void Draw(long QQid, string msg, int msgID)
		{
			int num = 1;
			Regex mul = new Regex("[1-9]x");
			msg = msg.Replace(".draw ", "");
			if (mul.IsMatch(msg))
			{
				int.TryParse(mul.Match(msg).ToString().Substring(0, 1), out num);
				msg = mul.Replace(msg, "");
			}
			DirectoryInfo d = new DirectoryInfo(CSPath + "\\Decks\\" + msg);
			FileInfo[] fis = d.GetFiles("*.jpg");
			List<FileInfo> sends = new List<FileInfo>();
			for (int i = 0; i < num; i++)
			{
				sends.Add(fis[rd.Next(fis.Length)]);
				File.Copy(sends[i].FullName, CQPath + "\\data\\image\\" + sends[i].Name, true);
				Send(CQ.CQCode_Image(sends[i].Name.Replace("&", "&amp;").Replace(",", "&#44;").Replace("[", "&#91;").Replace("]", "&#93;")));
			}
			
		}
	}


	class GroupSession
	{

		static public Dictionary<long, GroupSession> Sessions = new Dictionary<long, GroupSession>();

		static string CSPath = "C:\\酷Q Pro\\CSharpPlugins";
		static string CQPath = "C:\\酷Q Pro";

		Random rd = new Random();

		long GroupID;

		bool Logging = false;
		string LogFile = "";

		Regex CQAT = new Regex("\\[CQ:at,qq=[0-9]*\\]");
		Regex CQIMG = new Regex("\\[CQ:image,file=[\\S ]*?\\]");

		AdventureTime time = new AdventureTime();



		Dictionary<long, string> CharBinding = new Dictionary<long, string>();

		long Owner;
		List<long> Admin = new List<long>();


		int nya_mood = 30;
		string[] nya_normal = { "喵~", "喵？", "喵！", "喵喵！", "喵~喵~", "喵呜？", "喵…", "喵喵？" };
		string[] nya_happy = { "喵~喵~喵~", "喵~~~", "喵？喵！", "喵？" };
		string[] nya_sad = { "喵~……", "……", "……喵？" };
		string[] nya_lazy = { };
		string[] nya_angry = { "喵呜！！！", "嘶！", "嗷呜！" };


		struct AdventureTime
		{
			int Year, Month, Day, Hour, Minute;
			static Regex min = new Regex("[0-9]+min");
			static Regex h = new Regex("[0-9]+h");
			static Regex d = new Regex("[0-9]+d");
			static Regex m = new Regex("[0-9]+m");
			static Regex y = new Regex("[0-9]+y");


			public void Set(string set)
			{
				this = new AdventureTime();
				Match mt;
				mt = min.Match(set);
				if (mt.Success)
				{
					this.Minute += int.Parse(mt.ToString().Replace("min", ""));
					set = min.Replace(set, "");
				}
				if (this.Minute >= 60)
				{
					this.Minute -= 60;
					this.Hour++;
				}
				mt = h.Match(set);
				if (mt.Success)
				{
					this.Hour += int.Parse(mt.ToString().Replace("h", ""));
				}
				if (this.Hour >= 24)
				{
					this.Hour -= 24;
					this.Day++;
				}
				mt = d.Match(set);
				if (mt.Success)
				{
					this.Day += int.Parse(mt.ToString().Replace("d", ""));
				}
				if (this.Day > 30)
				{
					this.Day -= 30;
					this.Month++;
				}
				mt = m.Match(set);
				if (mt.Success)
				{
					this.Month += int.Parse(mt.ToString().Replace("m", ""));
				}
				if (this.Month > 12)
				{
					this.Month -= 12;
					this.Hour++;
				}
				mt = y.Match(set);
				if (mt.Success)
				{
					this.Year += int.Parse(mt.ToString().Replace("y", ""));
				}
			}
			public static AdventureTime operator +(AdventureTime t, string add)
			{
				Match mt;
				mt = min.Match(add);
				if (mt.Success)
				{
					t.Minute += int.Parse(mt.ToString().Replace("min", ""));
					add = min.Replace(add, "");
				}
				if (t.Minute >= 60)
				{
					t.Minute -= 60;
					t.Hour++;
				}
				mt = h.Match(add);
				if (mt.Success)
				{
					t.Hour += int.Parse(mt.ToString().Replace("h", ""));
				}
				if (t.Hour >= 24)
				{
					t.Hour -= 24;
					t.Day++;
				}
				mt = d.Match(add);
				if (mt.Success)
				{
					t.Day += int.Parse(mt.ToString().Replace("d", ""));
				}
				if (t.Day > 30)
				{
					t.Day -= 30;
					t.Month++;
				}
				mt = m.Match(add);
				if (mt.Success)
				{
					t.Month += int.Parse(mt.ToString().Replace("m", ""));
				}
				if (t.Month > 12)
				{
					t.Month -= 12;
					t.Hour++;
				}
				mt = y.Match(add);
				if (mt.Success)
				{
					t.Year += int.Parse(mt.ToString().Replace("y", ""));
				}
				return t;
			}
			public override string ToString()
			{
				return string.Format("现在是{0}年{1}月{2}日{3}时{4}分", Year, Month, Day, Hour, Minute);
			}
		}


		public GroupSession(long id)
		{
			GroupID = id;
			Admin.Add(long.Parse(IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "GeneralSetting", "DebugID", "")));
			CharBinding = new Dictionary<long, string>();
			foreach (string kv in IniFileHelper.GetAllItems(CSPath + "\\Config.ini", "G" + id + "_CharBinding"))
			{
				if (File.Exists(kv.Substring(kv.IndexOf('=') + 1)))
				{
					CharBinding.Add(long.Parse(kv.Substring(0, kv.IndexOf('='))), kv.Substring(kv.IndexOf('=') + 1));
				}
			}
		}

		public bool isAdmin(long QQid)
		{
			if (Admin.Contains(QQid)) return true;
			if (IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "GeneralSetting", "SuperAdminID", "").Contains(QQid.ToString()))
			{
				Owner = QQid;
				Admin.Add(QQid);
				return true;
			}
			return false;
		}

		int lasMsg;

		Dictionary<long, string> buffer = new Dictionary<long, string>();
		public int Send(string msg, long ToQQ = 0)
		{
			if (buffer.ContainsKey(ToQQ))
			{
				buffer[ToQQ] += "\n" + msg;
				return 0;
			}
			lasMsg = CQ.SendGroupMessage(GroupID, msg);
			if (Logging) Log(msg, 0);
			return lasMsg;
		}

		public void MsgBuffer(long QQid)
		{
			if (!buffer.ContainsKey(QQid))
			{
				buffer.Add(QQid, "");
			}
		}

		public int SendBuffer(long QQid)
		{
			if (buffer.ContainsKey(QQid))
			{
				string msg = buffer[QQid];
				buffer.Remove(QQid);
				if (msg.StartsWith("\n")) msg = msg.Substring(1);
				return Send(msg, QQid);

			}
			return 0;
		}

		Dictionary<long, DateTime> GroupInfoCache = new Dictionary<long, DateTime>();
		Dictionary<long, string> MemberName = new Dictionary<long, string>();
		public string GetName(long QQid)
		{
			WebClient wc = new WebClient();
			return Regex.Match(Encoding.Default.GetString(wc.DownloadData(@"http://r.pengyou.com/fcg-bin/cgi_get_portrait.fcg?uins=" + QQid)), ",0,0,0,\"(?<Name>[\\S ]+?)\",0").Groups["Name"].ToString();

		}



		public void Nya(long QQid, string msg, int msgID)
		{
			if (IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "Nya", "Like", "").Contains(QQid.ToString())) nya_mood += 20;
			if (nya_mood > 75)
			{
				Send(nya_happy[rd.Next(0, 4)], QQid);
			}
			else if (nya_mood < 15)
			{
				Send(nya_angry[rd.Next(0, 3)], QQid);
				if (!isAdmin(QQid)) CQ.SetGroupMemberGag(GroupID, QQid, 15);
			}
			else if (nya_mood < 35)
			{
				Send(nya_sad[rd.Next(0, 3)], QQid);
			}

			else
			{
				Send(nya_normal[rd.Next(0, 8)], QQid);
			}
			if (IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "Nya", "Hate", "").Contains(QQid.ToString())) nya_mood -= 20;
			else if (!IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "Nya", "Like", "").Contains(QQid.ToString())) nya_mood -= 5;
		}

		public void SaveSession(long QQid)
		{

		}

		public void LoadSession(long QQid)
		{

		}

		public void GroupMessageHandler(long QQid, string msg, int MsgID, bool subCmd = false)
		{
			if (msg.StartsWith("喵")) Nya(QQid, msg, MsgID);
			if (Logging && !subCmd) Log(msg, QQid);
			if (isAdmin(QQid) && msg.StartsWith("+"))
			{
				time += msg;
				Send(time.ToString(), QQid);
			}
			string[] msgstr;
			if (msg.StartsWith("。")) msg = "." + msg.Substring(1);
			if (!subCmd && msg.StartsWith(".") && CharBinding.ContainsKey(QQid))
			{
				foreach (string k in IniFileHelper.GetAllItemKeys(CharBinding[QQid], "CustomCmd"))
				{
					msg = msg.Replace("." + k, IniFileHelper.GetStringValue(CharBinding[QQid], "CustomCmd", k, ""));
					while (msg.Contains("+++ "))
					{
						msg = msg.Replace("+++ ", "+++");
					}
					msg = msg.Replace("+++", "");
				}
				msgstr = msg.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
				if (msgstr.Length > 1)
				{
					MsgBuffer(QQid);
					foreach (string m in msgstr)
					{
						GroupMessageHandler(QQid, "." + m, MsgID, true);
					}
					SendBuffer(QQid);
					return;
				}
				else
				{
					if (!msg.StartsWith(".")) msg = "." + msg;
				}
			}
			msgstr = msg.Split(' ');
			switch (msgstr[0].ToLower())
			{
				case ".reset":
					if (QQid == Owner && msg.Contains(GroupID.ToString()))
					{
						Send("群会话已重置", QQid);
						if (Logging)
						{
							LoggerToggle(QQid, "", MsgID);
						}
						Sessions.Remove(GroupID);

					}
					break;
				case ".gn":
					Send(GetName(QQid), QQid);
					break;
				case ".settime":
					if (isAdmin(QQid) && msgstr.Length > 1)
					{
						time.Set(msgstr[1]);
						Send(time.ToString(), QQid);
					}
					break;
				case ".time":
					Send(time.ToString(), QQid);
					break;
				case ".cmd":
					if (CharBinding.ContainsKey(QQid))
					{
						string cmd = "自定义命令：";
						foreach (string s in IniFileHelper.GetAllItems(CharBinding[QQid], "CustomCmd"))
						{
							cmd += "\n" + s;
						}
						CQ.SendPrivateMessage(QQid, cmd);
					}
					break;
				case ".delast":
					CQ.DeleteMsg(lasMsg).ToString();
					break;
				case ".r":
					Roll(QQid, msg, MsgID);
					nya_mood += 5;
					break;
				case ".save":
					SaveSession(QQid);
					break;
				case ".load":
					LoadSession(QQid);
					break;
				case ".s":
					Search(QQid, msg, MsgID);
					nya_mood += 5;
					break;
				case ".rs":

					Roll(QQid, msg, MsgID);
					break;
				case ".csel":
					if (msgstr.Length == 1)
					{
						CharSelection(QQid);
					}
					else
					{
						CharBind(QQid, msgstr[1], MsgID);
					}
					break;
				case ".cset":
					if (msgstr.Length == 3)
					{
						CharModify(QQid, msgstr[1], msgstr[2]);
					}
					break;
				case ".sset":
					if (msgstr.Length == 3)
					{
						SideModify(QQid, msgstr[1], msgstr[2]);
					}
					break;
				case ".mset":
					if (msgstr.Length == 3)
					{
						MemorySet(QQid, msgstr[1], msgstr[2]);
					}
					break;
				case ".msset":
					if (msgstr.Length == 3)
					{
						SideMemorySet(QQid, msgstr[1], msgstr[2]);
					}
					break;
				case ".ctset":
					CounterSet(QQid, msgstr[1], msgstr[2]);
					break;
				case ".ltset":
					ListSet(QQid, msgstr[1], msg);
					break;
				case ".ctsets":
					SideCounterSet(QQid, msgstr[1], msgstr[2]);
					break;
				case ".ltsets":
					SideListSet(QQid, msgstr[1], msg);
					break;
				case ".cdis":
					CharDisbinding(QQid);
					break;
				case ".m":
					Memory(QQid, msg, MsgID);
					break;
				case ".ms":
					SideMemory(QQid, msg, MsgID);
					break;
				case ".setdm":
					if (msgstr.Length > 1) SetDM(QQid, msgstr[1], MsgID);
					break;
				case ".ar":
					if (msgstr.Length == 2) AllRoll(QQid, msgstr[1]);
					break;
				case ".i":
					if (msgstr.Length > 1) GHI(QQid, msgstr[1], MsgID);
					break;
				case ".ghis":
					if (msgstr.Length > 1) GHIs(QQid, msgstr[1], MsgID);
					break;
				case ".map":
					if (msgstr.Length > 1 && isAdmin(QQid)) InitMap(QQid, msg, MsgID);
					else ShowMap(QQid, msg, MsgID);
					break;
				case ".view":
					SetView(QQid, msg, MsgID);
					break;
				case ".mov":
					MoveIcon(QQid, msg, MsgID);
					break;
				case ".movs":
					MoveIcon(QQid, msg, MsgID);
					break;
				case ".add":
					AddIcon(QQid, msg, MsgID);
					break;
				case ".help":
					Help();
					break;
				case ".rest":
					Rest(QQid, msg, MsgID);
					break;
				case ".log":
					if (msgstr.Length > 1)
					{
						LoggerToggle(QQid, msgstr[1], MsgID);
					}
					else
					{
						LoggerToggle(QQid, DateTime.Now.ToString(), MsgID);
					}
					break;
			}



		}

		ExcelPackage ep;
		ExcelWorksheet LogTable;
		int RecConter;
		long CurrentLogUser;

		Dictionary<long, Image> face = new Dictionary<long, Image>();
		Dictionary<long, Color> color = new Dictionary<long, Color>();
		List<Color> unusedColor
			= new List<Color>() { Color.Blue,Color.Brown,Color.CadetBlue,Color.DarkOrange,Color.DarkViolet,
			Color.ForestGreen,Color.Fuchsia,Color.Goldenrod,Color.Red};
		List<Color> predefColor
					= new List<Color>() { Color.Blue,Color.Brown,Color.CadetBlue,Color.DarkOrange,Color.DarkViolet,
			Color.ForestGreen,Color.Fuchsia,Color.Goldenrod,Color.Red};

		public void LoggerToggle(long QQid, string msg, int msgID)
		{
			if (!isAdmin(QQid)) return;
			if (Logging == false)
			{
				DirectoryInfo d = new DirectoryInfo(CSPath + "\\CharSettings");
				Logging = true;
				LogFile = msg;

				ep = new ExcelPackage();
				LogTable = ep.Workbook.Worksheets.Add(LogFile);
				RecConter = 1;
				LogTable.Column(1).Width = 12.73d;
				LogTable.Column(2).Width = 20d;
				LogTable.Column(3).Width = 67.67d;
				LogTable.Column(4).Width = 20d;
				LogTable.Column(2).Style.WrapText = true;
				LogTable.Column(3).Style.WrapText = true;
				LogTable.Cells[1, 1].Value = "头像";
				LogTable.Cells[1, 2].Value = "人物/昵称";
				LogTable.Cells[1, 3].Value = "记录";
				LogTable.Cells[1, 4].Value = "时间";
				if (!color.ContainsKey(0)) color.Add(0, Color.Black);
				if (!color.ContainsKey(long.Parse(IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "GeneralSetting", "DebugID", ""))))
					color.Add(long.Parse(IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "GeneralSetting", "DebugID", "")), Color.DeepSkyBlue);


				Send(string.Format("=======群日志 {0}=======", msg), QQid);
				Send(string.Format("=======群号：{0}=======", GroupID), QQid);
				Send(string.Format("=======发起者：{0}=======", QQid), QQid);
				Send(string.Format("======={0}开始记录=======", DateTime.Now), QQid);
			}
			else
			{
				Send("=======记录结束=======", QQid);
				color.Clear();
				face.Clear();
				ep.SaveAs(new FileInfo(CSPath + "\\LogFiles\\" + LogFile + "-" + GroupID + ".xlsx"));
				LogFile = "";
				Logging = false;
			}
		}


		DateTime lastTimeStamp = DateTime.Now;

		public void LogColorTest()
		{
			Logging = false;
			foreach (KnownColor color in Enum.GetValues(typeof(KnownColor)))
			{
				RecConter++;
				LogTable.Cells[RecConter, 1].Style.Font.Color.SetColor(Color.FromKnownColor(color));
				LogTable.Cells[RecConter, 1].Value = color.ToString();
			}
			Logging = true;
		}

		public void Log(string msg, long QQid = 0)
		{

			foreach (Match m in CQAT.Matches(msg))
			{
				msg = msg.Replace(m.ToString(), "@" + GetName(long.Parse(m.ToString().Replace("[CQ:at,qq=", "").Replace("]", ""))));
			}
			NewRecord(QQid, msg.Replace("&#91;", "[").Replace("&#93;", "]"));
			/*msg = msg.Replace("\n", ";;");
			if (DateTime.Now.Minute != lastTimeStamp.Minute && DateTime.Now.Minute % 10 == 0)
			{
				lastTimeStamp = DateTime.Now;
				LogWriter.WriteLine(string.Format("======={0}=======", DateTime.Now));
			}
			if (QQid == 0)
			{
				if (!msg.StartsWith("=")) msg = "====" + msg;
				LogWriter.WriteLine(string.Format("{0}", msg));
			}
			else if (CharBinding.ContainsKey(QQid))
			{
				LogWriter.WriteLine(string.Format("{0}:{1}", IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "CharName", CQE.GetQQGetName(QQid)), msg));

			}
			else
			{
				LogWriter.WriteLine(string.Format("{0}:{1}", CQE.GetQQGetName(QQid), msg));
			}*/
		}

		public void NewRecord(long QQid, string msg)
		{
			if (msg.StartsWith("(") || msg.StartsWith("（"))
			{
				msg = CQIMG.Replace(msg, "");
				LogTable.Cells[RecConter, 3].Style.Font.Color.SetColor(Color.Gray);
			}
			foreach (Match m in CQIMG.Matches(msg))
			{
				ImgRecord(QQid, m.ToString().Replace("[CQ:image,file=", "").Replace("]", ""));
			}
			msg = CQIMG.Replace(msg, "");
			if (msg == "") return;
			RecConter++;
			LogTable.Cells[RecConter, 2].Value = CharBinding.ContainsKey(QQid)
						? IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "CharName", QQid == 0 ? "=======" : GetName(QQid))
						: GetName(QQid);
			LogTable.Cells[RecConter, 3].Value = msg;
			LogTable.Cells[RecConter, 4].Value = DateTime.Now.ToLongTimeString();
			if (msg.StartsWith("\"") || msg.StartsWith("“") || msg.StartsWith("”") || msg.Contains(":\"") || msg.Contains("：“") || msg.Contains("：”"))
				LogTable.Cells[RecConter, 3].Style.Font.Bold = true;
			SetColor(QQid);
			if (msg.StartsWith("(") || msg.StartsWith("（")) LogTable.Cells[RecConter, 3].Style.Font.Color.SetColor(Color.Gray);

			LogTable.Row(RecConter).CustomHeight = false;
			if (LogTable.Row(RecConter).Height < 75 && (CurrentLogUser != QQid || QQid != 0))
			{
				LogTable.Row(RecConter).CustomHeight = true;
				LogTable.Row(RecConter).Height = 75;
			}
			SetFace(QQid);
			if (RecConter % 5 == 0)
			{
				ep.SaveAs(new FileInfo(CSPath + "\\LogFiles\\" + LogFile + "-" + GroupID + ".xlsx"));
			}
		}

		public void ImgRecord(long QQid, string imgID)
		{
			RecConter++;
			Image img;

			OfficeOpenXml.Drawing.ExcelPicture pic;
			if (QQid == 0)
			{
				img = Image.FromFile(CQPath + "\\data\\image\\" + imgID);
				pic = LogTable.Drawings.AddPicture(imgID + RecConter, img);

			}
			else
			{
				img = Tools.GetImage(imgID);
				pic = LogTable.Drawings.AddPicture(imgID + RecConter, img);
			}
			double height = img.Height;
			if (img.Width > 1024)
			{
				height = 770d * img.Height / img.Width;
			}
			else
			{
				height = img.Height / 1.33;
			}
			if (height > 75)
				LogTable.Row(RecConter).Height = height;
			else
				LogTable.Row(RecConter).Height = 75;
			SetFace(QQid);
			SetColor(QQid);
			LogTable.Cells[RecConter, 2].Value = CharBinding.ContainsKey(QQid)
				? IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "CharName"
				, QQid == 0 ? "=======" : GetName(QQid)) : QQid == 0 ? "=======" : GetName(QQid);
			if (img.Width > 1024)
			{
				pic.SetPosition(RecConter - 1, 0, 2, 0);
				pic.SetSize(1024, 1024 * img.Height / img.Width);
			}
			else
			{
				pic.SetPosition(RecConter - 1, 0, 2, 0);
			}
		}

		public void SetFace(long QQid)
		{
			if (!face.ContainsKey(QQid))
			{
				if (CharBinding.ContainsKey(QQid))
				{
					string ico;
					ico = IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "Icon", "");
					if (ico != "")
					{
						face.Add(QQid, Image.FromFile(CSPath + "\\Icons\\" + ico));
					}
					else
					{
						face.Add(QQid, Tools.GetFace(QQid));
					}
				}
				else
				{
					face.Add(QQid, Tools.GetFace(QQid));

				}

			}
			if (CurrentLogUser == QQid || QQid == 0) return;
			CurrentLogUser = QQid;
			var pic = LogTable.Drawings.AddPicture(QQid.ToString() + RecConter, face[QQid]);
			pic.SetPosition(RecConter - 1, 0, 0, 0);
			pic.SetSize(90, 100);
		}

		public void SetColor(long QQid)
		{
			if (!CharBinding.ContainsKey(QQid) && !color.ContainsKey(QQid)) color.Add(QQid, Color.Gray);
			else if (!color.ContainsKey(QQid))
			{
				color.Add(QQid
				, Color.FromName(IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "LogColor", "White")));
			}
			else if (color[QQid] == Color.Gray && CharBinding.ContainsKey(QQid))
			{
				color[QQid] = Color.FromName(IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "LogColor", "White"));
			}
			if (color[QQid] == Color.White)
			{
				color[QQid] = unusedColor[rd.Next(0, unusedColor.Count)];
				unusedColor.Remove(color[QQid]);
			}
			LogTable.Cells[RecConter, 2].Style.Font.Color.SetColor(color[QQid]);
			LogTable.Cells[RecConter, 3].Style.Font.Color.SetColor(color[QQid]);

		}





		bool mapping = false;
		Image orimap;
		Image map;
		Rectangle view;
		Rectangle fullview;
		Graphics mapg;
		float row;
		float col;
		float blockW;
		float blockH;

		int lastMap;

		Dictionary<Point, int> IconCounter;
		DataTable Icons;
		Point um = new Point(-1, -1);
		int icoid = 1;
		int monsid = 1;
		List<string> letter = new List<string>() {
			"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
			"N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
			"AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM",
			"AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ",
			"BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM",
			"BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ",
			"CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM",
			"CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ" };

		public void InitMap(long QQid, string msg, int msgID)
		{
			if (!isAdmin(QQid)) return;
			CQ.DeleteMsg(msgID);
			if (mapping)
			{
				Icons.Clear();
				mapg.Dispose();
				orimap.Dispose();
				map.Dispose();
				Icons.Dispose();
			}
			else
			{
				mapping = true;
			}
			GC.Collect();
			string[] files = Directory.GetFiles(CSPath + "\\Maps", "*" + msg.Replace(".map ", "") + "*.jpg");
			if (files.Length != 1)
			{
				Send("无法确定地图，请确认文件名后再试", QQid);
				mapping = false;
				return;
			}
			icoid = 1;
			monsid = 1;
			InitIcon(QQid, msg);
			orimap = Image.FromFile(files[0]);
			map = (Image)orimap.Clone();
			mapg = Graphics.FromImage(map);
			string filename = new FileInfo(files[0]).Name;
			CQ.SendPrivateMessage(QQid, "已加载地图：" + filename);
			row = float.Parse(Regex.Match(filename, "R[.0-9]+").ToString().Replace("R", ""));
			col = float.Parse(Regex.Match(filename, "C[.0-9]+").ToString().Replace("C", ""));
			blockW = map.Width / col;
			blockH = map.Height / row;
			if (filename.Contains("NG")) AddGrid();
			view = new Rectangle(0, 0, (int)col, (int)row);
			fullview = view;
			DrawMapMark();
			CQ.SendPrivateMessage(QQid, SendMap(map));
		}

		public void AddGrid()
		{
			Pen p1 = new Pen(Color.BurlyWood, 5);
			Pen p2 = new Pen(Color.Chocolate, 3);
			Pen p3 = new Pen(Color.DarkGoldenrod, 2);
			for (int r = 0; r < row; r++)
			{
				mapg.DrawLine(p1, new PointF(0f, r * blockH), new PointF(map.Width, r * blockH));
				mapg.DrawLine(p2, new PointF(0f, r * blockH), new PointF(map.Width, r * blockH));
				mapg.DrawLine(p3, new PointF(0f, r * blockH), new PointF(map.Width, r * blockH));
			}
			for (int c = 0; c < col; c++)
			{
				mapg.DrawLine(p1, new PointF(c * blockW, 0f), new PointF(c * blockW, map.Height));
				mapg.DrawLine(p2, new PointF(c * blockW, 0f), new PointF(c * blockW, map.Height));
				mapg.DrawLine(p3, new PointF(c * blockW, 0f), new PointF(c * blockW, map.Height));
			}
			orimap = (Image)map.Clone();

		}

		public void InitIcon(long QQid, string msg)
		{
			Icons = new DataTable();
			IconCounter = new Dictionary<Point, int>();
			Icons.Columns.Add("id", typeof(string));
			Icons.Columns.Add("X", typeof(int));
			Icons.Columns.Add("Y", typeof(int));
			Icons.Columns.Add("size", typeof(string));
			Icons.Columns.Add("filename", typeof(string));
			Icons.Columns.Add("owner", typeof(string));
			IconCounter.Add(um, 0);
			string str = "自动添加的标记：";
			foreach (KeyValuePair<long, string> r in CharBinding)
			{
				Icons.Rows.Add("c" + icoid, -1, -1, IniFileHelper.GetStringValue(r.Value, "CharInfo", "Size", "x1"), 
					CSPath + "\\Icons\\" + IniFileHelper.GetStringValue(r.Value, "CharInfo", "Icon", ""), r.Key);
				IconCounter[um]++;
				str += "\nc" + icoid + "    " + IniFileHelper.GetStringValue(r.Value, "CharInfo", "CharName", "");
				if (IniFileHelper.GetStringValue(r.Value, "SideMarco", "SideName", "") != "")
				{
					Icons.Rows.Add("s" + icoid, -1, -1, IniFileHelper.GetStringValue(r.Value, "SideMarco", "Size", "x1"), 
						CSPath + "\\Icons\\" + IniFileHelper.GetStringValue(r.Value, "SideMarco", "Icon", ""), r.Key);
					IconCounter[um]++;
					str += "\ns" + icoid + "    " + IniFileHelper.GetStringValue(r.Value, "SideMarco", "SideName", "");
				}
				icoid++;
			}
			CQ.SendPrivateMessage(QQid, str);
		}

		int lastIconList = 0;
		public void AddIcon(long QQid, string msg, int msgID)
		{
			long owner;
			string id;
			Regex monster = new Regex("M[0-9]+");
			Regex size = new Regex("x[2-6]");
			string ms;
			CQ.DeleteMsg(msgID);
			msg = msg.Replace(".add", "").Replace("。add", "");
			if (!isAdmin(QQid) || !monster.IsMatch(msg) || !mapping) return;
			ms = monster.Match(msg).ToString();
			if (!File.Exists(CSPath + "\\MonsterIcons\\" + ms + ".jpg"))
			{
				Send("编号无效", QQid);
				return;
			}
			string sz = size.Match(msg).ToString();
			if (sz == "") sz = "x1";
			msg = monster.Replace(msg, "");
			msg = size.Replace(msg, "");
			if (CQAT.IsMatch(msg))
			{
				owner = long.Parse(CQAT.Match(msg).ToString().Replace("[CQ:at,qq=", "").Replace("]", ""));
			}
			else
			{
				owner = QQid;
			}
			id = "m" + monsid;
			monsid++;
			Icons.Rows.Add(id, -1, -1, sz, CSPath + "\\MonsterIcons\\" + ms + ".jpg", owner);
			IconCounter[um]++;
			string s="待部署的怪物有：";
			foreach (DataRow dr in Icons.Select("X = -1 AND Y = -1 AND (id LIKE 'n*' OR id LIKE 'm*')"))
			{
				s += "\n" + (string)dr["id"] + " " + ((string)dr["filename"])
					.Replace(CSPath + "\\MonsterIcons\\", "").Replace(".jpg", "")
					+ (string)dr["size"];
			}
			CQ.DeleteMsg(lastIconList);
			lastIconList = CQ.SendPrivateMessage(QQid, s);
		}

		public void ShowMap(long QQid, string msg, int msgID = 0)
		{
			if (mapping)
			{
				mapg.Dispose();
				map.Dispose();
			}
			else
			{
				return;
			}
			CQ.DeleteMsg(msgID);
			GC.Collect();
			map = (Image)orimap.Clone();
			mapg = Graphics.FromImage(map);
			Font ft;
			Dictionary<Point, int> IcoDrawCounter = new Dictionary<Point, int>(IconCounter);
			for (int i = 6; i > 1; i--)
			{
				foreach (DataRow dr in Icons.Select("size = 'x" + i + "'"))
				{
					if (new Point((int)dr["X"], (int)dr["Y"]) == um) continue;
					mapg.DrawImage(Image.FromFile((string)dr["filename"]),
						(int)dr["X"] * blockW, (int)dr["Y"] * blockH, blockW * i, blockH * i);
					ft = new Font("微软雅黑", 20 + 2 * i, FontStyle.Bold);
					mapg.DrawString((string)dr["id"], ft, Brushes.Red,
						(int)dr["X"] * blockW + 0.3f * blockH, (int)dr["Y"] * blockH + 0.3f * blockH);
					ft = new Font("微软雅黑", 18 + 2 * i);
					mapg.DrawString((string)dr["id"], ft, Brushes.White,
						(int)dr["X"] * blockW + 0.3f * blockH, (int)dr["Y"] * blockH + 0.3f * blockH);
					IcoDrawCounter[new Point((int)dr["X"], (int)dr["Y"])]--;
				}
			}
			foreach (DataRow dr in Icons.Select("size = 'x1'"))
			{
				if (new Point((int)dr["X"], (int)dr["Y"]) == um) continue;
				if (IconCounter[new Point((int)dr["X"], (int)dr["Y"])] == 1)
				{
					mapg.DrawImage(Image.FromFile((string)dr["filename"]), 
						(int)dr["X"] * blockW, (int)dr["Y"] * blockH, blockW, blockH);
					ft = new Font("微软雅黑", 20, FontStyle.Bold);
					mapg.DrawString((string)dr["id"], ft, Brushes.Red,
						(int)dr["X"] * blockW + 0.7f * blockH, (int)dr["Y"] * blockH + 0.7f * blockH);
					ft = new Font("微软雅黑", 18);
					mapg.DrawString((string)dr["id"], ft, Brushes.White,
						(int)dr["X"] * blockW + 0.7f * blockH, (int)dr["Y"] * blockH + 0.7f * blockH);
					
					
					
					
				}
				else
				{
					switch (IcoDrawCounter[new Point((int)dr["X"], (int)dr["Y"])])
					{
						case 4:
							mapg.DrawImage(Image.FromFile((string)dr["filename"]),
								(int)dr["X"] * blockW + blockW / 2, (int)dr["Y"] * blockH + blockH / 2, blockW / 2, blockH / 2);
							ft = new Font("微软雅黑", 16, FontStyle.Bold);
							mapg.DrawString((string)dr["id"], ft, Brushes.Red,
								(int)dr["X"] * blockW + 0.8f * blockH, (int)dr["Y"] * blockH + 0.8f * blockH);
							ft = new Font("微软雅黑", 14);
							mapg.DrawString((string)dr["id"], ft, Brushes.White,
								(int)dr["X"] * blockW + 0.8f * blockH, (int)dr["Y"] * blockH + 0.8f * blockH);

							break;
						case 3:
							mapg.DrawImage(Image.FromFile((string)dr["filename"]),
								(int)dr["X"] * blockW, (int)dr["Y"] * blockH + blockH / 2, blockW / 2, blockH / 2);
							ft = new Font("微软雅黑", 16, FontStyle.Bold);
							mapg.DrawString((string)dr["id"], ft, Brushes.Red,
								(int)dr["X"] * blockW + 0.3f * blockH, (int)dr["Y"] * blockH + 0.8f * blockH);
							ft = new Font("微软雅黑", 14);
							mapg.DrawString((string)dr["id"], ft, Brushes.White,
								(int)dr["X"] * blockW + 0.3f * blockH, (int)dr["Y"] * blockH + 0.8f * blockH);
							break;
						case 2:
							mapg.DrawImage(Image.FromFile((string)dr["filename"]),
								(int)dr["X"] * blockW + blockW / 2, (int)dr["Y"] * blockH, blockW / 2, blockH / 2);
							ft = new Font("微软雅黑", 16, FontStyle.Bold);
							mapg.DrawString((string)dr["id"], ft, Brushes.Red,
								(int)dr["X"] * blockW + 0.8f * blockH, (int)dr["Y"] * blockH + 0.3f * blockH);
							ft = new Font("微软雅黑", 14);
							mapg.DrawString((string)dr["id"], ft, Brushes.White,
								(int)dr["X"] * blockW + 0.8f * blockH, (int)dr["Y"] * blockH + 0.3f * blockH);
							break;
						case 1:
							mapg.DrawImage(Image.FromFile((string)dr["filename"]),
								(int)dr["X"] * blockW, (int)dr["Y"] * blockH, blockW / 2, blockH / 2);
							ft = new Font("微软雅黑", 16, FontStyle.Bold);
							mapg.DrawString((string)dr["id"], ft, Brushes.Red,
								(int)dr["X"] * blockW + 0.3f * blockH, (int)dr["Y"] * blockH + 0.3f * blockH);
							ft = new Font("微软雅黑", 14);
							mapg.DrawString((string)dr["id"], ft, Brushes.White,
								(int)dr["X"] * blockW + 0.3f * blockH, (int)dr["Y"] * blockH + 0.3f * blockH);
							break;

					}
					IcoDrawCounter[new Point((int)dr["X"], (int)dr["Y"])]--;
				}
			}

			DrawMapMark(view.X, view.Y);
			//Send(SendMap(), QQid);
			Bitmap bmp = new Bitmap(map);
			float ratio = bmp.Size.Width / map.Width;
			Rectangle destView = new Rectangle(new Point((int)(view.X * blockW * ratio), (int)(view.Y * blockH * ratio)),
				new Size((int)(view.Width * blockW * ratio), (int)(view.Height * blockH * ratio)));
			Bitmap dest = new Bitmap(destView.Width, destView.Height);
			Graphics g = Graphics.FromImage(dest);
			g.DrawImage(bmp, new Rectangle(0, 0, destView.Width, destView.Height), destView, GraphicsUnit.Pixel);
			CQ.DeleteMsg(lastMap);
			lastMap = Send(SendMap(dest), QQid);
			g.Dispose();
			bmp.Dispose();
			dest.Dispose();
		}

		public void SetView(long QQid, string msg, int msgID)
		{
			if (mapping)
			{
				mapg.Dispose();
				map.Dispose();
			}
			else
			{
				return;
			}
			if (!isAdmin(QQid)) return;
			if (!Regex.IsMatch(msg, "(?<x1>[A-Z]+)(?<y1>[0-9]+)-(?<x2>[A-Z]+)(?<y2>[0-9]+)") && !msg.Contains("all")) return;
			CQ.DeleteMsg(msgID);
			if (msg.Contains("all"))
			{
				view = fullview;
				map = (Image)orimap.Clone();
				mapg = Graphics.FromImage(map);
				DrawMapMark();
				CQ.SendPrivateMessage(QQid, SendMap(map));
				return;
			}
			Match m = Regex.Match(msg, "(?<x1>[A-Z]+)(?<y1>[0-9]+)-(?<x2>[A-Z]+)(?<y2>[0-9]+)");
			int x1 = letter.IndexOf(m.Groups["x1"].ToString());
			int x2 = letter.IndexOf(m.Groups["x2"].ToString());
			int y1 = int.Parse(m.Groups["y1"].ToString()) - 1;
			int y2 = int.Parse(m.Groups["y2"].ToString()) - 1;
			view = new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
			map = (Image)orimap.Clone();
			mapg = Graphics.FromImage(map);
			Pen p = new Pen(Color.Red, 7);
			mapg.DrawLine(p, view.X * blockW, view.Y * blockH, (view.X + view.Width) * blockW, view.Y * blockH);
			mapg.DrawLine(p, view.X * blockW, view.Y * blockH, view.X * blockW, (view.Y + view.Height) * blockH);
			mapg.DrawLine(p, (view.X + view.Width) * blockW, view.Y * blockH, (view.X + view.Width) * blockW, (view.Y + view.Height) * blockH);
			mapg.DrawLine(p, view.X * blockW, (view.Y + view.Height) * blockH, (view.X + view.Width) * blockW, (view.Y + view.Height) * blockH);
			DrawMapMark();
			CQ.SendPrivateMessage(QQid, SendMap(map));
		}

		public void DrawMapMark(int x = 0, int y = 0)
		{
			Pen line = new Pen(Color.Red, 5);
			Brush br = Brushes.White;
			Font ft;
			int i;
			for (i = x; i <= col - 1; i++)
			{
				ft = new Font("微软雅黑", 20f, FontStyle.Bold);
				mapg.DrawString(letter[i], ft, Brushes.White, new PointF(blockW * i + blockW * 0.4f, 5 + blockW * y));
				ft = new Font("微软雅黑", 18);
				mapg.DrawString(letter[i], ft, Brushes.Blue, new PointF(blockW * i + blockW * 0.4f, 5 + blockW * y));
			}
			for (i = y; i <= row - 1; i++)
			{
				ft = new Font("微软雅黑", 20f, FontStyle.Bold);
				mapg.DrawString((i + 1).ToString(), ft, Brushes.Blue, new PointF(5 + blockH * x, blockH * i + blockH * 0.4f));
				ft = new Font("微软雅黑", 18);
				mapg.DrawString((i + 1).ToString(), ft, Brushes.White, new PointF(5 + blockH * x, blockH * i + blockH * 0.4f));
			}
		}

		public void MoveIcon(long QQid, string msg, int msgID)
		{
			if (!Regex.IsMatch(msg, "(?<x>[A-Z]+)(?<y>[0-9]+)")) return;
			CQ.DeleteMsg(msgID);
			Match m = Regex.Match(msg, "(?<x>[A-Z]+)(?<y>[0-9]+)");
			Regex id = new Regex("[csmn][0-9]+");
			int x = letter.IndexOf(m.Groups["x"].ToString());
			int y = int.Parse(m.Groups["y"].ToString()) - 1;
			DataRow[] drs;
			if (id.IsMatch(msg))
			{ 
				drs = Icons.Select("id = '" + id.Match(msg).ToString() + "'");
				if (drs.Length == 1)
				{
					if ((string)drs[0]["owner"] == QQid.ToString() || isAdmin(QQid))
					{
						IconCounter[new Point((int)drs[0]["X"], (int)drs[0]["Y"])]--;
						if (IconCounter.ContainsKey(new Point(x, y))) IconCounter[new Point(x, y)]++;
						else IconCounter.Add(new Point(x, y), 1);
						drs[0]["X"] = x;
						drs[0]["Y"] = y;
					}
				}
			}
			else
			{
				if (msg.StartsWith(".movs"))
				{
					drs = Icons.Select("owner = " + QQid + "AND id LIKE 's*'");
					if (drs.Length == 1)
					{
						IconCounter[new Point((int)drs[0]["X"], (int)drs[0]["Y"])]--;
						if (IconCounter.ContainsKey(new Point(x, y))) IconCounter[new Point(x, y)]++;
						else IconCounter.Add(new Point(x, y), 1);
						drs[0]["X"] = x;
						drs[0]["Y"] = y;
					}
				}
				else
				{
					drs = Icons.Select("owner = " + QQid + "AND id LIKE 'c*'");
					if (drs.Length == 1)
					{
						IconCounter[new Point((int)drs[0]["X"], (int)drs[0]["Y"])]--;
						if (IconCounter.ContainsKey(new Point(x, y))) IconCounter[new Point(x, y)]++;
						else IconCounter.Add(new Point(x, y), 1);
						drs[0]["X"] = x;
						drs[0]["Y"] = y;
					}
				}
			}
			ShowMap(QQid, msg);
		}

		public string SendMap(Image map)
		{
			string savename = "MapTemp" + Guid.NewGuid().ToString("N") + ".jpg";
			//map.Save(CQPath + "\\data\\image\\" + savename, ImageFormat.Jpeg);
			ImageCompress(map, CQPath + "\\data\\image\\" + savename, 50);
			return CQ.CQCode_Image(savename);
		}

		public static bool ImageCompress(Image iSource, string outPath, int flag)
		{
			ImageFormat tFormat = iSource.RawFormat;
			EncoderParameters ep = new EncoderParameters();
			long[] qy = new long[1];
			qy[0] = flag;
			EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
			ep.Param[0] = eParam;
			try
			{
				ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageDecoders();
				ImageCodecInfo jpegICIinfo = null;
				for (int x = 0; x < arrayICI.Length; x++)
				{
					if (arrayICI[x].FormatDescription.Equals("JPEG"))
					{
						jpegICIinfo = arrayICI[x];
						break;
					}
				}
				if (jpegICIinfo != null)
					iSource.Save(outPath, jpegICIinfo, ep);
				else
					iSource.Save(outPath, tFormat);
				iSource.Dispose();
				return true;
			}
			catch(Exception e)
			{
				Tools.SendDebugMessage(e.ToString());
				iSource.Dispose();
				return false;
			}
			
		}


		Dictionary<long, List<FileInfo>> SearchMenu = new Dictionary<long, List<FileInfo>>();
		Dictionary<long, int> lastSearchMenu = new Dictionary<long, int>();

		public void Search(long QQid, string msg, int msgID)
		{
			msg = msg.Replace(".s", "").Replace("。s", "");
			string[] msgs = msg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			DirectoryInfo d = new DirectoryInfo(CSPath + "\\Data");
			int res = 0;
			if (lastSearchMenu.ContainsKey(QQid))
			{
				CQ.DeleteMsg(lastSearchMenu[QQid]);
				lastSearchMenu.Remove(QQid);
			}
			if (!SearchMenu.ContainsKey(QQid) || !Regex.IsMatch(msgs[0], "[0-9]+") || !int.TryParse(msgs[0],out res) || res < 1 || res > SearchMenu[QQid].Count) 
			{
				SearchMenu.Remove(QQid);
				
				List<FileInfo> NewMenu = new List<FileInfo>();
				SearchMenu.Add(QQid, new List<FileInfo>(d.GetFiles("*.jpg",SearchOption.AllDirectories)));
				
				foreach (FileInfo fi in SearchMenu[QQid])
				{
					foreach (string str in msgs)
					{
						if (str.StartsWith("^"))
						{

							if (fi.Name.Contains(str.Substring(1)) && fi.Name.ToLower().Contains(str.Substring(1)))
							{
								goto bk;
							}
						}
						else
						{
							if (!fi.Name.Contains(str) && !fi.Name.ToLower().Contains(str))
							{
								goto bk;
							}
						}
						
					}
					NewMenu.Add(fi);
					bk: continue;
				}

				if (NewMenu.Count == 1)
				{
					File.Copy(NewMenu[0].FullName, CQPath + "\\data\\image\\" + NewMenu[0].Name, true);
					Send(CQ.CQCode_Image(NewMenu[0].Name.Replace("&", "&amp;").Replace(",", "&#44;").Replace("[", "&#91;").Replace("]", "&#93;")));
					SearchMenu.Remove(QQid);
				}
				else
				{
					string rtmsg = string.Format("[{0}]查找到了{1}项:", CQ.CQCode_At(QQid), NewMenu.Count);
					if (NewMenu.Count > 10)
					{
						rtmsg += "\n匹配项目过多，仅显示随机10项，建议更换或添加关键字";
						while (NewMenu.Count > 10)
						{
							NewMenu.RemoveAt(rd.Next(0, NewMenu.Count));
						}
						//NewMenu.RemoveRange(10, NewMenu.Count - 10);
					}
					SearchMenu[QQid] = NewMenu;
					foreach (FileInfo fi in NewMenu)
					{
						rtmsg += "\n" + (NewMenu.IndexOf(fi) + 1).ToString() + "." + fi.Name.Replace(fi.Extension, "");
					}
					if (NewMenu.Count > 0) rtmsg += "\n请输入.s+序号";
					else rtmsg += "\n请更换关键字后重试";
					lastSearchMenu.Add(QQid, Send(rtmsg));
				}
			}
			else
			{
				CQ.DeleteMsg(msgID);
				FileInfo sel = SearchMenu[QQid][res - 1];
				File.Copy(sel.FullName, CQPath + "\\data\\image\\" + sel.Name, true);
				Send(CQ.CQCode_Image(sel.Name.Replace("&", "&amp;").Replace(",", "&#44;").Replace("[", "&#91;").Replace("]", "&#93;")));
				SearchMenu.Remove(QQid);
			}
		}

		public void CharSelection(long QQid)
		{
			Dictionary<string, string> menu = new Dictionary<string, string>();
			DirectoryInfo d = new DirectoryInfo(CSPath + "\\CharSettings");
			foreach (FileInfo f in d.GetFiles("*.ini"))
			{

				if (IniFileHelper.GetStringValue(f.FullName, "CharInfo", "PlayerID", "") == QQid.ToString())
				{
					menu.Add(IniFileHelper.GetStringValue(f.FullName, "CharInfo", "CharID", "0"),
						IniFileHelper.GetStringValue(f.FullName, "CharInfo", "CharName", "Unnamed")
						+"--"+ IniFileHelper.GetStringValue(f.FullName, "CharInfo", "CharDesc", "Unknown"));
				}
			}
			string m = CQ.CQCode_At(QQid) + " 可用角色：\n";
			foreach (KeyValuePair<string, string> e in menu)
			{
				m += e.Key + ". " + e.Value + "\n";
			}
			m += "输入.csel+序号进行选择";
			menu.Clear();
			Send(m, QQid);
		}

		public void CharBind(long QQid, string msg, int msgID)
		{
			string selection = msg.Replace(".csel ", "").Replace("。csel ", "");
			DirectoryInfo d = new DirectoryInfo(CSPath + "\\CharSettings");
			if (CQAT.IsMatch(selection) && isAdmin(QQid))
			{
				QQid = long.Parse(CQAT.Match(selection).ToString().Replace("[CQ:at,qq=", "").Replace("]", ""));
				selection = CQAT.Replace(selection, "");
			}
			foreach (FileInfo f in d.GetFiles("*.ini"))
			{
				if (IniFileHelper.GetStringValue(f.FullName, "CharInfo", "CharID", "") == selection)
				{
					IniFileHelper.WriteValue(CSPath + "\\Config.ini", "G" + GroupID + "_CharBinding", QQid.ToString(), f.FullName);
					if (CharBinding.ContainsKey(QQid))
						CharBinding[QQid] = f.FullName;
					else
						CharBinding.Add(QQid, f.FullName);
				}
			}
			string col;
			string ico;
			if (Logging)
			{
				col = IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "Color", "");
				if (predefColor.Contains(color[QQid]) && !unusedColor.Contains(color[QQid]))
				{
					unusedColor.Add(color[QQid]);
				}
				if (col != "")
				{
					
					color[QQid] = Color.FromName(col);
				}
				else
				{
					color[QQid] = unusedColor[rd.Next(0, unusedColor.Count)];
					unusedColor.Remove(color[QQid]);
				}
				ico = IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "Icon", "");
				if (ico != "")
				{
					face[QQid] = Image.FromFile(CSPath + "\\Icons\\" + ico);
				}
				else
				{
					face[QQid] = Tools.GetFace(QQid);
				}
			}
			if (mapping)
			{
				Icons.Rows.Add("c" + icoid, -1, -1, IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "Size", "x1"),
					CSPath + "\\Icons\\" + IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "Icon", ""), QQid);
				IconCounter[um]++;
				if (IniFileHelper.GetStringValue(CharBinding[QQid], "SideMarco", "SideName", "") != "")
				{
					Icons.Rows.Add("s" + icoid, -1, -1, IniFileHelper.GetStringValue(CharBinding[QQid], "SideMarco", "Size", "x1"),
						CSPath + "\\Icons\\" + IniFileHelper.GetStringValue(CharBinding[QQid], "SideMarco", "Icon", ""), QQid);
					IconCounter[um]++;
				}
				icoid++;
			}
			Send(string.Format("{0} 绑定了角色 {1}", CQ.CQCode_At(QQid),
				IniFileHelper.GetStringValue(CharBinding[QQid].ToString(), "CharInfo", "CharName", "")));
		}

		public void CharDisbinding(long QQid)
		{
			CharBinding.Remove(QQid);
			Send(string.Format("{0} 解除绑定了当前角色", CQ.CQCode_At(QQid)), QQid);
		}

		public void CharModify(long QQid, string key, string value)
		{
			if (CharBinding.ContainsKey(QQid))
			{
				FileStream fs = File.Open(CharBinding[QQid], FileMode.Open);
				FileStream tmp = File.Open(CSPath + "\\CharSettings\\tmp.ini", FileMode.Create);
				StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
				StreamWriter sw = new StreamWriter(tmp, System.Text.Encoding.Default);
				value = value.Replace("&#91;", "[").Replace("&#93;", "]");
				string str;
				bool changed = false;
				bool sectControl = false;
				while (!sr.EndOfStream)
				{
					str = sr.ReadLine();
					if (str.StartsWith("[CharMarco]")) sectControl = true;
					if (sectControl && str.StartsWith(key + "="))
					{
						str = key + "=" + value;
						changed = true;
					}
					
					if (sectControl && str.StartsWith("[") && (str != "[CharMarco]"))
					{
						sectControl = false;
						if (!changed) sw.WriteLine(key + "=" + value);
					}
					sw.WriteLine(str);
				}
				if (sectControl && !changed)
				{
					sw.WriteLine(key + "=" + value);
				}
				sr.Close();
				sw.Close();
				fs.Close();
				tmp.Close();
				File.Replace(CSPath + "\\CharSettings\\tmp.ini", CharBinding[QQid], CSPath + "\\CharSettings\\LastChange.bak");
				Send(string.Format("[{0}]{1}已修改为{2}", CQ.CQCode_At(QQid), key, value), QQid);
			}
			
		}

		public void SideModify(long QQid, string key, string value)
		{
			if (CharBinding.ContainsKey(QQid))
			{
				FileStream fs = File.Open(CharBinding[QQid], FileMode.Open);
				FileStream tmp = File.Open(CSPath + "\\CharSettings\\tmp.ini", FileMode.Create);
				StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
				StreamWriter sw = new StreamWriter(tmp, System.Text.Encoding.Default);
				value = value.Replace("&#91;", "[").Replace("&#93;", "]");
				string str;
				bool changed = false;
				bool sectControl = false;
				while (!sr.EndOfStream)
				{
					str = sr.ReadLine();
					if (str.StartsWith("[SideMarco]")) sectControl = true;
					if (sectControl && str.StartsWith(key + "="))
					{
						str = key + "=" + value;
						changed = true;
					}

					if (sectControl && str.StartsWith("[") && (str != "[SideMarco]"))
					{
						sectControl = false;
						if (!changed) sw.WriteLine(key + "=" + value);
					}
					sw.WriteLine(str);
				}
				if (sectControl && !changed)
				{
					sw.WriteLine(key + "=" + value);
				}
				sr.Close();
				sw.Close();
				fs.Close();
				tmp.Close();
				File.Replace(CSPath + "\\CharSettings\\tmp.ini", CharBinding[QQid], CSPath + "\\CharSettings\\LastChange.bak");
				Send(string.Format("[{0}]{1}已修改为{2}", CQ.CQCode_At(QQid), key, value), QQid);
			}
			
		}

		public void CharRoll(long QQid, string rollstr, int msgID)
		{
			string orirsn = rollstr.Replace(".r ", "");
			if (CharBinding.ContainsKey(QQid))
			{

				foreach (string str in IniFileHelper.GetAllItems(CharBinding[QQid], "CharMarco"))
				{
					rollstr = rollstr.Replace(str.Split('=')[0], str.Split('=')[1]);
				}
				string[] rps, kv;
				foreach (string str in IniFileHelper.GetAllItems(CharBinding[QQid], "MarcoReplace"))
				{
					if (orirsn.Contains(str.Split('=')[0]) && str.Split('=')[0] != "DefaultReplace" 
						&& str.Split('=').Length > 1)  
					{
						rps = str.Split('=')[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string rp in rps)
						{
							kv = rp.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
							if (kv.Length > 1)
							{
								rollstr = rollstr.Replace(kv[0], kv[1]);
							}
						}
					}
				}
				if (IniFileHelper.GetStringValue(CharBinding[QQid], "MarcoReplace", "DefaultReplace", "")
					.Split('=').Length > 1)
				{
					rps = IniFileHelper.GetStringValue(CharBinding[QQid], "MarcoReplace", "DefaultReplace", "")
					.Split('=')[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string rp in rps)
					{
						kv = rp.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
						if (kv.Length > 1)
						{
							rollstr = rollstr.Replace(kv[0], kv[1]);
						}
					}
				}
			}
			string[] substr = rollstr.Split(':');
			string[] rsn;
			string msg = "";
			rsn = new Regex(".r\\s[\\s\\S]*").Match(rollstr).ToString().Split(' ');
			if (rsn.Length > 2)
			{
				msg += String.Format("[{0}]{1}：",
						IniFileHelper.GetStringValue(CharBinding[QQid].ToString(), "CharInfo", "CharName", CQ.CQCode_At(QQid)),
						rsn[2]);
			}
			else
			{
				msg += String.Format("[{0}]{1}：",
						IniFileHelper.GetStringValue(CharBinding[QQid].ToString(), "CharInfo", "CharName", CQ.CQCode_At(QQid)),
						orirsn);
			}
			foreach (string s in substr)
			{
				if (substr.Length > 1) msg += "\n";
				msg += Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"));
				
			}
			Send(msg, QQid);
		}

		public void SideRoll(long QQid, string rollstr, int msgID)
		{
			string orirsn = rollstr.Replace(".rs ", "");
			if (CharBinding.ContainsKey(QQid))
			{

				foreach (string str in IniFileHelper.GetAllItems(CharBinding[QQid], "SideMarco"))
				{
					rollstr = rollstr.Replace(str.Split('=')[0], str.Split('=')[1]);
				}
				string[] rps, kv;
				foreach (string str in IniFileHelper.GetAllItems(CharBinding[QQid], "MarcoReplace"))
				{
					if (orirsn.Contains(str.Split('=')[0]))
					{
						rps = str.Split('=')[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string rp in rps)
						{
							kv = rp.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
							if (kv.Length > 1)
							{
								rollstr = rollstr.Replace(kv[0], kv[1]);
							}
						}
					}
				}
				rps = IniFileHelper.GetStringValue(CharBinding[QQid], "MarcoReplace", "DefaultReplace", "")
					.Split('=')[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string rp in rps)
				{
					kv = rp.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
					if (kv.Length > 1)
					{
						rollstr = rollstr.Replace(kv[0], kv[1]);
					}
				}
			}
			string[] substr = rollstr.Split(':');
			string[] rsn;
			string msg = "";
			rsn = new Regex(".rs\\s[\\s\\S]*").Match(rollstr).ToString().Split(' ');
			if (rsn.Length > 2)
			{
				msg += String.Format("[{0}]{1}-{2}：",
						IniFileHelper.GetStringValue(CharBinding[QQid].ToString(), "CharInfo", "CharName", CQ.CQCode_At(QQid)),
						IniFileHelper.GetStringValue(CharBinding[QQid].ToString(), "SideMarco", "SideName", "???"),
						rsn[2]);
			}
			else
			{
				msg += String.Format("[{0}]{1}-{2}：",
						IniFileHelper.GetStringValue(CharBinding[QQid].ToString(), "CharInfo", "CharName", CQ.CQCode_At(QQid)),
						IniFileHelper.GetStringValue(CharBinding[QQid].ToString(), "SideMarco", "SideName", "???"),
						orirsn);
			}
			foreach (string s in substr)
			{
				if (substr.Length > 1) msg += "\n";
				msg += Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"));

			}
			Send(msg, QQid);
		}

		public void Memory(long QQid, string msg, int msgID)
		{

			Regex at = new Regex("\\[CQ:at,qq=[0-9]*\\]");
			string key = at.Replace(msg, "").Replace(" ", "").Replace(".m", "");
			string rtn = string.Format("{0}, {1}的查询结果为：", CQ.CQCode_At(QQid), key);
			long qq;
			foreach (Match m in at.Matches(msg))
			{
				qq = long.Parse(m.ToString().Replace("[CQ:at,qq=", "").Replace("]", ""));
				if (CharBinding.ContainsKey(qq))
				{
					rtn += string.Format("\n{0}：{1}",
						IniFileHelper.GetStringValue(CharBinding[qq], "CharInfo", "CharName", CQ.CQCode_At(qq)),
						IniFileHelper.GetStringValue(CharBinding[qq], "CharMemo", key, "未找到")
						.Replace("CT:", "").Replace("LT:", "")
						.Replace(";", "\n").Replace("[", "&#91;").Replace("]", "&#93;"));
				}
			}
			if (at.Matches(msg).Count == 0)
			{
				rtn += string.Format("\n{0}：{1}",
						IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "CharName", CQ.CQCode_At(QQid)),
						IniFileHelper.GetStringValue(CharBinding[QQid], "CharMemo", key, "未找到")
						.Replace("CT:", "").Replace("LT:", "").Replace(";", "\n").Replace("[", "&#91;").Replace("]", "&#93;"));
			}
			Send(rtn, QQid);
		}

		public void MemorySet(long QQid, string key, string value, bool quiet = false)
		{
			if (CharBinding.ContainsKey(QQid))
			{
				FileStream fs = File.Open(CharBinding[QQid], FileMode.Open);
				FileStream tmp = File.Open(CSPath + "\\CharSettings\\tmp.ini", FileMode.Create);
				StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
				StreamWriter sw = new StreamWriter(tmp, System.Text.Encoding.Default);
				value = value.Replace("&#91;", "[").Replace("&#93;", "]");
				string str;
				bool changed = false;
				bool sectControl = false;
				while (!sr.EndOfStream)
				{
					str = sr.ReadLine();
					if (str.StartsWith("[CharMemo]")) sectControl = true;
					if (sectControl && str.StartsWith(key + "="))
					{
						str = key + "=" + value;
						changed = true;
					}

					if (sectControl && str.StartsWith("[") && (str != "[CharMemo]"))
					{
						sectControl = false;
						if (!changed) sw.WriteLine(key + "=" + value);
					}
					sw.WriteLine(str);
				}
				if (sectControl && !changed)
				{
					sw.WriteLine(key + "=" + value);
				}
				sr.Close();
				sw.Close();
				fs.Close();
				tmp.Close();
				File.Replace(CSPath + "\\CharSettings\\tmp.ini", CharBinding[QQid], CSPath + "\\CharSettings\\LastChange.bak");
				if (!quiet) Send(string.Format("[{0}]{1}现为:{2}", 
					IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "CharName", CQ.CQCode_At(QQid)),
					  key, value.Replace("CT:", "").Replace("LT:", "")), QQid);//.Replace("CT:", "计数：").Replace("LT:", "列表：")
			}
		}

		public void CounterSet(long QQid, string key, string value, bool quiet = false)
		{
			string oristr = IniFileHelper.GetStringValue(CharBinding[QQid], "CharMemo", key, "");
			if (!oristr.StartsWith("CT:")) return;
			string maxstr = IniFileHelper.GetStringValue(CharBinding[QQid], "CharMemo", key + "-Max", "");
			Regex desc = new Regex("\\(\\S+?\\)");
			string descstr = desc.Match(oristr).ToString();
			oristr = desc.Replace(oristr, "");
			float orivalue = float.Parse(oristr.Substring(3));
			float maxvalue = float.MaxValue;
			if (maxstr != "") maxvalue = float.Parse(maxstr);
			value = value.Replace("+ ", "+").Replace("- ", "-").Replace("+ ", "+").Replace("- ", "-");
			float vvalue;
			if (value.StartsWith("="))
			{
				vvalue = float.Parse(value.Substring(1));
				if (vvalue <= maxvalue) MemorySet(QQid, key, "CT:" + vvalue + descstr, quiet);
				else { MemorySet(QQid, key, "CT:" + maxvalue + descstr, quiet); }
			}
			else if (value.StartsWith("-"))
			{
				vvalue = float.Parse(value.Substring(1));
				MemorySet(QQid, key, "CT:" + (orivalue - vvalue) + descstr, quiet);
			}
			else if (value.StartsWith("+"))
			{
				vvalue = float.Parse(value.Substring(1));
				if (orivalue+vvalue <= maxvalue) MemorySet(QQid, key, "CT:" + (orivalue + vvalue) + descstr, quiet);
				else { MemorySet(QQid, key, "CT:" + maxvalue + descstr, quiet); }
			}


		}

		public void ListSet(long QQid, string key, string msg, bool quiet = false)
		{
			if (!IniFileHelper.GetStringValue(CharBinding[QQid], "CharMemo", key, "").StartsWith("LT:")) return;
			msg = msg.Replace(".ltset " + key + " ", "");
			msg = msg.Replace("+ ", "+").Replace("- ", "-").Replace("+ ", "+").Replace("- ", "-");
			if (msg.StartsWith("="))
			{
				MemorySet(QQid, key, "LT:" + msg.Substring(1), quiet);
				return;
			}
			Dictionary<string, int> items = new Dictionary<string, int>();
			string[] opt = msg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			int num;
			string name;
			Regex itemnum = new Regex("x[0-9]+");
			foreach (string str in (IniFileHelper.GetStringValue(CharBinding[QQid], "CharMemo", key, "")
				.Substring(3).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))) 
			{
				num = Tools.DiceNum("+" + itemnum.Match(str).ToString().Replace("x", ""));
				name = itemnum.Replace(str, "").ToString();
				items.Add(name, num == 0 ? 1 : num);
			}
			foreach (string str in opt)
			{
				if (str.StartsWith("+"))
				{
					num = Tools.DiceNum("+" + itemnum.Match(str).ToString().Replace("x", ""));
					name = itemnum.Replace(str, "").ToString().Substring(1);
					if (num == 0) num = 1;
					if (items.ContainsKey(name))
					{
						items[name] += num;
					}
					else
					{
						items.Add(name, num);
					}
				}
				else if (str.StartsWith("-"))
				{
					num = Tools.DiceNum("+" + itemnum.Match(str).ToString().Replace("x", ""));
					name = itemnum.Replace(str, "").ToString().Substring(1);
					if (num == 0) num = 1;
					if (items.ContainsKey(name))
					{
						items[name] -= num;
					}
					else
					{
						Send("没有" + name, QQid);
						return;
					}
					if (items.ContainsKey(name) && items[name] == 0) items.Remove(name);
					if (items.ContainsKey(name) && items[name] < 0)
					{
						Send(name + "不足", QQid);
						return;
					}
				}
			}
			string rtnstr = "";
			foreach (KeyValuePair<string,int> i in items)
			{
				rtnstr += ";" + i.Key + "x" + i.Value;
			}
			if (rtnstr.StartsWith(";")) rtnstr = rtnstr.Substring(1);
			MemorySet(QQid, key, "LT:" + rtnstr, quiet);

		}

		public void SideMemory(long QQid, string msg, int msgID)
		{
			Regex at = new Regex("\\[CQ:at,qq=[0-9]*\\]");
			string key = at.Replace(msg, "").Replace(" ", "").Replace(".ms", "");
			string rtn = string.Format("{0}, {1}的查询结果为：", CQ.CQCode_At(QQid), key);
			long qq;
			foreach (Match m in at.Matches(msg))
			{
				qq = long.Parse(m.ToString().Replace("[CQ:at,qq=", "").Replace("]", ""));
				if (CharBinding.ContainsKey(qq))
				{
					rtn += string.Format("\n{0}：{1}",
						IniFileHelper.GetStringValue(CharBinding[qq], "SideMarco", "SideName", CQ.CQCode_At(qq)),
						IniFileHelper.GetStringValue(CharBinding[qq], "SideMemo", key, "未找到").Replace(";", "\n").Replace("[", "&#91;").Replace("]", "&#93;").Replace("CT:", ""));
				}
			}
			if (at.Matches(msg).Count == 0)
			{
				rtn += string.Format("\n{0}：{1}",
						IniFileHelper.GetStringValue(CharBinding[QQid], "SideMarco", "SideName", CQ.CQCode_At(QQid)),
						IniFileHelper.GetStringValue(CharBinding[QQid], "SideMemo", key, "未找到").Replace(";", "\n").Replace("[", "&#91;").Replace("]", "&#93;").Replace("CT:", ""));
			}
			Send(rtn, QQid);
		}

		public void SideMemorySet(long QQid, string key, string value, bool quiet = false)
		{
			if (CharBinding.ContainsKey(QQid))
			{
				FileStream fs = File.Open(CharBinding[QQid], FileMode.Open);
				FileStream tmp = File.Open(CSPath + "\\CharSettings\\tmp.ini", FileMode.Create);
				StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
				StreamWriter sw = new StreamWriter(tmp, System.Text.Encoding.Default);
				value = value.Replace("&#91;", "[").Replace("&#93;", "]");
				string str;
				bool changed = false;
				bool sectControl = false;
				while (!sr.EndOfStream)
				{
					str = sr.ReadLine();
					if (str.StartsWith("[SideMemo]")) sectControl = true;
					if (sectControl && str.StartsWith(key + "="))
					{
						str = key + "=" + value;
						changed = true;
					}

					if (sectControl && str.StartsWith("[") && (str != "[SideMemo]"))
					{
						sectControl = false;
						if (!changed) sw.WriteLine(key + "=" + value);
					}
					sw.WriteLine(str);
				}
				if (sectControl && !changed)
				{
					sw.WriteLine(key + "=" + value);
				}
				sr.Close();
				sw.Close();
				fs.Close();
				tmp.Close();
				File.Replace(CSPath + "\\CharSettings\\tmp.ini", CharBinding[QQid], CSPath + "\\CharSettings\\LastChange.bak");
				if (!quiet) Send(string.Format("[{0}]{1}-{2}现为:{3}", 
					IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "CharName", CQ.CQCode_At(QQid)),
					  IniFileHelper.GetStringValue(CharBinding[QQid], "SideMarco", "SideName", ""),
					  key, value.Replace("CT:", "").Replace("LT:", "")), QQid);//.Replace("CT:", "计数器：").Replace("LT:", "列表：")
			}

		}


		public void SideCounterSet(long QQid, string key, string value, bool quiet = false)
		{
			string oristr = IniFileHelper.GetStringValue(CharBinding[QQid], "SideMemo", key, "");
			if (!oristr.StartsWith("CT:")) return;
			string maxstr = IniFileHelper.GetStringValue(CharBinding[QQid], "SideMemo", key + "-Max", "");
			Regex desc = new Regex("\\(\\S+?\\)");
			string descstr = desc.Match(oristr).ToString();
			oristr = desc.Replace(oristr, "");
			float orivalue = float.Parse(oristr.Substring(3));
			float maxvalue = float.MaxValue;
			if (maxstr != "") maxvalue = float.Parse(maxstr);
			float vvalue;
			value = value.Replace("+ ", "+").Replace("- ", "-").Replace("+ ", "+").Replace("- ", "-");
			if (value.StartsWith("="))
			{
				vvalue = float.Parse(value.Substring(1));
				if (vvalue <= maxvalue) SideMemorySet(QQid, key, "CT:" + vvalue + descstr, quiet);
				else { SideMemorySet(QQid, key, "CT:" + maxvalue + descstr, quiet); }
			}
			else if (value.StartsWith("-"))
			{
				vvalue = float.Parse(value.Substring(1));
				SideMemorySet(QQid, key, "CT:" + (orivalue - vvalue) + descstr, quiet);
			}
			else if (value.StartsWith("+"))
			{
				vvalue = float.Parse(value.Substring(1));
				if (orivalue + vvalue <= maxvalue) SideMemorySet(QQid, key, "CT:" + (orivalue + vvalue) + descstr, quiet);
				else { SideMemorySet(QQid, key, "CT:" + maxvalue + descstr, quiet); }
			}


		}

		public void SideListSet(long QQid, string key, string msg, bool quiet = false)
		{
			if (!IniFileHelper.GetStringValue(CharBinding[QQid], "SideMemo", key, "").StartsWith("LT:")) return;
			msg = msg.Replace(".ltset " + key + " ", "");
			msg = msg.Replace("+ ", "+").Replace("- ", "-").Replace("+ ", "+").Replace("- ", "-");
			if (msg.StartsWith("="))
			{
				MemorySet(QQid, key, "LT:" + msg.Substring(1), quiet);
				return;
			}
			Dictionary<string, int> items = new Dictionary<string, int>();
			string[] opt = msg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			int num;
			string name;
			Regex itemnum = new Regex("x[0-9]+");
			foreach (string str in (IniFileHelper.GetStringValue(CharBinding[QQid], "SideMemo", key, "")
				.Substring(3).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)))
			{
				num = Tools.DiceNum("+" + itemnum.Match(str).ToString().Replace("x", ""));
				name = itemnum.Replace(str, "").ToString();
				items.Add(name, num == 0 ? 1 : num);
			}
			foreach (string str in opt)
			{
				if (str.StartsWith("+"))
				{
					num = Tools.DiceNum("+" + itemnum.Match(str).ToString().Replace("x", ""));
					name = itemnum.Replace(str, "").ToString().Substring(1);
					if (num == 0) num = 1;
					if (items.ContainsKey(name))
					{
						items[name] += num;
					}
					else
					{
						items.Add(name, num);
					}
				}
				else if (str.StartsWith("-"))
				{
					num = Tools.DiceNum("+" + itemnum.Match(str).ToString().Replace("x", ""));
					name = itemnum.Replace(str, "").ToString().Substring(1);
					if (num == 0) num = 1;
					if (items.ContainsKey(name))
					{
						items[name] -= num;
					}
					else
					{
						Send("没有" + name, QQid);
						return;
					}
					if (items.ContainsKey(name) && items[name] == 0) items.Remove(name);
					if (items.ContainsKey(name) && items[name] < 0)
					{
						Send(name + "不足", QQid);
						return;
					}
				}
			}
			string rtnstr = "";
			foreach (KeyValuePair<string, int> i in items)
			{
				rtnstr += ";" + i.Key + "x" + i.Value;
			}
			if (rtnstr.StartsWith(";")) rtnstr = rtnstr.Substring(1);
			SideMemorySet(QQid, key, "LT:" + rtnstr, quiet);

		}

		public void Rest(long QQid, string msg, int msgID)
		{
			if (!isAdmin(QQid)) return;
			List<long> optqq = new List<long>();
			string orikey;
			string rtnstr = "经过休息……";
			if (msg.Contains("Me")) optqq.Add(QQid);
			foreach (Match m in CQAT.Matches(msg))
			{
				optqq.Add(long.Parse(m.ToString().Replace("[CQ:at,qq=", "").Replace("]", "")));
			}
			if (optqq.Count == 0) optqq.AddRange(CharBinding.Keys);
			string prechange;
			string postchange;
			string premsg;
			foreach (long qq in optqq)
			{
				if (!CharBinding.ContainsKey(qq)) continue;
				rtnstr += "\n" + IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "CharInfo", "CharName", "") + ":";
				premsg = rtnstr;
				foreach (string key in IniFileHelper.GetAllItemKeys(CharBinding[qq], "CharMemo"))
				{
					if (key.EndsWith("-Rest"))
					{
						orikey = key.Replace("-Rest", "");
						prechange = IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "CharMemo", orikey, "");
						if (prechange.StartsWith("CT:"))
						{
							CounterSet(qq, orikey, IniFileHelper.GetStringValue(CharBinding[qq], "CharMemo", key, ""), true);
							postchange = IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "CharMemo", orikey, "");
							if (prechange != postchange) rtnstr += "\n" + orikey + " 恢复至：" + postchange.Replace("CT:", "");

						}
						else if (prechange.StartsWith("LT:"))
						{
							ListSet(qq, orikey, IniFileHelper.GetStringValue(CharBinding[qq], "CharMemo", key, ""), true);
							postchange = IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "CharMemo", orikey, "");
							if (prechange != postchange) rtnstr += "\n" + orikey + " 恢复为：" + postchange.Replace("LT:", "");

						}
					}
				}
				if (premsg == rtnstr) rtnstr += " 没有变化";
				if (IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "SideMarco", "SideName", "") != "")
				{
					rtnstr += "\n" + IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "SideMarco", "SideName", "") + ":";
					premsg = rtnstr;
					foreach (string key in IniFileHelper.GetAllItemKeys(CharBinding[qq], "SideMemo"))
					{
						if (key.EndsWith("-Rest"))
						{
							orikey = key.Replace("-Rest", "");
							prechange = IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "CharMemo", orikey, "");
							if (prechange.StartsWith("CT:"))
							{
								SideCounterSet(qq, orikey, IniFileHelper.GetStringValue(CharBinding[qq], "SideMemo", key, ""), true);
								postchange = IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "SideMemo", orikey, "");
								if (prechange != postchange) rtnstr += "\n" + orikey + " 恢复至：" + postchange.Replace("CT:", "");

							}
							else if (prechange.StartsWith("LT:"))
							{
								SideListSet(qq, orikey, IniFileHelper.GetStringValue(CharBinding[qq], "SideMemo", key, ""), true);
								postchange = IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "SideMemo", orikey, "");
								if (prechange != postchange) rtnstr += "\n" + orikey + " 恢复为：" + postchange.Replace("LT:", "");

							}
						}
					}
					if (premsg == rtnstr) rtnstr += " 没有变化";
				}
			}
			Send(rtnstr, QQid);
		}

		public void SetDM(long QQid, string msg, int msgID)
		{
			if (!isAdmin(QQid)) return;
			long qq;
			foreach (Match m in CQAT.Matches(msg))
			{
				qq = long.Parse(m.ToString().Replace("[CQ:at,qq=", "").Replace("]", ""));
				if (isAdmin(qq) && Owner != qq)
				{
					if (qq != long.Parse(IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "GeneralSetting", "DebugID", ""))) Admin.Remove(qq);
					Send(string.Format("{0}已被取消DM", GetName(qq)), QQid);
				}
				else
				{
					Admin.Add(qq);
					Send(string.Format("{0}已被设置为DM", GetName(qq)), QQid);
				}
				
			}
		}

		

		DataTable GHIDT = new DataTable();
		Dictionary<long, int> GHIcounter = new Dictionary<long, int>();
		Dictionary<long, int> GHIsidecounter = new Dictionary<long, int>();
		static List<string> GHIdice = new List<string>(new string[] { "+1", "d2", "d3", "d4", "d6", "d8", "d10", "d12" });
		int lastInitList = 0;
		string GHILastList = "";

		public void GHI(long QQid, string msg, int msgID)
		{
			if (!CharBinding.ContainsKey(QQid)) return;
			if (GHIDT.Columns.Count == 0)
			{
				GHIDT.Columns.Add("Dice", typeof(String));
				GHIDT.Columns.Add("Roll", typeof(int));
				GHIDT.Columns.Add("Dex", typeof(int));
				GHIDT.Columns.Add("Action", typeof(String));
				GHIDT.Columns.Add("ID", typeof(long));
				GHIDT.Columns.Add("Result", typeof(int));
				GHIDT.Columns.Add("Reroll", typeof(String));
			}
			if (!GHIcounter.ContainsKey(QQid)) GHIcounter.Add(QQid, 0);
			if (!GHIsidecounter.ContainsKey(QQid)) GHIsidecounter.Add(QQid, 0);
			string rtmsg = "";
			string dice = "";
			string action = msg;
			if (msg.ToLower() == "go")
			{
				GHILastList = GHIGO();
				if (isAdmin(QQid)) Send(GHILastList, QQid);
				return;
			}
			else if (msg.ToLower() == "last")
			{
				Send(GHILastList, QQid);
				return;
			}
			foreach (string k in IniFileHelper.GetAllItemKeys(CharBinding[QQid], "InitSetting"))
			{
				if (msg.Contains(k)) msg += IniFileHelper.GetStringValue(CharBinding[QQid], "InitSetting", k, "");
			}
			if (Regex.Match(msg, "(d[0-9]+)|(\\+[0-9]+)").Success)
			{
				dice = Regex.Match(msg, "(d[0-9]+)|(\\+[0-9]+)").ToString();
				action = Regex.Replace(msg, "(d[0-9]+)|(\\+[0-9]+)", "");
			}
			else if (msg.Contains("重攻击"))
			{
				dice = "d12";
			}
			else if (msg.Contains("轻攻击") || msg.Contains("射击"))
			{
				dice = "d4";
			}
			else if (msg.Contains("施放") || msg.Contains("施法") || msg.Contains("攻击"))
			{
				dice = "d8";
			}
			else
			{
				dice = "d6";
			}
			int dex;
			if (msg.StartsWith("s-"))
			{
				dex = Tools.DiceNum(IniFileHelper.GetStringValue(CharBinding[(long)QQid], "SideMarco", "先攻", "???")
					.Replace("d20", "+0"));
			}
			else
			{
				dex = Tools.DiceNum(IniFileHelper.GetStringValue(CharBinding[(long)QQid], "CharMarco", "先攻", "???")
					.Replace("d20", "+0"));
			}
			
			if (dex >= 6)
			{
				if(GHIdice.Contains(dice))
					dice = GHIdice[(GHIdice.IndexOf(dice) - dex / 6) >= 0 ? GHIdice.IndexOf(dice) - dex / 6 : 0];
			}
			GHIDT.Rows.Add(dice, 0, 0, action, QQid, 0, "");
			if (rtmsg == "")
			{
				rtmsg = "先攻：";
				foreach (DataRow dr in GHIDT.Rows)
				{
					if (((string)dr["Action"]).StartsWith("s-"))
					{
						rtmsg += string.Format("\n{0}-{1}:{2}",
								IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "SideMarco", "SideName", "???"),
								(((string)dr["Action"]).StartsWith("s-h") || ((string)dr["Action"]).StartsWith("s-l"))
								? ((string)dr["Action"]).Substring(3) : ((string)dr["Action"]).Substring(2), dr["Dice"]);
					}
					else
					{
						rtmsg += string.Format("\n{0}-{1}:{2}",
								IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "CharInfo", "CharName", "???"),
								(((string)dr["Action"]).StartsWith("h") || ((string)dr["Action"]).StartsWith("l"))
								? ((string)dr["Action"]).Substring(1) : dr["Action"], dr["Dice"]);
					}
						
				}
			}
			CQ.DeleteMsg(lastInitList);
			lastInitList = Send(rtmsg, QQid);
		}

		public string GHIGO()
		{
			int d;
			CQ.DeleteMsg(lastInitList);
			string rtmsg = "先攻：";
			foreach (DataRow dr in GHIDT.Rows)
			{
				if (((string)dr["Action"]).StartsWith("s-"))
				{
					dr["Dex"] = int.Parse(IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "SideMarco", "敏捷", "???")
						.Replace("d20", "").Replace("[敏捷]", ""));
				}
				else
				{
					dr["Dex"] = int.Parse(IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "CharMarco", "敏捷", "???")
						.Replace("d20", "").Replace("[敏捷]", ""));
				}
					
			}
			foreach (DataRow dr in GHIDT.Rows)
			{
				dr["Roll"] = Tools.DiceNum((string)dr["Dice"]);
				d = int.Parse(((string)dr["Dice"]).Substring(1));
				if (((string)dr["Action"]).StartsWith("s-"))
				{
					if (IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "SideMemo", "专长", "").Contains("精通先攻")
						&& ((int)dr["Roll"] > d - (int)(d / 4)))
					{
						dr["Reroll"] = dr["Roll"] + "=>";
						dr["Roll"] = Tools.DiceNum((string)dr["Dice"]);
					}
				}
				else
				{
					if (IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "CharMemo", "专长", "").Contains("精通先攻")
						&& ((int)dr["Roll"] > d - (int)(d / 4)))
					{
						dr["Reroll"] = dr["Roll"] + "=>";
						dr["Roll"] = Tools.DiceNum((string)dr["Dice"]);
					}
				}
				
			}
			//GHIDT.DefaultView.Sort = "Roll,Dex Desc";
			//GHIDT = GHIDT.DefaultView.ToTable();
			foreach (DataRow dr in GHIDT.Rows)
			{
				if (((string)dr["Action"]).StartsWith("s-"))
				{
					GHIsidecounter[(long)dr["ID"]] += (int)dr["Roll"];
					dr["Result"] = GHIsidecounter[(long)dr["ID"]];
				}
				else
				{
					GHIcounter[(long)dr["ID"]] += (int)dr["Roll"];
					dr["Result"] = GHIcounter[(long)dr["ID"]];
				}
					
			}
			GHIDT.DefaultView.Sort = "Result,Dex Desc";
			GHIDT = GHIDT.DefaultView.ToTable();
			foreach (DataRow dr in GHIDT.Rows)
			{
				if (((string)dr["Action"]).StartsWith("s-"))
				{
					rtmsg += string.Format("\n{0}-{1}:{2}({3}{4})",
						IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "SideMarco", "SideName", "???"),
						((string)dr["Action"]).Substring(2), dr["Result"], dr["Reroll"], dr["Roll"]);
				}
				else
				{
					rtmsg += string.Format("\n{0}-{1}:{2}({3}{4})",
						IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "CharInfo", "CharName", "???"),
						dr["Action"], dr["Result"], dr["Reroll"], dr["Roll"]);
				}
				
			}
			GHIDT.Clear();
			GHIcounter.Clear();
			GHIsidecounter.Clear();
			return rtmsg;
		}

		public void GHIs(long QQid, string msg, int msgID)
		{
			GHI(QQid, "s-" + msg, msgID);
		}


		public void Roll(long QQid, string rollstr, int msgID)
		{
			CQ.DeleteMsg(msgID);
			if (new Regex(("\\[CQ:at,qq=[0-9]*\\]")).IsMatch(rollstr))
			{
				ARoll(QQid, rollstr, msgID);
				return;
			}
			if (CharBinding.ContainsKey(QQid))
			{
				if (rollstr.Split(' ')[0] == ".rs")
				{
					SideRoll(QQid, rollstr, msgID);
				}
				else
				{
					CharRoll(QQid, rollstr, msgID);
				}
				return;
			}
			string[] substr = rollstr.Split(':');
			string[] rsn = new Regex(".r\\s[\\s\\S]*").Match(rollstr).ToString().Split(' ');
			string msg = "";
			rsn = new Regex(".r\\s[\\s\\S]*").Match(rollstr).ToString().Split(' ');
			if (rsn.Length > 2)
			{
				msg += String.Format("[{0}]{1}：",
						CQ.CQCode_At(QQid),
						rsn[2]);
			}
			else
			{
				msg += String.Format("[{0}]{1}：",
						CQ.CQCode_At(QQid),
						rollstr.Replace(".r ", "").Replace("。r ", ""));
			}
			foreach (string s in substr)
			{
				if (substr.Length > 1) msg += "\n";
				msg += Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"));

			}
			Send(msg, QQid);
			
			/*if (rsn.Length > 2)
			{
				Send(String.Format("[{0}]{1}：{2}", CQ.CQCode_At(QQid), rsn[2], Tools.Dice(rollstr)), QQid);
			}
			else
			{
				Send(String.Format("[{0}]{1}：{2}", CQ.CQCode_At(QQid), rollstr.Replace(".r ", "").Replace("。r ", "", QQid)
					, Tools.Dice(rollstr)));
			}*/
		}

		public void AllRoll(long QQid, string key)
		{
			if (!isAdmin(QQid)) return;
			string msg = "全体 " + key + " 检定结果为：";
			string rollstr, dicestr, origin;
			int result;
			string[] substr;
			DataTable AR = new DataTable();
			AR.Columns.Add("Origin", typeof(String));
			AR.Columns.Add("Dice", typeof(String));
			AR.Columns.Add("Roll", typeof(int));
			AR.DefaultView.Sort = "Roll Desc";
			foreach (KeyValuePair<long, string> c in CharBinding)
			{
				rollstr = key;
				foreach (string str in IniFileHelper.GetAllItems(c.Value, "CharMarco"))
				{
					rollstr = rollstr.Replace(str.Split('=')[0], str.Split('=')[1]);
				}
				substr = rollstr.Split(':');
				if(substr.Length>1) AR.DefaultView.Sort = "Origin, Roll Desc";
				//msg += String.Format("\n[{0}]：",
				//		IniFileHelper.GetStringValue(c.Value.ToString(), "CharInfo", "CharName", CQ.CQCode_At(c.Key)));
				origin = IniFileHelper.GetStringValue(c.Value.ToString(), "CharInfo", "CharName", CQ.CQCode_At(c.Key));
				foreach (string s in substr)
				{
					//if (substr.Length > 1) msg += "\n";
					//msg += Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"));
					dicestr = Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"), out result);
					AR.Rows.Add(origin, dicestr, result);

				}
				if (IniFileHelper.GetStringValue(c.Value.ToString(), "SideMarco", "SideName", "") != "")
				{
					rollstr = key;
					foreach (string str in IniFileHelper.GetAllItems(c.Value, "SideMarco"))
					{
						rollstr = rollstr.Replace(str.Split('=')[0], str.Split('=')[1]);
					}
					substr = rollstr.Split(':');
					if (substr.Length > 1) AR.DefaultView.Sort = "Origin, Roll Desc";
					//msg += String.Format("\n[{0}]：",
					//		IniFileHelper.GetStringValue(c.Value.ToString(), "SideMarco", "SideName", CQ.CQCode_At(c.Key)));
					origin = IniFileHelper.GetStringValue(c.Value.ToString(), "SideMarco", "SideName", CQ.CQCode_At(c.Key));
					foreach (string s in substr)
					{
						//if (substr.Length > 1) msg += "\n";
						//msg += Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"));
						dicestr = Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"), out result);
						AR.Rows.Add(origin, dicestr, result);
					}
				}
			}
			AR = AR.DefaultView.ToTable();
			origin = "";
			foreach (DataRow dr in AR.Rows)
			{
				if ((string)dr["Origin"] != origin)
				{
					origin = (string)dr["Origin"];
					msg += string.Format("\n[{0}]: ", origin);
				}
				msg += string.Format("\n{0}", dr["Dice"]);
			}
			Send(msg, QQid);
		}

		public void ARoll(long QQid, string msg, int msgID)
		{

			string key = CQAT.Replace(msg, "").Replace(" ", "").Replace(".r", "");
			string rtn = string.Format("{0}的检定结果为：", key);
			long qq;
			string rollstr;
			string[] substr;
			foreach (Match m in CQAT.Matches(msg))
			{
				qq = long.Parse(m.ToString().Replace("[CQ:at,qq=", "").Replace("]", ""));
				if (CharBinding.ContainsKey(qq))
				{
					rollstr = key;
					foreach (string str in IniFileHelper.GetAllItems(CharBinding[qq], "CharMarco"))
					{
						rollstr = rollstr.Replace(str.Split('=')[0], str.Split('=')[1]);
					}
					substr = rollstr.Split(':');
					rtn += String.Format("\n[{0}]：",
							IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "CharInfo", "CharName", CQ.CQCode_At(qq)));
					foreach (string s in substr)
					{
						if (substr.Length > 1) rtn += "\n";
						rtn += Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"));

					}
					if (IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "SideMarco", "SideName", "") != "")
					{
						rollstr = key;
						foreach (string str in IniFileHelper.GetAllItems(CharBinding[qq], "SideMarco"))
						{
							rollstr = rollstr.Replace(str.Split('=')[0], str.Split('=')[1]);
						}
						substr = rollstr.Split(':');
						rtn += String.Format("\n[{0}]：",
								IniFileHelper.GetStringValue(CharBinding[qq].ToString(), "SideMarco", "SideName", CQ.CQCode_At(qq)));
						foreach (string s in substr)
						{
							if (substr.Length > 1) rtn += "\n";
							rtn += Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"));

						}
					}
				}
			}
			Send(rtn, QQid);
		}

		public void GroupTalk()
		{

		}

		public void PrivateTalk()
		{

		}

		int helpcount = 1;
		public void Help()
		{
			switch (helpcount)
			{
				case 1:
					Send("喵？(不明所以)");
					break;
				case 2:
					Send("喵~喵~(假装听不懂)");
					break;
				case 3:
					Send("喵~~~~(假装听不懂)");
					break;
				case 4:
					Send(".......(无视)");
					break;
				case 5:
					Send("...(故意无视)");
					break;
				case 6:
					Send(CQ.CQCode_At(long.Parse(IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "GeneralSetting", "DebugID", ""))) + "喵~喵！喵！喵？喵~");
					helpcount = 0;
					break;
			}
			helpcount++;
		}
	}

	class Tools
	{
		static Random rd = new Random();
		static string CSPath = "C:\\酷Q Pro\\CSharpPlugins";
		static string CQPath = "C:\\酷Q Pro";
		static public Object SendDebugMessage(Object msg)
		{
			CQ.SendPrivateMessage(long.Parse(IniFileHelper.GetStringValue(CSPath + "\\Config.ini", "GeneralSetting", "DebugID", "")), msg.ToString());
			return msg;
		}

		public static Image GetImage(string imgID)
		{

			DirectoryInfo di = new DirectoryInfo(CQPath + "\\data\\image\\");
			FileInfo fi = di.GetFiles(imgID + "*.cqimg")[0];
			string url = IniFileHelper.GetStringValue(fi.FullName, "image", "url", "");
			WebRequest request = WebRequest.Create(url);
			WebResponse response = request.GetResponse();
			Stream reader = response.GetResponseStream();
			Image img = Image.FromStream(reader);
			reader.Close();
			reader.Dispose();
			response.Close();
			return img;
		}

		public static Image GetFace(long QQid)
		{
			string url = "http://q1.qlogo.cn/g?b=qq&nk=" + QQid + "&s=100";
			WebRequest request = WebRequest.Create(url);
			WebResponse response = request.GetResponse();
			Stream reader = response.GetResponseStream();
			Image img = Image.FromStream(reader);
			reader.Close();
			reader.Dispose();
			response.Close();
			return img;
		}


		static FileInfo ReceiveImage(string img)
		{
			FileInfo result = null;
			string path = CSPath + "\\Data\\image\\";  //目录  
			string picUrl = IniFileHelper.GetStringValue(path + img + ".cqimg", "image", "url", "");
			
			try
			{
				if (!String.IsNullOrEmpty(picUrl))
				{
					Random rd = new Random();
					DateTime nowTime = DateTime.Now;
					string fileName = img + ".jpg";
					WebClient webClient = new WebClient();
					webClient.DownloadFile(picUrl, path + fileName);

					result = new FileInfo(path + fileName);
				}
			}
			catch { return null; }
			return result;
		}

		static public string Dice(string rollstr)
		{
			string str = "", rtstr = "";
			string[] spl;
			
			Dictionary<int, int> roll = new Dictionary<int, int>();
			Regex d = new Regex("((\\+|\\-)?[0-9]*(d[0-9]+)(h|l|r)?([0-9]*)?((&#91;\\S+?&#93;)?)|(\\+|\\-)[0-9]+)((&#91;\\S+?&#93;)?)");
			//Regex num = new Regex("(?<sign>(\\+|\\-))[0-9]*d");
			Regex mode = new Regex("(h|l)?([0-9]*)?");
			Regex des = new Regex("&#91;[\\S ]+&#93;");//((\\[\\S+\\])?)
			int i, num, sign = 1, n, sum = 0, size, arg = 1;
			string desc;
			List<int> r;
			foreach (Match m in d.Matches(rollstr))
			{
				str = m.ToString();
				desc = des.Match(str).ToString();
				str = des.Replace(str, "");
				n = 0;
				r = new List<int>();
				if (str[0] == '-') sign = -1;
				else sign = 1;
				if (rtstr != "")
				{
					if (sign == 1) rtstr += " + ";
					else rtstr += " - ";
				}
				if (!str.Contains("d"))
				{
					rtstr += str.Replace("+", "").Replace("-", "") + desc;
					sum += int.Parse(str);
					continue;
				}

				spl = str.Split(new char[] { 'h', 'l', 'r' });
				if (spl.Length > 1) 
				{
					if (!int.TryParse(spl[1], out arg)) arg = 1;
				}
				spl = spl[0].Split(new char[] { '+', '-', 'd' }, StringSplitOptions.RemoveEmptyEntries);
				num = int.Parse(spl[0]);
				if (spl.Length == 1) num = 1;
				size = int.Parse(spl[spl.Length - 1]);
				for (i = 0; i < num; i++)
				{
					r.Add(rd.Next(1, size + 1));
					
				}
				if (num == 1)
				{
					rtstr += r[0];
					n += r[0];
				}
				else
				{
					rtstr += "(";
					if (str.Contains("h"))
					{
						r.Sort();
						for (i = num - 1; i >= 0; i--) 
						{
							if (i != num - arg - 1)
							{
								if (i != num - 1)
								{
									if (i < num - arg - 1)
										rtstr += ",";
									else
										rtstr += " + ";
								}
							}
							else
							{
								rtstr += "|";
							}
							if (i >= num - arg) n += r[i];
							rtstr += r[i];
						}
					}
					else if (str.Contains("l"))
					{
						r.Sort();
						for (i = 0; i < num; i++)
						{
							if (i != arg)
							{
								if (i != 0)
								{
									if (i > arg)
										rtstr += ",";
									else
										rtstr += " + ";
								}
							}
							else
							{
								rtstr += "|";
							}
							if (i < arg) n += r[i];
							rtstr += r[i];
						}
					}
					else if(str.Contains("r"))
					{
						for (i = 0; i < num; i++)
						{
							if (i != 0) rtstr += " + ";
							if (r[i] <= arg)
							{
								rtstr += "{" + r[i] + "=>";
								r[i] = rd.Next(1, size + 1);
								rtstr += r[i] + "}";
								n += r[i];
							}
							else
							{
								n += r[i];
								rtstr += r[i];
							}
						}
					}
					else
					{
						for (i = 0; i < num; i++)
						{
							if (i != 0) rtstr += " + ";
							n += r[i];
							rtstr += r[i];
						}
					}
					rtstr += " = " + n + ")";
				}
				n *= sign;
				sum += n;

				rtstr += desc;

			}

			rtstr += " = " + sum;
			return rtstr;
		}

		static public string Dice(string rollstr, out int result)
		{
			string str = "", rtstr = "";
			string[] spl;

			Dictionary<int, int> roll = new Dictionary<int, int>();
			Regex d = new Regex("((\\+|\\-)?[0-9]*(d[0-9]+)(h|l|r)?([0-9]*)?((&#91;\\S+?&#93;)?)|(\\+|\\-)[0-9]+)((&#91;\\S+?&#93;)?)");
			//Regex num = new Regex("(?<sign>(\\+|\\-))[0-9]*d");
			Regex mode = new Regex("(h|l)?([0-9]*)?");
			Regex des = new Regex("&#91;[\\S ]+&#93;");//((\\[\\S+\\])?)
			int i, num, sign = 1, n, sum = 0, size, arg = 1;
			string desc;
			List<int> r;
			foreach (Match m in d.Matches(rollstr))
			{
				str = m.ToString();
				desc = des.Match(str).ToString();
				str = des.Replace(str, "");
				n = 0;
				r = new List<int>();
				if (str[0] == '-') sign = -1;
				else sign = 1;
				if (rtstr != "")
				{
					if (sign == 1) rtstr += " + ";
					else rtstr += " - ";
				}
				if (!str.Contains("d"))
				{
					rtstr += str.Replace("+", "").Replace("-", "") + desc;
					sum += int.Parse(str);
					continue;
				}

				spl = str.Split(new char[] { 'h', 'l', 'r' });
				if (spl.Length > 1)
				{
					if (!int.TryParse(spl[1], out arg)) arg = 1;
				}
				spl = spl[0].Split(new char[] { '+', '-', 'd' }, StringSplitOptions.RemoveEmptyEntries);
				num = int.Parse(spl[0]);
				if (spl.Length == 1) num = 1;
				size = int.Parse(spl[spl.Length - 1]);
				for (i = 0; i < num; i++)
				{
					r.Add(rd.Next(1, size + 1));

				}
				if (num == 1)
				{
					rtstr += r[0];
					n += r[0];
				}
				else
				{
					rtstr += "(";
					if (str.Contains("h"))
					{
						r.Sort();
						for (i = num - 1; i >= 0; i--)
						{
							if (i != num - arg - 1)
							{
								if (i != num - 1)
								{
									if (i < num - arg - 1)
										rtstr += ",";
									else
										rtstr += " + ";
								}
							}
							else
							{
								rtstr += "|";
							}
							if (i >= num - arg) n += r[i];
							rtstr += r[i];
						}
					}
					else if (str.Contains("l"))
					{
						r.Sort();
						for (i = 0; i < num; i++)
						{
							if (i != arg)
							{
								if (i != 0)
								{
									if (i > arg)
										rtstr += ",";
									else
										rtstr += " + ";
								}
							}
							else
							{
								rtstr += "|";
							}
							if (i < arg) n += r[i];
							rtstr += r[i];
						}
					}
					else if (str.Contains("r"))
					{
						for (i = 0; i < num; i++)
						{
							if (i != 0) rtstr += " + ";
							if (r[i] <= arg)
							{
								rtstr += "{" + r[i] + "=>";
								r[i] = rd.Next(1, size + 1);
								rtstr += r[i] + "}";
								n += r[i];
							}
							else
							{
								n += r[i];
								rtstr += r[i];
							}
						}
					}
					else
					{
						for (i = 0; i < num; i++)
						{
							if (i != 0) rtstr += " + ";
							n += r[i];
							rtstr += r[i];
						}
					}
					rtstr += " = " + n + ")";
				}
				n *= sign;
				sum += n;

				rtstr += desc;

			}

			rtstr += " = " + sum;
			result = sum;
			return rtstr;
		}

		static public int DiceNum(string rollstr)
		{
			string str = "", rtstr = "";
			string[] spl;

			Dictionary<int, int> roll = new Dictionary<int, int>();
			Regex d = new Regex("((\\+|\\-)?[0-9]*(d[0-9]+)(h|l|r)?([0-9]*)?((&#91;\\S+?&#93;)?)|(\\+|\\-)[0-9]+)((&#91;\\S+?&#93;)?)");
			//Regex num = new Regex("(?<sign>(\\+|\\-))[0-9]*d");
			Regex mode = new Regex("(h|l)?([0-9]*)?");
			Regex des = new Regex("&#91;\\S+&#93;");//((\\[\\S+\\])?)
			int i, num, sign = 1, n, sum = 0, size, arg = 1;
			string desc;
			List<int> r;
			foreach (Match m in d.Matches(rollstr))
			{
				str = m.ToString();
				desc = des.Match(str).ToString();
				str = des.Replace(str, "");
				n = 0;
				r = new List<int>();
				if (str[0] == '-') sign = -1;
				else sign = 1;
				if (rtstr != "")
				{
					if (sign == 1) rtstr += " + ";
					else rtstr += " - ";
				}
				if (!str.Contains("d"))
				{
					rtstr += str.Replace("+", "").Replace("-", "") + desc;
					sum += int.Parse(str);
					continue;
				}

				spl = str.Split(new char[] { 'h', 'l', 'r' });
				if (spl.Length > 1)
				{
					if (!int.TryParse(spl[1], out arg)) arg = 1;
				}
				spl = spl[0].Split(new char[] { '+', '-', 'd' }, StringSplitOptions.RemoveEmptyEntries);
				num = int.Parse(spl[0]);
				if (spl.Length == 1) num = 1;
				size = int.Parse(spl[spl.Length - 1]);
				for (i = 0; i < num; i++)
				{
					r.Add(rd.Next(1, size + 1));

				}
				if (num == 1)
				{
					rtstr += r[0];
					n += r[0];
				}
				else
				{
					rtstr += "(";
					if (str.Contains("h"))
					{
						r.Sort();
						for (i = num - 1; i >= 0; i--)
						{
							if (i != num - arg - 1)
							{
								if (i != num - 1)
								{
									if (i < num - arg - 1)
										rtstr += ",";
									else
										rtstr += " + ";
								}
							}
							else
							{
								rtstr += "|";
							}
							if (i >= num - arg) n += r[i];
							rtstr += r[i];
						}
					}
					else if (str.Contains("l"))
					{
						r.Sort();
						for (i = 0; i < num; i++)
						{
							if (i != arg)
							{
								if (i != 0)
								{
									if (i > arg)
										rtstr += ",";
									else
										rtstr += " + ";
								}
							}
							else
							{
								rtstr += "|";
							}
							if (i < arg) n += r[i];
							rtstr += r[i];
						}
					}
					else if (str.Contains("r"))
					{
						for (i = 0; i < num; i++)
						{
							if (i != 0) rtstr += " + ";
							if (r[i] <= arg)
							{
								rtstr += "{" + r[i] + "=>";
								r[i] = rd.Next(1, size + 1);
								rtstr += r[i] + "}";
								n += r[i];
							}
							else
							{
								n += r[i];
								rtstr += r[i];
							}
						}
					}
					else
					{
						for (i = 0; i < num; i++)
						{
							if (i != 0) rtstr += " + ";
							n += r[i];
							rtstr += r[i];
						}
					}
					rtstr += " = " + n + ")";
				}
				n *= sign;
				sum += n;

				rtstr += desc;

			}

			rtstr += " = " + sum;
			rollstr = rtstr;
			return sum;
			
		}

		public static string NameGenerator()
		{
			string name = string.Empty;
			string[] currentconsonant;
			string[] vowels = "a,a,a,a,a,e,e,e,e,e,e,e,e,e,e,e,i,i,i,o,o,o,u,y,ee,ee,ea,ea,ey,eau,eigh,oa,oo,ou,ough,ay".Split(',');
			string[] commonconsonants = "s,s,s,s,t,t,t,t,t,n,n,r,l,d,sm,sl,sh,sh,th,th,th".Split(',');
			string[] averageconsonants = "sh,sh,st,st,b,c,f,g,h,k,l,m,p,p,ph,wh".Split(',');
			string[] middleconsonants = "x,ss,ss,ch,ch,ck,ck,dd,kn,rt,gh,mm,nd,nd,nn,pp,ps,tt,ff,rr,rk,mp,ll".Split(','); //can't start
			string[] rareconsonants = "j,j,j,v,v,w,w,w,z,qu,qu".Split(',');
			Random rng = new Random(Guid.NewGuid().GetHashCode()); //http://codebetter.com/blogs/59496.aspx
			int[] lengtharray = new int[] { 2, 2, 2, 2, 2, 2, 3, 3, 3, 4 }; //favor shorter names but allow longer ones
			int Length = lengtharray[rng.Next(lengtharray.Length)];
			for (int i = 0; i < Length; i++)
			{
				int lettertype = rng.Next(1000);
				if (lettertype < 775) currentconsonant = commonconsonants;
				else if (lettertype < 875 && i > 0) currentconsonant = middleconsonants;
				else if (lettertype < 985) currentconsonant = averageconsonants;
				else currentconsonant = rareconsonants;
				name += currentconsonant[rng.Next(currentconsonant.Length)];
				name += vowels[rng.Next(vowels.Length)];
				if (name.Length > 4 && rng.Next(1000) < 800) break; //getting long, must roll to save
				if (name.Length > 6 && rng.Next(1000) < 950) break; //really long, roll again to save
				if (name.Length > 7) break; //probably ridiculous, stop building and add ending
			}
			int endingtype = rng.Next(1000);
			if (name.Length > 6)
				endingtype -= (name.Length * 25); //don't add long endings if already long
			else
				endingtype += (name.Length * 10); //favor long endings if short
			if (endingtype < 400) { } // ends with vowel
			else if (endingtype < 775) name += commonconsonants[rng.Next(commonconsonants.Length)];
			else if (endingtype < 825) name += averageconsonants[rng.Next(averageconsonants.Length)];
			else if (endingtype < 840) name += "ski";
			else if (endingtype < 860) name += "son";
			else if (Regex.IsMatch(name, "(.+)(ay|e|ee|ea|oo)$") || name.Length < 5)
			{
				name = "Mc" + name.Substring(0, 1).ToUpper() + name.Substring(1);
				return name;
			}
			else name += "ez";
			name = name.Substring(0, 1).ToUpper() + name.Substring(1); //capitalize first letter
			return name;
		}
	}

	class RandomCreator
	{
		PrivateSession pSession;
		string MainFile;
		string BuildStr;
		string InputKey;
		FileStream log;
		StreamWriter sw;
		string LogFile;
		Dictionary<string, string> Inputs = new Dictionary<string, string>();
		Dictionary<string, int> Nums = new Dictionary<string, int>();
		Regex sp = new Regex("《[\\S\\s]*?》");
		Regex mod = new Regex("\\{\\S*?\\|\\|[\\+\\-][0-9d]+?\\}");
		Regex set = new Regex("\\{[^\\s{}]*?\\|\\|=[\\+\\-]?[0-9d]+?\\}");
		Regex res = new Regex("\\{\\S*?\\}");
		Regex desc = new Regex("\\([\\S\\s]*?\\)");
		Random rd = new Random();

		public RandomCreator(string msg, PrivateSession ps)
		{
			MainFile = CQ.GetCSPluginsFolder() + @"\RandomCreator\" + msg + ".ini";
			LogFile = CQ.GetCSPluginsFolder() + @"\RandomCreator\" + msg + "-Log.txt";
			if (File.Exists(MainFile)) BuildStr = IniFileHelper.GetStringValue(MainFile, "Info", "Template", "");
			pSession = ps;
			log = new FileStream(LogFile, FileMode.Append);
			sw = new StreamWriter(log);
			sw.WriteLine("====================================");
			sw.Close();
			log.Close();
		}


		public void Build(string i = "")
		{
			string m;
			string rp;
			string d;
			string c;
			int j;
			if (!File.Exists(MainFile)) return;
			if (i != "")
			{
				if (i == "Rnd")
				{
					i = rd.Next(1, IniFileHelper.GetAllItemKeys(MainFile, InputKey.Substring(1, InputKey.Length - 2)).Length).ToString();
				}
				//Tools.SendDebugMessage("Input=" + i);
				Inputs.Add(desc.Replace(IniFileHelper.GetStringValue(MainFile, InputKey.Substring(1, InputKey.Length - 2), "Dice", InputKey), "").Replace("Input:", ""), i);
				BuildStr = new Regex(Regex.Escape(InputKey)).Replace(BuildStr, IniFileHelper.GetStringValue(MainFile,
					InputKey.Substring(1, InputKey.Length - 2), i,
					IniFileHelper.GetStringValue(MainFile, InputKey.Substring(1, InputKey.Length - 2), "Default",
					"【" + InputKey.Substring(1, InputKey.Length - 2) + "】").Replace("_", "")), 1);
				//BuildStr = BuildStr.Replace(InputKey, IniFileHelper.GetStringValue(MainFile,
				//InputKey.Substring(1, InputKey.Length - 2), i, ""));
				log = new FileStream(LogFile, FileMode.Append);
				sw = new StreamWriter(log);
				sw.WriteLine(i);
				sw.Close();
				log.Close();
				InputKey = "";
				pSession.rcInput = false;
			}

			while (sp.IsMatch(BuildStr))
			{
				m = sp.Match(BuildStr).ToString();
				rp = m;
				m = m.Substring(1, m.Length - 2);
				log = new FileStream(LogFile, FileMode.Append);
				sw = new StreamWriter(log);
				sw.WriteLine(m);
				sw.Close();
				log.Close();
				//Tools.SendDebugMessage(m);
				d = IniFileHelper.GetStringValue(MainFile, m, "Dice", "");
				if (m.StartsWith("Input:"))
				{
					m = m.Replace("Input:", "");
					if (Inputs.ContainsKey(m.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[0]))
					{
						Inputs[m.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[0]] 
							= m.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[1];
					}
					else
					{
						Inputs.Add(m.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[0]
							, m.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[1]);
					}
					BuildStr = BuildStr.Replace(rp, "");
					continue;
				}
				if (m.Contains("×"))
				{
					j = Tools.DiceNum("+" + m.Split('×')[0]);
					m = "《" + m.Split('×')[1] + "》";
					//Tools.SendDebugMessage(m);
					d = "";
					for (; j > 0; j--) 
					{
						d += m;
					}
					//Tools.SendDebugMessage(d);
					BuildStr = BuildStr.Replace(rp, d);
					continue;
				}
				if (m == "RdName")
				{
					BuildStr = new Regex(Regex.Escape(rp)).Replace(BuildStr, Tools.NameGenerator(), 1);
					continue;
				}
				if (!m.Contains("=="))
				{
					if (d.StartsWith("Input:"))
					{
						d = desc.Replace(d.Replace("Input:", ""), "");
						if (d.Contains("=="))
						{
							if (!Inputs.ContainsKey(d.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[0]))
							{
								Inputs.Add(d.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[0]
									, d.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[1]);
							}
							else
							{
								Inputs[d.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[0]]
									= d.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[1];
							}
							d = d.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[0];
						}
						if (Inputs.ContainsKey(d))
						{
							BuildStr = new Regex(Regex.Escape(rp)).Replace(BuildStr, IniFileHelper.GetStringValue(MainFile, m, 
								Inputs[d], IniFileHelper.GetStringValue(MainFile, m, "Default", "【" + m + "】")
								.Replace("_", "")), 1);
							//BuildStr = BuildStr.Replace(rp, IniFileHelper.GetStringValue(MainFile, m, Inputs[d], ""));
						}
						if(!Inputs.ContainsKey(d))
						{
							Input(rp);
							return;
						}
					}
					else
					{
						BuildStr = new Regex(Regex.Escape(rp)).Replace(BuildStr, IniFileHelper.GetStringValue(MainFile, m, 
							Tools.DiceNum(IniFileHelper.GetStringValue(MainFile, m, "Dice", "")).ToString(),
							IniFileHelper.GetStringValue(MainFile, m, "Default", "【" + m + "】").Replace("_", "")), 1);
						//BuildStr = BuildStr.Replace(rp, IniFileHelper.GetStringValue(MainFile, Tools.SendDebugMessage(m)
						//, Tools.SendDebugMessage(Tools.DiceNum(IniFileHelper.GetStringValue(MainFile, m, "Dice", "")).ToString()), ""));
					}
				}
				else
				{
					c = m.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[1];
					m = m.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries)[0];
					BuildStr = new Regex(Regex.Escape(rp)).Replace(BuildStr, IniFileHelper.GetStringValue(MainFile, m,
						c, IniFileHelper.GetStringValue(MainFile, m, "Default", "【" + m + "】").Replace("_", "")), 1);
				}
			}
				string[] strs;
				foreach (Match mt in set.Matches(BuildStr))
				{
					strs = mt.ToString().Substring(1, mt.ToString().Length - 2).Split(new string[] { "||=" }, StringSplitOptions.RemoveEmptyEntries);
					if (Nums.ContainsKey(strs[0]))
					{
						Nums[strs[0]] = Tools.DiceNum("+" + strs[1]);
					}
					else
					{
						Nums.Add(strs[0], Tools.DiceNum("+" + strs[1]));
					}
				}
				BuildStr = set.Replace(BuildStr, "");
				foreach (Match mt in mod.Matches(BuildStr)) 
				{
					strs = mt.ToString().Substring(1, mt.ToString().Length - 2).Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
					if (Nums.ContainsKey(strs[0]))
					{
						Nums[strs[0]] = Tools.DiceNum("+" + Nums[strs[0]].ToString() + strs[1]);
					}
					else
					{
						Nums.Add(strs[0], Tools.DiceNum("+" + strs[1]));
					}
				}
				BuildStr = mod.Replace(BuildStr, "");
				while (res.IsMatch(BuildStr))
				{
					m = res.Match(BuildStr).ToString();
					if (Nums.ContainsKey(m.Substring(1, m.Length - 2)))
					{
						BuildStr = BuildStr.Replace(m, Nums[m.Substring(1, m.Length - 2)].ToString());
					}
					else
					{
						BuildStr = BuildStr.Replace(m, "0");
					}
				
				
			}

			pSession.Send(BuildStr.Replace(";;", "\n").Replace("++", "+").Replace("+-", "-"));
		}

		public void Input(string key)
		{
			InputKey = key;
			pSession.Send("请输入" + IniFileHelper.GetStringValue(MainFile, key.Substring(1, key.Length - 2), "Dice", "").Replace("Input:", "").Replace(";;", "\n") + "的值");
			pSession.rcInput = true;
		}

	}
}

