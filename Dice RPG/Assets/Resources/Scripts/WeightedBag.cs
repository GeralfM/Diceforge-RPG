using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class WeightedBag
{
    public Dictionary<string, float> content = new Dictionary<string, float>();

    public WeightedBag() { }

    public void AddElement(string name, float weight) { content.Add(name, weight); }
    public void AddWeight(string name, float weight) { content[name] += weight; }
    public void MultiplyWeight(string name, float factor) { content[name] *= factor; }

    public string Draw(bool isProba) // isProba to draw from 0-1 range, else to draw from weighted collection
    {
        float accu = 0f;
        Dictionary<string, float> accuContent = new Dictionary<string, float>();
        foreach(string name in content.Keys)
        {
            if (content[name] > 0f)
            {
                accuContent.Add(name, accu);
                accu += Mathf.Max(0f, content[name]);
            }
        }
        if (isProba && accu<1f) { accuContent.Add("nothing", accu); }

        float result = Random.Range(0f, (isProba)?(Mathf.Max(1f,accu)):accu);
        float selectedFloat = accuContent.Values.ToList().FindAll(x => x <= result).Max();

        return accuContent.First(item => item.Value == selectedFloat).Key;
    }
    public void DescribeBag()
    {
        foreach(string name in content.Keys) { Debug.Log(name + " : " + content[name]); }
    }
}
