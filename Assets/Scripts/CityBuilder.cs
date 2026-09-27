using System.Collections.Generic;
using UnityEngine;

public static class CityBuilder
{
    static readonly Color[] BuildingColors =
    {
        new Color(0.78f, 0.76f, 0.71f),
        new Color(0.58f, 0.62f, 0.66f),
        new Color(0.86f, 0.82f, 0.74f),
        new Color(0.46f, 0.52f, 0.58f),
        new Color(0.69f, 0.5f, 0.44f),
        new Color(0.9f, 0.9f, 0.88f),
        new Color(0.42f, 0.48f, 0.46f),
        new Color(0.73f, 0.7f, 0.62f)
    };

    static readonly Color[] CarColors =
    {
        new Color(0.75f, 0.12f, 0.1f),
        new Color(0.12f, 0.28f, 0.72f),
        new Color(0.92f, 0.9f, 0.86f),
        new Color(0.9f, 0.72f, 0.12f),
        new Color(0.1f, 0.11f, 0.12f),
        new Color(0.12f, 0.55f, 0.28f)
    };

    public static Vector3 Build()
    {
        Random.InitState(23);
        CityArt.Load();
        LabDoor.Reset();
        var root = new GameObject("Sehir");

        float map = CityLayout.Half * 2f + 80f;
        Primitive(root.transform, "Zemin", new Vector3(0f, -1f, 0f), new Vector3(map, 2f, map), new Color(0.18f, 0.19f, 0.2f), 0.02f);

        var curb = new PhysicsMaterial("Kaldirim")
        {
            dynamicFriction = 0.8f,
            staticFriction = 0.9f,
            bounciness = 0f
        };

        for (int i = 0; i <= CityLayout.Blocks; i++)
        {
            float line = -CityLayout.Half + i * CityLayout.Cell;
            float roadLength = CityLayout.Blocks * CityLayout.Cell + CityLayout.Road;
            Visual(root.transform, "YolZ", new Vector3(line, 0.025f, 0f), new Vector3(CityLayout.Road, 0.05f, roadLength), new Color(0.22f, 0.23f, 0.25f), 0.08f);
            Visual(root.transform, "YolX", new Vector3(0f, 0.03f, line), new Vector3(roadLength, 0.05f, CityLayout.Road), new Color(0.22f, 0.23f, 0.25f), 0.08f);

            if (i % 3 == 0)
            {
                for (float d = -CityLayout.Half; d < CityLayout.Half; d += 16f)
                {
                    Visual(root.transform, "Serit", new Vector3(line, 0.06f, d), new Vector3(0.18f, 0.02f, 2.4f), new Color(0.85f, 0.75f, 0.25f), 0.1f);
                    Visual(root.transform, "Serit", new Vector3(d, 0.065f, line), new Vector3(2.4f, 0.02f, 0.18f), new Color(0.85f, 0.75f, 0.25f), 0.1f);
                }
            }
        }

        for (int i = 0; i <= CityLayout.Blocks; i += 2)
        {
            float line = -CityLayout.Half + i * CityLayout.Cell;
            float edge = CityLayout.Road * 0.5f + 1.4f;
            for (float d = -CityLayout.Half + 18f; d < CityLayout.Half; d += 52f)
            {
                LampPost(root.transform, new Vector3(line + edge, 0f, d));
                LampPost(root.transform, new Vector3(d, 0f, line + edge));
            }
        }

        for (int ix = 0; ix < CityLayout.Blocks; ix++)
        {
            for (int iz = 0; iz < CityLayout.Blocks; iz++)
            {
                float originX = -CityLayout.Half + ix * CityLayout.Cell + CityLayout.Road * 0.5f + CityLayout.SidewalkWidth;
                float originZ = -CityLayout.Half + iz * CityLayout.Cell + CityLayout.Road * 0.5f + CityLayout.SidewalkWidth;
                float inner = CityLayout.Cell - CityLayout.Road - CityLayout.SidewalkWidth * 2f;
                float centerX = -CityLayout.Half + ix * CityLayout.Cell + CityLayout.Cell * 0.5f;
                float centerZ = -CityLayout.Half + iz * CityLayout.Cell + CityLayout.Cell * 0.5f;

                var sidewalk = Primitive(root.transform, "Kaldirim",
                    new Vector3(centerX, CityLayout.SidewalkHeight * 0.5f, centerZ),
                    new Vector3(inner + CityLayout.SidewalkWidth * 2f, CityLayout.SidewalkHeight, inner + CityLayout.SidewalkWidth * 2f),
                    new Color(0.62f, 0.62f, 0.6f), 0.05f);
                sidewalk.GetComponent<Collider>().material = curb;

                if ((ix * 5 + iz) % 18 == 0)
                {
                    PlantPark(root.transform, originX, originZ, inner);
                    continue;
                }

                DressBlock(root.transform, originX, originZ, inner);

                int columns = 2;
                int rows = 2;
                for (int sx = 0; sx < columns; sx++)
                {
                    for (int sz = 0; sz < rows; sz++)
                    {
                        if (Random.value < 0.05f)
                            continue;

                        float slotX = inner / columns;
                        float slotZ = inner / rows;
                        float width = slotX * Random.Range(0.84f, 0.97f);
                        float depth = slotZ * Random.Range(0.84f, 0.97f);
                        float height = Random.value > 0.7f ? Random.Range(26f, 48f) : Random.Range(9f, 22f);
                        float x = originX + sx * slotX + slotX * 0.5f;
                        float z = originZ + sz * slotZ + slotZ * 0.5f;
                        if (!PlaceArtBuilding(root.transform, x, z, width, depth, height))
                        {
                            Color color = BuildingColors[Random.Range(0, BuildingColors.Length)];
                            Primitive(root.transform, "Bina",
                                new Vector3(x, CityLayout.SidewalkHeight + height * 0.5f, z),
                                new Vector3(width, height, depth),
                                color, 0.18f);

                            if (height > 12f)
                            {
                                Visual(root.transform, "Cam",
                                    new Vector3(x, CityLayout.SidewalkHeight + height * 0.62f, z),
                                    new Vector3(width * 0.72f, height * 0.45f, depth * 0.72f),
                                    new Color(0.55f, 0.72f, 0.82f), 0.55f);
                            }
                        }
                    }
                }
            }
        }

        SpawnWalkers(root.transform);
        return new Vector3(CityLayout.Road * 0.5f + 2.2f, 0.45f, 8f);
    }

