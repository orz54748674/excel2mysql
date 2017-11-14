using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Excel2Mysql
{
    public partial class Main : Form
    {
        private Dictionary<string, string> _files = new Dictionary<string, string>();
        private util.Mysql mysql = util.Mysql.Instance;
        private entity.UpdataPro updateFunc;
        private entity.SetMaxPro setFunc;
        private entity.DbConfig[] dbConfigs;
        private static bool lockStatus = false;
        private List<string> myLockList = new List<string>();

        public Main()
        {
            InitializeComponent();
        }

        private void mainLoad(object sender, EventArgs e)
        {
            //配置文件
            bool exit = false;
            try
            {
                if (!System.IO.File.Exists("config.json"))
                {
                    exit = true;
                    MessageBox.Show("配置文件不存在", "加载config.json");
                }
                else
                {
                    entity.JsConfig jsConfig = util.Json.parse<entity.JsConfig>(System.IO.File.ReadAllText("config.json"));
                    if (jsConfig.User == null || jsConfig.DbConfigs == null)
                    {
                        exit = true;
                        MessageBox.Show("配置文件错误", "加载config.json");
                    }
                    else
                    {
                        mysql.UserName = jsConfig.User.name;
                        dbConfigs = jsConfig.DbConfigs;
                        for (int i = 0; i < dbConfigs.Length; i++)
                        {
                            dbList.Items.Add(dbConfigs[i].desc);
                        }
                        dbList.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception err)
            {
                exit = true;
                MessageBox.Show(err.Message, "加载配置文件出错");
            }
            if (exit || !checkUserName())
            {
                System.Environment.Exit(0);
                return;
            }

            //进度设置
            setFunc = (maxVal) =>
            {
                showProgress.Value = 0;
                showProgress.Maximum = maxVal;
            };
            updateFunc = (fileName) =>
            {
                if (fileName == "")
                {
                    showProgress.Value = 0;
                    showProgress.Maximum = 0;
                    updateLabel.Text = "";
                }
                else
                {
                    updateLabel.Text = fileName;
                    showProgress.Value += 1;
                }
            };
        }

        private void fileDragDrop(object sender, DragEventArgs e)
        {
            if (lockStatus)
            {
                return;
            }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in filePath)
            {
                addExeclFile(file);
            }
        }

        private void formCloseUnlockTable(object sender, FormClosedEventArgs e)
        {
            unlockTable();
        }

        private void dbList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dbConfigs.Length != 0 && dbList.SelectedIndex != -1)
            {
                mysql.dbConfig = dbConfigs[dbList.SelectedIndex];
                loadMyLockTbl();
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < fileList.Items.Count; i++)
            {
                fileList.SetItemChecked(i, true);
            }
        }

        private void btnInverse_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < fileList.Items.Count; i++)
            {
                fileList.SetItemChecked(i, fileList.GetItemChecked(i) != true);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择Excel文件";
            dialog.Filter = "Excel文件(*.xls,*.xlsx,*.xlsm)|*.xls;*.xlsx;*.xlsm";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] names = dialog.FileNames;
                foreach (string file in names)
                {
                    addExeclFile(file);
                }
            }
        }

        private void addExeclFile(string filePath)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string ext = System.IO.Path.GetExtension(filePath).ToLower();
            if (ext != ".xls" && ext != ".xlsx" && ext != ".xlsm")
            {
                MessageBox.Show("只允许.xls或.xlsx或.xlsm文件！", name);
                return;
            }
            if (!_files.ContainsKey(name))
            {
                fileList.Items.Add(name);
                fileList.SetItemChecked(fileList.Items.Count - 1, true);
                _files.Add(name, filePath);
            }
            else
            {
                int index = fileList.Items.IndexOf(name);

                if (index != -1)
                {
                    fileList.SetItemChecked(index, true);
                }
            }
        }

        private void list_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Point p = e.Location;
                int index = fileList.IndexFromPoint(p);
                if (index == -1)
                {
                    return;
                }
                string name = fileList.Items[index].ToString();
                if (_files.Remove(name))
                {
                    fileList.Items.RemoveAt(index);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                System.Drawing.Point p = e.Location;
                int index = fileList.IndexFromPoint(p);
                if (index == -1)
                {
                    fileList.SelectedIndex = -1;
                }
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (!checkDbconfig())
            {
                return;
            }
            lockAllBtn(0);
            mysql.DownloadToExcel(setFunc, updateFunc);
            unlockAllBtn(0);
            DialogResult dr = MessageBox.Show("下载完成！", "结束", MessageBoxButtons.OK);
            if (dr == DialogResult.OK)
            {
                updateFunc("");
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (!checkDbconfig())
            {
                return;
            }
            if (fileList.Items.Count == 0)
            {
                MessageBox.Show("请添加要上传的表！", "提示");
                return;
            }
            int checkCount = 0;
            for (int i = 0; i < fileList.Items.Count; i++)
            {
                if (fileList.GetItemChecked(i))
                {
                    checkCount++;
                }
            }
            if (checkCount == 0)
            {
                MessageBox.Show("请勾选要上传的表！", "提示");
                return;
            }
            if (!lockStatus)
            {
                MessageBox.Show("请先锁表！", "提示");
                return;
            }
            lockAllBtn(0);
            setFunc(checkCount);
            int failCnt = 0;
            int skipCnt = 0;
            string emptyStr = "";
            for (int i = 0; i < fileList.Items.Count; i++)
            {
                if (fileList.GetItemChecked(i))
                {
                    string errMsg = "";
                    string tblName = fileList.Items[i].ToString();
                    updateFunc(tblName);
                    mysql.UploadExecl(_files[tblName], out errMsg);
                    if (errMsg != "")
                    {
                        fileList.SetItemChecked(i, fileList.GetItemChecked(i) != true);
                        if (errMsg == "data empty")
                        {
                            skipCnt++;
                            emptyStr += "\t" + tblName + "\n";
                        }
                        else
                        {
                            failCnt++;
                            MessageBox.Show(errMsg, tblName);
                        }
                    }
                }
            }
            string result = "上传完成，总数：" + checkCount + "，成功：" + (checkCount - failCnt - skipCnt) + "，失败：" + failCnt + "，未处理：" + skipCnt;
            if (emptyStr != "")
            {
                result += "\n自动跳过无数据的表：\n" + emptyStr;
            }
            unlockTable();
            unlockAllBtn(1);
            DialogResult dr = MessageBox.Show(result, "结束", MessageBoxButtons.OK);
            if (dr == DialogResult.OK)
            {
                updateFunc("");
            }
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            if (!checkDbconfig())
            {
                return;
            }
            if (lockStatus)
            {
                unlockTable();
                unlockAllBtn(1);
                return;
            }
            List<string> lockTbl = null;
            if (checkLockTable(out lockTbl))
            {
                if (lockTable(lockTbl))
                {
                    myLockList.Clear();
                    myLockList = new List<string>(lockTbl.ToArray());
                    lockAllBtn(1);
                }
            }
        }

        private bool checkLockTable(out List<string> lockTbl)
        {
            lockTbl = new List<string>();
            if (fileList.Items.Count == 0)
            {
                MessageBox.Show("请添加要锁定的表！", "提示");
                return false;
            }
            for (int i = 0; i < fileList.Items.Count; i++)
            {
                if (fileList.GetItemChecked(i))
                {
                    lockTbl.Add(fileList.Items[i].ToString());
                }
            }
            if (lockTbl.Count == 0)
            {
                MessageBox.Show("请勾选要锁定的表！", "提示");
                return false;
            }
            Dictionary<string, string> retTbl = null;
            if (!mysql.QueryTableLock(lockTbl, out retTbl))
            {
                return false;
            }
            if (retTbl.Count != 0)
            {
                string lockStr = "";
                foreach (var item in retTbl)
                {
                    lockStr += "\t" + item.Key + "," + item.Value + "\n";
                    //自动去除勾选
                    for (int i = 0; i < fileList.Items.Count; i++)
                    {
                        if (fileList.GetItemChecked(i) && fileList.Items[i].ToString() == item.Key)
                        {
                            fileList.SetItemChecked(i, false);
                            break;
                        }
                    }
                }
                MessageBox.Show("以下表锁定失败：\n\t表名\t锁定人\n" + lockStr + "\n已自动去除失败项", "失败");
                return false;
            }
            return true;
        }

        private bool lockTable(List<string> lockList)
        {
            if (lockList.Count == 0)
            {
                return false;
            }
            return mysql.UpdateTableLock(lockList, true);
        }

        private void lockAllBtn(int code)
        {
            lockStatus = true;
            fileList.Enabled = false;
            btnSelectAll.Enabled = false;
            btnInverse.Enabled = false;
            btnOpen.Enabled = false;
            dbList.Enabled = false;
            if (code == 0)
            {
                btnLock.Enabled = false;
                btnDownload.Enabled = false;
                btnUpload.Enabled = false;
            }
            else if (code == 1)
            {
                btnLock.Text = "解锁";
                btnDownload.Enabled = false;
            }
            else if (code == 2)
            {
                btnLock.Text = "解锁";
                btnDownload.Enabled = false;
                btnUpload.Enabled = false;
            }
        }

        private void unlockAllBtn(int code)
        {
            lockStatus = false;
            fileList.Enabled = true;
            btnSelectAll.Enabled = true;
            btnInverse.Enabled = true;
            btnOpen.Enabled = true;
            btnUpload.Enabled = true;
            btnLock.Enabled = true;
            btnDownload.Enabled = true;
            dbList.Enabled = true;
            if (code == 0)
            {
            }
            else if (code == 1)
            {
                btnLock.Text = "锁表";
            }
        }

        private void unlockTable()
        {
            if (myLockList.Count > 0)
            {
                if (mysql.UpdateTableLock(myLockList, false))
                {
                    myLockList.Clear();
                }
            }
        }

        private bool checkDbconfig()
        {
            if (dbConfigs.Length == 0)
            {
                MessageBox.Show("请设置配置文件！", "提示");
                return false;
            }
            if (dbList.SelectedIndex == -1)
            {
                MessageBox.Show("请选择数据库地址！", "提示");
                return false;
            }
            if (mysql.dbConfig == null)
            {
                MessageBox.Show("设置数据库配置失败！", "提示");
                return false;
            }
            return true;
        }

        private void loadMyLockTbl()
        {
            List<string> retTbl = null;
            mysql.QueryMyLockTbl(out retTbl);
            if (retTbl.Count == 0)
            {
                return;
            }

            //处理方式一：上次没有解锁的直接解锁
            myLockList.Clear();
            myLockList = new List<string>(retTbl.ToArray());
            unlockTable();

            //处理方式二
            //for (int i = 0; i < retTbl.Count; i++)
            //{
            //    fileList.Items.Add(retTbl[i]);
            //    fileList.SetItemChecked(fileList.Items.Count - 1, true);
            //}
            //myLockList.Clear();
            //myLockList = new List<string>(retTbl.ToArray());
            //lockAllBtn(2);
        }

        private bool checkUserName()
        {
            if (mysql.UserName == "")
            {
                MessageBox.Show("用户名不能为空", "提示");
                return false;
            }
            string pattern = @"^[a-zA-Z]*$";
            Regex regex = new Regex(pattern);
            if (!regex.IsMatch(mysql.UserName))
            {
                MessageBox.Show("用户名只能包含字母：" + mysql.UserName, "提示");
                return false;
            }
            return true;
        }
    }
}
