using System;
using Microsoft.Office.Interop.Excel;
using SubscriptionEngine.Core.ActiveMQ;

namespace SubscriptionEngine.Core
{
    public class Listener
    {
        private IRTDUpdateEvent rtdUpdateEvent;
        public TopicSubscriber TopicSubscriber { get; private set; }
        public string LatestValue { get; private set; }

        public Listener(TopicSubscriber topicSubscriber, IRTDUpdateEvent callback)
        {
            LatestValue = "no update recieved";
            this.rtdUpdateEvent = callback;
            this.TopicSubscriber = topicSubscriber;
            TopicSubscriber.OnMessageRecieved += OnMessageRecieved; 
        }

        private void OnMessageRecieved(object sender, MessageEventArgs messageEventArgs)
        {
            LatestValue = messageEventArgs.Message;
            rtdUpdateEvent.UpdateNotify();
        }

        public void StopListening()
        {
            TopicSubscriber.OnMessageRecieved -= OnMessageRecieved;
        }

//        #region Implementation of IMessageListener
//
//        public void OnMessage(IMessage message)
//        {
//            try
//            {
//                if (m_correlationID != "")
//                {
//                    if (m_correlationID != message.NMSCorrelationID)
//                    {
//                        return;
//                    }
//                }
//
//                bool found = false;
//                if (m_mappedField != "")
//                {
                    // We have a Mapped Field so check for the Mapp Message
//                    IMapMessage mapMessage = null;
//                    IEnumerator e;
//                    try
//                    {
//                        mapMessage = message as IMapMessage;
//                        e = mapMessage.Body.Keys.GetEnumerator();
//                    }
//                    catch (Exception)
//                    {
//                        throw new Exception("A parameter for a Map message was passed but the message type is not mapped");
//                    }
//
//                    while (e.MoveNext())
//                    {
//                        String key = (String)e.Current;
//                        if (key == m_mappedField)
//                        {
//                            if (m_text != "")
//                            {
//                                if (m_text != null)
//                                {
//                                    m_text = (String)mapMessage.Body[key];
//                                }
//                            }
//                            found = true;
//                            break;
//                        }
//                    }
//                    if (!found)
//                    {
//                        m_text = "The Key " + m_mappedField + " was not found in this message";
//                    }
//
//                }
//                else
//                {
//                    m_text = ((Apache.NMS.ActiveMQ.Commands.ActiveMQTextMessage)(message)).Text;
//                }
//
//
//            }
//            catch (Exception exc)
//            {
//                m_text = exc.Message;
//            }
//
//            if (m_text == "")
//            {
//                return;
//            }
//
//            if (m_text == null)
//            {
//                return;
//            }
//
//            try
//            {
//                m_callback.UpdateNotify();
//            }
//            catch (Exception)
//            {
                //
//            }
//        }
//
//        #endregion
    }
}
