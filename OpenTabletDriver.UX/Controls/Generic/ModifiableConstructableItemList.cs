namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class ModifiableConstructableItemList<T> : ModifiableItemList<T> where T : new()
    {
        protected override void AddNew() => base.Add(ItemSource.Count, new T());
    }
}
