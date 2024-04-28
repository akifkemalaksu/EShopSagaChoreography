using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ.Settings
{
    public class EventBusSettings
    {
        public string ConnectionUrl { get; set; }
        public string ClientName { get; set; }
    }
}
