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

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
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


    [StateStepConfigurationMetadata(DocumentType.Amendment, WorkflowStep.EmailDetails)]
    [StateStepConfigurationMetadata(DocumentType.Order, WorkflowStep.EmailDetails)]
    public class EmailStepCapability : IStateStepConfiguration
    {
        #region IStateStepConfiguration Members

        public IEnumerable<KeyValuePair<WorkflowTrigger, WorkflowStep>> Permissions
        {
            get
            {
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Approve, WorkflowStep.Done);
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Reject, WorkflowStep.Done);
            }
        }

        #endregion
    } 

    

    [StateStepConfigurationMetadata(DocumentType.Amendment, WorkflowStep.Approve)]
    [StateStepConfigurationMetadata(DocumentType.Order, WorkflowStep.Approve)]
    public class ApproveStepCapability : IStateStepConfiguration
    {

        #region IStateStepConfiguration Members

        public IEnumerable<KeyValuePair<WorkflowTrigger, WorkflowStep>> Permissions
        {
            get
            {
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Approve, WorkflowStep.EmailDetails);
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Reject, WorkflowStep.Done);
            }
        }

        #endregion
    } 


    [StateStepConfigurationMetadata(DocumentType.Amendment, WorkflowStep.New)]
    [StateStepConfigurationMetadata(DocumentType.Order, WorkflowStep.New)]
    public class NewOrderStepCapability : IStateStepConfiguration
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
 
    [StateStepConfigurationMetadata(DocumentType.Cancellation, WorkflowStep.New)]
    public class NewCancellationStepConfiguration : IStateStepConfiguration
    {

        #region IStateStepConfiguration Members

        public IEnumerable<KeyValuePair<WorkflowTrigger, WorkflowStep>> Permissions
        {
            get {
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Approve, WorkflowStep.EmailDetails);
                yield return 
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Reject, WorkflowStep.Done);
            }
        }

        #endregion
    }

    [StateStepConfigurationMetadata(DocumentType.Cancellation, WorkflowStep.EmailDetails)]
    public class EmailDetailsCancellationStepConfiguration : IStateStepConfiguration
    {

        #region IStateStepConfiguration Members

        public IEnumerable<KeyValuePair<WorkflowTrigger, WorkflowStep>> Permissions
        {
            get
            {
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Approve, WorkflowStep.Done);
                yield return 
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Reject, WorkflowStep.Done);
            }
        }

        #endregion
    }


    [StateStepConfigurationMetadata(DocumentType.Order, WorkflowStep.Done)]
    [StateStepConfigurationMetadata(DocumentType.Amendment, WorkflowStep.Done)]
    [StateStepConfigurationMetadata(DocumentType.Cancellation, WorkflowStep.Done)]
    public class GenericDoneStepConfiguration : IStateStepConfiguration
    {

        #region IStateStepConfiguration Members

        public IEnumerable<KeyValuePair<WorkflowTrigger, WorkflowStep>> Permissions
        {
            get
            {
                yield break;
            }
        }

        #endregion
    }



}
