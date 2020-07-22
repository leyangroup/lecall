using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace LeQing
{
    public partial class Form1 : Form
    {

        // 場宎趙扢掘(Initialize device, call it when your app stars)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_Init")]
        public static extern int AD800_Init();

        // 庋溫扢掘(Free device, call it when your app closes)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_Free")]
        public static extern void AD800_Free();

        // 衄謗笱源宒彶DLL袨怓ㄛ珨笱岆籵徹秏洘ㄛ梗珨笱岆隙覃滲杅(There are two way to get status, one is windows message, another is callback function)
        // 扢离隙冞秏洘曆梟(windows message mode, set handle to receive message)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_SetMsgHwnd")]
        public static extern void AD800_SetMsgHwnd(int hWnd);

        // 扢离隙覃滲杅(callback function mode, set function address)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_SetCallbackFun")]
        public static extern void AD800_SetCallbackFun(IntPtr fun);

        // 腕善扢掘杅(Get the number of devices)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetTotalDev")]
        public static extern int AD800_GetTotalDev();

        // 腕善籵耋杅(Get the number of channels)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetTotalCh")]
        public static extern int AD800_GetTotalCh();

        // 黍俋盄袨怓(Get line status of the channel)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetChState")]
        public static extern int AD800_GetChState(int iChannel);

        // 黍懂萇瘍鎢(Get caller id number of the channel)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetCallerId", CharSet = CharSet.Ansi)]
        public static extern int AD800_GetCallerId(int iChannel, StringBuilder szBuff, uint iLen);

        // 黍畢瘍(Get dialed number of the channel)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetDialed", CharSet = CharSet.Ansi)]
        public static extern int AD800_GetDialed(int iChannel, StringBuilder szBuff, uint iLen);

        // 黍唳掛瘍(Get device version)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetVer")]
        public static extern void AD800_GetVer(int iDevice, StringBuilder szBuff, uint iLen);

        // 黍扢掘唗瘍(Get device serial no.)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetDevSN")]
        public static extern int AD800_GetDevSN(int iDevice);

        // 扢离/黍 枑儂ㄛ境儂陓瘍淈聆統杅(蘇萇揤毓峓岆 3 - 20v 3善20眳潔憩峈岆枑儂ㄛ湮衾20憩峈岆境儂, 淈聆奀潔岆枑儂500ms, 境儂300ms, 裁萇200ms)
        //(Set/Get hook detection voltage setting(default range: 3v-20v), if line voltage is less then 3, line status is power off, 
        // if line voltage is between 3 and 20, line status is hook off(pickup), if line voltage is bigger then 20, line status is hook on(hangup) )
        // default hook off detecting time(means voltage continuance time) is 500ms, hook on is 300ms
        [DllImport("AD800Device.dll", EntryPoint = "AD800_SetHookVoltage")]
        public static extern int AD800_SetHookVoltage(int iChannel, byte szHookOffVol, byte szHookOnVol);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_SetHookTime")]
        public static extern int AD800_SetHookTime(int iChannel, int iHookOffTime, int iHookOnTime, int iPowerOffTime);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetHookVoltage")]
        public static extern void AD800_GetHookVoltage(int iChannel, ref byte szHookOffVol, ref byte szHookOnVol);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetHookTime")]
        public static extern void AD800_GetHookTime(int iChannel, ref int iHookOffTime, ref int iHookOnTime, ref int iPowerOffTime);

        // 扢离/黍 懂萇瘍鎢賦旰奀潔(蘇奀潔岆200ms)(Get/Set caller id end time for receive DTMF caller id, calculate time from last received digit)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_SetRevCIDTime")]
        public static extern int AD800_SetRevCIDTime(int iChannel, int iCIDTime);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetRevCIDTime")]
        public static extern void AD800_GetRevCIDTime(int iChannel, ref int iCIDTime);

        // 扢离翹秞睿溫潔秞講(Set volume of recording/playback)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_SetRecVolume")]
        public static extern int AD800_SetRecVolume(int iChannel, int iVol);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_SetPlaybackVolume")]
        public static extern int AD800_SetPlaybackVolume(int iChannel, int iVol);

        // 腕善絞腔秞講硉(Get current volume of recording/playback)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetRecVolume")]
        public static extern int AD800_GetRecVolume(int iChannel);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetPlaybackVolume")]
        public static extern int AD800_GetPlaybackVolume(int iChannel);

        // 羲宎翹秞(Recording API,file recording and memory recording, 
        // file recording, just give a file name, then DLL will save voice data to the file,
        // memory recording, give a memory buffer, DLL save voice data to buffer, when buffer is buff, DLL will send full status)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_StartFileRec", CharSet = CharSet.Ansi)]
        public static extern int AD800_StartFileRec(int iChannel, StringBuilder szFile);

       // [DllImport("AD800Device.dll", EntryPoint = "AD800_StartMemRec")]
        //public static extern int AD800_StartMemRec(int iChannel, byte *pszBuff, uint iLen);

        // 囀湔翹秞奀ㄛ腕善絞翹秞遣湔爵湔賸嗣屾跺BYTE腔杅擂(memory recording, get the number of bytes in memory buff)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetMemRecBytes")]
        public static extern int AD800_GetMemRecBytes(int iChannel);

        // 礿砦翹秞(Stop recording)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_StopRec")]
        public static extern int AD800_StopRec(int iChannel);

        // 溫秞(Play voice, the same as recording, there are file mode and memory buffer mode)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_PlayFile", CharSet = CharSet.Ansi)]
        public static extern int AD800_PlayFile(int iChannel, StringBuilder szFile);

        //[DllImport("AD800Device.dll", EntryPoint = "AD800_PlayMem")]
       // public static extern int AD800_PlayMem(int iChannel, byte *pszBuff, uint iLen);

        // 礿砦溫秞(Stop playback)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_StopPlay")]
        public static extern void AD800_StopPlay(int iChannel);

        // 逄秞揖楷(Enable voice trigger, after enabled voice trigger, if voice reachs the set threshold, DLL will send trigger status)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_VoiceTrigger")]
        public static extern int AD800_VoiceTrigger(int iChannel);

        // 礿砦逄秞揖楷(Disable voice trigger)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_StopVoiceTrigger")]
        public static extern void AD800_StopVoiceTrigger(int iChannel);

        // 逄秞揖楷統杅(Set/Get voice trigger parameters, voice/silence threshold, voice/silence continuance time)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_SetVoiceThreshold")]
        public static extern int AD800_SetVoiceThreshold(int iChannel, int iTime, int iLevel);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_SetSilenceThreshold")]
        public static extern int AD800_SetSilenceThreshold(int iChannel, int iTime, int iLevel);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetVoiceThreshold")]
        public static extern void AD800_GetVoiceThreshold(int iChannel, ref int iTime, ref int iLevel);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_GetSilenceThreshold")]
        public static extern void AD800_GetSilenceThreshold(int iChannel, ref int iTime, ref int iLevel);

        // 疆秞淈聆(Start detecting busy tone)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_DetBusyTone")]
        public static extern int AD800_DetBusyTone(int iChannel);

        // 礿砦疆秞淈聆(Stop detecting busy tone)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_StopDetBusyTone")]
        public static extern void AD800_StopDetBusyTone(int iChannel);

        // 礿砦疆秞淈聆(Stop detecting busy tone)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_SendDTMF", CharSet = CharSet.Ansi)]
        public static extern int AD800_SendDTMF(int iChannel, StringBuilder szBuff, uint iLen);

        // 枑境睿境儂(Pickup/Hangup line, line you pickup/huangup phone)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_PickUp")]
        public static extern int AD800_PickUp(int iChannel);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_HangUp")]
        public static extern int AD800_HangUp(int iChannel);

        // 剿睿蟀諉萇趕儂(Disconnect/Connect phone, Cut/Connect connection between phone and line)
        [DllImport("AD800Device.dll", EntryPoint = "AD800_DisconnectPhone")]
        public static extern int AD800_DisconnectPhone(int iChannel);

        [DllImport("AD800Device.dll", EntryPoint = "AD800_ConnectPhone")]
        public static extern int AD800_ConnectPhone(int iChannel);

        // windows秏洘(windows message definition)
        public const int WM_AD800MSG = 1024 + 1800;

        // 傷諳袨怓(All Channel Status)
        public enum AD800_STATUS : int
        {
            AD800_DEVICE_CONNECTION = 0,	// 扢掘蟀諉袨怓(Device connection status)

            AD800_LINE_STATUS, // 俋盄袨怓(Line Status e.g pickup,hangup,ringing,power off)

            AD800_LINE_VOLTAGE, // 俋盄萇揤(Line voltage)

            AD800_LINE_POLARITY, // 俋盄憤俶(Line Polarity Changed)

            AD800_LINE_CALLERID, // 俋盄懂萇瘍鎢(Caller Id number)

            AD800_LINE_DTMF, // 萇趕儂畢瘍(Dialed number)

            AD800_REC_DATA,	// 翹秞杅擂(Recording data)

            AD800_PLAY_FINISHED, // 溫秞俇傖(Playback finished)

            AD800_VOICETRIGGER, // 逄秞揖楷袨怓(Voice trigger status)

            AD800_BUSYTONE,	// 疆秞袨怓(Busy tone status)

            AD800_DTMF_FINISHED // DTMF楷冞俇傖(Send DTMF finished)

        };

        // 扢掘蟀諉袨怓(Device Connection Status Definition)
        public enum AD800_CONNECTION : int
        {
            AD800DEVICE_DISCONNECTED = 0,	// 剿羲(Device connected)
            AD800DEVICE_CONNECTED			// 蟀奻(Device disconnected)
        };

        // 俋盄袨怓(Line Status Definition)
        public enum AD800_LINESTATUS : int
        {
            AD800LINE_POWEROFF = 0, // 裁萇(Power off/no line)
            AD800LINE_HOOKOFF,		// 枑儂(Pick up)
            AD800LINE_HOOKON,		// 境儂(Hang up)
            AD800LINE_RING		// 砒鍊(Ringing)
        };


        public enum LIST_COLUMN : int
        {
            LISTCOL_CH = 0,
            LISTCOL_SN,
            LISTCOL_LINESTATUS,
            LISTCOL_VOLTAGE,
            LISTCOL_VOICETRIGGER,
            LISTCOL_CALLERID,
            LISTCOL_DIALED,
            LISTCOL_VER
        };

        public struct CHSTATUS
        {
            public bool bRecord;
            public bool bPlay;
            public bool bLineBusy;
            public bool bDisPhone;
            public bool bDial;
            public bool bVoiceTrigger;
            public bool bDetBusytone;

            public int dwLost;
            public int dwAdd;

            public int dwBlock;
            public int dwBlockACK;

            public int iRecVol;
            public int iPlayVol;
        };

        public const int MAX_CH = 32;

        CHSTATUS[] m_tagCHState = new CHSTATUS[MAX_CH];

        public bool m_bClickRecVol = false;
        public bool m_bClickPlayVol = false;


        public void ClearStatus(int iChannel)
        {
            if (iChannel < 0 || iChannel >= MAX_CH) return;

            m_tagCHState[iChannel].bRecord = false;
            m_tagCHState[iChannel].bPlay = false;
            m_tagCHState[iChannel].bLineBusy = false;
            m_tagCHState[iChannel].bDisPhone = false;
            m_tagCHState[iChannel].bDial = false;
            m_tagCHState[iChannel].bVoiceTrigger = false;
            m_tagCHState[iChannel].bDetBusytone = false;

            m_tagCHState[iChannel].dwLost = 0;
            m_tagCHState[iChannel].dwAdd = 0;

            m_tagCHState[iChannel].dwBlock = 0;
            m_tagCHState[iChannel].dwBlockACK = 0;

            m_tagCHState[iChannel].iRecVol = 0;
            m_tagCHState[iChannel].iPlayVol = 0;
        }


        public string WavPath = ConfigurationManager.AppSettings["WavPath"];
        public string APIPath = ConfigurationManager.AppSettings["APIPath"];

        public Form1()
        {
            InitializeComponent();

            listView1.Columns.Clear();
            listView1.Columns.Add("線號", 50, HorizontalAlignment.Left);
            listView1.Columns.Add("日期/時間", 180, HorizontalAlignment.Left);
            listView1.Columns.Add("設備訊息", 100, HorizontalAlignment.Left);
            listView1.Columns.Add("電壓", 50, HorizontalAlignment.Left);
            listView1.Columns.Add("電話號碼", 100, HorizontalAlignment.Left);
            listView1.Columns.Add("進出", 50, HorizontalAlignment.Left);
            listView1.Columns.Add("音檔", 350, HorizontalAlignment.Left);
            listView1.Columns.Add("id", 160, HorizontalAlignment.Left);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AD800_SetMsgHwnd(Handle.ToInt32());
            if (0 == AD800_Init())
            {
                MessageBox.Show("設備初始化失敗!!");
            }           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AD800_Free();
        }

        private void OnDeviceMsg(IntPtr wParam, IntPtr lParam)
        {
            try
            {

                int iEvent = new int();
                int iChannel = new int();
                iEvent = wParam.ToInt32() % 65536;
                iChannel = wParam.ToInt32() / 65536;
                //listView1.Items[iChannel].SubItems[5].Text = iEvent.ToString();
                switch (iEvent)
                {
                    case (int)AD800_STATUS.AD800_DEVICE_CONNECTION:
                        {
                            if ((int)AD800_CONNECTION.AD800DEVICE_CONNECTED == lParam.ToInt32())
                            {
                                //EnableCtrls(true);
                                int iDev = iChannel;
                                StringBuilder szVer = new StringBuilder(32);
                                AD800_GetVer(iDev, szVer, 20);
                                string strSN;
                                strSN = string.Format("{0:D}", AD800_GetDevSN(iDev));
                                int iCount = listView1.Items.Count;
                                string strCH;
                                for (int i = iCount; i < iCount + 8; i++)
                                {
                                    strCH = string.Format("{0:D}", i + 1);
                                    listView1.Items.Add(strCH);
                                    listView1.Items[i].SubItems.Add("");
                                    listView1.Items[i].SubItems.Add("");
                                    listView1.Items[i].SubItems.Add("");
                                    listView1.Items[i].SubItems.Add("");
                                    listView1.Items[i].SubItems.Add("");
                                    listView1.Items[i].SubItems.Add("");
                                    listView1.Items[i].SubItems.Add("");
                                    /*
                                    if (iCount == i)
                                    {
                                        listView1.Items[i].SubItems[(int)LIST_COLUMN.LISTCOL_SN].Text = strSN;
                                        listView1.Items[i].SubItems[(int)LIST_COLUMN.LISTCOL_VER].Text = szVer.ToString();
                                    }
                                    if (i == 0)
                                    {
                                        listView1.Items[i].Selected = true;
                                    }*/
                                }
                            }
                            else
                            {
                                //EnableCtrls(false);
                                listView1.Items.Clear();
                                m_bClickRecVol = false;
                                m_bClickPlayVol = false;
                                for (int i = 0; i < MAX_CH; i++)
                                {
                                    ClearStatus(i);
                                }
                            }
                        }
                        break;
                    case (int)AD800_STATUS.AD800_LINE_STATUS:
                        {
                            switch (lParam.ToInt32())
                            {
                                case (int)AD800_LINESTATUS.AD800LINE_POWEROFF:
                                    listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_LINESTATUS].Text = "Power Off";
                                    break;
                                case (int)AD800_LINESTATUS.AD800LINE_HOOKOFF:

                                    listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_LINESTATUS].Text = "Hook Off";
                                    //listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_CALLERID].Text = "";
                                    //listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_DIALED].Text = "";

                                    break;
                                case (int)AD800_LINESTATUS.AD800LINE_HOOKON:
                                    listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_LINESTATUS].Text = "Hook On";
                                    if (m_tagCHState[iChannel].bRecord == true)
                                    {
                                        AD800_StopRec(iChannel);
                                        if (System.IO.Directory.Exists(WavPath + DateTime.Now.ToString("yyyy/MM/dd")) == false)
                                        {
                                            System.IO.Directory.CreateDirectory(WavPath + DateTime.Now.ToString("yyyy/MM/dd"));
                                        }
                                        string sourceFile = "";
                                        string destFile = "";
                                        if (listView1.Items[iChannel].SubItems[5].Text == "IN")
                                        {
                                            sourceFile = listView1.Items[iChannel].SubItems[6].Text;
                                            destFile = listView1.Items[iChannel].SubItems[6].Text.Replace("*", "星");
                                            //char character = (char)1;
                                            //string text = character.ToString();
                                            //destFile = listView1.Items[iChannel].SubItems[6].Text.Replace(text, "");
                                            destFile = Regex.Replace(destFile, @"[^a-zA-Z0-9_*.]", "");

                                            System.Threading.Thread.Sleep(300);
                                            try
                                            {
                                                LogLog("move: " + listView1.Items[iChannel].SubItems[4].Text + "::" + destFile);
                                                this.label1.Text = "move: " + sourceFile + "::" + destFile;
                                                System.IO.File.Move(sourceFile, WavPath + DateTime.Now.ToString("yyyy/MM/dd") + "/" + destFile);
                                            }
                                            catch (WebException ex)
                                            {
                                                LogLog("Exception Message:move: " + ex.Message);
                                                LogLog("Exception Message:move: " + sourceFile + "::" + destFile);
                                                this.label1.Text = ex.Message;
                                            }
                                            putWave(iChannel);
                                        }
                                        else
                                        {

                                            sourceFile = string.Format("OUT_{0:D}", iChannel + 1) + ".wav";
                                            destFile = DateTime.Now.ToString("HHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + string.Format("_OUT_{0:D}_", iChannel + 1) + listView1.Items[iChannel].SubItems[4].Text + ".wav";
                                            destFile = destFile.Replace("*", "星");
                                            //char character = (char)1;
                                            //string text = character.ToString();
                                            //destFile = listView1.Items[iChannel].SubItems[6].Text.Replace(text, "");
                                            destFile = Regex.Replace(destFile, @"[^a-zA-Z0-9_*.]", "");
                                            listView1.Items[iChannel].SubItems[6].Text = destFile;
                                            getCallID(iChannel, "ou", listView1.Items[iChannel].SubItems[4].Text);
                                            System.Threading.Thread.Sleep(300);
                                            try
                                            {
                                                LogLog("move: " + listView1.Items[iChannel].SubItems[4].Text + "::" + destFile);
                                                this.label1.Text = "move: " + sourceFile + "::" + destFile;
                                                System.IO.File.Move(sourceFile, WavPath + DateTime.Now.ToString("yyyy/MM/dd") + "/" + destFile);
                                            }
                                            catch (WebException ex)
                                            {
                                                LogLog("Exception Message:move_error: " + ex.Message);
                                                LogLog("Exception Message:move_error: " + sourceFile + "::" + destFile);
                                                this.label1.Text = ex.Message;
                                            }
                                            putWave(iChannel);
                                        }

                                        m_tagCHState[iChannel].bRecord = false;
                                    }
                                    break;
                                case (int)AD800_LINESTATUS.AD800LINE_RING:
                                    listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_LINESTATUS].Text = "Ringing";
                                    if (m_tagCHState[iChannel].bRecord == false)
                                    {
                                        StringBuilder szFile = new StringBuilder(256);
                                        listView1.Items[iChannel].SubItems[5].Text = "IN";
                                        string tmpstr = DateTime.Now.ToString("HHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + string.Format("_IN_{0:D}_", iChannel + 1) + listView1.Items[iChannel].SubItems[4].Text + ".wav";
                                        listView1.Items[iChannel].SubItems[6].Text = tmpstr;
                                        szFile.Append(tmpstr);
                                        if (0 != AD800_StartFileRec(iChannel, szFile))
                                        {
                                            m_tagCHState[iChannel].bRecord = true;
                                        }
                                        else
                                        {
                                            m_tagCHState[iChannel].bRecord = false;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case (int)AD800_STATUS.AD800_LINE_VOLTAGE:
                        {
                            string strText;
                            strText = string.Format("{0:D}V", lParam);
                            listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_VOLTAGE].Text = strText;
                        }
                        break;
                    case (int)AD800_STATUS.AD800_LINE_POLARITY:
                        {
                            if (0 == lParam.ToInt32())
                            {
                                // 憤俶曹趙ㄛ絞憤俶峈蛹
                            }
                            else
                            {
                                // 憤俶曹趙ㄛ絞憤俶峈淏
                            }
                        }
                        break;
                    case (int)AD800_STATUS.AD800_LINE_CALLERID:
                        {
                            StringBuilder szBuff = new StringBuilder(128);
                            AD800_GetCallerId(iChannel, szBuff, 64);
                            //來電號碼
                            listView1.Items[iChannel].SubItems[1].Text = DateTime.Now.ToString();
                            listView1.Items[iChannel].SubItems[4].Text = szBuff.ToString();
                            getCallID(iChannel, "in", szBuff.ToString());
                        }
                        break;
                    case (int)AD800_STATUS.AD800_LINE_DTMF:
                        {
                            StringBuilder szBuff = new StringBuilder(128);
                            szBuff.Length = 0;
                            AD800_GetDialed(iChannel, szBuff, 64);
                            //撥出號碼
                            listView1.Items[iChannel].SubItems[1].Text = DateTime.Now.ToString();
                            listView1.Items[iChannel].SubItems[4].Text = szBuff.ToString();

                            if (m_tagCHState[iChannel].bRecord == false)
                            {
                                StringBuilder szFile = new StringBuilder(256);
                                listView1.Items[iChannel].SubItems[5].Text = "OUT";
                                szFile.Append(string.Format("OUT_{0:D}", iChannel + 1) + ".wav");
                                //getCallID(iChannel, "ou", szBuff.ToString());
                                if (0 != AD800_StartFileRec(iChannel, szFile))
                                {
                                    m_tagCHState[iChannel].bRecord = true;
                                }
                                else
                                {
                                    m_tagCHState[iChannel].bRecord = false;
                                }
                            }
                        }
                        break;
                    case (int)AD800_STATUS.AD800_PLAY_FINISHED:
                        {
                            AD800_StopPlay(iChannel);

                            m_tagCHState[iChannel].bPlay = false;
                        }
                        break;
                    case (int)AD800_STATUS.AD800_VOICETRIGGER:
                        {
                            if (0 == lParam.ToInt32())
                            {
                                listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_VOICETRIGGER].Text = "";
                            }
                            else if (1 == lParam.ToInt32())
                            {
                                listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_VOICETRIGGER].Text = "Voice";
                            }
                        }
                        break;
                    case (int)AD800_STATUS.AD800_BUSYTONE:
                        {
                            listView1.Items[iChannel].SubItems[(int)LIST_COLUMN.LISTCOL_LINESTATUS].Text = "Busy Tone";
                        }
                        break;
                    case (int)AD800_STATUS.AD800_DTMF_FINISHED:
                        {
                            m_tagCHState[iChannel].bDial = false;
                            //button_Dial.Enabled = true;
                        }
                        break;
                    case (int)AD800_STATUS.AD800_REC_DATA:
                        break;
                    default:
                        break;
                }
            }
            catch (WebException ex)
            {
                LogLog("Exception Message:OnDeviceMsg: " + ex.Message.ToString());
            }
        }
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            try
            {
                switch (m.Msg)
                {
                    case WM_AD800MSG:
                        OnDeviceMsg(m.WParam, m.LParam);
                        break;
                    default:
                        base.DefWndProc(ref m);
                        break;
                }
            }
            catch (WebException ex)
            {
                LogLog("Exception Message:DefWndProc: " + ex.Message.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ListViewItem lv1 = new ListViewItem();
            ListViewItem lv2 = new ListViewItem();
            ListViewItem lv3 = new ListViewItem();
            ListViewItem lv4 = new ListViewItem();
            ListViewItem lv5 = new ListViewItem();
            ListViewItem lv6 = new ListViewItem();
            ListViewItem lv7 = new ListViewItem();
            ListViewItem lv8 = new ListViewItem();

            lv1.SubItems[0].Text = "1"; lv1.SubItems.Add(""); lv1.SubItems.Add(""); lv1.SubItems.Add(""); lv1.SubItems.Add(""); lv1.SubItems.Add(""); lv1.SubItems.Add(""); lv1.SubItems.Add("");
            lv2.SubItems[0].Text = "2"; lv2.SubItems.Add(""); lv2.SubItems.Add(""); lv2.SubItems.Add(""); lv2.SubItems.Add(""); lv2.SubItems.Add(""); lv2.SubItems.Add(""); lv2.SubItems.Add("");
            lv3.SubItems[0].Text = "3"; lv3.SubItems.Add(""); lv3.SubItems.Add(""); lv3.SubItems.Add(""); lv3.SubItems.Add(""); lv3.SubItems.Add(""); lv3.SubItems.Add(""); lv3.SubItems.Add("");
            lv4.SubItems[0].Text = "4"; lv4.SubItems.Add(""); lv4.SubItems.Add(""); lv4.SubItems.Add(""); lv4.SubItems.Add(""); lv4.SubItems.Add(""); lv4.SubItems.Add(""); lv4.SubItems.Add("");
            lv5.SubItems[0].Text = "5"; lv5.SubItems.Add(""); lv5.SubItems.Add(""); lv5.SubItems.Add(""); lv5.SubItems.Add(""); lv5.SubItems.Add(""); lv5.SubItems.Add(""); lv5.SubItems.Add("");
            lv6.SubItems[0].Text = "6"; lv6.SubItems.Add(""); lv6.SubItems.Add(""); lv6.SubItems.Add(""); lv6.SubItems.Add(""); lv6.SubItems.Add(""); lv6.SubItems.Add(""); lv6.SubItems.Add("");
            lv7.SubItems[0].Text = "7"; lv7.SubItems.Add(""); lv7.SubItems.Add(""); lv7.SubItems.Add(""); lv7.SubItems.Add(""); lv7.SubItems.Add(""); lv7.SubItems.Add(""); lv7.SubItems.Add("");
            lv8.SubItems[0].Text = "8"; lv8.SubItems.Add(""); lv8.SubItems.Add(""); lv8.SubItems.Add(""); lv8.SubItems.Add(""); lv8.SubItems.Add(""); lv8.SubItems.Add(""); lv8.SubItems.Add("");
            listView1.Items.Add(lv1);
            listView1.Items.Add(lv2);
            listView1.Items.Add(lv3);
            listView1.Items.Add(lv4);
            listView1.Items.Add(lv5);
            listView1.Items.Add(lv6);
            listView1.Items.Add(lv7);
            listView1.Items.Add(lv8);

            AD800_SetMsgHwnd(Handle.ToInt32());
            if (0 == AD800_Init())
            {
                MessageBox.Show("設備初始化失敗!!");
            }
            LogLog("Message:Load: 程式啟動!!");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string ss = getCallID(1,"in", "0912345678");
        }

        public string getCallID(int iChannel,string par1,string par2)
        {
            try
            {
                LogLog("Message:getCallID: iChannel::" +iChannel.ToString() + ",par1::" + par1 + ",par2::" + par2);

                //par1 = "in";  // in  ou  re 
                //par2 = "in?pn=0912345678";//自己指定URL
                string par3 = "";
                par3 = par1+"?pn=" + par2;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(APIPath + par3);
                request.Method = "GET";
                request.ContentType = "text/json;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                JObject oo_json = JsonConvert.DeserializeObject<JObject>(retString);
                listView1.Items[iChannel].SubItems[7].Text = oo_json["id"].ToString();
                LogLog("Message:getCallID: " + oo_json["id"].ToString());

                return oo_json["id"].ToString(); //  "{\"status\":\"success\",\"message\":\"Create incoming call success.\",\"id\":6}"  
              
            }
            catch (WebException ex)
            {
                LogLog("Exception Message:getCallID: " + ex.Message.ToString() );
                //var error = (HttpWebResponse)ex.Response;
                //Stream myResponseStream = error.GetResponseStream();
                //StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                //string retString = myStreamReader.ReadToEnd();
                //myStreamReader.Close();
                //myResponseStream.Close();
                //LogLog("Exception Message:getCallID: " + ex.Message.ToString() + "::" + retString);
                this.label1.Text = ex.Message.ToString();
                return "0";
            }
        }

        public string putWave(int iChannel)
        {
            try
            {
                //par1 = "in";  // in  ou  re 
                //par2 = re?id=4&url=135022_IN_1_0933222111
                string par3 = "";
                par3 = "re?id="+listView1.Items[iChannel].SubItems[7].Text+"&url="+listView1.Items[iChannel].SubItems[6].Text;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(APIPath + par3);
                request.Method = "GET";
                request.ContentType = "text/json;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                JObject oo_json = JsonConvert.DeserializeObject<JObject>(retString);                
                return oo_json["status"].ToString(); //  {"status":"success","message":"Update call record url success."}           
            }
            catch (WebException ex)
            {
                LogLog("Exception Message:putWave: " + ex.Message.ToString());
                var error = (HttpWebResponse)ex.Response;
                Stream myResponseStream = error.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                LogLog("Exception Message:putWave: " + retString);
                this.label1.Text = retString;
                return retString;
                
            }
        }

        public void LogLog(string tmpstr)
        {
            try
            {
                StreamWriter sw = File.AppendText(DateTime.Now.ToString("yyyyMMdd") + ".log");   //小寫TXT     
                // Add some text to the file.
                sw.WriteLine(DateTime.Now.ToString() + "::" + tmpstr);
                sw.Close();
            }
            catch (WebException ex)
            {                
                this.label1.Text = ex.Message;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LogLog("123");
            char character = (char)1;
            string text = character.ToString();

            string s = @"zoo13579~!多奇_數位@"+ text + @"_$#%^%$&*().,>?]123IN_ggg";
            string r = Regex.Replace(s, @"[^a-zA-Z0-9_*.]", "");
        }
    }
}
