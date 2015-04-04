using LoLLauncher;
using LoLLauncher.RiotObjects.Platform.Catalog.Champion;
using LoLLauncher.RiotObjects.Platform.Clientfacade.Domain;
using LoLLauncher.RiotObjects.Platform.Game;
using LoLLauncher.RiotObjects.Platform.Game.Message;
using LoLLauncher.RiotObjects.Platform.Matchmaking;
using LoLLauncher.RiotObjects.Platform.Statistics;
using LoLLauncher.RiotObjects;
using LoLLauncher.RiotObjects.Leagues.Pojo;
using LoLLauncher.RiotObjects.Platform.Game.Practice;
using LoLLauncher.RiotObjects.Platform.Harassment;
using LoLLauncher.RiotObjects.Platform.Leagues.Client.Dto;
using LoLLauncher.RiotObjects.Platform.Login;
using LoLLauncher.RiotObjects.Platform.Reroll.Pojo;
using LoLLauncher.RiotObjects.Platform.Statistics.Team;
using LoLLauncher.RiotObjects.Platform.Summoner;
using LoLLauncher.RiotObjects.Platform.Summoner.Boost;
using LoLLauncher.RiotObjects.Platform.Summoner.Masterybook;
using LoLLauncher.RiotObjects.Platform.Summoner.Runes;
using LoLLauncher.RiotObjects.Platform.Summoner.Spellbook;
using LoLLauncher.RiotObjects.Team;
using LoLLauncher.RiotObjects.Team.Dto;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using LoLLauncher.RiotObjects.Platform.Game.Map;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using LoLLauncher.RiotObjects.Platform.Summoner.Icon;
using LoLLauncher.RiotObjects.Platform.Catalog.Icon;
using System.Timers;

namespace hAutobot
{
    internal class Bot
    {
        public LoginDataPacket loginPacket = new LoginDataPacket();
        public GameDTO currentGame = new GameDTO();
        public LoLConnection connection = new LoLConnection();
        public List<ChampionDTO> availableChamps = new List<ChampionDTO>();
        public LoLLauncher.RiotObjects.Platform.Catalog.Champion.ChampionDTO[] availableChampsArray;
        public bool firstTimeInLobby = true;
        public bool firstTimeInQueuePop = true;
        public bool firstTimeInCustom = true;
        public Process exeProcess;
        public string ipath;
        public string Accountname;
        public string Password;
        public int threadID;
        public double sumLevel { get; set; }
        public double archiveSumLevel { get; set; }
        public double rpBalance { get; set; }
        public QueueTypes queueType { get; set; }
        public QueueTypes actualQueueType { get; set; }
        public int m_leaverBustedPenalty { get; set; }
        public string m_accessToken { get; set; }
        public string region { get; set; }
        public string regionURL;
        public bool QueueFlag;
        public int LastAntiBusterAttempt = 0;

        public Bot(string username, string password, string reg, string path, int threadid, QueueTypes QueueType)
        {
            ipath = path;
            Accountname = username;
            Password = password;
            threadID = threadid;
            queueType = QueueType;
            region = reg;
            connection.OnConnect += new LoLConnection.OnConnectHandler(this.connection_OnConnect);
            connection.OnDisconnect += new LoLConnection.OnDisconnectHandler(this.connection_OnDisconnect);
            connection.OnError += new LoLConnection.OnErrorHandler(this.connection_OnError);
            connection.OnLogin += new LoLConnection.OnLoginHandler(this.connection_OnLogin);
            connection.OnLoginQueueUpdate += new LoLConnection.OnLoginQueueUpdateHandler(this.connection_OnLoginQueueUpdate);
            connection.OnMessageReceived += new LoLConnection.OnMessageReceivedHandler(this.connection_OnMessageReceived);
            switch (region)
            {
                case "EUW":
                    connection.Connect(username, password, Region.EUW, FormMain.cversion);
                    break;
                case "EUNE":
                    connection.Connect(username, password, Region.EUN, FormMain.cversion);
                    break;
                case "NA":
                    connection.Connect(username, password, Region.NA, FormMain.cversion);
                    regionURL = "NA1";
                    break;
                case "KR":
                    connection.Connect(username, password, Region.KR, FormMain.cversion);
                    break;
                case "BR":
                    connection.Connect(username, password, Region.BR, FormMain.cversion);
                    break;
                case "OCE":
                    connection.Connect(username, password, Region.OCE, FormMain.cversion);
                    break;
                case "RU":
                    connection.Connect(username, password, Region.RU, FormMain.cversion);
                    break;
                case "TR":
                    connection.Connect(username, password, Region.TR, FormMain.cversion);
                    break;
                case "LAS":
                    connection.Connect(username, password, Region.LAS, FormMain.cversion);
                    break;
                case "LAN":
                    connection.Connect(username, password, Region.LAN, FormMain.cversion);
                    break;
            }
        }

