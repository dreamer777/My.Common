#region usings
using System;
using System.CodeDom;
using System.Reflection;
using System.Web.Compilation;
using System.Web.UI;

using JetBrains.Annotations;


#endregion



namespace My.Common
{
    //[ExpressionPrefix("Config")]
    [UsedImplicitly]
    public class ConfigExpressionBuilder : ExpressionBuilder
    {
        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            string property = (string) parsedData;

            CodePrimitiveExpression prim = new CodePrimitiveExpression(property);
            CodeExpression[] args = new[] {prim};
            CodeTypeReferenceExpression refType = new CodeTypeReferenceExpression(this.GetType());
            return new CodeMethodInvokeExpression(refType, "GetProperty", args);
        }


        /// <summary>
        ///     Must be set in application start event
        /// </summary>
        public static Type ConfigType;


        [UsedImplicitly]
        public static object GetProperty(string propertyName)
        {
            try
            {
                return ConfigType.InvokeMember(propertyName, BindingFlags.DeclaredOnly | BindingFlags.Public |
                                                             BindingFlags.Static | BindingFlags.GetProperty,
                                               null, null, null);
            }
            catch (MissingFieldException)
            {
                throw new Exception(string.Format("Config type [{1}] has no property {0}", propertyName, ConfigType.FullName));
            }
        }


        public override object ParseExpression(string expression, Type propertyType, ExpressionBuilderContext context)
        {
            return expression;
        }


        /// <summary>
        ///     Для некомпилируемой страницы. Нужно только если SupportsEvaluate==true
        /// </summary>
        public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            return base.EvaluateExpression(target, entry, parsedData, context);
        }


        public override bool SupportsEvaluate { get { return base.SupportsEvaluate; } }
    }
}