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

namespace KidareBot
{
    public partial class KidareDashboardFrm : Form
    {
        public KidareDashboardFrm()
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
        private static readonly string Kharidaram = "خریدارم";
        private static readonly string Forooshandeam = "مغازه دارم";

        private static readonly string CREATE_USER_PROMPT = "\u2705 تبریک ، شماره ی تلفن شما با موفقیت ثبت شد.";
        private static readonly string NOT_MEMBER_PROMPT = "شما در سیستم عضو نیستید ، درصورتی که مایل به عضویت در سیستم کی داره هستید " + "لطفا روی دکمه ی ارسال شماره تماس کلیک کنید تا در اسرع وقت با شما تماس بگیریم👇👇👇";

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
                if (message.Text != "منوی قبلی 🔙" && message.Text != "منوی قبلی" && message.Text != "جزوات بعدی" && message.Text != "جزوات قبلی")
                {
                    if (!message.Text.Contains(" || "))
                        HLP.UpdateRecord(UCTable, "usercommand = '" + message.Text.Replace(" 🏠", "") + "',maproute = CONCAT(maproute, '" + message.Text.Replace(" 🏠", "") + "-')", "uid = '" + message.Chat.Id.ToString() + "'");
                    dastoor[indx][1] = UTIL.RemoveKeshidanAndNimSpace(message.Text);
                    dastoor[indx][2] = "nothing";
                    if (!message.Text.Contains(" || "))
                        dastoor[indx][3] = dastoor[indx][3] + UTIL.RemoveKeshidanAndNimSpace(message.Text) + "-";//HLP.GetLastRecordWithWhere(UCTable,"id","idf = '"+ message.Chat.Id.ToString()+ "'","maproute");
                }
                if (message.Text == "جزوات بعدی")
                {
                    dastoor[indx][2] = "nothing";
                    int res = (Convert.ToInt32(dastoor[indx][2]) - Convert.ToInt32(dastoor[indx][4]));
                    if (res <= 0)
                        HLP.UpdateRecord(UCTable, "offset = 1", "idf = '" + message.Chat.Id.ToString() + "'");
                    else
                        HLP.UpdateRecord(UCTable, "offset = offset - " + dastoor[indx][4], "idf = '" + message.Chat.Id.ToString() + "'");

                    dastoor[indx][2] = HLP.GetLastRecordWithWhere(UCTable, "id", "idf = '" + message.Chat.Id.ToString() + "'", "offset");
                }
                else if (message.Text == "جزوات قبلی")
                {
                    HLP.UpdateRecord(UCTable, "offset = offset + " + dastoor[indx][4], "idf = '" + message.Chat.Id.ToString() + "' and offset >= 1");
                    dastoor[indx][2] = HLP.GetLastRecordWithWhere(UCTable, "id", "idf = '" + message.Chat.Id.ToString() + "'", "offset");

                    if (Convert.ToInt32(dastoor[indx][2]) <= 0)
                    {
                        dastoor[indx][2] = "1";
                        HLP.UpdateRecord(UCTable, "offset = 1", "idf = '" + message.Chat.Id.ToString() + "'");
                    }
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
                if (menuname == "منوی اولیه" && msg.Text.StartsWith("/start"))
                {
                    usage = "سلام. به سیستم 'کی داره' خوش اومدین."
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
                                        new KeyboardButton("مغازه دارم") { RequestContact = true },
                                        new KeyboardButton("خریدارم"),
                                    },
                                    new []
                                    {
                                        new KeyboardButton("ارتباط با ما"),
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
                                        new KeyboardButton("مغازه دارم") { RequestContact = true },
                                        new KeyboardButton("خریدارم"),
                                    },
                                    new []
                                    {
                                        new KeyboardButton("ارتباط با ما"),
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

                var usage = "کالای مورد نظر شما در کدام یک از گروه های زیر است؟ \n (اگر گروه مورد نظر در گزینه های زیر نیست، گزینه آخری که بر اساس اسم کالا می باشد را بزنید) ";

                Task<string[][]> arrayKeyTemp = GetListAsync(SelectQuery1, CategoryListTable, "name", "* ");
                string[][] arrayKeyTemp2 = await arrayKeyTemp;
                string[][] arrayKey = new string[arrayKeyTemp2.Length + 1][];
                for (int i = 0; i < arrayKeyTemp2.Length; i++)
                {
                    arrayKey[i] = arrayKeyTemp2[i];
                }
                arrayKey[arrayKeyTemp2.Length] = new string[] { "منوی اولیه 🏠" };

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
                                        new KeyboardButton("ارسال شماره تماس") { RequestContact = true },
                                    },
                                    new []
                                    {
                                        new KeyboardButton("منوی اولیه 🏠"),
                                    }
                                });
                var keyboardMenu = new ReplyKeyboardMarkup(new[]
                        {
                                    new []
                                    {
                                        new KeyboardButton("منوی اولیه 🏠"),
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

                    string deActiveForooshandeMSG = "⛔️درصورتی که توسط بازاریاب های ما اطلاعات فروشگاه شما دریافت و ثبت نشده است، لطفا روی لینک زیر کلیک کرده و اطلاعات خودتان را وارد کنید. \n ✳️ نکته :  برای فعال شدن فوری در سامانه از طریق لینک زیر می توانید اطلاعات اولیه خود را وارد نمایید. \n http://kidarebot.com/usermanager/update.php?name=" + finalIdCoded;

                    if (IsActive == "True")
                    {
                        string guildID = HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "guild_id");
                        senf = HLP.GetLastRecordWithWhere(CategoryListTable, "id", "id = '" + guildID + "'", "name");
                        if (senf == "بر اساس اسم کالا 📦")
                        {
                            senf = HLP.GetLastRecordWithWhere(MarketDetailTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "tags");
                        }
                        string activeForooshandeMSG = "شما در سیستم به عنوان مغازه دار تایید شده و فعال، در صنف " + senf + " عضویت دارید. "
                                  + " هر زمان از طرف خریدار درخواست خریدی گرفته شود، برای شما ارسال می نماییم."
                                  + " \n "
                                  + " برای ویرایش و یا اصلاح اطلاعات خود به لینک زیر رفته و آن را اصلاح نمایید و یا در قسمت ارتباط با ما (در منوی اصلی) درخواست اصلاح بنویسید : \n http://kidarebot.com/usermanager/update.php?name=" + finalIdCoded + " \n";
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

                        var usaget = @"اگر توضیحی دارید در مورد کالای  خواسته شده، بنویسید و ارسال کنید "
                                    + " \n "
                                    + " \n /nodescription - ⬅️ توضیحی ندارم \n"
                                    + " \n /menu - ⬅️ 🏠 کنسل و رفتن به منوی اولیه";
                        await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                            replyMarkup: keyboardHide);
                    }
                    else
                    {
                        string usaget = "لطفا قیمت را فقط  به صورت عدد وارد نمایید که به صورت تومان تلقی می گردد، مثلا اگر قیمت یک کالا چهارده هزار و پانصد تومان است، وارد نمایید : 14500";
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
                        description = "توضیحی ندارم";
                    HLP.UpdateRecord(MarketDetailTable, "currentdescription = '" + description + "', maproute = 'getimage'", "uid = '" + msg.Chat.Id.ToString() + "'");

                    var usaget = @"اگر عکسی از کالا ی مورد نظر و یا کالای مشابه دارید دکمه 📎 را بزنید و عکس را ارسال نمایید "
                                + " \n 💢 توجه ، توجه 💢 لطفا فقط یک عکس ارسال نمایید"
                                + " \n "
                                + " \n /nopic - ⬅️ عکسی از کالا ندارم \n"
                                + " \n /menu - ⬅️ 🏠 کنسل و رفتن به منوی اولیه";
                    await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                        replyMarkup: keyboardHide);
                }
                else if (FMapRoute == "getimage")
                {
                    var usaget = @"اطلاعات شما برای مشتری فرستاده شد. اگر مشتری موافق باشد با شما از طریق تلگرام یا تلفنی و یا حضوری درخواست خرید می نماید.";
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
                            pic = "عکسی از کالا ندارم";

                        HLP.UpdateRecord(MarketDetailTable, "currentpic = '" + pic + "', maproute = ''", "uid = '" + msg.Chat.Id.ToString() + "'");
                        string allP = "";
                        if (!string.IsNullOrWhiteSpace(description))
                            allP = " \n توضیحات: " + description;
                        if (!string.IsNullOrWhiteSpace(address))
                            allP += " \n آدرس: " + address + "...";
                        if (!string.IsNullOrWhiteSpace(phoneNumber))
                            allP += " \n 📱: " + phoneNumber;
                        if (!string.IsNullOrWhiteSpace(phoneNumberS))
                            allP += " \n ☎️: " + phoneNumberS;
                        if (!string.IsNullOrWhiteSpace(userName))
                            allP += " \n 🆔: @" + userName;
                        var PhotoCaption = @"قیمت: " + UTIL.PriceSpliter(price) + " تومان"
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

                            string txtFinal = "قیمت ، توضیحات ، عکس و مشخصات تماس شما برای مشتری ارسال شد. اگر مشتری با قیمت و سایر مشخصات موافق باشد با شما در تلگرام یا به صورت تماس تلفنی و یا حضوری ارتباط برقرار می نماید."
                                              + " \n "
                                              + " \n /menu - ⬅️ 🏠 منوی اولیه";
                            await Bot.SendTextMessageAsync(msg.Chat.Id, txtFinal,
                                replyMarkup: keyboardHide);

                            HLP.UpdateRecord(MarketDetailTable, "currentstoreid = '', maproute = '', currentpic = '', currentdescription = '', currentprice = ''", "uid = '" + msg.Chat.Id.ToString() + "'");
                            var ReplyKey = new InlineKeyboardMarkup(new[]
                                        {
                                        new[]
                                        {
                                            new InlineKeyboardButton("مشخصات کامل","marketDetail:"+msg.Chat.Id.ToString())
                                        }
                                    });
                            await Bot.SendTextMessageAsync(curentStoreID, "مشخصات کامل 👆👆👆 فروشنده",
                                replyMarkup: ReplyKey);
                            string finishMSG = "اگر خرید شما انجام شده و نمی خواهید قیمت و توضیحات از فروشندگان دیگر برای شما ارسال شود، گزینه خریدم انجام شد را بزنید. "
                                                + " \n "
                                                + " \n /finish - ⬅️ خریدم انجام شد ، دیگه نفرست";
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
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "سیستم در حال دریافت عکس ارسالی از طرف شماست، لطفا صبر کنید...",
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
                                    allP = " \n توضیحات: " + description;
                                if (!string.IsNullOrWhiteSpace(address))
                                    allP += " \n آدرس: " + address + "...";
                                if (!string.IsNullOrWhiteSpace(phoneNumber))
                                    allP += " \n 📱: " + phoneNumber;
                                if (!string.IsNullOrWhiteSpace(phoneNumberS))
                                    allP += " \n ☎️: " + phoneNumberS;
                                if (!string.IsNullOrWhiteSpace(userName))
                                    allP += " \n 🆔: @" + userName;
                                var PhotoCaption = @"قیمت: " + UTIL.PriceSpliter(price) + " تومان"
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

                                string txtFinal = "قیمت ، توضیحات ، عکس و مشخصات تماس شما برای مشتری ارسال شد. اگر مشتری با قیمت و سایر مشخصات موافق باشد با شما در تلگرام یا به صورت تماس تلفنی و یا حضوری ارتباط برقرار می نماید."
                                                  + " \n "
                                                  + " \n /menu - ⬅️ 🏠 منوی اولیه";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, txtFinal,
                                    replyMarkup: keyboardHide);

