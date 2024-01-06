namespace PlayFab.Multiplayer
{
    using System;
    using PlayFab.Multiplayer.InteropWrapper;

    /// <summary>
    /// Helper class for the error code of operations.
    /// </summary>
    public class LobbyError
    {
        /// <summary>
        /// Indicates that the operation succeeded.
        /// </summary>
        public const int Success = 0x00000000;
        
        /// <summary>
        /// Indicates that the operation failed due to an invalid argument.
        /// </summary>
        public const int InvalidArg = unchecked((int)0x80070057);

        /// <summary>
        /// Generic test for success on any status value (non-negative numbers indicate success).
        /// </summary>
        /// <param name="error">
        /// Error code of an operation
        /// </param>
        /// <returns>
        /// True for success on any status value, false othwerwise.
        /// </returns>
        public static bool SUCCEEDED(int error)
        {
            return error >= 0;
        }

        /// <summary>
        /// Generic test for failure on any status value.
        /// </summary>
        /// <param name="error">
        /// Error code of an operation.
        /// </param>
        /// <returns>
        /// True for a failed status value, false othwerwise.
        /// </returns>
        public static bool FAILED(int error)
        {
            return !SUCCEEDED(error);
        }
    }
}
