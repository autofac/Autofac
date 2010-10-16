using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Autofac.Util.WindowsPhone
{
    ///<summary>
    ///</summary>
    public static class ExpressionExtensions
    {
        ///<summary>
        ///</summary>
        ///<param name="expression"></param>
        ///<typeparam name="TDelegate"></typeparam>
        ///<returns></returns>
        public static TDelegate Compile<TDelegate>(this Expression<TDelegate> expression)
        {
            var compiledDelegate = CreateDelegateFromExpression(expression, typeof(TDelegate));
            return (TDelegate)compiledDelegate;
        }

        static object CreateDelegateFromExpression(LambdaExpression expression, Type delegateType)
        {
            throw new NotImplementedException(string.Format("Could not create delegate on WP7 from expression {0}", expression));
        }
    }
}
