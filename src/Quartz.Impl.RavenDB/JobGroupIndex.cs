using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;

namespace Quartz.Impl.RavenDB
{
    internal class JobGroupIndex : AbstractIndexCreationTask<Job, JobGroupIndex.Result>
    {
        public class Result
        {
            public string Group { get; set; }
            public int Count { get; set; }
        }

        public JobGroupIndex()
        {
            Map = entries =>
                from entry in entries
                select new
                {
                    Group = entry.Group,
                    Count = 1
                };

            Reduce = results =>
                from result in results
                group result by result.Group
                into g
                select new
                {
                    Group = g.Key,
                    Count = g.Sum(a => a.Count)
                };
        }
    }
}
