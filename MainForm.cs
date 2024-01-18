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
        #region ���������캯��
        private float angle;
        Dictionary<string, string> translateData = new Dictionary<string, string>();
        public MainForm()
        {
            InitializeComponent();

            // ��ȡ��ǰ��ʾ������������
            using (Graphics graphics = this.CreateGraphics())
            {
                float dpiX = graphics.DpiX;
                float dpiY = graphics.DpiY;

                // ���� DPI ֵ������������
                float scaleFactor = dpiX / 86f;

                // �������ڵĴ�С
                this.Width = (int)(this.Width * scaleFactor);
                this.Height = (int)(this.Height * scaleFactor);

                // ���������еĿؼ��������С
                AdjustControlsAndFonts(this.Controls, scaleFactor);
            }
        }

        // �ݹ�����ؼ��������С
        private void AdjustControlsAndFonts(Control.ControlCollection controls, float scale)
        {
            foreach (Control control in controls)
            {
                // �����ؼ��Ĵ�С
                control.Scale(new SizeF(scale, scale));

                // �����ؼ��������С
                control.Font = new Font(control.Font.FontFamily, control.Font.Size * scale, control.Font.Style);

                // ����ؼ������ӿؼ�����ݹ�����ӿؼ��Ĵ�С������
                if (control.Controls.Count > 0)
                {
                    AdjustControlsAndFonts(control.Controls, scale);
                }
            }
        }
        #endregion

        #region �ؼ��¼�        

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
            // ��ʾ�ļ�������Ի���
            DialogResult result = folderBrowserDialog1.ShowDialog();

            // ����û��Ƿ�ѡ�����ļ���
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
            {
                // ��ȡ�û�ѡ����ļ���·��
                string selectedFolder = folderBrowserDialog1.SelectedPath;
                txtTranslatorFolder.Text = selectedFolder;
                ShowAllXml(selectedFolder);
            }
        }
        private void butUpdateDict_Click(object sender, EventArgs e)
        {
            rtbResult.Text = string.Empty;
            rtbLog.Text = string.Empty;
            Log($"�������ֵ��ļ�����{translateData.Count} ��");
            var xmlData = LoadXmlData(txtTranslatorFolder.Text).Where(k => translateData.ContainsKey(k) == false);
            var source = new ConcurrentQueue<string>(xmlData);
            Log($"����ȴ��������乲�ƣ�{source.Count()} ��");
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
            Log("�����ֵ����", Color.Green);
        }

        private void btuStartTranslation_Click(object sender, EventArgs e)
        {
            rtbResult.Text = string.Empty;
            rtbLog.Text = string.Empty;
            TranslateXml(translateData, txtTranslatorFolder.Text);
            Log("������ɣ����޴��󣬽��Զ��ú���Ŀ¼����Ŀ���ļ���", Color.Green);
        }
        private void lblStatus_Paint(object sender, PaintEventArgs e)
        {
            // ��ȡlblStatus �� Graphics ����
            Graphics g = e.Graphics;

            // �����ͼ����
            g.Clear(lblStatus.BackColor);

            // ���ÿ����ģʽ
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // ������ת�ǶȺ���ת���ĵ�
            PointF center = new PointF(lblStatus.Width / 2, lblStatus.Height / 2);

            // Ӧ����ת�任
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(angle);
            g.TranslateTransform(-center.X, -center.Y);

            // ������ת����ַ�
            g.DrawString(lblStatus.Text, lblStatus.Font, Brushes.Black, 0, 0);
        }
        private async void MainForm_Load(object sender, EventArgs e)
        {
            lblOfflineDict.Text = "�����С�����";
            // ������ʱ��
            timer1.Start();
            // �첽����LoadTranslateData����
            await Task.Run(() => RefreshDict()).ConfigureAwait(false); //���õȴ��ߣ���Щ�ȴ��Ž��������̶߳��������߳�            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            angle += 5f;            //������ת�Ƕ�    
            lblStatus.Invalidate(); //�ػ�lblStatus
        }

        //await Task.Run(() => GetAllXml()).ConfigureAwait(false);       
        void ShowAllXml(string path)
        {
            Invoke((MethodInvoker)(() => rtbResult.Text = string.Empty));
            foreach (var filename in Directory.GetFiles(path, @"*.XML", SearchOption.AllDirectories))
            // �����߳��ϸ���
            Invoke((MethodInvoker)(() => rtbResult.AppendText(filename+"\r\n")));
           
        }
        void RefreshDict()
        {
            translateData = LoadTranslateData();
            // �����߳��ϸ���
            Invoke((MethodInvoker)(() =>
            {
                lblOfflineDict.Text = $"���������ֵ�����{translateData.Count} ��";
                lblStatus.Text = "�e";
                timer1.Stop();
            }));
        }
        
        
        #endregion

        #region ������ط���
        /// <summary>
        /// ��ȡxml
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
        /// ��������������
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
        /// ���ж���ȡ���ַ���
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
            Log("д��json�ļ����");
        }

        /// <summary>
        /// ʹ���ֵ��ļ�,����ָ��Ŀ¼�������ļ�
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
                    Log("��� " + fileInfo.Name);
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
                        // �޸ġ����ӣ���ֻ��һ����ʱ�����з������з��š���ʾ�Ѻ������������η���
                        if (text == "." || text.Contains("��"))
                            continue;
                        if (dic.ContainsKey(text))
                        {
                            var t = dic[text];
                            if (CompareStrings(t, text))
                                continue;
                        }

                        //�޸ģ�ƥ���ֵ䣬���򺺻������ں�������������ӡ����Ա��Ϊ��������
                        if (dic.ContainsKey(text))
                        {
                            item.Value = $"��{dic[text]}��{text}"; // dic[text] + "\r\n" + text;                           
                            rtbResult.AppendText($"{text.Trim()} ==> ");
                            rtbResult.SelectionColor = Color.Green;
                            rtbResult.AppendText($"{dic[text].Trim()}\r\n");
                            rtbResult.SelectionColor = rtbResult.ForeColor;
                        }
                    }
                    doc.Save(outFilename);
                    // ��Ŀ��Ŀ¼�ڴα���ԭ�ļ���Ȼ�����滻Ϊ����xml
                    Backup_Copy_xml(fileInfo.FullName, outFilename);
                }
                catch (Exception ex)
                {
                    Log(ex.Message, Color.Red);
                }
            }
        }

        /// <summary>
        /// ����xml�ļ�,�������ظ������
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

        // �Ƿ�ΪXML��ʽ�ĸ�֪�ļ�
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
        /// �����Ѿ�������������ֵ�
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
                    Log($"����{filename}�ļ������쳣:{ex.Message}\r\n{ex.StackTrace}", Color.Red);
                }
            }
            return result;
        }

        /// <summary>
        /// �ж��ַ������Ƿ��������
        /// </summary>
        /// <param name="str">��Ҫ�жϵ��ַ���</param>
        /// <returns>�жϽ��</returns>
        public bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        /// <summary>
        /// ��ͬ��Ŀ¼����ԭʼ�ļ�����Ӧ�÷�����XML
        /// </summary>
        /// <returns></returns>
        public bool Backup_Copy_xml(string en_file, string outFilename)
        {
            if (chkReplace.Checked == false)
            {
                Log("�ֶ�״̬�������Զ��滻Ŀ���ļ���", Color.Green);
                return false;
            }
            string bakfile = $"{en_file}.en.bak";
            if (File.Exists(bakfile))
            {
                Log($"Ŀ���ļ� {Path.GetFileName(en_file)} �ѱ��ݣ������ڱ���");

            }
            else
            {
                try
                {
                    File.Copy(en_file, bakfile, false);   //����
                }
                catch (Exception ex)
                {
                    Log($"����Ŀ���ļ� {Path.GetFileName(en_file)} ���ݵ� {bakfile} ʱ����������ȷ���й���ԱȨ�޺����ԣ�", Color.Red);

                }
                if (!File.Exists(bakfile))
                {
                    Log("Ŀ��ԭ�ļ�����ʧ�ܣ�", Color.Red);
                    return false;
                }
                Log("Ŀ���ļ����ݳɹ���", Color.Green);

            }
            //����
            try
            {
                File.Copy(outFilename, en_file, true);
            }
            catch (Exception ex)
            {
                Log("�޷��滻Ŀ���ļ�" + ex.Message, Color.Red);
                return false;
            }

            // ɾ������Ŀ¼�еĺ����ļ�
            try
            {
                File.Delete(outFilename);
                Log("���ǳɹ�", Color.Green);
            }
            catch (Exception ex)
            {
                Log("ɾ������Ŀ¼�еĺ����ļ���" + ex.Message, Color.Red);
                return false;
            }
            return true;
        }

        /// <summary>
        /// ����Ӣ�ĺ����ĵı�����
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        bool CompareStrings(string str1, string str2)
        {
            // �Ƴ�����Ӣ�ı����ź����ı�����
            string pattern = @"[\p{P}\r\n]";// @"[\p{P}-[,.��������]]";
            string cleanStr1 = Regex.Replace(str1, pattern, "");
            string cleanStr2 = Regex.Replace(str2, pattern, "");

            // ���Դ�Сд�Ƚ�
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
            // �ָ���ɫΪĬ����ɫ
            rtbLog.SelectionColor = rtbLog.ForeColor;
        }
        #endregion        
    }
}
