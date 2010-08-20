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

    public class StateStepModule : MetadataModule<IStateStepConfiguration, IStateStepConfigurationMetadata>
    {

        public override void Register(IMetadataRegistrar<IStateStepConfiguration, IStateStepConfigurationMetadata> registrar)
        {
            registrar.RegisterType<EmailStepCapability>(new StateStepConfigurationMetadata(DocumentType.Amendment,
                                                                                           WorkflowStep.EmailDetails));

            registrar.RegisterType<EmailStepCapability>(new StateStepConfigurationMetadata(DocumentType.Order,
                                                                               WorkflowStep.EmailDetails));

            registrar.RegisterType<ApproveStepCapability>(new StateStepConfigurationMetadata(DocumentType.Amendment,
                                                                                             WorkflowStep.Approve));

            registrar.RegisterType<ApproveStepCapability>(new StateStepConfigurationMetadata(DocumentType.Order,
                                                                                             WorkflowStep.Approve));

            registrar.RegisterType<NewOrderStepCapability>(new StateStepConfigurationMetadata(DocumentType.Amendment,
                                                                                              WorkflowStep.New));

            registrar.RegisterType<NewOrderStepCapability>(new StateStepConfigurationMetadata(DocumentType.Order,
                                                                                              WorkflowStep.New));

            registrar.RegisterType<NewCancellationStepConfiguration>(new StateStepConfigurationMetadata(DocumentType.Cancellation, WorkflowStep.New));

            registrar.RegisterType<EmailDetailsCancellationStepConfiguration>(new StateStepConfigurationMetadata(DocumentType.Cancellation, WorkflowStep.EmailDetails));


            registrar.RegisterType<GenericDoneStepConfiguration>(new StateStepConfigurationMetadata(DocumentType.Order, WorkflowStep.Done));
            registrar.RegisterType<GenericDoneStepConfiguration>(new StateStepConfigurationMetadata(DocumentType.Amendment, WorkflowStep.Done));
            registrar.RegisterType<GenericDoneStepConfiguration>(new StateStepConfigurationMetadata(DocumentType.Cancellation, WorkflowStep.Done));

        }
    }

    //[StateStepConfigurationMetadata(DocumentType.Amendment, WorkflowStep.EmailDetails)]
    //[StateStepConfigurationMetadata(DocumentType.Order, WorkflowStep.EmailDetails)]
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



    //[StateStepConfigurationMetadata(DocumentType.Amendment, WorkflowStep.Approve)]
    //[StateStepConfigurationMetadata(DocumentType.Order, WorkflowStep.Approve)]
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


    //[StateStepConfigurationMetadata(DocumentType.Amendment, WorkflowStep.New)]
    //[StateStepConfigurationMetadata(DocumentType.Order, WorkflowStep.New)]
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

    //[StateStepConfigurationMetadata(DocumentType.Cancellation, WorkflowStep.New)]
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

    //[StateStepConfigurationMetadata(DocumentType.Cancellation, WorkflowStep.EmailDetails)]
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


    //[StateStepConfigurationMetadata(DocumentType.Order, WorkflowStep.Done)]
    //[StateStepConfigurationMetadata(DocumentType.Amendment, WorkflowStep.Done)]
    //[StateStepConfigurationMetadata(DocumentType.Cancellation, WorkflowStep.Done)]
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
