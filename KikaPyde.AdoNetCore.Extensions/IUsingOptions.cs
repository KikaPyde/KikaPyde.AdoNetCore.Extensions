﻿namespace KikaPyde.AdoNetCore.Extensions
{
    public interface IUsingOptions
    {
        /// <summary>
        /// If the tryFunc was completed successfully
        /// </summary>
        public bool AllowTransactionCommitOnSuccessTryFunc { get; set; }
        /// <summary>
        /// If the catchFunc was completed successfully
        /// </summary>
        public bool AllowTransactionCommitOnSuccessCatchFunc { get; set; }
        /// <summary>
        /// If the catchFunc is null
        /// </summary>
        public bool AllowTransactionRollbackOnFailTryFunc { get; set; }
        /// <summary>
        /// If the catchFunc failed
        /// </summary>
        public bool AllowTransactionRollbackOnFailCatchFunc { get; set; }
    }
}
