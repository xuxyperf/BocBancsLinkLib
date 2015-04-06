using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net.Cache;

namespace BocBancsLinkPfLib
{
    public class BancsLinkHttpPf
    {
        private object sync_SaveData = new object();
        private TripleDESCryptoServiceProvider cryptprovider = new TripleDESCryptoServiceProvider();
        private TcpClient activeClient = null;
        private NetworkStream activeStream = null;

        #region Public Methods

        public string BLPHttpBot(string xmlMsg, string uri, string mothod, string contentType,string referer)
        {
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            try
            {
                string response = string.Empty;
                #if DEBUG
                string sendTime = string.Empty, recevieTime = string.Empty;
                #endif
                xmlMsg = xmlMsg.Substring(xmlMsg.IndexOf("=") + 1);
                uri = uri.Substring(uri.IndexOf("=") + 1);
                mothod = mothod.Substring(mothod.IndexOf("=") + 1);
                contentType = contentType.Substring(contentType.IndexOf("=") + 1);
                referer = referer.Substring(referer.IndexOf("=") + 1);

                req = (HttpWebRequest)WebRequest.Create(uri);
                req.PreAuthenticate = false;
                req.ContentType = contentType;
                req.Timeout = Parameter.timeOut;
                req.AllowAutoRedirect = true;
                if(Parameter.IsUserAgentValue)
                   req.UserAgent = Parameter.UserAgent;
                req.KeepAlive = Parameter.IsKeepAlive;
                if (Parameter.IsUserCache)
                {
                    HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                    req.CachePolicy = policy;
                }
                switch (contentType)
                {
                    case "text/html":
                        {
                            req.Method = mothod;
                            #if DEBUG
                            if (Parameter.IsLog)
                                sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            if (xmlMsg.CompareTo("") != 0 && String.Compare(req.Method,"POST") == 0)
                            {
                                req.UserAgent = Parameter.UserAgent;
                                req.Accept = Parameter.ClientAccept;
                                req.ContentType = "application/x-www-form-urlencoded";
                                char[] source = xmlMsg.ToCharArray();
                                byte[] bs = Encoding.UTF8.GetBytes(xmlMsg);
                                req.ContentLength = bs.Length;
                                Stream reqstream= req.GetRequestStream();
                                reqstream.Write(bs, 0, bs.Length);
                                reqstream.Close();
                            }
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                //yyyyMMdd HHmmssfff-yyyy-MM-dd HH:mm:ss.fff
                                recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            while (rspstream != null && rspstream.CanRead)
                            {
                                StreamReader sr = new StreamReader(rspstream);
                                response = sr.ReadToEnd();
                                sr.Close();
                                rspstream.Close();
                            }
                        }
                        break;
                    case "text/css":
                        {
                            req.Method = mothod;
                            req.Referer = referer;
                            #if DEBUG
                            if (Parameter.IsLog)
                                sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            while (rspstream != null && rspstream.CanRead)
                            {
                                StreamReader sr = new StreamReader(rspstream);
                                response = sr.ReadToEnd();
                                sr.Close();
                                rspstream.Close();
                            }
                        }
                        break;
                    case "text/xml":
                        {
                            req.Method = mothod;
                            #if DEBUG
                            if (Parameter.IsLog)
                                sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            while (rspstream != null && rspstream.CanRead)
                            {
                                StreamReader sr = new StreamReader(rspstream);
                                response = sr.ReadToEnd();
                                sr.Close();
                                rspstream.Close();
                            }
                        }
                        break;
                    case "application/x-javascript":
                        {
                            req.Method = mothod;
                            req.Referer = referer;
                            #if DEBUG
                            if (Parameter.IsLog)
                                sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            while (rspstream != null && rspstream.CanRead)
                            {
                                StreamReader sr = new StreamReader(rspstream);
                                response = sr.ReadToEnd();
                                sr.Close();
                                rspstream.Close();
                            }
                        }
                        break;
                    case "application/x-www-form-urlencoded":
                        {
                            req.Referer = referer;
                            if (xmlMsg.CompareTo("") != 0)
                            {
                                req.Method = mothod;
                                xmlMsg = "ta=" + System.Web.HttpUtility.UrlEncode(xmlMsg);
                                char[] source = xmlMsg.ToCharArray();
                                byte[] bs = Encoding.UTF8.GetBytes(source);
                                Stream reqstream = req.GetRequestStream();
                                reqstream.Write(bs, 0, bs.Length);
                                reqstream.Close();
                            }
                            #if DEBUG
                            if (Parameter.IsLog)
                                sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            while (rspstream != null && rspstream.CanRead)
                            {
                                StreamReader sr = new StreamReader(rspstream);
                                response += sr.ReadToEnd();
                                sr.Close();
                                rspstream.Close();
                            }
                        }
                        break;
                    case "application/octet-stream":
                        {
                            req.Method = mothod;
                            if (xmlMsg.CompareTo("") != 0)
                            {
                                char[] source = xmlMsg.ToCharArray();
                                byte[] bs = Encoding.UTF8.GetBytes(source);
                                byte[] compressBytes = BOC.BOCGZip.Compress(bs);
                                Stream reqstream = req.GetRequestStream();
                                reqstream.Write(compressBytes, 0, compressBytes.Length);
                                reqstream.Close();
                            }
                            #if DEBUG
                            if(Parameter.IsLog)
                                 sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                 recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            byte[] dest = BOC.BOCGZip.Decompress(rspstream);
                            response = new UTF8Encoding().GetString(dest);
                        }
                        break;
                    default:
                        {
                            req.Referer = referer;
                            if (xmlMsg.CompareTo("") != 0)
                            {
                                req.Method = mothod;
                                char[] source = xmlMsg.ToCharArray();
                                byte[] bs = Encoding.UTF8.GetBytes(source);
                                Stream reqstream = req.GetRequestStream();
                                reqstream.Write(bs, 0, bs.Length);
                                reqstream.Close();
                            }
                            #if DEBUG
                            if (Parameter.IsLog)
                                sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            if (Parameter.IsUploadFile)
                            {
                                response += rsp.Headers["result"];
                            }
                            while (rspstream != null && rspstream.CanRead)
                            {
                                StreamReader sr = new StreamReader(rspstream);
                                response += sr.ReadToEnd();
                                sr.Close();
                                rspstream.Close();
                            }
                        }
                        break;
                }
                #if DEBUG
                double timeDiff = DateTimeDiff(sendTime,recevieTime);
                response = timeDiff + "|" + response;
                if (Parameter.IsLog && timeDiff <= 0.100 )
                {
                    StringBuilder sbContent = new StringBuilder();
                    sbContent.AppendLine("xmlMsg:" + xmlMsg + "\r\n");
                    sbContent.AppendLine("uri:" + uri + "\r\n");
                    sbContent.AppendLine("mothod:" + mothod + "\r\n");
                    sbContent.AppendLine("contentType:" + contentType + "\r\n");
                    sbContent.AppendLine("referer:" + referer + "\r\n");
                    sbContent.AppendLine("timeOut:" + Parameter.timeOut + "\r\n");
                    sbContent.AppendLine("response:" + response + "\r\n");
                    sbContent.AppendLine("sendTime:" + sendTime + "\r\n");
                    sbContent.AppendLine("recevieTime:" + recevieTime + "\r\n");
                    WriterLog wl = new WriterLog();
                    wl.Logger(sbContent.ToString());
                }
                #endif
                return response;//返回返回的报文内容
            }
            catch
            {
                return "1";
            }
            finally
            {
                if (rsp != null)
                {
                    rsp.Close();
                }
                if (req != null)
                {
                    req.Abort();
                }
            }
        }

