using System.Drawing;

namespace RussianElectionResultsScraper.Model
{
    public class CounterDescription
        {
        public virtual Election     Election { get; set; }
        public virtual int          Id { get; set; }
        public virtual int          Counter { get; set; }
        public virtual string       Name { get; set; }
        public virtual string       ShortName { get; set; }
        public virtual Color        Color { get; set; }
        public virtual string       HtmlColor { get; set; }
        }
}
