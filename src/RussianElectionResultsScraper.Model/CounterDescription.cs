using System.Drawing;
using System.Linq;

namespace RussianElectionResultsScraper.Model
{
    public class CounterDescription
        {
        public static Color[] counterColors = new string[]
                                                  {
                                                 "#418CF0", "#FCB441", "#E0400A", "#056492", "#BFBFBF", "#1A3B69", "#FFE382", "#129CDD", "#CA6B4B", "#005CDB", "#F3D288", "#506381", "#F1B9A8", "#E0830A", "#7893BE"
                                                  }.Select( ColorTranslator.FromHtml ).ToArray();

        private Color? _color;
        public virtual Election     Election { get; set; }
        public virtual int          Id { get; set; }
        public virtual string       Counter { get; set; }
        public virtual string       Name { get; set; }
        public virtual string       ShortName { get; set; }
        public virtual bool         IsCandidate { get; set; }

        public virtual Color        Color 
            { 
            get {
                if ( !this._color.HasValue )
                    {
                    if (Election != null && Election.Candidates.Contains(this))
                        return counterColors[Election.Candidates.IndexOf(this)];
                    else
                        return Color.Transparent;
                    }
                else
                    {
                    return this._color.Value;
                    }
                }
            set {
                this._color = value;
                }
            }

        public virtual string               HtmlColor
            {
            get
                {
                return this._color != null ? ColorTranslator.ToHtml( (Color) this._color ) : null;
                }

            set 
                {
                this._color = value != null ? (Color?)ColorTranslator.FromHtml( value ) : null;
                }
            }
        }
}
