using LoLLauncher;
using hAutobot.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace hAutobot
{
    public partial class FormMain : Form
    {
        public static DataTable dt = new DataTable();
        public static string Path2;
        public static string Region;
        public static int connectedAccs = 0;
        public static int curRunning = 0;
        public static string cversion = "5.6.15_03_31_12_48";
        private static bool closeFlag = false;
        private static bool startFlag = false;
        private static Dictionary<string, Bot> dictBot = new Dictionary<string, Bot>();

        public FormMain()
        {
            InitializeComponent();
            this.Load += FormMain_Load;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            InitEvent();
            InitDefault();
            InitChecks();
            LoadConfiguration();

            while (!File.Exists(Path2 + "lol.launcher.exe"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                MessageBox.Show("Wrong launcher path. \ncheck the config\\settings.xml and restart hAutobot");
                closeFlag = true;
                break;
            }

            if (closeFlag)
            {
                Application.Exit();
                return;
            }

            LoadAccounts();

            cboDelay.SelectedIndex = 0;
            cboReplaceCfg.SelectedIndex = 1;
        }

        private void InitEvent()
        {
            ConfigInitialize.Click += delegate(object sender, EventArgs e)
            {   
                string path = Path2 + @"Config\\game.cfg";

                FileInfo fileInfo = new FileInfo(path);


                if (fileInfo.Exists)
                {
                    FileSecurity fileSecurity = File.GetAccessControl(path);
                    fileSecurity.AddAccessRule(new FileSystemAccessRule(System.Security.Principal.WindowsIdentity.GetCurrent().Name, FileSystemRights.Delete, AccessControlType.Allow));
                    File.SetAccessControl(path, fileSecurity);

                    fileInfo.IsReadOnly = false;
                    fileInfo.Delete();

                    MessageBox.Show("Initialization is complete.", "Initialize", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                else
                    MessageBox.Show("Has already been initialized.", "Initialize", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            };

            startBtn.Click += delegate(object sender, EventArgs e) 
            {
                if (cboReplaceCfg.Text == "True")
                    Gamecfg();


                foreach (DataRow dr in dt.Rows)
                {
                    ConnectAccount(dr);
                    Thread.Sleep(Convert.ToInt16(cboDelay.Text) * 1000);
                }

                startFlag = true;
                startBtn.Enabled = false;
                stopBtn.Enabled = true;
            };
            stopBtn.Click += delegate(object sender, EventArgs e) 
            {
                DisConnectAccount(null);
                startFlag = false;
                stopBtn.Enabled = false;
                startBtn.Enabled = true;
            };
            Connect.Click += delegate(object sender, EventArgs e)
            {
                DataGridViewSelectedRowCollection drsc = dataGridView1.SelectedRows;

                if (drsc.Count > 0)
                {
                    DataGridViewRow dvr = drsc[0];
                    DataRow dr = ((DataRowView)dvr.DataBoundItem).Row;
                    ConnectAccount(dr);
                }
            };
            Disconnect.Click += delegate(object sender, EventArgs e)
            {
                DataGridViewSelectedRowCollection drsc = dataGridView1.SelectedRows;

                if (drsc.Count > 0)
                {
                    DataGridViewRow dvr = drsc[0];

                    DataGridViewCellCollection dvc = dvr.Cells;

                    if (dictBot.ContainsKey(dvc[1].Value.ToString()))
                    {
                        DisConnectAccount(dvc);
                    }
                }
            };

            addAccountsBtn.Click += addAccountsBtn_Click;
            removeAccountsBtn.Click += removeAccountsBtn_Click;
            this.FormClosing += FormMain_FormClosing;
        }

        private void InitDefault()
        {
            QueueTypeInput.SelectedIndex = 0;
            SelectChampionInput.SelectedIndex = 6;
            Spell1Input.SelectedIndex = 8;
            Spell2Input.SelectedIndex = 5;
            RegionInput.SelectedIndex = 0;

       
            dt.Columns.Add("Region");
            dt.Columns.Add("ID");
            dt.Columns.Add("Password");
            dt.Columns.Add("Summoner");
            dt.Columns.Add("Queue Type");
            dt.Columns.Add("Champion");
            dt.Columns.Add("Spell1");
            dt.Columns.Add("Spell2");
            dt.Columns.Add("Max Level");
            dt.Columns.Add("Level");
            dt.Columns.Add("XP");
            dt.Columns.Add("IP (Get IP)");
            dt.Columns.Add("Status");

            dataGridView1.DataSource = dt;


            foreach (DataGridViewColumn columns in dataGridView1.Columns)
            {
                switch (columns.Name)
                {
                    case "Region":
                        columns.Visible = false;
                        break;
                    case "ID":
                        columns.Width = 100;
                        break;
                    case "Password":
                        columns.Width = 100;
                        break;
                    case "Summoner":
                        columns.Width = 85;
                        break;
                    case "Queue Type":
                        columns.Width = 120;
                        break;
                    case "Champion":
                        columns.Width = 80;
                        break;
                    case "Spell1":
                        columns.Width = 70;
                        break;
                    case "Spell2":
                        columns.Width = 70;
                        break;
                    case "Max Level":
                        columns.Width = 50;
                        break;
                    case "Level":
                        columns.Width = 50;
                        break;
                    case "XP":
                        columns.Width = 50;
                        columns.Visible = false;
                        break;
                    case "IP (Get IP)":
                        columns.Width = 100;
                        break;
                    case "Status":
                        columns.Width = 200;
                        break;
                }
            }
            dataGridView1.MultiSelect = false;
        }

        private static void InitChecks()
        {
            var theFolder = AppDomain.CurrentDomain.BaseDirectory + @"config\\";
            var accountsTxtLocation = AppDomain.CurrentDomain.BaseDirectory + @"config\\accounts.txt";
            var configTxtLocation = AppDomain.CurrentDomain.BaseDirectory + @"config\\settings.xml";

            if (!Directory.Exists(theFolder))
            {
                Directory.CreateDirectory(theFolder);
            }

            if (!File.Exists(configTxtLocation))
                MessageBox.Show("Not Found the settings.xml");
            if (!File.Exists(accountsTxtLocation))
                MessageBox.Show("Not Found the accounts.txt");

        }

        public void LoadAccounts()
        {
            var accountsTxtPath = AppDomain.CurrentDomain.BaseDirectory + "config\\accounts.txt";
            TextReader tr = File.OpenText(accountsTxtPath);
            string line;
            while ((line = tr.ReadLine()) != null)
            {
                DataRow dr = dt.NewRow();

                string[] acc = line.Split('|');
                if (acc.Length == 8)
                {
                    dr["Region"] = acc[0];
                    dr["ID"] = acc[1];
                    dr["Password"] = acc[2];
                    dr["Queue Type"] = acc[3];
                    dr["Champion"] = acc[4];
                    dr["Spell1"] = acc[5];
                    dr["Spell2"] = acc[6];
                    dr["Max Level"] = acc[7];
                    dr["Level"] = "";
                    dr["XP"] = "";
                    dr["IP (Get IP)"] = "";
                    dr["Status"] = "Ready";

                    dt.Rows.Add(dr);
                }
            }
            tr.Close();
        }

        public static void LoadConfiguration()
        {
            try
            {
                XmlUtil xml = new XmlUtil(AppDomain.CurrentDomain.BaseDirectory + "config\\settings.xml");
                Path2 = xml.GetNodeValue("/config/launcherpath");
                cversion = xml.GetNodeValue("/config/version") == "0" ? cversion : xml.GetNodeValue("/config/version");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Thread.Sleep(10000);
                Application.Exit();
            }
        }

        public static void Gamecfg()
        {
            try
            {

                string path = Path2 + @"Config\\game.cfg";
                FileInfo fileInfo = new FileInfo(path);
                fileInfo.IsReadOnly = false;
                fileInfo.Refresh();
                string str = "[General]\nGameMouseSpeed=9\nEnableAudio=0\nUserSetResolution=1\nBindSysKeys=0\nSnapCameraOnRespawn=1\nOSXMouseAcceleration=1\nAutoAcquireTarget=0\nEnableLightFx=0\nWindowMode=1\nShowTurretRangeIndicators=0\nPredictMovement=0\nWaitForVerticalSync=0\nColors=16\nHeight=200\nWidth=300\nSystemMouseSpeed=0\nCfgVersion=4.13.265\n\n[HUD]\nShowNeutralCamps=0\nDrawHealthBars=0\nAutoDisplayTarget=0\nMinimapMoveSelf=0\nItemShopPrevY=19\nItemShopPrevX=117\nShowAllChannelChat=0\nShowTimestamps=0\nObjectTooltips=0\nFlashScreenWhenDamaged=0\nNameTagDisplay=1\nShowChampionIndicator=0\nShowSummonerNames=0\nScrollSmoothingEnabled=0\nMiddleMouseScrollSpeed=0.5000\nMapScrollSpeed=0.5000\nShowAttackRadius=0\nNumericCooldownFormat=3\nSmartCastOnKeyRelease=0\nEnableLineMissileVis=0\nFlipMiniMap=0\nItemShopResizeHeight=47\nItemShopResizeWidth=455\nItemShopPrevResizeHeight=200\nItemShopPrevResizeWidth=300\nItemShopItemDisplayMode=1\nItemShopStartPane=1\n\n[Performance]\nShadowsEnabled=0\nEnableHUDAnimations=0\nPerPixelPointLighting=0\nEnableParticleOptimizations=0\nBudgetOverdrawAverage=10\nBudgetSkinnedVertexCount=10\nBudgetSkinnedDrawCallCount=10\nBudgetTextureUsage=10\nBudgetVertexCount=10\nBudgetTriangleCount=10\nBudgetDrawCallCount=1000\nEnableGrassSwaying=0\nEnableFXAA=0\nAdvancedShader=0\nFrameCapType=3\nGammaEnabled=1\nFull3DModeEnabled=0\nAutoPerformanceSettings=0\n=0\nEnvironmentQuality=0\nEffectsQuality=0\nShadowQuality=0\nGraphicsSlider=0\n\n[Volume]\nMasterVolume=1\nMusicMute=0\n\n[LossOfControl]\nShowSlows=0\n\n[ColorPalette]\nColorPalette=0\n\n[FloatingText]\nCountdown_Enabled=0\nEnemyTrueDamage_Enabled=0\nEnemyMagicalDamage_Enabled=0\nEnemyPhysicalDamage_Enabled=0\nTrueDamage_Enabled=0\nMagicalDamage_Enabled=0\nPhysicalDamage_Enabled=0\nScore_Enabled=0\nDisable_Enabled=0\nLevel_Enabled=0\nGold_Enabled=0\nDodge_Enabled=0\nHeal_Enabled=0\nSpecial_Enabled=0\nInvulnerable_Enabled=0\nDebug_Enabled=1\nAbsorbed_Enabled=1\nOMW_Enabled=1\nEnemyCritical_Enabled=0\nQuestComplete_Enabled=0\nQuestReceived_Enabled=0\nMagicCritical_Enabled=0\nCritical_Enabled=1\n\n[Replay]\nEnableHelpTip=0";
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(str);
                using (StreamWriter writer = new StreamWriter(Path2 + @"Config\game.cfg"))
                {
                    writer.Write(builder.ToString());
                }
                fileInfo.IsReadOnly = true;
                fileInfo.Refresh();
            }
            catch (Exception exception2)
            {
                Console.WriteLine("game.cfg Error: If using VMWare Shared Folder, make sure it is not set to Read-Only.\nException:" + exception2.Message);
            }
        }

        #region 함수
        
        public static void ConnectAccount(DataRow dr)
        {
            if (dr != null)
            {
                curRunning += 1;
                Bot bot;
                if (!dr["Queue Type"].ToString().Equals(""))
                {
                    QueueTypes queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), dr["Queue Type"].ToString());
                    bot = new Bot(dr["ID"].ToString(), dr["Password"].ToString(), dr["Region"].ToString(), Path2, curRunning, queuetype);
                }
                else
                {
                    QueueTypes queuetype = QueueTypes.ARAM;
                    bot = new Bot(dr["ID"].ToString(), dr["Password"].ToString(), dr["Region"].ToString(), Path2, curRunning, queuetype);
                }

                if (dictBot.ContainsKey(dr["ID"].ToString()))
                    dictBot[dr["ID"].ToString()] = bot;
                else
                    dictBot.Add(dr["ID"].ToString(), bot);
            }
        }


        public static void DisConnectAccount(DataGridViewCellCollection row)
        {
            if (row != null)
            {
                if (dictBot.ContainsKey(row[1].Value.ToString()))
                {
                    dictBot[row[1].Value.ToString()].connection.Disconnect();

                    Process.GetCurrentProcess().Threads[dictBot[row[1].Value.ToString()].threadID].Dispose();
                    dictBot.Remove(row[1].Value.ToString());
                }
            }
            else
            {
                foreach (Bot bot in dictBot.Values)
                {
                    bot.connection.Disconnect();

                    if (bot.connection.heartbeatThread.IsAlive)
                        bot.connection.heartbeatThread.Abort();

                    Process.GetCurrentProcess().Threads[bot.threadID].Dispose();
                }
                

                dictBot.Clear();
            }
        }


        public static String GetTimestamp()
        {
            return "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";
        }

        public static string GetChampion(string accName)
        {
            DataRow[] tempRow = dt.Select("ID = '" + accName + "'");
            return tempRow[0]["Champion"].ToString();
        }
        
        public static int GetMaxLevel(string accName)
        {
            DataRow[] tempRow = dt.Select("ID = '" + accName + "'");
            return Convert.ToInt16(tempRow[0]["Max Level"].ToString());
        }
        public static string GetSpells(string accName, string spellNumber)
        {
            DataRow[] tempRow = dt.Select("ID = '" + accName + "'");
            return tempRow[0]["Spell" + spellNumber].ToString();
        }

        public static void UpdateStatus(string message, string accname, string columnName)
        {
            try
            {
                foreach (DataRow rows in dt.Rows)
                {
                    if (rows["ID"].ToString().Equals(accname))
                    {
                        if (columnName != "Status")
                        {
                            if (columnName == "IP (Get IP)")
                            {
                                Regex reg = new Regex("[0-9]+");

                                int getIP = rows[columnName].ToString() != "" ? Convert.ToInt32(message) - Convert.ToInt32(reg.Match(rows[columnName].ToString()).Value) : 0;
                                try
                                {
                                    rows[columnName] = message + " (+" + getIP + ")";
                                }
                                catch (Exception)
                                {
                                    rows[columnName] = message;
                                }

                            }
                            else
                                rows[columnName] = message;
                        }
                        else
                            rows[columnName] = GetTimestamp() + message;
                    }
                }
            }
            catch (Exception)
            {
            }
            
        }        
        #endregion

        #region 이벤트

        private void addAccountsBtn_Click(object sender, EventArgs e)
        {
            if (newUserNameInput.Text.Length == 0)
            { MessageBox.Show("Please input to ID."); return; }


            if (newPasswordInput.Text.Length == 0)
            { MessageBox.Show("Please input to Password."); return; }


            foreach (DataRow rows in dt.Rows)
            {
                if (newUserNameInput.Text.Equals(rows["ID"].ToString()))
                { MessageBox.Show("Has already been added."); return; }
            }

            DataRow dr = dt.NewRow();
            dr["Region"] = RegionInput.Text;
            dr["ID"] = newUserNameInput.Text;
            dr["Password"] = newPasswordInput.Text;           
            dr["Queue Type"] = QueueTypeInput.Text;
            dr["Champion"] = SelectChampionInput.Text;
            dr["Spell1"] = Spell1Input.Text;
            dr["Spell2"] = Spell2Input.Text;
            dr["Max Level"] = MaxLevelInput.Text;
            dr["Status"] = "Ready";
            dt.Rows.Add(dr);

            FileHandlers.Account("ADD", dr[0].ToString(), dr[1].ToString(), dr[2].ToString(), dr[4].ToString(), dr[5].ToString(), dr[6].ToString(), dr[7].ToString(), dr[8].ToString());

            if (startFlag)
                ConnectAccount(dr);
        }

        private void removeAccountsBtn_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection drsc = dataGridView1.SelectedRows;

            if (drsc.Count > 0)
            {
                DataGridViewRow dvr = drsc[0];

                DataGridViewCellCollection dvc = dvr.Cells;

                if (dictBot.ContainsKey(dvc[1].Value.ToString()))
                {
                    DialogResult result = MessageBox.Show("If Remove the Row, Disconnect is as well. \nDo you want to continue?", "Disconnect", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == System.Windows.Forms.DialogResult.Yes)
                        DisConnectAccount(dvc);
                    else
                        return;
                }
                FileHandlers.Account("REMOVE", (string)dvc[0].Value, (string)dvc[1].Value, (string)dvc[2].Value, (string)dvc[4].Value, (string)dvc[5].Value, (string)dvc[6].Value, (string)dvc[7].Value, (string)dvc[8].Value);
                dataGridView1.Rows.Remove(dvr);
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Bot bot in dictBot.Values)
                bot.connection.Disconnect();

            Application.ExitThread();
            Application.Exit();

            Process[] myProceses = Process.GetProcessesByName("hAutobot");
            foreach (var proc in myProceses)
            {
                proc.CloseMainWindow();
                proc.Close();
            }
        }


        #endregion

    }
}
