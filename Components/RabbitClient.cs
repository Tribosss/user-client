using Org.BouncyCastle.Ocsp;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace user_client.Components
{
    public class RabbitClient
    {

        private IConnection _conn;
        private IChannel _channel;
        private string _empId;
        private AgentClient _agcli;
        private BlockSiteClient _bscli;

        public RabbitClient(string empId)
        {
            _empId = empId;
            _agcli = new AgentClient();
            _bscli = new BlockSiteClient();
        }

        public void StartAgent(string empId)
        {
            if (_conn == null || _channel == null || !_conn.IsOpen || !_channel.IsOpen)
                ConnectRabbitServer();

            _agcli.StartAgent(empId);
        }

        public void ParseAdminMessage(string msg)
        {
            string[] msgs = msg.Split("<");
            string policyType = msgs[0];
            string data = msgs[1].Split(">")[0];

            switch (policyType)
            {
                case "AGENT":
                    {
                        if (data == "OFF")
                        {
                            _agcli.KillAgent();
                        }
                        else if (data == "ON")
                        {
                            StartAgent(_empId);
                        }
                        break;
                    }
                case "DOMAIN":
                    {
                        if (data == "CLEAR")
                        {
                            _bscli.ClearDomain();
                            break;
                        }

                        bool isExistDomain = _bscli.IsExistDomain(data);

                        if (isExistDomain) {
                            _bscli.RemoveDomain(data);
                        } else
                        {
                            _bscli.BlockDomain(data);
                        }
                        break;
                    }
            }

        }
        public async Task ConnectRabbitServer()
        {
            if (_empId == null || string.IsNullOrEmpty(_empId)) return;

            string queueName = $"client_{_empId}";
            string exchangeName = "tribosss";
            string routingKey = $"policy.set.{_empId}";
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                ClientProvidedName = $"[{_empId}]"
            };
            _conn = await factory.CreateConnectionAsync();
            _channel = await _conn.CreateChannelAsync();
            await _channel.QueueDeclareAsync(
                queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
            await _channel.QueueBindAsync(
                queueName,
                exchangeName,
                routingKey,
                null
            );
            await _channel.ExchangeDeclareAsync(
                exchangeName,
                ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += ReceivedMessageAtAdmin;

            string consumerTag = await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );
        }

        public async Task ReceivedMessageAtAdmin(object model, BasicDeliverEventArgs ea)
        {
            byte[] body = ea.Body.ToArray();
            string msg = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received: [{msg}]");

            ParseAdminMessage(msg);

            await _channel.BasicAckAsync(
                deliveryTag: ea.DeliveryTag,
                multiple: false
            );
        }
    }
}
