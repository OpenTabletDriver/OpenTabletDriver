namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class ModifiableConstructableItemList<T> : ModifiableItemList<T> where T : new()
    {
        protected override void AddNew(int index) => base.Add(index, new T());
    }
}