        public string BLPHttpMultipart(string xmlMsg, string uri, string mothod, string contentType, string referer, string attachmentMetadata,string octetStream)
        {
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            try
            {
                string response = string.Empty;
                #if DEBUG
                string sendTime = string.Empty, recevieTime = string.Empty;
                #endif
                xmlMsg = xmlMsg.Substring(xmlMsg.IndexOf("=") + 1);
                uri = uri.Substring(uri.IndexOf("=") + 1);
                mothod = mothod.Substring(mothod.IndexOf("=") + 1);
                contentType = contentType.Substring(contentType.IndexOf("=") + 1);
                referer = referer.Substring(referer.IndexOf("=") + 1);
                attachmentMetadata = attachmentMetadata.Substring(attachmentMetadata.IndexOf("=") + 1);
                octetStream = octetStream.Substring(octetStream.IndexOf("=") + 1);
                
                string boundary= "------734775441363750000";
                string fileName = attachmentMetadata.Substring(attachmentMetadata.IndexOf("<UniqueKey>") + 11, 36);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--" + boundary);
                sb.AppendLine("Content-Disposition: form-data; name=\"xml\";filename=\"POSTDATA\"");
                sb.AppendLine("Content-Type: text/xml");
                sb.AppendLine(Environment.NewLine);
                sb.AppendLine(Environment.NewLine);
                sb.AppendLine("--" + boundary);
                sb.AppendLine("Content-Disposition: form-data; name=\"file\";filename=\"AttachmentMetadata.xml\"");
                sb.AppendLine("Content-Type: attachInfo/xml" + "\r\n");
                sb.AppendLine(Environment.NewLine);
                sb.AppendLine(attachmentMetadata);
                sb.AppendLine("--" + boundary);
                sb.AppendLine("Content-Disposition: form-data; name=\"binary\";filename=\"" + fileName + "\"");
                sb.AppendLine("Content-Type: application/octet-stream");
                sb.AppendLine(Environment.NewLine);
                sb.AppendLine(octetStream);
                sb.AppendLine(Environment.NewLine);
                sb.AppendLine("--" + boundary);
                string postData = sb.ToString();

                req = (HttpWebRequest)WebRequest.Create(uri);
                req.PreAuthenticate = false;
                req.ContentType = contentType + ";" + "boundary=" + boundary;
                req.Timeout = Parameter.timeOut;
                req.AllowAutoRedirect = true;
                if (Parameter.IsUserAgentValue)
                   req.UserAgent = Parameter.UserAgent;
                req.KeepAlive = Parameter.IsKeepAlive;
                if (Parameter.IsUserCache)
                {
                    HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                    req.CachePolicy = policy;
                }
                switch (contentType)
                {
                    case "multipart/form-data":
                        {
                            req.Referer = referer;
                            req.Method = mothod;
                            char[] source = postData.ToCharArray();
                            byte[] bs = Encoding.UTF8.GetBytes(source);
                            req.ContentLength = bs.Length;
                            BinaryWriter bw = new BinaryWriter(req.GetRequestStream());
                            bw.Write(bs);
                            bw.Close();
                            #if DEBUG
                            if (Parameter.IsLog)
                                sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            while (rspstream != null && rspstream.CanRead)
                            {
                                StreamReader sr = new StreamReader(rspstream);
                                response = sr.ReadToEnd();
                                sr.Close();
                                rspstream.Close();
                            }
                        }
                        break;
                }
                #if DEBUG
                if (Parameter.IsLog)
                {
                    StringBuilder sbContent = new StringBuilder();
                    sbContent.AppendLine("xmlMsg:" + xmlMsg + "\r\n");
                    sbContent.AppendLine("uri:" + uri + "\r\n");
                    sbContent.AppendLine("mothod:" + mothod + "\r\n");
                    sbContent.AppendLine("contentType:" + contentType + "\r\n");
                    sbContent.AppendLine("referer:" + referer + "\r\n");
                    sbContent.AppendLine("timeOut:" + Parameter.timeOut + "\r\n");
                    sbContent.AppendLine("response:" + response + "\r\n");
                    sbContent.AppendLine("sendTime:" + sendTime + "\r\n");
                    sbContent.AppendLine("recevieTime:" + recevieTime + "\r\n");
                    WriterLog wl = new WriterLog();
                    wl.Logger(sbContent.ToString());
                }
                #endif
                return response;//返回返回的报文内容
            }
            catch
            {
                return "1";
            }
            finally
            {
                if (rsp != null)
                {
                    rsp.Close();
                }
                if (req != null)
                {
                    req.Abort();
                }
            }
        }

