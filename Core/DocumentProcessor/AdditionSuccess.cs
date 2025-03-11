namespace Core.DocumentProcessor;

/// <summary>
/// Represents result of attempt to add badge on page.
/// 0 means unsuccessfull attempt to add badge in processable line through widht lack.
/// 1 means unsuccessfull attempt to add badge in processable line through height lack.
/// </summary>

internal enum AdditionSuccess
{
    FailureByWidth = 0,
    FailureByHeight = 1,
    Success = 2
}