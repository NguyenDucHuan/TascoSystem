using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.NotificationService.Core.Interfaces
{
    public interface IRabbitMQConnection
    {
        IConnection GetConnection();
    }
}
