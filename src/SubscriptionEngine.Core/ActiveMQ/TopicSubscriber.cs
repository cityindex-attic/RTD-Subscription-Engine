using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;

namespace SubscriptionEngine.Core.ActiveMQ
{
    public class TopicSubscriber:IDisposable
    {
        private bool disposed;
        private readonly ISession session;
        private readonly ITopic topic;

        public TopicSubscriber(ISession session, string topicName)
        {
            this.session = session;
            TopicName = topicName;
            topic = new ActiveMQTopic(topicName);
        }

        public event EventHandler<MessageEventArgs> OnMessageRecieved;
        public IMessageConsumer Consumer { get; private set; }
        public string ConsumerId { get; private set; }
        public string TopicName { get; private set; }


        public void Start(string consumerId)
        {
            ConsumerId = consumerId;
            Consumer = session.CreateDurableConsumer(topic, consumerId, null, false);
            Consumer.Listener += (message =>
                                      {
                                          var textMessage = message as ITextMessage;
                                          if (textMessage == null) throw new InvalidCastException();
                                          if (OnMessageRecieved != null)
                                          {
                                              OnMessageRecieved(this, new MessageEventArgs(textMessage.Text));
                                          }
                                      });
        }

        public void Dispose()
        {
            if (disposed) return;
            if (Consumer != null)
            {
                Consumer.Close();
                Consumer.Dispose();
            }
            disposed = true;
        }

    }
}
