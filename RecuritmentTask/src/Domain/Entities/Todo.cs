namespace RecuritmentTask.src.Domain.Entities
{
    public class Todo
    {
        public int Id { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double CompletedPercentage { get; set; } // [0,1]

        public bool IsDone => CompletedPercentage >= 1.0;


    }
}
