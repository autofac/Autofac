// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Autofac.Diagnostics
{
    /// <summary>
    /// Base class for creating diagnostic tracers that follow Autofac diagnostic events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Following events from a <see cref="System.Diagnostics.DiagnosticListener"/>
    /// involves subscribing to that listener with an <see cref="IObserver{T}"/>
    /// where the observable is a <see cref="KeyValuePair{K,V}"/> of <see cref="string"/>
    /// event names and <see cref="object"/> event data.
    /// </para>
    /// <para>
    /// This class helps with that by providing strongly typed event handlers
    /// pre-wired to convert the event data into the right object type, allowing you
    /// to focus on the handling logic and not the event/data parsing.
    /// </para>
    /// <para>
    /// While technically you could subscribe to non-Autofac events using this
    /// observer, it's not a general purpose mechanism - it's very much tailored
    /// to Autofac.
    /// </para>
    /// <para>
    /// Should you want to start emitting and subscribing to your own events in the
    /// Autofac pipeline (e.g., a custom middleware that emits your own custom events)
    /// you have some options:
    /// </para>
    /// <para>
    /// First, you could implement your own <see cref="IObserver{T}"/> that listens
    /// for your custom events. Since you can subscribe any number of <see cref="IObserver{T}"/>
    /// to a given <see cref="System.Diagnostics.DiagnosticListener"/>, having one
    /// that derives from this class and a separate one of your own making is acceptable.
    /// </para>
    /// <para>
    /// Second, you could create your own <see cref="IObserver{T}"/> that does not
    /// derive from this class but still listens for Autofac events as well as any
    /// custom events you emit. Any diagnostics observer can listen for these events as
    /// long as it's subscribed to the <see cref="IContainer.DiagnosticSource"/>;
    /// it doesn't have to be one of this specific class type.
    /// </para>
    /// <para>
    /// Finally, if you want to use this as a base but to listen for your custom events,
    /// you can derive from this and override the <see cref="DiagnosticTracerBase.Write(string, object)"/>
    /// method. Handle your custom events, or if it's not one of your custom events, call
    /// the base to handle the Autofac events.
    /// </para>
    /// </remarks>
    /// <seealso cref="DefaultDiagnosticTracer"/>
    public abstract class DiagnosticTracerBase : IObserver<KeyValuePair<string, object>>
    {
        /// <summary>
        /// The list of event names to which this observer is subscribed.
        /// </summary>
        private readonly List<string> _subscriptions = new List<string>();

        /// <summary>
        /// Subscribes the observer to a particular named diagnostic event.
        /// </summary>
        /// <param name="diagnosticName">
        /// The name of the event to which the observer should subscribe. Diagnostic
        /// names are case-sensitive.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default the observer is not subscribed to any events. You must
        /// opt-in to any events you wish to handle. You may use
        /// <see cref="EnableAll"/> to subscribe to all Autofac events at once.
        /// </para>
        /// </remarks>
        /// <seealso cref="EnableAll"/>
        /// <seealso cref="Disable"/>
        public virtual void Enable(string diagnosticName)
        {
            EnableBase(diagnosticName);
        }

        /// <summary>
        /// Subscribes the observer to a particular named diagnostic event.
        /// </summary>
        /// <param name="diagnosticName">
        /// The name of the event to which the observer should subscribe. Diagnostic
        /// names are case-sensitive.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes may override the public <see cref="Enable"/> method to
        /// handle consumer calls to enabling events; constructors needing to call
        /// non-virtual methods may directly call this to avoid executing against
        /// partially constructed derived classes.
        /// </para>
        /// </remarks>
        /// <seealso cref="DisableBase"/>
        protected void EnableBase(string diagnosticName)
        {
            if (diagnosticName == null)
            {
                throw new ArgumentNullException(nameof(diagnosticName));
            }

            if (!_subscriptions.Contains(diagnosticName))
            {
                _subscriptions.Add(diagnosticName);
            }
        }

        /// <summary>
        /// Subscribes the observer to all Autofac events.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default the observer is not subscribed to any events. You must
        /// opt-in to any events you wish to handle. This method helps you
        /// to quickly subscribe to all the Autofac events at once. You may use
        /// <see cref="Enable"/> to subscribe to individual events.
        /// </para>
        /// </remarks>
        /// <seealso cref="Enable"/>
        /// <seealso cref="Disable"/>
        public void EnableAll()
        {
            // EnableAll is intentionally not virtual so it can be called
            // from the OperationDiagnosticTracerBase constructor.
            //
            // Subscriptions ordered intentionally from most to least common events.
            EnableBase(DiagnosticEventKeys.MiddlewareStart);
            EnableBase(DiagnosticEventKeys.MiddlewareSuccess);
            EnableBase(DiagnosticEventKeys.RequestStart);
            EnableBase(DiagnosticEventKeys.RequestSuccess);
            EnableBase(DiagnosticEventKeys.OperationStart);
            EnableBase(DiagnosticEventKeys.OperationSuccess);
            EnableBase(DiagnosticEventKeys.MiddlewareFailure);
            EnableBase(DiagnosticEventKeys.RequestFailure);
            EnableBase(DiagnosticEventKeys.OperationFailure);
        }

        /// <summary>
        /// Unsubscribes the observer from a particular named diagnostic event.
        /// </summary>
        /// <param name="diagnosticName">
        /// The name of the event to which the observer should unsubscribe. Diagnostic
        /// names are case-sensitive.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default the observer is not subscribed to any events. You must
        /// opt-in to any events you wish to handle. You may use
        /// <see cref="EnableAll"/> to subscribe to all Autofac events at once,
        /// or <see cref="Enable"/> to subscribe to individual events.
        /// </para>
        /// </remarks>
        /// <seealso cref="EnableAll"/>
        /// <seealso cref="Enable"/>
        public virtual void Disable(string diagnosticName)
        {
            DisableBase(diagnosticName);
        }

        /// <summary>
        /// Unsubscribes the observer from a particular named diagnostic event.
        /// </summary>
        /// <param name="diagnosticName">
        /// The name of the event to which the observer should unsubscribe. Diagnostic
        /// names are case-sensitive.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes may override the public <see cref="Disable"/> method to
        /// handle consumer calls to disabling events; constructors needing to call
        /// non-virtual methods may directly call this to avoid executing against
        /// partially constructed derived classes.
        /// </para>
        /// </remarks>
        /// <seealso cref="EnableBase"/>
        protected void DisableBase(string diagnosticName)
        {
            if (diagnosticName == null)
            {
                throw new ArgumentNullException(nameof(diagnosticName));
            }

            _subscriptions.Remove(diagnosticName);
        }

        /// <summary>
        /// Determines if this observer is enabled for listening to a specific
        /// named event.
        /// </summary>
        /// <param name="diagnosticName">
        /// The name of the event to check. Diagnostic names are case-sensitive.
        /// </param>
        public bool IsEnabled(string diagnosticName)
        {
            if (_subscriptions.Count == 0)
            {
                return false;
            }

            return _subscriptions.Contains(diagnosticName);
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        void IObserver<KeyValuePair<string, object>>.OnCompleted()
        {
            // If there was something to dispose or clean up, here's where it would
            // happen, but we don't have anything like that.
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">
        /// An object that provides additional information about the error.
        /// </param>
        void IObserver<KeyValuePair<string, object>>.OnError(Exception error)
        {
            // The internal diagnostic source isn't really going to experience an
            // error condition (which is not the same as _reporting errors_) so
            // there's nothing to do here.
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">
        /// The current notification information.
        /// </param>
        void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> value)
        {
            // This is what gets called when a new diagnostic event occurs.
            Write(value.Key, value.Value);
        }

        /// <summary>
        /// Handles the event raised when middleware encounters an error.
        /// </summary>
        /// <param name="data">
        /// Diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method and perform actions based
        /// on the event. By default, the base class does nothing.
        /// </para>
        /// </remarks>
        protected virtual void OnMiddlewareFailure(MiddlewareDiagnosticData data)
        {
        }

        /// <summary>
        /// Handles the event raised when middleware starts.
        /// </summary>
        /// <param name="data">
        /// Diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method and perform actions based
        /// on the event. By default, the base class does nothing.
        /// </para>
        /// </remarks>
        protected virtual void OnMiddlewareStart(MiddlewareDiagnosticData data)
        {
        }

        /// <summary>
        /// Handles the event raised when middleware exits successfully.
        /// </summary>
        /// <param name="data">
        /// Diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method and perform actions based
        /// on the event. By default, the base class does nothing.
        /// </para>
        /// </remarks>
        protected virtual void OnMiddlewareSuccess(MiddlewareDiagnosticData data)
        {
        }

        /// <summary>
        /// Handles the event raised when a resolve operation encounters an error.
        /// </summary>
        /// <param name="data">
        /// Diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method and perform actions based
        /// on the event. By default, the base class does nothing.
        /// </para>
        /// </remarks>
        protected virtual void OnOperationFailure(OperationFailureDiagnosticData data)
        {
        }

        /// <summary>
        /// Handles the event raised when a resolve operation starts.
        /// </summary>
        /// <param name="data">
        /// Diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method and perform actions based
        /// on the event. By default, the base class does nothing.
        /// </para>
        /// </remarks>
        protected virtual void OnOperationStart(OperationStartDiagnosticData data)
        {
        }

        /// <summary>
        /// Handles the event raised when a resolve operation completes successfully.
        /// </summary>
        /// <param name="data">
        /// Diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method and perform actions based
        /// on the event. By default, the base class does nothing.
        /// </para>
        /// </remarks>
        protected virtual void OnOperationSuccess(OperationSuccessDiagnosticData data)
        {
        }

        /// <summary>
        /// Handles the event raised when a resolve request encounters an error.
        /// </summary>
        /// <param name="data">
        /// Diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method and perform actions based
        /// on the event. By default, the base class does nothing.
        /// </para>
        /// </remarks>
        protected virtual void OnRequestFailure(RequestFailureDiagnosticData data)
        {
        }

        /// <summary>
        /// Handles the event raised when a resolve request starts.
        /// </summary>
        /// <param name="data">
        /// Diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method and perform actions based
        /// on the event. By default, the base class does nothing.
        /// </para>
        /// </remarks>
        protected virtual void OnRequestStart(RequestDiagnosticData data)
        {
        }

        /// <summary>
        /// Handles the event raised when a resolve request completes successfully.
        /// </summary>
        /// <param name="data">
        /// Diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// Derived classes can override this method and perform actions based
        /// on the event. By default, the base class does nothing.
        /// </para>
        /// </remarks>
        protected virtual void OnRequestSuccess(RequestDiagnosticData data)
        {
        }

        /// <summary>
        /// Handles inbound events and converts the diagnostic data to a
        /// strongly-typed object that can be handled by the other methods
        /// in this observer.
        /// </summary>
        /// <param name="diagnosticName">
        /// The name of the event that was raised. Diagnostic names are case-sensitive.
        /// </param>
        /// <param name="data">
        /// The diagnostic data associated with the event.
        /// </param>
        /// <remarks>
        /// <para>
        /// If you are interested in handling custom events, check out the top-level
        /// <see cref="DiagnosticTracerBase"/> documentation which discusses some
        /// options.
        /// </para>
        /// </remarks>
        protected virtual void Write(string diagnosticName, object data)
        {
            if (data == null || !IsEnabled(diagnosticName))
            {
                return;
            }

            switch (diagnosticName)
            {
                case DiagnosticEventKeys.MiddlewareStart:
                    OnMiddlewareStart((MiddlewareDiagnosticData)data);
                    break;
                case DiagnosticEventKeys.MiddlewareFailure:
                    OnMiddlewareFailure((MiddlewareDiagnosticData)data);
                    break;
                case DiagnosticEventKeys.MiddlewareSuccess:
                    OnMiddlewareSuccess((MiddlewareDiagnosticData)data);
                    break;
                case DiagnosticEventKeys.OperationFailure:
                    OnOperationFailure((OperationFailureDiagnosticData)data);
                    break;
                case DiagnosticEventKeys.OperationStart:
                    OnOperationStart((OperationStartDiagnosticData)data);
                    break;
                case DiagnosticEventKeys.OperationSuccess:
                    OnOperationSuccess((OperationSuccessDiagnosticData)data);
                    break;
                case DiagnosticEventKeys.RequestFailure:
                    OnRequestFailure((RequestFailureDiagnosticData)data);
                    break;
                case DiagnosticEventKeys.RequestStart:
                    OnRequestStart((RequestDiagnosticData)data);
                    break;
                case DiagnosticEventKeys.RequestSuccess:
                    OnRequestSuccess((RequestDiagnosticData)data);
                    break;
                default:
                    break;
            }
        }
    }
}