        public string BLPHttpSoap(string xmlMsg, string uri, string mothod, string contentType, string referer,string soapAction)
        {
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            try
            {
                string response = string.Empty;
                #if DEBUG
                string sendTime = string.Empty, recevieTime = string.Empty;
                #endif
                xmlMsg = xmlMsg.Substring(xmlMsg.IndexOf("=") + 1);
                uri = uri.Substring(uri.IndexOf("=") + 1);
                mothod = mothod.Substring(mothod.IndexOf("=") + 1);
                contentType = contentType.Substring(contentType.IndexOf("=") + 1);
                referer = referer.Substring(referer.IndexOf("=") + 1);
                soapAction = soapAction.Substring(soapAction.IndexOf("=") + 1);

                req = (HttpWebRequest)WebRequest.Create(uri);
                WebHeaderCollection whc = new WebHeaderCollection();
                whc.Add("SOAPAction: "+ soapAction);
                req.PreAuthenticate = false;
                req.Timeout = Parameter.timeOut;
                req.AllowAutoRedirect = true;
                if (Parameter.IsUserAgentValue)
                    req.UserAgent = Parameter.UserAgent;
                req.KeepAlive = Parameter.IsKeepAlive;
                if (Parameter.IsUserCache)
                {
                    HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                    req.CachePolicy = policy;
                }
                switch (contentType)
                {
                    case "text/xml":
                        {
                            req.Method = mothod;
                            req.Headers = whc;
                            req.ContentType = @"text/xml; charset=utf-8";
                            char[] source = xmlMsg.ToCharArray();
                            byte[] bs = Encoding.UTF8.GetBytes(source);
                            Stream reqstream = req.GetRequestStream();
                            reqstream.Write(bs, 0, bs.Length);
                            reqstream.Close();
                            #if DEBUG
                            if (Parameter.IsLog)
                                sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            while (rspstream != null && rspstream.CanRead)
                            {
                                StreamReader sr = new StreamReader(rspstream);
                                response = sr.ReadToEnd();
                                sr.Close();
                                rspstream.Close();
                            }
                        }
                        break;
                }
                #if DEBUG
                if (Parameter.IsLog)
                {
                    StringBuilder sbContent = new StringBuilder();
                    sbContent.AppendLine("xmlMsg:" + xmlMsg + "\r\n");
                    sbContent.AppendLine("uri:" + uri + "\r\n");
                    sbContent.AppendLine("mothod:" + mothod + "\r\n");
                    sbContent.AppendLine("contentType:" + contentType + "\r\n");
                    sbContent.AppendLine("referer:" + referer + "\r\n");
                    sbContent.AppendLine("timeOut:" + Parameter.timeOut + "\r\n");
                    sbContent.AppendLine("response:" + response + "\r\n");
                    sbContent.AppendLine("sendTime:" + sendTime + "\r\n");
                    sbContent.AppendLine("recevieTime:" + recevieTime + "\r\n");
                    WriterLog wl = new WriterLog();
                    wl.Logger(sbContent.ToString());
                }
                #endif
                return response;//返回返回的报文内容
            }
            catch
            {
                return "1";
            }
            finally
            {
                if (rsp != null)
                {
                    rsp.Close();
                }
                if (req != null)
                {
                    req.Abort();
                }
            }
        }

        public string BLPHttpSoapGetSecretKey(string xmlMsg, string uri, string mothod, string contentType, string referer, string soapAction, string userName, string password, string domain)
        {
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            try
            {
                string response = string.Empty;
                #if DEBUG
                string sendTime = string.Empty, recevieTime = string.Empty;
                #endif
                xmlMsg = xmlMsg.Substring(xmlMsg.IndexOf("=") + 1);
                uri = uri.Substring(uri.IndexOf("=") + 1);
                mothod = mothod.Substring(mothod.IndexOf("=") + 1);
                contentType = contentType.Substring(contentType.IndexOf("=") + 1);
                referer = referer.Substring(referer.IndexOf("=") + 1);
                soapAction = soapAction.Substring(soapAction.IndexOf("=") + 1);
                userName = userName.Substring(userName.IndexOf("=") + 1);
                password = password.Substring(password.IndexOf("=") + 1);
                domain = domain.Substring(domain.IndexOf("=") + 1);

                req = (HttpWebRequest)WebRequest.Create(uri);
                WebHeaderCollection whc = new WebHeaderCollection();
                whc.Add("SOAPAction: "+ soapAction);
                req.PreAuthenticate = false;
                req.Timeout = Parameter.timeOut;
                req.AllowAutoRedirect = true;
                if (Parameter.IsUserAgentValue)
                    req.UserAgent = Parameter.UserAgent;
                req.KeepAlive = Parameter.IsKeepAlive;
                if (Parameter.IsUserCache)
                {
                    HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                    req.CachePolicy = policy;
                }
                if (Parameter.IsCredentials)
                {
                    //req.PreAuthenticate = true;
                    CredentialCache credCache = new CredentialCache();
                    credCache.Add(new Uri(uri), "NTLM", new NetworkCredential(userName, password, domain));
                    req.Credentials = credCache;
                }
                switch (contentType)
                {
                    case "text/xml":
                        {
                            req.Method = mothod;
                            req.Headers = whc;
                            req.ContentType = @"text/xml; charset=utf-8";
                            char[] source = xmlMsg.ToCharArray();
                            byte[] bs = Encoding.UTF8.GetBytes(source);
                            Stream reqstream = req.GetRequestStream();
                            reqstream.Write(bs, 0, bs.Length);
                            reqstream.Close();
                            #if DEBUG
                            if (Parameter.IsLog)
                                sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            rsp = (HttpWebResponse)req.GetResponse();
                            Stream rspstream = rsp.GetResponseStream();
                            #if DEBUG
                            if (Parameter.IsLog)
                                recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            #endif
                            while (rspstream != null && rspstream.CanRead)
                            {
                                StreamReader sr = new StreamReader(rspstream);
                                response = sr.ReadToEnd();
                                sr.Close();
                                rspstream.Close();
                            }
                        }
                        break;
                }
                #if DEBUG
                if (Parameter.IsLog)
                {
                    StringBuilder sbContent = new StringBuilder();
                    sbContent.AppendLine("xmlMsg:" + xmlMsg + "\r\n");
                    sbContent.AppendLine("uri:" + uri + "\r\n");
                    sbContent.AppendLine("mothod:" + mothod + "\r\n");
                    sbContent.AppendLine("contentType:" + contentType + "\r\n");
                    sbContent.AppendLine("referer:" + referer + "\r\n");
                    sbContent.AppendLine("timeOut:" + Parameter.timeOut + "\r\n");
                    sbContent.AppendLine("response:" + response + "\r\n");
                    sbContent.AppendLine("sendTime:" + sendTime + "\r\n");
                    sbContent.AppendLine("recevieTime:" + recevieTime + "\r\n");
                    WriterLog wl = new WriterLog();
                    wl.Logger(sbContent.ToString());
                }
                #endif
                return response;//返回返回的报文内容
            }
            catch
            {
                return "1";
            }
            finally
            {
                if (rsp != null)
                {
                    rsp.Close();
                }
                if (req != null)
                {
                    req.Abort();
                }
            }
        }

