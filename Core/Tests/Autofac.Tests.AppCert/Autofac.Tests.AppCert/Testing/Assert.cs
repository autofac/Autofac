using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Tests.AppCert.Testing
{
    public static class Assert
    {
        public static void AreEqual(object expected, object actual, string message = null)
        {
            if (!ObjectsEqual(expected, actual))
            {
                var exMessage = message == null ? null : message + Environment.NewLine;
                throw new AssertionException(String.Format("Expected: {0}{2}Actual: {1}{2}{3}", expected, actual, Environment.NewLine, exMessage));
            }
        }

        public static void IsFalse(bool actual, string message = null)
        {
            if (actual)
            {
                var exMessage = message == null ? null : Environment.NewLine + message + Environment.NewLine;
                throw new AssertionException(String.Format("Value was not FALSE.{0}", exMessage));
            }
        }

        public static void IsTrue(bool actual, string message = null)
        {
            if (!actual)
            {
                var exMessage = message == null ? null : Environment.NewLine + message + Environment.NewLine;
                throw new AssertionException(String.Format("Value was not TRUE.{0}", exMessage));
            }
        }

        public static void Throws<T>(Action testExpression, string message = null) where T : Exception
        {
            var exMessage = message == null ? null : message + Environment.NewLine;
            try
            {
                testExpression();
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(T))
                {
                    throw new AssertionException(String.Format("{0}Expected exception type: {2}{1}Actual exception type: {3}{1}", exMessage, Environment.NewLine, typeof(T), ex.GetType(), ex));
                }
                return;
            }
            throw new AssertionException(String.Format("{0}Expected exception type {1} was not thrown.", exMessage, typeof(T)));
        }

        private static bool ObjectsEqual(object x, object y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            if (Object.ReferenceEquals(x, y))
            {
                return true;
            }
            return x.Equals(y);
        }
    }
}