                                HLP.UpdateRecord(MarketDetailTable, "currentstoreid = '', maproute = '', currentpic = '', currentdescription = '', currentprice = ''", "uid = '" + msg.Chat.Id.ToString() + "'");
                                var ReplyKey = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            new InlineKeyboardButton("مشخصات کامل","marketDetail:"+msg.Chat.Id.ToString())
                                        }
                                    });
                                await Bot.SendTextMessageAsync(curentStoreID, "مشخصات کامل فروشنده محصول بالا 👇",
                                    replyMarkup: ReplyKey);
                                string finishMSG = "اگر خرید شما انجام شده و نمی خواهید قیمت و توضیحات از فروشندگان دیگر برای شما ارسال شود، گزینه خریدم انجام شد را بزنید."
                                    + " \n "
                                    + " \n /finish - ⬅️ دیگه نفرست، خریدم انجام شد";
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
            //await Bot.SendTextMessageAsync(msg.Chat.Id, "به علت درخواست زیاد، موقتا قسمت 'مغازه دارم' فقط فعال می باشد. \n /menu");
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
                                if (FinalMsgW == "بر اساس اسم کالا")
                                {
                                    var usaget = @"لطفا بنویسید از چه نوع مغازه ای کالای خود را می خواهید (مغازه چی چی فروشی)؟ یا چه مدل جنسی میخواهید؟ "
                                             + " \n مثال : عطاری"
                                             + " \n یا ، مثال : بلبرینگ"
                                             + " \n "
                                             + " \n /menu - ⬅️ 🏠 کنسل و رفتن به منوی اولیه";
                                    HLP.UpdateRecord(BuyerTable, "nameandfamily = '" + (msg.Chat.FirstName + " " + msg.Chat.LastName).Trim() + "', maproute = 'getmodeltags'", "uid = '" + msg.Chat.Id.ToString() + "'");
                                    await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                                        replyMarkup: keyboardHide);
                                }
                                else
                                {
                                    var usaget = @"لطفا نام و یا توضیحات کالای مورد نظرتان را نوشته و ارسال نمایید "
                                             + " \n "
                                             + " \n /noname - ⬅️ نام یا توضیحی ندارم \n "
                                             + " \n /menu - ⬅️ 🏠 کنسل و رفتن به منوی اولیه";
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
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "خطا!!! لطفا فقط یک متن به عنوان نام و یا توضیحات بنویسید و ارسال کنید.",
    replyMarkup: keyboardHide);
                        return;
                    }
                    string namedescription = UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim();
                    if (namedescription == "/noname")
                        namedescription = "نام و توضیحی ندارم";
                    HLP.UpdateRecord(BuyerProductTable, "nameanddescription = '" + namedescription + "'", "uid = '" + msg.Chat.Id.ToString() + "' and id = " + LastProductID);

                    var usaget = @"اگر عکس کالای مورد نظر و یا مشابه آن را دارید دکمه 📎 را بزنید سپس عکس مورد نظر را گرفته و یا از گالری خود، انتخاب نموده و در نهایت ارسال نمایید."
                                     + " \n 💢 توجه ، توجه 💢 لطفا فقط یک عکس ارسال نمایید"
                                     + " \n "
                                     + " \n /nopic - ⬅️ عکسی از کالا ندارم \n "
                                     + " \n /menu - ⬅️ 🏠 کنسل و رفتن به منوی اولیه";
                    HLP.UpdateRecord(BuyerTable, "maproute = 'getimage'", "uid = '" + msg.Chat.Id.ToString() + "'");
                    await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                        replyMarkup: keyboardHide);
                }
                else if (KMapRoute == "getmodeltags")
                {
                    if (msg.Type != MessageType.TextMessage)
                    {
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "خطا!!! لطفا فقط یک متن در جواب سئوال قبلی بنویسید و ارسال کنید.",
    replyMarkup: keyboardHide);
                        return;
                    }
                    string namedescription = UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim();
                    if (namedescription.Length < 4)
                    {
                        var usaget = @"خطا : باید یک کلمه بیش از 5 حرفی در مورد مغازه و یا جنسی که می خواهید بنویسید. "
                                 + " \n "
                                 + " \n /menu - ⬅️ 🏠 کنسل و رفتن به منوی اولیه";
                        HLP.UpdateRecord(BuyerTable, "maproute = 'getmodeltags'", "uid = '" + msg.Chat.Id.ToString() + "'");
                        await Bot.SendTextMessageAsync(msg.Chat.Id, usaget,
                            replyMarkup: keyboardHide);
                    }
                    else
                    {
                        if (HLP.CountOfRecordsWhere(MarketDetailTable, "tags like '%" + namedescription + "%' and is_active = 1 and blocked = 1") <= 0)
                        {
                            var usagett = @"متاسفانه فروشنده ای برای این مورد در سامانه نمی باشد، لطفا با کلمه دیگری دوباره سعی کنید. "
                                         + " \n "
                                         + " \n /menu - ⬅️ 🏠 کنسل و رفتن به منوی اولیه";
                            HLP.UpdateRecord(BuyerTable, "maproute = 'getmodeltags'", "uid = '" + msg.Chat.Id.ToString() + "'");
                            await Bot.SendTextMessageAsync(msg.Chat.Id, usagett,
                                replyMarkup: keyboardHide);
                        }
                        else
                        {
                            HLP.UpdateRecord(BuyerProductTable, "tags = '" + namedescription + "'", "uid = '" + msg.Chat.Id.ToString() + "' and id = " + LastProductID);

                            var usaget = @"لطفا توضیحات کالای مورد نظرتان را نوشته و ارسال نمایید "
                                     + " \n "
                                     + " \n /noname - ⬅️ نام یا توضیحی ندارم \n "
                                     + " \n /menu - ⬅️ 🏠 کنسل و رفتن به منوی اولیه";
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
                        if (HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + msg.Chat.Id.ToString() + "'", "nameanddescription") == "نام و توضیحی ندارم")
                        {
                            HLP.UpdateRecord(BuyerTable, "maproute = 'getnameanddescription'", "uid = '" + msg.Chat.Id.ToString() + "'");
                            var usagett = @"⛔️⛔️⛔️ هیچ توضیح، نام و یا عکسی وارد نکرده اید، لطفا دوباره اطلاعات درست را وارد نمایید."
                                     + " \n "
                                     + "لطفا نام و یا توضیحات کالای مورد نظرتان را بنویسید و ارسال نمایید "
                                     + " \n "
                                     + " \n /noname - ⬅️ نام یا توضیحی ندارم \n "
                                     + " \n /menu - ⬅️ 🏠 کنسل و رفتن به منوی اولیه";
                            await Bot.SendTextMessageAsync(msg.Chat.Id, usagett,
                            replyMarkup: keyboardHide);
                            return;

                        }
                        string pic = UTIL.RemoveKeshidanAndNimSpace(msg.Text).Trim();
                        if (pic == "/nopic")
                            pic = "عکسی از کالا ندارم";
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
                            PhotoCaption = "محصول بالا 👆👆👆";
                        var usaget = @"مشخصات " + PhotoCaption + " برای فروشندگانی که این محصول یا کالا را دارند ارسال شد."
                                     + " \n بعد از مدت زمان کوتاهی از طرف فروشندگان، قیمت و سایر مشخصات این محصول یا کالا ، برای شما در همین جا ارسال خواهد شد."
                                     + " \n هر زمان که از فروشندگان جوابی آمد ، ما شما را در اینجا خبر می نماییم"
                                     + " \n "
                                     + " \n /menu - ⬅️ 🏠 منوی اولیه";
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
                                            new InlineKeyboardButton("تایید - " + "دسته:" + cat + " N:" + name + " UN:" + uname , "approve:"+channelRetMsg.MessageId.ToString())
                                        },
                                        new[]
                                        {
                                            new InlineKeyboardButton("لغو - " + "دسته:" + cat + " N:" + name + " UN:" + uname , "notapprove:"+channelRetMsg.MessageId.ToString())
                                        }
                                    });
                                    await Bot.SendTextMessageAsync(uid, "کالای بالا 👆👆👆 را تایید می کنید؟", replyMarkup: ReplyKey);
                                } catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message + " ||| uid: " + uid);
                                }
                            }
                        }
                    }
                    else if (msg.Type == MessageType.PhotoMessage)
                    {
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "سیستم در حال دریافت عکس ارسالی از طرف شماست، لطفا صبر کنید...",
                            replyMarkup: keyboardHide);
                        HLP.UpdateRecord(BuyerProductTable, "pic = 'دارد'", "uid = '" + msg.Chat.Id.ToString() + "' and id = " + LastProductID);

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
                                PhotoCaption = "محصول بالا 👆👆👆";
                            var usaget = @"مشخصات " + PhotoCaption + " برای فروشندگانی که این محصول را دارند ارسال می شود."
                                         + " \n یک مدت کوتاه زمان میبرد که از طرف فروشندگان برای شما قیمت و توضیحات ارسال شود. هر زمان ارسال شد، به شما در همین جا اطلاع می دهیم"
                                         + " \n "
                                         + " \n /menu - ⬅️ 🏠 منوی اولیه";
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
                                                new InlineKeyboardButton("تایید - " + "دسته:" + cat + " N:" + name + " UN:" + uname , "approve:"+channelRetMsg.MessageId.ToString())
                                            },
                                            new[]
                                            {
                                                new InlineKeyboardButton("لغو - " + "دسته:" + cat + " N:" + name + " UN:" + uname , "notapprove:"+channelRetMsg.MessageId.ToString())
                                            }
                                        });
                                        await Bot.SendTextMessageAsync(uid, "کالای بالا 👆👆👆 را تایید می کنید؟", replyMarkup: ReplyKey);
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
                                        new KeyboardButton("منوی اولیه 🏠"),
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
                        await ShowMenu(message, "منوی اولیه");
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
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "این شماره از قبل موجود می باشد : " + DR[0].ToString());
                                    HaveTell = true;
                                }
                            }
                            if (!HaveTell)
                                await Bot.SendTextMessageAsync(message.Chat.Id, "وجود ندارد");
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
                                await Bot.SendTextMessageAsync(message.Chat.Id,"اطلاعات این کاربر وارد شده است");
                            else
                                await Bot.SendTextMessageAsync(message.Chat.Id, "اطلاعات کامل نیست");
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
                    string deActiveForooshandeMSG = CREATE_USER_PROMPT + " \n ⛔️درصورتی که توسط بازاریاب های ما اطلاعات فروشگاه شما دریافت و ثبت نشده است، لطفا روی لینک زیر کلیک کرده و اطلاعات خودتان را وارد کنید. \n ✳️ نکته :  برای فعال شدن فوری در سامانه از طریق لینک زیر می توانید اطلاعات اولیه خود را وارد نمایید. \n http://kidarebot.com/usermanager/update.php?name=" + finalIdCoded;

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
                                await Bot.SendTextMessageAsync(uid, "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); 
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " ||| uid: " + uid);
                            }
                        }
                    }
                    //await Bot.SendTextMessageAsync("70832084", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //Mahdi Alizade
                    //await Bot.SendTextMessageAsync("350995247", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //Sepide Amoodi
                    //await Bot.SendTextMessageAsync("130949826", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //nima darshib niya
                    //await Bot.SendTextMessageAsync("316721834", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //shiva
                    //await Bot.SendTextMessageAsync("424919314", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //farnaz
                    //await Bot.SendTextMessageAsync("232257747", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //hadi
                    //await Bot.SendTextMessageAsync(53025970)
                    HomeMenuBtn(message.Chat.Id.ToString());
                    return;
                }
                else if (message.Text == "ارسال شماره تماس")
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
                                await Bot.SendTextMessageAsync(uid, "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " ||| uid: " + uid);
                            }
                        }
                    }
                    //await Bot.SendTextMessageAsync("70832084", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //Mahdi Alizade
                    //await Bot.SendTextMessageAsync("350995247", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //Sepide Amoodi
                    //await Bot.SendTextMessageAsync("130949826", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //nima darshib niya
                    //await Bot.SendTextMessageAsync("316721834", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //shiva
                    //await Bot.SendTextMessageAsync("424919314", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //farnaz
                    //await Bot.SendTextMessageAsync("232257747", "این شخص با این مشخصات ثبت نام کرد : \n" + "نام و نام خانوادگی : " + fname + " " + lname + " \n User Name : " + uname + " \n  شماره موبایل : " + messageEventArgs.Message.Contact.PhoneNumber.ToString()); //hadi
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
                    await Bot.SendTextMessageAsync(message.Chat.Id, "خطا!!! لطفا فقط یک متن در جواب سئوال قبلی بنویسید و ارسال کنید.",
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
                    await Bot.SendTextMessageAsync(message.Chat.Id, "خطا!!! لطفا فقط یک متن در جواب سئوال قبلی بنویسید و ارسال کنید.",
                            replyMarkup: keyboardHide);
                    return;
                }

                if (message.Type == MessageType.TextMessage)
                {
                    
                    Mys HLP = new Mys(StrConn);

                    if (message.Text == "دیگه نفرست، خریدم انجام شد" || message.Text == "/finish")
                    {
                        HomeMenuBtn(message.Chat.Id.ToString());

                        string Naghes_Id = HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "uid = '" + message.Chat.Id.ToString() + "'", "id");
                        HLP.UpdateRecord(BuyerProductTable, "status = ''", "id = '" + Naghes_Id + "'");

                        ucm = CheckAndGetUCommands(message);
                        HLP.UpdateRecord(BuyerTable, "maproute = ''", "uid = '" + message.Chat.Id.ToString() + "'");
                        HLP.DeleteRecord(BuyerProductTable, "uid = '" + message.Chat.Id.ToString() + "' and storeid = ''");
                        await ShowMenu(message, "منوی اولیه");
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
                                    + " \n /menu - ⬅️ 🏠 منوی اولیه";
                        await Bot.SendTextMessageAsync(message.Chat.Id, usaget, replyMarkup: keyboardHide);
                        return;
                    }
                    if (message.Text == "/law")
                    {
                        var keyboardHide = new ReplyKeyboardHide();
                        HomeMenuBtn(message.Chat.Id.ToString());
                        ucm = CheckAndGetUCommands(message);
                        string usaget = "اخطار:خریدار محترم لطفا از انتشار هر گونه عکس و درخواست غیر اخلاقی جدا خودداری فرمائید در غیر این صورت پیگرد قانونی دارد."
                                    + " \n "
                                    + " \n 📛 توجه : 'کی داره' هیچ گونه منفعت و مسئولیتی در قبال معامله شما ندارد، با مطالعه راهنمایی خرید ایمن، آسوده تر معامله کنید:"
                                    + " \n ۱- هرگز پیش از دریافت کالا هزینه پرداخت نکنید."
                                    + " \n ۲- بهترین و مطمئن ترین روش انجام معاملات اینترنتی، خرید حضوری و دیدن کالا از نزدیک است تا از کیفیت کالا و صحت اظهارات فروشنده اطمینان حاصل فرمائید."
                                    + " \n ۳- در صورت امکان،  هنگام خرید کالا رسید کتبی یا فاکتور معتبر از فروشنده دریافت کنید."
                                    + " \n ۴- میخواهید کالاهای مارک دار بخرید بهتر است از فروشگاه مربوطه ملاقات کنید تا از صحت آن چه اعلام شده اطمینان یابید."
                                    + " \n "
                                    + " \n ‼️❌نکته❌‼️ : سامانه 'کی داره' یک سامانه واسط می باشد، که هیچ گونه کمیسیون و یا پورسانتی از معاملات نمی گیرد و فقط فروشندگان یک کالا یا محصول را به خریدار و متقاضی آن مرتبط می کند، لذا مسئولیت خرید و فروش کالا به عهده خریدار و فروشنده می باشد و این سامانه هیچ گونه مسئولیتی در قبال فروشنده و خریدار و معاملات بین آنها ندارد."
                                    + " \n "
                                    + " \n قوانین کامل و توصیه ها را در وب سایت ما مشاهده بفرمایید"
                                    + " \n http://www.kidarebot.com"
                                    + " \n "
                                    + " \n /menu - ⬅️ 🏠 منوی اولیه";
                        await Bot.SendTextMessageAsync(message.Chat.Id, usaget, replyMarkup: keyboardHide);
                        return;
                    }

                    string uc = HLP.GetLastRecordWithWhere(UCTable, "id", "uid = '" + message.Chat.Id.ToString() + "'", "usercommand");

                    if (message.Text == "/start" || message.Text.StartsWith("/start ") || message.Text == "start" || message.Text == "" || message.Text.Contains("اولیه") || message.Text.Contains("menu") || message.Text.Contains("اوليه") || message.Text == "شروع" || message.Text == "اینایی که فرستادمو بخونید. تمام" || message.Text == Kharidaram || message.Text == "ارتباط با ما" || message.Text == Forooshandeam)
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

                    if (message.Text.Contains("منوی اولیه") || message.Text == "اینایی که فرستادمو بخونید. تمام" || message.Text == "menu" || message.Text == "/menu")
                    {
                        ucm = CheckAndGetUCommands(message);
                        HomeMenuBtn(message.Chat.Id.ToString());
                    }
                    if (message.Text.Contains("منوی قبلی"))
                    {
                        ucm = CheckAndGetUCommands(message);
                        BackMenuBtn(message);
                    }
                    if (!string.IsNullOrWhiteSpace(ucm[1]))
                    {
                        if (ucm[1] == @"/start" || ucm[1] == "start" || ucm[1] == "" || ucm[1].Contains("اولیه") || ucm[1] == "شروع" || ucm[1] == "اینایی که فرستادمو بخونید. تمام" || ucm[1] == "/menu" || ucm[1] == "menu")
                            await ShowMenu(message, "منوی اولیه");
                        else if (ucm[1] == Kharidaram)
                        {
                            await ShowMyCategoriesAsync(message);
                        }
                        else if (ucm[1] == Forooshandeam)
                        {
                            await ForooShandeRequestButton(message);
                            return;
                        }
                        else if (ucm[1] == "ارتباط با ما" && message.Text == "ارتباط با ما")
                        {
                            await ShowMenu(message, "ارتباط با ما");
                        }
                    }
                    else
                    {
                        if (LastCommand == "ارتباط با ما")
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
                        await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id, "لطفا به سئوالات زیر پاسخ دهید");
                        await Bot.ForwardMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, ChannelId, Convert.ToInt32(storeID[1]));
                        HLP.UpdateRecord(MarketDetailTable, "maproute = 'getprice', currentstoreid = '" + storeID[1] + "'", "uid = '" + callbackQueryEventArgs.CallbackQuery.From.Id.ToString() + "'");
                        string usaget = "لطفا قیمت محصول بالا 👆👆👆 را همانند مثال بنویسید (فقط عدد وارد شود و عددی که وارد می نمایید به منزله تومان می باشد) "
                            + " \n مثال : 12500"
                            + " \n مثال : 523000";
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, usaget, replyMarkup: keyboardHide);
                    }
                    else
                    {
                        string usaget = "این جنس به فروش رسیده است"
                            + " \n "
                             + " \n /menu - ⬅️ 🏠 منوی اولیه";
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
                            paymentMethod = "مشخص نشده است";
                        if (delivery == "1")
                            delivery = "دارد";
                        else
                            delivery = "ندارد";

                        if (replacement == "1")
                            replacement = "دارد";
                        else
                            replacement = "ندارد";

                        if (haveReturn == "1")
                            haveReturn = "دارد";
                        else
                            haveReturn = "ندارد";

                        if (conditionSell == "1")
                            conditionSell = "دارد";
                        else
                            conditionSell = "ندارد";

                        if (string.IsNullOrWhiteSpace(replacementDescription))
                            replacementDescription = "ندارد";
                        if (string.IsNullOrWhiteSpace(haveReturnDescription))
                            haveReturnDescription = "ندارد";

                        var CompleteDescription = @"مشخصات فروشنده محصول بالا 👆👆👆"
                                          + " \n آدرس: " + address
                                          + " \n نحوه پرداخت: " + paymentMethod
                                          + " \n نام فروشگاه: " + marketName
                                          + " \n نام فروشنده: " + sellerName
                                          + " \n ارسال با پیک: " + delivery
                                          + " \n شرایط پیک: " + deliveryCondition
                                          + " \n تعویضی: " + replacement
                                          + " \n توضیح تعویضی: " + replacementDescription
                                          + " \n برگشتی: " + haveReturn
                                          + " \n توضیح برگشتی: " + haveReturnDescription
                                          + " \n فروش شرایطی: " + conditionSell
                                          + " \n شماره تماس: " + tell
                                          + " \n 🆔: @" + tid
                                          + " \n @kidarebot"
                                          + " \n اگر خرید شما انجام شده و نمی خواهید قیمت و توضیحات از فروشندگان دیگر برای شما ارسال شود، گزینه خریدم انجام شد را بزنید."
                                          + " \n "
                                          + " \n /finish - ⬅️ دیگه نفرست، خریدم انجام شد";
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
                        string msgtxt = @"متاسفانه درخواست شما تایید نشده است"
                                        + " \n دلایل زیر می تواند باعث عدم تایید شده باشد: "
                                        + " \n 1 - عکس و توضیحات نا مرتبط"
                                        + " \n 2 - عکس و یا توضیحات خارج از قوانین کشور"
                                        + " \n 3 - مطالب و عکس خارج از درخواست خرید"
                                        + " \n لطفا به منوی اولیه رفته و درخواست دیگری ارسال نمایید"
                                        + " \n "
                                        + " /menu - ⬅️ منوی اولیه و درخواست خرید دیگر";
                        await Bot.SendTextMessageAsync(uid, msgtxt);
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, "قبلا تایید یا عدم تایید شده است");
                    }
                }
                else if (callbackQueryEventArgs.CallbackQuery.Data.Contains("approve:"))
                {
                    Mys HLP = new Mys(StrConn);

                    string[] StoreID = callbackQueryEventArgs.CallbackQuery.Data.Split(':');
                    if (HLP.GetLastRecordWithWhere(BuyerProductTable, "id", "storeid = '" + StoreID[1] + "'", "approved") == "0")
                    {
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, "تایید شد");
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
                                                new InlineKeyboardButton("من دارم","ihave:"+Convert.ToInt32(StoreID[1]).ToString())
                                            }
                                    });
                                    await Bot.SendTextMessageAsync(uid, "کالای بالا 👆👆👆 را دارید؟", replyMarkup: ReplyKey);
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
                        await Bot.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.From.Id, "قبلا تایید یا عدم تایید شده است");
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
                                        new KeyboardButton("منوی اولیه 🏠"),
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
                MessageBox.Show("خطا : لطفا یک دسته از افراد را برای ارسال انتخاب کنید");
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
                    string Khorooji = "شما در سامانه 'کی داره' به عنوان فروشنده ( مغازه دار و یا فروشنده آنلاین ) ثبت نام نموده ولی به دلیل عدم وجود اطلاعات شما، فعال نشده اید، لطفا با زدن بر روی لینک زیر اطلاعات خود را وارد و ذخیره نمایید تا شما در سامانه فعال شوید. \n http://kidarebot.com/usermanager/update.php?name=" + finalIdCoded;
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
