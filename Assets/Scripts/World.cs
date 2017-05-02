﻿using System.Collections;
using UnityEngine;

public class World : MonoBehaviour
{
    void Start()
    {
        var town = new LevelBuilder("Act 1 - Town");
        var bloodMoor = CreateBloodMoor();

        var townOffset = new Vector2i(bloodMoor.gridWidth * bloodMoor.gridX - town.gridWidth * town.gridX, bloodMoor.gridHeight * bloodMoor.gridY);
        town.Instantiate(townOffset);
        bloodMoor.Instantiate(new Vector2i(0, 0));

        var doe = CreateDenOfEvil();
        var doeOffset = new Vector2i(120, 0);
        doe.Instantiate(doeOffset);

        var entry = town.FindEntry();
        SpawnPlayer(Iso.MapTileToWorld(entry + townOffset));
    }

    LevelBuilder CreateDenOfEvil()
    {
        var builder = new LevelBuilder("Act 1 - Cave 1", 24, 24);
        var palette = new Maze.Palette();
        palette.special = new LevelPreset[][] {
            new LevelPreset[] {
                LevelPreset.Find("Act 1 - Cave Prev W"),
                LevelPreset.Find("Act 1 - Cave Prev E"),
                LevelPreset.Find("Act 1 - Cave Prev S"),
                LevelPreset.Find("Act 1 - Cave Prev N")
            },
            new LevelPreset[] {
                LevelPreset.Find("Act 1 - Cave Den Of Evil W"),
                LevelPreset.Find("Act 1 - Cave Den Of Evil E"),
                LevelPreset.Find("Act 1 - Cave Den Of Evil S"),
                LevelPreset.Find("Act 1 - Cave Den Of Evil N")
            }
        };
        palette.rooms = new LevelPreset[16];
        for (int i = 0; i < 15; ++i)
            palette.rooms[i + 1] = LevelPreset.sheet[53 + i];
        palette.themedRooms = new LevelPreset[16];
        for (int i = 0; i < 15; ++i)
            palette.themedRooms[i + 1] = LevelPreset.sheet[68 + i];
        Maze.Generate(builder, palette);
        return builder;
    }

    LevelBuilder CreateBloodMoor()
    {
        var bloodMoor = new LevelBuilder("Act 1 - Wilderness 1", 8, 8);
        var riverN = DS1.Load(@"data\global\tiles\act1\outdoors\UriverN.ds1");
        var uRiver = DS1.Load(@"data\global\tiles\act1\outdoors\Uriver.ds1");
        var lRiver = DS1.Load(@"data\global\tiles\act1\outdoors\Lriver.ds1");
        var bord1 = LevelPreset.Find("Act 1 - Wild Border 1");
        var bord2 = LevelPreset.Find("Act 1 - Wild Border 2");
        var bord3 = LevelPreset.Find("Act 1 - Wild Border 3");
        var bord5 = LevelPreset.Find("Act 1 - Wild Border 5");
        var bord6 = LevelPreset.Find("Act 1 - Wild Border 6");
        var bord9 = LevelPreset.Find("Act 1 - Wild Border 9");
        var cottage = LevelPreset.Find("Act 1 - Cottages 1");
        var denEntrance = LevelPreset.Find("Act 1 - DOE Entrance");

        for (int i = 0; i < bloodMoor.gridHeight; ++i)
            bloodMoor.Place(lRiver, new Vector2i(bloodMoor.gridWidth - 1, i));
        for (int i = 1; i < bloodMoor.gridHeight; ++i)
            bloodMoor.Place(uRiver, new Vector2i(bloodMoor.gridWidth - 2, i));
        bloodMoor.Place(riverN, new Vector2i(bloodMoor.gridWidth - 2, 0));

        for (int i = 1; i < bloodMoor.gridHeight - 1; ++i)
            bloodMoor.Place(bord2, new Vector2i(0, i), 0, 3);
        bloodMoor.Place(bord5, new Vector2i(0, bloodMoor.gridHeight - 1));

        for (int i = 1; i < 3; ++i)
            bloodMoor.Place(bord1, new Vector2i(i, bloodMoor.gridHeight - 1), 0, 3);
        bloodMoor.Place(bord9, new Vector2i(3, bloodMoor.gridHeight - 1));

        for (int i = 1; i < bloodMoor.gridWidth - 2; ++i)
            bloodMoor.Place(bord3, new Vector2i(i, 0), 0, 3);

        bloodMoor.Place(bord6, new Vector2i(0, 0));
        for (int i = 1; i < 5; ++i)
            bloodMoor.Place(cottage, new Vector2i(i, 4 + Random.Range(-1, 1)));
        bloodMoor.Place(denEntrance, new Vector2i(5, 7));

        return bloodMoor;
    }

