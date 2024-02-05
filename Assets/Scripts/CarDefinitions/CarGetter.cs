using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGetter : MonoBehaviour
{
    Car car;
    public CarMeshScriptable carMeshCollection;
    GameObject carObject;
    CarModel carModel;
    Transform hitbox;
    Vector3 offset;
    public bool overrideCar = false;
    public int carId;
    public bool usingProvidedCar = false;
    public Car providedCar = null;

    bool started = false;

    public CarModel GetCarModel()
    {
        return carModel;
    }

    public Car GetCar()
    {
        return car;
    }

    public Vector2 GetHitboxSize()
    {
        return carModel.GetHitboxSize();
    }

    public AudioScriptableObject GetSoundProfile()
    {
        return carModel.soundProfile;
    }

    // Start is called before the first frame update
    public void CustomStart()
    {
        //offset = new Vector3(0f, -0.25f, 0);
        offset = new Vector3(0, 0, 0);
        hitbox = transform.Find("Hitbox");

        if (usingProvidedCar == true)
        {
            Debug.Log("Using Provided Car");
            car = providedCar;
        }
        else
        {
            if (!overrideCar)
                car = CarCollection.FindCarByIndex(CarCollection.localCarIndex);
            else
                car = CarCollection.FindCarByIndex(carId);
        }


        carObject = Instantiate(carMeshCollection.GetGameObject(car.GetModel()), transform, false);
        carObject.transform.localScale = Vector3.one;
        carModel = carObject.GetComponent<CarModel>();

        hitbox.localScale = new Vector3(carModel.GetHitboxSize().x * 0.8f, hitbox.localScale.y, carModel.GetHitboxSize().y * 0.8f);
        hitbox.transform.localPosition = offset + new Vector3(0, 0.5f, 0);

        Debug.Log("CarGetter");
        started = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
