using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CarCollection
{


    static Car[] cars =
    {
        //0 (tier 1)
        new Car(
            "starter1",
            275,    //maxSpeed
            1.75f,
            1.77f,
            acceleration:  106f,   //acceleration
            boostStrength: 30,   //boostStrength 
            driftAcceleration: 25f,   //driftAcceleration
            traction: 0.25f,   //traction
            oversteer: 0.0f,
            tier: 1, //tier
            "f2",
            new Color(33, 33, 40) / 255),

        //1 (tier 1)
        new Car(
            "starter_drift",
            266,    //maxSpeed
            2.1f,
            2.4f,
            98,   //acceleration
            18f,   //boostStrength 
            45,   //driftAcceleration
            0.15f,   //traction
            0.2f,   //oversteer
            1, //tier
            "f2",
            new Color(33, 33, 40) / 255),

        //2 (tier 1)
        new Car(
            "starter_mustang",
            296,    //maxSpeed
            1.3f,
            1.35f,
            146,   //acceleration
            27,   //boostStrength 
            12,   //driftAcceleration
            0.43f,   //traction
            -0.1f, //oversteer
            1, //tier
            "f2",
            new Color(33, 33, 40) / 255),

        //3 (tier 1)
        new Car(
            "starter_boost",
            262f,    //maxSpeed
            1.7f,
            1.8f,
            102,   //acceleration
            44,   //boostStrength 
            22,   //driftAcceleration
            0.19f,   //traction
            0.1f,    //oversteer
            1, //tier
            "f2",
            new Color(33, 33, 40) / 255),

        new Car(
            "drift",    
            310,    //maxSpeed
            2.24f, //used to be 1.54f  
            2.54f,     
            102f,   //acceleration
            120f,  //brakes
            32f,   //boostStrength 
            51f,   //driftAcceleration
            0.24f,   //traction
            0.4f,   //oversteer
            2,  //tier
            "drift"),

        //5: tier 2
        new Car(
            "tuner",    
            288,    
            2.4f, //1.62f   
            2.6f,     
            93f, //acceleration
            64f,   //booststrength 
            50f,    //driftAcceleration
            0.7f,   //traction
            0.23f,   //oversteer
            2, //tier
            "f2", 
            new Color(91, 13, 186) / 255),
        
        //6: tier 2
        new Car(
            "balance",  
            300,    
            2.05f,   
            2.1f,  
            110f,  
            50f,    
            40f,
            0.45f,
            0.05f,   //oversteer
            2, //tier 
            "smooth", 
            new Color(153, 153, 153) / 255),

        //7: tier 2
        new Car(
            "muscle",   
            330,    
            1.5f,  //0.97
            1.7f,     
            142f,  
            20f,    
            44f,
            0.3f,
            0.45f,   //oversteer
            2, //tier
            "muscle", 
            new Color(0.1f, 0.5f, 0.8f)),

        //8: tier 2.5
        new Car(
            "Booster",
            328,
            1.85f,
            2.6f,
            105f,   //acceleration
            70f,    //boostStrength
            17f,    //driftAcceleration
            0.6f,   //traction
            0.1f,   //oversteer
            2, //tier
            "boost"),

        //9: tier 2
        new Car(
            "grip",     
            332,    
            2.1f,     
            1.14f,   
            132f,   //acceleration
            165f,  //brakes
            37f,    //boostStrength
            10f,     //driftAcceleration
            1f,     //traction
            -0.2f,   //oversteer
            2, //tier
            "grip"),

        //10: tier 3
        new Car(
            "trueno8600x", 
            281,
            2.72f,     //1.58f
            2.96f,     
            130f,    //acceleration
            68f,    //boostStrength
            42f,    //driftAcceleration
            0.33f,  //traction
            0.25f,   //oversteer
            2, //tier
            "sidebooster", 
            Color.white),

        //11: tier 2
        new Car(
            "exotic",   
            314,    
            1.7f,   
            1.5f,   
            250f,    //acceleration
            170,    //brakes
            33f,    //boostStrength
            25f,    //driftAcceleration
            0.55f,  //traction
            -0.1f,   //oversteer
            2, //tier 
            "smooth", 
            new Color(0.1f, 0.1f, 0.1f)),

        //12: special
        new Car(
            "sled",     
            250f,
            2.13f,
            2.13f,   
            10f,    
            0f,    
            120f,     
            -0.25f,
            0.15f,   //oversteer
            4, //tier
            "sidebooster", 
            new Color(0.8f, 1, 1)),

        //13: tier 2
        new Car(
            "slow god",     
            270f,   
            2.8f,   
            3f,   
            172f,    //acceleration
            100f,   //brakes
            40f,     //boostStrength
            62f,     //driftAcceleration
            0.72f,    //traction
            -0.2f,   //oversteer
            2, //tier
            "sidebooster", 
            new Color(0.1f, 0.3f, 0.6f)),

        //14: tier 2
        new Car(
            "hover car", 
            276f,
            2.3f,     //1.3f
            2.65f,    
            108f,    //acceleration
            100f,    //brakes
            65f,    //boostStrength
            62f,    //driftAcceleration
            0.1f,   //traction
            0.45f,   //oversteer
            2, //tier
            "hover"),

        //15: tier 2
        new Car(
            "speedster", 
            332,     
            1.4f,   
            1.4f,   
            60,    //acceleration
            100,   //brakes
            80,    //boostStrength
            72f,    //driftAcceleration
            0.76f,   //traction
            0.3f,   //oversteer
            3, //tier 
            "muscle",   
            Color.green),

        //16: tier 2
        new Car(
            "weird", 
            292,
            2f, //2.56f
            1.9f, 
            130f,   //acceleration
            92f,   //boostStrength
            36,    //driftAcceleration
            0.76f,    //traction
            -0.25f,   //oversteer
            2, //tier
            "sled-galarian", 
            (Color.red + Color.yellow)),

        //18: tier 3
        new Car(
            "burst", 
            322, //maxSpeed
            1.64f, 
            1.95f, 
            235, //acceleration
            82, //boostStrength
            56, //driftAcceleration
            0.62f,  //traction
            0.1f,   //oversteer
            3, //tier
            "smooth", 
            new Color(0.8f, 0.1f, 0.4f)),

        //19: tier 3
        new Car(
            "understeer", 
            345,    //maxSpeed
            1.98f, 
            1.55f, 
            218,    //acceleration
            68,     //boostStrength
            42,     //driftAcceleration
            0.62f, //traction
            -0.3f,   //oversteer
            3, //tier
            "smooth", 
            new Color(0.2f, 0.0f, 0.4f)),

        //20: tier 3
        new Car(
            "speed and balance",
            336,    //maxSpeed
            2.2f,
            2.4f,
            186f,   //acceleration
           50f,   //boostStrength 
            52f,   //driftAcceleration
            0.6f,   //traction
            0.4f,   //oversteer
            3, //tier
            "f2",
            new Color(0, 173, 64) / 255),

        //21: tier 3
        new Car(
            "junkman starter",
            316,    //maxSpeed
            3.15f,
            3.2f,
            179f,   //acceleration
            58f,   //boostStrength 
            44f,   //driftAcceleration
            0.48f,   //traction
            0.1f,   //oversteer
            3, //tier
            "f2",
            new Color(240, 240, 252) / 255),

        //22: tier 3
        new Car(
            "no-drift acc",
            354,    //maxSpeed
            2.65f,
            2.2f,
            148f,   //acceleration
            76f,    //boostStrength 
            8f,    //driftAcceleration
            0.9f,   //traction
            -0.45f,   //oversteer
            3, //tier
            "grip",
            new Color(100, 240, 252) / 255),

        //23: tier 3
        new Car(
            "sled prototype",
            295f,
            2.13f - 0.3f,
            2.13f + 0.6f,
            122f, //acceleration
            38f, //boostStrength
            95f, //driftAcceleration
            0.25f, //traction
            0.5f,   //oversteer
            3, //tier
            "sidebooster",
            new Color(0.8f, 1, 1)),

        //99 special
        new Car(
            "random",   
            Random.Range(250, 350), 
            Random.Range(0.5f, 3.5f), 
            Random.Range(0.5f, 3.5f), 
            Random.Range(20f, 256f), 
            Random.Range(10, 100), 
            Random.Range(0, 60), 
            Random.Range(-0.1f, 1.1f),
            Random.Range(-0.4f, 0.6f),   //oversteer
            4, //tier
            "sled-galarian", 
            new Color(Random.value, Random.value, Random.value))
    };

    public static List<int>[] allTiers;

    public static void InitializeTiers()
    {
        if (allTiers == null)
        {
            allTiers = new List<int>[4];

            for (int i = 0; i < allTiers.Length; i++)
            {
                allTiers[i] = new List<int>();
            }

            for (int i = 0; i < GetCarList().Length; i++)
            {
                allTiers[FindCarByIndex(i).GetTier() - 1].Add(i);
            }
        }
    }

    public static int localCarIndex = 0;

    public static int GetCarCount()
    {
        return cars.Length;
    }

    public static Car FindCarByName(string name)
    {
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].GetName().Equals(name))
            {
                return cars[i];
            }
        }
        return null;
    }

    public static Car FindCarByIndex(int index)
    {
        return cars[index];
    }

    public static Car[] GetCarList()
    {
        return cars;
    }

    public static int GetIndex(Car car)
    {
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].Equals(car))
            {
                return i;
            }
        }
        return -1;
    }

    public static int GetIndex(string name)
    {
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].GetName().Equals(name))
            {
                return i;
            }
        }
        return -1;
    }

    public static void PickLocalCar(string carName)
    {
        localCarIndex = GetIndex(carName);
    }

    public static void PickLocalCar(int index)
    {
        localCarIndex = index;
    }

    public static void DisplayStats(int index)
    {
        Debug.Log("maxSpeed: " + cars[index].GetMaxSpeed() + "\n" +
            "acc: " + cars[index].GetAcceleration() + "\n" +
            "d-acc " + cars[index].GetDriftAcceleration() + "\n" +
            "hand " + cars[index].GetHandling() + "\n" +
            "d-hand " + cars[index].GetDHandling() + "\n" +
            "boost: " + cars[index].GetBoostStrength() + "\n" +
            "traction: " + cars[index].GetTraction() + "\n");
    }

}
