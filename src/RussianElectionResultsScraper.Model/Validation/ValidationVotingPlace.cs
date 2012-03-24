using System;
using System.Collections.Generic;
using System.Linq;
using RussianElectionResultsScraper.Model.Validation;

namespace RussianElectionResultsScraper.Model
{

    public class ValidationVotingPlace
        {
        public string                                     Id;
        public string                                     ParentId;
        public Model.Type                                 Type;
        public Dictionary<string, ValidationVotingResult> Results = new Dictionary<string, ValidationVotingResult>();
        public List<ValidationVotingPlace>                Children = new List<ValidationVotingPlace>();
        private ValidationVotingPlace                     _parent;


        public ValidationVotingPlace    Parent
            {
            get {
                return _parent;
                }
            set {
                if ( this._parent != null) this._parent.Children.Remove( this );
                this._parent = value;
                if ( this._parent != null) this._parent.Children.Add( this );
                }
            }

        public IList<ValidationRule>                        GetCheckRules()
            {
            return new ValidationRule[]
                       {
                       new ChildCounterIsMissingRule( this ),
                       new ChildCounterIsMissingInParentRule( this ),
                       new ParentChildrenSumMismatchRule( this ),
                       new AbsenteeBallotsCountersMismatchRule( this )
                       };
            }

        public IEnumerable<ValidationProblem> Check()
            {
             var r = new List<ValidationProblem>();
            foreach ( var c in this.Children )
                {
                r.AddRange( c.Check());
                }
            var r2 = this.CheckItself().ToList();
            r.AddRange( r2 );
            return r;
            }

        private IEnumerable<ValidationProblem> CheckItself()
            {
            if ( this.Children.Any() )
                {
                foreach( var counter in this.Results.Values )
                    {
                    ValidationVotingResult counter1 = counter;
                    bool foundProblems = false;
                    foreach ( var rule in this.GetCheckRules() )
                        {
                        foreach ( var r in rule.Check(counter1) )
                            {
                            yield return r;
                            foundProblems = true;
                            }
                        if ( foundProblems )
                            break;
                        }
                    }
                }

            foreach (var v in GetCheckRules().SelectMany(x => x.Check()) )
                yield return v;
            }

        class HierarchicalCounters : Dictionary<Type, Dictionary<string,string>>
            {
            public string ChildCounter( Type type, string counter )
                {
                if ( !Counters.Hierarchical.Contains(counter) ) 
                    throw new Exception( string.Format( "Counter {0} is not hierarchical", counter ) );

                var a = this[ type ];
                if ( a.ContainsKey( counter ) )
                    {
                    return a[ counter ];
                    }
                else
                    {
                    throw new Exception( string.Format( "Counter \"{0}\" is not allowed for {1}", counter, type ) );
                    }
                }
            }

        public int? GetCounter(string counter)
            {
            ValidationVotingResult vr;
            this.Results.TryGetValue(counter, out vr);
            return vr != null ? vr.Value : (int?)null;
            }
 
        static readonly HierarchicalCounters dd = new HierarchicalCounters()
                                                    {
                                                        { Type.CIK, new Dictionary<string,string>()
                                                                        {
                                                                        { Counters.И, Counters.д },
                                                                        { Counters.К, Counters.е },
                                                                        { Counters.Л, Counters.ж },
                                                                        { Counters.М, Counters.з }
                                                                        }
                                                        },
                                                        { Type.Summary, new Dictionary<string,string>()
                                                                        {
                                                                        { Counters.а, Counters.а },
                                                                        { Counters.б, Counters.б },
                                                                        { Counters.в, Counters.в },
                                                                        { Counters.г, Counters.г },
                                                                        { Counters.д, Counters.д },
                                                                        { Counters.е, Counters.е },
                                                                        { Counters.ж, Counters.ж },
                                                                        { Counters.з, Counters.з }
                                                                        }
                                                        },
                                                        { Type.RIK, new Dictionary<string,string>()
                                                                        {
                                                                        { Counters.д, Counters.а },
                                                                        { Counters.е, Counters.б },
                                                                        { Counters.ж, Counters.в },
                                                                        { Counters.з, Counters.г }
                                                                        }
                                                            },
                                                        { Type.TIK, new Dictionary<string,string>()
                                                                        {
                                                                        { Counters.а, null },
                                                                        { Counters.б, null },
                                                                        { Counters.в, null },
                                                                        { Counters.г, null }
                                                                        }
                                                        }
                                                    };

        public string ChildCounter( string counter ) 
            {
            return Counters.Hierarchical.Contains(counter) 
                ? dd.ChildCounter(this.Type, counter)
                : counter;
            }

        }
}
