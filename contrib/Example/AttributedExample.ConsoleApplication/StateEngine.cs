using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using AttributedExample.ConsoleApplication.StateTypes;
using Stateless;

namespace AttributedExample.ConsoleApplication
{
    public class StateEngine
    {
        public StateEngine(DocumentType documentType)
        {
            var stateMachine = new StateMachine<WorkflowStep, WorkflowTrigger>(WorkflowStep.New);

            
        }
    }

    [MetadataAttribute]
    public class StateStepConfigurationMetadataAttribute : Attribute
    {
        public StateStepConfigurationMetadataAttribute(DocumentType documentType, WorkflowStep workflowStep)
        {
            DocumentType = documentType;
            WorkflowStep = workflowStep;
        }

        public StateStepConfigurationMetadataAttribute( WorkflowStep workflowStep)
        {
            DocumentType = null;
            WorkflowStep = workflowStep;
        }
        
        public DocumentType? DocumentType { get; set; }
        public WorkflowStep WorkflowStep { get; set; }
    }

    public interface IStateStepConfigurationMetadata
    {
        DocumentType? DocumentType { get; }
        WorkflowStep WorkflowStep { get; }
    }



    public interface IStateStepConfiguration
    {
        IEnumerable<KeyValuePair<WorkflowTrigger, WorkflowStep>> Permissions{ get; }
    }

    public class StateStepCapability : IStateStepConfiguration
    {

        #region IStateStepConfiguration Members

        public IEnumerable<KeyValuePair<WorkflowTrigger, WorkflowStep>> Permissions
        {
            get {
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Approve, WorkflowStep.Approve);
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Reject, WorkflowStep.Done);
            }
        }

        #endregion
    }  
}