        public string BLPHttpReadUploadFile(string fileName)
        {
            string hashAndUrl = string.Empty, content = string.Empty, str = string.Empty;
            try
            {
                byte[] bs = File.ReadAllBytes(fileName);
                content = System.Text.Encoding.UTF8.GetString(bs);
                str = this.Sign(bs);
                hashAndUrl = content + "|" + str;
                return hashAndUrl;
            }
            catch
            {
                return "1";
            }
        }

        public string BLPHttpDeleteFile(string fileName)
        {
            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                return "0";
            }
            catch
            {
                return "1";
            }
        }

        public string BLPHttpUploadAsyncHandler(string fileName, string uri, string mothod, string contentType, string referer)
        {
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            try
            {
                string response = string.Empty;
                #if DEBUG
                string sendTime = string.Empty, recevieTime = string.Empty;
                #endif
                fileName = fileName.Substring(fileName.IndexOf("=") + 1);
                byte[] bs = File.ReadAllBytes(fileName);
                string str = this.Sign(bs);
                uri = uri.Substring(uri.IndexOf("=") + 1);
                Uri requestUri = new Uri(string.Format("{0}&hash={1}", uri, str));
                mothod = mothod.Substring(mothod.IndexOf("=") + 1);
                contentType = contentType.Substring(contentType.IndexOf("=") + 1);
                referer = referer.Substring(referer.IndexOf("=") + 1);


                req = (HttpWebRequest)WebRequest.Create(requestUri);
                req.PreAuthenticate = false;
                req.ContentType = contentType;
                req.Timeout = Parameter.timeOut;
                req.AllowAutoRedirect = true;
                if (Parameter.IsUserAgentValue)
                    req.UserAgent = Parameter.UserAgent;
                req.Method = mothod;
                req.Referer = referer;
                req.KeepAlive = Parameter.IsKeepAlive;
                if (Parameter.IsUserCache)
                {
                    HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                    req.CachePolicy = policy;
                }
                #if DEBUG
                if (Parameter.IsLog)
                    sendTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                #endif
                Stream reqstream = req.GetRequestStream();
                reqstream.Write(bs, 0, bs.Length);
                reqstream.Close();
                rsp = (HttpWebResponse)req.GetResponse();
                Stream rspstream = rsp.GetResponseStream();
                HttpStatusCode statusCode = rsp.StatusCode;
                #if DEBUG
                if (Parameter.IsLog)
                    recevieTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                #endif
                while (rspstream != null && rspstream.CanRead)
                {
                    StreamReader sr = new StreamReader(rspstream);
                    response += sr.ReadToEnd() + "|";
                    sr.Close();
                    rspstream.Close();
                }
                response += statusCode.ToString() + "|" + rsp.Headers["result"];
                #if DEBUG
                if (Parameter.IsLog)
                {
                    StringBuilder sbContent = new StringBuilder();
                    sbContent.AppendLine("fileName:" + fileName + "\r\n");
                    sbContent.AppendLine("uri:" + requestUri + "\r\n");
                    sbContent.AppendLine("mothod:" + mothod + "\r\n");
                    sbContent.AppendLine("contentType:" + contentType + "\r\n");
                    sbContent.AppendLine("referer:" + referer + "\r\n");
                    sbContent.AppendLine("timeOut:" + Parameter.timeOut + "\r\n");
                    sbContent.AppendLine("response:" + response + "\r\n");
                    sbContent.AppendLine("sendTime:" + sendTime + "\r\n");
                    sbContent.AppendLine("recevieTime:" + recevieTime + "\r\n");
                    WriterLog wl = new WriterLog();
                    wl.Logger(sbContent.ToString());
                }
                #endif
                return response;//返回返回的报文内容
            }
            catch
            {
                return "1";
            }
            finally
            {
                if (rsp != null)
                {
                    rsp.Close();
                }
                if (req != null)
                {
                    req.Abort();
                }
            }
        }

