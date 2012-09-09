using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    public class UndoRedoCommand : ICommand<UndoCommandData>, ICommand<RedoCommandData>
    {
        public void Execute(UndoCommandData data)
        {
        }

        public void Execute(RedoCommandData data)
        {
        }
    }
}
