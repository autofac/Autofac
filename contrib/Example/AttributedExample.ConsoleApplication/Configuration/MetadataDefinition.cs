using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using AttributedExample.ConsoleApplication.StateTypes;

namespace AttributedExample.ConsoleApplication.Configuration
{
    public class StateStepConfigurationMetadata : IStateStepConfigurationMetadata
    {
        public StateStepConfigurationMetadata(IEnumerable<DocumentType> documentTypes, WorkflowStep workflowStep)
        {
            DocumentTypes = documentTypes;
            WorkflowStep = workflowStep;
        }
        #region IStateStepConfigurationMetadata Members

        public IEnumerable<DocumentType> DocumentTypes
        {
            get; private set;
        }

        public WorkflowStep WorkflowStep
        {
            get; private set;
        }

        #endregion
    }


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
        IEnumerable<DocumentType> DocumentTypes { get; }
        WorkflowStep WorkflowStep { get; }
    }
}
