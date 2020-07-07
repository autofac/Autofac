// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace Autofac.Core.Diagnostics
{
    public abstract class DiagnosticTracerBase : IObserver<KeyValuePair<string, object>>
    {
        private readonly List<string> _subscriptions = new List<string>();

        public virtual void Enable(string diagnosticName)
        {
            if (!_subscriptions.Contains(diagnosticName))
            {
                _subscriptions.Add(diagnosticName);
            }
        }

        public void EnableAll()
        {
            _subscriptions.Add(DiagnosticEventKeys.MiddlewareEntry);
            _subscriptions.Add(DiagnosticEventKeys.MiddlewareFailure);
            _subscriptions.Add(DiagnosticEventKeys.MiddlewareSuccess);
            _subscriptions.Add(DiagnosticEventKeys.OperationFailure);
            _subscriptions.Add(DiagnosticEventKeys.OperationStart);
            _subscriptions.Add(DiagnosticEventKeys.OperationSuccess);
            _subscriptions.Add(DiagnosticEventKeys.RequestFailure);
            _subscriptions.Add(DiagnosticEventKeys.RequestStart);
            _subscriptions.Add(DiagnosticEventKeys.RequestSuccess);
        }

        public virtual void Disable(string diagnosticName)
        {
            _subscriptions.Remove(diagnosticName);
        }

        public virtual bool IsEnabled(string diagnosticName)
        {
            if (_subscriptions.Count == 0)
            {
                return false;
            }

            return _subscriptions.Contains(diagnosticName);
        }

        void IObserver<KeyValuePair<string, object>>.OnCompleted()
        {
            // Do nothing
        }

        void IObserver<KeyValuePair<string, object>>.OnError(Exception error)
        {
            // Do nothing
        }

        void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> value)
        {
            Write(value.Key, value.Value);
        }

        public virtual void OnMiddlewareEntry(MiddlewareDiagnosticData data)
        {
        }

        public virtual void OnMiddlewareFailure(MiddlewareDiagnosticData data)
        {
        }

        public virtual void OnMiddlewareSuccess(MiddlewareDiagnosticData data)
        {
        }

        public virtual void OnOperationFailure(OperationFailureDiagnosticData data)
        {
        }

        public virtual void OnOperationStart(OperationStartDiagnosticData data)
        {
        }

        public virtual void OnOperationSuccess(OperationSuccessDiagnosticData data)
        {
        }

        public virtual void OnRequestFailure(RequestFailureDiagnosticData data)
        {
        }

        public virtual void OnRequestStart(RequestDiagnosticData data)
        {
        }

        public virtual void OnRequestSuccess(RequestDiagnosticData data)
        {
        }

        public virtual void Write(string diagnosticName, object parameters)
        {
            if (parameters == null || !IsEnabled(diagnosticName))
            {
                return;
            }

            switch (diagnosticName)
            {
                case DiagnosticEventKeys.MiddlewareEntry:
                    OnMiddlewareEntry((MiddlewareDiagnosticData)parameters);
                    break;
                case DiagnosticEventKeys.MiddlewareFailure:
                    OnMiddlewareFailure((MiddlewareDiagnosticData)parameters);
                    break;
                case DiagnosticEventKeys.MiddlewareSuccess:
                    OnMiddlewareSuccess((MiddlewareDiagnosticData)parameters);
                    break;
                case DiagnosticEventKeys.OperationFailure:
                    OnOperationFailure((OperationFailureDiagnosticData)parameters);
                    break;
                case DiagnosticEventKeys.OperationStart:
                    OnOperationStart((OperationStartDiagnosticData)parameters);
                    break;
                case DiagnosticEventKeys.OperationSuccess:
                    OnOperationSuccess((OperationSuccessDiagnosticData)parameters);
                    break;
                case DiagnosticEventKeys.RequestFailure:
                    OnRequestFailure((RequestFailureDiagnosticData)parameters);
                    break;
                case DiagnosticEventKeys.RequestStart:
                    OnRequestStart((RequestDiagnosticData)parameters);
                    break;
                case DiagnosticEventKeys.RequestSuccess:
                    OnRequestSuccess((RequestDiagnosticData)parameters);
                    break;
                default:
                    break;
            }
        }
    }
}
