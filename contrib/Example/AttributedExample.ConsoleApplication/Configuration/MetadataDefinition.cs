using System;
using System.ComponentModel.Composition;
using AttributedExample.ConsoleApplication.StateTypes;

namespace AttributedExample.ConsoleApplication.Configuration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class StateStepConfigurationMetadataAttribute : Attribute
    {
        public StateStepConfigurationMetadataAttribute(DocumentType documentType, WorkflowStep workflowStep)
        {
            DocumentType = documentType;
            WorkflowStep = workflowStep;
        }

        public StateStepConfigurationMetadataAttribute(WorkflowStep workflowStep)
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
}