        public async void connection_OnMessageReceived(object sender, object message)
        {
            if (message is GameDTO)
            {
                GameDTO game = message as GameDTO;
                switch (game.GameState)
                {
                    case "CHAMP_SELECT":
                        if (this.firstTimeInLobby)
                        {
                            QueueFlag = true;
                            firstTimeInLobby = false;
                            FormMain.UpdateStatus("Champion Select", Accountname, "Status");
                            object obj = await connection.SetClientReceivedGameMessage(game.Id, "CHAMP_SELECT_CLIENT");
                            if (queueType != QueueTypes.ARAM)
                            {


                                int Spell1 = Enums.spellToId(FormMain.GetSpells(Accountname, "1"));
                                int Spell2 = Enums.spellToId(FormMain.GetSpells(Accountname, "2"));


                                await connection.SelectChampion(Enums.championToId(FormMain.GetChampion(Accountname)));
                                await connection.SelectSpells(Spell1, Spell2);
                                await connection.ChampionSelectCompleted();

                            }
                            break;
                        }
                        else
                            break;
                    case "POST_CHAMP_SELECT":
                        firstTimeInLobby = false;
                        FormMain.UpdateStatus("Lock Champion", Accountname, "Status");
                        break;
                    case "GAME_START_CLIENT":
                        FormMain.UpdateStatus("Game client run", Accountname, "Status");
                        break;
                    case "GameClientConnectedToServer":
                        FormMain.UpdateStatus("Client connected to the server", Accountname, "Status");
                        break;
                    case "IN_QUEUE":
                        FormMain.UpdateStatus("In Queue", Accountname, "Status");
                        QueueFlag = true;
                        break;
                    case "TERMINATED":
                        FormMain.UpdateStatus("In Queue", Accountname, "Status");
                        this.firstTimeInQueuePop = true;
                        break;
                    case "JOINING_CHAMP_SELECT":
                        if (this.firstTimeInQueuePop && game.StatusOfParticipants.Contains("1"))
                        {
                            FormMain.UpdateStatus("Accept Queue", Accountname, "Status");
                            this.firstTimeInQueuePop = false;
                            this.firstTimeInLobby = true;
                            object obj = await this.connection.AcceptPoppedGame(true);
                            break;
                        }
                        else
                            break;
                    case "LEAVER_BUSTED":
                        FormMain.UpdateStatus("Leave busted", Accountname, "Status");
                        break;
                }
            }
            else if (message is PlayerCredentialsDto)
            {
                string str = Enumerable.Last<string>((IEnumerable<string>)Enumerable.OrderBy<string, DateTime>(Directory.EnumerateDirectories((this.ipath ?? "") + "RADS\\solutions\\lol_game_client_sln\\releases\\"), (Func<string, DateTime>)(f => new DirectoryInfo(f).CreationTime))) + "\\deploy\\";
                LoLLauncher.RiotObjects.Platform.Game.PlayerCredentialsDto credentials = message as PlayerCredentialsDto;
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.WorkingDirectory = str;
                startInfo.FileName = "League of Legends.exe";
                startInfo.Arguments = "\"8394\" \"LoLLauncher.exe\" \"\" \"" + credentials.ServerIp + " " +
                credentials.ServerPort + " " + credentials.EncryptionKey + " " + credentials.SummonerId + "\"";
                FormMain.UpdateStatus("Launching League of Legends", Accountname, "Status");
                new Thread((ThreadStart)(() =>
                {
                    exeProcess = Process.Start(startInfo);
                    exeProcess.Exited += exeProcess_Exited;
                    while (exeProcess.MainWindowHandle == IntPtr.Zero) ;
                    exeProcess.PriorityClass = ProcessPriorityClass.Idle;
                    exeProcess.EnableRaisingEvents = true;
                })).Start();
            }
            else if (!(message is GameNotification) && !(message is SearchingForMatchNotification))
            {
                if (message is EndOfGameStats)
                {
                    EndOfGameStats msg = message as EndOfGameStats;
                    LoLLauncher.RiotObjects.Platform.Matchmaking.MatchMakerParams matchParams = new LoLLauncher.RiotObjects.Platform.Matchmaking.MatchMakerParams();
                    if (queueType == QueueTypes.INTRO_BOT)
                    {
                        matchParams.BotDifficulty = "INTRO";
                    }
                    else if (queueType == QueueTypes.BEGINNER_BOT)
                    {
                        matchParams.BotDifficulty = "EASY";
                    }
                    else if (queueType == QueueTypes.MEDIUM_BOT)
                    {
                        matchParams.BotDifficulty = "MEDIUM";
                    }

                    if (sumLevel == 3 && actualQueueType == QueueTypes.NORMAL_5x5)
                    {
                        queueType = actualQueueType;
                    }
                    else if (sumLevel == 6 && actualQueueType == QueueTypes.ARAM)
                    {
                        queueType = actualQueueType;
                    }
                    else if (sumLevel == 7 && actualQueueType == QueueTypes.NORMAL_3x3)
                    {
                        queueType = actualQueueType;
                    }

                    matchParams.QueueIds = new Int32[1] { (int)queueType };
                    LoLLauncher.RiotObjects.Platform.Matchmaking.SearchingForMatchNotification m = await connection.AttachToQueue(matchParams);
                    
                    if (m.PlayerJoinFailures == null)
                    {
                        FormMain.UpdateStatus("In Queue", Accountname, "Status");
                    }
                    else
                    {

                        foreach (QueueDodger current in m.PlayerJoinFailures)
                        {
                            if (current.ReasonFailed == "LEAVER_BUSTED")
                            {
                                m_accessToken = current.AccessToken;
                                if (current.LeaverPenaltyMillisRemaining > this.m_leaverBustedPenalty)
                                {
                                    this.m_leaverBustedPenalty = current.LeaverPenaltyMillisRemaining;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(this.m_accessToken))
                        {
                            FormMain.UpdateStatus("Waiting For Leaver Busted: " + (float)(this.m_leaverBustedPenalty / 1000) / 60f + " Minute", this.Accountname, "Status");
                            Thread.Sleep(TimeSpan.FromMilliseconds((double)this.m_leaverBustedPenalty));
                            
                            m = await connection.AttachToLowPriorityQueue(matchParams, this.m_accessToken);
                            if (m.PlayerJoinFailures == null)
                            {
                                FormMain.UpdateStatus("In Queue: " + queueType.ToString(), this.Accountname, "Status");
                            }
                            else
                            {
                                FormMain.UpdateStatus("There was an error in joining lower priority queue.\nDisconnecting.", this.Accountname, "Status");
                                this.connection.Disconnect();
                            }
                        }
                    }
                }
                else
                {
                    if (message.ToString().Contains("EndOfGameStats"))
                    {
                        EndOfGameStats eog = new EndOfGameStats();
                        connection_OnMessageReceived(sender, eog);
                        exeProcess.Exited -= exeProcess_Exited;
                        exeProcess.Kill();
                        Thread.Sleep(500);

                        Process.Start("taskkill /F /IM \"League of Legends.exe\"");
                        loginPacket = await this.connection.GetLoginDataPacketForUser();
                        archiveSumLevel = sumLevel;
                        sumLevel = loginPacket.AllSummonerData.SummonerLevel.Level;
                        

                        FormMain.UpdateStatus(loginPacket.IpBalance.ToString("#"), Accountname, "IP (Get IP)");
                        FormMain.UpdateStatus(loginPacket.AllSummonerData.SummonerLevelAndPoints.ExpPoints.ToString("#") , Accountname, "XP");
                        if (sumLevel != archiveSumLevel)
                        {
                            levelUp();
                        }
                    }
                }
            }
        }

        void exeProcess_Exited(object sender, EventArgs e)
        {
            if (connection.IsConnected())
            {
                FormMain.UpdateStatus("Restart League of Legends.", Accountname, "Status");
                Thread.Sleep(1000);
                if (this.loginPacket.ReconnectInfo != null && this.loginPacket.ReconnectInfo.Game != null)
                {
                    this.connection_OnMessageReceived(sender, (object)this.loginPacket.ReconnectInfo.PlayerCredentials);
                }
                else
                    this.connection_OnMessageReceived(sender, (object)new EndOfGameStats());
            }
        }
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
      
        
        private async void RegisterNotifications()
        {
            object obj1 = await this.connection.Subscribe("bc", this.connection.AccountID());
            object obj2 = await this.connection.Subscribe("cn", this.connection.AccountID());
            object obj3 = await this.connection.Subscribe("gn", this.connection.AccountID());
        }
        
        private void connection_OnLoginQueueUpdate(object sender, int positionInLine)
        {
            if (positionInLine <= 0)
                return;
            FormMain.UpdateStatus("Position to login: " + (object)positionInLine, Accountname, "Status");
        }

        private void connection_OnLogin(object sender, string username, string ipAddress)
        {
            new Thread((ThreadStart)(async () =>
            {
                FormMain.UpdateStatus("Connecting", Accountname, "Status");
                this.RegisterNotifications();
                this.loginPacket = await this.connection.GetLoginDataPacketForUser(); 
                if (loginPacket.AllSummonerData == null)
                {
                    Random rnd = new Random();
                    String summonerName = Accountname;
                    if (summonerName.Length > 16)
                        summonerName = summonerName.Substring(0, 12) + new Random().Next(1000, 9999).ToString();
                    LoLLauncher.RiotObjects.Platform.Summoner.AllSummonerData sumData = await connection.CreateDefaultSummoner(summonerName);
                    loginPacket.AllSummonerData = sumData;
                    FormMain.UpdateStatus("Created Summonername " + summonerName, Accountname, "Status");
                }
                sumLevel = loginPacket.AllSummonerData.SummonerLevel.Level;
                string sumName = loginPacket.AllSummonerData.Summoner.Name;
                double sumId = loginPacket.AllSummonerData.Summoner.SumId;
                rpBalance = loginPacket.RpBalance;
                FormMain.UpdateStatus(loginPacket.IpBalance.ToString("#"), Accountname, "IP (Get IP)");
                FormMain.UpdateStatus(loginPacket.AllSummonerData.SummonerLevel.Level.ToString("#"), Accountname, "Level");
                FormMain.UpdateStatus(loginPacket.AllSummonerData.SummonerLevel.ExpToNextLevel.ToString("#"), Accountname, "XP");
                FormMain.UpdateStatus(loginPacket.AllSummonerData.Summoner.Name, Accountname, "Summoner");
                if (sumLevel >= FormMain.GetMaxLevel(Accountname))
                {
                    connection.Disconnect();
                    FormMain.UpdateStatus("is already max level.", Accountname, "Status");
                    //FormMain.lognNewAccount();
                    return;
                }
                
                if (sumLevel < 3.0 && queueType == QueueTypes.NORMAL_5x5)
                {
                    queueType = QueueTypes.BEGINNER_BOT;
                    actualQueueType = QueueTypes.NORMAL_5x5;
                } else if (sumLevel < 6.0 && queueType == QueueTypes.ARAM)
                {
                    queueType = QueueTypes.BEGINNER_BOT;
                    actualQueueType = QueueTypes.ARAM;
                } else if (sumLevel < 7.0 && queueType == QueueTypes.NORMAL_3x3)
                {
                    queueType = QueueTypes.BEGINNER_BOT;
                    actualQueueType = QueueTypes.NORMAL_3x3;
                }

                //FormMain.UpdateStatus("Logged in as " + loginPacket.AllSummonerData.Summoner.Name + " @ level " + loginPacket.AllSummonerData.SummonerLevel.Level, Accountname);
                //FormMain.UpdateStatus(loginPacket.AllSummonerData.Summoner.Name + "@ IP : " + loginPacket.IpBalance, Accountname);
                availableChampsArray = await connection.GetAvailableChampions();
                LoLLauncher.RiotObjects.Team.Dto.PlayerDTO player = await connection.CreatePlayer();
                if (this.loginPacket.ReconnectInfo != null && this.loginPacket.ReconnectInfo.Game != null)
                {
                    this.connection_OnMessageReceived(sender, (object)this.loginPacket.ReconnectInfo.PlayerCredentials);
                }
                else
                    this.connection_OnMessageReceived(sender, (object)new EndOfGameStats());
            })).Start();
        }
        
        private void connection_OnError(object sender, LoLLauncher.Error error)
        {
            if (error.Message.Contains("is not owned by summoner"))
            {
                return;
            }
            else if (error.Message.Contains("Your summoner level is too low to select the spell"))
            {
                var random = new Random();
                var spellList = new List<int> { 13, 6, 7, 10, 1, 11, 21, 12, 3, 14, 2, 4 };

                int index = random.Next(spellList.Count);
                int index2 = random.Next(spellList.Count);

                int randomSpell1 = spellList[index];
                int randomSpell2 = spellList[index2];

                if (randomSpell1 == randomSpell2)
                {
                    int index3 = random.Next(spellList.Count);
                    randomSpell2 = spellList[index3];
                }

                int Spell1 = Convert.ToInt32(randomSpell1);
                int Spell2 = Convert.ToInt32(randomSpell2);
                return;
            }
            FormMain.UpdateStatus(error.Message, Accountname, "Status");
        }
        
        private void connection_OnDisconnect(object sender, EventArgs e)
        {
            FormMain.connectedAccs -= 1;
            //Console.Title = " Current Connected: " + FormMain.connectedAccs;
            FormMain.UpdateStatus("Disconnected", Accountname, "Status");
        }
       
        private void connection_OnConnect(object sender, EventArgs e)
        {
            FormMain.connectedAccs += 1;
            //Console.Title = " Current Connected: " + FormMain.connectedAccs;
        }
 
        public void levelUp()
        {
            FormMain.UpdateStatus(sumLevel.ToString("#"), Accountname, "Level");
            rpBalance = loginPacket.RpBalance;
            if (sumLevel >= FormMain.GetMaxLevel(Accountname))
            {
                connection.Disconnect();
                //bool connectStatus = await connection.IsConnected();
                if (!connection.IsConnected()) {
                FormMain.ConnectAccount(null); 
                }
            }
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (rng == null) throw new ArgumentNullException("rng");

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng)
        {
            List<T> buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}
