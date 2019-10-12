using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using Maticsoft.Common;

namespace HttpMessageTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string url = "https://zehusdrivermanufacturer-integr.azurewebsites.net/index.html";
            url = "https://zehusdrivermanufacturer-integr.azurewebsites.net/api/v1/FirmwareProductTypes?page=1&rows=5";
            string urlLogin = "https://zehusidentityserverwebintegration.azurewebsites.net/connect/authorize/";
            ///api/v{version}/FirmwareProductTypes?page={page}&rows={rows}
            string paramstr = "/api/v{version}/FirmwareProductTypes?page={page}&rows={rows}";
            Maticsoft.Common.WebClient webClient = new Maticsoft.Common.WebClient();

            urlLogin += "Username=SelcomGroupUser@grr.la&Password=%7LD2GPfNjv8";
            textBox1.Text = webClient.GetHtml(urlLogin);
            //textBox1.Text = HttpGet(url,paramstr);
        }

        static CookieContainer GetCookie(string postString, string postUrl)
        {
            CookieContainer cookie = new CookieContainer();

            HttpWebRequest httpRequset = (HttpWebRequest)HttpWebRequest.Create(postUrl);//创建http 请求
            httpRequset.CookieContainer = cookie;//设置cookie
            httpRequset.Method = "POST";//POST 提交
            httpRequset.KeepAlive = true;
            httpRequset.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            httpRequset.Accept = "text/html, application/xhtml+xml, */*";
            httpRequset.ContentType = "application/x-www-form-urlencoded";//以上信息在监听请求的时候都有的直接复制过来
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postString);
            httpRequset.ContentLength = bytes.Length;
            Stream stream = httpRequset.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();//以上是POST数据的写入
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequset.GetResponse();//获得 服务端响应
            return cookie;//拿到cookie
        }

        static string GetContent(CookieContainer cookie, string url)
        {
            string content;
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            httpRequest.CookieContainer = cookie;
            httpRequest.Referer = url;
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            httpRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Method = "GET";
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (Stream responsestream = httpResponse.GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(responsestream, System.Text.Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }
            }
            return content;
        }

        public void TestRequest()
        {
            //post请求并调用

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("id", "4");
            var postRes = GetResponseString(CreatePostHttpResponse("https://www.baidu.com/", dic));
            //get请求并调用

            var getRes = GetResponseString(CreateGetHttpResponse("https://i.cnblogs.com/EditPosts.aspx?opt=1"));
        }

        /// <summary>
        /// 发送http post请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="parameters">查询参数集合</param>
        /// <returns></returns>
        public HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;//创建请求对象
            request.Method = "POST";//请求方式
            request.ContentType = "application/x-www-form-urlencoded";//链接类型
                                                                      //构造查询字符串
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                bool first = true;
                foreach (string key in parameters.Keys)
                {

                    if (!first)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        first = false;
                    }
                }
                byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());
                //写入请求流
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 发送http Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpWebResponse CreateGetHttpResponse(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";//链接类型
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 从HttpWebResponse对象中提取响应的数据转换为字符串
        /// </summary>
        /// <param name="webresponse"></param>
        /// <returns></returns>
        public string GetResponseString(HttpWebResponse webresponse)
        {
            using (Stream s = webresponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string loginstr = @"SelcomGroupUser@grr.la\%7LD2GPfNjv8";
            //从登陆的地址获取cookie
            CookieContainer cookie = GetCookie(loginstr, "http://passport.cnblogs.com/login.aspx");
            //这个是进入后台地址
            textBox1.Text += "Content:\r\n" + GetContent(cookie, "http://i.cnblogs.com/EditPosts.aspx");
        }

        static void Mains(string[] args)
        {
            //string loginUrl = "https://zehusdrivermanufacturer-integr.azurewebsites.net/index.html";
            //string loginData = "uid=******&pwd=******";
            //CookieContainer cookies = new CookieContainer();
            //string loginResult = HttpPost(loginUrl, loginData, cookies);
            //Console.WriteLine("这是登陆后的界面信息！");
            //Console.WriteLine(loginResult);

            //预约明天的票
            //string checkUrl =
            //    "http://202.114.74.218/web3/baobiao/Queue/QueueSystem.aspx";
            //string checkData = "ImageButton2.x=167&ImageButton2.y=20&deptID=3&dateType=NextDday&timeType=AM";
            //string checkResult = HttpPost(checkUrl, checkData, cookies);
            //Console.WriteLine("这是取票结果");
            //Console.WriteLine(checkResult);
            //Console.ReadKey();
        }

        /************************************************************************/
        /* Http Get请求
         * url为请求的网址
         * data为GET请求参数（格式为：key1=value1&key2=value2）
         */
        /************************************************************************/
        public static string HttpGet(string url, string data)
        {
            url = url + "?" + data;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/hmtl;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream);
            string result = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            return result;
        }

        /************************************************************************/
        /* Http Post 请求
         * url为请求的网址
         * data为POST请求参数（格式为：key1=value1&key2=value2）
         * cookie为存储Cookie的容器CookieContainer
         */
        /************************************************************************/
        public static string HttpPost(string url, string data, CookieContainer cookies)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            //FORM元素的enctype属性指定了表单数据向服务器提交时所采用的编码类型，默认的缺省值是“application/x-www-form-urlencoded”
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(data);
            request.CookieContainer = cookies;
            Stream requetStream = request.GetRequestStream();
            StreamWriter streamWriter = new StreamWriter(requetStream);
            streamWriter.Write(data);
            streamWriter.Close();

            request.CookieContainer = cookies;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Cookies = cookies.GetCookies(response.ResponseUri);
            cookies.Add(response.Cookies);
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("GB2312"));
            string result = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            return result;
        }
    }
}
