using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using System.Reflection;
using MySHelper;
using MySql.Data.MySqlClient;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Examples.Echo;
using System.Net;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using Telegram.Bot.Exceptions;

namespace LiViCiBot
{
    public partial class LiViCiBotDashboardFrm : Form
    {
        public LiViCiBotDashboardFrm()
        {
            InitializeComponent();
        }

        private static string StrConn = "";
        private System.IO.StringWriter writer = new System.IO.StringWriter();
        private const int MaxConsolTxt = 800;
        private static util UTIL = new util();
        private static readonly TelegramBotClient Bot = new TelegramBotClient("Encrypted...");
        private static readonly string PreTableName = "tbl_kidarebot_";
        private static string CategoryListTable = PreTableName + "guilds";
        private static string MarketDetailTable = PreTableName + "market";
        private static string MarketPayMentTable = PreTableName + "market_payment_method";
        private static string MarketSellerDetailDocumentsTable = PreTableName + "market_seller_info";
        private static string MenuTable = PreTableName + "menu";
        private static string UCTable = PreTableName + "ucommand";
        private static string NotInCommandTable = PreTableName + "notincommand";
        private static string ContactUsTable = PreTableName + "contactus";
        private static string MarketOptionTable = PreTableName + "market_options";
        private static string UserOptionTable = PreTableName + "user_options";
        private static string BuyerTable = PreTableName + "buyer";
        private static string BuyerProductTable = PreTableName + "buyer_products";
        private static string MarketSentTable = PreTableName + "market_sent";
        private static string UsersTable = PreTableName + "users";
        private static string VisitorPermision = PreTableName + "visitor_permision";


        private static long ChannelId = -1;

        //Command Text Variable
        private static readonly string Kharidaram = "I'm Buyer";
        private static readonly string Forooshandeam = "I'm Seller";

        private static readonly string CREATE_USER_PROMPT = "\u2705 Congratulations, your phone number has been successfully registered.";
        private static readonly string NOT_MEMBER_PROMPT = "You are not a member of the system, if you want to join the system LiViCi Please click on the button to send the contact number so that we can contact you as soon as possible👇👇👇";

        private static List<string[]> dastoor = new List<string[]>();

        private delegate void SetControlValueCallback(Control oControl, string propName, object propValue);
        private void SetControlPropertyValue(Control oControl, string propName, object propValue)
        {
            if (oControl.InvokeRequired)
            {
                SetControlValueCallback d = new SetControlValueCallback(SetControlPropertyValue);
                oControl.Invoke(d, new object[] {
            oControl,
            propName,
            propValue
        });
            }
            else
            {
                Type t = oControl.GetType();
                PropertyInfo[] props = t.GetProperties();
                foreach (PropertyInfo p in props)
                {
                    if (p.Name.ToUpper() == propName.ToUpper())
                    {
                        p.SetValue(oControl, propValue, null);
                    }
                }
            }
        }

