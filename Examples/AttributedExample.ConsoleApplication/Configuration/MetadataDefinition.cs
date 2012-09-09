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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class StateStepConfigurationMetadataAttribute : Attribute, IStateStepConfigurationMetadata
    {
        public StateStepConfigurationMetadataAttribute(DocumentType[] documentTypes, WorkflowStep workflowStep)
        {
            DocumentTypes = documentTypes;
            WorkflowStep = workflowStep;
        }


        public WorkflowStep WorkflowStep { get; set; }

        #region IStateStepConfigurationMetadata Members

        public IEnumerable<DocumentType> DocumentTypes
        {
            get; private set;
        }

        #endregion
    }

    public interface IStateStepConfigurationMetadata
    {
        IEnumerable<DocumentType> DocumentTypes { get; }
        WorkflowStep WorkflowStep { get; }
    }
}
