using Flexlive.CQP.Framework;
using Flexlive.CQP.Framework.Utils;
using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;

namespace Wennx.CQP.CSharpPlugins.TRPGBot
{
	/// <summary>
	/// 酷Q C#版插件Demo
	/// </summary>
	public class Plugin : CQAppAbstract
	{
		Dictionary<long, GroupSession> SessionTable = new Dictionary<long, GroupSession>();
		static Random rd = new Random();
		

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
		public override void PrivateMessage(int subType, int sendTime, long fromQQ, string msg, int font)
		{
			/*try
			{
				if (!PrivateSession.Sessions.ContainsKey(fromQQ))
				{
					PrivateSession.Sessions.Add(fromQQ, new PrivateSession(fromQQ));
				}
				if (msg.StartsWith("."))
				{
					PrivateSession.Sessions[fromQQ].PrivateMessageHandler(msg);
				}
			}
			catch (Exception e)
			{
				Tools.SendDebugMessage(e.ToString());
			}*/
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
		public override void GroupMessage(int subType, int sendTime, long fromGroup, long fromQQ, string fromAnonymous, string msg, int font)
		{
			try
			{
				if (!GroupSession.Sessions.ContainsKey(fromGroup))
				{
					GroupSession.Sessions.Add(fromGroup, new GroupSession(fromGroup));
				}
				GroupSession.Sessions[fromGroup].GroupMessageHandler(fromQQ, msg);
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
		public override void DiscussMessage(int subType, int sendTime, long fromDiscuss, long fromQQ, string msg, int font)
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
			CQ.SendGroupMessage(fromGroup, String.Format("[{0}]{2}({1})被{3}管理员权限。", CQ.ProxyType, beingOperateQQ, CQE.GetQQName(beingOperateQQ), subType == 1 ? "取消了" : "设置为"));
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
			CQ.SendGroupMessage(fromGroup, String.Format("[{0}]群员{2}({1}){3}", CQ.ProxyType, beingOperateQQ, CQE.GetQQName(beingOperateQQ), subType == 1 ? "退群。" : String.Format("被{0}({1})踢除。", CQE.GetQQName(fromQQ), fromQQ)));
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
			CQ.SendGroupMessage(fromGroup, String.Format("[{0}]群里来了新人{2}({1})，管理员{3}({4}){5}", CQ.ProxyType, beingOperateQQ, CQE.GetQQName(beingOperateQQ), CQE.GetQQName(fromQQ), fromQQ, subType == 1 ? "同意。" : "邀请。"));
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
		static public Dictionary<long, PrivateSession> Sessions = new Dictionary<long, PrivateSession>();
		long QQid;
		long group = 0;

		public PrivateSession(long id)
		{
			QQid = id;
		}

		public void PrivateMessageHandler(string msg)
		{
			string[] msgstr = msg.Split(' ');
			switch (msgstr[0])
			{

			}
		}
	}

	class GroupSession
	{

		static public Dictionary<long, GroupSession> Sessions = new Dictionary<long, GroupSession>();

		Random rd = new Random();

		long GroupID;

		bool Logging = false;
		FileStream LogStream;
		StreamWriter LogWriter;
		string LogFile = "";

		HSSFWorkbook LogTable;
		Dictionary<long, HSSFFont> NormalFont = new Dictionary<long, HSSFFont>();


		Dictionary<long, string> CharBinding = new Dictionary<long, string>();

		long Owner;
		List<long> Admin = new List<long>();
		

		int nya_mood = 100;
		string[] nya_normal = { "喵~", "喵？", "喵！", "喵喵！", "喵~喵~", "喵呜？", "喵…", "喵喵？" };
		string[] nya_happy = { };
		string[] nya_sad = { };
		string[] nya_lazy = { };
		string[] nya_angry = { };


		public GroupSession(long id)
		{
			GroupID = id;
			foreach (CQGroupMemberInfo gmi in CQE.GetGroupMemberList(id))
			{
				if (gmi.Authority == "群主")
				{
					Owner = gmi.QQNumber;
					Admin.Add(gmi.QQNumber);
				}
				//CQE.GetQQFace(gmi.QQNumber).Save(CQ.GetCSPluginsFolder() + "\\LogFiles\\" + gmi.QQNumber.ToString() + ".jpg");
			}
		}

		public void Send(string msg)
		{
			if (Logging) Log(msg);
			CQ.SendGroupMessage(GroupID, msg);
		}

		public void Nya(long QQid, string msg)
		{
			Send(nya_normal[rd.Next(0, 8)]);
		}

		public void GroupMessageHandler(long QQid, string msg)
		{
			if (msg.StartsWith("喵")) Nya(QQid, msg);
			if (Logging) Log(msg, QQid);
			string[] msgstr = msg.Replace("。", ".").Split(' ');
			switch (msgstr[0].ToLower())
			{
				case ".r":
					if (CharBinding.ContainsKey(QQid))
					{
						CharRoll(QQid, msg);
					}
					else
					{
						Roll(QQid, msg);
					}
					break;
				case ".rs":
					if (CharBinding.ContainsKey(QQid))
					{
						SideRoll(QQid, msg);
					}
					else
					{
						Roll(QQid, msg);
					}
					break;
				case ".csel":
					if (msgstr.Length == 1)
					{
						CharSelection(QQid);
					}
					else
					{
						CharBind(QQid, msgstr[1]);
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
				case ".cdis":
					CharDisbinding(QQid);
					break;
				case ".m":
					Memory(QQid, msg);
					break;
				case ".ms":
					SideMemory(QQid, msg);
					break;
				case ".setdm":
					if (msgstr.Length == 2) SetDM(QQid, msgstr[1]);
					break;
				case ".ar":
					if (msgstr.Length == 2) AllRoll(QQid, msgstr[1]);
					break;
				case ".ghi":
					if (msgstr.Length > 1) GHI(QQid, msgstr[1]);
					break;
				case ".help":
					Help();
					break;
				case ".log":
					if (msgstr.Length > 1)
					{
						LoggerToggle(QQid, msgstr[1]);
					}
					else
					{
						LoggerToggle(QQid, DateTime.Now.ToString());
					}
					break;
			}


			
		}



		public void LoggerToggle(long QQid, string msg)
		{
			if (!Admin.Contains(QQid)) return;
			if (Logging == false)
			{
				DirectoryInfo d = new DirectoryInfo(CQ.GetCSPluginsFolder() + "\\CharSettings");
				Logging = true;
				LogFile = msg;
				LogStream = new FileStream(CQ.GetCSPluginsFolder() + "\\LogFiles\\" + LogFile + "-" + GroupID + ".txt"
					, FileMode.Append, FileAccess.Write);
				LogWriter = new StreamWriter(LogStream);
				Send(string.Format("=======群日志 {0}=======", msg));
				Send(string.Format("=======群号：{0}=======", GroupID));
				Send(string.Format("=======发起者：{0}=======", QQid));
				Send(string.Format("======={0}开始记录=======", DateTime.Now));
			}
			else
			{
				Send("=======记录结束=======");
				LogWriter.Close();
				LogStream.Close();
				FileStream fs = new FileStream(CQ.GetCSPluginsFolder() + "\\LogFiles\\" + LogFile + "-" + GroupID + ".xls", FileMode.Create);
				LogTable.Write(fs);
				fs.Close();
				LogFile = "";
				Logging = false;
			}
		}

		public void TbLog(long QQid, string msg)
		{

		}

		DateTime lastTimeStamp = DateTime.Now;

		public void Log(string msg, long QQid = 0)
		{
			msg = msg.Replace("\n", ";;");
			foreach (Match m in new Regex("\\[CQ:at,qq=[0-9]*\\]").Matches(msg))
			{
				msg.Replace(m.ToString(), "@" + CQE.GetQQName(long.Parse(m.ToString().Replace("[CQ:at,qq=", "").Replace("]", ""))));
			}
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
				LogWriter.WriteLine(string.Format("{0}:{1}", IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "CharName", CQE.GetQQName(QQid)), msg));

			}
			else
			{
				LogWriter.WriteLine(string.Format("{0}:{1}", CQE.GetQQName(QQid), msg));
			}
		}

		public void CharSelection(long QQid)
		{
			Dictionary<string, string> menu = new Dictionary<string, string>();
			DirectoryInfo d = new DirectoryInfo(CQ.GetCSPluginsFolder() + "\\CharSettings");
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
			Send(m);
		}

		public void CharBind(long QQid, string selection)
		{
			DirectoryInfo d = new DirectoryInfo(CQ.GetCSPluginsFolder() + "\\CharSettings");
			foreach (FileInfo f in d.GetFiles("*.ini"))
			{
				if (IniFileHelper.GetStringValue(f.FullName, "CharInfo", "CharID", "") == selection)
				{
					if (CharBinding.ContainsKey(QQid))
						CharBinding[QQid] = f.FullName;
					else
						CharBinding.Add(QQid, f.FullName);
				}
			}
			Send(string.Format("{0} 绑定了角色 {1}", CQ.CQCode_At(QQid),
				IniFileHelper.GetStringValue(CharBinding[QQid].ToString(), "CharInfo", "CharName", "")));
		}

		public void CharDisbinding(long QQid)
		{
			CharBinding.Remove(QQid);
			Send(string.Format("{0} 解除绑定了当前角色", CQ.CQCode_At(QQid)));
		}

		public void CharModify(long QQid, string key, string value)
		{
			if (CharBinding.ContainsKey(QQid))
			{
				FileStream fs = File.Open(CharBinding[QQid], FileMode.Open);
				FileStream tmp = File.Open(CQ.GetCSPluginsFolder() + "\\CharSettings\\tmp.ini", FileMode.Create);
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
				File.Replace(CQ.GetCSPluginsFolder() + "\\CharSettings\\tmp.ini", CharBinding[QQid], CQ.GetCSPluginsFolder() + "\\CharSettings\\LastChange.bak");
				Send(string.Format("[{0}]{1}已修改为{2}", CQ.CQCode_At(QQid), key, value));
			}
			
		}

		public void SideModify(long QQid, string key, string value)
		{
			if (CharBinding.ContainsKey(QQid))
			{
				FileStream fs = File.Open(CharBinding[QQid], FileMode.Open);
				FileStream tmp = File.Open(CQ.GetCSPluginsFolder() + "\\CharSettings\\tmp.ini", FileMode.Create);
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
				File.Replace(CQ.GetCSPluginsFolder() + "\\CharSettings\\tmp.ini", CharBinding[QQid], CQ.GetCSPluginsFolder() + "\\CharSettings\\LastChange.bak");
				Send(string.Format("[{0}]{1}已修改为{2}", CQ.CQCode_At(QQid), key, value));
			}
			
		}

		public void CharRoll(long QQid, string rollstr)
		{
			string orirsn = rollstr.Replace(".r ", "");
			if (CharBinding.ContainsKey(QQid))
			{

				foreach (string str in IniFileHelper.GetAllItems(CharBinding[QQid], "CharMarco"))
				{
					rollstr = rollstr.Replace(str.Split('=')[0], str.Split('=')[1]);
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
			Send(msg);
		}

		public void SideRoll(long QQid, string rollstr)
		{
			string orirsn = rollstr.Replace(".rs ", "");
			if (CharBinding.ContainsKey(QQid))
			{

				foreach (string str in IniFileHelper.GetAllItems(CharBinding[QQid], "SideMarco"))
				{
					rollstr = rollstr.Replace(str.Split('=')[0], str.Split('=')[1]);
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
			Send(msg);
		}

		public void Memory(long QQid, string msg)
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
						IniFileHelper.GetStringValue(CharBinding[qq], "CharMemo", key, "未找到").Replace(";", "\n").Replace("[", "&#91;").Replace("]", "&#93;"));
				}
			}
			if (at.Matches(msg).Count == 0)
			{
				rtn += string.Format("\n{0}：{1}",
						IniFileHelper.GetStringValue(CharBinding[QQid], "CharInfo", "CharName", CQ.CQCode_At(QQid)),
						IniFileHelper.GetStringValue(CharBinding[QQid], "CharMemo", key, "未找到").Replace(";", "\n").Replace("[", "&#91;").Replace("]", "&#93;"));
			}
			Send(rtn);
		}

		public void MemorySet(long QQid, string key, string value)
		{
			if (CharBinding.ContainsKey(QQid))
			{
				FileStream fs = File.Open(CharBinding[QQid], FileMode.Open);
				FileStream tmp = File.Open(CQ.GetCSPluginsFolder() + "\\CharSettings\\tmp.ini", FileMode.Create);
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
				File.Replace(CQ.GetCSPluginsFolder() + "\\CharSettings\\tmp.ini", CharBinding[QQid], CQ.GetCSPluginsFolder() + "\\CharSettings\\LastChange.bak");
				Send(string.Format("[{0}]{1}已修改为{2}", CQ.CQCode_At(QQid), key, value));
			}
		}

		public void SideMemory(long QQid, string msg)
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
			Send(rtn);
		}

		public void SideMemorySet(long QQid, string key, string value)
		{
			if (CharBinding.ContainsKey(QQid))
			{
				FileStream fs = File.Open(CharBinding[QQid], FileMode.Open);
				FileStream tmp = File.Open(CQ.GetCSPluginsFolder() + "\\CharSettings\\tmp.ini", FileMode.Create);
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
				File.Replace(CQ.GetCSPluginsFolder() + "\\CharSettings\\tmp.ini", CharBinding[QQid], CQ.GetCSPluginsFolder() + "\\CharSettings\\LastChange.bak");
				Send(string.Format("[{0}]{1}已修改为{2}", CQ.CQCode_At(QQid), key, value));
			}

		}

		public void SetDM(long QQid, string msg)
		{
			if (!Admin.Contains(QQid)) return;
			Regex at = new Regex("\\[CQ:at,qq=[0-9]*\\]");
			long qq;
			foreach (Match m in at.Matches(msg))
			{
				qq = long.Parse(m.ToString().Replace("[CQ:at,qq=", "").Replace("]", ""));
				if (Admin.Contains(qq) && Owner != qq)
				{
					Admin.Remove(qq);
					Send(string.Format("{0}已被取消DM", qq));
				}
				else
				{
					Admin.Add(qq);
					Send(string.Format("{0}已被设置为DM", qq));
				}
				
			}
		}

		public void AllRoll(long QQid, string key)
		{
			if (!Admin.Contains(QQid)) return;
			string msg = "全体 " + key + " 检定结果为：";
			string rollstr;
			string[] substr;
			foreach (KeyValuePair<long, string> c in CharBinding)
			{
				rollstr = key;
				foreach (string str in IniFileHelper.GetAllItems(c.Value, "CharMarco"))
				{
					rollstr = rollstr.Replace(str.Split('=')[0], str.Split('=')[1]);
				}
				substr = rollstr.Split(':');
				msg += String.Format("\n[{0}]：",
						IniFileHelper.GetStringValue(c.Value.ToString(), "CharInfo", "CharName", CQ.CQCode_At(c.Key)));
				foreach (string s in substr)
				{
					if (substr.Length > 1) msg += "\n";
					msg += Tools.Dice(s.Replace("[", "&#91;").Replace("]", "&#93;"));

				}
			}
			Send(msg);
		}

		DataTable GHIDT = new DataTable();
		Dictionary<long, int> GHIcounter = new Dictionary<long, int>();
		static List<string> GHIdice = new List<string>(new string[] { "+1", "d2", "d3", "d4", "d6", "d8", "d10", "d12" });
		public void GHI(long QQid, string msg)
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
			string rtmsg = "";
			string dice = "";
			string action = msg;
			if (msg.ToLower() == "go")
			{
				Send(GHIGO());
				return;
			}
			else if (Regex.Match(msg, "(d[0-9]+)|(\\+[0-9]+)").Success)
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
			else if (msg.Contains("施放") || msg.Contains("攻击"))
			{
				dice = "d8";
			}
			else
			{
				dice = "d6";
			}
			int dex = int.Parse(IniFileHelper.GetStringValue(CharBinding[(long)QQid], "CharMarco", "敏捷", "???")
					.Replace("d20", "").Replace("[敏捷]", ""));
			if (dex >= 4)
			{
				if(GHIdice.Contains(dice))
					dice = GHIdice[(GHIdice.IndexOf(dice) - dex / 4) >= 0 ? GHIdice.IndexOf(dice) - dex / 4 : 0];
			}
			GHIDT.Rows.Add(dice, 0, 0, action, QQid, 0, "");
			if (rtmsg == "")
			{
				rtmsg = "灰鹰先攻：";
				foreach (DataRow dr in GHIDT.Rows)
				{
					rtmsg += string.Format("\n{0}-{1}:{2}",
								IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "CharInfo", "CharName", "???"),
								(((string)dr["Action"]).StartsWith("h") || ((string)dr["Action"]).StartsWith("l")) 
								? ((string)dr["Action"]).Substring(1) : dr["Action"], dr["Dice"]);
				}
			}
			Send(rtmsg);
		}

