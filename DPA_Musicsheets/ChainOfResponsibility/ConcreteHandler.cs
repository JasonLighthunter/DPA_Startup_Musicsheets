using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Commands;

namespace DPA_Musicsheets.ChainOfResponsibility
{
    class ConcreteHandler : BaseCommandHandler
    {
        public ConcreteHandler(ISJCommand command, HashSet<string> shortcutKeys) : base(command, shortcutKeys)
        {
        }
    }
}
