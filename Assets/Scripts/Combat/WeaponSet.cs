public enum WeaponSet
{
    None,
    LongRangeOne,
    LongRangeTwo,
    LongRangeTwoHanded,
    MeleeOne,
    MeleeTwo,
    MeleeTwoHanded,
    MeleeAndLongRange,
    Throwen
}

public static class WeaponSetExtension
{
    public static bool IsMeleeSet(this WeaponSet set)
    {
        switch (set)
        {
            case WeaponSet.MeleeOne:
                return true;
            case WeaponSet.MeleeTwo:
                return true;
            case WeaponSet.MeleeTwoHanded:
                return true;
            case WeaponSet.MeleeAndLongRange:
                return true;
            default:
                return false;
        }
    }
}