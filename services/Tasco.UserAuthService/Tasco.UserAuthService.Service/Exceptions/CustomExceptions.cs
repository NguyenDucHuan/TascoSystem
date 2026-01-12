using System;

namespace Tasco.UserAuthService.Service.Exceptions
{
    // Base exception for authentication related errors
    public abstract class AuthenticationException : Exception
    {
        protected AuthenticationException(string message) : base(message) { }
        protected AuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    }

    // User related exceptions
    public class UserNotFoundException : AuthenticationException
    {
        public UserNotFoundException() : base("User not found") { }
        public UserNotFoundException(string message) : base(message) { }
    }

    public class EmailNotConfirmedException : AuthenticationException
    {
        public EmailNotConfirmedException() : base("Email address has not been confirmed") { }
        public EmailNotConfirmedException(string message) : base(message) { }
    }

    public class AccountDisabledException : AuthenticationException
    {
        public AccountDisabledException() : base("User account has been disabled") { }
        public AccountDisabledException(string message) : base(message) { }
    }

    public class InvalidCredentialsException : AuthenticationException
    {
        public InvalidCredentialsException() : base("Invalid email or password") { }
        public InvalidCredentialsException(string message) : base(message) { }
    }

    public class EmailAlreadyExistsException : AuthenticationException
    {
        public EmailAlreadyExistsException() : base("An account with this email address already exists") { }
        public EmailAlreadyExistsException(string message) : base(message) { }
    }

    public class UserCreationFailedException : AuthenticationException
    {
        public UserCreationFailedException(string errors) : base($"Failed to create user: {errors}") { }
    }

    public class RoleAssignmentFailedException : AuthenticationException
    {
        public RoleAssignmentFailedException(string errors) : base($"Failed to assign roles: {errors}") { }
    }

    public class EmailConfirmationFailedException : AuthenticationException
    {
        public EmailConfirmationFailedException() : base("Email confirmation failed") { }
        public EmailConfirmationFailedException(string message) : base(message) { }
    }

    public class UserUpdateFailedException : AuthenticationException
    {
        public UserUpdateFailedException(string errors) : base($"Failed to update user: {errors}") { }
    }

    // Configuration related exceptions
    public class ConfigurationMissingException : Exception
    {
        public ConfigurationMissingException(string configKey) : base($"Required configuration '{configKey}' is missing") { }
    }

    // Email service exceptions
    public class EmailTemplateNotFoundException : Exception
    {
        public EmailTemplateNotFoundException(string templatePath) : base($"Email template not found at path: {templatePath}") { }
    }

    public class EmailLogoNotFoundException : Exception
    {
        public EmailLogoNotFoundException(string logoPath) : base($"Email logo image not found at path: {logoPath}") { }
    }

    public class EmailSendFailedException : Exception
    {
        public EmailSendFailedException(string message) : base($"Failed to send email: {message}") { }
        public EmailSendFailedException(string message, Exception innerException) : base($"Failed to send email: {message}", innerException) { }
    }

    // Token service exceptions
    public class TokenGenerationFailedException : Exception
    {
        public TokenGenerationFailedException(string message) : base($"Failed to generate token: {message}") { }
        public TokenGenerationFailedException(string message, Exception innerException) : base($"Failed to generate token: {message}", innerException) { }
    }

    // Validation exceptions
    public class InvalidTokenException : AuthenticationException
    {
        public InvalidTokenException() : base("Invalid or expired token") { }
        public InvalidTokenException(string message) : base(message) { }
    }

    public class InvalidUserIdException : AuthenticationException
    {
        public InvalidUserIdException() : base("Invalid user ID provided") { }
        public InvalidUserIdException(string message) : base(message) { }
    }
} 