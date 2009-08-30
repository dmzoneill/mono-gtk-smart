using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Drawing;
using System.Text.RegularExpressions;


namespace Smart
{	
	public class SmartTalker
	{
		
		private string forum_user;
		private string forum_password;
		private string modem_user;
		private string modem_password;
		private string usageDetails;
		private CookieContainer vbCookie = new CookieContainer();
		public string[] summary { get; set;}
		
		public SmartTalker(string vbuser, string vbpasswd, string modemuser, string modempasswd)
		{
			this.forum_user = vbuser;
			this.forum_password = vbpasswd;
			this.modem_user = modemuser;
			this.modem_password = modempasswd;
		}
		
		public bool vbLogin()
		{
			string code = "";
			
			try
			{			
				
				ASCIIEncoding encoding = new ASCIIEncoding();
				string md5Pass = GetMD5Hash(this.forum_password);
				string postData = "do=login&url=%2Findex.php&vb_login_md5password=" + md5Pass + "&vb_login_username=" + this.forum_user + "&cookieuser=1";
				byte[] data = encoding.GetBytes(postData);			
				
				string forums = "http://support.smarttelecom.ie/forums/login.php?do=login";
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create(forums);
				request.AllowAutoRedirect = true;
				request.CookieContainer = this.vbCookie;
				request.Method = "POST";
				request.ContentType="application/x-www-form-urlencoded";
				request.UserAgent = "Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.9.1.4pre) Gecko/20090829 Ubuntu/9.04 (jaunty) Shiretoko/3.5.4pre";
				request.ContentLength = data.Length;		
				
				// write to the stream			
				Stream newStream = request.GetRequestStream();
				newStream.Write(data,0,data.Length);
				newStream.Close();
				
				// read from the stream
				HttpWebResponse WebResp = (HttpWebResponse)request.GetResponse();			
				Stream htmlStream = WebResp.GetResponseStream();
	    		StreamReader html = new StreamReader(htmlStream);
				code = html.ReadToEnd();
				
			}
			catch {}
			
			if(code.Contains("Thank you for logging in"))
			{
				return true;
			}
			else 
			{			
				return false;	
			}
		}
		
		public bool bbUsageLogin()
		{
			string code = "";
			
			try
			{
			
				ASCIIEncoding encoding = new ASCIIEncoding();
				string postData = "user=" + this.modem_user + "&pass=" + this.modem_password + "&submit=SUBMIT";
				byte[] data = encoding.GetBytes(postData);			
				
				string forums = "http://support.smarttelecom.ie/forums/smart_usage";
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create(forums);
				request.AllowAutoRedirect = true;
				request.CookieContainer = this.vbCookie;
				request.Method = "POST";
				request.ContentType="application/x-www-form-urlencoded";
				request.UserAgent = "Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.9.1.4pre) Gecko/20090829 Ubuntu/9.04 (jaunty) Shiretoko/3.5.4pre";
				request.ContentLength = data.Length;		
				
				// write to the stream			
				Stream newStream = request.GetRequestStream();
				newStream.Write(data,0,data.Length);
				newStream.Close();
				
				// read from the stream
				HttpWebResponse WebResp = (HttpWebResponse)request.GetResponse();			
				Stream htmlStream = WebResp.GetResponseStream();
	    		StreamReader html = new StreamReader(htmlStream);
				code = html.ReadToEnd();
				
			}
			catch {}
			
			if(code.Contains("USAGE DATA FOR USERNAME"))
			{
				this.usageDetails = code;
				return true;
			}
			else 
			{			
				return false;	
			}	
		}
		
		public void parseBBUsage()
		{
			string source = this.usageDetails;
			string[] startSplit = new string[] {"SUMMARY"};
			string[] endSplit = new string[] {"DETAIL"};
			string[] result;
			string[] result2;
			string data;

			result = source.Split(startSplit, StringSplitOptions.None);
			result2 = result[1].Split(endSplit, StringSplitOptions.RemoveEmptyEntries);
			
			data = result2[0];
			
			string[] lines = Regex.Split(this.StripHTML("SUMMARY" + data ), "\n");

			string[] list = new string[16];	
			string trimline;
			
			int x = 0;
			
        	foreach (string line in lines)
        	{
				trimline = line.Trim();
				if(trimline.Length > 0)
				{	
            		list[x] = trimline;
					x++;
				}
        	}

			this.summary = list;	
			
		}
		
		
		public void grabUsageImages()
		{
			try
			{
				
				string sumimage = "http://support.smarttelecom.ie/forums/smart_usage?img=sum";
				HttpWebRequest requestImage1 = (HttpWebRequest) WebRequest.Create(sumimage);
				requestImage1.AllowAutoRedirect = true;
				requestImage1.CookieContainer = this.vbCookie;
				requestImage1.UserAgent = "Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.9.1.4pre) Gecko/20090829 Ubuntu/9.04 (jaunty) Shiretoko/3.5.4pre";
				
				HttpWebResponse WebResp1 = (HttpWebResponse)requestImage1.GetResponse();			
				System.Drawing.Image webImage1 = Image.FromStream(WebResp1.GetResponseStream());		
				webImage1.Save("sum.png");
				
				
				string dailyimage = "http://support.smarttelecom.ie/forums/smart_usage?img=daily";
				HttpWebRequest requestImage2 = (HttpWebRequest) WebRequest.Create(dailyimage);
				requestImage2.AllowAutoRedirect = true;
				requestImage2.CookieContainer = this.vbCookie;
				requestImage2.UserAgent = "Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.9.1.4pre) Gecko/20090829 Ubuntu/9.04 (jaunty) Shiretoko/3.5.4pre";
				
				HttpWebResponse WebResp2 = (HttpWebResponse)requestImage2.GetResponse();			
				System.Drawing.Image webImage2 = Image.FromStream(WebResp2.GetResponseStream());		
				webImage2.Save("daily.png");
				
			}
			catch {}	
		}
			
		
		public string GetMD5Hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }
		
		
		public string StripHTML (string inputString)
		{
			string HTML_TAG_PATTERN = "<.*?>";
			
   			return Regex.Replace (inputString, HTML_TAG_PATTERN, string.Empty);
		}
	}
}
