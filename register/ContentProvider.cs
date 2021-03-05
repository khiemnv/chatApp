using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace register
{
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
        Dictionary<UInt64, MyUser> m_userDict;
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

            m_userDict = new Dictionary<ulong, MyUser>();
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


        public List<MyProgram> GetAllProgs()
        {
            List<MyProgram> progLst = new List<MyProgram>();
            var qry = "select top 10 * from programs order by startDate desc";
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
            List<MyReg> regLst = new List<MyReg>();
            var qry = "SELECT * FROM program_user WHERE programID = ?";
            var cmd = new OleDbCommand(qry, m_cnn);
            cmd.Parameters.Add(cmd.CreateParameter());
            cmd.Parameters[0].Value = prog.ID;
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
                cmd.Parameters.Add("", OleDbType.Numeric).Value = reg.ID;
                cmd.Parameters.Add("", OleDbType.Numeric).Value = reg.nStatus;
                cmd.Parameters.Add("", OleDbType.Char).Value = reg.zNote;
                return cmd.ExecuteNonQuery();
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
    }

}
