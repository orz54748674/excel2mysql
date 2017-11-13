namespace Excel2Mysql
{
    partial class Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnUpload = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.fileList = new System.Windows.Forms.CheckedListBox();
            this.btnInverse = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.showProgress = new System.Windows.Forms.ProgressBar();
            this.showLabel = new System.Windows.Forms.Label();
            this.btnLock = new System.Windows.Forms.Button();
            this.updateLabel = new System.Windows.Forms.Label();
            this.dbList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(9, 255);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(50, 30);
            this.btnSelectAll.TabIndex = 0;
            this.btnSelectAll.Text = "全选";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(340, 327);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(65, 30);
            this.btnUpload.TabIndex = 0;
            this.btnUpload.Text = "上传表";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // fileList
            // 
            this.fileList.FormattingEnabled = true;
            this.fileList.Location = new System.Drawing.Point(9, 5);
            this.fileList.Name = "fileList";
            this.fileList.Size = new System.Drawing.Size(396, 244);
            this.fileList.TabIndex = 3;
            this.fileList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.list_MouseDown);
            // 
            // btnInverse
            // 
            this.btnInverse.Location = new System.Drawing.Point(65, 255);
            this.btnInverse.Name = "btnInverse";
            this.btnInverse.Size = new System.Drawing.Size(50, 30);
            this.btnInverse.TabIndex = 0;
            this.btnInverse.Text = "反选";
            this.btnInverse.UseVisualStyleBackColor = true;
            this.btnInverse.Click += new System.EventHandler(this.btnInverse_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(9, 290);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(50, 30);
            this.btnOpen.TabIndex = 4;
            this.btnOpen.Text = "打开";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(269, 327);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(65, 30);
            this.btnDownload.TabIndex = 5;
            this.btnDownload.Text = "下载表";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // showProgress
            // 
            this.showProgress.Location = new System.Drawing.Point(124, 290);
            this.showProgress.Name = "showProgress";
            this.showProgress.Size = new System.Drawing.Size(281, 30);
            this.showProgress.TabIndex = 6;
            // 
            // showLabel
            // 
            this.showLabel.AutoSize = true;
            this.showLabel.BackColor = System.Drawing.Color.Transparent;
            this.showLabel.Location = new System.Drawing.Point(122, 264);
            this.showLabel.Name = "showLabel";
            this.showLabel.Size = new System.Drawing.Size(53, 12);
            this.showLabel.TabIndex = 7;
            this.showLabel.Text = "操作表：";
            // 
            // btnLock
            // 
            this.btnLock.Location = new System.Drawing.Point(65, 290);
            this.btnLock.Name = "btnLock";
            this.btnLock.Size = new System.Drawing.Size(50, 30);
            this.btnLock.TabIndex = 8;
            this.btnLock.Text = "锁表";
            this.btnLock.UseVisualStyleBackColor = true;
            this.btnLock.Click += new System.EventHandler(this.btnLock_Click);
            // 
            // updateLabel
            // 
            this.updateLabel.AutoSize = true;
            this.updateLabel.Location = new System.Drawing.Point(181, 264);
            this.updateLabel.Name = "updateLabel";
            this.updateLabel.Size = new System.Drawing.Size(0, 12);
            this.updateLabel.TabIndex = 9;
            // 
            // dbList
            // 
            this.dbList.DisplayMember = "1";
            this.dbList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dbList.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dbList.FormattingEnabled = true;
            this.dbList.Location = new System.Drawing.Point(124, 333);
            this.dbList.Name = "dbList";
            this.dbList.Size = new System.Drawing.Size(139, 20);
            this.dbList.TabIndex = 11;
            this.dbList.SelectedIndexChanged += new System.EventHandler(this.dbList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 337);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "选择数据库地址：";
            // 
            // Main
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(413, 364);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dbList);
            this.Controls.Add(this.updateLabel);
            this.Controls.Add(this.btnLock);
            this.Controls.Add(this.showLabel);
            this.Controls.Add(this.showProgress);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.fileList);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.btnInverse);
            this.Controls.Add(this.btnSelectAll);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Excel2Mysql";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formCloseUnlockTable);
            this.Load += new System.EventHandler(this.mainLoad);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.fileDragDrop);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnUpload;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.CheckedListBox fileList;
        private System.Windows.Forms.Button btnInverse;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.ProgressBar showProgress;
        private System.Windows.Forms.Label showLabel;
        private System.Windows.Forms.Button btnLock;
        private System.Windows.Forms.Label updateLabel;
        private System.Windows.Forms.ComboBox dbList;
        private System.Windows.Forms.Label label1;


    }
}

