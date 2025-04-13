using Questao5.Domain.Enumerators;

namespace Questao5.Application.Exceptions
{
    public class BusinessException : ApplicationException
    {
        public BusinessErrorType ErrorType { get; }

        public BusinessException(BusinessErrorType errorType, string message) : base(message)
        {
            ErrorType = errorType;
        }
    }
}