    static void SpawnPlayer(Vector3 pos)
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = pos;
        var character = player.AddComponent<Character>();
        character.basePath = @"data\global\chars";
        character.token = "BA";
        character.weaponClass = "1SS";
        character.gear = new string[] { "LIT", "LIT", "LIT", "LIT", "LIT", "AXE", "AXE", "", "LIT", "LIT", "", "", "", "", "", "" };
        //character.token = "PA";
        //character.weaponClass = "1HS";
        //character.gear = new string[] { "CRN", "HVY", "HVY", "HVY", "HVY", "SCM", "", "KIT", "", "", "", "", "", "", "", "" };
        character.directionCount = 16;
        character.run = true;
        character.walkSpeed = 7;
        character.runSpeed = 15;
        character.maxHealth = 10000;
        character.health = 10000;
        PlayerController.instance.SetCharacter(character);

        var body = player.AddComponent<Rigidbody2D>();
        body.isKinematic = true;
        var collider = player.AddComponent<CircleCollider2D>();
        collider.radius = Iso.tileSizeY;
    }

    public static Character SpawnMonster(string id, Vector3 pos, Transform parent = null)
    {
        MonStat monStat = MonStat.Find(id);
        if (monStat == null)
        {
            Debug.LogWarning("Monster id not found: " + id);
            return null;
        }
        return SpawnMonster(monStat, pos);
    }

    public static Character SpawnMonster(MonStat monStat, Vector3 pos, Transform parent = null)
    {
        pos = Iso.MapToIso(pos);
        if (!CollisionMap.Fit(pos, out pos, monStat.ext.sizeX))
        {
            return null;
        }
        pos = Iso.MapToWorld(pos);

        var monster = new GameObject(monStat.nameStr);
        monster.transform.SetParent(parent);
        monster.transform.position = pos;

        var character = monster.AddComponent<Character>();
        character.monStat = monStat;
        character.title = monStat.name;
        character.basePath = @"data\global\monsters";
        character.token = monStat.code;
        character.weaponClass = monStat.ext.baseWeaponClass;
        character.run = false;
        character.walkSpeed = monStat.speed;
        character.runSpeed = monStat.runSpeed;
        int health = Random.Range(monStat.minHP, monStat.maxHP + 1);
        character.health = health;
        character.maxHealth = health;

        character.gear = new string[monStat.ext.gearVariants.Length];
        for (int i = 0; i < character.gear.Length; ++i)
        {
            var variants = monStat.ext.gearVariants[i];
            if (variants == null)
                continue;
            character.gear[i] = variants[Random.Range(0, variants.Length)];
        }

        if (monStat.ai == "Npc")
            monster.AddComponent<NpcController>();
        else if (monStat.ai != "Idle" && monStat.ai != "NpcStationary")
            monster.AddComponent<MonsterController>();

        var body = monster.AddComponent<Rigidbody2D>();
        body.isKinematic = true;
        var collider = monster.AddComponent<CircleCollider2D>();
        collider.radius = monStat.ext.sizeX * Iso.tileSizeY;

        return character;
    }

    public static StaticObject SpawnObject(ObjectInfo objectInfo, Vector3 pos, bool fit = false, Transform parent = null)
    {
        if (fit)
        {
            pos = Iso.MapToIso(pos);
            if (!CollisionMap.Fit(pos, out pos, objectInfo.sizeX))
            {
                return null;
            }
            pos = Iso.MapToWorld(pos);
        }

        var gameObject = new GameObject(objectInfo.description);
        gameObject.transform.position = pos;

        var staticObject = gameObject.AddComponent<StaticObject>();
        staticObject.objectInfo = objectInfo;
        staticObject.title = objectInfo.name;

        gameObject.transform.SetParent(parent, true);

        return staticObject;
    }

    public static StaticObject SpawnObject(string token, Vector3 pos)
    {
        ObjectInfo objectInfo = ObjectInfo.Find(token);
        if (objectInfo == null)
        {
            Debug.LogWarning("ObjectInfo with token'" + token + "' not found");
            return null;
        }
        return SpawnObject(objectInfo, pos);
    }

    public static Pickup SpawnItem(string code, Vector3 pos)
    {
        var item = ItemInfo.Find(code);
        if (item == null)
            return null;

        return Pickup.Create(pos, item.flippyFile, item.name);
    }
}
