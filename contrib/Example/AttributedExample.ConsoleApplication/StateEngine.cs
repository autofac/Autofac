using System;
using System.Collections.Generic;
using System.Linq;
using AttributedExample.ConsoleApplication.StateTypes;
using Stateless;

namespace AttributedExample.ConsoleApplication
{
    /// <summary>
    /// This is an encapsulation of the state machine allowing for configuration option querying
    /// </summary>
    public class StateEngine
    {
        private StateMachine<WorkflowStep, WorkflowTrigger> _stateMachine;

        /// <summary>
        /// ctor that configures the state machine according to document type.
        /// in this scenario, multiple document types exist where a different flow through the state machine for each. the configuration
        /// of the flow is handled through metadata.
        /// 
        /// injection of the enumerated state steps is done here. and while useful for demonstration purposes, it may also be useful
        /// to inject this list from another source.  this is especially useful when ambient properties (i.e. user roles) play a role
        /// in determining the steps.
        /// </summary>
        /// <param name="documentType">document type to use for the state machine</param>
        /// <param name="stateSteps">enumeration of all state steps along with their respective metadata</param>
        public StateEngine(DocumentType documentType, IEnumerable<Lazy<IStateStepConfiguration, IStateStepConfigurationMetadata>> stateSteps)
        {
            _stateMachine = new StateMachine<WorkflowStep, WorkflowTrigger>(WorkflowStep.New);

            Console.WriteLine("Entering new step at {0}", DateTime.Now.ToLongTimeString());

            // here, we'll filter according to the rules we are looking for.  the incorportation of rules is another reason
            // consider the filtering process to be injected and atomically testable
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

        /// <summary>
        /// provide a lsit of actions permitted for the given state
        /// </summary>
        public IEnumerable<WorkflowTrigger> Actions
        {
            get
            {
                return _stateMachine.PermittedTriggers;
            }
        }

        /// <summary>
        /// fires the requested trigger
        /// </summary>
        /// <param name="workflowTrigger">trigger to run</param>
        public void Fire(WorkflowTrigger workflowTrigger)
        {
            _stateMachine.Fire(workflowTrigger);
        }
    }
}