        public string BLPSocketActiveInit(string ipAddr, int port)
        {
            try
            {
                activeClient = new TcpClient();
                activeClient.Connect(ipAddr, port);
                return "0";
            }
            catch (Exception ex)
            {
                if (activeClient != null)
                {
                    activeClient.Close();
                }
                return ex.Message;
            }
        }

        public string BLPSocketActiveMsg(string msgStr)
        {
            activeClient.SendTimeout = Parameter.timeOut;
            string response = string.Empty;
            try
            {
                if (activeClient.Connected)
                {
                    byte[] sendBuf = Encoding.ASCII.GetBytes(msgStr);
                    activeStream = activeClient.GetStream();
                    activeStream.WriteTimeout = Parameter.timeOut;
                    activeStream.Write(sendBuf, 0, sendBuf.Length);
                    if (!Parameter.IsAsync)
                    {
                        byte[] revBuf = new byte[Parameter.SocketRecevieBufferLength];
                        activeStream.ReadTimeout = Parameter.timeOut;
                        if (activeStream.CanRead)
                        {
                            int received = activeStream.Read(revBuf, 0, Parameter.SocketRecevieBufferLength);
                            if (received <= 0)
                            {
                                return "接收的长度为" + received;
                            }
                        }
                        else
                        {
                            return "数据流不能读取";
                        }
                        response = Encoding.Default.GetString(revBuf);
                    }
                    else
                    {
                        response = "0";
                    }
                }
                else
                {
                    response = "1";
                }

                return response;
            }
            catch (Exception ex)
            {
                if (activeStream != null)
                {
                    activeStream.Close();
                }
                if (activeClient != null)
                {
                    activeClient.Close();
                }
                return ex.Message;
            }
        }

        public string BLPSocketActiveMsg( string msgStr, int headerHexLength)
        {
            activeClient.SendTimeout = Parameter.timeOut;
            string response = string.Empty;
            string hex = string.Empty;
            byte[] sendBytes = null;
            try
            {
                if (activeClient.Connected)
                {
                    int headerLengthValue = Convert.ToInt32(msgStr.Substring(0, headerHexLength));
                    hex = Convert.ToString(headerLengthValue, 16).PadLeft(4, '0');
                    byte[] bytes = HexStrToHexByte(hex);
                    byte[] sendBuf = Encoding.ASCII.GetBytes(msgStr.Substring(headerHexLength));
                    if (bytes != null && sendBuf != null)
                    {
                        sendBytes = CopyByte(bytes, sendBuf);
                    }
                    activeStream = activeClient.GetStream();
                    activeStream.WriteTimeout = Parameter.timeOut;
                    activeStream.Write(sendBytes, 0, sendBytes.Length);
                    if (!Parameter.IsAsync)
                    {
                        byte[] revBuf = new byte[Parameter.SocketRecevieBufferLength];
                        activeStream.ReadTimeout = Parameter.timeOut;
                        if (activeStream.CanRead)
                        {
                            int received = activeStream.Read(revBuf, 0, Parameter.SocketRecevieBufferLength);
                            if (received <= 0)
                            {
                                return "接收的长度为" + received;
                            }
                        }
                        else
                        {
                            return "数据流不能读取";
                        }
                        response = Encoding.Default.GetString(revBuf);
                    }
                    else
                    {
                        response = "0";
                    }
                }
                else
                {
                    response = "1";
                }
                return response;
            }
            catch (Exception ex)
            {
                if (activeStream != null)
                {
                    activeStream.Close();
                }
                if (activeClient != null)
                {
                    activeClient.Close();
                }
                if (sendBytes != null)
                {
                    sendBytes = null;
                }
                return ex.Message;
            }
        }

        public void BLPSocketActiveEnd()
        {
            if (activeStream != null)
            {
                activeStream.Close();
            }
            if (activeClient != null)
            {
                activeClient.Close();
            }
        }

        public string BLPSocketBot(string ipAddr,int port,string msgStr)
        {
            TcpClient client = new TcpClient();
            NetworkStream stream = null;
            client.SendTimeout = Parameter.timeOut;
            string response = string.Empty;
            try
            {
                byte[] sendBuf = Encoding.ASCII.GetBytes(msgStr);
                client.Connect(ipAddr, port);
                stream = client.GetStream();
                stream.WriteTimeout = Parameter.timeOut;
                stream.Write(sendBuf,0,sendBuf.Length);
                if (!Parameter.IsAsync)
                {
                    byte[] revBuf = new byte[Parameter.SocketRecevieBufferLength];
                    stream.ReadTimeout = Parameter.timeOut;
                    if (stream.CanRead)
                    {
                        int received = stream.Read(revBuf, 0, Parameter.SocketRecevieBufferLength);
                        if (received <= 0)
                        {
                            return "接收的长度为" + received;
                        }
                    }
                    else
                    {
                        return "数据流不能读取";
                    }
                    response = Encoding.Default.GetString(revBuf);
                }
                else
                {
                    response = "0";
                }
                return response;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (client != null)
                {
                    client.Close();
                }
            }
        }

