using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AttributedExample.ConsoleApplication.StateTypes;
using Stateless;

namespace AttributedExample.ConsoleApplication
{
    public class StateEngine
    {
        private StateMachine<WorkflowStep, WorkflowTrigger> _stateMachine;

        public StateEngine(DocumentType documentType, IEnumerable<Lazy<IStateStepConfiguration, IStateStepConfigurationMetadata>> stateSteps)
        {
            _stateMachine = new StateMachine<WorkflowStep, WorkflowTrigger>(WorkflowStep.New);

            Console.WriteLine("Entering new step at {0}", DateTime.Now.ToLongTimeString());

            foreach (var item in (from p in stateSteps where p.Metadata.DocumentType == documentType select p))
            {
                var localItem = item;
                var stateConfig = _stateMachine.Configure(localItem.Metadata.WorkflowStep);

                foreach(var permission in localItem.Value.Permissions)
                    stateConfig.Permit(permission.Key, permission.Value);

                stateConfig.OnEntry(
                    () =>
                    Console.WriteLine("Entering {0} Step at {1}", localItem.Metadata.WorkflowStep,
                                      DateTime.Now.ToLongTimeString()));
            }
        }

        public IEnumerable<WorkflowTrigger> Actions
        {
            get
            {
                return _stateMachine.PermittedTriggers;
            }
        }

        public void Fire(WorkflowTrigger workflowTrigger)
        {
            _stateMachine.Fire(workflowTrigger);
        }
    }
}
