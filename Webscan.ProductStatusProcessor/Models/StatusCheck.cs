namespace Webscan.ProductStatusProcessor.Models
{
    public class StatusCheck
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string XPath { get; set; }
        public string XPathContentFailureString { get; set; }
        public string Url { get; set; }
        // Can either have CronExpression for more then 1 minute increments, Cron takes precidense
        public string CronExpression { get; set; }
        // Or can have QueryTimeInSeconds (will query every X seconds)
        public int QueryTimeInSeconds { get; set; }

    }
}
