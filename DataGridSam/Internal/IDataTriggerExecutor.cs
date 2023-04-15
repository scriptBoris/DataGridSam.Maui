namespace DataGridSam.Internal;

public interface IDataTriggerExecutor
{
    /// <summary>
    /// Try set or unset unsetup trigger on this object by value
    /// </summary>
    /// <returns>
    /// Returns Bool flag as answered of has changed or not?
    /// </returns>
    /// <param name="trigger">Context trigger</param>
    /// <param name="value">Value for trigger conditions</param>
    bool ExecuteTrigger(IDataTrigger trigger, object? value);
}