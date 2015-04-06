using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using BocBancsLinkPfLib;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Threading;


namespace BocBancsLinkLib
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WebRequest req = null;
            WebResponse rsp = null;

            char[] source = "<componentDocuments><componentDocument version=\"2.0\"><header><flow ID=\"SignOnFlow\" nodeID=\"Start\"><declares></declares><fileName>xml/workflows/SignOnFlow.xml</fileName></flow><component className=\"FNS.FrontEnd.Workflow.Server.FlowDistributor.CDistributor3\" assemblyName=\"FNSServerFlowDistributor3\" runAt=\"server\" version=\"3.0\"><method result=\"True\" name=\"startflow\"><fileName type=\"literal\">xml/workflows/SignOnFlow.xml</fileName><flowID type=\"literal\">SignOnFlow</flowID><nodeID type=\"literal\">Start</nodeID><declares></declares></method></component></header><body><userInfo persist=\"true\"><InstitutionNo>0</InstitutionNo><BranchNo>0</BranchNo><TellerNo>0</TellerNo><TellerName></TellerName><WorkstationNo>0</WorkstationNo><Capability>0</Capability><HostDate></HostDate><LOGONSTATUS>LOGGEDOFF</LOGONSTATUS><Mode>ONLINE</Mode><BranchStatus>OPEN</BranchStatus><UserType>1</UserType><URL></URL><ProvinceCode></ProvinceCode><MACAddress>001E909BF524</MACAddress><MachineName>BOC72</MachineName><BranchNoFromPBC></BranchNoFromPBC></userInfo><screenData persist=\"true\"><TranNo>009001</TranNo><DynamicKey>8476f42c868745ae8c3a08c0535d1b0a</DynamicKey></screenData></body></componentDocument></componentDocuments>".ToCharArray();
            string uri = "http://22.188.155.125:8200/engine.aspx";
            req = WebRequest.Create(uri);
            req.Method = "POST";
            req.PreAuthenticate = false;
            req.ContentType = "application/octet-stream";
            req.Timeout = 120000;
            Stream reqstream = req.GetRequestStream();
            byte[] bs = Encoding.UTF8.GetBytes(source);
            byte[] compressBytes = BOC.BOCGZip.Compress(bs);
            reqstream.Write(compressBytes, 0, compressBytes.Length);
            //Stream stream = new MemoryStream(compressBytes);
            //string readcontent = "";
            //while (stream != null && stream.CanRead)
            //{
            //    StreamReader sr = new StreamReader(stream);
            //    readcontent = Encoding.UTF8.GetString(compressBytes);
            //    sr.Close();
            //    stream.Close();
            //}
            reqstream.Close();
            rsp = req.GetResponse();
            Stream rspstream = rsp.GetResponseStream();
            byte[] dest = BOC.BOCGZip.Decompress(rspstream);
            string content = new UTF8Encoding().GetString(dest);
            if (content.Contains("<UserType>1</UserType>"))
            {
                MessageBox.Show("SUCC");
            }
            else
            {
                MessageBox.Show(content);
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

        private static byte[] HexStrToHexByte(string hexStr)
        {
            hexStr = hexStr.Replace(" ", "");
            if ((hexStr.Length % 2) != 0)
                hexStr += " ";
            byte[] returnBytes = new byte[hexStr.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexStr.Substring(i * 2, 2), 16);

            return returnBytes;
        }

        public void WriteStreamFile(string destFolder, string fileName, byte[] bytes)
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
                    sw.WriteLine(bytes);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch
            {
                Thread.Sleep(50);
                WriteStreamFile(destFolder, fileName, bytes);
            }
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
                using (FileStream fs=new FileStream(destFolder + fileName, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    fs.Write(bytes,0,bytes.Length);
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
                if(data != null)
                {
                    data = null;
                }
            }

            return returnStr;
        }

        public string ParaGenEbcdicFile(string sourceFolder,string sourceFileName,string destFolder, string destFileName,string hexIndexStr, int hexLength,string replaceStr)
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
                if (!string.IsNullOrEmpty(sourceFolder) && !string.IsNullOrEmpty(sourceFileName) && !string.IsNullOrEmpty(destFolder)&&!string.IsNullOrEmpty(destFileName))
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

        public double DateDiff(string dateTimeOneStr, string dateTimeTwoStr)
        {
            double dateDiff = 0.000;
            try
            {
                DateTime DateTimeOne = DateTime.Parse(dateTimeOneStr);
                DateTime DateTimeTwo = DateTime.Parse(dateTimeTwoStr);
                TimeSpan tso = new TimeSpan(DateTimeOne.Ticks);
                TimeSpan tst = new TimeSpan(DateTimeTwo.Ticks);
                TimeSpan ts = tso.Subtract(tst).Duration();

                dateDiff = Convert.ToDouble(ts.Days * 24 * 3600 + ts.Hours * 3600 + ts.Minutes * 60 + ts.Seconds + "." + ts.Milliseconds.ToString());
            }
            catch
            {
                return 99999.999;
            }

            return dateDiff;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            string content = "";
            string responseText = string.Empty, message_BPS1 = string.Empty, message_BPS = string.Empty;
            BancsLinkHttpPf blpf = new BancsLinkHttpPf();
            string uuid = System.Guid.NewGuid().ToString("N");
            string key = "",iv = "",id = "",tellerNo = "";
            //string uuidMega = uuid.Replace("-","");

           // string test = blpf.JITDetachSign("CN=normal, E=normal@jit.com.cn, O=jit, C=cn", "Xuxy");

            double timeDiff = blpf.DateTimeDiff("2013-03-02 06:45:33.872", "2013-03-03 06:46:34.933");

            responseText = blpf.BLPHttpBot("Message=ID=PTAUC01311000001",
               "Url=http://localhost:8080/WebTest/SystemTimeServlet",
               "Mothod=POST",
               "ContentType=text/html",
               "Referer="
               );

            string message1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                  "<GUPPMsg>\n" +
                           "<Header>\n" +
                             "<BankCode>106</BankCode>\n" +
                             "<InstitutionId>51227</InstitutionId>\n" +
                             "<SystemId>56</SystemId>\n" +
                            "</Header>\n" +
                           "<SubmitPaymentReq>\n" +
                             "<TellerId>9880100</TellerId>\n" +
                             "<RequestId>2138618858</RequestId>\n" +
                             "<PayingAccount>100000500002213</PayingAccount>\n" +
                             "<PayingAccountSubType/>\n" +
                             "<PayingAmount>0000000000000000000000</PayingAmount>\n" +
                             "<PayingCurrency>AUD</PayingCurrency>\n" +
                             "<BenificiaryBankCode>036639</BenificiaryBankCode>\n" +
                             "<BenificiaryNCCCode>036639</BenificiaryNCCCode>\n" +
                             "<BenificiaryAccount>100000500002257</BenificiaryAccount>\n" +
                             "<BenificiaryName>UATAU</BenificiaryName>\n" +
                             "<BenificiaryAmount>000000000000002013.600</BenificiaryAmount>\n" +
                             "<BenificiaryCurrency>AUD</BenificiaryCurrency>\n" +
                             "<Remark>45645</Remark>\n" +
                             "<Mode>0</Mode>\n" +
                             "<FeeDiscount>100</FeeDiscount>\n" +
                             "<FxRate>00001467.698900</FxRate>\n" +
                             "<FxRateUnit>000001.0000</FxRateUnit>\n" +
                             "<HomeCurrency>AUD</HomeCurrency>\n" +
                             "<FxDirection>Y</FxDirection>\n" +
                             "<FxVersion>000001</FxVersion>\n" +
                             "<FxDate>0110</FxDate>\n" +
                            "</SubmitPaymentReq>\n" +
                  "</GUPPMsg>\n";

            string message2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                  "<GUPPMsg>\n" +
                           "<Header>\n" +
                             "<BankCode>106</BankCode>\n" +
                             "<InstitutionId>51227</InstitutionId>\n" +
                             "<SystemId>56</SystemId>\n" +
                            "</Header>\n" +
                           "<SubmitPaymentReq>\n" +
                             "<TellerId>9880100</TellerId>\n" +
                             "<RequestId>2138618859</RequestId>\n" +
                             "<PayingAccount>100000500002213</PayingAccount>\n" +
                             "<PayingAccountSubType/>\n" +
                             "<PayingAmount>0000000000000000000000</PayingAmount>\n" +
                             "<PayingCurrency>AUD</PayingCurrency>\n" +
                             "<BenificiaryBankCode>036639</BenificiaryBankCode>\n" +
                             "<BenificiaryNCCCode>036639</BenificiaryNCCCode>\n" +
                             "<BenificiaryAccount>100000500002257</BenificiaryAccount>\n" +
                             "<BenificiaryName>UATAU</BenificiaryName>\n" +
                             "<BenificiaryAmount>000000000000002013.600</BenificiaryAmount>\n" +
                             "<BenificiaryCurrency>AUD</BenificiaryCurrency>\n" +
                             "<Remark>45645</Remark>\n" +
                             "<Mode>0</Mode>\n" +
                             "<FeeDiscount>100</FeeDiscount>\n" +
                             "<FxRate>00001467.698900</FxRate>\n" +
                             "<FxRateUnit>000001.0000</FxRateUnit>\n" +
                             "<HomeCurrency>AUD</HomeCurrency>\n" +
                             "<FxDirection>Y</FxDirection>\n" +
                             "<FxVersion>000001</FxVersion>\n" +
                             "<FxDate>0110</FxDate>\n" +
                            "</SubmitPaymentReq>\n" +
                  "</GUPPMsg>\n";

            blpf.BLPSocketActiveInit("22.127.26.145", 10005);
            content = blpf.BLPSocketActiveMsg("12345XXXX12345");
            content = blpf.BLPSocketActiveMsg("12345XXXX12345");
            blpf.BLPSocketActiveEnd();

            string[] test1 = {"11,E540140101,10001,10001,100201.23,00004"};

            //string source = "我们ASDFG";

            message_BPS = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
          "<req>" +
          "<header>" +
          "<transid>000202</transid>" +
          "<orgidt>51226</orgidt>" +
          "<chnlid>0</chnlid>" +
          "<proorg>51011</proorg>" +
          "<termid>277</termid>" +
          "<telno>1086127</telno>" +
          "<spvno1>1086127</spvno1>" +
          "<wkstation>277</wkstation>" +
          "<bankcode>106</bankcode>" +
          "</header>" +
          "<body>" +
          "<systemname>01</systemname>" +
          "<valuedate>20130601</valuedate>" +
          "<bpsfilename>14.BPMT.32.106.51226.20130620.00001784.DAT</bpsfilename>" +
          "</body>" +
          "</req>";

            message_BPS1 = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                      "<req>" +
                      "<header>" +
                      "<transid>000203</transid>" +
                      "<orgidt>51226</orgidt>" +
                      "<chnlid>0</chnlid>" +
                      "<proorg>51011</proorg>" +
                      "<termid>277</termid>" +
                      "<telno>1086127</telno>" +
                      "<spvno1>1086127</spvno1>" +
                      "<wkstation>277</wkstation>" +
                      "<bankcode>106</bankcode>" +
                      "</header>" +
                      "<body>" +
                      "<systemname>01</systemname>" +
                      "<valuedate>20130601</valuedate>" +
                      "<bpsfilename>14.BPMT.32.106.51226.20130620.00001784.DAT</bpsfilename>" +

                      "<accounttype>DEP</accounttype>" +
                      "<postingtype>01</postingtype>" +
                      "<chargetype>03</chargetype>" +
                      "</body>" +
                      "</req>";

            responseText = blpf.BLPSocketBot("22.188.134.31", 9527, "412.." + message_BPS, 3);

            responseText = blpf.BLPSocketBot("22.188.134.31", 9527, "498.." + message_BPS1, 3);

            content = responseText.Replace("\\n","");

            //.output_message(responseText);

            //if(responseText.Contains("<msgcde>0000</msgcde>") && responseText.Contains("<status>8800</status>"))


            content = ParaGenEbcdicFile("C:\\", "COVPAYMENTORDER", "C:\\destnation", "COVPAYMENTORDER_new", "F1F2F1F1F1F2F2F8F7F3F9", 22, "12111228741");

            content = ParaGenEbcdicFile("C:\\", "COVPAYMENTORDER", "C:\\destnation", "COVPAYMENTORDER_new", "F1F2F1F1F1F2F2F8F7F3F9", 22, "12111228742");

            byte[] data = File.ReadAllBytes("C:\\COVPAYMENTORDER");

            string hexStr = ByteToHexStr(data);

            string no = hexStr.Substring(hexStr.IndexOf("F1F2F1F1F1"),22);

            byte[] nobytes = HexStrToHexByte(no);

            IBM1388Encoding encoding = new IBM1388Encoding();

            string destinationStr = encoding.GetString(nobytes);

            string nores = HexStrToStr(no, Encoding.Default);

            byte[] editdata = HexStrToHexByte(hexStr);

            WriteFileStream("C:\\TestEbcdic", "test1.dat", editdata);

            //string str = HexStringToString(hexStr);

            //IBM1388Encoding encoding = new IBM1388Encoding();

            //EbcdicEncoding ebc = new EbcdicEncoding();
            //string dst = ebc.GetString(data);

            //byte[] sendData = new byte[24576];

            //encoding.GetBytes(source).CopyTo(sendData, 0);

            //string destinationStr = encoding.GetString(data,0,data.Length);


            string hex = Convert.ToString(555,16);
            BocBancsLinkPfLib.Parameter.timeOut = 10000;
           // BocBancsLinkPfLib.Parameter.IsAsync = true;
            content = blpf.BLPSocketBot("22.188.134.31", 9527, "555..<?xml version=\"1.0\" encoding=\"UTF-8\"?><req><header><transid>000056</transid><orgidt>51230</orgidt><chnlid>0</chnlid><proorg>51013</proorg><termid></termid><telno>5896459</telno><spvno1>0000000</spvno1><wkstation>013</wkstation><bankcode>106</bankcode></header><body><branchno>51230</branchno><amount1>00000000000000000+</amount1><amount2>00000000000000000+</amount2><tradedate1>00000000</tradedate1><tradedate2>00000000</tradedate2><valuedate1>00000000</valuedate1><valuedate2>00000000</valuedate2><reptype>A</reptype><pageno>00021</pageno></body></req>", 3);

            //Parameter.timeOut = 140000;



            //content = BLPSocketBot("22.188.155.131", 10001, " 0130                   **                    003089890482965428E5400105000000000           0 0000000  00002152209073        76685     ");

            //string text = System.Web.HttpUtility.UrlEncode("<?xml version=\"1.0\" encoding=\"UTF-8\"?><req><header><transid>B023  </transid><orgidt>72298</orgidt><chnlid>0</chnlid><proorg>00008</proorg><provinceno>00008</provinceno><telno>0722985</telno><spvno1>0000000</spvno1><wkstation>048</wkstation></header><body><date>D</date><starttime>00000000</starttime><endtime>00000000</endtime><startno>1</startno></body></req>");

            //BocBancsLinkPfLib.Parameter.IsAsyncValue = true;

            content = blpf.BLPSocketBot("22.188.134.52", 37776, "<?xml version='1.0' encoding='UTF-8'?><scfp><generalInfo><mainRef>10133</mainRef><msgType/><msgSendTime>20130110104453</msgSendTime><transCode>AG0200</transCode><pageInfo><totalRec>0</totalRec><pageSize>0</pageSize><startRec>1</startRec></pageInfo></generalInfo><transData><agfFinc><mainRef>10133</mainRef><trxDate>20130131160053</trxDate><relatedRef>relatedRef001</relatedRef><fincType>1</fincType><unitCode>02249</unitCode><channelFlg>1</channelFlg><channelCustID>96062587</channelCustID><applyFlg>2</applyFlg><applID>282796232</applID><custRemark>custRemark001</custRemark><circleNum>2</circleNum><agfFincData><bocnetRef>10134</bocnetRef><buyerID>buyerID001</buyerID><buyerErpID>buyerErpID001</buyerErpID><sellerID>sellerID001</sellerID><agmNo>D-òé??±?±ào?001</agmNo><agreeRef>agreeRef001</agreeRef><guarantApplyRef>·???3Dμ￡oˉ±ào?1</guarantApplyRef><ctrtOrdNo>o?í???μ￥o?04</ctrtOrdNo><settMthd>1</settMthd><goodsDesc>goodsDesc001</goodsDesc><orderDate>2012-12-26</orderDate><transportDate>2012-12-27</transportDate><taxInvNo>x1122557</taxInvNo><recvBillNo>x1122557</recvBillNo><accPayCcy>CNY</accPayCcy><accPayAmt>5000</accPayAmt><applyFincAmt>247.79</applyFincAmt><applyFloatPercentFlg>3</applyFloatPercentFlg><applyFloatPercent>0</applyFloatPercent><applyFincDays>60</applyFincDays></agfFincData><agfFincData><bocnetRef>10135</bocnetRef><buyerID>buyerID002</buyerID><buyerErpID>buyerErpID002</buyerErpID><sellerID>sellerID002</sellerID><agmNo>D-òé??±?±ào?001</agmNo><agreeRef>agreeRef002</agreeRef><guarantApplyRef>·???3Dμ￡oˉ±ào?1</guarantApplyRef><ctrtOrdNo>o?í???μ￥o?05</ctrtOrdNo><settMthd>1</settMthd><goodsDesc>goodsDesc002</goodsDesc><orderDate>2012-12-28</orderDate><transportDate>2012-12-29</transportDate><taxInvNo>x1122557</taxInvNo><recvBillNo>x1122557</recvBillNo><accPayCcy>CNY</accPayCcy><accPayAmt>5000</accPayAmt><applyFincAmt>247.80</applyFincAmt><applyFloatPercentFlg>3</applyFloatPercentFlg><applyFloatPercent>0</applyFloatPercent><applyFincDays>59</applyFincDays></agfFincData></agfFinc></transData></scfp>");

             content = blpf.BLPHttpSoapGetSecretKey(
              "Message=<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><soap:Body><getSecretKey xmlns=\"http://tempuri.org/\"><request><TellerNum>10001</TellerNum><ProvinceCode>00004</ProvinceCode></request></getSecretKey></soap:Body></soap:Envelope>",
              "URL=http://22.188.155.163:9003/GetSecretKey.asmx",
              "Method=POST",
              "ContentType=text/xml",
              "Referer=",
              "SOAPAction= http://tempuri.org/getSecretKey",
              "UserName=administrator",
              "Password=ZJboc2010",
              "Domain=boctest"
              );

            if (content.Contains("<SecretKey>"))
            {
                key = content.Substring(content.IndexOf("<Key>") + 5, content.IndexOf("</Key>") - content.IndexOf("<Key>") - 5);
                iv = content.Substring(content.IndexOf("<IV>") + 4, content.IndexOf("</IV>") - content.IndexOf("<IV>") - 4);
                id = content.Substring(content.IndexOf("<ID>") + 4, content.IndexOf("</ID>") - content.IndexOf("<ID>") - 4);
                tellerNo = "10001";
            }

            string ret = blpf.BLPRecordOperToFile(test1,key,iv,id,tellerNo,uuid);


            string temp = BLPHttpReadUploadFile("C:\\Bsms\\SubmitFiles\\10001_20120817_59c57e0529874174b202ca59632d97c6.bsms");

            this.TransferTest(temp);

            content = blpf.BLPHttpUploadAsyncHandler(
              "FileName=C:\\Bsms\\SubmitFiles\\10001_20120817_59c57e0529874174b202ca59632d97c6.bsms",
              "URL=http://22.188.155.145:9002/WebUploadAsyncHandler.ashx",
              "Method=POST",
              "ContentType=",
              "Referer="
              );

            //Transfer("C:\\Bsms\\SubmitFiles\\10001_20120817_59c57e0529874174b202ca59632d97c6.bsms");

            content = blpf.BLPHttpBot(
           "Message=?SecretID=10000000554" + "\r\n" +
                     "ektc6kt42/YlnnAF/lL77lC+woHFlZ5717r7E8CNpERqxSt+IH1EkduUuR9fYzdN5X9pO052LQVuOVogAIEzxrSmKW0jLPKkOwYd6y/sksGLbvKUDysmKdJUnySrS8Sr" + "\r\n",
           "URL=http://22.188.155.145:9002/WebUploadAsyncHandler.ashx?FileName=10001_20120817_59c57e0529874174b202ca59632d97c6.bsms&hash=8F5E6A17F89E007296014627CBDCCAF2",
           "Method=POST",
           "ContentType=",
           "Referer="
           );

            content = blpf.BLPHttpBot(
           "Message=",
           "URL=http://22.188.155.41:8000/Release/resource/Province/13/zh-CHS/xml/menus/B13menu.xml",
           "Method=HEAD",
           "ContentType=",
           "Referer="
           );

            content = blpf.BLPHttpBot("Message=",
                                          "Url=http://22.188.155.125:8200/Menu.aspx?sourceFile=XML%2fMENUS%2fONLINE001.XML&userType=001",
                                          "Mothod=GET",
                                          "ContentType=application/octet-stream",
                                          "Referer="
                                          );

            content = blpf.BLPHttpSoap("Message=<?xml version=\"1.0\"  encoding=\"UTF-8\" standalone=\"no\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><processDocument xmlns=\"http://tempuri.org/\"><oCurrentInDocument><componentDocuments xmlns=\"\"><componentDocument version=\"2.0\"><header><flow nodeID=\"\" ID=\"\"><declares/><fileName/></flow><component version=\"3.0\" runAt=\"\" assemblyName=\"\" className=\"\"><method name=\"\" result=\"True\"><fileName type=\"literal\"/><flowID type=\"literal\"/><nodeID type=\"literal\"/><declares/></method></component></header><body><!--柜员信息配置节--><userInfo><InstitutionNo>3</InstitutionNo><BranchNo>13169</BranchNo><TellerNo>1923628</TellerNo><TellerName/><WorkstationNo>48</WorkstationNo><Capability>14</Capability><HostDate>11/05/2013</HostDate><LOGONSTATUS>LOGGEDON</LOGONSTATUS><Mode>ONLINE</Mode><BranchStatus>OPEN</BranchStatus><UserType>1</UserType><UserSection/><URL/><ProvinceCode>13</ProvinceCode><MACAddress>001E909BF524</MACAddress><MachineName>BOC72</MachineName><BranchNoFromPBC/><ProvinceBranchNo>00004</ProvinceBranchNo></userInfo><!--动态交易信息配置节--><hostDataIn><hostDataIn_060460><TranNo>060460</TranNo><CustOrAcctNum>282772755</CustOrAcctNum><CustOrAcct>C</CustOrAcct><Option>U</Option><Flag1>8</Flag1><ReenableFlag/><UUID/></hostDataIn_060460></hostDataIn></body></componentDocument></componentDocuments></oCurrentInDocument></processDocument></s:Body></s:Envelope>",
                                          "Url=http://22.188.155.125:8099/WCFCommunicationService.svc",
                                          "Mothod=POST",
                                          "ContentType=text/xml",
                                          "Referer=",
                                          "SOAPAction=http://tempuri.org/IWCFCommunicationService/processDocument"
                                          );

            content = blpf.BLPHttpMultipart("Message=",
                 "Uri=http://22.188.155.125:8200/AttachmentItem.aspx",
                 "Mothod=POST",
                 "ContentType=multipart/form-data",
                 "Referer=",
                 "AttachmentMetadata=<Attachments>" + "\r\n" +
                                    "  <Attachment>" + "\r\n" +
                                    "    <AttachmentType>Transaction</AttachmentType>" + "\r\n" +
                                    "    <Description />" + "\r\n" +
                                    "    <Identity>Binary</Identity>" + "\r\n" +
                                    "    <IsHistory>false</IsHistory>" + "\r\n" +
                                    "    <IsSaveToDataBase>true</IsSaveToDataBase>" + "\r\n" +
                                    "    <IsValid>true</IsValid>" + "\r\n" +
                                    "    <Name>Transaction State Info</Name>" + "\r\n" +
                                    "    <ProcessSystem />" + "\r\n" +
                                    "    <ProcessSystemID />" + "\r\n" +
                                    "    <ServerID />" + "\r\n" +
                                    "    <UniqueKey>F7008f64-2497-4645-babc-e502558bb964</UniqueKey>" + "\r\n" +
                                    "    <ValueType>Binary</ValueType>" + "\r\n" +
                                    "    <Attributes />" + "\r\n" +
                                    "  </Attachment>" + "\r\n" +
                                    "</Attachments>",
                 "OctetStream=<transactionState flowInstanceID=\"62bd4f0c-708d-49b1-a0ef-e24e8e28893e\" timestamp=\"8cf2ce6c251039f\" lastScreenUrl=\"000500.htm\" senderName=\"Transmit\">" + "\r\n" +
                                    "  <transactionDatas>" + "\r\n" +
                                    "    <data name=\"F000500\" disable=\"false\" readOnly=\"false\" visibility=\"visible\">" + "\r\n" +
                                    "      <data name=\"ProvinceCode1\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"13\" />" + "\r\n" +
                                    "      <data name=\"OldAccntNumber1\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"accntNumber1\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"111\" />" + "\r\n" +
                                    "      <data name=\"Search\" disable=\"false\" readOnly=\"false\" visibility=\"visible\" value=\"搜索(&lt;U&gt;S&lt;/U&gt;)\" />" + "\r\n" +
                                    "      <data name=\"CardNumber\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"NewVoucherType\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"2201\" />" + "\r\n" +
                                    "      <data name=\"NewCertificateNo\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"111\" />" + "\r\n" +
                                    "      <data name=\"BusinessType\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"1\" />" + "\r\n" +
                                    "      <data name=\"promono\" disable=\"false\" readOnly=\"false\" visibility=\"hidden\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"VolumeNum\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"OldVoucherType\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"OldCertificateNo\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"WithdrawalType\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"6\" />" + "\r\n" +
                                    "      <data name=\"WithdrawalPwd\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"ReportLostJnl\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"ReportLostDt\" disable=\"false\" readOnly=\"false\" visibility=\"\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"DefaultString1\" disable=\"false\" readOnly=\"false\" visibility=\"hidden\" value=\"N\" />" + "\r\n" +
                                    "      <data name=\"IBDType\" disable=\"false\" readOnly=\"false\" visibility=\"hidden\" value=\"01\" />" + "\r\n" +
                                    "      <data name=\"NewIBDNumber\" disable=\"false\" readOnly=\"false\" visibility=\"hidden\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"OffsetPos1\" disable=\"false\" readOnly=\"false\" visibility=\"hidden\" value=\"29\" />" + "\r\n" +
                                    "      <data name=\"OldFlagNo\" disable=\"false\" readOnly=\"false\" visibility=\"hidden\" value=\"1\" />" + "\r\n" +
                                    "      <data name=\"OldIBDNumber\" disable=\"false\" readOnly=\"false\" visibility=\"hidden\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"InputPasswordAgain\" disable=\"false\" readOnly=\"false\" visibility=\"hidden\" value=\"\" />" + "\r\n" +
                                    "      <data name=\"Transmit\" disable=\"false\" readOnly=\"false\" visibility=\"visible\" value=\"提交(&lt;U&gt;T&lt;/U&gt;)\" />" + "\r\n" +
                                    "      <data name=\"Close\" disable=\"false\" readOnly=\"false\" visibility=\"visible\" value=\"关闭(&lt;U&gt;C&lt;/U&gt;)\" />" + "\r\n" +
                                    "    </data>" + "\r\n" +
                                    "  </transactionDatas>" + "\r\n" +
                                    "  <attachments />" + "\r\n" +
                                    "  <transactionMsg>" + "\r\n" +
                                    "    <header result=\"DefaultTransmit\">" + "\r\n" +
                                    "      <flow ID=\"StandardFlow\" nodeID=\"ShowScreen\" flowInstanceID=\"62bd4f0c-708d-49b1-a0ef-e24e8e28893e\" flowInstanceCount=\"8\" flowPersistenceID=\"\">" + "\r\n" +
                                    "        <fileName>xml/workflows/StandardFlow.xml</fileName>" + "\r\n" +
                                    "        <declares>" + "\r\n" +
                                    "          <variable name=\"screenSettings\" type=\"variable\" scope=\"in\" />" + "\r\n" +
                                    "          <variable name=\"userInfo\" type=\"variable\" scope=\"inout\" />" + "\r\n" +
                                    "          <variable name=\"hostDataOut\" type=\"variable\" scope=\"in\" />" + "\r\n" +
                                    "          <variable name=\"screenData\" type=\"variable\" scope=\"in\" />" + "\r\n" +
                                    "          <variable name=\"IsDialogFlow\" type=\"variable\" scope=\"in\" />" + "\r\n" +
                                    "          <variable name=\"Track123Data\" type=\"variable\" scope=\"out\" />" + "\r\n" +
                                    "          <variable name=\"uuid\" type=\"variable\" scope=\"inout\" />" + "\r\n" +
                                    "        </declares>" + "\r\n" +
                                    "        <sequence flowID=\"\" nodeID=\"\" outcome=\"true\" />" + "\r\n" +
                                    "      </flow>" + "\r\n" +
                                    "      <component className=\"Screen\" assemblyName=\"\" runAt=\"client\" version=\"3.0\">" + "\r\n" +
                                    "        <method name=\"show\" result=\"DefaultTransmit\" version=\"1.0\">" + "\r\n" +
                                    "          <RXSection type=\"xpath\">body/hostDataOut</RXSection>" + "\r\n" +
                                    "          <TXSection type=\"xpath\">body/screenData</TXSection>" + "\r\n" +
                                    "          <screen type=\"xpath\">body/screenSettings/screen</screen>" + "\r\n" +
                                    "          <enabled type=\"literal\">true</enabled>" + "\r\n" +
                                    "          <passThrough type=\"literal\">false</passThrough>" + "\r\n" +
                                    "          <multicurrencyfill>false</multicurrencyfill>" + "\r\n" +
                                    "          <reversefill>false</reversefill>" + "\r\n" +
                                    "          <repaittxn>false</repaittxn>" + "\r\n" +
                                    "          <reversescreen>false</reversescreen>" + "\r\n" +
                                    "          <ViewMode>false</ViewMode>" + "\r\n" +
                                    "          <ScreenType type=\"xpath\">body/screentype</ScreenType>" + "\r\n" +
                                    "          <display type=\"xpath\">body/screenSettings/display</display>" + "\r\n" +
                                    "          <AutoCloseScreen type=\"literal\" />" + "\r\n" +
                                    "          <top type=\"literal\" />" + "\r\n" +
                                    "          <left type=\"literal\" />" + "\r\n" +
                                    "          <editMode type=\"literal\" />" + "\r\n" +
                                    "          <WindowID type=\"literal\" />" + "\r\n" +
                                    "          <ParentID type=\"literal\" />" + "\r\n" +
                                    "          <Target type=\"literal\" />" + "\r\n" +
                                    "          <InvokeAction type=\"literal\" />" + "\r\n" +
                                    "          <Param1 type=\"literal\" />" + "\r\n" +
                                    "          <Param2 type=\"literal\" />" + "\r\n" +
                                    "          <Param3 type=\"literal\" />" + "\r\n" +
                                    "          <Param4 type=\"literal\" />" + "\r\n" +
                                    "          <Param5 type=\"literal\" />" + "\r\n" +
                                    "          <DisplayFor type=\"literal\" />" + "\r\n" +
                                    "          <NeedState type=\"xpath\">body/needState</NeedState>" + "\r\n" +
                                    "        </method>" + "\r\n" +
                                    "      </component>" + "\r\n" +
                                    "      <returnStack />" + "\r\n" +
                                    "      <index>1</index>" + "\r\n" +
                                    "    </header>" + "\r\n" +
                                    "  </transactionMsg>" + "\r\n" +
                                    "</transactionState>"
                 );

            content = blpf.BLPHttpBot("Message=",
                                          "Url=http://22.188.155.125:8200/Menu.aspx?sourceFile=XML%2fMENUS%2fONLINE001.XML&userType=001",
                                          "Mothod=GET",
                                          "ContentType=application/octet-stream",
                                          "Referer="
                                          );

            content = blpf.BLPHttpBot("Message=",
                                          "Url=http://22.188.155.41:8000/Release/resource/Center/Common/zh-CHS/Images/BTM_BGRND.jpg",
                                          "Mothod=POST",
                                          "ContentType=image/gif",
                                          "Referer=http://22.188.155.41:8000/Release/resource/Center/HeadOffice/zh-CHS/html/032026.htm"
                                          );




            content = blpf.BLPHttpBot("Message=<componentDocuments><componentDocument Version=\"3.0\"><header result=\"DefaultTransmit\"><flow ID=\"StandardFlow\" nodeID=\"ShowScreen\" flowInstanceID=\"e1b8799e-f0d2-f003-ddbe-d71356adbeb6\" flowInstanceCount=\"8\" flowPersistenceID=\"\"><fileName>xml/workflows/StandardFlow.xml</fileName><declares><variable name=\"screenSettings\" type=\"variable\" scope=\"in\" /><variable name=\"userInfo\" type=\"variable\" scope=\"inout\" /><variable name=\"hostDataOut\" type=\"variable\" scope=\"in\" /><variable name=\"screenData\" type=\"variable\" scope=\"in\" /><variable name=\"IsDialogFlow\" type=\"variable\" scope=\"in\" /><variable name=\"Track123Data\" type=\"variable\" scope=\"out\" /><variable name=\"uuid\" type=\"variable\" scope=\"inout\" /></declares><sequence flowID=\"\" nodeID=\"\" outcome=\"true\" /></flow><component className=\"Screen\" assemblyName=\"\" runAt=\"client\" version=\"3.0\"><method name=\"show\" result=\"DefaultTransmit\" version=\"1.0\"><RXSection type=\"xpath\">body/hostDataOut</RXSection><TXSection type=\"xpath\">body/screenData</TXSection><screen type=\"xpath\">body/screenSettings/screen</screen><enabled type=\"literal\">true</enabled><passThrough type=\"literal\">false</passThrough><multicurrencyfill>false</multicurrencyfill><reversefill>false</reversefill><repaittxn>false</repaittxn><reversescreen>false</reversescreen><ViewMode>false</ViewMode><ScreenType type=\"xpath\">body/screentype</ScreenType><display type=\"xpath\">body/screenSettings/display</display><AutoCloseScreen type=\"literal\" /><top type=\"literal\" /><left type=\"literal\" /><editMode type=\"literal\" /><WindowID type=\"literal\" /><ParentID type=\"literal\" /><Target type=\"literal\" /><InvokeAction type=\"literal\" /><Param1 type=\"literal\" /><Param2 type=\"literal\" /><Param3 type=\"literal\" /><Param4 type=\"literal\" /><Param5 type=\"literal\" /><DisplayFor type=\"literal\" /><NeedState type=\"xpath\">body/needState</NeedState></method></component><returnStack /><index>1</index></header><body><userInfo persist=\"true\"><InstitutionNo>3</InstitutionNo><BranchNo>10001</BranchNo><TellerNo>10001</TellerNo><TellerName></TellerName><WorkstationNo>48</WorkstationNo><Capability>5</Capability><HostDate>11/05/2013</HostDate><LOGONSTATUS>LOGGEDON</LOGONSTATUS><Mode>ONLINE</Mode><BranchStatus>OPEN</BranchStatus><UserType>1</UserType><UserSection></UserSection><URL></URL><ProvinceCode>13</ProvinceCode><MACAddress>00238B837301</MACAddress><MachineName>郭鑫</MachineName><BranchNoFromPBC></BranchNoFromPBC><ProvinceBranchNo>4</ProvinceBranchNo></userInfo><screenData persist=\"True\"><ProvinceCode1>13</ProvinceCode1><accntNumber1>123</accntNumber1><NewVoucherType>2201</NewVoucherType><NewCertificateNo>456</NewCertificateNo><DefaultString2>1</DefaultString2><WithdrawalType>6</WithdrawalType><DefaultString1>N</DefaultString1><IBDType>01</IBDType><OffsetPos1>29</OffsetPos1><OldFlagNo>1</OldFlagNo><TranNo>000500</TranNo><TranName>\\\\ SCR:000500 DEP:补换发存折.</TranName><Screen>000500</Screen><HostJournalNo /></screenData><screenSettings persist=\"True\"><screen>000500.htm</screen><ReenableFlag></ReenableFlag></screenSettings><Track123Data persist=\"True\"><Track2Data /><Track3Data /><Track1Data /><Track4Data /><Accno /><AccType /><PassbookNo /><Seqno /><CheckStatus /></Track123Data><CashDrawer persist=\"True\"><CDID1></CDID1></CashDrawer><screentype persist=\"True\">MainScreen</screentype><transactionStates persist=\"True\"><timestamp>8cf2744189f3278</timestamp></transactionStates></body></componentDocument></componentDocuments>",
                                          "Url=http://22.188.155.151:8010/engine.aspx",
                                          "Mothod=POST",
                                          "ContentType=application/octet-stream",
                                          "Referer="
                                          );

            content = blpf.BLPHttpBot("Message=<componentDocuments><componentDocument Version=\"3.0\"><header result=\"SendForOverride\"><flow ID=\"SupervisorOverrideFlow\" nodeID=\"ShowModalDialog.16\" flowInstanceID=\"80964909-637f-4e46-b734-3c141e994521\" flowInstanceCount=\"38\" flowPersistenceID=\"\"><fileName>xml\\workflows\\SupervisorOverrideFlow.xml</fileName><declares><variable name=\"hostDataOut\" type=\"variable\" scope=\"in\">hostDataOut</variable><variable name=\"userInfo\" type=\"variable\" scope=\"in\">userInfo</variable><variable name=\"BLJournalID\" type=\"variable\" scope=\"in\">journalNo</variable><variable name=\"DeclineExit\" type=\"literal\" scope=\"in\" /><variable name=\"PreviouslyApprovedSUPErrors\" type=\"variable\" scope=\"inout\">PreviouslyApprovedSUPErrors</variable><variable name=\"hostDataIn\" type=\"variable\" scope=\"inout\">screenData</variable><variable name=\"supervisorData\" type=\"variable\" scope=\"out\">supervisorData</variable><variable name=\"QueueSupData\" type=\"variable\" scope=\"inout\">QueueSupData<variable name=\"localSupScreen\" type=\"literal\" scope=\"local\">LocalSupervisorOverrideRequired</variable></variable><variable name=\"screenData\" type=\"variable\" scope=\"in\">screenData</variable><variable name=\"Comments\" type=\"variable\" scope=\"out\">Comments</variable><variable name=\"DeleteQueueID\" type=\"variable\" scope=\"in\">DeleteQueueID</variable><variable name=\"screenSettings\" type=\"variable\" scope=\"inout\" /><variable name=\"ReenableFlag\" type=\"variable\" scope=\"inout\" /></declares><sequence flowID=\"\" nodeID=\"\" outcome=\"true\" /></flow><component className=\"Screen\" assemblyName=\"\" runAt=\"client\" version=\"3.0\"><method name=\"show\" result=\"SendForOverride\" version=\"1.0\"><RXSection type=\"xpath\">body/hostDataOut</RXSection><TXSection type=\"xpath\">body/queueData</TXSection><screen type=\"xpath\">body/screenSettings/screen</screen><enabled type=\"literal\">true</enabled><passThrough type=\"literal\" /><multicurrencyfill>false</multicurrencyfill><reversefill>false</reversefill><repaittxn>false</repaittxn><reversescreen>false</reversescreen><ViewMode>false</ViewMode><ScreenType type=\"literal\">DialogScreen</ScreenType><display type=\"literal\">Modal</display><AutoCloseScreen type=\"literal\" /><top type=\"literal\" /><left type=\"literal\" /><editMode type=\"xpath\">body/screenSettings/screen/screen</editMode><WindowID type=\"literal\" /><ParentID type=\"literal\" /><Target type=\"literal\" /><InvokeAction type=\"literal\" /><Param1 type=\"literal\" /><Param2 type=\"literal\" /><Param3 type=\"literal\" /><Param4 type=\"literal\" /><Param5 type=\"literal\" /></method></component><returnStack><flow ID=\"StandardFlow\" nodeID=\"Handle Output Types\" flowInstanceID=\"80964909-637f-4e46-b734-3c141e994521\" flowInstanceCount=\"16\" flowPersistenceID=\"\"><fileName>xml/workflows/StandardFlow.xml</fileName><declares><variable name=\"screenSettings\" type=\"variable\" scope=\"in\" /><variable name=\"userInfo\" type=\"variable\" scope=\"inout\" /><variable name=\"hostDataOut\" type=\"variable\" scope=\"in\" /><variable name=\"screenData\" type=\"variable\" scope=\"in\" /><variable name=\"IsDialogFlow\" type=\"variable\" scope=\"in\" /><variable name=\"Track123Data\" type=\"variable\" scope=\"out\" /><variable name=\"uuid\" type=\"variable\" scope=\"inout\" /></declares><sequence flowID=\"\" nodeID=\"\" outcome=\"true\" /></flow><flow ID=\"StandardFlowOutputTypeInterface\" nodeID=\"Supervisor Override\" flowInstanceID=\"80964909-637f-4e46-b734-3c141e994521\" flowInstanceCount=\"20\" flowPersistenceID=\"\"><fileName>xml\\workflows\\StandardFlowOutputTypeInterface.xml</fileName><declares><variable name=\"screenSettings\" type=\"variable\" scope=\"out\">screenSettings</variable><variable name=\"hostDataIn\" type=\"variable\" scope=\"in\">screenData</variable><variable name=\"journalNo\" type=\"variable\" scope=\"in\">journalNo</variable><variable name=\"handlePrinting\" type=\"literal\" scope=\"in\">True</variable><variable name=\"handleTellerMessages\" type=\"literal\" scope=\"in\">True</variable><variable name=\"handleSupervisorOverrides\" type=\"literal\" scope=\"in\">True</variable><variable name=\"handleEmail\" type=\"literal\" scope=\"in\">True</variable><variable name=\"handleOKMessages\" type=\"literal\" scope=\"in\">False</variable><variable name=\"handleErrorMessages\" type=\"literal\" scope=\"in\">True</variable><variable name=\"userInfo\" type=\"variable\" scope=\"in\">userInfo</variable><variable name=\"screenData\" type=\"variable\" scope=\"inout\">screenData</variable><variable name=\"CDTransactionID1\" type=\"variable\" scope=\"inout\">CDTransactionID1</variable><variable name=\"CDScreenDataIn1\" type=\"variable\" scope=\"inout\">CDScreenDataIn1</variable><variable name=\"cashDrawerData1\" type=\"variable\" scope=\"inout\">cashDrawerData1<variable name=\"CurrencyCode\" type=\"variable\" scope=\"inout\">cashDrawerData1/CurrencyCode</variable></variable><variable name=\"CDScreenDataLocalOut1\" type=\"variable\" scope=\"inout\">CDScreenDataLocalOut1</variable><variable name=\"hostDataOut\" type=\"variable\" scope=\"in\">hostDataOut</variable><variable name=\"Flag500\" type=\"variable\" scope=\"in\" /></declares><sequence flowID=\"\" nodeID=\"\" outcome=\"true\" /></flow></returnStack><index>1</index></header><body><userInfo persist=\"true\"><InstitutionNo>3</InstitutionNo><BranchNo>10001</BranchNo><TellerNo>10001</TellerNo><TellerName></TellerName><WorkstationNo>48</WorkstationNo><Capability>14</Capability><HostDate>11/05/2013</HostDate><LOGONSTATUS>LOGGEDON</LOGONSTATUS><Mode>ONLINE</Mode><BranchStatus>OPEN</BranchStatus><UserType>1</UserType><UserSection></UserSection><URL></URL><ProvinceCode>13</ProvinceCode><MACAddress>001E909BEFE3</MACAddress><MachineName>BOCUFUYFUYT</MachineName><BranchNoFromPBC></BranchNoFromPBC><ProvinceBranchNo>4</ProvinceBranchNo></userInfo><screenData persist=\"True\"><ProvinceCode1>13</ProvinceCode1><accntNumber1>102102010201020</accntNumber1><NewVoucherType>2201</NewVoucherType><NewCertificateNo>1221212</NewCertificateNo><DefaultString2>1</DefaultString2><WithdrawalType>1</WithdrawalType><DefaultString1>N</DefaultString1><IBDType>01</IBDType><OffsetPos1>29</OffsetPos1><OldFlagNo>1</OldFlagNo><TranNo>000500</TranNo><TranName>\\ SCR:000500 DEP:补换发存折.</TranName><Screen>000500</Screen><HostJournalNo /><Chk-Pwd></Chk-Pwd><Chk-IBDType /><Chk-IBDNo /><Chk-Amt /><Chk-Date /><Chk-OldAct /><Chk-Act /><Chk-VchrWdType /><DebugQueue>1</DebugQueue><ReenableFlag /><UUID>1200d2ddbd6843c2816052e575986c07</UUID><SupervisorID /></screenData><screenSettings persist=\"True\"><screen>000500.htm<assembly>CADlg.dll</assembly><type>BOC.WinFormUI.RemoteSupervisorOverrideDlg</type><screen></screen></screen><ReenableFlag></ReenableFlag></screenSettings><hostDataOut persist=\"True\"><Filler1></Filler1><MsgLen>201</MsgLen><Filler2>99</Filler2><MsgType>0</MsgType><Filler3>77</Filler3><CycleNo>0</CycleNo><MsgNo>0</MsgNo><SegNo>0</SegNo><SegNo2>1</SegNo2><FrontEndNo>0</FrontEndNo><TerminalNo>255498</TerminalNo><InstitutionNo>3</InstitutionNo><BranchNo>3255</BranchNo><WorkstationNo>498</WorkstationNo><TellerNo>4255498</TellerNo><TranNo>020030</TranNo><ACTION>020030</ACTION><JournalNo>8794484</JournalNo><HeaderDate>19122009</HeaderDate><Filler4></Filler4><Filler5></Filler5><Flag1></Flag1><Flag2>2</Flag2><Flag3></Flag3><Flag4>0</Flag4><Filler81></Filler81><Filler8>000000</Filler8><Filler82>0</Filler82><UUID>b0a36dd89d2347bf96e3f8e7f1425aa5</UUID><OutputType>01</OutputType><ErrorType>SUP</ErrorType><ErrorCode>6197</ErrorCode><Capability>5</Capability><ErrorMessage>强制核准，要求主管核准。</ErrorMessage><SupervisorID></SupervisorID><Screen>SupervisorOverrideRequired</Screen></hostDataOut><Track123Data persist=\"True\"><Track2Data /><Track3Data /><Track1Data /><Track4Data /><Accno /><AccType /><PassbookNo /><Seqno /><CheckStatus /></Track123Data><CashDrawer persist=\"True\"><CDID1></CDID1></CashDrawer><screentype persist=\"True\">MainScreen</screentype><transactionStates persist=\"True\"><timestamp>8cf2e450ab12272</timestamp></transactionStates><needState persist=\"True\"></needState><ReenableFlag persist=\"True\"></ReenableFlag><RatesFBFlag persist=\"True\">1</RatesFBFlag><journalNo persist=\"True\">299756</journalNo><LastOverridingTeller persist=\"True\"></LastOverridingTeller><supervisorData persist=\"True\"><LocalQueueID>1405</LocalQueueID></supervisorData><RemoteSupervisorData persist=\"True\"></RemoteSupervisorData><SupervisorInfo persist=\"True\"></SupervisorInfo><PrevApprovedError persist=\"True\"></PrevApprovedError><validateTellerData persist=\"True\"></validateTellerData><queueData persist=\"True\"><IsSOMS>1</IsSOMS><queueID>1405</queueID><IsRemote>1</IsRemote><IsForSequentialAuth>1</IsForSequentialAuth><SupervisorGroupName>SupGroup1</SupervisorGroupName><AuthorizationCenterName>Center1</AuthorizationCenterName><TransName>补换发存折</TransName><IsSequential>0</IsSequential><SequenceFlowID>046da648-ff32-4449-b4c7-da22e6c27f6b</SequenceFlowID><capability>5</capability><attachments /></queueData><tempData persist=\"True\"></tempData><QueueSupData persist=\"True\"><screen>SupervisorOverrideRequired</screen></QueueSupData><SupervisorOverrideFlowLocalData persist=\"True\"><BLJournalID>299756</BLJournalID></SupervisorOverrideFlowLocalData><PreviouslyApprovedSUPErrors persist=\"True\" /><SOMSServerCache persist=\"True\"><IsSequentialEnable>1</IsSequentialEnable><IsMultiple>0</IsMultiple><IsRefusePage>0</IsRefusePage></SOMSServerCache></body></componentDocument></componentDocuments>",
                                          "Url=http://22.188.155.151:8010/engine.aspx",
                                          "Mothod=POST",
                                          "ContentType=application/octet-stream",
                                          "Referer="
                                          ); 

            content = blpf.BLPHttpBot("Message=<componentDocuments><componentDocument version=\"2.0\"><header><flow ID=\"StandardFlow\" nodeID=\"MenuStart\"><declares></declares><fileName>xml/workflows/StandardFlow.xml</fileName></flow><component className=\"FNS.FrontEnd.Workflow.Server.FlowDistributor.CDistributor3\" assemblyName=\"FNSServerFlowDistributor3\" runAt=\"server\" version=\"3.0\"><method result=\"True\" name=\"startflow\"><fileName type=\"literal\">xml/workflows/StandardFlow.xml</fileName><flowID type=\"literal\">StandardFlow</flowID><nodeID type=\"literal\">MenuStart</nodeID><declares></declares></method></component></header><body><userInfo persist=\"true\"><InstitutionNo>3</InstitutionNo><BranchNo>{BranchNo}</BranchNo><TellerNo>{TellerNo}</TellerNo><TellerName></TellerName><WorkstationNo>48</WorkstationNo><Capability>{TellerCapability}</Capability><HostDate>11/05/2013</HostDate><LOGONSTATUS>LOGGEDON</LOGONSTATUS><Mode>ONLINE</Mode><BranchStatus>OPEN</BranchStatus><UserType>1</UserType><UserSection></UserSection><URL></URL><ProvinceCode>13</ProvinceCode><MACAddress>00238B837301</MACAddress><MachineName>郭鑫</MachineName><BranchNoFromPBC></BranchNoFromPBC><ProvinceBranchNo>4</ProvinceBranchNo></userInfo><screenData persist=\"true\"><TranNo>000500</TranNo></screenData><screenSettings persist=\"true\"><screen>000500.htm</screen></screenSettings></body></componentDocument></componentDocuments>",
                              "Url=http://22.188.155.151:8010/engine.aspx",
                              "Mothod=POST",
                              "ContentType=application/octet-stream",
                              "Referer="
                              );

            content = blpf.BLPHttpBot("Message=<componentDocuments><componentDocument Version=\"3.0\"><header result=\"DefaultTransmit\"><flow ID=\"StandardFlow\" nodeID=\"ShowScreen\" flowInstanceID=\"e1b8799e-f0d2-f003-ddbe-d71356adbeb6\" flowInstanceCount=\"8\" flowPersistenceID=\"\"><fileName>xml/workflows/StandardFlow.xml</fileName><declares><variable name=\"screenSettings\" type=\"variable\" scope=\"in\" /><variable name=\"userInfo\" type=\"variable\" scope=\"inout\" /><variable name=\"hostDataOut\" type=\"variable\" scope=\"in\" /><variable name=\"screenData\" type=\"variable\" scope=\"in\" /><variable name=\"IsDialogFlow\" type=\"variable\" scope=\"in\" /><variable name=\"Track123Data\" type=\"variable\" scope=\"out\" /><variable name=\"uuid\" type=\"variable\" scope=\"inout\" /></declares><sequence flowID=\"\" nodeID=\"\" outcome=\"true\" /></flow><component className=\"Screen\" assemblyName=\"\" runAt=\"client\" version=\"3.0\"><method name=\"show\" result=\"DefaultTransmit\" version=\"1.0\"><RXSection type=\"xpath\">body/hostDataOut</RXSection><TXSection type=\"xpath\">body/screenData</TXSection><screen type=\"xpath\">body/screenSettings/screen</screen><enabled type=\"literal\">true</enabled><passThrough type=\"literal\">false</passThrough><multicurrencyfill>false</multicurrencyfill><reversefill>false</reversefill><repaittxn>false</repaittxn><reversescreen>false</reversescreen><ViewMode>false</ViewMode><ScreenType type=\"xpath\">body/screentype</ScreenType><display type=\"xpath\">body/screenSettings/display</display><AutoCloseScreen type=\"literal\" /><top type=\"literal\" /><left type=\"literal\" /><editMode type=\"literal\" /><WindowID type=\"literal\" /><ParentID type=\"literal\" /><Target type=\"literal\" /><InvokeAction type=\"literal\" /><Param1 type=\"literal\" /><Param2 type=\"literal\" /><Param3 type=\"literal\" /><Param4 type=\"literal\" /><Param5 type=\"literal\" /><DisplayFor type=\"literal\" /><NeedState type=\"xpath\">body/needState</NeedState></method></component><returnStack /><index>1</index></header><body><userInfo persist=\"true\"><InstitutionNo>3</InstitutionNo><BranchNo>10001</BranchNo><TellerNo>10001</TellerNo><TellerName></TellerName><WorkstationNo>48</WorkstationNo><Capability>5</Capability><HostDate>11/05/2013</HostDate><LOGONSTATUS>LOGGEDON</LOGONSTATUS><Mode>ONLINE</Mode><BranchStatus>OPEN</BranchStatus><UserType>1</UserType><UserSection></UserSection><URL></URL><ProvinceCode>13</ProvinceCode><MACAddress>00238B837301</MACAddress><MachineName>郭鑫</MachineName><BranchNoFromPBC></BranchNoFromPBC><ProvinceBranchNo>4</ProvinceBranchNo></userInfo><screenData persist=\"True\"><ProvinceCode1>13</ProvinceCode1><accntNumber1>123</accntNumber1><NewVoucherType>2201</NewVoucherType><NewCertificateNo>456</NewCertificateNo><DefaultString2>1</DefaultString2><WithdrawalType>6</WithdrawalType><DefaultString1>N</DefaultString1><IBDType>01</IBDType><OffsetPos1>29</OffsetPos1><OldFlagNo>1</OldFlagNo><TranNo>000500</TranNo><TranName>\\ SCR:000500 DEP:补换发存折.</TranName><Screen>000500</Screen><HostJournalNo /></screenData><screenSettings persist=\"True\"><screen>000500.htm</screen><ReenableFlag></ReenableFlag></screenSettings><Track123Data persist=\"True\"><Track2Data /><Track3Data /><Track1Data /><Track4Data /><Accno /><AccType /><PassbookNo /><Seqno /><CheckStatus /></Track123Data><CashDrawer persist=\"True\"><CDID1></CDID1></CashDrawer><screentype persist=\"True\">MainScreen</screentype><transactionStates persist=\"True\"><timestamp>8cf2744189f3278</timestamp></transactionStates></body></componentDocument></componentDocuments>",
                                          "Url=http://22.188.155.151:8010/engine.aspx",
                                          "Mothod=POST",
                                          "ContentType=application/octet-stream",
                                          "Referer="
                                          );



            content = blpf.BLPHttpBot("Message=",
                                                "Uri=http://22.188.155.125:8200/TellerStatusChk.aspx?Teller=1923628&Branch=13169&",
                                                "Mothod=GET",
                                                "ContentType=text/html",
                                                "Referer="
                                                );
            content = blpf.BLPHttpBot("Message=",
                                                "Uri=http://22.188.155.42:8002/QueueReceive.aspx?TellerID=1923628&Capability=14&ProvinceCode=13&SupervisorStatusID=1&QueueTypeID=2&",
                                                "Mothod=GET",
                                                "ContentType=text/xml",
                                                "Referer="
                                                );
            content = blpf.BLPHttpBot("Message=",
                                         "Uri=http://22.188.155.41:8000/Release/resource/Center/Common/zh-CHS/CSS/Screen.css",
                                         "Mothod=GET",
                                         "ContentType=text/css",
                                         "Referer=http://22.188.155.41:8000/Release/resource/Center/HeadOffice/zh-CHS/html/001010.htm"
                                         );
            content = blpf.BLPHttpBot("Message=",
                             "Uri=http://22.188.155.41:8000/Release/resource/Center/HeadOffice/zh-CHS/scripts/001010.js",
                             "Mothod=GET",
                             "ContentType=application/x-javascript",
                             "Referer=http://22.188.155.41:8000/Release/resource/Center/HeadOffice/zh-CHS/html/001010.htm"
                             );


        }

        public string BLPHttpReadUploadFile(string fileName)
        {
            string hashAndUrl = "", content = "", str = "";
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

        private bool TransferTest(string hashAndUrl)
        {
            bool flag;
            string fileContent = hashAndUrl.Substring(0,hashAndUrl.IndexOf("|"));
            char[] data = fileContent.ToCharArray();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(data);
            string content = System.Text.Encoding.UTF8.GetString(bs);
            string hash = hashAndUrl.Substring(hashAndUrl.IndexOf("|")+1);
            Uri requestUri = new Uri(string.Format("{0}&hash={1}", "http://22.188.155.145:9002/WebUploadAsyncHandler.ashx?FileName=10001_20120816_7597ac367f3045a1a09c37e8f5234d85.bsms", hash));
            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.Method = "POST";
            request.KeepAlive = false;
            request.Timeout = 120000;
            request.Credentials = CredentialCache.DefaultCredentials;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bs, 0, bs.Length);
            requestStream.Flush();
            requestStream.Close();
            HttpWebResponse respose = null;
            try
            {
                respose = (HttpWebResponse)request.GetResponse();
                HttpStatusCode statusCode = respose.StatusCode;
                if (statusCode != HttpStatusCode.OK)
                {
                    throw new Exception("上传文件失败,服务器返回状态:" + statusCode.ToString());
                }
                string str3 = respose.Headers["result"];
                if (str3.Trim().Equals("0"))
                {
                    return true;
                }
                if (!str3.StartsWith("8"))
                {
                    //throw new Exception("上传'" + filename + "'失败,result:" + str3);
                }
                flag = true;
            }
            catch (WebException exception)
            {
                HttpWebResponse response = (HttpWebResponse)exception.Response;
                if (response != null)
                {
                    //throw new Exception("上传'" + filename + "'失败,result:" + response.StatusCode.ToString());
                }
                throw exception;
            }
            finally
            {
                try
                {
                    if (respose != null)
                    {
                        respose.Close();
                    }
                }
                catch
                {
                }
            }
            return flag;
        }


        private bool Transfer(string filename)
        {
            bool flag;
            byte[] data = File.ReadAllBytes(filename);
            string content = System.Text.Encoding.UTF8.GetString(data);
            string str = this.Sign(data);
            Uri requestUri = new Uri(string.Format("{0}?FileName={1}&hash={2}", "http://22.188.155.145:9002/WebUploadAsyncHandler.ashx", Path.GetFileName(filename), str));
            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.Method = "POST";
            request.KeepAlive = false;
            request.Timeout = 120000;
            request.Credentials = CredentialCache.DefaultCredentials;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Flush();
            requestStream.Close();
            HttpWebResponse respose = null;
            try
            {
                respose = (HttpWebResponse)request.GetResponse();
                HttpStatusCode statusCode = respose.StatusCode;
                if (statusCode != HttpStatusCode.OK)
                {
                    throw new Exception("上传文件失败,服务器返回状态:" + statusCode.ToString());
                }
                string str3 = respose.Headers["result"];
                if (str3.Trim().Equals("0"))
                {
                    return true;
                }
                if (!str3.StartsWith("8"))
                {
                    throw new Exception("上传'" + filename + "'失败,result:" + str3  );
                }
                flag = true;
            }
            catch (WebException exception)
            {
                HttpWebResponse response = (HttpWebResponse)exception.Response;
                if (response != null)
                {
                    throw new Exception("上传'" + filename + "'失败,result:" + response.StatusCode.ToString());
                }
                throw exception;
            }
            finally
            {
                try
                {
                    if (respose != null)
                    {
                        respose.Close();
                    }
                }
                catch
                {
                }
            }
            return flag;
        }

        private string Sign(byte[] data)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(data)).Replace("-", "");
        }


        public string BLPSocketBot(string ipAddr,int port,string msgStr)
        {
            TcpClient client = new TcpClient();
            NetworkStream stream = null;
            client.SendTimeout = Parameter.timeOut;
            string response = "";
            try
            {
                byte[] sendBuf = Encoding.ASCII.GetBytes(msgStr);
                client.Connect(ipAddr, port);
                stream = client.GetStream();
                stream.Write(sendBuf,0,sendBuf.Length);
                byte[] revBuf = new byte[4096];
                int received = stream.Read(revBuf, 0, 4096);
                if (received <= 0)
                {
                    return "1";
                }
                response = Encoding.Default.GetString(revBuf);
                stream.Close();
                client.Close();
                return response;
            }
            catch (SocketException ex)
            {
                return ex.Message;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (client.Connected)
                {
                    client.Close();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string path = "D:\\iDev\\Projects\\附件4：XML格式报文示例\\";
            string fileName = "cips.111.001.01.xml";

            byte[] bs = File.ReadAllBytes(path + fileName);
            string content = System.Text.Encoding.UTF8.GetString(bs).Replace("\r\n", "").Replace("\t","").Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

    }
}
