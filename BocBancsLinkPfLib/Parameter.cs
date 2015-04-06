using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BocBancsLinkPfLib
{
    public static class Parameter
    {
        public static int timeOutValue = 120000;
        public static bool IsCredentialsValue = true;
        public static bool IsUploadFileValue = false;
        public static bool IsKeepAliveValue = true;
        public static bool IsUserAgentValue = false;
        public static bool IsUserCacheValue = false;
        public static bool IsAsyncValue = false;
        public static int SocketRecevieBufferLengthValue = 4096;
        #if DEBUG
        public static bool IsLog = true;
        #endif
        public static string UserAgentValue = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0)";
        public static string AcceptValue = "text/html,application/xhtml+xml,application/xml,image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";

        public static int timeOut
        {
            get
            {
                return timeOutValue;
            }
            set
            {
                timeOutValue = value;
            }
        }
        public static bool IsCredentials
        {
            get
            {
                return IsCredentialsValue;
            }
            set
            {
                IsCredentialsValue = value;
            }
        }
        public static bool IsUploadFile
        {
            get
            {
                return IsUploadFileValue;
            }
            set
            {
                IsUploadFileValue = value;
            }
        }
        public static string UserAgent
        {
            get
            {
                return UserAgentValue;
            }
            set
            {
                UserAgentValue = value;
            }
        }
        public static string ClientAccept
        {
            get
            {
                return AcceptValue;
            }
            set
            {
                AcceptValue = value;
            }
        }
        public static bool IsKeepAlive
        {
            get
            {
                return IsKeepAliveValue;
            }
            set
            {
                IsKeepAliveValue = value;
            }
        }
        public static bool IsUserAgent
        {
            get
            {
                return IsUserAgentValue;
            }
            set
            {
                IsUserAgentValue = value;
            }
        }
        public static bool IsUserCache
        {
            get
            {
                return IsUserCacheValue;
            }
            set
            {
                IsUserCacheValue = value;
            }
        }

        public static bool IsAsync
        {
            get
            {
                return IsAsyncValue;
            }
            set
            {
                IsAsyncValue = value;
            }
        }

        public static int SocketRecevieBufferLength
        {
            get
            {
                return SocketRecevieBufferLengthValue;
            }
            set
            {
                SocketRecevieBufferLengthValue = value;
            }
        }
    }
}
