namespace KikaPyde.AdoNetCore.Extensions
{
    public class UsingOptions : IUsingOptions
    {
        public virtual bool AllowTransactionCommitOnSuccessTryFunc { get; set; } = AdoNetCoreHelper.DefaultUsingOptions?.AllowTransactionCommitOnSuccessTryFunc ?? true;
        public virtual bool AllowTransactionRollbackOnFailTryFunc { get; set; } = AdoNetCoreHelper.DefaultUsingOptions?.AllowTransactionRollbackOnFailTryFunc ?? true;
        public virtual bool AllowTransactionRollbackOnFailTryFuncAndCatchFuncIsNull { get; set; } = AdoNetCoreHelper.DefaultUsingOptions?.AllowTransactionRollbackOnFailTryFuncAndCatchFuncIsNull ?? true;
        public virtual bool AllowTransactionCommitOnSuccessCatchFunc { get; set; } = AdoNetCoreHelper.DefaultUsingOptions?.AllowTransactionCommitOnSuccessCatchFunc ?? true;
        public virtual bool AllowTransactionRollbackOnFailCatchFunc { get; set; } = AdoNetCoreHelper.DefaultUsingOptions?.AllowTransactionRollbackOnFailCatchFunc ?? true;
    }
}
