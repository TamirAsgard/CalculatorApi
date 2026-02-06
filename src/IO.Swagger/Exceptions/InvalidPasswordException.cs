using System;

namespace IO.Swagger.Exceptions;

/// <summary>
/// Exception thrown when an invalid password is provided during authentication
/// </summary>
public class InvalidPasswordException : Exception
{
    /// <summary>
    /// Initializes a new instance with a default message
    /// </summary>
    public InvalidPasswordException() 
        : base("The provided password is incorrect.")
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message
    /// </summary>
    /// <param name="message">The error message</param>
    public InvalidPasswordException(string message) 
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public InvalidPasswordException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
