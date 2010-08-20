using System.Collections.Generic;
using AttributedExample.ConsoleApplication.Configuration;
using AttributedExample.ConsoleApplication.StateTypes;
using AutofacContrib.Attributed;

namespace AttributedExample.ConsoleApplication
{
    //
    // this demonstration configures a state machine to move a theoretical document through
    // various stages in the system.  The idea is that the user must perform an action dicated
    // by the state machine at each step in order to progress through the machine.
    //
    // the idea with the system is that there are multiple document types, each with a differing
    // set of workflow steps.  metadata is used to decorate the implementation classes with respect
    // to their role in a document type workflow. in addition, the attributes are used in autofac
    // registration to simplify the process and return enumerations that can be iterated upon
    // referencing Lazy<T,Metadata> or Meta<T, Metadata>
    //
    // in general terms, there are three document types each with a defined workflow.
    // For demonstration, amendments and orders follow each other with two attributes for each
    // step while cancellations have a separete workflow
    //
    // Order & Amendment : New => Approval => Email => Done 
    // while
    // Cancel : New => Email => Done
    //
    // a rejection at any step forces the process to the 'Done' step.
    //
    // the done step is shared by all and therefore has three attributes denoting its
    // use in the various document type workflows

    public class StateStepMetadataModule : MetadataModule<IStateStepConfiguration, IStateStepConfigurationMetadata>
    {

        public override void Register(IMetadataRegistrar<IStateStepConfiguration, IStateStepConfigurationMetadata> registrar)
        {
            // here, we are declaring which types are assigned according to document type and step
            registrar.RegisterType<EmailStepCapability>(new StateStepConfigurationMetadata(new [] {DocumentType.Amendment, DocumentType.Order},
                                                                                           WorkflowStep.EmailDetails));

            registrar.RegisterType<ApproveStepCapability>(new StateStepConfigurationMetadata(new [] {DocumentType.Amendment, DocumentType.Order},
                                                                                             WorkflowStep.Approve));

            registrar.RegisterType<NewOrderStepCapability>(new StateStepConfigurationMetadata(new [] {DocumentType.Amendment, DocumentType.Order},
                                                                                              WorkflowStep.New));

            registrar.RegisterType<NewCancellationStepConfiguration>(new StateStepConfigurationMetadata(new [] {DocumentType.Cancellation}, WorkflowStep.New));

            registrar.RegisterType<EmailDetailsCancellationStepConfiguration>(new StateStepConfigurationMetadata(new [] {DocumentType.Cancellation}, WorkflowStep.EmailDetails));
            
            registrar.RegisterType<GenericDoneStepConfiguration>(new StateStepConfigurationMetadata(new [] { DocumentType.Order, DocumentType.Amendment, DocumentType.Cancellation},
                                                                                                    WorkflowStep.Done));

        }
    }

    [StateStepConfigurationMetadata(new [] {DocumentType.Amendment, DocumentType.Order}, WorkflowStep.EmailDetails)]
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


    [StateStepConfigurationMetadata(new[] { DocumentType.Amendment, DocumentType.Order }, WorkflowStep.Approve)]
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


    [StateStepConfigurationMetadata(new[] { DocumentType.Amendment, DocumentType.Order }, WorkflowStep.New)]
    public class NewOrderStepCapability : IStateStepConfiguration
    {

        #region IStateStepConfiguration Members

        public IEnumerable<KeyValuePair<WorkflowTrigger, WorkflowStep>> Permissions
        {
            get
            {
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Approve, WorkflowStep.Approve);
                yield return
                    new KeyValuePair<WorkflowTrigger, WorkflowStep>(WorkflowTrigger.Reject, WorkflowStep.Done);
            }
        }

        #endregion
    }

    [StateStepConfigurationMetadata(new[] { DocumentType.Cancellation }, WorkflowStep.New)]
    public class NewCancellationStepConfiguration : IStateStepConfiguration
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

    [StateStepConfigurationMetadata(new[] { DocumentType.Cancellation }, WorkflowStep.EmailDetails)]
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

    [StateStepConfigurationMetadata(new[] { DocumentType.Amendment, DocumentType.Order, DocumentType.Cancellation }, WorkflowStep.Done)]
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
