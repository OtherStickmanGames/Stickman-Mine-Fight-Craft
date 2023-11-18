using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class EventsHolder
{

    public class PlayerSpawned : UnityEvent<Player> { }

    public static PlayerSpawned playerSpawnedMine = new PlayerSpawned();

    //-----------------------------------------------------------------------

    public class StickmanDestroyed : UnityEvent<Player> { }

    public static StickmanDestroyed onStickmanDestroyed = new();

    //-----------------------------------------------------------------------

    public class BodyPartCollicion : UnityEvent<CollisionHandler, Collision2D> { }

    public static BodyPartCollicion onBodyPartCollicion = new();

    //-----------------------------------------------------------------------
    public class BlockConnected : UnityEvent<GameObject> { }

    public static BlockConnected onBlockConnected = new();

    //-----------------------------------------------------------------------

    public class MissionComplete : UnityEvent { }

    public static MissionComplete onMissionComplete = new();

    //-----------------------------------------------------------------------

    public class MissionDefeat : UnityEvent { }

    public static MissionDefeat onMissionDefeat = new();

    //-----------------------------------------------------------------------


    public class TileMined : UnityEvent<Mineable> { }

    public static TileMined onTileMined = new();

    //-----------------------------------------------------------------------

    public class PlayerSpawnedAny : UnityEvent<Player> { }

    public static PlayerSpawnedAny playerSpawnedAny = new();

    //-----------------------------------------------------------------------

    public class LeftJoystickMoved : UnityEvent<Vector2> { }

    public static LeftJoystickMoved leftJoystickMoved = new LeftJoystickMoved();

    //-----------------------------------------------------------------------

    public class LeftJoystickUp : UnityEvent { }

    public static LeftJoystickUp onLeftJoystickUp = new();

    //-----------------------------------------------------------------------

    #region    ============== GAME LOGIC ===================
    //         =============================================
    public class PlayerLayerChanged : UnityEvent<int> { }

    public static PlayerLayerChanged onPlayerLayerChanged = new();

    //-----------------------------------------------------------------------
    #endregion =============================================
    public class RightJoystickMoved : UnityEvent<Vector2> { }

    public static RightJoystickMoved rightJoystickMoved = new RightJoystickMoved();

    //-----------------------------------------------------------------------

    public class RightJoystickUp : UnityEvent { }

    public static RightJoystickUp rightJoystickUp = new RightJoystickUp();

    //-----------------------------------------------------------------------

    public class JumpClicked : UnityEvent { }

    public static JumpClicked jumpClicked = new JumpClicked();

    //-----------------------------------------------------------------------

    public class PunchClicked : UnityEvent { }

    public static PunchClicked onPunchClicked = new();

    //-----------------------------------------------------------------------


    public class WeaponPicked : UnityEvent<Player, Weapon> { }

    public static WeaponPicked weaponPicked = new();

    //-----------------------------------------------------------------------

    public class WeaponThrowed : UnityEvent<Player, Weapon> { }

    public static WeaponThrowed weaponThrowed = new WeaponThrowed();

    //-----------------------------------------------------------------------

    public class WeaponTriggered : UnityEvent<Player, Weapon> { }

    public static WeaponTriggered weaponTriggered = new WeaponTriggered();

    //-----------------------------------------------------------------------

    public class WeaponTriggerExit : UnityEvent<Player, Weapon> { }

    public static WeaponTriggerExit weaponTriggerExit = new WeaponTriggerExit();

    //-----------------------------------------------------------------------

    public class PlayerKeepItem : UnityEvent<GameObject> { }

    public static PlayerKeepItem onPlayerKeepItem = new();

    //-----------------------------------------------------------------------

    public class PlayerDropItem : UnityEvent { }

    public static PlayerDropItem onPlayerDropItem = new();

    //-----------------------------------------------------------------------

    public class MeleeStucked : UnityEvent<Melee> { }

    public static MeleeStucked onMeleeStucked = new();

    //-----------------------------------------------------------------------

    public class WeaponSetInited : UnityEvent<WeaponSet> { }

    public static WeaponSetInited weaponSetInited = new WeaponSetInited();

    //-----------------------------------------------------------------------
    public class ObjectSpawned : UnityEvent<GameObject> { }

    public static ObjectSpawned onObjectSpawned = new();

    //-----------------------------------------------------------------------

    public class CritPunch : UnityEvent<Player> { }

    public static CritPunch onCritPunch = new();

    //-----------------------------------------------------------------------

    public class MineModeSwitch : UnityEvent<bool> { }

    public static MineModeSwitch onMineModeSwitch = new();

    //-----------------------------------------------------------------------

    public class LeftControl : UnityEvent { }

    public static LeftControl onLeftControl = new();

    //-----------------------------------------------------------------------

    //========================= UI ============================

    public class TileViewClick : UnityEvent<int> { }

    public static TileViewClick onTileViewClick = new();

    //-----------------------------------------------------------------------
    public class BuildEditorMode : UnityEvent<bool> { }

    public static BuildEditorMode onBuildEditorMode = new();

    //-----------------------------------------------------------------------

    public class BtnCamClicked : UnityEvent { }

    public static BtnCamClicked onBtnCamClicked = new();

    //-----------------------------------------------------------------------

    public class BtnNextLayer : UnityEvent { }

    public static BtnNextLayer onBtnNextLayer = new();

    //-----------------------------------------------------------------------

    public class BtnPrevLayer : UnityEvent { }

    public static BtnPrevLayer onBtnPrevLayer = new();

    //-----------------------------------------------------------------------

}