    static void FitHeight(GameObject go, float height)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        if (bounds.size.y < 0.01f)
            return;
        go.transform.localScale *= height / bounds.size.y;
    }

    static void LampPost(Transform parent, Vector3 position)
    {
        Visual(parent, "Direk", position + new Vector3(0f, 2.2f, 0f), new Vector3(0.12f, 4.4f, 0.12f), new Color(0.2f, 0.22f, 0.24f), 0.3f, true);
        Visual(parent, "Lamba", position + new Vector3(0f, 4.45f, 0f), new Vector3(0.45f, 0.12f, 0.45f), new Color(1f, 0.92f, 0.7f), 0.6f, true);
        var lamp = new GameObject("SokakLambasi");
        lamp.transform.SetParent(parent, false);
        lamp.transform.position = position + new Vector3(0f, 4.2f, 0f);
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.9f, 0.7f);
        light.range = 16f;
        light.intensity = 1.3f;
        light.shadows = LightShadows.None;
    }

    public static void SpawnShips()
    {
        if (CityArt.Ships == null)
            return;

        float y = 4f;
        int model = 0;
        CreateShip(new Vector3(0f, y, 18f), Quaternion.identity, model++);
        for (int i = 1; i < CityLayout.Blocks; i += 2)
        {
            float line = -CityLayout.Half + i * CityLayout.Cell;
            float side = i % 4 == 1 ? 8f : -8f;
            float high = 6f + (i % 3) * 10f;
            CreateShip(new Vector3(line + side, high, 26f), Quaternion.Euler(0f, 180f, 0f), model++);
            CreateShip(new Vector3(26f, high + 8f, line + side), Quaternion.Euler(0f, 90f, 0f), model++);
        }
    }

    public static void SpawnSkyline()
    {
        if (CityArt.Ships == null)
            return;

        Vector3[] pads =
        {
            new Vector3(14f, 18f, 30f),
            new Vector3(-22f, 26f, 8f),
            new Vector3(36f, 34f, -12f),
            new Vector3(-8f, 42f, -28f),
            new Vector3(60f, 22f, 48f),
            new Vector3(-48f, 30f, 36f),
            new Vector3(8f, 50f, 64f),
            new Vector3(-36f, 16f, -20f)
        };
        for (int i = 0; i < pads.Length; i++)
            SkyPad(pads[i], i);

        float[] radii = { 28f, 36f, 44f, 52f, 64f, 76f, 88f, 40f, 58f, 96f, 110f, 24f, 70f, 84f, 48f, 120f };
        float[] heights = { 11f, 14f, 18f, 22f, 16f, 28f, 34f, 40f, 26f, 46f, 20f, 13f, 32f, 38f, 24f, 54f };
        Vector3[] centers =
        {
            new Vector3(10f, 0f, 12f),
            new Vector3(-16f, 0f, 20f),
            new Vector3(24f, 0f, -8f),
            Vector3.zero
        };
        for (int i = 0; i < radii.Length; i++)
            SkyShip(centers[i % centers.Length], radii[i], heights[i], 9f + (i % 5) * 2f, i);
    }

    static void SkyPad(Vector3 position, int index)
    {
        string floor = index % 2 == 0
            ? "Assets/Sci-Fi Styled Modular Pack/Prefabs/Floors/floor_2.prefab"
            : "Assets/Sci-Fi Styled Modular Pack/Prefabs/Floors/floor_3.prefab";
        GameObject prefab = CityArt.LoadProp(floor);
        if (prefab != null)
        {
            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    GameObject tile = Object.Instantiate(prefab, position + new Vector3(x * 4f, 0f, z * 4f), Quaternion.identity);
                    Collider[] colliders = tile.GetComponentsInChildren<Collider>();
                    for (int c = 0; c < colliders.Length; c++)
                        colliders[c].enabled = false;
                }
            }
        }

        var lamp = new GameObject("PistIsik");
        lamp.transform.position = position + new Vector3(4f, 3f, 4f);
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.6f, 0.85f, 1f);
        light.range = 22f;
        light.intensity = 2f;
        light.shadows = LightShadows.None;

        if (CityArt.Ships != null && CityArt.Ships.Length > 0 && CityArt.Ships[index % CityArt.Ships.Length] != null)
        {
            GameObject parked = Object.Instantiate(CityArt.Ships[index % CityArt.Ships.Length], position + new Vector3(4f, 2.2f, 4f), Quaternion.Euler(0f, 40f + index * 30f, 0f));
            FitLongest(parked, 8f);
            Collider[] colliders = parked.GetComponentsInChildren<Collider>();
            for (int c = 0; c < colliders.Length; c++)
                colliders[c].enabled = false;
            var hull = parked.AddComponent<BoxCollider>();
            hull.center = new Vector3(0f, 0.3f, 0f);
            hull.size = new Vector3(7.2f, 2.2f, 9.2f);
            parked.AddComponent<ShipHull>();
        }
    }

    static void SkyShip(Vector3 center, float radius, float height, float speed, int model)
    {
        if (CityArt.Ships == null || CityArt.Ships.Length == 0)
            return;
        GameObject prefab = CityArt.Ships[model % CityArt.Ships.Length];
        if (prefab == null)
            return;

        var root = new GameObject("Devriye");
        GameObject visual = Object.Instantiate(prefab, root.transform);
        visual.transform.localPosition = Vector3.zero;
        FitLongest(visual, 10f);
        Collider[] colliders = visual.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        var lane = root.AddComponent<SkyLane>();
        lane.Center = center;
        lane.Radius = radius;
        lane.Height = height;
        lane.Speed = speed;
    }

    public static void SpawnRings()
    {
        SkyRing.ResetCount();
        Vector3[] spots =
        {
            new Vector3(0f, 14f, 52f),
            new Vector3(0f, 22f, 110f),
            new Vector3(70f, 18f, 110f),
            new Vector3(70f, 30f, 40f),
            new Vector3(-40f, 16f, 40f),
            new Vector3(-40f, 36f, -30f),
            new Vector3(20f, 26f, -70f),
            new Vector3(0f, 48f, 0f)
        };
        for (int i = 0; i < spots.Length; i++)
            CreateRing(spots[i]);
    }

    static void CreateRing(Vector3 position)
    {
        var root = new GameObject("Halka");
        root.transform.position = position;
        var glow = GlowMaterial(new Color(0.15f, 0.82f, 0.95f));
        Hoop(root.transform, glow, 11f, 0.28f, 48);
        Hoop(root.transform, glow, 9.2f, 0.12f, 48);

        var lightObject = new GameObject("HalkaIsik");
        lightObject.transform.SetParent(root.transform, false);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.45f, 0.9f, 1f);
        light.range = 18f;
        light.intensity = 1.4f;
        light.shadows = LightShadows.None;

        var trigger = root.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 8f;
        var ring = root.AddComponent<SkyRing>();
        ring.Glow = glow;
    }

    static void Hoop(Transform parent, Material material, float radius, float thickness, int pieces)
    {
        float length = (Mathf.PI * 2f * radius / pieces) * 1.05f;
        for (int i = 0; i < pieces; i++)
        {
            float angle = i / (float)pieces * Mathf.PI * 2f;
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            piece.name = "Halka";
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Vector3 tangent = new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0f);
            piece.transform.localRotation = Quaternion.FromToRotation(Vector3.up, tangent);
            piece.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);
            piece.GetComponent<Renderer>().sharedMaterial = material;
            Object.Destroy(piece.GetComponent<Collider>());
        }
    }

    static Material GlowMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");
        var material = new Material(shader);
        material.color = color;
        material.EnableKeyword("_EMISSION");
        if (material.HasProperty("_EmissionColor"))
            material.SetColor("_EmissionColor", color * 0.45f);
        return material;
    }

    static void CreateShip(Vector3 position, Quaternion rotation, int index)
    {
        GameObject prefab = null;
        if (CityArt.Ships != null && CityArt.Ships.Length > 0)
            prefab = CityArt.Ships[index % CityArt.Ships.Length];
        if (prefab == null)
            return;

        var root = new GameObject("Gemi");
        root.transform.SetPositionAndRotation(position, rotation);

        var body = root.AddComponent<Rigidbody>();
        body.mass = 800f;
        body.linearDamping = 0.05f;
        body.angularDamping = 4f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.constraints = RigidbodyConstraints.None;
        body.useGravity = false;

        var box = root.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.4f, 0f);
        box.size = new Vector3(6.5f, 1.6f, 8.5f);
        root.AddComponent<ShipHull>();

        GameObject visual = Object.Instantiate(prefab, root.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        FitLongest(visual, 8.5f);
        Collider[] colliders = visual.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        var beacon = Visual(root.transform, "Isaret", new Vector3(0f, 3.2f, 0f), new Vector3(0.35f, 0.35f, 0.35f), new Color(0.3f, 0.85f, 1f), 0.6f);
        var glow = Visual(root.transform, "Isaret", new Vector3(0f, 0.15f, -3.4f), new Vector3(0.55f, 0.55f, 0.55f), new Color(0.35f, 0.85f, 1f), 0.9f);
        var ship = root.AddComponent<DriveableCar>();
        ship.Beacon = beacon.transform;
        ship.Glow = glow.transform;
        ship.Flight = true;
        ship.RestHeight = position.y;
    }

    static void FitLongest(GameObject go, float size)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 0; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        float longest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        if (longest < 0.01f)
            return;
        go.transform.localScale *= size / longest;
    }

    static void DressBlock(Transform parent, float originX, float originZ, float inner)
    {
        GameObject plant = CityArt.LoadProp("Assets/Sci-Fi Styled Modular Pack/Prefabs/Decorative elements/decorative_plant_small.prefab");
        Vector3[] corners =
        {
            new Vector3(originX + 1.2f, 0f, originZ + 1.2f),
            new Vector3(originX + inner - 1.2f, 0f, originZ + 1.2f),
            new Vector3(originX + 1.2f, 0f, originZ + inner - 1.2f),
            new Vector3(originX + inner - 1.2f, 0f, originZ + inner - 1.2f)
        };
        for (int i = 0; i < corners.Length; i++)
        {
            if (plant != null && i % 2 == 0)
            {
                GameObject prop = Object.Instantiate(plant, corners[i], Quaternion.Euler(0f, i * 40f, 0f), parent);
                FitHeight(prop, 0.9f);
                Collider[] colliders = prop.GetComponentsInChildren<Collider>();
                for (int c = 0; c < colliders.Length; c++)
                    colliders[c].enabled = false;
                Block(prop);
            }
            else
            {
                Visual(parent, "Saksı", corners[i] + new Vector3(0f, 0.35f, 0f), new Vector3(0.7f, 0.7f, 0.7f), new Color(0.2f, 0.55f, 0.38f), 0.15f, true);
            }
        }
    }

    static void PlantPark(Transform parent, float originX, float originZ, float inner)
    {
        var grass = new Color(0.28f, 0.48f, 0.27f);
        var trunk = new Color(0.4f, 0.28f, 0.16f);
        var leaf = new Color(0.18f, 0.5f, 0.22f);
        for (int i = 0; i < 3; i++)
        {
            float x = originX + inner * (0.25f + 0.25f * i);
            float z = originZ + inner * (0.3f + 0.18f * (i % 2));
            Visual(parent, "Govde", new Vector3(x, 1.3f, z), new Vector3(0.35f, 2.2f, 0.35f), trunk, 0.05f, true);
            Visual(parent, "Kafa", new Vector3(x, 2.8f, z), new Vector3(2.2f, 1.6f, 2.2f), leaf, 0.1f, true);
        }
        Visual(parent, "Cam", new Vector3(originX + inner * 0.5f, 0.28f, originZ + inner * 0.5f), new Vector3(inner * 0.7f, 0.08f, inner * 0.7f), grass, 0.02f);
    }

    static void SpawnWalkers(Transform parent)
    {
        float shoulder = CityLayout.Road * 0.5f + 1.6f;
        for (int i = 0; i <= CityLayout.Blocks; i++)
        {
            float line = -CityLayout.Half + i * CityLayout.Cell;
            CreateWalker(parent, new Vector3(line + shoulder, 0.22f, -70f + i * 14f), Vector3.forward);
            CreateWalker(parent, new Vector3(line + shoulder, 0.22f, 20f + i * 11f), Vector3.forward);
            CreateWalker(parent, new Vector3(line - shoulder, 0.22f, 80f - i * 13f), Vector3.back);
            CreateWalker(parent, new Vector3(line - shoulder, 0.22f, -15f - i * 9f), Vector3.back);
            CreateWalker(parent, new Vector3(-55f + i * 12f, 0.22f, line + shoulder), Vector3.right);
        }
    }

    static void CreateWalker(Transform parent, Vector3 position, Vector3 direction)
    {
        var person = new GameObject("Asker");
        person.transform.SetParent(parent, false);
        person.transform.position = position;
        var walker = person.AddComponent<StreetWalker>();
        walker.Direction = direction;
        walker.Speed = Random.Range(1.3f, 2.1f);
        walker.Limit = CityLayout.Half - 8f;

        if (CityArt.Soldier == null)
            return;

        GameObject model = Object.Instantiate(CityArt.Soldier, person.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        FitHeight(model, 1.85f);
        AlignFeet(model);
        model.AddComponent<SoldierMarch>();
        Collider[] colliders = model.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        var body = person.AddComponent<CapsuleCollider>();
        body.isTrigger = true;
        body.height = 1.7f;
        body.radius = 0.38f;
        body.center = new Vector3(0f, 0.95f, 0f);

        Vital vital = person.AddComponent<Vital>();
        vital.Team = 1;
        vital.Max = 30f;
        vital.Current = 30f;
        person.AddComponent<SoldierFight>().Arm(model.transform);
    }

    static void AlignFeet(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        float feet = bounds.min.y - model.transform.position.y;
        model.transform.localPosition = new Vector3(0f, -feet, 0f);
    }

    static DriveableCar CreateCar(Vector3 position, Quaternion rotation, Color color)
    {
        var root = new GameObject("Araba");
        root.transform.SetPositionAndRotation(position, rotation);

        var body = root.AddComponent<Rigidbody>();
        body.mass = 1300f;
        body.linearDamping = 0.02f;
        body.angularDamping = 4f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        body.centerOfMass = new Vector3(0f, 0.15f, 0f);

        var box = root.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.72f, 0f);
        box.size = new Vector3(1.85f, 1.25f, 4.45f);
        box.material = new PhysicsMaterial("Araba")
        {
            dynamicFriction = 0.55f,
            staticFriction = 0.65f,
            bounciness = 0.02f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };

        Visual(root.transform, "Kasa", new Vector3(0f, 0.62f, 0f), new Vector3(1.8f, 0.55f, 4.3f), color, 0.45f);
        Visual(root.transform, "Kabin", new Vector3(0f, 1.05f, -0.15f), new Vector3(1.55f, 0.48f, 2.05f), new Color(0.15f, 0.2f, 0.24f), 0.7f);
        Visual(root.transform, "Far", new Vector3(-0.55f, 0.68f, 2.1f), new Vector3(0.28f, 0.16f, 0.08f), new Color(1f, 0.95f, 0.7f), 0.8f);
        Visual(root.transform, "Far", new Vector3(0.55f, 0.68f, 2.1f), new Vector3(0.28f, 0.16f, 0.08f), new Color(1f, 0.95f, 0.7f), 0.8f);

        Transform wheelFL = Wheel(root.transform, new Vector3(-0.82f, 0.36f, 1.35f));
        Transform wheelFR = Wheel(root.transform, new Vector3(0.82f, 0.36f, 1.35f));
        Transform wheelRL = Wheel(root.transform, new Vector3(-0.82f, 0.36f, -1.35f));
        Transform wheelRR = Wheel(root.transform, new Vector3(0.82f, 0.36f, -1.35f));
        var beacon = Visual(root.transform, "Isaret", new Vector3(0f, 2.5f, 0f), new Vector3(0.28f, 0.28f, 0.28f), new Color(1f, 0.85f, 0.15f), 0.6f);

        var car = root.AddComponent<DriveableCar>();
        car.Beacon = beacon.transform;
        car.FrontWheels = new[] { wheelFL, wheelFR };
        car.Wheels = new[] { wheelRL, wheelRR };
        return car;
    }

    static Transform Wheel(Transform parent, Vector3 localPosition)
    {
        var wheel = Visual(parent, "Teker", localPosition, new Vector3(0.62f, 0.18f, 0.62f), new Color(0.08f, 0.08f, 0.08f), 0.2f);
        wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        return wheel.transform;
    }

    static readonly Dictionary<GameObject, Vector3> FacadeSizes = new Dictionary<GameObject, Vector3>();

    static bool PlaceArtBuilding(Transform parent, float x, float z, float width, float depth, float height)
    {
        if (CityArt.Facades == null || CityArt.Facades.Length == 0)
            return false;

        GameObject prefab = CityArt.Facades[Random.Range(0, CityArt.Facades.Length)];
        if (!FacadeSize(prefab, out Vector3 size))
            return false;

        float pieceW = Mathf.Max(size.x, size.z);
        float pieceH = size.y;
        if (pieceW < 0.2f || pieceH < 0.2f)
            return false;

        bool wideOnX = size.x >= size.z;
        var root = new GameObject("Bina");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(x, CityLayout.SidewalkHeight, z);
        var box = root.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, height * 0.5f, 0f);
        box.size = new Vector3(width, height, depth);

        PlaceFacade(root.transform, prefab, pieceW, pieceH, wideOnX, Vector3.forward, Vector3.right, depth, width, height);
        PlaceFacade(root.transform, prefab, pieceW, pieceH, wideOnX, Vector3.back, Vector3.left, depth, width, height);
        PlaceFacade(root.transform, prefab, pieceW, pieceH, wideOnX, Vector3.right, Vector3.back, width, depth, height);
        PlaceFacade(root.transform, prefab, pieceW, pieceH, wideOnX, Vector3.left, Vector3.forward, width, depth, height);

        Visual(root.transform, "Cati", new Vector3(0f, height, 0f), new Vector3(width, 0.2f, depth), new Color(0.16f, 0.18f, 0.2f), 0.25f);
        LabDoor.TryCreate(root.transform, depth);
        return true;
    }

    static void PlaceFacade(Transform root, GameObject prefab, float pieceW, float pieceH, bool wideOnX, Vector3 outward, Vector3 tangent, float outwardSpan, float faceSpan, float height)
    {
        int columns = Mathf.Clamp(Mathf.RoundToInt(faceSpan / pieceW), 1, 3);
        int rows = Mathf.Clamp(Mathf.RoundToInt(height / pieceH), 1, 3);
        float cellW = faceSpan / columns;
        float cellH = height / rows;
        Vector3 scale = wideOnX
            ? new Vector3(cellW / pieceW, cellH / pieceH, 1f)
            : new Vector3(1f, cellH / pieceH, cellW / pieceW);
        Quaternion rotation = Quaternion.LookRotation(outward, Vector3.up);
        if (!wideOnX)
            rotation *= Quaternion.Euler(0f, 90f, 0f);

        for (int column = 0; column < columns; column++)
        {
            for (int row = 0; row < rows; row++)
            {
                Vector3 center = root.position
                    + outward * (outwardSpan * 0.5f)
                    + tangent * ((column + 0.5f) * cellW - faceSpan * 0.5f)
                    + Vector3.up * ((row + 0.5f) * cellH);

                GameObject piece = Object.Instantiate(prefab, root);
                piece.transform.localScale = scale;
                piece.transform.rotation = rotation;
                Renderer[] renderers = piece.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                        bounds.Encapsulate(renderers[i].bounds);
                    piece.transform.position += center - bounds.center;
                }

                Collider[] colliders = piece.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                    colliders[i].enabled = false;
                Light[] lights = piece.GetComponentsInChildren<Light>();
                for (int i = 0; i < lights.Length; i++)
                    lights[i].enabled = false;
            }
        }
    }

    static bool FacadeSize(GameObject prefab, out Vector3 size)
    {
        if (FacadeSizes.TryGetValue(prefab, out size))
            return size.y > 0.05f;

        GameObject probe = Object.Instantiate(prefab);
        probe.transform.position = new Vector3(0f, -400f, 0f);
        Renderer[] renderers = probe.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Object.Destroy(probe);
            size = Vector3.zero;
            FacadeSizes[prefab] = size;
            return false;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        size = bounds.size;
        FacadeSizes[prefab] = size;
        Object.Destroy(probe);
        return size.y > 0.05f;
    }

    static GameObject Primitive(Transform parent, string name, Vector3 position, Vector3 scale, Color color, float smoothness)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = Material(color, smoothness);
        return go;
    }

    public static void Block(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds world = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            world.Encapsulate(renderers[i].bounds);
        if (world.size.y < 0.25f)
            return;

        var blocker = new GameObject("Engel");
        blocker.transform.SetPositionAndRotation(world.center, Quaternion.identity);
        var box = blocker.AddComponent<BoxCollider>();
        box.size = world.size * 0.92f;
        blocker.transform.SetParent(model.transform, true);
    }

    static GameObject Visual(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color, float smoothness, bool solid = false)
    {
        var go = GameObject.CreatePrimitive(name == "Kafa" || name == "Isaret" ? PrimitiveType.Sphere : PrimitiveType.Cube);
        if (name == "Teker")
        {
            Object.Destroy(go);
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        }
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = Material(color, smoothness);
        if (!solid)
            Object.Destroy(go.GetComponent<Collider>());
        return go;
    }

    static readonly Dictionary<int, Material> Materials = new Dictionary<int, Material>();

    static Material Material(Color color, float smoothness)
    {
        int key = ((Color32)color).r
            | (((Color32)color).g << 8)
            | (((Color32)color).b << 16)
            | (Mathf.RoundToInt(smoothness * 20f) << 24);
        if (Materials.TryGetValue(key, out Material cached) && cached != null)
            return cached;

        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");
        var material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", smoothness);
        Materials[key] = material;
        return material;
    }
}