        public string BLPSocketBot(string ipAddr, int port, string msgStr,int headerHexLength)
        {
            TcpClient client = new TcpClient();
            NetworkStream stream = null;
            client.SendTimeout = Parameter.timeOut;
            string response = string.Empty;
            string hex = string.Empty;
            byte[] sendBytes = null;
            try
            {
                int headerLengthValue= Convert.ToInt32(msgStr.Substring(0, headerHexLength));
                hex = Convert.ToString(headerLengthValue, 16).PadLeft(4, '0');
                byte[] bytes = HexStrToHexByte(hex);
                byte[] sendBuf = Encoding.ASCII.GetBytes(msgStr.Substring(headerHexLength));
                if (bytes != null && sendBuf != null)
                {
                    sendBytes = CopyByte(bytes, sendBuf);
                }
                client.Connect(ipAddr, port);
                stream = client.GetStream();
                stream.WriteTimeout = Parameter.timeOut;
                stream.Write(sendBytes, 0, sendBytes.Length);
                if (!Parameter.IsAsync)
                {
                    byte[] revBuf = new byte[Parameter.SocketRecevieBufferLength];
                    stream.ReadTimeout = Parameter.timeOut;
                    if (stream.CanRead)
                    {
                        int received = stream.Read(revBuf, 0, Parameter.SocketRecevieBufferLength);
                        if (received <= 0)
                        {
                            return "接收的长度为" + received;
                        }
                    }
                    else
                    {
                        return "数据流不能读取";
                    }
                    response = Encoding.Default.GetString(revBuf);
                }
                else
                {
                    response = "0";
                }
                return response;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (client != null)
                {
                    client.Close();
                }
                if (sendBytes != null)
                {
                    sendBytes = null;
                }
            }
        }

        public void DataVerifyToFile(string destFolder,string fileName,string dataStr)
        {
                try
                {
                    if (!(Directory.Exists(destFolder)))
                    {
                        Directory.CreateDirectory(destFolder);
                    }
                    if (!destFolder.EndsWith("\\"))
                    {
                        destFolder += "\\";
                    }
                    using (StreamWriter sw = new StreamWriter(destFolder + fileName, true, Encoding.Default))
                    {
                        sw.WriteLine(dataStr);
                        sw.Flush();
                        sw.Close();
                    }
                }
                catch
                {
                    Thread.Sleep(50);
                    DataVerifyToFile(destFolder, fileName, dataStr);
                }
        }


