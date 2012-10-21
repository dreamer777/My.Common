#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetBrains.Annotations;


#endregion



namespace My.Common.Web
{
    /// <summary>
    ///     Ошибка в параметрах запроса (нет нужного параметра, неправильный формат, нет такого объекта)
    /// </summary>
    [Serializable]
    public class RequestParamsException : MyException
    {
        [StringFormatMethod("message")]
        [TerminatesProgram]
        public new static void Throw(string message, params object[] args)
        {
            throw new PageDeniedException(message, args);
        }


        public RequestParamsException(string message, Exception ex = null)
                : base(message, ex) {}


        [NotNull]
        public static RequestParamsException CreateAsWrongParam([NotNull] string paramName, [NotNull] string obj
                                                                , [NotNull] string val)
        {
            return new RequestParamsException(string.Format("В параметре {0} указан неправильный {2} ({1})"
                                                            , paramName, val, obj));
        }


        [NotNull]
        public static RequestParamsException CreateAsNoRequiredParam([NotNull] string paramName)
        {
            return new RequestParamsException(string.Format("Не указан обязательный параметр {0}", paramName));
        }


        [NotNull]
        public static RequestParamsException CreateAsWrongIntAr([NotNull] string paramName, [NotNull] string paramValue)
        {
            return new RequestParamsException(string.Format("Параметр {0} должен быть перечислением через запятую целых чисел. Указано [{1}]", paramName, paramValue));
        }


        [NotNull]
        public static RequestParamsException CreateAsWrongInt([NotNull] string paramName, [NotNull] string paramValue)
        {
            return new RequestParamsException(string.Format("Параметр {0} должен быть целым числом. Указано [{1}]", paramName, paramValue));
        }


        [NotNull]
        public static RequestParamsException CreateAsWrongBool([NotNull] string paramName, [NotNull] string paramValue)
        {
            return new RequestParamsException(string.Format("Параметр {0} должен быть true или false. Указано [{1}]", paramName, paramValue));
        }


        [NotNull]
        public static RequestParamsException CreateAsWrongDecimal([NotNull] string paramName, [NotNull] string paramValue)
        {
            return new RequestParamsException(string.Format("Параметр {0} должен быть десятичным числом. Указано [{1}]", paramName, paramValue));
        }


        [NotNull]
        public static RequestParamsException CreateAsWrongDateTime([NotNull] string paramName, [NotNull] string paramValue)
        {
            return new RequestParamsException(string.Format("Параметр {0} должен быть датой (формат [dd.MM.yyyy( HH:mm:ss)?]). Указано [{1}]", paramName, paramValue));
        }


        [NotNull]
        public static RequestParamsException CreateAsWrongGuid([NotNull] string paramName, [NotNull] string paramValue)
        {
            return new RequestParamsException(string.Format("Параметр {0} должен быть строковым представлением Guid. Указано [{1}]", paramName, paramValue));
        }
    }
}