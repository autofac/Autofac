
namespace Remember.Model
{
    public class Task : IIdentifiable
    {
        public virtual int Id { get; set; }

        public virtual string Title { get; set; }

        public virtual bool IsComplete { get; set; }
    }
}
