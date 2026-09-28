using System.Collections.Generic;
using UnityEngine;

public static class CityArt
{
    public static GameObject Character { get; private set; }
    public static RuntimeAnimatorController Moves { get; private set; }
    public static GameObject[] Facades { get; private set; }
    public static GameObject Corridor { get; private set; }
    public static GameObject Plant { get; private set; }
    public static GameObject Console { get; private set; }
    public static GameObject Generator { get; private set; }
    public static GameObject Door { get; private set; }
    public static GameObject Soldier { get; private set; }
    public static GameObject Pistol { get; private set; }
    public static GameObject Rifle { get; private set; }
    public static GameObject[] Ships { get; private set; }
    public static GameObject Bomber { get; private set; }
    public static GameObject BayFloor { get; private set; }
    public static GameObject BayWall { get; private set; }
    public static GameObject BayRoof { get; private set; }
    public static GameObject BayDoor { get; private set; }
    public static Material SpaceSky { get; private set; }
    public static Material StarSky { get; private set; }
    public static Texture2D EarthFace { get; private set; }

    static bool _loaded;

    public static void Load()
    {
        if (_loaded)
            return;
        _loaded = true;

#if UNITY_EDITOR
        Character = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/FreeTestCharacterAsuna/FBX/FreeTestCharacterAsuna/FreeTestCharacterAsuna.fbx");
        Moves = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/FreeTestCharacterAsuna/FBX/Animations/FreeTestAnimationController.controller");

        string[] paths =
        {
            "Assets/Sci-Fi Styled Modular Pack/Prefabs/Walls/decorative_wall_1.prefab",
            "Assets/Sci-Fi Styled Modular Pack/Prefabs/Walls/decorative_wall_2.prefab",
            "Assets/Sci-Fi Styled Modular Pack/Prefabs/Walls/decorative_wall_3.prefab",
            "Assets/Sci-Fi Styled Modular Pack/Prefabs/Walls/decorative_wall_5.prefab",
            "Assets/Sci-Fi Styled Modular Pack/Prefabs/Walls/decorative_wall_6.prefab",
            "Assets/Sci-Fi Styled Modular Pack/Prefabs/Walls/wall_big.prefab",
            "Assets/Sci-Fi Styled Modular Pack/Prefabs/Windows/Complete Windows/window_big_corner.prefab"
        };

        var facades = new List<GameObject>();
        for (int i = 0; i < paths.Length; i++)
        {
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            if (prefab != null)
                facades.Add(prefab);
        }

        Facades = facades.ToArray();
        Corridor = Load("Assets/Sci-Fi Styled Modular Pack/Prefabs/Corridors/Corridor_I.prefab");
        Plant = Load("Assets/Sci-Fi Styled Modular Pack/Prefabs/Decorative elements/decorative_plant.prefab");
        Console = Load("Assets/Sci-Fi Styled Modular Pack/Prefabs/Decorative elements/console.prefab");
        Generator = Load("Assets/Sci-Fi Styled Modular Pack/Prefabs/Machines/generator.prefab");
        Door = Load("Assets/Sci-Fi Styled Modular Pack/Prefabs/Doors/door_1.prefab");
        Soldier = Load("Assets/MyAssets/CyberSoldier/CyberSoldier.fbx");
        Pistol = Load("Assets/FreeTestCharacterAsuna/Prefabs/CharacterStandardEquipment/ScifiPistolMNL21MasterPrefab.prefab");
        Rifle = Load("Assets/FreeTestCharacterAsuna/Prefabs/CharacterStandardEquipment/ScifiRifleWLT78MasterPrefab.prefab");
        Ships = new[]
        {
            Load("Assets/HiRezSpaceshipsCreatorFree/Prefabs/Examples/Example1_Grey.prefab"),
            Load("Assets/HiRezSpaceshipsCreatorFree/Prefabs/Examples/Example2_Grey.prefab"),
            Load("Assets/HiRezSpaceshipsCreatorFree/Prefabs/Examples/Example3_Red.prefab"),
            Load("Assets/HiRezSpaceshipsCreatorFree/Prefabs/Examples/Example4_Grey.prefab"),
            Load("Assets/HiRezSpaceshipsCreatorFree/Prefabs/Examples/Example5_Grey.prefab"),
            Load("Assets/HiRezSpaceshipsCreatorFree/Prefabs/ExamplesNoInterior/Example1NoInterior_Grey.prefab")
        };
        Bomber = Load("Assets/Hessburg - Stealth Bomber/Prefabs/Stealth_Bomber.prefab");
        if (Bomber == null)
            Bomber = Load("Assets/Hessburg - Stealth Bomber/Stealth_Bomber.fbx");
        BayFloor = Load("Assets/Barking_Dog/3D Free Modular Kit/Prefabs/Floor_01.prefab");
        BayWall = Load("Assets/Barking_Dog/3D Free Modular Kit/Prefabs/Wall_Simple_01.prefab");
        BayRoof = Load("Assets/Barking_Dog/3D Free Modular Kit/Prefabs/Roof_01.prefab");
        BayDoor = Load("Assets/Barking_Dog/3D Free Modular Kit/Prefabs/Door_Arch_01.prefab");
        SpaceSky = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Stagit/SkyboxEarthPlanets/skyboxes/skyboxv1_earthfar.mat");
        StarSky = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Stagit/SkyboxEarthPlanets/skyboxes/skyboxv1_starsonly.mat");
        EarthFace = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Stagit/SkyboxEarthPlanets/textures/skyboxv1_earthfar/earth300001.png");
#else
        Facades = new GameObject[0];
#endif
    }

#if UNITY_EDITOR
    static GameObject Load(string path)
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
#endif

    public static GameObject LoadProp(string path)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
        return null;
#endif
    }
}
