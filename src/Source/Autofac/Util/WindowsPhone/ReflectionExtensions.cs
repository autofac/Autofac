using System;
using System.Linq.Expressions;

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
            return Expressions.LambdaCompiler.Compile(expression);
        }
    }
}