		public string GHIGO()
		{
			int d;
			
			string rtmsg = "灰鹰先攻：";
			foreach (DataRow dr in GHIDT.Rows)
			{
				dr["Dex"] = int.Parse(IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "CharMarco", "敏捷", "???")
					.Replace("d20", "").Replace("[敏捷]", ""));
			}
			foreach (DataRow dr in GHIDT.Rows)
			{
				dr["Roll"] = Tools.DiceNum((string)dr["Dice"]);
				d = int.Parse(((string)dr["Dice"]).Substring(1));
				if (IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "CharMemo", "专长", "").Contains("精通先攻")
					&& ((int)dr["Roll"] > d - (int)(d / 4)))
				{
					dr["Reroll"] = dr["Roll"] + "=>";
					dr["Roll"] = Tools.DiceNum((string)dr["Dice"]);
				}
			}
			GHIDT.DefaultView.Sort = "Roll,Dex Desc";
			GHIDT = GHIDT.DefaultView.ToTable();
			foreach (DataRow dr in GHIDT.Rows)
			{
				GHIcounter[(long)dr["ID"]] += (int)dr["Roll"];
				dr["Result"] = GHIcounter[(long)dr["ID"]];
			}
			GHIDT.DefaultView.Sort = "Result,Dex Desc";
			GHIDT = GHIDT.DefaultView.ToTable();
			foreach (DataRow dr in GHIDT.Rows)
			{

				rtmsg += string.Format("\n{0}-{1}:{2}({3}{4})",
					IniFileHelper.GetStringValue(CharBinding[(long)dr["ID"]], "CharInfo", "CharName", "???"),
					dr["Action"], dr["Result"], dr["Reroll"], dr["Roll"]);
			}
			GHIDT.Clear();
			GHIcounter.Clear();
			return rtmsg;
		}



		public void Roll(long QQid, string rollstr)
		{
			string[] rsn = new Regex(".r\\s[\\s\\S]*").Match(rollstr).ToString().Split(' ');
			if (rsn.Length > 2)
			{
				Send(String.Format("[{0}]{1}：{2}", CQ.CQCode_At(QQid), rsn[2], Tools.Dice(rollstr)));
			}
			else
			{
				Send(String.Format("[{0}]{1}", CQ.CQCode_At(QQid), Tools.Dice(rollstr)));
			}
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
					Send(CQ.CQCode_At(495258764) + "喵~喵！喵！喵？喵~");
					helpcount = 0;
					break;
			}
			helpcount++;
		}
	}

	class Tools
	{
		static Random rd = new Random();

		static public void SendDebugMessage(string msg)
		{
			CQ.SendPrivateMessage(495258764, msg);
		}

		static public string Dice(string rollstr)
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

	}
}

