using System.Collections.Generic;
using AttributedExample.ConsoleApplication.StateTypes;

namespace AttributedExample.ConsoleApplication.Configuration
{
    public interface IStateStepConfiguration
    {
        IEnumerable<KeyValuePair<WorkflowTrigger, WorkflowStep>> Permissions { get; }
    }
}