        private void ReadReceivedGprsPacket()
        {
            while (true)
            {
                try
                {
                    if (!StopConsoleChkBox.Checked)
                    {
                        Console.SetOut(writer);
                        SetControlPropertyValue(ConsolTxtBox, "Text", writer.ToString());

                        if (MaxConsolTxt <= ConsolTxtBox.Lines.Length)
                        {
                            writer = new System.IO.StringWriter();
                            Console.Clear();
                            SetControlPropertyValue(ConsolTxtBox, "Text", null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Thread.Sleep(50);
            }
        }

        private static string[] CheckAndGetUCommands(Telegram.Bot.Types.Message message)
        {
            if (dastoor.Count > 1000) dastoor.RemoveRange(0, 30);
            int indx = dastoor.FindIndex(x => x[0] == message.Chat.Id.ToString());
            string[] RetVal = new string[5];

            Mys HLP = new Mys(StrConn);
            if (indx < 0)
            {
                string[] uc = new string[5];
                using (MySqlDataReader DR = HLP.Sql_Query_DataReader(String.Format("select * from {0} where uid = {1}", UCTable, message.Chat.Id.ToString())))
                {
                    while (DR.Read())
                    {
                        uc[0] = DR["uid"].ToString();
                        uc[1] = DR["usercommand"].ToString();
                        uc[2] = "nothing";//DR["offset"].ToString();
                        uc[3] = DR["maproute"].ToString();
                        uc[4] = "nothing";//DR["pviewcount"].ToString();
                    }
                }
                if (string.IsNullOrWhiteSpace(uc[0]))
                {
                    uc[0] = message.Chat.Id.ToString();
                    uc[1] = "start";
                    uc[2] = "nothing";
                    uc[3] = "start-";
                    uc[4] = "nothing";
                    string uname = "";
                    if (!string.IsNullOrWhiteSpace(message.Chat.Username))
                        uname = message.Chat.Username.ToString();
                    HLP.AddRecord(UCTable, "uid,usercommand,maproute,username,nameandfamily", "'" + message.Chat.Id.ToString() + "','start','start-','" + uname + "','" + message.Chat.FirstName + " " + message.Chat.LastName + "'");
                }
                else
                {
                    if (!message.Text.Contains(" || "))
                        HLP.UpdateRecord(UCTable, "usercommand = '" + UTIL.RemoveKeshidanAndNimSpace(message.Text) + "',maproute = CONCAT(maproute, '" + UTIL.RemoveKeshidanAndNimSpace(message.Text) + "-')", "uid = '" + message.Chat.Id.ToString() + "'");
                    uc[1] = UTIL.RemoveKeshidanAndNimSpace(message.Text);
                }
                dastoor.Add(uc);
                indx = dastoor.FindIndex(x => x[0] == message.Chat.Id.ToString());
                RetVal = dastoor[indx];
            }
            else
            {
                if (message.Text != "Previous menu 🔙" && message.Text != "Previous menu")
                {
                    if (!message.Text.Contains(" || "))
                        HLP.UpdateRecord(UCTable, "usercommand = '" + message.Text.Replace(" 🏠", "") + "',maproute = CONCAT(maproute, '" + message.Text.Replace(" 🏠", "") + "-')", "uid = '" + message.Chat.Id.ToString() + "'");
                    dastoor[indx][1] = UTIL.RemoveKeshidanAndNimSpace(message.Text);
                    dastoor[indx][2] = "nothing";
                    if (!message.Text.Contains(" || "))
                        dastoor[indx][3] = dastoor[indx][3] + UTIL.RemoveKeshidanAndNimSpace(message.Text) + "-";//HLP.GetLastRecordWithWhere(UCTable,"id","idf = '"+ message.Chat.Id.ToString()+ "'","maproute");
                }
                else
                {
                    //string pvc = HLP.GetLastRecordWithWhere(UCTable, "id", "idf = '" + message.Chat.Id.ToString() + "'", "pviewcount");
                    //HLP.UpdateRecord(UCTable, "offset = 1", "idf = '" + message.Chat.Id.ToString() + "'");
                    dastoor[indx][2] = "nothing";
                }
                RetVal = dastoor[indx];
            }
            return RetVal;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread RRGP = new Thread(ReadReceivedGprsPacket);
            RRGP.Start();

            EncryptRWTextFile enrwtf = new EncryptRWTextFile();
            //enrwtf.WriteOnTextFile(enrwtf.Encrypt("server=localhost;uid=root;password=;database=kidare;pooling=false;CharSet=utf8;default command timeout=600;"));
            string strConnection = enrwtf.Decrypt(enrwtf.ReadFromTextFile());
            StrConn = strConnection;
            Console.WriteLine(StrConn);

            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            Bot.OnUpdate += BotOnUpdate;
        }

        private static void BackMenuBtn(Telegram.Bot.Types.Message message)
        {
            if (dastoor.Count > 1000) dastoor.RemoveRange(0, 10);
            int indx = dastoor.FindIndex(x => x[0] == message.Chat.Id.ToString());
            Mys HLP = new Mys(StrConn);
            string[] dastoorMapRoute = dastoor[indx][3].Split('-');
            dastoor[indx][3] = dastoor[indx][3].Replace(dastoorMapRoute[dastoorMapRoute.Length - 2] + "-", "");
            dastoor[indx][1] = dastoorMapRoute[dastoorMapRoute.Length - 3];
            HLP.UpdateRecord(UCTable, "maproute = '" + dastoor[indx][3] + "', usercommand = '" + dastoorMapRoute[dastoorMapRoute.Length - 3] + "'", "uid = '" + message.Chat.Id.ToString() + "'");
        }
        private static void HomeMenuBtn(string messageID)
        {
            try
            {
                if (dastoor.Count > 1000) dastoor.RemoveRange(0, 10);
                int indx = dastoor.FindIndex(x => x[0] == messageID);
                Mys HLP = new Mys(StrConn);
                try
                {
                    dastoor[indx][3] = "start-";
                    dastoor[indx][1] = "start";
                    HLP.UpdateRecord(UCTable, "maproute = '" + dastoor[indx][3] + "', usercommand = 'start'", "uid = '" + messageID + "'");
                }
                catch (Exception ex) { }
                string Naghes_Id = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '' and uid = '" + messageID + "'","id");
                if (string.IsNullOrEmpty(Naghes_Id))
                    HLP.UpdateRecord(BuyerProductTable, "status = ''", "id = " + Naghes_Id);
                HLP.UpdateRecord(MarketDetailTable, "maproute = '', currentstoreid = '', currentpic = '' , currentprice = '' , currentdescription = '' ", "uid = '" + messageID + "'");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void WriteOnConsole(Chat chat, string CText)
        {
            Console.WriteLine("Communicate with : FN: " + chat.FirstName + " - LN: " + chat.LastName + " - UN: " + chat.Username + " - ID: " + chat.Id.ToString());
            Console.WriteLine("--- " + CText);
        }

        private static KeyboardButton[][] SetKeyboards(int RowCount, string[][] KeyboardsText)
        {
            KeyboardButton[][] KeyBord = new KeyboardButton[RowCount][];
            for (int r = 0; r < RowCount; r++)
            {
                KeyboardButton[] keybord = new KeyboardButton[KeyboardsText[r].Length];
                for (int c = 0; c < KeyboardsText[r].Length; c++)
                {
                    keybord[c] = KeyboardsText[r][c];
                }
                KeyBord[r] = keybord;
            }
            return KeyBord;
        }

        private static async Task ShowMenu(Telegram.Bot.Types.Message msg, string menuname, string hint = "")
        {
            try
            {
                Mys HLP = new Mys(StrConn);
                string[] EachRow = HLP.GetLastRecordWithWhere(MenuTable, "id", "menuname like '" + menuname + "'", "items").Split('~');
                string[][] Menues = new string[EachRow.Length][];
                for (int i = 0; i < EachRow.Length; i++)
                {
                    //⬅️➡️💰☎️🏠👜🆔🎁📝❌➕
                    //EachRow[i] = EachRow[i].Replace(" قبلی", " قبلی ⬅️");
                    //EachRow[i] = EachRow[i].Replace(" بعدی", " بعدی ➡️");
                    //EachRow[i] = EachRow[i].Replace(" اولیه", " اولیه 🏠");
                    //EachRow[i] = EachRow[i].Replace("جدید", "جدید ➕");
                    //EachRow[i] = EachRow[i].Replace("ویرایش", "ویرایش 📝");
                    //EachRow[i] = EachRow[i].Replace("خریدار هستم", "خریدار هستم 👜");
                    //EachRow[i] = EachRow[i].Replace("فروشنده هستم", "فروشنده هستم 💰");

                    string[] EachColumn = EachRow[i].Split('^');
                    Menues[i] = EachColumn;
                }
                string usage = hint;
                if (string.IsNullOrWhiteSpace(usage))
                    usage = HLP.GetLastRecordWithWhere(MenuTable, "id", "menuname = '" + menuname + "'", "hint");
                if (menuname == "Main Menu" && msg.Text.StartsWith("/start"))
                {
                    usage = "Hi Welcome to LiViCi"
                        + " \n " + usage;
                    if (msg.Text.StartsWith("/start "))
                    {
                        
                    }
                }
                if (HLP.CountOfRecordsWhere(MarketDetailTable, "uid = '" + msg.Chat.Id.ToString() + "'") <= 0)
                {
                    var keyboardSendContact = new ReplyKeyboardMarkup(new[]
                        {
                                    new []
                                    {
                                        new KeyboardButton(Forooshandeam) { RequestContact = true },
                                        new KeyboardButton(Kharidaram),
                                    },
                                    new []
                                    {
                                        new KeyboardButton("Contact us"),
                                    }
                    });
                    await Bot.SendTextMessageAsync(msg.Chat.Id, usage,
                        replyMarkup: keyboardSendContact);
                }
                else if ((HLP.CountOfRecordsWhere(MarketDetailTable, "uid = '" + msg.Chat.Id.ToString() + "'") > 0) && (!string.IsNullOrWhiteSpace(HLP.GetLastRecordWithWhere(MarketDetailTable, "uid = '" + msg.Chat.Id.ToString() + "'","seller_mobile"))))
                {
                    var keyboardSendContact = new ReplyKeyboardMarkup(new[]
                        {
                                    new []
                                    {
                                        new KeyboardButton(Forooshandeam) { RequestContact = true },
                                        new KeyboardButton(Kharidaram),
                                    },
                                    new []
                                    {
                                        new KeyboardButton("Contact us"),
                                    }
                    });
                    await Bot.SendTextMessageAsync(msg.Chat.Id, usage,
                        replyMarkup: keyboardSendContact);
                }
                else
                {
                    var keyboard = new ReplyKeyboardMarkup(SetKeyboards(EachRow.Length, Menues));
                    await Bot.SendTextMessageAsync(msg.Chat.Id, usage,
                        replyMarkup: keyboard);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task<string[][]> GetListAsync(string Where, string tbl, string fieldName, string keySymbol)
        {
            Mys HLP = new Mys(StrConn);
            int catCount = 0;

            using (MySqlDataReader DR = HLP.Sql_Query_DataReader(String.Format("select * from {0} where {1}", tbl, Where)))
            {
                while (DR.Read())
                {
                    catCount++;
                }
            }

            string[][] productsList = new string[catCount][];
            catCount = 0;
            using (MySqlDataReader DR = HLP.Sql_Query_DataReader(String.Format("select * from {0} where {1}", tbl, Where)))
            {
                while (DR.Read())
                {
                    string[] strCat = new string[] { keySymbol + DR[fieldName].ToString() };
                    productsList[catCount] = strCat;
                    catCount++;
                }
            }
            return productsList;
        }

        private static async Task ShowMyCategoriesAsync(Telegram.Bot.Types.Message msg)
        {
            try
            {
                Mys HLP = new Mys(StrConn);
                string SelectQuery1 = "";
                SelectQuery1 = String.Format("name != '' and idref = 0 and status = 'active' order by id");

                var usage = "In which of the following groups is your product? \n (If the desired group is not in the following options, select the last option based on the product name)";

                Task<string[][]> arrayKeyTemp = GetListAsync(SelectQuery1, CategoryListTable, "name", "* ");
                string[][] arrayKeyTemp2 = await arrayKeyTemp;
                string[][] arrayKey = new string[arrayKeyTemp2.Length + 1][];
                for (int i = 0; i < arrayKeyTemp2.Length; i++)
                {
                    arrayKey[i] = arrayKeyTemp2[i];
                }
                arrayKey[arrayKeyTemp2.Length] = new string[] { "Main Menu 🏠" };

                var keyboard = new ReplyKeyboardMarkup(SetKeyboards(arrayKey.Length, arrayKey));
                await Bot.SendTextMessageAsync(msg.Chat.Id, usage,
                    replyMarkup: keyboard);//,parseMode: ParseMode.Html
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task StartAsync(Telegram.Bot.Types.Message msg)
        {
            await ShowMenu(msg, "منوی اولیه");
        }

        private static async Task ForooShandeRequestButton(Telegram.Bot.Types.Message msg)
        {
            try
            {
                var keyboardHide = new ReplyKeyboardHide();

                var keyboardSendContact = new ReplyKeyboardMarkup(new[]
                                    {
                                    new []
                                    {
                                        new KeyboardButton("Send Phone Number") { RequestContact = true },
                                    },
                                    new []
                                    {
                                        new KeyboardButton("Main Menu 🏠"),
                                    }
                                });
                var keyboardMenu = new ReplyKeyboardMarkup(new[]
                        {
                                    new []
                                    {
                                        new KeyboardButton("Main Menu 🏠"),
                                    }
                                });
                Mys HLP = new Mys(StrConn);
                if (HLP.CountOfRecordsWhere(MarketDetailTable, "uid = '" + msg.Chat.Id.ToString() + "'") > 0)
                {
                    string senf = "";
                    string IsActive = HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "is_active");

                    string finalIdCoded = "";
                    string s = msg.Chat.Id.ToString();
                    foreach (char c in s)
                    {
                        finalIdCoded += ((int)c + 63).ToString();
                    }

                    string deActiveForooshandeMSG = "⛔️If your store information has not been received and registered by our marketers, please click on the link below and enter your information. \n ✳️ Note: To activate immediately in the system, you can enter your basic information through the following link. \n http://kidarebot.com/usermanager/update.php?name=" + finalIdCoded;

                    if (IsActive == "True")
                    {
                        string guildID = HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "guild_id");
                        senf = HLP.GetLastRecordWithWhere(CategoryListTable, "id", "id = '" + guildID + "'", "name");
                        if (senf == "Based on the name of the product 📦")
                        {
                            senf = HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "tags");
                        }
                        string activeForooshandeMSG = "You are in the system as a certified and active shopkeeper, in the class " + senf + " You have a membership. "
                                  + " We will send it to you whenever a purchase request is received from the buyer."
                                  + " \n "
                                  + " To edit or modify your information, go to the link below and modify it, or write a modification request in the Contact Us section (in the main menu). : \n http://kidarebot.com/usermanager/update.php?name=" + finalIdCoded + " \n";
                        await Bot.SendTextMessageAsync(msg.Chat.Id, activeForooshandeMSG,
                                 replyMarkup: keyboardMenu);
                    }
                    else
                        await Bot.SendTextMessageAsync(msg.Chat.Id, deActiveForooshandeMSG,
                             replyMarkup: keyboardMenu);
                }
                else
                {
                    await Bot.SendTextMessageAsync(msg.Chat.Id, NOT_MEMBER_PROMPT,
                        replyMarkup: keyboardSendContact);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task SellerRequestAsync(Telegram.Bot.Types.Message msg)
        {
            try
            {
                var keyboardHide = new ReplyKeyboardHide();

                Mys HLP = new Mys(StrConn);

                string FMapRoute = HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "maproute");

                if (FMapRoute == "getprice")
                {
                    if (msg.Type != MessageType.TextMessage)
                        return;
                    string price = UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim();
                    long num;
                    if (long.TryParse(price, out num))
                    {
                        HLP.UpdateRecord(MarketDetailTable, "currentprice = '" + UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim() + "', maproute = 'getdescription'", "uid = '" + msg.Chat.Id.ToString() + "'");

                        var usaget = @"If you have a description of the requested product, write and send "
                                    + " \n "
                                    + " \n /nodescription - ⬅️ I have no explanation \n"
                                    + " \n /menu - ⬅️ 🏠 Cancel ang go to Main Menu";
                        await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                            replyMarkup: keyboardHide);
                    }
                    else
                    {
                        string usaget = "Please enter the price only as a number, which is considered in Tomans, for example, if the price of a product is fourteen thousand five hundred Tomans, enter : 14500";
                        await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                            replyMarkup: keyboardHide);
                    }
                }
                else if (FMapRoute == "getdescription")
                {
                    if (msg.Type != MessageType.TextMessage)
                        return;
                    string description = UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim();//nodescription
                    if (description == "/nodescription")
                        description = "I have no explanation";
                    HLP.UpdateRecord(MarketDetailTable, "currentdescription = '" + description + "', maproute = 'getimage'", "uid = '" + msg.Chat.Id.ToString() + "'");

                    var usaget = @"If you have a picture of the desired product or similar product, click the button 📎 an send it. "
                                + " \n Please send only one picture"
                                + " \n "
                                + " \n /nopic - ⬅️ I do not have a photo of the product \n"
                                + " \n /menu - ⬅️ 🏠 Cancel and go to Main Menu";
                    await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                        replyMarkup: keyboardHide);
                }
                else if (FMapRoute == "getimage")
                {
                    var usaget = @"Your information was sent to the customer. If the customer agrees, he will request a purchase with you by telegram or by phone or in person.";
                    string price = "";
                    string description = "";
                    string phoneNumber = "";
                    string phoneNumberS = "";
                    string userName = "";
                    string address = "";

                    using (MySqlDataReader DR = HLP.Sql_Query_DataReader(String.Format("select * from {0} where uid = '{1}' order by uid desc limit 1", MarketDetailTable, msg.Chat.Id.ToString())))
                    {
                        while (DR.Read())
                        {
                            price = DR["currentprice"].ToString();
                            description = DR["currentdescription"].ToString();
                            phoneNumber = DR["seller_mobile"].ToString();
                            phoneNumberS = DR["market_phone"].ToString();
                            userName = DR["seller_tid"].ToString();
                            address = DR["market_address"].ToString();
                        }
                    }
                    if (description.Length > 62)
                        description = description.Remove(62);
                    if (address.Length > 24)
                        address = address.Remove(24);

                    if (msg.Type == MessageType.TextMessage)
                    {
                        string pic = UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim();//nodescription
                        if (pic == "/nopic")
                            pic = "I do not have a photo of the product";

                        HLP.UpdateRecord(MarketDetailTable, "currentpic = '" + pic + "', maproute = ''", "uid = '" + msg.Chat.Id.ToString() + "'");
                        string allP = "";
                        if (!string.IsNullOrWhiteSpace(description))
                            allP = " \n Description: " + description;
                        if (!string.IsNullOrWhiteSpace(address))
                            allP += " \n Address: " + address + "...";
                        if (!string.IsNullOrWhiteSpace(phoneNumber))
                            allP += " \n 📱: " + phoneNumber;
                        if (!string.IsNullOrWhiteSpace(phoneNumberS))
                            allP += " \n ☎️: " + phoneNumberS;
                        if (!string.IsNullOrWhiteSpace(userName))
                            allP += " \n 🆔: @" + userName;
                        var PhotoCaption = @"Price: " + UTIL.PriceSpliter(price) + " $"
                             + allP
                             + " \n @kidarebot";
                        if (PhotoCaption.Length > 197)
                        {
                            PhotoCaption.Remove(197);
                        }

                        string curentStoreID = HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "currentstoreid");
                        curentStoreID = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '" + curentStoreID + "'", "uid");
                        var ChannelRetMsg = await Bot.SendTextMessageAsync(ChannelId, PhotoCaption);
                        try
                        {
                            await Bot.ForwardMessageAsync(Convert.ToInt32(curentStoreID), ChannelId, ChannelRetMsg.MessageId);
                            HLP.AddRecord(MarketSentTable, "uid, price, description, storeid, final_buyer_uid", "'" + msg.Chat.Id.ToString() + "', '" + price + "', '" + description + "', '" + ChannelRetMsg.MessageId.ToString() + "', '" + curentStoreID + "'");

                            string txtFinal = "Your price, description, photo and contact details were sent to the customer. If the customer agrees with the price and other specifications, he will contact you by telegram, either by phone or in person.."
                                              + " \n "
                                              + " \n /menu - ⬅️ 🏠 Main Menu";
                            await Bot.SendTextMessageAsync(msg.Chat.Id, txtFinal,
                                replyMarkup: keyboardHide);

                            HLP.UpdateRecord(MarketDetailTable, "currentstoreid = '', maproute = '', currentpic = '', currentdescription = '', currentprice = ''", "uid = '" + msg.Chat.Id.ToString() + "'");
                            var ReplyKey = new InlineKeyboardMarkup(new[]
                                        {
                                        new[]
                                        {
                                            new InlineKeyboardButton("Full specifications","marketDetail:"+msg.Chat.Id.ToString())
                                        }
                                    });
                            await Bot.SendTextMessageAsync(curentStoreID, "Seller Full specifications 👆👆👆 ",
                                replyMarkup: ReplyKey);
                            string finishMSG = "If your purchase is done and you do not want the price and description to be sent to you from other sellers, click the My purchase was done"
                                                + " \n "
                                                + " \n /finish - ⬅️ I bought it, don't send it anymore";
                            await Bot.SendTextMessageAsync(curentStoreID, finishMSG,
                                replyMarkup: keyboardHide);
                        }
                        catch (ApiRequestException ax)
                        {
                            Console.WriteLine(ax.Message);
                        }
                    }
                    else if (msg.Type == MessageType.PhotoMessage)
                    {
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "The system is receiving the photo sent by you, please wait ...",
                                replyMarkup: keyboardHide);
                        HLP.UpdateRecord(MarketDetailTable, "currentpic = 'دارد', maproute = ''", "uid = '" + msg.Chat.Id.ToString() + "'");

                        var fm = msg.Photo;
                        string ss = fm[fm.Length - 1].FileId;
                        ss = "https://api.telegram.org/bot404715834:AAEBz8MnqTZEqMfqXLrexxvKzVOb3XhUnFg/getFile?file_id=" + ss;
                        using (WebClient wc = new WebClient())
                        {
                            try
                            {
                                var json = wc.DownloadString(ss);
                                RootObject ro = JsonConvert.DeserializeObject<RootObject>(json);
                                bool ok = ro.ok;
                                ss = "https://api.telegram.org/file/bot404715834:AAEBz8MnqTZEqMfqXLrexxvKzVOb3XhUnFg/" + ro.result.file_path;
                            }
                            catch (Exception asd) { }
                        }
                        WebRequest req = HttpWebRequest.Create(ss);
                        try
                        {
                            using (Stream stream = req.GetResponse().GetResponseStream())
                            {
                                var fts = new FileToSend(ss.Split('/').Last(), stream);
                                string allP = "";
                                if (!string.IsNullOrWhiteSpace(description))
                                    allP = " \n Description: " + description;
                                if (!string.IsNullOrWhiteSpace(address))
                                    allP += " \n Address: " + address + "...";
                                if (!string.IsNullOrWhiteSpace(phoneNumber))
                                    allP += " \n 📱: " + phoneNumber;
                                if (!string.IsNullOrWhiteSpace(phoneNumberS))
                                    allP += " \n ☎️: " + phoneNumberS;
                                if (!string.IsNullOrWhiteSpace(userName))
                                    allP += " \n 🆔: @" + userName;
                                var PhotoCaption = @"Price: " + UTIL.PriceSpliter(price) + " $"
                                     + allP
                                     + " \n @kidarebot";
                                if (PhotoCaption.Length > 195)
                                {
                                    PhotoCaption.Remove(195);
                                }

                                string curentStoreID = HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "currentstoreid");
                                curentStoreID = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '" + curentStoreID + "'", "uid");
                                var ChannelRetMsg = await Bot.SendPhotoAsync(ChannelId, fts, PhotoCaption);
                                await Bot.ForwardMessageAsync(Convert.ToInt32(curentStoreID), ChannelId, ChannelRetMsg.MessageId);
                                HLP.AddRecord(MarketSentTable, "uid, price, description, storeid, final_buyer_uid", "'" + msg.Chat.Id.ToString() + "', '" + price + "', '" + description + "', '" + ChannelRetMsg.MessageId.ToString() + "', '" + curentStoreID + "'");

                                string txtFinal = "Your price, description, photo and contact details were sent to the customer. If the customer agrees with the price and other specifications, he will contact you by telegram, either by phone or in person."
                                                  + " \n "
                                                  + " \n /menu - ⬅️ 🏠 Main Menu";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, txtFinal,
                                    replyMarkup: keyboardHide);

                                HLP.UpdateRecord(MarketDetailTable, "currentstoreid = '', maproute = '', currentpic = '', currentdescription = '', currentprice = ''", "uid = '" + msg.Chat.Id.ToString() + "'");
                                var ReplyKey = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            new InlineKeyboardButton("Full specifications","marketDetail:"+msg.Chat.Id.ToString())
                                        }
                                    });
                                await Bot.SendTextMessageAsync(curentStoreID, "Full profile of the above product seller 👇",
                                    replyMarkup: ReplyKey);
                                string finishMSG = "If your purchase is done and you do not want the price and description to be sent to you from other sellers, click the My purchase was done."
                                    + " \n "
                                    + " \n /finish - ⬅️ Do not send anymore, my purchase was done";
                                await Bot.SendTextMessageAsync(curentStoreID, finishMSG,
                                    replyMarkup: keyboardHide);
                            }
                        }
                        catch (ApiRequestException ax)
                        {
                            Console.WriteLine(ax.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task BuyerRequestAsync(Telegram.Bot.Types.Message msg)
        {
            try
            {
                Mys HLP = new Mys(StrConn);
                var keyboardHide = new ReplyKeyboardHide();

                string LastProductID = "";
                try
                {
                    LastProductID = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'");
                }
                catch (Exception ex1) { }
                if (HLP.CountOfRecordsWhere(BuyerTable, "uid = '" + msg.Chat.Id.ToString() + "'") < 1)
                    HLP.AddRecord(BuyerTable, "uid", msg.Chat.Id.ToString());

                string KMapRoute = HLP.GetLastRecordWithWhere(BuyerTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "maproute");
                if (string.IsNullOrEmpty(KMapRoute))
                {
                    if (!string.IsNullOrWhiteSpace(msg.Chat.Username))
                        HLP.UpdateRecord(BuyerTable, "username = '" + msg.Chat.Username + "'", "uid = '" + msg.Chat.Id.ToString() + "'");

                    if (!string.IsNullOrWhiteSpace(msg.Chat.FirstName) || !string.IsNullOrWhiteSpace(msg.Chat.LastName))
                    {
                        if (msg.Type != MessageType.TextMessage)
                            return;
                        if (msg.Text.StartsWith("* "))
                        {
                            string[] msgW = msg.Text.Split(' ');
                            string FinalMsgW = "";
                            for (int i = 1; i < msgW.Length - 1; i++)
                            {
                                FinalMsgW = FinalMsgW + " " + msgW[i];
                            }
                            FinalMsgW = FinalMsgW.Trim();
                            if (HLP.CountOfRecordsWhere(CategoryListTable, "name like '%" + FinalMsgW + "%'") > 0)
                            {
                                HLP.AddRecord(BuyerProductTable, "category, uid", "'" + FinalMsgW + "', " + msg.Chat.Id.ToString());
                                if (FinalMsgW == "Based on the name of the product")
                                {
                                    var usaget = @"Please write what kind of shop do you want your product from (what shop do you sell)? Or what sexual model do you want?"
                                             + " \n Example: Clothing"
                                             + " \n Or, example: bearings"
                                             + " \n "
                                             + " \n /menu - ⬅️ 🏠 Cancel and go to Main Menu";
                                    HLP.UpdateRecord(BuyerTable, "nameandfamily = '" + (msg.Chat.FirstName + " " + msg.Chat.LastName).Trim() + "', maproute = 'getmodeltags'", "uid = '" + msg.Chat.Id.ToString() + "'");
                                    await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                                        replyMarkup: keyboardHide);
                                }
                                else
                                {
                                    var usaget = @"Please write and send the name or description of the product you want "
                                             + " \n "
                                             + " \n /noname - ⬅️ I have no name or explanation \n "
                                             + " \n /menu - ⬅️ 🏠 Cancel and go to Main Menu";
                                    HLP.UpdateRecord(BuyerTable, "nameandfamily = '" + (msg.Chat.FirstName + " " + msg.Chat.LastName).Trim() + "', maproute = 'getnameanddescription'", "uid = '" + msg.Chat.Id.ToString() + "'");
                                    await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                                        replyMarkup: keyboardHide);
                                }
                            }
                            else
                            {
                                await ShowMyCategoriesAsync(msg);
                            }
                        }
                        else
                        {
                            await ShowMyCategoriesAsync(msg);
                            try
                            {
                                LastProductID = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'");
                            }
                            catch (Exception ens) { }
                        }

                        //}
                        //else
                        //    HLP.UpdateRecord(BuyerTable, "nameandfamily = '" + (msg.Chat.FirstName + " " + msg.Chat.LastName).Trim() + "', maproute = 'getnameanddescription'", "uid = '" + msg.Chat.Id.ToString() + "'");
                    }
                }
                else if (KMapRoute == "getnameanddescription")
                {
                    if (msg.Type != MessageType.TextMessage)
                    {
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "Error !!! Please write and submit only one text as name or description.",
    replyMarkup: keyboardHide);
                        return;
                    }
                    string namedescription = UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim();
                    if (namedescription == "/noname")
                        namedescription = "نام و توضیحی ندارم";
                    HLP.UpdateRecord(BuyerProductTable, "nameanddescription = '" + namedescription + "'", "uid = '" + msg.Chat.Id.ToString() + "' and id = " + LastProductID);

                    var usaget = @"If you have a photo of the desired product or similar, click the button 📎 Then take the desired photo or select it from your gallery and finally send it."
                                     + " \n Please Send Only One Picture"
                                     + " \n "
                                     + " \n /nopic - ⬅️ I do not have a photo of the product \n "
                                     + " \n /menu - ⬅️ 🏠 Cancel and go to Main Menu";
                    HLP.UpdateRecord(BuyerTable, "maproute = 'getimage'", "uid = '" + msg.Chat.Id.ToString() + "'");
                    await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                        replyMarkup: keyboardHide);
                }
                else if (KMapRoute == "getmodeltags")
                {
                    if (msg.Type != MessageType.TextMessage)
                    {
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "Error !!! Please write and submit only one text in response to the previous question.",
    replyMarkup: keyboardHide);
                        return;
                    }
                    string namedescription = UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim();
                    if (namedescription.Length < 4)
                    {
                        var usaget = @"Error: You must write a word longer than 5 letters about the shop or gender you want. "
                                 + " \n "
                                 + " \n /menu - ⬅️ 🏠 Cancel and go to Main Menu";
                        HLP.UpdateRecord(BuyerTable, "maproute = 'getmodeltags'", "uid = '" + msg.Chat.Id.ToString() + "'");
                        await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                            replyMarkup: keyboardHide);
                    }
                    else
                    {
                        if (HLP.CountOfRecordsWhere(MarketDetailTable, "tags like '%" + namedescription + "%' and is_active = 1 and blocked = 1") <= 0)
                        {
                            var usagett = @"Sorry there is no seller for this item in the system, please try again with another word."
                                         + " \n "
                                         + " \n /menu - ⬅️ 🏠 Cancel and go to Main Menu";
                            HLP.UpdateRecord(BuyerTable, "maproute = 'getmodeltags'", "uid = '" + msg.Chat.Id.ToString() + "'");
                            await Bot.SendTextMessageAsync(msg.Chat.Id, usagett,
                                replyMarkup: keyboardHide);
                        }
                        else
                        {
                            HLP.UpdateRecord(BuyerProductTable, "tags = '" + namedescription + "'", "uid = '" + msg.Chat.Id.ToString() + "' and id = " + LastProductID);

                            var usaget = @"Please write and send the description of the product you want "
                                     + " \n "
                                     + " \n /noname - ⬅️ I have no name or explanation \n "
                                     + " \n /menu - ⬅️ 🏠 Cancel and go to Main Menu";
                            HLP.UpdateRecord(BuyerTable, "nameandfamily = '" + (msg.Chat.FirstName + " " + msg.Chat.LastName).Trim() + "', maproute = 'getnameanddescription'", "uid = '" + msg.Chat.Id.ToString() + "'");
                            await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                                replyMarkup: keyboardHide);
                        }
                    }
                }
                else if (KMapRoute == "getimage")
                {
                    if (msg.Type == MessageType.TextMessage)
                    {
                        if (HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "nameanddescription") == "I have no name or explanation")
                        {
                            HLP.UpdateRecord(BuyerTable, "maproute = 'getnameanddescription'", "uid = '" + msg.Chat.Id.ToString() + "'");
                            var usagett = @"⛔️⛔️⛔️ You have not entered any description, name or photo, please enter the correct information again."
                                     + " \n "
                                     + "Please write and send the name or description of the product you want "
                                     + " \n "
                                     + " \n /noname - ⬅️ I have no name or explanation \n "
                                     + " \n /menu - ⬅️ 🏠 Cancel and go to Main Menu";
                            await Bot.SendTextMessageAsync(msg.Chat.Id, usagett,
                            replyMarkup: keyboardHide);
                            return;

                        }
                        string pic = UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim();
                        if (pic == "/nopic")
                            pic = "I do not have a photo of the product";
                        HLP.UpdateRecord(BuyerProductTable, "pic = '" + pic + "'", "uid = '" + msg.Chat.Id.ToString() + "' and id = " + LastProductID);

                        var PhotoCaption = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "nameanddescription")
                                                                    + " \n @kidarebot";
                        if (PhotoCaption.Length > 195)
                            PhotoCaption.Remove(195);
                        var channelRetMsg = await Bot.SendTextMessageAsync(msg.Chat.Id, PhotoCaption);
                        channelRetMsg = await Bot.ForwardMessageAsync(ChannelId, msg.Chat.Id, channelRetMsg.MessageId);
                        HLP.UpdateRecord(BuyerProductTable, "storeid = '" + channelRetMsg.MessageId.ToString() + "'", "uid = '" + msg.Chat.Id.ToString() + "' and id = " + LastProductID);
                        PhotoCaption = "* " + PhotoCaption.Replace("\n @kidarebot", "") + " *";
                        if (PhotoCaption.Length > 35)
                            PhotoCaption = "This Product 👆👆👆";
                        var usaget = @"Specifications " + PhotoCaption + " Sent to sellers who have this product or product."
                                     + " \n After a short time by the sellers, the price and other specifications of this product or product will be sent to you here."
                                     + " \n Whenever there is a response from the sellers, we will notify you here"
                                     + " \n "
                                     + " \n /menu - ⬅️ 🏠 Main Menu";
                        HLP.UpdateRecord(BuyerTable, "maproute = ''", "uid = '" + msg.Chat.Id.ToString() + "'");
                        await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
        replyMarkup: keyboardHide);

