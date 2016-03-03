using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HarbauerApp.classes
{
    public class Job
    {

        #region Library Functions

        public static void log(String message, Exception excep, string logfile="app")
        {
            try
            {
                System.IO.File.AppendAllText(logfile + ".log", Environment.NewLine + Environment.NewLine + DateTime.Today.ToString() + Environment.NewLine + message + Environment.NewLine + (excep == null ? new Exception("NOEXCEP") : excep));
            } catch(Exception) { }
        }

        public static System.Windows.Media.Imaging.BitmapImage getImageFromMedia(string filename)
        {
            try
            {
                #region MyRegion
                System.Windows.Media.Imaging.BitmapImage image = new System.Windows.Media.Imaging.BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("pack://application:,,,/HarbauerApp;component/media/" + filename);
                image.EndInit();
                return image;
                #endregion
            } catch(Exception ex)
            {
                throw new Exception("Unable to get image from resources. [" + ex.Message + "]");
            }
            return null;
        }

        public static string joinArray<T>(T[] arr, string sep)
        {
            string output = "";
            if (arr.Length == 0) return output;
            foreach(T o in arr)
            {
                output += o.ToString() + sep;
            }
            return output.Substring(0, output.Length - sep.Length);
        }

        #endregion

        #region Security Functions
        public static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        #endregion

        #region Database
        public static class Database
        {

            #region Database Variables

            public const String DATABASE_FILE = "database.db";
            public const String DATABASE_VERSION = "3";
            private static SQLiteConnection sqlConnection = null;

            #endregion

            #region Database Methods

            public static long last_inserted_rowid()
            {
                return (long)Job.Database.executeScalar("select last_insert_rowid()");
            }

            public static bool isDatabaseConnected()
            {
                try
                {
                    return sqlConnection == null || (sqlConnection != null && (sqlConnection.State == System.Data.ConnectionState.Closed || sqlConnection.State == System.Data.ConnectionState.Broken)) ? false : true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Unexpected database error. [" + ex.Message + "]");
                }
                return false;
            }

            public static bool openDatabaseConnection()
            {
                try
                {
                    bool itWasNewDB = false;
                    if (isDatabaseConnected()) return true;
                    if (!System.IO.File.Exists(DATABASE_FILE))
                    {
                        SQLiteConnection.CreateFile(DATABASE_FILE);
                        itWasNewDB = true;
                    }
                    else
                    {
                        try
                        {
                            if (System.IO.File.ReadAllBytes(DATABASE_FILE).Length == 0)
                            {
                                itWasNewDB = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Unable to create database, please check directory permissions. [" + ex.Message + "]");
                        }
                    }

                    sqlConnection = new SQLiteConnection("Data Source=" + DATABASE_FILE + "; Version=" + DATABASE_VERSION + "; New=False; FailIfMissing=True; Synchronous=Full; Compress=True;");
                    sqlConnection.StateChange += sqlConnection_StateChange;
                    sqlConnection.Open();
                    if (itWasNewDB)
                    {
                        #region Database Initializing

                        executeQuery("create table if not exists _user(userId integer primary key autoincrement, userToken text, userKind text, userName text, userPassword text)");
                        string user = "admin", pass = Hash("admin");
                        string kind = Hash("admin|" + user);
                        executeQuery("insert into _user(userKind, userName, userPassword) values(@kind,@name,@pass)", new SQLiteParameter[] { new SQLiteParameter("@kind", kind), new SQLiteParameter("@name", user), new SQLiteParameter("@pass", pass) });

                        executeQuery("create table if not exists _report(reportId integer primary key autoincrement,reportDate timestamp default current_timestamp)");
                        executeQuery("create table if not exists _report_contamination(contId integer primary key autoincrement, contReportId integer, contTitle text, contRawWaterQuality text, contPermissableLimits text, contTreatedWaterQuality text)");

                        #endregion
                    }
                    return isDatabaseConnected();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while establishing connection with database. [" + ex.Message + "]");
                    //MessageBox.Show(mainForm, "Sorry, error occured while establishing connection with database, please try again." + Environment.NewLine + "Error message: " + ex.Message, "Can't connect to database.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            static void sqlConnection_StateChange(object sender, System.Data.StateChangeEventArgs e)
            {
                //mainForm.tlblDatabaseStatus.Text = "Database: " + getConnectionStatus(e.CurrentState);
            }

            static string getConnectionStatus(System.Data.ConnectionState state)
            {
                switch (state)
                {
                    case ConnectionState.Broken:
                        return "Broken";
                    case ConnectionState.Closed:
                        return "Disconnected";
                    case ConnectionState.Connecting:
                        return "Connecting...";
                    case ConnectionState.Executing:
                        return "Operating...";
                    case ConnectionState.Fetching:
                        return "Retrieving...";
                    case ConnectionState.Open:
                        return "Connected";
                }
                return "n/a";
            }

            public static void closeDatabaseConnection()
            {
                try
                {
                    if (isDatabaseConnected())
                    {
                        sqlConnection.Close();
                        sqlConnection = null;
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception("Error occured while closing database connection. [" + ex.Message + "]");
                    //MessageBox.Show(mainForm, "Sorry, error occured while closing database connection, please try again." + Environment.NewLine + "Error message: " + ex.Message, "Can't disconnect to database.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            public static int executeQuery(String query, SQLiteParameter[] parameterArray = null)
            {
                if (isDatabaseConnected())
                {
                    try
                    {
                        SQLiteCommand cmd = new SQLiteCommand(query, sqlConnection);
                        if (parameterArray != null)
                            cmd.Parameters.AddRange(parameterArray);
                        return cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Unable to perform operation on database. [" + ex.Message + "]");
                    }
                }
                else
                {
                    throw new Exception("No database connected.");
                }
                return -1;
            }

            public static object executeScalar(String query)
            {
                try
                {
                    if (isDatabaseConnected())
                    {
                        SQLiteCommand cmd = new SQLiteCommand(query, sqlConnection);
                        object obj = cmd.ExecuteScalar();
                        cmd.Dispose();
                        return obj;
                    }
                    else
                    {
                        throw new Exception("No database connected.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to perform scalar operation.");
                }
            }

            public static SQLiteDataReader executeReader(String query, SQLiteParameter[] parameterArray = null)
            {
                if (isDatabaseConnected())
                {
                    try
                    {
                        SQLiteCommand cmd = new SQLiteCommand(query, sqlConnection);
                        if (parameterArray != null)
                            cmd.Parameters.AddRange(parameterArray);
                        return cmd.ExecuteReader();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Unable to perform operation on database. [" + ex.Message + "]");
                        //MessageBox.Show(mainForm, "Sorry, error while performing operation on database, please try again.", "Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    throw new Exception("No database connected.");
                    //MessageBox.Show(mainForm, "No database connected, please try again by restarting software.", "No database connection found.", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                return null;
            }

            public static long countRows(String table, String where = null, SQLiteParameter[] pars=null)
            {
                try
                {
                    String _w = where != null ? " where " + where : "";
                    SQLiteDataReader dr = Job.Database.executeReader("select count(*) as total from " + table + _w, pars);
                    if (dr.Read())
                    {
                        return long.Parse(dr["total"].ToString());
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to perform count operation on database.");
                }
                return -1;
            }

            #endregion

            #region Database Operation Methods

            public static bool Login(string user, string pass, bool isAdministrator)
            {
                try
                {
                    pass = Hash(pass);
                    string kind = Hash((isAdministrator ? "admin" : "technician") + "|" + user);
                    SQLiteDataReader dr = executeReader("select * from _user where userKind=@kind and userName=@user and userPassword=@pass",
                        new SQLiteParameter[] {
                            new SQLiteParameter("@user",user),
                            new SQLiteParameter("@pass",pass),
                            new SQLiteParameter("@kind",kind)
                        });

                    if(dr!=null)
                    {
                        string userId = dr["userId"].ToString();
                        string userToken = Hash("token|" + user + "|" + pass + "|" + kind);
                        bool isUpdated= executeQuery("update _user set userToken=@token where userId=@id", new SQLiteParameter[] {
                            new SQLiteParameter("@token", userToken),
                            new SQLiteParameter("@id", userId)
                        }) == 1;
                        Properties.Settings.Default.appLogin = userToken;
                        Properties.Settings.Default["appLoginName"] = user;
                        Properties.Settings.Default.Save();
                        return isUpdated;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to perform login operation, please try again. [" + ex.Message + "]");
                }
                return false;
            }

            public static bool Logout()
            {
                try
                {
                    #region MyRegion
                    string token = Properties.Settings.Default.appLogin;
                    bool isLogouted = executeQuery("update _user set userToken='' where userToken=@token", new SQLiteParameter[] { new SQLiteParameter("@token", token) }) == 1;
                    if(isLogouted)
                    {
                        Properties.Settings.Default.appLogin = Properties.Settings.Default.appLoginName = "";
                        Properties.Settings.Default.Save();
                    }
                    return isLogouted;
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to logout user. [" + ex.Message + "]");
                }
            }

            public static bool validateUserToken(ref bool isAdmin)
            {
                try
                {
                    #region MyRegion
                    string token = Properties.Settings.Default.appLogin;
                    string user = Properties.Settings.Default.appLoginName;

                    SQLiteDataReader dr = executeReader("select userKind from _user where userToken=@token and userName=@user", new SQLiteParameter[] {
                        new SQLiteParameter("@token", token),
                        new SQLiteParameter("@user",user)
                    });

                    if(dr!=null && dr.Read())
                    {
                        string temp = "admin|" + user;
                        isAdmin = Hash(temp) == dr["userKind"].ToString();
                        return true;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to validate user token. [" + ex.Message + "]");
                }
                return false;
            }

            public static classes.User getUser(long id)
            {
                try
                {
                    #region UserInitCode
                    SQLiteDataReader dr = executeReader("select * from _user where userId=@id", new SQLiteParameter[] { new SQLiteParameter("@id", id) });
                    if(dr!=null && dr.Read())
                    {
                        classes.User user = new User();
                        user.userId = id;
                        user.userName = dr["userName"].ToString();
                        string userKindHash = Hash("admin|" + user.userName);
                        string userKind = dr["userKind"].ToString();
                        user.userKind = userKind.Equals(userKindHash) ? "administrator" : "technician";
                        return user;
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to get user data. [" + ex.Message + "]");
                }
                return null;
            }

            public static bool addUser(string userName, string userPass, bool isAdmin)
            {
                try
                {
                    #region MyRegion
                    userName = userName.ToLower();

                    string userKind = isAdmin ? "admin|" : "technician|";
                    userKind += userName;
                    userKind = Hash(userKind);

                    long count = countRows("_user", "userName=@name and userKind=@kind", new SQLiteParameter[] { new SQLiteParameter("@name", userName), new SQLiteParameter("@kind", userKind) });

                    if(count>0)
                    {
                        throw new Exception("Username is already taken, please try again with another username.");
                    }

                    return executeQuery("insert into _user(userKind,userName,userPassword) values(@kind,@user,@pass)", new SQLiteParameter[] {
                        new SQLiteParameter("@user",userName),
                        new SQLiteParameter("@kind", userKind),
                        new SQLiteParameter("@pass", Hash(userPass))
                    }) == 1;
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to get add new user. [" + ex.Message + "]");
                }
                return false;
            }

            public static bool changePassword(string current, string newone)
            {
                try
                {
                    #region UpdatePassword
                    if (Properties.Settings.Default.appLogin.Trim().Length > 0)
                    {
                        string oldPassword = Hash(current);
                        string newPassword = Hash(newone);
                        int aff = executeQuery("update _user set userPassword=@new where userPassword=@old and userToken=@token and userName=@user", new SQLiteParameter[] {
                            new SQLiteParameter("@new",newPassword),
                            new SQLiteParameter("@old",oldPassword),
                            new SQLiteParameter("@token",Properties.Settings.Default.appLogin),
                            new SQLiteParameter("@user", Properties.Settings.Default.appLoginName)
                        });
                        if (aff == 1) return true;
                        throw new Exception("Invalid current password or user token or current password and new password are same.");
                    }
                    else
                    {
                        throw new Exception("User not logged.");
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to update password. [" + ex.Message + "]");
                }
                return false;
            }

            public static List<classes.User> getAllUsers()
            {
                List<classes.User> userList = new List<User>();
                try
                {
                    #region MyRegion
                    SQLiteDataReader dr = executeReader("select userId from _user");
                    if(dr!=null)
                    {
                        while(dr.Read())
                        {
                            classes.User user = getUser(long.Parse(dr["userId"].ToString()));
                            if (user != null)
                                userList.Add(user);
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to get users. [" + ex.Message + "]");
                }
                return userList;
            }

            public static bool deleteUsers(List<long> idList)
            {
                try
                {
                    #region DeleteUsers code
                    idList.RemoveAll(x => (x == 1));
                    string ids = joinArray<long>(idList.ToArray(), ",");
                    return executeQuery("delete from _user where userId in(" + ids + ")") > 0;
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to delete users. [" + ex.Message + "]");
                }
                return false;
            }

            public static bool addReport(double aRaw, double aTreated, double iRaw, double iTreated, bool IsBacterRawPositive, bool IsBacterTreatedPositive)
            {
                try
                {
                    #region Logic
                    executeQuery("insert into _report(reportId) values(null)");
                    long reportId = last_inserted_rowid();
                    double aLimit = Properties.Settings.Default.appALimit;
                    double iLimit = Properties.Settings.Default.appILimit;
                    bool bLimit = Properties.Settings.Default.appBLimit == 0;
                    int aRow = executeQuery("insert into _report_contamination(contReportId,contTitle,contRawWaterQuality,contPermissableLimits,contTreatedWaterQuality) values(@report,@title,@rawQ,@limit,@treatedQ)", new SQLiteParameter[] {
                        new SQLiteParameter("@report",reportId),
                        new SQLiteParameter("@title","Arsenic (in mg/l)"),
                        new SQLiteParameter("@rawQ", aRaw),
                        new SQLiteParameter("@limit", aLimit),
                        new SQLiteParameter("@treatedQ", aTreated)
                    });

                    int iRow = executeQuery("insert into _report_contamination(contReportId,contTitle,contRawWaterQuality,contPermissableLimits,contTreatedWaterQuality) values(@report,@title,@rawQ,@limit,@treatedQ)", new SQLiteParameter[] {
                        new SQLiteParameter("@report",reportId),
                        new SQLiteParameter("@title","Iron (in mg/l)"),
                        new SQLiteParameter("@rawQ", iRaw),
                        new SQLiteParameter("@limit", iLimit),
                        new SQLiteParameter("@treatedQ", iTreated)
                    });

                    int bRow = executeQuery("insert into _report_contamination(contReportId,contTitle,contRawWaterQuality,contPermissableLimits,contTreatedWaterQuality) values(@report,@title,@rawQ,@limit,@treatedQ)", new SQLiteParameter[] {
                        new SQLiteParameter("@report",reportId),
                        new SQLiteParameter("@title","Bacteriological"),
                        new SQLiteParameter("@rawQ", IsBacterRawPositive?"positive":"negative"),
                        new SQLiteParameter("@limit", bLimit?"positive":"negative"),
                        new SQLiteParameter("@treatedQ",IsBacterTreatedPositive?"positive":"negative")
                    });

                    return aRow > 0 && iRow > 0 && bRow > 0;

                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to add new report. [" + ex.Message + "]");
                }
                return false;
            }

            public static bool updateReport(long reportId, double aRaw, double aTreated, double iRaw, double iTreated, bool IsBacterRawPositive, bool IsBacterTreatedPositive)
            {
                try
                {
                    #region Logic
                    bool flag = false;
                    long aRowId = 0, iRowId = 0, bRowId = 0;
                    SQLiteDataReader dr = executeReader("select * from _report_contamination where contReportId=@id", new SQLiteParameter[] { new SQLiteParameter("@id", reportId) });

                    if(dr!=null)
                    {
                        int i = 1;
                        while(dr.Read())
                        {
                            switch(i)
                            {
                                case 1:
                                    aRowId = long.Parse(dr["contId"].ToString());
                                    break;
                                case 2:
                                    iRowId = long.Parse(dr["contId"].ToString());
                                    break;
                                case 3:
                                    bRowId = long.Parse(dr["contId"].ToString());
                                    break;
                            }
                            i++;
                        }
                        flag = i == 4;
                    }

                    if (!flag) throw new Exception("Unable to get report's parameters.");

                    double aLimit = Properties.Settings.Default.appALimit;
                    double iLimit = Properties.Settings.Default.appILimit;
                    bool bLimit = Properties.Settings.Default.appBLimit == 0;

                    int aRow = executeQuery("update _report_contamination set contTitle=@title,contRawWaterQuality=@rawQ,contPermissableLimits=@limit,contTreatedWaterQuality=@treatedQ where contId=@id", new SQLiteParameter[] {
                        new SQLiteParameter("@id",aRowId),
                        new SQLiteParameter("@title","Arsenic (in mg/l)"),
                        new SQLiteParameter("@rawQ", aRaw),
                        new SQLiteParameter("@limit", aLimit),
                        new SQLiteParameter("@treatedQ", aTreated)
                    });

                    int iRow = executeQuery("update _report_contamination set contTitle=@title,contRawWaterQuality=@rawQ,contPermissableLimits=@limit,contTreatedWaterQuality=@treatedQ where contId=@id", new SQLiteParameter[] {
                        new SQLiteParameter("@id",iRowId),
                        new SQLiteParameter("@title","Iron (in mg/l)"),
                        new SQLiteParameter("@rawQ", iRaw),
                        new SQLiteParameter("@limit", iLimit),
                        new SQLiteParameter("@treatedQ", iTreated)
                    });

                    int bRow = executeQuery("update _report_contamination set contTitle=@title,contRawWaterQuality=@rawQ,contPermissableLimits=@limit,contTreatedWaterQuality=@treatedQ where contId=@id", new SQLiteParameter[] {
                        new SQLiteParameter("@id",bRowId),
                        new SQLiteParameter("@title","Bacteriological"),
                        new SQLiteParameter("@rawQ", IsBacterRawPositive?"positive":"negative"),
                        new SQLiteParameter("@limit", bLimit?"positive":"negative"),
                        new SQLiteParameter("@treatedQ",IsBacterTreatedPositive?"positive":"negative")
                    });

                    return aRow >= 0 && iRow >= 0 && bRow >= 0;

                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to save report. [" + ex.Message + "]");
                }
                return false;
            }

            public static bool deleteReport(long reportId)
            {
                try
                {
                    return executeQuery("delete from _report where reportId=@id", new SQLiteParameter[] { new SQLiteParameter("@id", reportId) }) > 0;
                } catch(Exception ex)
                {
                    throw new Exception("Unable to delete report. [" + ex.Message + "]");
                }
                return false;
            }

            public static List<classes.Report> getAllReports(long limit = 0, string sort="asc")
            {
                List<classes.Report> list = new List<Report>();
                try
                {
                    #region Code
                    string limitString = "";
                    if(limit>0)
                    {
                        limitString = " limit " + limit;
                    }
                    SQLiteDataReader dr = executeReader("select * from _report order by reportId " + sort + limitString);
                    if(dr!=null)
                    {
                        while(dr.Read())
                        {
                            list.Add(new Report() { reportId = long.Parse(dr["reportId"].ToString()), reportTime=(DateTime?)dr["reportDate"] , reportContaminations = getReportContamination(long.Parse(dr["reportId"].ToString())) });
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to get all reports. [" + ex.Message + "]");
                }
                return list;
            }

            public static classes.Report rpLast = null;
            public static classes.Report getLastReport()
            {
                if(rpLast==null)
                {
                    List<classes.Report> reps = getAllReports(1, "desc");
                    if (reps.Count > 0)
                        rpLast = reps[0];
                }
                return rpLast;
            }

            public static classes.Report LastReport
            {
                get
                {
                    return getLastReport();
                }
            }

            public static List<classes.ReportContamination> getReportContamination(long reportId)
            {
                List<classes.ReportContamination> list = new List<ReportContamination>();
                try
                {
                    #region Code
                    SQLiteDataReader dr = executeReader("select * from _report_contamination where contReportId=@id", new SQLiteParameter[] { new SQLiteParameter("@id", reportId) });
                    if(dr!=null)
                    {
                        while(dr.Read())
                        {
                            classes.ReportContamination rc = new ReportContamination() {
                                contId = long.Parse(dr["contId"].ToString()),
                                reportId = reportId,
                                title = dr["contTitle"].ToString(),
                                rawQ = dr["contRawWaterQuality"],
                                treatedQ=dr["contTreatedWaterQuality"],
                                limit=dr["contPermissableLimits"]
                            };
                            list.Add(rc);
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to get contamination details of report. [" + ex.Message + "]");
                }
                return list;
            }

            #endregion
        }
        #endregion

    }
}
