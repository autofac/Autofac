using System;
using System.Collections.Generic;
using System.Linq;
using AttributedExample.ConsoleApplication.Configuration;
using AttributedExample.ConsoleApplication.StateTypes;
using Stateless;

namespace AttributedExample.ConsoleApplication
{
    /// <summary>
    /// This is an encapsulation of the state machine allowing for configuration option querying
    /// </summary>
    public class StateEngine
    {
        private readonly StateMachine<WorkflowStep, WorkflowTrigger> _stateMachine;

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
        /// <param name="stateConfiguration">document type state configuration filtering component</param>
        public StateEngine(DocumentType documentType, Func<DocumentType, StateEngineConfiguration> stateConfiguration)
        {


            var stateMachine = new StateMachine<WorkflowStep, WorkflowTrigger>(WorkflowStep.New);

            // here, we'll get the state steps particular to our document type and configure the workflow

            var stepCount =
                stateConfiguration(documentType).StateSteps.Select(p => ConfigureStateMachine(stateMachine, p)).Count();

            Console.WriteLine("Creating the workflow at {0} with {1} steps", DateTime.Now.ToLongTimeString(), stepCount);

            _stateMachine = stateMachine;
        }

        private static Lazy<IStateStepConfiguration, IStateStepConfigurationMetadata> ConfigureStateMachine(StateMachine<WorkflowStep, WorkflowTrigger> stateMachine, 
                                           Lazy<IStateStepConfiguration, IStateStepConfigurationMetadata> stepConfiguration)
        {
            var stateConfig = stateMachine.Configure(stepConfiguration.Metadata.WorkflowStep);

            foreach (var permission in stepConfiguration.Value.Permissions)
                stateConfig.Permit(permission.Key, permission.Value);

            stateConfig.OnEntry(
                () => Console.WriteLine("Entering {0} Step at {1}", stepConfiguration.Metadata.WorkflowStep,
                                        DateTime.Now.ToLongTimeString()));

            return stepConfiguration;
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
