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
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        public class AdminUsernameRepeat : Exception
        {
            public AdminUsernameRepeat(string message) : base(message)
            {

            }
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            if (!GlobalVar.login.GetPermission())
            {
                listView1.Visible = false;
                groupBox3.Enabled = false;
            }
            else
            {
                InitLvAdmin();
            }
        }

        private int InitLvAdmin()
        {
            listView1.Items.Clear();
            int n = 0;
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string mysqlstr = "select * from tb_admin";
                MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetBoolean(3)) continue;
                    ListViewItem item = new ListViewItem();
                    item.Text = (++n).ToString();
                    item.SubItems.Add(reader.GetString(1));
                    item.SubItems.Add(reader.GetString(2));
                    listView1.Items.Add(item);
                }
                reader.Close();
            }
            catch(Exception e)
            {
                MessageBox.Show(this, string.Format("Exception: {0}", e));
            }
            finally
            {
                conn.Close();
            }
            return n;
        }

        private void btnChangeself_Click(object sender, EventArgs e)
        {
            string oldpassword = txtSelfOldPassword.Text;
            string password = txtSelfNewPassword.Text;
            string confirm = txtSelfConfirm.Text;
            if (oldpassword.Length != 0 && password.Length != 0 && confirm.Length != 0)
            {
                if (password.Equals(confirm))
                {
                    MySqlConnection conn = MysqlConnector.GetInstance();
                    try
                    {
                        conn.Open();
                        string mysqlstr = string.Format("update tb_admin set admin_password='{0}' where admin_id={1} and admin_password='{2}';",
                            password, GlobalVar.login.Id, oldpassword);
                        MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                        int res = comm.ExecuteNonQuery();
                        if (res != 0)
                        {
                            MessageBox.Show(this, "修改成功");
                            txtSelfConfirm.Text = "";
                            txtSelfNewPassword.Text = "";
                            txtSelfOldPassword.Text = "";
                        }
                        else
                        {
                            MessageBox.Show(this, "修改失败");
                        }
                    }
                    catch(Exception e1)
                    {
                        MessageBox.Show(this, String.Format("Exception: {0}", e1));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
                else
                {
                    MessageBox.Show(this, "两次输入的密码不一致");
                }
            }
            else
            {
                MessageBox.Show(this, "信息不能为空");
            }
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            txtUsername.Text = listView1.SelectedItems[0].SubItems[1].Text;
            txtPassword.Text = listView1.SelectedItems[0].SubItems[2].Text;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "未选中任何项");
                return;
            }
            string username = listView1.SelectedItems[0].SubItems[1].Text;
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string mysqlstr = string.Format("delete from tb_admin where admin_username='{0}';", username);
                MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                int re = comm.ExecuteNonQuery();
                if (re != 0)
                {
                    MessageBox.Show(this, "删除成功");
                    listView1.Items.Remove(listView1.SelectedItems[0]);
                }
                else
                {
                    MessageBox.Show(this, "删除失败");
                }
            }
            catch(MySqlException e1)
            {
                MessageBox.Show(this, "无法删除此管理员，因为此管理员有操作记录，不可删除！");
            }
            catch(Exception e1)
            {
                MessageBox.Show(this, string.Format("Exception: {0}", e1));
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "未选中任何项");
                return;
            }
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string oldusername = listView1.SelectedItems[0].SubItems[1].Text;

            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                if (!username.Equals(oldusername))
                {
                    string sqlstr = string.Format("select admin_id from tb_admin where admin_username='{0}';", username);
                    MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                    object re = comm.ExecuteScalar();
                    if (re != null)
                    {
                        throw new AdminUsernameRepeat("用户名已存在");
                    }
                }
                string mysqlstr = string.Format("update tb_admin set admin_username='{0}',admin_password='{1}' where admin_username='{2}';",
                    username, password, oldusername);
                MySqlCommand comm1 = new MySqlCommand(mysqlstr, conn);
                int res = comm1.ExecuteNonQuery();
                if (res != 0)
                {
                    listView1.SelectedItems[0].SubItems[1].Text = username;
                    listView1.SelectedItems[0].SubItems[2].Text = password;
                    MessageBox.Show(this, "修改成功");
                }
                else
                {
                    MessageBox.Show(this, "修改失败");
                }
            }
            catch (AdminUsernameRepeat e1)
            {
                MessageBox.Show(this, e1.Message);
            }
            catch (Exception e1)
            {
                MessageBox.Show(this, string.Format("Exception: {0}", e1));
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            if (username.Length != 0 && password.Length != 0)
            {
                MySqlConnection conn = MysqlConnector.GetInstance();
                try
                {
                    conn.Open();
                    string mysqlstr = string.Format("select admin_id from tb_admin where admin_username='{0}';", username);
                    MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                    object reo = comm.ExecuteScalar();
                    if (reo != null)
                    {
                        throw new AdminUsernameRepeat("用户名已存在");
                    }
                    else
                    {
                        string sqlstr = string.Format("insert into tb_admin(admin_username,admin_password) value('{0}','{1}');", username, password);
                        MySqlCommand comm1 = new MySqlCommand(sqlstr, conn);
                        int re = comm1.ExecuteNonQuery();
                        if (re == 1)
                        {
                            ListViewItem item = new ListViewItem();
                            item.Text = ((listView1.Items.Count + 1).ToString());
                            item.SubItems.Add(username);
                            item.SubItems.Add(password);
                            listView1.Items.Add(item);
                            MessageBox.Show(this, "添加成功");
                        }
                        else
                        {
                            MessageBox.Show(this, "添加失败");
                        }
                    }
                }
                catch (AdminUsernameRepeat e1)
                {
                    MessageBox.Show(this, e1.Message);
                }
                catch (Exception e1)
                {
                    MessageBox.Show(this, string.Format("Exception: {0}", e1));
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
                MessageBox.Show(this, "用户名和密码不允许为空");
            }
        }
    }
}
