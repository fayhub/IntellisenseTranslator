using GTranslate.Translators;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace IntellisenseTranslator
{
    public partial class MainForm : Form
    {
        #region 变量、构造函数
        private float angle;
        Dictionary<string, string> translateData = new Dictionary<string, string>();
        public MainForm()
        {
            InitializeComponent();

            // 获取当前显示器的缩放因子
            using (Graphics graphics = this.CreateGraphics())
            {
                float dpiX = graphics.DpiX;
                float dpiY = graphics.DpiY;

                // 根据 DPI 值计算缩放因子
                float scaleFactor = dpiX / 86f;

                // 调整窗口的大小
                this.Width = (int)(this.Width * scaleFactor);
                this.Height = (int)(this.Height * scaleFactor);

                // 调整窗口中的控件和字体大小
                AdjustControlsAndFonts(this.Controls, scaleFactor);
            }
        }

        // 递归调整控件和字体大小
        private void AdjustControlsAndFonts(Control.ControlCollection controls, float scale)
        {
            foreach (Control control in controls)
            {
                // 调整控件的大小
                control.Scale(new SizeF(scale, scale));

                // 调整控件的字体大小
                control.Font = new Font(control.Font.FontFamily, control.Font.Size * scale, control.Font.Style);

                // 如果控件包含子控件，则递归调整子控件的大小和字体
                if (control.Controls.Count > 0)
                {
                    AdjustControlsAndFonts(control.Controls, scale);
                }
            }
        }
        #endregion

        #region 控件事件        

        private void txtTranslatorFolder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void txtTranslatorFolder_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string directoryPath = Path.GetDirectoryName(files[0]);
                txtTranslatorFolder.Text = directoryPath;
                ShowAllXml(directoryPath);
            }
        }
        private void txtTranslatorFolder_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ShowAllXml(txtTranslatorFolder.Text);
            }
        }
        private void butOpenFolder_Click(object sender, EventArgs e)
        {
            // 显示文件夹浏览对话框
            DialogResult result = folderBrowserDialog1.ShowDialog();

            // 检查用户是否选择了文件夹
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
            {
                // 获取用户选择的文件夹路径
                string selectedFolder = folderBrowserDialog1.SelectedPath;
                txtTranslatorFolder.Text = selectedFolder;
                ShowAllXml(selectedFolder);
            }
        }
        private void butUpdateDict_Click(object sender, EventArgs e)
        {
            rtbResult.Text = string.Empty;
            rtbLog.Text = string.Empty;
            Log($"已载入字典文件共：{translateData.Count} 项");
            var xmlData = LoadXmlData(txtTranslatorFolder.Text).Where(k => translateData.ContainsKey(k) == false);
            var source = new ConcurrentQueue<string>(xmlData);
            Log($"载入等待翻译的语句共计：{source.Count()} 项");
            var thread_num = 5;
            var temp_dic = new ConcurrentDictionary<string, string>();
            var task_list = new List<Task>();
            for (int i = 0; i < thread_num; i++)
            {
                var task = new Task(() =>
                {
                    var translator = new AggregateTranslator();
                    while (source.Count > 0)
                    {
                        try
                        {
                            var sb = new StringBuilder();
                            var array = DequeueArray(source, 4000);
                            foreach (var item in array)
                            {
                                sb.AppendLine(item);
                                sb.AppendLine("@@@@");
                            }

                            var result = translator.TranslateAsync(sb.ToString(), "zh-cn").Result;
                            if (result == null || string.IsNullOrWhiteSpace(result.Translation))
                                continue;

                            var result_dic = AnalyzeText(array, result.Translation);
                            foreach (var item in result_dic)
                            {
                                Log($"{temp_dic.Count}/{source.Count}\t{item.Key}\t{item.Value}");
                                translateData[item.Key] = item.Value;
                                temp_dic[item.Key] = item.Value;
                                if (temp_dic.Count > 10000)
                                {
                                    lock (temp_dic)
                                    {
                                        if (temp_dic.Count > 10000)
                                        {
                                            var dic2 = temp_dic;
                                            temp_dic = new ConcurrentDictionary<string, string>();
                                            SaveDataFile(dic2);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log(ex.Message, Color.Red);
                        }
                    }
                }, TaskCreationOptions.LongRunning);
                task.Start();
                task_list.Add(task);
            }

            Task.WaitAll(task_list.ToArray());

            if (temp_dic.Count >= 0)
            {
                SaveDataFile(temp_dic);
                temp_dic.Clear();
            }
            RefreshDict();
            Log("更新字典完成", Color.Green);
        }

        private void btuStartTranslation_Click(object sender, EventArgs e)
        {
            rtbResult.Text = string.Empty;
            rtbLog.Text = string.Empty;
            TranslateXml(translateData, txtTranslatorFolder.Text);
            Log("翻译完成，如无错误，将自动用汉化目录覆盖目标文件。", Color.Green);
        }
        private void lblStatus_Paint(object sender, PaintEventArgs e)
        {
            // 获取lblStatus 的 Graphics 对象
            Graphics g = e.Graphics;

            // 清除绘图表面
            g.Clear(lblStatus.BackColor);

            // 设置抗锯齿模式
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 设置旋转角度和旋转中心点
            PointF center = new PointF(lblStatus.Width / 2, lblStatus.Height / 2);

            // 应用旋转变换
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(angle);
            g.TranslateTransform(-center.X, -center.Y);

            // 绘制旋转后的字符
            g.DrawString(lblStatus.Text, lblStatus.Font, Brushes.Black, 0, 0);
        }
        private async void MainForm_Load(object sender, EventArgs e)
        {
            lblOfflineDict.Text = "加载中。。。";
            // 启动定时器
            timer1.Start();
            // 异步调用LoadTranslateData方法
            await Task.Run(() => RefreshDict()).ConfigureAwait(false); //设置等待者，这些等待着将是任务线程而不是主线程            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            angle += 5f;            //更新旋转角度    
            lblStatus.Invalidate(); //重绘lblStatus
        }

        //await Task.Run(() => GetAllXml()).ConfigureAwait(false);       
        void ShowAllXml(string path)
        {
            Invoke((MethodInvoker)(() => rtbResult.Text = string.Empty));
            foreach (var filename in Directory.GetFiles(path, @"*.XML", SearchOption.AllDirectories))
            // 在主线程上更新
            Invoke((MethodInvoker)(() => rtbResult.AppendText(filename+"\r\n")));
           
        }
        void RefreshDict()
        {
            translateData = LoadTranslateData();
            // 在主线程上更新
            Invoke((MethodInvoker)(() =>
            {
                lblOfflineDict.Text = $"本地离线字典数：{translateData.Count} 项";
                lblStatus.Text = "卐";
                timer1.Stop();
            }));
        }
        
        
        #endregion

        #region 翻译相关方法
        /// <summary>
        /// 读取xml
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public IEnumerable<XmlNode> ReadXmlNodes(XmlNode node)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                if (item.ChildNodes.Count > 0)
                    foreach (XmlNode sub_item in ReadXmlNodes(item))
                    {
                        if (sub_item.Value != null && sub_item.NodeType == XmlNodeType.Text && (item.ParentNode == null || item.ParentNode.Name != "name"))
                            yield return sub_item;
                    }
                else if (item.Value != null && item.NodeType == XmlNodeType.Text && (item.ParentNode == null || item.ParentNode.Name != "name"))
                    yield return item;
            }
        }

        /// <summary>
        /// 解析翻译后的内容
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Dictionary<string, string> AnalyzeText(string[] source, string text)
        {
            if (Regex.Matches(text, "@@@@").Count != source.Length)
                throw new ArgumentOutOfRangeException();

            var reader = new StringReader(text);
            var dic = new Dictionary<string, string>();
            var sb = new StringBuilder();
            var index = 0;
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    return dic;
                else if (line == "@@@@")
                {
                    dic[source[index++]] = sb.ToString();
                    sb.Clear();
                }
                else
                    sb.AppendLine(line);
            }
        }

        /// <summary>
        /// 从列队中取出字符串
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="limit_char_number"></param>
        /// <returns></returns>
        public string[] DequeueArray(ConcurrentQueue<string> queue, int limit_char_number)
        {
            int total_char_number = 0;
            int total_line_number = 0;
            List<string> list = new List<string>();
            while (queue.Count > 0)
            {
                if (queue.TryDequeue(out string result))
                {

                    if (result.Length > 2000)
                        continue;

                    if (total_char_number + (total_line_number * (2 + 4)) > limit_char_number)
                    {
                        queue.Enqueue(result);
                        break;
                    }
                    else
                    {
                        total_char_number += result.Length;
                        total_line_number++;
                        list.Add(result);
                    }
                }
            }
            return list.ToArray();
        }

        public void SaveDataFile(IEnumerable<KeyValuePair<string, string>> temp_dic)
        {
            File.WriteAllText($@"..\..\..\Data\{Environment.GetEnvironmentVariable("UserName")?.ToString()}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.json", JsonConvert.SerializeObject(temp_dic));
            Log("写入json文件完成");
        }

        /// <summary>
        /// 使用字典文件,翻译指定目录的所有文件
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="path"></param>
        public void TranslateXml(Dictionary<string, string> dic, string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;

            foreach (var filename in Directory.GetFiles(path, @"*.XML", SearchOption.AllDirectories))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filename);
                    if (IsIntellisenseXml(doc) == false)
                        continue;

                    var fileInfo = new FileInfo(filename);
                    Log("输出 " + fileInfo.Name);
                    var outPath = filename.Substring(path.Length, filename.Length - path.Length - fileInfo.Name.Length - 1);
                    Directory.CreateDirectory(@$".\translate\{outPath}");
                    Directory.CreateDirectory(@$".\backup\{outPath}");
                    var outFilename = @$".\translate\{outPath}\{fileInfo.Name}";
                    var backupFilename = @$".\backup\{outPath}\{fileInfo.Name}";
                    doc.Save(backupFilename);

                    foreach (var item in ReadXmlNodes(doc))
                    {
                        if (item.Value == null)
                            continue;
                        var text = item.Value;
                        // 修改、增加：当只有一个点时不进行翻译且有符号【表示已汉化过，禁二次翻译
                        if (text == "." || text.Contains("【"))
                            continue;
                        if (dic.ContainsKey(text))
                        {
                            var t = dic[text];
                            if (CompareStrings(t, text))
                                continue;
                        }

                        //修改：匹配字典，有则汉化，且在汉化内容两边添加【】以标记为汉化内容
                        if (dic.ContainsKey(text))
                        {
                            item.Value = $"【{dic[text]}】{text}"; // dic[text] + "\r\n" + text;                           
                            rtbResult.AppendText($"{text.Trim()} ==> ");
                            rtbResult.SelectionColor = Color.Green;
                            rtbResult.AppendText($"{dic[text].Trim()}\r\n");
                            rtbResult.SelectionColor = rtbResult.ForeColor;
                        }
                    }
                    doc.Save(outFilename);
                    // 在目标目录在次备份原文件，然后在替换为汉化xml
                    Backup_Copy_xml(fileInfo.FullName, outFilename);
                }
                catch (Exception ex)
                {
                    Log(ex.Message, Color.Red);
                }
            }
        }

        /// <summary>
        /// 载入xml文件,并过滤重复的语句
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> LoadXmlData(string path)
        {
            var hash = new HashSet<string>();

            Dictionary<string, string> dic = new Dictionary<string, string>();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;

            foreach (var fileName in Directory.GetFiles(path, @"*.xml", SearchOption.AllDirectories))
            {
                Log($"load {fileName}");
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);
                    if (IsIntellisenseXml(doc) == false)
                        continue;
                    foreach (var item in ReadXmlNodes(doc))
                    {
                        if (item.Value == null)
                            continue;
                        if (HasChinese(item.Value) == false)
                            hash.Add(item.Value);
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.Message, Color.Red);
                }
            }
            return hash;
        }

        public XmlNode FindXmlNote(XmlNodeList nodes, string name)
        {
            foreach (XmlNode item in nodes)
            {
                if (item.Name == name)
                    return item;
            }
            return null;
        }

        // 是否为XML格式的感知文件
        public bool IsIntellisenseXml(XmlDocument doc)
        {
            var doc_node = FindXmlNote(doc.ChildNodes, "doc");
            if (doc_node == null)
                return false;

            var assembly_node = FindXmlNote(doc_node.ChildNodes, "assembly");
            if (assembly_node == null)
                return false;

            if (FindXmlNote(assembly_node.ChildNodes, "name") == null)
                return false;

            if (FindXmlNote(doc_node.ChildNodes, "members") == null)
                return false;

            return true;
        }

        /// <summary>
        /// 载入已经翻译过的数据字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> LoadTranslateData()
        {
            var result = new Dictionary<string, string>();
            foreach (var filename in Directory.GetFiles(@"..\..\..\Data\"))
            {
                try
                {
                    var json = File.ReadAllText(filename);
                    var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    foreach (var item in items.Where(k => string.IsNullOrWhiteSpace(k.Value) == false))
                        result[item.Key] = item.Value;
                }
                catch (Exception ex)
                {
                    Log($"载入{filename}文件出现异常:{ex.Message}\r\n{ex.StackTrace}", Color.Red);
                }
            }
            return result;
        }

        /// <summary>
        /// 判断字符串中是否包含中文
        /// </summary>
        /// <param name="str">需要判断的字符串</param>
        /// <returns>判断结果</returns>
        public bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        /// <summary>
        /// 在同级目录备份原始文件，并应用翻译后的XML
        /// </summary>
        /// <returns></returns>
        public bool Backup_Copy_xml(string en_file, string outFilename)
        {
            if (chkReplace.Checked == false)
            {
                Log("手动状态，不会自动替换目标文件。", Color.Green);
                return false;
            }
            string bakfile = $"{en_file}.en.bak";
            if (File.Exists(bakfile))
            {
                Log($"目标文件 {Path.GetFileName(en_file)} 已备份，不用在备份");

            }
            else
            {
                try
                {
                    File.Copy(en_file, bakfile, false);   //备份
                }
                catch (Exception ex)
                {
                    Log($"备份目标文件 {Path.GetFileName(en_file)} 备份到 {bakfile} 时发生错误，请确保有管理员权限后重试！", Color.Red);

                }
                if (!File.Exists(bakfile))
                {
                    Log("目标原文件备份失败！", Color.Red);
                    return false;
                }
                Log("目标文件备份成功！", Color.Green);

            }
            //覆盖
            try
            {
                File.Copy(outFilename, en_file, true);
            }
            catch (Exception ex)
            {
                Log("无法替换目标文件" + ex.Message, Color.Red);
                return false;
            }

            // 删除翻译目录中的汉化文件
            try
            {
                File.Delete(outFilename);
                Log("覆盖成功", Color.Green);
            }
            catch (Exception ex)
            {
                Log("删除翻译目录中的汉化文件：" + ex.Message, Color.Red);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 忽略英文和中文的标点符号
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        bool CompareStrings(string str1, string str2)
        {
            // 移除所有英文标点符号和中文标点符号
            string pattern = @"[\p{P}\r\n]";// @"[\p{P}-[,.，。！？]]";
            string cleanStr1 = Regex.Replace(str1, pattern, "");
            string cleanStr2 = Regex.Replace(str2, pattern, "");

            // 忽略大小写比较
            bool r = string.Equals(cleanStr1, cleanStr2, StringComparison.OrdinalIgnoreCase);
            return r;
        }
        void Log(string msg)
        {
            rtbLog.AppendText(msg);
            rtbLog.AppendText("\r\n");
        }
        void Log(string msg, Color color)
        {
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(msg);
            rtbLog.AppendText("\r\n");
            // 恢复颜色为默认颜色
            rtbLog.SelectionColor = rtbLog.ForeColor;
        }
        #endregion        
    }
}
