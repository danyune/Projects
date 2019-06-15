using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team7GUI.Commands
{
    class Select : ICommand
    {
        public string Query { get; set; }

        public Select(string query)
        {
            Query = query;
        }
        public void Execute()
        {
            ExecuteQuery.RunSelectQuery(Query);
        }
    }
}
