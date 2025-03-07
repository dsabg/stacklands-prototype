using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using System.Linq;
using Utility;

[JsonConverter(typeof(PackageConverter))]
public class PackageData
{
    public string Name;
    public int Price;
    public List<CardRef> CommonCards = new List<CardRef>();
    public List<CardRef> UncommonCards = new List<CardRef>();
    public List<CardRef> RareCards = new List<CardRef>();
    public List<CardRef> LegendaryCards = new List<CardRef>();
}
public class PackageConverter : JsonConverter<PackageData>
{
    public override PackageData ReadJson(JsonReader reader, Type objectType, PackageData existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        PackageData result = new PackageData();

        JToken name = obj["Name"];
        if (name != null)
        {
            result.Name = name.Value<string>();
        }
        JToken price = obj["Price"];
        if (price!= null)
        {
            result.Price = price.Value<int>();
        }
        JToken commonCards = obj["CommonCards"];
        if (commonCards != null)
        {
            HydrateCardRefs(commonCards.Value<JArray>(), ref result.CommonCards, "CommonCards");
        }

        JToken uncommonCards = obj["UncommonCards"];
        if (uncommonCards != null)
        {
            HydrateCardRefs(uncommonCards.Value<JArray>(), ref result.UncommonCards, "UncommonCards");
        }
        JToken rareCards = obj["RareCards"];
        if (rareCards != null)
        {
            HydrateCardRefs(rareCards.Value<JArray>(), ref result.RareCards, "RareCards");
        }
        JToken legendaryCards = obj["LegendaryCards"];
        if (legendaryCards != null)
        {
            HydrateCardRefs(legendaryCards.Value<JArray>(), ref result.LegendaryCards, "LegendaryCards");
        }
        return result;

    }

    private void HydrateCardRefs(JArray array, ref List<CardRef> target, string arrayName)
    {
        foreach (JToken card in array)
        {
            if (card is JObject cardRefContainer)
            {
                CardRef cardRef = cardRefContainer.Properties().First().Value.ToObject<CardRef>();
                string cardName = cardRefContainer.Properties().First().Name;
                cardRef.CardName = cardName;
                target.Add(cardRef);
            }
            else if (card is JValue cardName)
            {
                CardRef cardRef = new CardRef();
                cardRef.Quantity = 1;
                cardRef.CardName = cardName.Value<string>();
                target.Add(cardRef);
            }
            else
            {
                throw new AssertFailedException($"Invalid type in interaction array '{arrayName}'.");
            }
        }
    }

    public override void WriteJson(JsonWriter writer, PackageData value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
