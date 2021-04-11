﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;

namespace register
{
    public class MyLog
    {
        public UInt64 id;
        public DateTime date;
        public string data;
    }
    class ContentProvider
    {
        public void init()
        {
            ClientSecrets cs = new ClientSecrets();
            cs.ClientId = "";
            cs.ClientSecret = "";
            //GoogleClientSecrets 
            //var fa = FirebaseAuth.DefaultInstance();
            //GoogleWebAuthorizationBroker.AuthorizeAsync(cs,
            //    new[] { DriveService.Scope.Drive},
            //        "user",
            //        )
            // Initialize the default app
            var cre = GoogleCredential.FromFile(@"C:\Users\Onsiter\Downloads\google-services.json");

            var appOpt = new AppOptions();
            appOpt.ProjectId = "testfirebase-18749";
            appOpt.ServiceAccountId = "";
            appOpt.Credential = cre;
            var defaultApp = FirebaseApp.Create(appOpt);
            Console.WriteLine(defaultApp.Name); // "[DEFAULT]"

            // Retrieve services by passing the defaultApp variable...
            var defaultAuth = FirebaseAuth.GetAuth(defaultApp);

            // ... or use the equivalent shorthand notation
            defaultAuth = FirebaseAuth.DefaultInstance;
        }
    }

    public class MyGroup
    {
        public UInt64 ID;
        public string zGroup;
        public int nGroup;
    }
    public class MyUser
    {
        public UInt64 ID;
        public string zUser;
        public string zFb;
        public int nGroup;

        public string zUserFb { get { return zUser + " - " + zFb; } }
    }

    public class MyProgram
    {
        public UInt64 ID;
        public string zName;
        public DateTime startDate;
        public string zPath;
    }
    public class lFireBaseContentProvider
    {

    }

    public class MyReg
    {
        public UInt64 ID;
        public MyProgram prog;
        public MyUser user;
        public string zNote;
        public int nStatus;
        public string zStatus;
        public UInt64 userID;
        public UInt64 programID;
    }
    public class lOleDbContentProvider
    {
        private OleDbConnection m_cnn;
        OleDbDataAdapter m_dataAdapter;
        //Dictionary<UInt64, MyUser> m_userDict;
        public void initCnn(string m_cnnStr)
        {
            //init cnn
            m_cnn = new OleDbConnection(m_cnnStr);
            m_cnn.Open();
            m_dataAdapter = new OleDbDataAdapter
            {
                SelectCommand = new OleDbCommand(
                string.Format("select * from users where titleId = ? order by ord"),
                m_cnn)
            };
            m_dataAdapter.SelectCommand.Parameters.Add(
                new OleDbParameter() { DbType = DbType.UInt64 });

            //m_userDict = new Dictionary<ulong, MyUser>();
        }
        public void clear()
        {
            //m_userDict.Clear();
        }

