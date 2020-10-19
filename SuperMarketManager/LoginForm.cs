using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperMarketManager
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            if (username.Length == 0 || password.Length == 0)
            {
                MessageBox.Show(this, "用户名和密码不能为空");
            }
            else
            {
                MySqlConnection conn = MysqlConnector.GetInstance();
                if (conn != null) 
                {
                    try
                    {
                        string sqlstr = string.Format("select * from tb_admin where admin_username='{0}' and admin_password='{1}';", username, password);
                        conn.Open();
                        MySqlCommand com = new MySqlCommand(sqlstr, conn);
                        MySqlDataReader reader = com.ExecuteReader();
                        if (reader.Read())
                        {
                            DialogResult = DialogResult.OK;     // 设置模态窗口状态为OK，代表登录成功
                            bool issu = reader.GetBoolean("admin_isowner");
                            if (issu)
                            {
                                GlobalVar.login = new SuperAdmin();
                            }
                            else
                            {
                                GlobalVar.login = new Admin();
                            }
                            GlobalVar.login.Username = username;
                            GlobalVar.login.Id = reader.GetInt32(0);
                        }
                        else
                        {
                            MessageBox.Show(this, "登录失败，用户名或密码错误");
                        }
                        reader.Close();
                    }
                    catch(Exception e1)
                    {
                        MessageBox.Show(this, string.Format("Exception: {0}", e1));
                    }
                    finally
                    {
                        conn.Close();
                    }
                    if (GlobalVar.login != null)
                    {
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show(this, "数据库连接失败");
                }
            }
        }
    }
}
