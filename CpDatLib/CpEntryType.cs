namespace CpDatLib;

public enum SchemeType : uint
{
    ClassBuilding = 0x80000000,
    ClassRobot = 0x01000000,
    ClassAnimal = 0x20000000,

    BunkerSmall = 0x80010000,
    BunkerMedium = 0x80020000,
    BunkerLarge = 0x80040000,
    Generator = 0x80000002,
    Mine = 0x80000004,
    Storage = 0x80000008,
    Plant = 0x80000010,
    Hangar = 0x80000040,
    TowerMedium = 0x80100000,
    TowerLarge = 0x80200000,
    MainTeleport = 0x80000200,
    Institute = 0x80000400,
    Bridge = 0x80001000,
    Ruine = 0x80002000,

    RobotTransport = 0x01002000,
    RobotBuilder = 0x01004000,
    RobotBattleunit = 0x01008000,
    RobotHq = 0x01010000,
    RobotHero = 0x01020000,
}