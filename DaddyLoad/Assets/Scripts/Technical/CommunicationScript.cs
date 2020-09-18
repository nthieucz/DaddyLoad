﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Chat;

public class CommunicationScript : MonoBehaviourPunCallbacks
{
    // single point of entry pro vsechnu komunikaci

    //FileManager fm;
    //MapGeneratorScript mgs;

    public void Start()
    {
        //fm = GameObject.Find("FileManager").GetComponent<FileManager>();
        //mgs = GameObject.Find("MapGenerator").GetComponent<MapGeneratorScript>();
        FileManager.getShipUpgradesString();
    }

    public MapGeneratorScript mgs()
    {
        return GameObject.Find("MapGenerator").GetComponent<MapGeneratorScript>();
    }

    [PunRPC]
    public void receiveMessage(string message)
    {
        Debug.Log("received message: " + message);
        string[] segmented = message.Split('/');

        if (segmented[0] == "blockdestroy")
            receiveBlockDestroyInfo(segmented[1], int.Parse(segmented[2]), int.Parse(segmented[3]));

        if (segmented[0] == "setseed")
            receiveSeedUpdate(int.Parse(segmented[1]));

        if (segmented[0] == "mapinfo")
            receiveMapInfo(segmented[1]);

        if (segmented[0] == "materialupdate")
            updateInventoryMaterial(segmented[1], int.Parse(segmented[2]));

        if (segmented[0] == "mastersaveinv")
            writeDownInventory();

        if (segmented[0] == "shipinfo")
            loadShipInfo(message.Replace("shipinfo/", ""));

    }

    private void loadShipInfo(string s)
    {
        Debug.Log("LSI fired; input: " + s);

        FileManager.loadShipUpgradesFromString(s);

    }

    // locally called metoda ktera updatne 1 material v mgs.globalinventory
    private void updateInventoryMaterial(string material, int amount)
    {
        mgs().inventory.materials[material] = amount;
    }

    // locally called metoda ktera removne block
    private void receiveBlockDestroyInfo(string resourceName, int x, int y) 
    {
        FileManager.destroyedBlockCoords.Add(new Coordinate(x, y));

        if (PhotonNetwork.IsMasterClient)
        {
            FileManager.unwrittenBlockCoords.Add(new Coordinate(x, y));
        }
        mgs().removeBlockAt(x, y);
        mgs().bm.desert.addBackground(x, y);
    }

    // locally called; updatne seed
    private void receiveSeedUpdate(int newSeed)
    {
        Debug.Log("Setting seed to: " + newSeed);
        mgs().setSeed(newSeed); 
    }

    private void receiveMapInfo(string mapInfo)
    {
        Debug.Log("receiving map info: " + mapInfo);
        FileManager.loadDestroyedBlockCoordinatesFromString(mapInfo);
       
    }

    private void writeDownInventory()
    {
        if (PhotonNetwork.IsMasterClient)
            FileManager.writeDownInventory();

    }


    // v tyhle metode master posle map info newly connected playerovi pri connectu
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player joined");
        if (!PhotonNetwork.IsMasterClient) return;

        // misto newplayer bylo all
        photonView.RPC ("receiveMessage", newPlayer, "setseed/" + mgs().seed);

        string destroyedBlockCoords = FileManager.getDestroyedBlockCoordinatesInString();
        photonView.RPC("receiveMessage", newPlayer, "mapinfo/" + destroyedBlockCoords);

        foreach (KeyValuePair<string, int> pair in mgs().inventory.materials)
        {
            photonView.RPC("receiveMessage", newPlayer, "materialupdate/" + pair.Key + "/" + pair.Value);
        }

        photonView.RPC("receiveMessage", newPlayer, "shipinfo/" + FileManager.getShipUpgradesString());

    }
}