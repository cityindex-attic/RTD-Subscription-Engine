using System;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using NUnit.Framework;
using SubscriptionEngine.Core.ActiveMQ;

namespace SubscriptionEngine.Core.IntegrationTests {
    
    [TestFixture]
    public class ActiveMQIntegrationTests 
    {
        private IConnectionFactory connectionFactory;
        private IConnection connection;
        private ISession session;

        [SetUp]
        public void SetUp()
        {
            connectionFactory = new ConnectionFactory("tcp://localhost:61616", GetType().Name);
            connection = connectionFactory.CreateConnection();
            connection.Start();
            session = connection.CreateSession();
        }

        [Test]
        public void CanPublishAndSubscribe() 
        {
            var messageToSend = DateTime.Now.ToLongTimeString();
            var recievedMessage = "";
            var waitForEvent = new EventWaitHandle(false,EventResetMode.AutoReset);
            using (var subscriber = new TopicSubscriber(session, "Excel.Test.Topic"))
            {
                subscriber.OnMessageRecieved += (sender, m) =>
                                                    {
                                                        waitForEvent.Set();
                                                        recievedMessage = m.Message;
                                                    };
                subscriber.Start("topic consumer");
                using (var publisher = new TopicPublisher(session, "Excel.Test.Topic"))
                {
                    publisher.SendMessage(messageToSend);
                }
                waitForEvent.WaitOne();
            }

            Assert.AreEqual(messageToSend, recievedMessage);
        }

        [Test]
        public void Publish10Messages()
        {
            using (var publisher = new TopicPublisher(session, "Excel.Test.Topic"))
            {
                for (int i = 0; i < 10; i++)
                {
                    publisher.SendMessage(DateTime.Now.ToLongTimeString());
                    Thread.Sleep(1000);
                }
            } 
        }

        [TearDown]
        public void TearDown()
        {
            session.Close();
            session.Dispose();
            connection.Stop();
            connection.Close();
            connection.Dispose();
        }
    }
}
