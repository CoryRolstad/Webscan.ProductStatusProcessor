using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webscan.ProductStatusProcessor.Models
{
    public class KafkaSettings
    {
        public string Broker { get; set; }
        public string NotifierTopicName { get; set; }
        public string SchedulerTopicName { get; set; }
        public string SchedulerTopicGroupId { get; set; }
    }
}
