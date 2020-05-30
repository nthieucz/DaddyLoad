﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapGeneratorScript : MonoBehaviour
{
    public GameObject dirt;
    public GameObject stone;
    public GameObject gold;
    public int seed;
    public Hash h = new Hash();

    int minGeneratedX = -50;
    int maxGeneratedX = 50;

    public void Start()
    {
        generateMap(minGeneratedX, maxGeneratedX - minGeneratedX, 25);
    }

    public void Update()
    {
        if (Input.GetKeyDown("o"))
        {
            Debug.Log("building left");
            this.generateMap(minGeneratedX - 10, 10, 25);
            minGeneratedX -= 10;
        }
        if (Input.GetKeyDown("p"))
        {
            Debug.Log("building right");
            this.generateMap(maxGeneratedX, 10, 25);
            maxGeneratedX += 10;
        }
    }

    public void generateMap(int xStart, int width, int yMin)
    {
        for (int x = xStart; x < xStart + width; x++)
        for (int y = 0; y < yMin; y++)
        {
            Debug.Log("building a block");
            Instantiate(computeBlockAt(x, y, seed), new Vector3(x, -y, 0), Quaternion.identity);
        }
            

    }

    public GameObject computeBlockAt(int x, int y, int seed)
    {
        h.setHash(x, y, seed);
        if (y < 3) return dirt;
        else if (y == 3 && (h.v % 2 == 0 || h.v % 3 == 0)) return dirt;
        else if ((y == 4 || y == 5) && h.v % 2 == 0) return dirt;
        else if (h.v < 10000) return gold;
        else return stone;
    }
}

public class Hash
{

    public int v;

    public void setHash(int x, int y, int seed)  
    {
        bool isLeft = x < 0;
        double rootSeed = Math.Pow(seed, 0.25);
        double modSeed8 = seed % 8;
        double modX64 = x > 0 ? (x % 64) : 1;
        double modY59 = y > 0 ? (y % 59) : 1;
        double x1 = Math.Pow(Math.Pow(x, 8) + seed * 8, 1f / 2f) / (modX64 + 2);
        double y1 = Math.Pow(Math.Pow(y, 7) + seed * 3.5, 1f / 3f) / (modY59 + 2);
        double big = Math.E * Math.Pow(x1, 0.5) * Math.Pow(y1, 0.5) / seed;
        Debug.Log("big1: " + big);
        big *= (this.getFullDecimal(x1 * y1 * Math.Pow(big, 0.5)) + modSeed8) * rootSeed;
        Debug.Log("big2: " + big);
        if (isLeft) big *= Math.Pow(seed / 18.5, 19f / 41f);
        Debug.Log("big3: " + big);
        v = int.Parse(this.getPart(big));
        //Debug.Log("x: " + x + ", y: " + y + ", v: " + v);
    }

    private double getFullDecimal(double input)
    {
        String s = input.ToString();
        return double.Parse(s.Substring(s.IndexOf(".") + 1).Replace("E", ""));
    }

    private string getPart(double input)
    {
        Debug.Log("in getpart, input:" + input);
        string s = input.ToString();
        int index = s.IndexOf(".");
        try { return reverseString(s.Substring(index + 1, 5));} catch (Exception e){}
        try{return reverseString(s.Substring(index -5, 5));}catch (Exception e) { }
        Debug.Log("pruser jak brno in mapgen.getpart");
        return "99999";
    }

    private String reverseString(String str)
    {
        char[] chars = str.ToCharArray();
        char[] result = new char[chars.Length];
        for (int i = 0, j = str.Length - 1; i < str.Length; i++, j--)
            result[i] = chars[j];
        return new string(result);
    }
}

