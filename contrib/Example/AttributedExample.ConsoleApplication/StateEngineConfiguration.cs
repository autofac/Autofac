using System;
using System.Collections.Generic;
using System.Linq;
using AttributedExample.ConsoleApplication.Configuration;
using AttributedExample.ConsoleApplication.StateTypes;

namespace AttributedExample.ConsoleApplication
{
    public class StateEngineConfiguration
    {
        private readonly Lazy<IEnumerable<Lazy<IStateStepConfiguration, IStateStepConfigurationMetadata>>> _stateSteps;

        public IEnumerable<Lazy<IStateStepConfiguration, IStateStepConfigurationMetadata>> StateSteps { get { return _stateSteps.Value; } }

        public StateEngineConfiguration(IEnumerable<Lazy<IStateStepConfiguration, IStateStepConfigurationMetadata>> stateStepConfigurations, DocumentType documentType)
        {
            // here, we'll filter according to the rules we are looking for.  the incorportation of rules is another reason
            // consider the filtering process to be injected and atomically testable
            //
            // and we'll be as lazy as possible
            _stateSteps = new Lazy<IEnumerable<Lazy<IStateStepConfiguration, IStateStepConfigurationMetadata>>>( () => from p in stateStepConfigurations where p.Metadata.DocumentTypes.Contains(documentType) select p );
        }
    }
}