        public MyUser GetUser(UInt64 uID)
        {
            return null;
        }
        public List<MyUser> GetAllUsers()
        {
            List<MyUser> userLst = new List<MyUser>();
            var qry = "select * from users";
            var cmd = new OleDbCommand(qry, m_cnn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                UInt64 uID = Convert.ToUInt64(reader["ID"]);
                userLst.Add(new MyUser()
                {
                    ID = uID,
                    zFb = Convert.ToString(reader["zFb"]),
                    zUser = Convert.ToString(reader["zUser"]),
                    nGroup = int.Parse(reader["nGroup"].ToString())
                });
            }
            reader.Close();
            return userLst;
        }
        public List<MyUser> GetAllUsers(List<string> tags)
        {
            List<MyUser> userLst = new List<MyUser>();
            foreach(var tag in tags)
            {
                userLst.AddRange(GetAllUsers(tag));
            }
            return userLst;
        }
        public List<MyUser> GetAllUsers(string tag)
        { 
            List<MyUser> userLst = new List<MyUser>();

            var qry = "select * from tags WHERE zTag = @zTag";
            var cmd = new OleDbCommand(qry, m_cnn);
            cmd.Parameters.Add("@zTag",OleDbType.Char);
            cmd.Parameters["@zTag"].Value = tag;

            var qry2 = "select * from user_tag WHERE tagID = @tagID";
            var cmd2 = new OleDbCommand(qry2, m_cnn);
            cmd2.Parameters.Add("@tagID",OleDbType.Numeric);

            var userDict = GetUserDict();

            var rd = cmd.ExecuteReader();
            while(rd.Read()){
                var tagID = Convert.ToUInt64(rd["ID"]);
                cmd2.Parameters["@tagID"].Value = tagID;
                var reader = cmd2.ExecuteReader();
                while (reader.Read())
                {
                    UInt64 uID = Convert.ToUInt64(reader["userID"]);
                    var user = userDict[uID];
                    userLst.Add(new MyUser()
                    {
                        ID = user.ID,
                        zFb = user.zFb,
                        zUser = user.zUser,
                        nGroup = user.nGroup
                    });
                }
                reader.Close();
            }
            rd.Close();
            
            return userLst;
        }

        private UInt64 AddPath(string path)
        {
            UInt64 id;
            var cmd = new OleDbCommand("SELECT * FROM paths where (zPath = ?)", m_cnn);
            cmd.Parameters.Add(new OleDbParameter("",path));
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                id = Convert.ToUInt64(reader["ID"]);
            }
            else
            {
                var addCmd = new OleDbCommand("INSERT INTO paths (zPath) VALUES (?)"); 
                addCmd.Parameters.Add(new OleDbParameter("",path));
                var n = addCmd.ExecuteNonQuery();

                addCmd.CommandText = "Select @@Identity";
                var obj = addCmd.ExecuteScalar();
                id = Convert.ToUInt64(obj);
            }
            return id;
        }

        public bool AddProg(MyProgram myProgram)
        {
            var pathID = AddPath(myProgram.zPath);
            var cmd = new OleDbCommand("INSERT INTO programs (zName, startDate, pathID) VALUES (@zName,@startDate,@pathID)", m_cnn);
            cmd.Parameters.Add(new OleDbParameter("@zName",OleDbType.Char));
            cmd.Parameters.Add(new OleDbParameter("@startDate",OleDbType.Date));
            cmd.Parameters.Add(new OleDbParameter("@pathID",OleDbType.Numeric));
            cmd.Parameters["@zName"].Value = myProgram.zName;
            cmd.Parameters["@startDate"].Value = myProgram.startDate;
            cmd.Parameters["@pathID"].Value = pathID;
            var n = cmd.ExecuteNonQuery();
            return n == 1;
        }

        class MyAction
        {
            public enum ActionType {
                add,
                delete,
                edit
            };

            public string actiontype;
            public string objType;
            public string jsonText;
        }

