using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Diagnostics;

namespace RIG
{
    public partial class Form1 : Form
    {
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        //SpeechRecognizer recEngine = new SpeechRecognizer();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadGrammar();

            recEngine.SetInputToDefaultAudioDevice();
            recEngine.SpeechRecognized += RecEngine_SpeechRecognized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;

            if (textBox1.Text == "")
                MessageBox.Show("No input", "FAIL");

            else
            {
                string html = GetHtmlCode(textBox1.Text);
                List<string> urls = GetUrls(html);
                var rnd = new Random();

                int randomUrl = rnd.Next(0, urls.Count - 1);

                string luckyUrl = urls[randomUrl];

                byte[] image = GetImage(luckyUrl);
                pictureBox1.Image = toImage(image);

                //pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null || pictureBox1 == null)
                MessageBox.Show("No image present", "FAIL");

            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "jpg|*.jpg|bmp|*.bmp|gif|*.gif";
                ImageFormat format = ImageFormat.Jpeg;

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string ext = System.IO.Path.GetExtension(sfd.FileName);
                    switch (ext)
                    {
                        case ".jpg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                        case ".gif":
                            format = ImageFormat.Gif;
                            break;
                    }
                }
                string filePath = sfd.FileName.ToString();

                Image tmp = pictureBox1.Image;
                tmp.Save(filePath);
                sfd.Dispose();
            }
        }

        private Image toImage(byte[] rawImage)
        {
            pictureBox1.Image = null;
            var stream = new MemoryStream(rawImage);
            return Image.FromStream(stream);
        }

        private string GetHtmlCode(string text)
        {

            string url = "https://www.google.com/search?q=" + text + "&tbm=isch";
            string data = "";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

            var response = (HttpWebResponse)request.GetResponse();

            using (Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return "";
                using (var sr = new StreamReader(dataStream))
                {
                    data = sr.ReadToEnd();
                }
            }
            return data;
        }

        private List<string> GetUrls(string html)
        {
            var urls = new List<string>();

            int ndx = html.IndexOf("\"ou\"", StringComparison.Ordinal);

            while (ndx >= 0)
            {
                ndx = html.IndexOf("\"", ndx + 4, StringComparison.Ordinal);
                ndx++;
                int ndx2 = html.IndexOf("\"", ndx, StringComparison.Ordinal);
                string url = html.Substring(ndx, ndx2 - ndx);
                urls.Add(url);
                ndx = html.IndexOf("\"ou\"", ndx2, StringComparison.Ordinal);
            }
            return urls;
        }

        private byte[] GetImage(string url)
        {
            var ctr = 0;

            while (ctr < 5)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    var response = (HttpWebResponse)request.GetResponse();

                    using (Stream dataStream = response.GetResponseStream())
                    {
                        if (dataStream == null)
                            return null;

                        using (var sr = new BinaryReader(dataStream))
                        {
                            byte[] bytes = sr.ReadBytes(100000000);

                            return bytes;
                        }
                    }
                }
                catch (Exception e)
                {
                    ctr++;
                    System.Threading.Thread.Sleep(250);
                    continue;
                }
            }
            

            return null;
        }
        
        private void RecEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Text.ToString() == "delete" || e.Result.Text.ToString() == "Delete")
                textBox1.Text = "";

            textBox1.Text = e.Result.Text.ToString();
        }

        private void LoadGrammar()
        {
            Choices texts = new Choices();
            string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\dictionary.txt");
            texts.Add(lines);
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(texts);
            Grammar wordList = new Grammar(gb);
            recEngine.LoadGrammarAsync(wordList);
            /*DictationGrammar diction = new DictationGrammar();
            recEngine.LoadGrammarAsync(diction);*/
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                recEngine.RecognizeAsync(RecognizeMode.Multiple);

            else
                recEngine.RecognizeAsyncStop();
        }
    }
}