        public string BLPRecordToFile(string[] operArray, string[] marketArray,string key, string iv, string id, string tellerNo, string guid)
        {
            int nums = 0;
            try
            {
                IList<string> recordNums = new List<string>();
                StringBuilder sb = new StringBuilder(500);
                if (operArray.Length >= marketArray.Length)
                {
                    nums = operArray.Length;
                }
                else
                {
                    nums = marketArray.Length;
                }
                for (int i = 0; i < nums; i++)
                {
                    if (i < operArray.Length)
                    {
                        string[] conArray = StrArray(operArray[i], ',');
                        string CustomerNo = conArray[0];
                        string businesscode = conArray[1];
                        string TellerNo = conArray[2];
                        string BranchID = conArray[3];
                        string amtfield = conArray[4];
                        string ProvinceBranchNo = conArray[5];

                        sb.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
                            "00",
                            CustomerNo,
                            businesscode,
                            TellerNo,
                            BranchID,
                            amtfield,
                            ProvinceBranchNo,
                            TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours,
                            System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                                Guid.NewGuid().ToString("N"));
                        recordNums.Add(sb.ToString());
                        sb.Remove(0, sb.Length);
                    }
                    if (i < marketArray.Length)
                    {
                        string[] conArray = StrArray(marketArray[i], ',');
                        string CustomerNo = conArray[0];
                        string ProductNo = conArray[1];
                        string ProductMarkingResult = conArray[2];
                        string ProductMarkingRemark = conArray[3];
                        string TellerNo = conArray[4];
                        string BranchID = conArray[5];
                        string ProvinceBranchNo = conArray[6];
                        sb.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}",
                        "01",
                        CustomerNo,
                        ProductNo,
                        ProductMarkingResult,
                        ProductMarkingRemark,
                        TellerNo,
                        BranchID,
                        ProvinceBranchNo,
                        TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours,
                        System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        Guid.NewGuid().ToString("N"));
                        recordNums.Add(sb.ToString());
                        sb.Remove(0, sb.Length);
                    }
                }
                int ret = SaveData(recordNums, key, iv, id, tellerNo, guid);
                if (ret == 0)
                    return "0";
                else
                    return "1";
            }
            catch
            {
                return "1";
            }
        }

        public string BLPRecordOperToFile(string[] strArray,string key,string iv,string id,string tellerNo,string guid)
        {
            try
            {
                IList<string> recordopers = new List<string>();
                StringBuilder sb = new StringBuilder(200);

                for (int i = 0; i < strArray.Length;i++ )
                {
                    string[] conArray = StrArray(strArray[i],',');
                    string CustomerNo = conArray[0];
                    string businesscode = conArray[1];
                    string TellerNo = conArray[2];
                    string BranchID = conArray[3];
                    string amtfield = conArray[4];
                    string ProvinceBranchNo = conArray[5];

                    sb.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
                        "00",
                        CustomerNo,
                        businesscode,
                        TellerNo,
                        BranchID,
                        amtfield,
                        ProvinceBranchNo,
                        TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours,
                        System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                            Guid.NewGuid().ToString("N"));
                    recordopers.Add(sb.ToString());
                    sb.Remove(0, sb.Length);
                }
                int ret = SaveData(recordopers,key,iv,id,tellerNo,guid);
                if (ret == 0)
                    return "0";
                else
                    return "1";
            }
            catch
            {
                return "1";
            }
        }

        public string BLPRecordProductMarkingToFile(string[] strArray, string key, string iv, string id, string tellerNo, string guid)
        {
            try
            {
                StringBuilder sb = new StringBuilder(500);
                IList<string> recoderitems = new List<string>();
                for (int i = 0; i < strArray.Length; i++)
                {
                    string[] conArray = StrArray(strArray[i], ',');
                    string CustomerNo = conArray[0];
                    string ProductNo = conArray[1];
                    string ProductMarkingResult = conArray[2];
                    string ProductMarkingRemark = conArray[3];
                    string TellerNo = conArray[4];
                    string BranchID = conArray[5];
                    string ProvinceBranchNo = conArray[6];
                    sb.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}",
                    "01",
                    CustomerNo,
                    ProductNo,
                    ProductMarkingResult,
                    ProductMarkingRemark,
                    TellerNo,
                    BranchID,
                    ProvinceBranchNo,
                    TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours,
                    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Guid.NewGuid().ToString("N"));
                    recoderitems.Add(sb.ToString());
                    sb.Remove(0, sb.Length);
                }
                int ret = SaveData(recoderitems, key, iv, id, tellerNo, guid);
                if (ret == 0)
                    return "0";
                else
                    return "1";
            }
            catch
            {
                return "1";
            }
        }

        public string[] StrArray(string tempStr, char splitChar)
        {
            return tempStr.Split(new char[] { splitChar });
        }

        public void WriteFileStream(string destFolder, string fileName, byte[] bytes)
        {
            try
            {
                if (!(Directory.Exists(destFolder)))
                {
                    Directory.CreateDirectory(destFolder);
                }
                if (!destFolder.EndsWith("\\"))
                {
                    destFolder += "\\";
                }
                using (FileStream fs = new FileStream(destFolder + fileName, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch
            {
                Thread.Sleep(50);
                WriteFileStream(destFolder, fileName, bytes);
            }
        }

        public string EbcdicFileSubmitStr(string sourceFolder, string fileName, string hexIndexStr, int hexLength)
        {
            byte[] data = null;
            string returnStr = string.Empty;
            IBM1388Encoding encoding = new IBM1388Encoding();
            try
            {
                if (!sourceFolder.EndsWith("\\"))
                {
                    sourceFolder += "\\";
                }
                if (!string.IsNullOrEmpty(sourceFolder) && !string.IsNullOrEmpty(fileName))
                {
                    data = File.ReadAllBytes(sourceFolder + fileName);
                    string hexStr = ByteToHexStr(data);
                    string parameter = hexStr.Substring(hexStr.IndexOf(hexIndexStr), hexLength);
                    byte[] parameterBytes = HexStrToHexByte(parameter);
                    returnStr = encoding.GetString(parameterBytes);
                }
                else
                {
                    return "源文件没有找到";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (data != null)
                {
                    data = null;
                }
            }

            return returnStr;
        }

        public string ParaGenEbcdicFile(string sourceFolder, string sourceFileName, string destFolder, string destFileName, string hexIndexStr, int hexLength, string replaceStr)
        {
            byte[] data = null;
            string resuleHexStr = string.Empty;
            IBM1388Encoding encoding = new IBM1388Encoding();
            try
            {
                if (!sourceFolder.EndsWith("\\"))
                {
                    sourceFolder += "\\";
                }
                if (!destFolder.EndsWith("\\"))
                {
                    destFolder += "\\";
                }
                if (!string.IsNullOrEmpty(sourceFolder) && !string.IsNullOrEmpty(sourceFileName) && !string.IsNullOrEmpty(destFolder) && !string.IsNullOrEmpty(destFileName))
                {
                    data = File.ReadAllBytes(sourceFolder + sourceFileName);
                    string hexStr = ByteToHexStr(data);
                    byte[] paraBytes = encoding.GetBytes(replaceStr.ToCharArray());
                    resuleHexStr = ByteToHexStr(paraBytes);
                    if (resuleHexStr.Length == hexIndexStr.Length)
                    {
                        hexStr = hexStr.Replace(hexIndexStr, resuleHexStr);
                        byte[] editdata = HexStrToHexByte(hexStr);
                        WriteFileStream(destFolder, destFileName, editdata);
                    }
                    else
                    {
                        return "要替换内容长度不一致";
                    }
                }
                else
                {
                    return "目标文件错误";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (data != null)
                {
                    data = null;
                }
            }

            return "0";
        }

        public string ParaGenEbcdicFile(string sourceFolder, string sourceFileName, string destFolder, string destFileName, string[] hexIndexStr, int hexLength, string[] replaceStr)
        {
            byte[] data = null,editdata =null;
            string resuleHexStr = string.Empty;
            IBM1388Encoding encoding = new IBM1388Encoding();
            try
            {
                if (!sourceFolder.EndsWith("\\"))
                {
                    sourceFolder += "\\";
                }
                if (!destFolder.EndsWith("\\"))
                {
                    destFolder += "\\";
                }
                if (!string.IsNullOrEmpty(sourceFolder) && !string.IsNullOrEmpty(sourceFileName) && !string.IsNullOrEmpty(destFolder) && !string.IsNullOrEmpty(destFileName))
                {
                    data = File.ReadAllBytes(sourceFolder + sourceFileName);
                    string hexStr = ByteToHexStr(data);
                    for (int i = 0; i < hexIndexStr.Length; i++)
                    {
                        byte[] paraBytes = encoding.GetBytes(replaceStr[i].ToCharArray());
                        resuleHexStr = ByteToHexStr(paraBytes);
                        if (resuleHexStr.Length == hexIndexStr[i].Length)
                        {
                            hexStr = hexStr.Replace(hexIndexStr[i], resuleHexStr);
                            editdata = HexStrToHexByte(hexStr);
                        }
                        else
                        {
                            return "要替换内容长度不一致";
                        }
                    }
                    WriteFileStream(destFolder, destFileName, editdata);
                }
                else
                {
                    return "目标文件错误";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (data != null)
                {
                    data = null;
                }
                if (editdata != null)
                {
                    editdata = null;
                }
            }

            return "0";
        }

        public double DateTimeDiff(string dateTimeStartStr, string dateTimeEndStr)
        {
            double dateDiff = 0.000;
            try
            {
                DateTime dateTimeStart = DateTime.Parse(dateTimeStartStr);
                DateTime dateTimeEnd = DateTime.Parse(dateTimeEndStr);
                TimeSpan tsStart = new TimeSpan(dateTimeStart.Ticks);
                TimeSpan tsEnd = new TimeSpan(dateTimeEnd.Ticks);
                TimeSpan ts = tsStart.Subtract(tsEnd).Duration();

                dateDiff = Convert.ToDouble(ts.Days * 24 * 3600 + ts.Hours * 3600 + ts.Minutes * 60 + ts.Seconds + "." + ts.Milliseconds.ToString().Replace("-","").PadLeft(3,'0'));
            }
            catch
            {
                return 99999.999;
            }

            return dateDiff;
        }

        /*public string JITDetachSign(string certInfo,string srcStr)
        {
            string signResult = string.Empty;
            JITComVCTKLib.JITDSignClass sign = null;
            try
            {
                sign = new JITComVCTKLib.JITDSignClass();
                signResult = sign.DetachSign(certInfo, srcStr);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (sign != null)
                {
                    sign = null;
                }
            }

            return signResult;
        }*/

        #endregion

        #region Private Methods

        private string Sign(byte[] data)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(data)).Replace("-", "");
        }

        private byte[] HexStringToBytesArray(string text)
        {
            text = text.Replace(" ", "");
            int l = text.Length;
            byte[] buffer = new byte[l / 2];
            for (int i = 0; i < l; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(text.Substring(i, 2), 16);
            }
            return buffer;
        }

        private int SaveData(IList<string> _data, string key, string iv, string id, string tellerNo, string guid)
        {
            int recordnumber = 0;
            try
            {
                lock (sync_SaveData)
                {
                    #region
                    if (!System.IO.Directory.Exists("C:\\Bsms\\SubmitFiles\\"))
                    {
                        System.IO.Directory.CreateDirectory("C:\\Bsms\\SubmitFiles\\");
                    }
                    if (string.IsNullOrEmpty(id)
                        || !System.IO.Directory.Exists("C:\\Bsms\\SubmitFiles\\")
                        || _data.Count() == 0)
                        return 1;

                    string savefilename = tellerNo + "_" + System.DateTime.Today.ToString("yyyyMMdd") + "_" + guid + ".bsms";
                    StreamWriter sw = new StreamWriter("C:\\Bsms\\SubmitFiles\\" + savefilename, true, Encoding.Default);
                    if (recordnumber == 0)
                    {
                        sw.WriteLine("SecretID=" + id);
                    }
                    try
                    {
                        byte[] keyByte = HexStringToBytesArray(key);
                        byte[] ivByte = HexStringToBytesArray(iv);
                        foreach (string str_item in _data)
                        {
                            string stre = this.Encrypt(str_item, keyByte, ivByte);
                            //string strd = this.Decrypt(stre, keyByte, ivByte);
                            //System.Diagnostics.Debug.Assert(str_item == strd);
                            sw.WriteLine(stre);
                            recordnumber++;
                        }

                    }
                    finally
                    {
                        sw.Close();
                    }
                    return 0;
                    #endregion
                }
            }
            catch
            {
                return 1;
            }
        }

        private string Encrypt(string indata,byte[] key, byte[] iv)
        {
            MemoryStream ms = new MemoryStream();
            CryptoStream cStream = new CryptoStream(ms,
                  cryptprovider.CreateEncryptor(key, iv),
                  CryptoStreamMode.Write);
            byte[] indata_byte = System.Text.Encoding.UTF8.GetBytes(indata);
            cStream.Write(indata_byte, 0, indata_byte.Length);
            cStream.FlushFinalBlock();
            return System.Convert.ToBase64String(ms.ToArray());
        }

        private string Decrypt(string indata, byte[] key, byte[] iv)
        {
            MemoryStream ms = new MemoryStream();
            CryptoStream cStream = new CryptoStream(ms,
                  cryptprovider.CreateDecryptor(key, iv),
                  CryptoStreamMode.Write);
            byte[] indata_byte = System.Convert.FromBase64String(indata);
            cStream.Write(indata_byte, 0, indata_byte.Length);
            cStream.FlushFinalBlock();
            return System.Text.Encoding.UTF8.GetString(ms.ToArray());
        }

        private static byte[] CopyByte(byte[] a,byte[] b)
        {
            if (a.Length > 0 && b.Length > 0)
            {
                byte[] c = new byte[a.Length + b.Length];
                a.CopyTo(c, 0);
                b.CopyTo(c, a.Length);
                return c;
            }
            else
            {
                return null;
            }
        }

        private static byte[] HexStrToHexByte(string hexStr)
        {
            if (!string.IsNullOrEmpty(hexStr))
            {
                hexStr = hexStr.Replace(" ", "");
            }
            if (hexStr.Length % 2 == 0)
            {
                byte[] hexBytes = new byte[hexStr.Length / 2];
                for (int i = 0; i < hexBytes.Length; i++)
                {
                    hexBytes[i] = Convert.ToByte(hexStr.Substring(i * 2, 2), 16);
                }
                return hexBytes;
            }
            else
            {
                return null;
            }
        }

        private static string ByteToHexStr(byte[] bytes)
        {
            string returnStr = string.Empty;
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }

            return returnStr;
        }

        private static string HexStrToStr(string hexStr, Encoding encoding)
        {
            string byteStr = string.Empty;
            byte[] bytes = new byte[hexStr.Length / 2];
            for (int i = 0; i < hexStr.Length / 2; i++)
            {
                byteStr = hexStr.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(byteStr, 16);
            }

            return encoding.GetString(bytes);
        }

        private static string StrToHexStr(string str, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(str);
            string hexResult = string.Empty;
            for (int i = 0; i < bytes.Length; i++)
            {
                hexResult += Convert.ToString(bytes[i], 16);
            }

            return hexResult;
        }

        private static byte[] HexStrToHexByte2(string hexStr)
        {
            hexStr = hexStr.Replace(" ", "");
            if ((hexStr.Length % 2) != 0)
                hexStr += " ";
            byte[] returnBytes = new byte[hexStr.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexStr.Substring(i * 2, 2), 16);

            return returnBytes;
        }

        private void CallGC()
        {
            GC.Collect();
        }

        #endregion
    }
}