        public bool LogsSaveAdd(MyProgram prog)
        {
            //get new id
            UInt64 lastId = 0;
            try
            {
                var cmd = new OleDbCommand("SELECT max(ID) from logs",m_cnn);
                lastId = Convert.ToUInt64(cmd.ExecuteScalar());
            }
            catch
            {
                //empty table
            }

            //insert
            var cmd2 = new OleDbCommand("INSERT INTO logs (ID,modifyDate,zLog) VALUES (@ID,@modifyDate,@zLog)",m_cnn);
            var action = new MyAction()
            {
                actiontype = "add",
                objType = prog.ToString(),
                jsonText = JsonConvert.SerializeObject(prog)
            };
            var txt = JsonConvert.SerializeObject(action);
            cmd2.Parameters.Add(new OleDbParameter("@ID", OleDbType.Numeric));
            cmd2.Parameters.Add(new OleDbParameter("@modifyDate", OleDbType.Date));
            cmd2.Parameters.Add(new OleDbParameter("@zLog", OleDbType.LongVarWChar));
            cmd2.Parameters["@ID"].Value = lastId + 1;
            cmd2.Parameters["@modifyDate"].Value = DateTime.Now;;
            cmd2.Parameters["@zLog"].Value = txt;
            var n = cmd2.ExecuteNonQuery();
            return n == 1;
        }
        public void AddLogs(List<MyLog> checkOutLst)
        {
            var cmd = new OleDbCommand("INSERT INTO logs (ID, modifyDate, zLog) VALUES (@id,@date,@data)", m_cnn);
            cmd.Parameters.Add(new OleDbParameter("@id",OleDbType.Numeric));
            cmd.Parameters.Add(new OleDbParameter("@date",OleDbType.Date));
            cmd.Parameters.Add(new OleDbParameter("@data",OleDbType.Char));
            foreach(MyLog log in checkOutLst)
            {
                cmd.Parameters["@id"].Value = log.id;
                cmd.Parameters["@date"].Value = log.date;
                cmd.Parameters["@data"].Value = log.data;
                cmd.ExecuteNonQuery();
            }
        }
        public List<MyLog> GetLogs(List<UInt64> ids)
        {
            List<MyLog> logs = new List<MyLog>();
            var cmd = new OleDbCommand("SELECT * FROM logs WHERE ID = @id", m_cnn);
            cmd.Parameters.Add(new OleDbParameter("@id",OleDbType.Numeric));
            foreach(UInt64 id in ids)
            {
                cmd.Parameters["@id"].Value = id;
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    var log = new MyLog();
                    log.id = Convert.ToUInt64(rd["ID"]);
                    log.date = Convert.ToDateTime(rd["modifyDate"]);
                    log.data = Convert.ToString(rd["zLog"]);
                    logs.Add(log);
                }
                rd.Close();
            }
            return logs;
        }

        public Dictionary<ulong, MyUser> GetUserDict()
        {
            Dictionary<ulong, MyUser> userDict = new Dictionary<ulong, MyUser>();
            List<MyUser> userLst = new List<MyUser>();
            var qry = "select * from users";
            var cmd = new OleDbCommand(qry, m_cnn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                UInt64 uID = Convert.ToUInt64(reader["ID"]);
                userLst.Add(new MyUser()
                {
                    ID = uID,
                    zFb = Convert.ToString(reader["zFb"]),
                    zUser = Convert.ToString(reader["zUser"]),
                    nGroup = int.Parse(reader["nGroup"].ToString())
                });
            }
            reader.Close();
            foreach(var user in userLst)
            {
                userDict.Add(user.ID,user);
            }
            return userDict;
        }

        public List<MyProgram> GetAllProgs()
        {
            List<MyProgram> progLst = new List<MyProgram>();
            var qry = "SELECT * FROM paths INNER JOIN programs ON paths.ID = programs.pathID "
                + " ORDER BY programs.startDate DESC;";
            var cmd = new OleDbCommand(qry, m_cnn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                progLst.Add(new MyProgram()
                {
                    ID = Convert.ToUInt64(reader["programs.ID"]),
                    zName = Convert.ToString(reader["zName"]),
                    startDate = Convert.ToDateTime(reader["startDate"]),
                    zPath = Convert.ToString(reader["zPath"]),
                });;
            }
            reader.Close();
            return progLst;
        }
        public List<MyProgram> GetAllProgs(int limit)
        {
            List<MyProgram> progLst = new List<MyProgram>();
            var qry = string.Format("select top {0} * from programs order by startDate desc", limit);
            var cmd = new OleDbCommand(qry, m_cnn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                progLst.Add(new MyProgram()
                {
                    ID = Convert.ToUInt64(reader["ID"]),
                    zName = Convert.ToString(reader["zName"]),
                    startDate = Convert.ToDateTime(reader["startDate"])
                });
            }
            reader.Close();
            return progLst;
        }

        public List<MyReg> GetRegs(MyProgram prog)
        {
            return GetRegs(prog.ID);
        }
        public List<MyReg> GetRegs(UInt64 progID)
        { 
            List<MyReg> regLst = new List<MyReg>();
            var qry = "SELECT * FROM program_user WHERE programID = ?";
            var cmd = new OleDbCommand(qry, m_cnn);
            cmd.Parameters.Add(cmd.CreateParameter());
            cmd.Parameters[0].Value = progID;
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                regLst.Add(new MyReg()
                {
                    ID = Convert.ToUInt64(reader["ID"]),
                    zNote = Convert.ToString(reader["zNote"]),
                    nStatus = int.Parse(reader["nStatus"].ToString()),
                    zStatus = Convert.ToString(reader["zStatus"]),
                    userID = Convert.ToUInt64(reader["userID"]),
                    programID = Convert.ToUInt64(reader["programID"])
                });
            }
            reader.Close();
            return regLst;
        }

        public List<MyGroup> GetAllGroups()
        {
            List<MyGroup> grpLst = new List<MyGroup>();
            var qry = "SELECT * FROM groups";
            var cmd = new OleDbCommand(qry, m_cnn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                grpLst.Add(new MyGroup()
                {
                    ID = Convert.ToUInt64(reader["ID"]),
                    nGroup = int.Parse(reader["nGroup"].ToString()),
                    zGroup = Convert.ToString(reader["zGroup"]),
                });
            }
            reader.Close();
            return grpLst;
        }
    //}
    //public class lOleDbContentProvider
    //{
    //    private OleDbConnection m_cnn;
    //    OleDbDataAdapter m_dataAdapter;
    //    Dictionary<UInt64, MyUser> m_userDict;
        //public void initCnn(string m_cnnStr)
        //{
        //    //init cnn
        //    m_cnn = new OleDbConnection(m_cnnStr);
        //    m_cnn.Open();
        //    m_dataAdapter = new OleDbDataAdapter
        //    {
        //        SelectCommand = new OleDbCommand(
        //        string.Format("select * from users where titleId = ? order by ord"),
        //        m_cnn)
        //    };
        //    m_dataAdapter.SelectCommand.Parameters.Add(
        //        new OleDbParameter() { DbType = DbType.UInt64 });

        //    m_userDict = new Dictionary<ulong, MyUser>();
        //}
        //public List<MyUser> GetAllUsers()
        //{
        //    List<MyUser> userLst = new List<MyUser>();
        //    var qry = "select * from users";
        //    var cmd = new OleDbCommand(qry, m_cnn);
        //    var reader = cmd.ExecuteReader();
        //    while (reader.Read())
        //    {
        //        UInt64 uID = Convert.ToUInt64(reader["ID"]);
        //        userLst.Add(new MyUser()
        //        {
        //            ID = uID,
        //            zFb = Convert.ToString(reader["zFb"]),
        //            zUser = Convert.ToString(reader["zUser"]),
        //            nGroup = int.Parse(reader["nGroup"].ToString())
        //        });
        //    }
        //    reader.Close();
        //    return userLst;
        //}

        //public List<MyProgram> GetAllProgs()
        //{
        //    List<MyProgram> progLst = new List<MyProgram>();
        //    var qry = "select * from programs";
        //    var cmd = new OleDbCommand(qry, m_cnn);
        //    var reader = cmd.ExecuteReader();
        //    while (reader.Read())
        //    {
        //        progLst.Add(new MyProgram()
        //        {
        //            ID = Convert.ToUInt64(reader["ID"]),
        //            zName = Convert.ToString(reader["zName"]),
        //            startDate = Convert.ToDateTime(reader["startDate"])
        //        });
        //    }
        //    reader.Close();
        //    return progLst;
        //}

        //public List<MyReg> GetRegs(MyProgram prog)
        //{
        //    List<MyReg> regLst = new List<MyReg>();
        //    var qry = "SELECT * FROM program_user WHERE programID = ?";
        //    var cmd = new OleDbCommand(qry, m_cnn);
        //    cmd.Parameters.Add(cmd.CreateParameter());
        //    cmd.Parameters[0].Value = prog.ID;
        //    var reader = cmd.ExecuteReader();
        //    while (reader.Read())
        //    {
        //        regLst.Add(new MyReg()
        //        {
        //            zNote = Convert.ToString(reader["zNote"]),
        //            nStatus = int.Parse(reader["nStatus"].ToString()),
        //            zStatus = Convert.ToString(reader["zStatus"]),
        //            userID = Convert.ToUInt64(reader["userID"]),
        //            programID = Convert.ToUInt64(reader["programID"])
        //        });
        //    }
        //    reader.Close();
        //    return regLst;
        //}
        //public List<MyGroup> GetAllGroups()
        //{
        //    List<MyGroup> grpLst = new List<MyGroup>();
        //    var qry = "SELECT * FROM groups";
        //    var cmd = new OleDbCommand(qry, m_cnn);
        //    var reader = cmd.ExecuteReader();
        //    while (reader.Read())
        //    {
        //        grpLst.Add(new MyGroup()
        //        {
        //            ID = Convert.ToUInt64(reader["ID"]),
        //            nGroup = int.Parse(reader["nGroup"].ToString()),
        //            zGroup = Convert.ToString(reader["zGroup"]),
        //        });
        //    }
        //    reader.Close();
        //    return grpLst;
        //}

        public int AddUpdateReg(MyReg reg)
        {
            {
                var cmd = new OleDbCommand("SELECT ID FROM program_user WHERE userID = ? AND programID = ?", m_cnn);
                cmd.Parameters.Add("", OleDbType.Numeric).Value = reg.userID;
                cmd.Parameters.Add("", OleDbType.Numeric).Value = reg.programID;
                var id = cmd.ExecuteScalar();
                if (id != null)
                {
                    reg.ID = Convert.ToUInt64(id);
                }
            }

            if (reg.ID != 0)
            {
                var cmd = new OleDbCommand("UPDATE program_user SET nStatus = ?, zNote = ? WHERE ID = ?", m_cnn);
                cmd.Parameters.Add("", OleDbType.Numeric).Value = reg.nStatus;
                cmd.Parameters.Add("", OleDbType.Char).Value = reg.zNote;
                cmd.Parameters.Add("", OleDbType.Numeric).Value = reg.ID;
                var n = cmd.ExecuteNonQuery();
                return n;
            }
            else
            {
                var cmd = new OleDbCommand("INSERT INTO program_user (userID, programID, nStatus, zNote) VALUES (?, ?, ?, ?)", m_cnn);
                cmd.Parameters.Add("", OleDbType.Numeric).Value = reg.userID;
                cmd.Parameters.Add("", OleDbType.Numeric).Value = reg.programID;
                cmd.Parameters.Add("", OleDbType.Numeric).Value = reg.nStatus;
                cmd.Parameters.Add("", OleDbType.Char,255).Value = reg.zNote;
                var n = cmd.ExecuteNonQuery();

                cmd.CommandText = "Select @@Identity";
                var id = cmd.ExecuteScalar();
                reg.ID = Convert.ToUInt64(id);
                return n;
            }
        }

        public List<string> GetAllTags()
        {
            return new List<string>()
            {
                "BCS", "Dự Thính", "Chính Thức"
            };
        }

        public List<UInt64> GetIds(string table)
        {
            List<UInt64> ids = new List<ulong>();
            var cmd = new OleDbCommand(string.Format("SELECT ID FROM {0}",table), m_cnn);
            var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                ids.Add(Convert.ToUInt64(rd["ID"]));
            }
            return ids;
        }
    }

}
