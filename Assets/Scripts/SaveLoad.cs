using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class SaveLoad
{
    public static List<Red> redesGuardadas = new List<Red>();



    public static void Load(string finalRuta = "RedNeuronal", bool cargadoAut = false)
    {
        finalRuta = "/" + finalRuta + ".bdq";
        
        if (File.Exists(Application.persistentDataPath + finalRuta))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + finalRuta, FileMode.Open);
            SaveLoad.redesGuardadas = (List<Red>)bf.Deserialize(file);
            file.Close();
            if(!cargadoAut)
                Debug.Log("CARGADO: " + Application.persistentDataPath + finalRuta);
        }
    }
    public static void Save(Red red, string finalRuta)
    {
        Debug.Log("GUARDADO EN : " + Application.persistentDataPath + finalRuta);
        redesGuardadas.Add(red);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + finalRuta);
        bf.Serialize(file, SaveLoad.redesGuardadas);
        file.Close();
    }
}
