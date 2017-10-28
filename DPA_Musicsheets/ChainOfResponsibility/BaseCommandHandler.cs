using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Commands;

namespace DPA_Musicsheets.ChainOfResponsibility
{
    public class BaseCommandHandler : ISJHandler
    {
        protected readonly ISJCommand _command;
        protected readonly HashSet<string> _shortcutKeys;
        protected ISJHandler _next;

        public BaseCommandHandler(ISJCommand command, HashSet<string> shortcutKeys)
        {
            _command = command;
            _shortcutKeys = shortcutKeys;
        }

        public void SetNext(ISJHandler handler)
        {
            _next = handler;
        }

        public bool Handle(HashSet<string> pressedKeys)
        {
            if (pressedKeys.SetEquals(_shortcutKeys))
            {
                _command.Execute();
                return true;
            }
            else
            {
                return _next?.Handle(pressedKeys) ?? false;
            }
        }
    }
}