                        string cat = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "category");
                        string name = HLP.GetLastRecordWithWhere(BuyerTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "nameandfamily");
                        string uname = HLP.GetLastRecordWithWhere(BuyerTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "username");


                        using (MySqlDataReader DR = HLP.Sql_Query_DataReader(String.Format("select * from {0}", UsersTable)))
                        {
                            while (DR.Read())
                            {
                                string uid = DR["uid"].ToString();
                                try
                                {
                                    await Bot.ForwardMessageAsync(uid, ChannelId, channelRetMsg.MessageId);
                                    var ReplyKey = new InlineKeyboardMarkup(new[]
                                        {
                                        new[]
                                        {
                                            new InlineKeyboardButton("Approve - " + "Category:" + cat + " N:" + name + " UN:" + uname , "approve:"+channelRetMsg.MessageId.ToString())
                                        },
                                        new[]
                                        {
                                            new InlineKeyboardButton("Cancel - " + "Category:" + cat + " N:" + name + " UN:" + uname , "notapprove:"+channelRetMsg.MessageId.ToString())
                                        }
                                    });
                                    await Bot.SendTextMessageAsync(uid, "Do you approve the above product?", replyMarkup: ReplyKey);
                                } catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message + " ||| uid: " + uid);
                                }
                            }
                        }
                    }
                    else if (msg.Type == MessageType.PhotoMessage)
                    {
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "The system is receiving the photo sent by you, please wait ...",
                            replyMarkup: keyboardHide);
                        HLP.UpdateRecord(BuyerProductTable, "pic = 'Has it'", "uid = '" + msg.Chat.Id.ToString() + "' and id = " + LastProductID);

                        var fm = msg.Photo;
                        string ss = fm[fm.Length - 1].FileId;
                        ss = "https://api.telegram.org/bot404715834:AAEBz8MnqTZEqMfqXLrexxvKzVOb3XhUnFg/getFile?file_id=" + ss;
                        using (WebClient wc = new WebClient())
                        {
                            try
                            {
                                var json = wc.DownloadString(ss);
                                RootObject ro = JsonConvert.DeserializeObject<RootObject>(json);
                                bool ok = ro.ok;
                                ss = "https://api.telegram.org/file/bot404715834:AAEBz8MnqTZEqMfqXLrexxvKzVOb3XhUnFg/" + ro.result.file_path;
                            }
                            catch (Exception asd) { }
                        }
                        WebRequest req = HttpWebRequest.Create(ss);
                        using (Stream stream = req.GetResponse().GetResponseStream())
                        {
                            var fts = new FileToSend(ss.Split('/').Last(), stream);
                            var PhotoCaption = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "nameanddescription")
                                                + " \n @kidarebot";
                            if (PhotoCaption.Length > 195)
                                PhotoCaption.Remove(195);
                            var channelRetMsg = await Bot.SendPhotoAsync(msg.Chat.Id, fts, PhotoCaption);
                            channelRetMsg = await Bot.ForwardMessageAsync(ChannelId, msg.Chat.Id, channelRetMsg.MessageId);
                            HLP.UpdateRecord(BuyerProductTable, "storeid = '" + channelRetMsg.MessageId.ToString() + "'", "uid = '" + msg.Chat.Id.ToString() + "' and id = " + LastProductID);
                            PhotoCaption = "* " + PhotoCaption.Replace("\n @kidarebot", "") + " *";
                            if (PhotoCaption.Length > 35)
                                PhotoCaption = "This Product 👆👆👆";
                            var usaget = @"Specifications " + PhotoCaption + " Sent to sellers who have this product or product."
                                     + " \n After a short time by the sellers, the price and other specifications of this product or product will be sent to you here."
                                     + " \n Whenever there is a response from the sellers, we will notify you here"
                                     + " \n "
                                     + " \n /menu - ⬅️ 🏠 Main Menu";
                            HLP.UpdateRecord(BuyerTable, "maproute = ''", "uid = '" + msg.Chat.Id.ToString() + "'");
                            await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                                    replyMarkup: keyboardHide);

                            string cat = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "category");
                            string name = HLP.GetLastRecordWithWhere(BuyerTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "nameandfamily");
                            string uname = HLP.GetLastRecordWithWhere(BuyerTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "username");

                            using (MySqlDataReader DR = HLP.Sql_Query_DataReader(String.Format("select * from {0}", UsersTable)))
                            {
                                while (DR.Read())
                                {
                                    string uid = DR["uid"].ToString();
                                    try
                                    {
                                        await Bot.ForwardMessageAsync(uid, ChannelId, channelRetMsg.MessageId);
                                        var ReplyKey = new InlineKeyboardMarkup(new[]
                                            {
                                            new[]
                                            {
                                                new InlineKeyboardButton("Approve - " + "Category:" + cat + " N:" + name + " UN:" + uname , "approve:"+channelRetMsg.MessageId.ToString())
                                            },
                                            new[]
                                            {
                                                new InlineKeyboardButton("Cancel - " + "Category:" + cat + " N:" + name + " UN:" + uname , "notapprove:"+channelRetMsg.MessageId.ToString())
                                            }
                                        });
                                        await Bot.SendTextMessageAsync(uid, "Do you approve the above product?", replyMarkup: ReplyKey);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message + " ||| uid: " + uid);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {

            try
            {
                var keyboardMenu = new ReplyKeyboardMarkup(new[]
                                {
                                    new []
                                    {
                                        new KeyboardButton("Main Menu 🏠"),
                                    }
                                });

                var message = messageEventArgs.Message;
                if (message == null) return;
                if (message.Type == MessageType.TextMessage || message.Type == MessageType.PhotoMessage || message.Type == MessageType.DocumentMessage || message.Type == MessageType.ContactMessage)
                    Application.DoEvents();
                else
                    return;
                try
                {
                    WriteOnConsole(message.Chat, "Selected : " + UTIL.RemoveKeshidanAndNimSpace(message.Text));
                }
                catch (Exception ec) { }

                Mys HLPn = new Mys(StrConn);

                if (message.Type == MessageType.TextMessage)
                {
                    if (message.Text.StartsWith("/start "))
                    {
                        string fname = "";
                        string lname = "";
                        string uname = "";
                        try
                        {
                            fname = message.Chat.FirstName.ToString();
                        }
                        catch (Exception exx1) { }
                        try
                        {
                            lname = message.Chat.LastName.ToString();
                        }
                        catch (Exception exx1) { }
                        try
                        {
                            uname = message.Chat.Username.ToString();
                        }
                        catch (Exception exx1) { }

                        string[] vn = message.Text.Split(' ');

                        if (HLPn.CountOfRecordsWhere(MarketDetailTable, "uid = '" + message.Chat.Id.ToString() + "'") > 0)
                            //HLPn.UpdateRecord(MarketDetailTable,")
                            System.Threading.Thread.Sleep(2);
                        else
                            HLPn.AddRecord(MarketDetailTable, "uid, seller_name, seller_tid, visitorname", "'" + message.Chat.Id.ToString() + "', '" + fname + " " + lname + "', '" + uname + "', '" + vn[1] + "'");
                        await ShowMenu(message, "Main Menu");
                    }
                 }

                if ((message.Chat.Id == 424919314 || message.Chat.Id == 398955499 || message.Chat.Id == 70832084 || message.Chat.Id == 350995247 || message.Chat.Id == 130949826 || message.Chat.Id == 316721834))
                {
                    try
                    {
                        if (message.Text.StartsWith("tel:"))
                        {
                            bool HaveTell = false;
                            using (MySqlDataReader DR = HLPn.Sql_Query_DataReader(String.Format("select seller_mobile from {0} where seller_mobile like '%{1}%'", MarketDetailTable, message.Text.Split(':')[1])))
                            {
                                while (DR.Read())
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "This number already exists : " + DR[0].ToString());
                                    HaveTell = true;
                                }
                            }
                            if (!HaveTell)
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Does not exist");
                            return;
                        }
                        else if (message.Text.StartsWith("marketcount:"))
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, HLPn.CountOfRecords(MarketDetailTable).ToString());
                            return;
                        }
                        else if (message.Text.StartsWith("isactive:"))
                        {
                            string s = "seller_mobile like '%" + message.Text.Split(':')[1] + "%' and is_active = 1";
                            if (HLPn.CountOfRecordsWhere(MarketDetailTable,"seller_mobile like '%"+ message.Text.Split(':')[1] + "%' and is_active = 1") > 0 )
                                await Bot.SendTextMessageAsync(message.Chat.Id, "User information entered");
                            else
                                await Bot.SendTextMessageAsync(message.Chat.Id, "The information is not complete");
                            return;
                        }
                    }
                    catch (Exception en) { }
                }

                string[] ucm = new string[5];

                string LastCommand = HLPn.GetLastRecordWithWhere(UCTable, "id", "uid = '" + message.Chat.Id.ToString() + "'", "usercommand");
                string KMapRoute = HLPn.GetLastRecordWithWhere(BuyerTable, "id", "uid = '" + message.Chat.Id.ToString() + "'", "maproute");
                string FMapRoute = HLPn.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + message.Chat.Id.ToString() + "'", "maproute");

                if (message.Type == MessageType.ContactMessage)
                {
                    string finalIdCoded = "";
                    string s = message.Chat.Id.ToString();
                    foreach (char c in s)
                    {
                        finalIdCoded += ((int)c + 63).ToString();
                    }
                    string deActiveForooshandeMSG = CREATE_USER_PROMPT + " \n ⛔️If your store information has not been received and registered by our marketers, please click on the link below and enter your information. \n ✳️ Note: To activate immediately in the system, you can enter your basic information through the following link. \n http://kidarebot.com/usermanager/update.php?name=" + finalIdCoded;

                    await Bot.SendTextMessageAsync(message.Chat.Id, deActiveForooshandeMSG, replyMarkup: keyboardMenu);
                    string fname = "";
                    string lname = "";
                    string uname = "";
                    try
                    {
                        fname = message.Chat.FirstName.ToString();
                    }
                    catch (Exception exx1) { }
                    try
                    {
                        lname = message.Chat.LastName.ToString();
                    }
                    catch (Exception exx1) { }
                    try
                    {
                        uname = message.Chat.Username.ToString();
                    }
                    catch (Exception exx1) { }

                    string FixedSellerNumber = messageEventArgs.Message.Contact.PhoneNumber.ToString();
                    try
                    {
                        FixedSellerNumber = FixedSellerNumber.Replace("+98", "0");
                    if (FixedSellerNumber.StartsWith("98"))
                        FixedSellerNumber = FixedSellerNumber.Remove(0, 2);
                    if (!FixedSellerNumber.StartsWith("0"))
                        FixedSellerNumber = "0" + FixedSellerNumber;
                        FixedSellerNumber = FixedSellerNumber.Remove(0, 1);
                        FixedSellerNumber = "+98" + FixedSellerNumber;
                    }
                    catch (Exception exx1) { }
                    if (HLPn.CountOfRecordsWhere(MarketDetailTable, "uid = '" + message.Chat.Id.ToString() + "'") > 0)
                        HLPn.UpdateRecord(MarketDetailTable, "seller_mobile = '" + FixedSellerNumber + "'", "uid = '" + message.Chat.Id.ToString() + "'");
                    else
                        HLPn.AddRecord(MarketDetailTable, "uid, seller_mobile, seller_name, seller_tid", "'" + message.Chat.Id.ToString() + "', '" + FixedSellerNumber + "', '" + fname + " " + lname + "', '" + uname + "'");

                    using (MySqlDataReader DR = HLPn.Sql_Query_DataReader(String.Format("select * from {0}", VisitorPermision)))////-
                    {
                        while (DR.Read())
                        {
                            string uid = DR["uid"].ToString();
                            try
                            {
                                await Bot.SendTextMessageAsync(uid, "This person registered with this profile : \n" + "first name and last name : " + fname + " " + lname + " \n User Name : " + uname + " \n  phone number : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); 
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " ||| uid: " + uid);
                            }
                        }
                    }
                    HomeMenuBtn(message.Chat.Id.ToString());
                    return;
                }
                else if (message.Text == "Send contact number")
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, CREATE_USER_PROMPT, replyMarkup: keyboardMenu);
                    string fname = "";
                    string lname = "";
                    string uname = "";
                    try
                    {
                        fname = message.Chat.FirstName.ToString();
                    }
                    catch (Exception exx1) { }
                    try
                    {
                        lname = message.Chat.LastName.ToString();
                    }
                    catch (Exception exx1) { }
                    try
                    {
                        uname = message.Chat.Username.ToString();
                    }
                    catch (Exception exx1) { }
                    if (HLPn.CountOfRecordsWhere(MarketDetailTable, "uid = '" + message.Chat.Id.ToString() + "'") > 0)
                        //HLPn.UpdateRecord(MarketDetailTable, "seller_mobile = '" + FixedSellerNumber + "'", "uid = '" + message.Chat.Id.ToString() + "'");
                        Thread.Sleep(2);
                    else
                        HLPn.AddRecord(MarketDetailTable, "uid, seller_name, seller_tid", "'" + message.Chat.Id.ToString() + "', '" + fname + " " + lname + "', '" + uname + "'");

                    using (MySqlDataReader DR = HLPn.Sql_Query_DataReader(String.Format("select * from {0}", VisitorPermision)))////-
                    {
                        while (DR.Read())
                        {
                            string uid = DR["uid"].ToString();
                            try
                            {
                                await Bot.SendTextMessageAsync(uid, "This person registered with this profile : \n" + "first name and last name : " + fname + " " + lname + " \n User Name : " + uname + " \n  phone number : " + messageEventArgs.Message.Contact.PhoneNumber.ToString());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " ||| uid: " + uid);
                            }
                        }
                    }
                    HomeMenuBtn(message.Chat.Id.ToString());
                    return;
                }

                if (message.Type == MessageType.PhotoMessage && KMapRoute == "getimage" && FMapRoute != "getimage")
                {
                    await BuyerRequestAsync(message);
                    return;
                }
                else if (message.Type == MessageType.PhotoMessage && (KMapRoute == "getnameanddescription" || KMapRoute == "getmodeltags"))
                {
                    var keyboardHide = new ReplyKeyboardHide();
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Error !!! Please write and submit only one text in answer to the previous question.",
                            replyMarkup: keyboardHide);
                    return;
                }

                if (message.Type == MessageType.PhotoMessage && FMapRoute == "getimage" && KMapRoute != "getimage")
                {
                    await SellerRequestAsync(message);
                    return;
                }
                else if (message.Type == MessageType.PhotoMessage && (FMapRoute == "getprice" || FMapRoute == "getdescription"))
                {
                    var keyboardHide = new ReplyKeyboardHide();
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Error !!! Please write and submit only one text in answer to the previous question.",
                            replyMarkup: keyboardHide);
                    return;
                }

                if (message.Type == MessageType.TextMessage)
                {
                    
                    Mys HLP = new Mys(StrConn);

                    if (message.Text == "Do not send anymore, my purchase was done" || message.Text == "/finish")
                    {
                        HomeMenuBtn(message.Chat.Id.ToString());

                        string Naghes_Id = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + message.Chat.Id.ToString() + "'", "id");
                        HLP.UpdateRecord(BuyerProductTable, "status = ''", "id = '" + Naghes_Id + "'");

                        ucm = CheckAndGetUCommands(message);
                        HLP.UpdateRecord(BuyerTable, "maproute = ''", "uid = '" + message.Chat.Id.ToString() + "'");
                        HLP.DeleteRecord(BuyerProductTable, "uid = '" + message.Chat.Id.ToString() + "' and storeid = ''");
                        await ShowMenu(message, "Main Menu");
                        return;
                    }

                    if (message.Text == "/website")
                    {
                        var keyboardHide = new ReplyKeyboardHide();
                        HomeMenuBtn(message.Chat.Id.ToString());
                        ucm = CheckAndGetUCommands(message);
                        string usaget = "http://www.kidarebot.com"
                                    + " \n http://www.kidarebot.ir"
                                    + " \n "
                                    + " \n /menu - ⬅️ 🏠 Main Menu";
                        await Bot.SendTextMessageAsync(message.Chat.Id, usaget, replyMarkup: keyboardHide);
                        return;
                    }
                    if (message.Text == "/law")
                    {
                        var keyboardHide = new ReplyKeyboardHide();
                        HomeMenuBtn(message.Chat.Id.ToString());
                        ucm = CheckAndGetUCommands(message);
                        string usaget = "Warning: Dear buyer, please refrain from publishing any photos and unethical requests separately, otherwise it will be prosecuted."
                                    + " \n"
                                    + " \n 📛 Note: 'Who has' has no interest or responsibility for your transaction, read the safe shopping tips, trade more easily:"
                                    + " \n 1- Never pay before receiving the goods."
                                    + " \n 2- The best and safest way to do online transactions is to buy in person and see the goods closely to ensure the quality of the goods and the accuracy of the seller's statements."
                                    + " \n 3- If possible, receive a written receipt or a valid invoice from the seller when purchasing the goods."
                                    + " \n 4- If you want to buy branded goods, it is better to visit the relevant store to make sure that what is announced is correct."
                                    + " \n"
                                    + " \n ‼ ❌Note❌ ️ ️: 'Who has' system is an interface system, which does not charge any commissions or commissions from transactions and only links the sellers of a product or product to the buyer and its applicant, so The buyer and seller are responsible for buying and selling goods, and this system has no responsibility for the seller and the buyer and the transactions between them."
                                    + " \n"
                                    + " \n See complete rules and recommendations on our website"
                                    + " \n http://www.kidarebot.com"
                                    + " \n "
                                    + " \n /menu - ⬅️ 🏠 Main Menu";
                        await Bot.SendTextMessageAsync(message.Chat.Id, usaget, replyMarkup: keyboardHide);
                        return;
                    }

                    string uc = HLP.GetLastRecordWithWhere(UCTable, "id", "uid = '" + message.Chat.Id.ToString() + "'", "usercommand");

                    if (message.Text == "/start" || message.Text.StartsWith("/start ") || message.Text == "start" || message.Text == "" || message.Text.Contains("Main") || message.Text.Contains("menu") || message.Text.Contains("Main") || message.Text == "Start" || message.Text == "Read the ones I sent. the whole" || message.Text == Kharidaram || message.Text == "Contact us" || message.Text == Forooshandeam)
                    {
                       /* if (message.Text.StartsWith("/start "))
                        {
                            string fname = "";
                            string lname = "";
                            string uname = "";
                            try
                            {
                                fname = message.Chat.FirstName.ToString();
                            }
                            catch (Exception exx1) { }
                            try
                            {
                                lname = message.Chat.LastName.ToString();
                            }
                            catch (Exception exx1) { }
                            try
                            {
                                uname = message.Chat.Username.ToString();
                            }
                            catch (Exception exx1) { }

                            string FixedSellerNumber = messageEventArgs.Message.Contact.PhoneNumber.ToString();
                            try
                            {
                                FixedSellerNumber = FixedSellerNumber.Replace("+98", "0");
                                if (FixedSellerNumber.StartsWith("98"))
                                    FixedSellerNumber = FixedSellerNumber.Remove(0, 2);
                                if (!FixedSellerNumber.StartsWith("0"))
                                    FixedSellerNumber = "0" + FixedSellerNumber;
                                FixedSellerNumber = FixedSellerNumber.Remove(0, 1);
                                FixedSellerNumber = "+98" + FixedSellerNumber;
                            }
                            catch (Exception exx1) { }
                            string[] vn = message.Text.Split(' ');

                            if (HLPn.CountOfRecordsWhere(MarketDetailTable, "uid = '" + message.Chat.Id.ToString() + "'") > 0)
                                //HLPn.UpdateRecord(MarketDetailTable,")
                                System.Threading.Thread.Sleep(2);
                            else
                                HLPn.AddRecord(MarketDetailTable, "uid, seller_mobile, seller_name, seller_tid, visitorname", "'" + message.Chat.Id.ToString() + "', '" + FixedSellerNumber + "', '" + fname + " " + lname + "', '" + uname + "', '" + vn[1] + "'");

                        }*/
                        ucm = CheckAndGetUCommands(message);
                        HLP.UpdateRecord(BuyerTable, "maproute = ''", "uid = '" + message.Chat.Id.ToString() + "'");
                        HLP.DeleteRecord(BuyerProductTable, "uid = '" + message.Chat.Id.ToString() + "' and storeid = ''");
                    }
                    else
                    {
                        string IsForooshande = HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + message.Chat.Id.ToString() + "'", "currentstoreid");
                        if (!string.IsNullOrWhiteSpace(IsForooshande))
                        {
                            await SellerRequestAsync(message);
                            return;
                        }
                        //else if (uc == Forooshandeam)
                        //{
                        //    await ForooShandeRequestButton(message);
                        //    return;
                        //}
                        else if (message.Text.StartsWith("* ") || uc == Kharidaram)
                        {
                            await BuyerRequestAsync(message);
                            return;
                        }
                        else
                        { }

                    }

                    if (message.Text.Contains("Main Menu") || message.Text == "Read the ones I sent. the whole" || message.Text == "menu" || message.Text == "/menu")
                    {
                        ucm = CheckAndGetUCommands(message);
                        HomeMenuBtn(message.Chat.Id.ToString());
                    }
                    if (message.Text.Contains("Previous menu"))
                    {
                        ucm = CheckAndGetUCommands(message);
                        BackMenuBtn(message);
                    }
                    if (!string.IsNullOrWhiteSpace(ucm[1]))
                    {
                        if (ucm[1] == @"/start" || ucm[1] == "start" || ucm[1] == "" || ucm[1].Contains("Main") || ucm[1] == "Start" || ucm[1] == "Read the ones I sent. the whole" || ucm[1] == "/menu" || ucm[1] == "menu")
                            await ShowMenu(message, "Main Menu");
                        else if (ucm[1] == Kharidaram)
                        {
                            await ShowMyCategoriesAsync(message);
                        }
                        else if (ucm[1] == Forooshandeam)
                        {
                            await ForooShandeRequestButton(message);
                            return;
                        }
                        else if (ucm[1] == "Contact us" && message.Text == "Contact us")
                        {
                            await ShowMenu(message, "Contact us");
                        }
                    }
                    else
                    {
                        if (LastCommand == "Contact us")
                        {
                            string uname = "";
                            if (!string.IsNullOrWhiteSpace(message.Chat.Username))
                                uname = message.Chat.Username.ToString();
                            HLP.AddRecord(ContactUsTable, "uid, username, nameandfamily, content", "'" + message.Chat.Id.ToString() + "','" + uname + "','" + message.Chat.FirstName.ToString() + "','" + message.Text.ToString() + "'");
                            await Bot.SendTextMessageAsync(70832084, "From : @" + uname + " - Name : " + message.Chat.FirstName.ToString() + " " + message.Chat.LastName.ToString() + " \n Message : " + message.Text.ToString());

                        }
                        else
                        {
                            HLP.AddRecord(NotInCommandTable, "command, uid", "'" + UTIL.RemoveKeshidanAndNimSpace(message.Text) + "','" + message.Chat.Id.ToString() + "'");
                            Console.WriteLine("not in command : " + UTIL.RemoveKeshidanAndNimSpace(message.Text));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            //Console.WriteLine("receive Inline Button...");
            try
            {
                try
                {
                    WriteOnConsole(callbackQueryEventArgs.CallbackQuery.Message.Chat, "Inline Button - Selected : " + UTIL.RemoveKeshidanAndNimSpace(callbackQueryEventArgs.CallbackQuery.Message.Text));
                }
                catch (Exception ec) { }

                if (callbackQueryEventArgs.CallbackQuery.Data.Contains("ihave:"))
                {
                    string[] storeID = callbackQueryEventArgs.CallbackQuery.Data.Split(':');

                    var keyboardHide = new ReplyKeyboardHide();

                    Mys HLP = new Mys(StrConn);
                    if (HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '" + storeID[1] + "'", "status") == "waiting")
                    {
                        await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id, "Please answer the following questions");
                        await Bot.ForwardMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, ChannelId, Convert.ToInt32(storeID[1]));
                        HLP.UpdateRecord(MarketDetailTable, "maproute = 'getprice', currentstoreid = '" + storeID[1] + "'", "uid = '" + callbackQueryEventArgs.CallbackQuery.From.Id.ToString() + "'");
                        string usaget = "Please write the price of the above product as an example (only the number should be entered and the number you enter is in Tomans)"
                            + " \n Example : 12500"
                            + " \n Example : 523000";
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, usaget, replyMarkup: keyboardHide);
                    }
                    else
                    {
                        string usaget = "This item has been sold"
                            + " \n "
                             + " \n /menu - ⬅️ 🏠 Main Menu";
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, usaget, replyMarkup: keyboardHide);
                    }
                }
                else if (callbackQueryEventArgs.CallbackQuery.Data.Contains("marketDetail:"))
                {
                    try
                    {
                        string[] MarketID = callbackQueryEventArgs.CallbackQuery.Data.Split(':');

                        Mys HLP = new Mys(StrConn);

                        string LastStoreId = HLP.GetLastRecordWithWhere(MarketSentTable, "id", "uid = '" + MarketID[1] + "' and final_buyer_uid = '" + callbackQueryEventArgs.CallbackQuery.From.Id.ToString() + "'", "storeid");

                        string address = "";
                        string paymentMethod = "";
                        string marketName = "";
                        string sellerName = "";
                        string delivery = "";
                        string deliveryCondition = "";
                        string replacement = "";
                        string replacementDescription = "";
                        string haveReturn = "";
                        string haveReturnDescription = "";
                        string conditionSell = "";
                        string tell = "";
                        string tid = "";
                        using (MySqlDataReader DR = HLP.Sql_Query_DataReader(String.Format("select * from {0} where uid = {1}", MarketDetailTable, MarketID[1])))
                        {
                            while (DR.Read())
                            {
                                address = DR["market_address"].ToString();
                                paymentMethod = DR["payment_method_id"].ToString();
                                marketName = DR["market_name"].ToString();
                                sellerName = DR["seller_name"].ToString();
                                delivery = DR["is_ok_delivery"].ToString();
                                deliveryCondition = DR["delivery_description"].ToString();
                                replacement = DR["is_ok_replacement"].ToString();
                                replacementDescription = DR["replacement_description"].ToString();
                                haveReturn = DR["is_ok_return"].ToString();
                                haveReturnDescription = DR["return_description"].ToString();
                                conditionSell = DR["is_ok_conditional"].ToString();
                                tell = DR["market_phone"].ToString() + " - " + DR["seller_mobile"].ToString();
                                tid = DR["seller_tid"].ToString();
                            }
                        }
                        if (!string.IsNullOrEmpty(paymentMethod))
                            if (paymentMethod.Contains(","))
                            {
                                string[] paymentMethods = paymentMethod.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                paymentMethod = "";
                                for (int i = 0; i < paymentMethods.Length; i++)
                                {
                                    paymentMethod = paymentMethod + HLP.GetLastRecordWithWhere(MarketPayMentTable, "id", "id = " + paymentMethods[i], "payment_method") + ", ";
                                }
                                if (paymentMethod.Length >= 4)
                                    paymentMethod = paymentMethod.Remove(paymentMethod.Length - 2);
                            }
                            else
                            {
                                paymentMethod = HLP.GetLastRecordWithWhere(MarketPayMentTable, "id", "id = " + paymentMethod, "payment_method");
                            }
                        else
                            paymentMethod = "Not specified";
                        if (delivery == "1")
                            delivery = "Has it";
                        else
                            delivery = "Has not it";

                        if (replacement == "1")
                            replacement = "Has it";
                        else
                            replacement = "Has not it";

                        if (haveReturn == "1")
                            haveReturn = "Has it";
                        else
                            haveReturn = "Has not it";

                        if (conditionSell == "1")
                            conditionSell = "Has it";
                        else
                            conditionSell = "Has not it";

                        if (string.IsNullOrWhiteSpace(replacementDescription))
                            replacementDescription = "Has not it";
                        if (string.IsNullOrWhiteSpace(haveReturnDescription))
                            haveReturnDescription = "Has it";

                        var CompleteDescription = @"Product seller profile 👆👆👆"
                                          + " \n Address: " + address
                                          + " \n Payment: " + paymentMethod
                                          + " \n Store Name: " + marketName
                                          + " \n Owner Name: " + sellerName
                                          + " \n Delivery: " + delivery
                                          + " \n Delivery Condition: " + deliveryCondition
                                          + " \n Phone Number: " + tell
                                          + " \n 🆔: @" + tid
                                          + " \n @kidarebot"
                                          + " \n If your purchase is done and you do not want the price and description to be sent to you from other sellers, click the I bought it option."
                                          + " \n "
                                          + " \n /finish - ⬅️ Do not send anymore, my purchase was done";
                        var keyboardHide = new ReplyKeyboardHide();
                        await Bot.ForwardMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, ChannelId, Convert.ToInt32(LastStoreId));
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, CompleteDescription, replyMarkup: keyboardHide);
                    }
                    catch (Exception exdetails) { }
                }
                else if (callbackQueryEventArgs.CallbackQuery.Data.Contains("notapprove:"))
                {
                    Mys HLP = new Mys(StrConn);

                    string[] StoreID = callbackQueryEventArgs.CallbackQuery.Data.Split(':');
                    if (HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '" + StoreID[1] + "'", "approved") == "0")
                    {
                        HLP.UpdateRecord(BuyerProductTable, "approved = 3", "storeid = '" + StoreID[1] + "'");
                        string uid = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '" + StoreID[1] + "'", "uid");
                        string msgtxt = @"Sorry, your request has not been approved"
                                        + " \n The following reasons may cause disapproval: "
                                        + " \n 1 - Unrelated photos and descriptions"
                                        + " \n 2 - Photos or descriptions outside the laws of the country"
                                        + " \n 3 - Content and photos outside the purchase request"
                                        + " \n Please go to the main menu and send another request"
                                        + " \n "
                                        + " /menu - ⬅️ Main menu and other purchase requests";
                        await Bot.SendTextMessageAsync(uid, msgtxt);
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, "Already approved or not approved");
                    }
                }
                else if (callbackQueryEventArgs.CallbackQuery.Data.Contains("approve:"))
                {
                    Mys HLP = new Mys(StrConn);

                    string[] StoreID = callbackQueryEventArgs.CallbackQuery.Data.Split(':');
                    if (HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '" + StoreID[1] + "'", "approved") == "0")
                    {
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, "Approved");
                        HLP.UpdateRecord(BuyerProductTable, "approved = 1", "storeid = '" + StoreID[1] + "'");
                        string catName = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '" + StoreID[1] + "'", "category");
                        string catID = HLP.GetLastRecordWithWhere(CategoryListTable, "id", "name like '%" + catName + "%'", "id");
                        string tags = "";
                        try
                        {
                            tags = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '" + StoreID[1] + "'", "tags");
                        }
                        catch (Exception exxx) { }
                        string strsql = "";

                        if (string.IsNullOrWhiteSpace(tags))
                        {
                            strsql = String.Format("select * from {0} where guild_id = '{1}' and is_active = 1 and blocked = 1 or ( uid = '138657205' or uid = '70832084' or uid = '177786216' or uid = '204582018' ) ", MarketDetailTable, catID);
                        }
                        else
                        {
                            strsql = String.Format("select * from {0} where tags like '%{1}%' and is_active = 1 and blocked = 1 or ( uid = '138657205' or uid = '70832084' or uid = '177786216' or uid = '204582018' )", MarketDetailTable, tags);
                        }
                        using (MySqlDataReader DR = HLP.Sql_Query_DataReader(strsql))
                        {
                            System.Threading.Thread.Sleep(350);
                            while (DR.Read())
                            {
                                string uid = DR["uid"].ToString();
                                try
                                {
                                    await Bot.ForwardMessageAsync(uid, ChannelId, Convert.ToInt32(StoreID[1]));
                                    var ReplyKey = new InlineKeyboardMarkup(new[]
                                        {
                                            new[]
                                            {
                                                new InlineKeyboardButton("I have","ihave:"+Convert.ToInt32(StoreID[1]).ToString())
                                            }
                                    });
                                    await Bot.SendTextMessageAsync(uid, "Have you like this?", replyMarkup: ReplyKey);
                                }
                                catch (ApiRequestException ax)
                                {
                                    HLP.UpdateRecord(MarketDetailTable, "blocked = 0", "uid = '" + uid + "'");
                                    Console.WriteLine(ax.Message + " ||| uid: " + uid );
                                }
                            }
                        }
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(200);
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, "Already approved or not approved");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine(receiveErrorEventArgs.ApiRequestException.Message);
            //Debugger.Break();
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received choosen inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            //ShowProductAsync()
            //inlineQueryEventArgs.InlineQuery.
            //InlineQueryResult[] results = {
            //    new InlineQueryResultDocument
            //    {
            //        Id = "1",
            //        Latitude = 40.7058316f, // displayed result
            //        Longitude = -74.2581888f,
            //        Title = "New York",
            //        InputMessageContent = new InputTextMessageContent // message if result is selected
            //        {
            //            Latitude = 40.7058316f,
            //            Longitude = -74.2581888f,
            //        }
            //    },

            //    new InlineQueryResultLocation
            //    {
            //        Id = "2",
            //        Longitude = 52.507629f, // displayed result
            //        Latitude = 13.1449577f,
            //        Title = "Berlin",
            //        InputMessageContent = new InputLocationMessageContent // message if result is selected
            //        {
            //            Longitude = 52.507629f,
            //            Latitude = 13.1449577f
            //        }
            //    }
            //};

            //await Bot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, results, isPersonal: true, cacheTime: 0);
        }

        private static async void BotOnUpdate(object sender, UpdateEventArgs EventArgs)
        {

            //var update = Bot.GetUpdatesAsync();
            try
            {
                
                //string s = EventArgs.Update.EditedMessage.From.Id.ToString();
                var ch = Bot.GetChatAsync(ChannelId);
                
            } catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            var me = Bot.GetMeAsync().Result;

            this.Text = me.Username;
            TimeSpan ts = new TimeSpan(0, 45, 30);
            Bot.UploadTimeout = ts;
            Bot.StartReceiving();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
            System.Environment.Exit(1);
        }

        private void ConsolTxtBox_TextChanged(object sender, EventArgs e)
        {
            ConsolTxtBox.SelectionStart = ConsolTxtBox.Text.Length;
            ConsolTxtBox.ScrollToCaret();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Mys HLP = new Mys(StrConn);
            Mys HLP2 = new Mys(StrConn);
            try
            {
                string uidNew = Convert.ToInt64(HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "seller_mobile = '" + textBox2.Text + "'", "uid")).ToString();
                await Bot.SendTextMessageAsync(Convert.ToInt64(uidNew), textBox1.Text);
                HLP.UpdateRecord(MarketDetailTable, "firstmessage = 'sent'", "uid = '" + uidNew + "'");
                Console.WriteLine("Sent to : " + uidNew);
            }
            catch (ApiRequestException ax)
            {
                HLP2.UpdateRecord(MarketDetailTable, "blocked = 0", "uid = '" + Convert.ToInt64(HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "seller_mobile = '" + textBox2.Text + "'", "uid")) + "'");
                Console.WriteLine("Blocked Or Deleted -  mobile number : " + textBox2.Text);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var keyboardMenu = new ReplyKeyboardMarkup(new[]
                                {
                                    new []
                                    {
                                        new KeyboardButton("Main Menu 🏠"),
                                    }
                                });

            Mys HLP = new Mys(StrConn);
            Mys HLP2 = new Mys(StrConn);
            string strsql = "";

            if (comboBox1.SelectedIndex == 0)
                strsql = String.Format("select * from {0} where is_active = 1 and firstmessage = 'not sent' and blocked = 1", MarketDetailTable);
            else if (comboBox1.SelectedIndex == 1)
                strsql = String.Format("select * from {0} where is_active = 1 and blocked = 1", MarketDetailTable);
            else if (comboBox1.SelectedIndex == 2)
                strsql = String.Format("select * from {0} where {1}", MarketDetailTable, textBox3.Text);
            Console.WriteLine("ComboBox Index : " + comboBox1.SelectedIndex.ToString());//is_active = 1 and firstmessage = 'not sent' and blocked = 1
            if (string.IsNullOrWhiteSpace(strsql))
            {
                MessageBox.Show("Error: Please select a group of people to post");
                return;
            }
            List<string> uidList = new List<string>();
            try
            {
                using (MySqlDataReader DR = HLP.Sql_Query_DataReader(strsql))
                {
                    while (DR.Read())
                    {
                        uidList.Add(DR["uid"].ToString());
                    }
                }
            }
            catch (Exception ax) { }

            for (int i = 0; i < uidList.Count - 1; i++)
            {
                try
                {
                    await Bot.SendTextMessageAsync(uidList[i], textBox1.Text, replyMarkup: keyboardMenu);
                    System.Threading.Thread.Sleep(50);
                    if (comboBox1.SelectedIndex == 0)
                        HLP.UpdateRecord(MarketDetailTable, "firstmessage = 'sent'", "uid = '" + uidList[i] + "'");
                    Console.WriteLine("Sent to : " + uidList[i]);
                }
                catch (Exception ax)
                {
                    try
                    {
                        HLP.UpdateRecord(MarketDetailTable, "blocked = 0", "uid = '" + uidList[i] + "'");
                        Console.WriteLine("Blocked or Deleted ||| uid : " + uidList[i]);
                    }
                    catch (Exception sax) { }
                }
            }
            //            DateTime DT = DateTime.Now.ToUniversalTime();
            //            DT = DT.AddMinutes(210);
            //            if (DT.Hour > 8)
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            Mys HLPn = new Mys(StrConn);
            List<string> uidList = new List<string>();
            using (MySqlDataReader DR = HLPn.Sql_Query_DataReader(String.Format("select uid from {0} where is_active = 0", MarketDetailTable)))
            {
                while (DR.Read())
                {
                    uidList.Add(DR[0].ToString());
                }
            }

            for (int i = 0; i < uidList.Count - 1; i++)
            {
                try
                {
                    string finalIdCoded = "";
                    string s = uidList[i].ToString();
                    foreach (char c in s)
                    {
                        finalIdCoded += ((int)c + 63).ToString();
                    }
                    string Khorooji = "You have registered as a seller (shopkeeper or online seller) in the 'LiViCi' system, but due to the lack of your information, you have not been activated. Please enter and save your information by clicking on the link below so that you can enter the system. Activate. \n http://kidarebot.com/usermanager/update.php?name=" + finalIdCoded;
                    Console.WriteLine(Khorooji);
                    await Bot.SendTextMessageAsync(uidList[i].ToString(), Khorooji);
                }
                catch (Exception ax)
                {
                    try
                    {
                        HLPn.UpdateRecord(MarketDetailTable, "blocked = 0", "uid = '" + uidList[i] + "'");
                        Console.WriteLine("Blocked or Deleted ||| uid : " + uidList[i]);
                    }
                    catch (Exception sax) { }
                }
            }
        }

    }

    public class Result
    {
        public string file_id { get; set; }
        public string file_size { get; set; }
        public string file_path { get; set; }
    }

    public class RootObject
    {
        public bool ok { get; set; }
        public Result result { get; set; }
    }
}
