using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cars
{
    public static class CarCollection
    {


        static Car[] cars =
        {
        //0 (tier 1)
        new Car(
            name: "starter1",
            maxSpeed: 275,    //maxSpeed
            handling: 1.75f,
            dHandling: 1.77f,
            acceleration:  106f,   //acceleration
            boostStrength: 30,   //boostStrength 
            driftAcceleration: 25f,   //driftAcceleration
            traction: 0.25f,   //traction
            oversteer: 0.0f,
            tier: 1, //tier
            carBody: "f2",
            color: new Color(33, 33, 40) / 255
            ),

        //1 (tier 1)
        new Car(
            name: "starter_drift",
            maxSpeed: 266,    //maxSpeed
            handling: 2.1f,
            dHandling: 2.4f,
            acceleration:  98,   //acceleration
            boostStrength: 18f,   //boostStrength 
            driftAcceleration: 45,   //driftAcceleration
            traction: 0.15f,   //traction
            oversteer: 0.2f,
            tier: 1, //tier
            carBody: "f2",
            color: new Color(33, 33, 40) / 255
            ),

        //2 (tier 1)
        new Car(
            name: "starter_mustang",
            maxSpeed: 296,    //maxSpeed
            handling: 1.3f,
            dHandling: 1.35f,
            acceleration:  146,   //acceleration
            boostStrength: 27,   //boostStrength 
            driftAcceleration: 12,   //driftAcceleration
            traction: 0.43f,   //traction
            oversteer: 0.1f,
            tier: 1, //tier
            carBody: "f2",
            color: new Color(33, 33, 40) / 255
            ),

        //3 (tier 1)
        new Car(
            name: "starter_boost",
            maxSpeed: 262f,    //maxSpeed
            handling: 1.7f,
            dHandling: 1.8f,
            acceleration:  102,   //acceleration
            boostStrength: 44,   //boostStrength 
            driftAcceleration: 22,   //driftAcceleration
            traction: 0.19f,   //traction
            oversteer: 0.1f,
            tier: 1, //tier
            carBody: "f2",
            color:new Color(33, 33, 40) / 255
            ),

        new Car(
            name: "drift",
            maxSpeed: 310,    //maxSpeed
            handling: 1.94f,
            dHandling: 2.54f,
            acceleration:  102f,   //acceleration
            brakes: 120f,
            boostStrength: 32f,   //boostStrength 
            driftAcceleration: 51f,   //driftAcceleration
            traction: 0.24f,   //traction
            oversteer: 0.32f,
            tier: 2, //tier
            carBody: "drift"
            ),

        //5: tier 2
        new Car(
            name: "tuner",
            maxSpeed: 285,    //maxSpeed
            handling: 2.15f,
            dHandling: 2.6f,
            acceleration:  90f,   //acceleration
            boostStrength: 62f,   //boostStrength 
            driftAcceleration: 50f,   //driftAcceleration
            traction: 0.7f,   //traction
            oversteer: 0.21f,
            tier: 2, //tier
            carBody: "f2",
            color: new Color(91, 13, 186) / 255),
        
        //6: tier 2
        new Car(
            name: "balance",
            maxSpeed: 300,    //maxSpeed
            handling: 2.05f,
            dHandling: 2.1f,
            acceleration:  110f,   //acceleration
            boostStrength: 50f,   //boostStrength 
            driftAcceleration: 40f,   //driftAcceleration
            traction: 0.45f,   //traction
            oversteer: 0.05f,
            tier: 2, //tier
            carBody: "smooth",
            color: new Color(153, 153, 153) / 255),

        //7: tier 2
        new Car(
            name: "muscle",
            maxSpeed: 330,    //maxSpeed
            handling: 1.05f,
            dHandling: 1.7f,
            acceleration:  142f,   //acceleration
            boostStrength: 20f,   //boostStrength 
            driftAcceleration: 44f,   //driftAcceleration
            traction: 0.3f,   //traction
            oversteer: 0.38f,
            tier: 2, //tier
            carBody: "muscle",
            color: new Color(0.1f, 0.5f, 0.8f)),

        //8: tier 2.5
        new Car(
            name: "Booster",
            maxSpeed: 328,    //maxSpeed
            handling: 1.85f,
            dHandling: 2.6f,
            acceleration:  105f,   //acceleration
            boostStrength: 70f,   //boostStrength 
            driftAcceleration: 17f,   //driftAcceleration
            traction: 0.6f,   //traction
            oversteer: 0.1f,
            tier: 2, //tier
            carBody: "boost"
            ),

        //9: tier 2
        new Car(
            name: "grip",
            maxSpeed: 332,    //maxSpeed
            handling: 2.6f,
            dHandling: 1.14f,
            acceleration:  139f,   //acceleration
            brakes: 165f,
            boostStrength: 41f,   //boostStrength 
            driftAcceleration: 10f,   //driftAcceleration
            traction: 1f,   //traction
            oversteer: -0.18f,
            tier: 2, //tier
            carBody: "grip"
            ),

        //10: tier 3
        new Car(
            name: "trueno8600x",
            maxSpeed: 281,
            handling: 2.52f,
            dHandling: 2.96f,
            acceleration:  130f,
            brakes: 150f,
            boostStrength: 58f, 
            driftAcceleration: 42f,
            traction: 0.33f,
            oversteer: 0.25f,
            tier: 2,
            carBody: "sidebooster",
            color: Color.white
            ),

        //11: tier 2
        new Car(
            name: "exotic",
            maxSpeed: 314,
            handling: 1.95f,
            dHandling: 1.5f,
            acceleration:  250f,
            brakes: 170,
            boostStrength: 37f,
            driftAcceleration: 28f,
            traction: 0.55f,
            oversteer: -0.1f,
            tier: 2,
            carBody: "smooth",
            color: new Color(0.1f, 0.1f, 0.1f)),

        //12: special
        new Car(
            name: "sled",
            maxSpeed: 250f,
            handling: 2.13f,
            dHandling: 2.13f,
            acceleration:  10f,
            brakes: 100f,
            boostStrength: 0f,
            driftAcceleration: 120f,
            traction: -0.25f,
            oversteer: 0.15f,
            tier: 4,
            carBody: "sidebooster",
            color: new Color(0.8f, 1, 1)),

        //13: tier 2
        new Car(
            name: "slow god",
            maxSpeed: 270f,
            handling: 2.8f,
            dHandling: 172f,
            acceleration:  172f,
            brakes: 100f,
            boostStrength: 40f,
            driftAcceleration: 62f,
            traction: 0.72f,
            oversteer: -0.2f,
            tier: 2,
            carBody: "sidebooster",
            color: new Color(0.1f, 0.3f, 0.6f)),

        //14: tier 2
        new Car(
            name: "hover car",
            maxSpeed: 276f,
            handling: 1.3f,
            dHandling: 2.65f,
            acceleration:  108f,
            brakes: 100f,
            boostStrength: 65f,
            driftAcceleration: 62f,
            traction: 0.1f,
            oversteer: 0.45f,
            tier: 2,
            carBody: "hover"
            ),

        //15: tier 2
        new Car(
            name: "speedster",
            maxSpeed: 332,
            handling: 1.4f,
            dHandling: 1.4f,
            acceleration:  60,
            brakes: 100,
            boostStrength: 80,
            driftAcceleration: 72f,
            traction: 0.76f,
            oversteer: 0.3f,
            tier: 3,
            carBody: "muscle",
            color: Color.green
            ),

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
            2.28f,
            1.55f,
            218,    //acceleration
            68,     //boostStrength
            42,     //driftAcceleration
            0.62f, //traction
            -0.07f,   //oversteer
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
}
