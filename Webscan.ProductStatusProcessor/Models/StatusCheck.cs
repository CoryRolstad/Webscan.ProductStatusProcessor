using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webscan.ProductStatusProcessor.Models
{
    public class StatusCheck
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string XPath { get; set; }
        public string XPathContentFailureString { get; set; }
        public string Url { get; set; }
        // Is Rendering JS required before we can get the xPath?
        public bool RenderingJavasciptRequired { get; set; }
        // Shortened Bitly URL - So users can get it in text.
        public string BitlyShortenedUrl { get; set; }
        // Can either have CronExpression for more then 1 minute increments, Cron takes precidense
        public string CronExpression { get; set; }
        // Or can have QueryTimeInSeconds (will query every X seconds)
        public int QueryTimeInSeconds { get; set; }
        // Time when the statuscheck is put onto the queue
        [NotMapped]
        public DateTime TimeScheduled { get; set; }
        // Last Time the users were notified (for notification cool down, so we aren't spamming users)
        public DateTime LastNotified { get; set; }

    }
}
