using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBM.WMQ.PCF;
using IBM.WMQ;

namespace BocBancsLinkLib
{
    public class MQNet
    {
        //本地代列管理器名称，可通过控制台创建！
        public void CreateQueueManage(string queueManageName)
        {
            throw new Exception("本地代列管理器名称，可通过控制台创建！");
        }

        //创建本地队列
        public void CreateQueue(string queueManageName, string queueName)
        {
            PCFMessageAgent agent = new PCFMessageAgent();
            PCFMessage pcfMessage = new PCFMessage(CMQCFC.MQCMD_CHANGE_Q);
            try
            {
                pcfMessage.AddParameter(MQC.MQCA_Q_NAME, queueName);
                pcfMessage.AddParameter(MQC.MQIA_Q_TYPE, MQC.MQQT_LOCAL);
                PCFMessage[] response = agent.Send(pcfMessage);
            }
            catch (MQException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (agent != null)
                {
                    agent.Disconnect();
                }
            }
        }

        //创建远程队列
        public void CreateRemoteQueue(string queueManageName, string queueName,string remoteQueueManageName,string remoteQueueName,string transferQueueName)
        {
            PCFMessageAgent agent = new PCFMessageAgent();
            PCFMessage pcfMessage = new PCFMessage(CMQCFC.MQCMD_CHANGE_Q);
            try
            {
                pcfMessage.AddParameter(MQC.MQCA_Q_NAME, queueName);
                pcfMessage.AddParameter(MQC.MQIA_Q_TYPE, MQC.MQQT_REMOTE);
                pcfMessage.AddParameter(MQC.MQCA_REMOTE_Q_MGR_NAME,remoteQueueManageName);
                pcfMessage.AddParameter(MQC.MQCA_REMOTE_Q_NAME,remoteQueueName);
                pcfMessage.AddParameter(MQC.MQCA_XMIT_Q_NAME,transferQueueName);
                PCFMessage[] response = agent.Send(pcfMessage);
            }
            catch (MQException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (agent != null)
                {
                    agent.Disconnect();
                }
            }
        }

        //创建通道
        public void CreateChannel(bool sendChannel, bool recevieChannel, string queueManageName, string channelName, string connectionName, string transferQueueName)
        {
            PCFMessageAgent agent = new PCFMessageAgent(queueManageName);
            PCFMessage pcfMessage = new PCFMessage(CMQCFC.MQCMD_CREATE_CHANNEL);
            try
            {
                pcfMessage.AddParameter(CMQCFC.MQCACH_CHANNEL_NAME, channelName);
                if (sendChannel)
                {
                    pcfMessage.AddParameter(CMQCFC.MQIACH_CHANNEL_TYPE, MQC.MQCHT_SENDER);
                    pcfMessage.AddParameter(CMQCFC.MQCACH_CONNECTION_NAME, connectionName);
                    pcfMessage.AddParameter(CMQCFC.MQCACH_XMIT_Q_NAME, transferQueueName);
                }
                PCFMessage[] response = agent.Send(pcfMessage);
            }
            catch (MQException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (agent != null)
                {
                    agent.Disconnect();
                }
            }
        }

        //删除队列
        public void DeleteQueue(string queueManageName, string queueName)
        {
            PCFMessageAgent agent = new PCFMessageAgent(queueManageName);
            PCFMessage pcfMessage = new PCFMessage(CMQCFC.MQCMD_DELETE_Q);
            try
            {
                pcfMessage.AddParameter(MQC.MQCA_Q_NAME, queueName);
                PCFMessage[] response = agent.Send(pcfMessage);
            }
            catch (MQException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (agent != null)
                {
                    agent.Disconnect();
                }
            }
        }

        //发送消息
        public void SendMessage(string queueManageName, string queueName,string body)
        {
            MQQueueManager queueManager = new MQQueueManager(queueManageName);
            MQQueue queue = queueManager.AccessQueue(queueName,MQC.MQOO_OUTPUT);
            try
            {
                MQMessage message = new MQMessage();
                message.WriteString(body);
                message.Format = MQC.MQFMT_STRING;
                queue.Put(message);
            }
            catch (MQException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                queue.Close();
                queueManager.Disconnect();
            }
        }

        //获取消息
        public string GetMessage(string queueManageName, string queueName)
        {
            MQQueueManager queueManager = new MQQueueManager(queueManageName);
            MQQueue queue = queueManager.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF|MQC.MQOO_FAIL_IF_QUIESCING);
            try
            {
                MQMessage message = new MQMessage();
                queue.Get(message);
                return message.ReadString(message.MessageLength);
            }
            catch (MQException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                queue.Close();
                queueManager.Disconnect();
            }
        }

        //等待时间获取消息
        public string GetMessage(string queueManageName, string queueName, int waitTime,out MQQueueManager queueManager)
        {
            queueManager = new MQQueueManager(queueManageName);
            MQQueue queue = queueManager.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);
            MQGetMessageOptions messageOptions = new MQGetMessageOptions();
            try
            {
                messageOptions.Options = MQC.MQGMO_WAIT;
                messageOptions.WaitInterval = waitTime;
                messageOptions.MatchOptions = MQC.MQMO_NONE;
                MQMessage message = new MQMessage();
                queue.Get(message, messageOptions);
                return message.ReadString(message.MessageLength);
            }
            catch (MQException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                queue.Close();
                queueManager.Disconnect();
            }
        }
    }
}
