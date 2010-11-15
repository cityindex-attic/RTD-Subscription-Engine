using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Microsoft.Office.Interop.Excel;
using SubscriptionEngine.Core.ActiveMQ;

namespace SubscriptionEngine.Core
{
    /// <summary>
    /// Use in Excel =RTD("SubscriptionEngine.ActiveMq",,"Excel.Test.Topic")
    /// </summary>
    [Guid("73A0D102-D998-4D33-B67D-2649FA9AEE74")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true), ProgId("SubscriptionEngine.ActiveMQ")]
    public class ActiveMQServer : IRtdServer
    {
        private IRTDUpdateEvent rtdCallback;
        private IConnection connection;
        private ISession session;
        private IConnectionFactory connectionFactory;

        private List<TopicSubscriber> currentTopicSubscribers = new List<TopicSubscriber>();
        private Dictionary<int, Listener> rtdTopicListeners = new Dictionary<int, Listener>();

        private bool isInitialised = false;
        private string lastErrorMsg;

        public string brokerUrl = "tcp://localhost:61616";

        public int ServerStart(IRTDUpdateEvent callback)
        {
            try
            {
                rtdCallback = callback;

                CreateActiveMQSession();

                isInitialised = true;

            }
            catch (Exception ex)
            {
                lastErrorMsg = ex.Message;
            }


            return 1;
        }

        private void CreateActiveMQSession()
        {
            connectionFactory = new ConnectionFactory(brokerUrl, GetType().Name);
            connection = connectionFactory.CreateConnection();
            connection.Start();
            session = connection.CreateSession();
        }

        public object ConnectData(int rtdTopicId, ref Array strings, ref bool newValues)
        {
            try
            {
                if (!isInitialised)
                {
                    // There was a problem setting up the connection properties
                    // so return the error and leave
                    return lastErrorMsg;
                }

                var topicName = strings.GetValue(0).ToString();

                // Have we all ready subscribed to this Topic
                // if we have we don't want to re-subscribe we can just use the other subscription
                // and have different check values
                var currentTopicSubscriber = GetCurrentTopicSubscriber(topicName);

                if (currentTopicSubscriber==null)
                {
                    // Create a new Consumer
                    currentTopicSubscriber = new TopicSubscriber(session, topicName);
                    currentTopicSubscriber.Start("myConsumerId");
                    currentTopicSubscribers.Add(currentTopicSubscriber);
                }

                var listener = new Listener(currentTopicSubscriber, rtdCallback);

                rtdTopicListeners.Add(rtdTopicId, listener);
                return "connected to ActiveMQ topic: " + topicName;
            }
            catch (Exception ex)
            {
                lastErrorMsg = ex.Message;
                return lastErrorMsg;
            }
        }

        private TopicSubscriber GetCurrentTopicSubscriber(string topicName)
        {
            TopicSubscriber topicSubscriber = null;
            foreach (var currentTopicSubscriber in currentTopicSubscribers)
            {
                if (currentTopicSubscriber.TopicName == topicName)
                {
                    return currentTopicSubscriber;
                }
            }
            return topicSubscriber;
        }

        public Array RefreshData(ref int rtdTopicCount)
        {
            rtdTopicCount = rtdTopicListeners.Count; //Tell Excel how many RTDs we're updating

            var data = new object[2, rtdTopicListeners.Count];
            var index = 0;

            foreach (var rtdTopicId in rtdTopicListeners.Keys)
            {
                data[0, index] = rtdTopicId;
                data[1, index] = rtdTopicListeners[rtdTopicId].LatestValue;
                ++index;
            }

            return data;
        }

        public void DisconnectData(int rtdTopicId)
        {
            rtdTopicListeners[rtdTopicId].StopListening();
            rtdTopicListeners.Remove(rtdTopicId);
        }

        public int Heartbeat()
        {
            // Check the Connection is Ok
            return 1;
        }

        public void ServerTerminate()
        {
            session.Close();
            session.Dispose();
            connection.Stop();
            connection.Close();
            connection.Dispose();
        }
    }
}
